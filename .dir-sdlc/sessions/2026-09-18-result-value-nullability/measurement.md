# Measurement -- MASTER-044 / CORE-005, at `42ec7e9`

Run by the orchestrator before decomposition, against a fixed commit, in the main checkout.
`git rev-parse HEAD` -> `42ec7e9805ecf5b7a792c0013da6390d6fd1074a`.

Every number below names the command that produced it. Nothing here is carried from the handoff or
from the register entry.

---

## 1. The entry as written, claim by claim

`docs/audit/00-master-index.md:232` -> `MASTER-044`, source entry `CORE-005` at
`docs/audit/01-domain-application.md:264`.

| # | Claim in `CORE-005` | State at `42ec7e9` | Verdict |
|---|---|---|---|
| 1 | Location `src/DungeonMaster.Application/Common/Results/Result.cs:9` | `Result.cs:6` declares the **non-generic** `public sealed record Result`. `Result<T>` is not in this file at all -- it is at `src/DungeonMaster.Application/Common/Results/ResultOfT.cs:9` | **stale citation** |
| 2 | Location `src/DungeonMaster.Application/Common/Results/Result.cs:16-19` | That range is now `IsSuccess` / `IsFailure` on the non-generic type | **stale citation** |
| 3 | `Result<T>` declares `public T? Value { get; init; }` with no constraint on `T` | `ResultOfT.cs:29` declares `public T? Value { get; private init; }`. Unconstrained `T`: holds. `public init`: **no longer true**, it is `private init` on both types | **holds in substance, wrong in the accessor** |
| 4 | Its XML summary says the property is "the value returned by the operation, or `null` when the operation failed" | `ResultOfT.cs:27`, verbatim, unchanged | **holds** |
| 5 | "Every command in this layer closes `T` as `Guid` or `bool`, both value types" | No command closes `T` as `bool` any more. The `bool` commands became the non-generic `Result`. Production closes `Result<T>` as **`Result<Guid>` and nothing else** | **half false** |
| 6 | "the documentation misdescribes the type for fourteen of the sixteen dispatchable operations" | Sixteen dispatchable operations is still right: 10 `ICommand` + 4 `ICommand<Guid>` + 2 `IQuery<>`. But only **4** of them use `Result<T>` | **false now** -- true when written, falsified by the non-generic `Result` |
| 7 | "the App layer already does exactly that (`ViewModels/DungeonDetailViewModel.cs:302` tests `result.Value == Guid.Empty`)" | `DungeonDetailViewModel.cs` is **202 lines** (`b.count(b'\n')`, binary), so `:302` cannot exist. The surviving sentinel is `src/DungeonMaster.App/ViewModels/MapCanvasViewModel.cs:213` | **stale citation** |

**Claims 3 and 4 are the entry's core and both hold.** Everything else has moved. The entry has never
been measured by any session, and claims 1, 2, 6 and 7 are the cost of that.

**Why claims 5 and 6 went false without anyone touching `MASTER-044`:** `CORE-006`, the entry
immediately below it at `01-domain-application.md:299` (the architect corrected this -- this document first said `:302`; `grep -n '^### CORE-00' docs/audit/01-domain-application.md` returns `264` for `CORE-005` and `299` for `CORE-006`, re-run by the orchestrator at `42ec7e9`), proposed "adding a non-generic `Result` (or a
`Unit` payload) for commands with no return value, and making the properties `private init`". Both
landed. `MASTER-044`'s premise was narrowed by a fix aimed at its neighbour, and no session updated
it.

---

## 2. What the defect actually is, at `42ec7e9`

### 2.1 The type

`src/DungeonMaster.Application/Common/Results/ResultOfT.cs`

```
 27 ///        Gets the value returned by the operation, or <see langword="null"/> when the operation failed.
 29 public T? Value { get; private init; }
```

`T` is unconstrained. Per the Microsoft nullable-reference-types reference the entry cites, `T?` on an
unconstrained type parameter closed with a value type is just `T` -- the annotation has no effect
without a `struct` constraint. So for the only `T` production uses, the summary at `:27` describes a
state the type cannot reach.

### 2.2 The population, counted two ways

**Way one -- request declarations.**
`grep -rn "ICommand<\|IQuery<" --include=*.cs src/` over non-test, non-`obj` paths:

| Closure | Count | Sites |
|---|---|---|
| `ICommand<Guid>` | 4 | `AddAreaCommand.cs:25`, `AddFloorCommand.cs:17`, `AddPlacedObjectCommand.cs:35`, `CreateDungeonCommand.cs:17` |
| `IQuery<DungeonDetailDto?>` | 1 | `GetDungeonDetailQuery.cs:13` |
| `IQuery<IReadOnlyList<DungeonSummaryDto>>` | 1 | `GetDungeonsQuery.cs:9` |
| `ICommand` (non-generic) | 10 | 10 files under `Features/Dungeons/Commands/` |

**The two queries are not in the population at all.** `IDispatcher.cs:60` declares
`Task<TResult> QueryAsync<TResult>(...)` -- it returns the value bare, not a `Result<TResult>`. Only
`SendAsync<TResult>` (`IDispatcher.cs:41`) returns `Task<Result<TResult>>`. A query never constructs a
`Result<T>`, so no query can exhibit this defect.

**Way two -- type occurrences.**
`grep -rhno "Result<[A-Za-z0-9_?<>, .]*>"` over `src/DungeonMaster.{Application,App,Infrastructure,Domain}`,
excluding `obj/` and `bin/`, tallied: `Result<Guid>` 6, `Result<Guid>>` 4, and the rest are the generic
plumbing (`Result<T>` 6, `Result<TResult>` variants 12, `Result<>` 1). **`Result<Guid>` is the only
closed construction in production.** The two ways agree.

**Predicate validation, per the standing rule.**

- A case that must match: `AddAreaCommand.cs:25` `: ICommand<Guid>` -- matched by both ways.
- A near-miss that must not match: `GetDungeonDetailQuery.cs:13` `: IQuery<DungeonDetailDto?>`. It is a
  generic dispatchable operation and looks like a member of the population, and it is not one, because
  `QueryAsync` does not return `Result<T>`. Excluded by reading `IDispatcher.cs:60`, not by the grep.

**`n = 4` request types, `Guid` only.** The entry's "fourteen" is not a figure any command can produce
today.

### 2.3 The consumers

`grep -rn "SendAsync<Guid>"` over `src/DungeonMaster.App`, excluding `obj/` and `bin/` -- **5 call
sites**:

| Site | Reads `.Value`? | What it does |
|---|---|---|
| `CreateDungeonViewModel.cs:58` | no | branches on `IsSuccess`, discards the `Guid` |
| `FloorListViewModel.cs:80` | no | branches on `IsFailure`, discards the `Guid` |
| `MapCanvasViewModel.cs:76` | no | branches on `IsFailure`, discards the `Guid` |
| `MapCanvasViewModel.cs:294` | no | branches on `IsFailure`, discards the `Guid` |
| `MapCanvasViewModel.cs:198` | **yes**, at `:213` and `:222` | the sentinel |

**Four of the five never read `Value` at all.** The `Guid` payload is carried and dropped.

`grep -rn "\.Value\b"` over `src/DungeonMaster.App` and `src/DungeonMaster.Application` returns 26
hits; all but three are `Nullable<T>.Value` on `Guid?` fields (`_editor.SelectedFloorId.Value` and
friends), one is `KeyValuePair.Value`, and one is `ErrorCategory!.Value` inside the conversion operator
at `ResultOfT.cs:129`. **`MapCanvasViewModel.cs:213` and `:222` are the only `Result<T>.Value` reads in
production.** The raw grep over-counts by 24; the `Nullable<T>.Value` sites are the near-miss.

### 2.4 The sentinel, read in place

`src/DungeonMaster.App/ViewModels/MapCanvasViewModel.cs:198-222`:

```
198  var addResult = await dispatcher.SendAsync<Guid>(new AddAreaCommand(...));
206  if (addResult.IsFailure)
208      context.ReportFailure(addResult);
210      return;
213  if (addResult.Value == Guid.Empty)
215      return;
218  var outlineResult = await dispatcher.SendAsync(new UpdateAreaOutlineCommand(..., addResult.Value, ...));
```

**The sentinel at `:213` is reached only after `IsFailure` has already returned.** It therefore tests
the *success* path -- it asks whether a successful `AddAreaCommand` produced `Guid.Empty`. This is the
shape `CORE-005` describes as "a null-check surrogate", and the measurement sharpens it: it is not
guarding failure at all, and on the success path `AddAreaCommandHandler` yields a freshly created area
id. The entry's warning that it "would silently mis-fire the day a command legitimately returns an
empty `Guid`" is correct in principle; what it does today is return silently, with no error reported,
on a branch nothing reaches.

### 2.5 The suite already pins the defect as intended behaviour

`src/DungeonMaster.Tests/Application/Common/Results/ResultOfTTests.cs:36-50`:

```
 33 ///        Verifies that a failed result carries the supplied message and category and no value.
 36 public void Failure_WithCategory_IsNotSuccessAndCarriesNoValue()
 46     result.Value.Should().Be(Guid.Empty);
```

**The test's name and its XML summary both say "no value"; its assertion says `Guid.Empty`.** That
gap, written down and passing, is `MASTER-044` expressed in the test suite rather than in the type.
Any change to `Value`'s shape or its documentation has to go through this test -- it is not incidental
coverage, it is the entry's own claim asserted as a requirement.

### 2.6 Where a failed `Result<Guid>` is constructed

`Result<T>.Failure` (`ResultOfT.cs:74`) and `Result<T>.ValidationFailure` (`:93`) both build with
`Value` unset, so `Value` is `default(T)` -- `Guid.Empty`. `MutateAsync<TResult>`
(`Common/Handlers/DungeonMutationExtensions.cs:84`) is the single generic production path that returns
a failed `Result<TResult>` of its own making, at `:98`, for the dungeon-not-found case. Every other
failure is the mutation's own result passed through at `:107`.

---

## 3. What is left of the entry

**The defect is real and it is smaller than the entry describes.** Reduced to what is true at
`42ec7e9`:

1. `ResultOfT.cs:27` documents a `null` that `Result<Guid>` cannot produce, for all four production
   command types.
2. `ResultOfT.cs:29` annotates `T?` on an unconstrained `T`, which is what makes (1) possible.
3. `MapCanvasViewModel.cs:213` is a sentinel on the success path that returns without reporting
   anything.
4. `ResultOfTTests.cs:36-50` asserts the defect under a name that denies it.

**What is NOT in scope from the entry as written:** the `public init` accessors (already `private
init`), the `bool` commands (gone), the queries (never used `Result<T>`), and the "fourteen of
sixteen" figure (4 of 16).
