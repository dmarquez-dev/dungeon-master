# Domain and Application -- code quality audit

## Scope

| Directory / glob | Files | Lines |
|---|---|---|
| `src/DungeonMaster.Domain/*.cs` (root) | 1 | 3 |
| `src/DungeonMaster.Domain/Entities/Entity.cs` | 1 | 28 |
| `src/DungeonMaster.Domain/Entities/Areas/**` | 2 | 107 |
| `src/DungeonMaster.Domain/Entities/Dungeons/**` | 2 | 503 |
| `src/DungeonMaster.Domain/Entities/Floors/**` | 10 | 559 |
| `src/DungeonMaster.Domain/ValueObjects/**` | 4 | 60 |
| `src/DungeonMaster.Application/Common/Behaviors/**` | 1 | 48 |
| `src/DungeonMaster.Application/Common/Dispatch/**` | 7 | 233 |
| `src/DungeonMaster.Application/Common/Map/**` | 3 | 224 |
| `src/DungeonMaster.Application/Common/Results/**` | 2 | 86 |
| `src/DungeonMaster.Application/DependencyInjection.cs` | 1 | 105 |
| `src/DungeonMaster.Application/Features/Dungeons/*.cs` (root) | 2 | 101 |
| `src/DungeonMaster.Application/Features/Dungeons/Commands/**` | 32 | 984 |
| `src/DungeonMaster.Application/Features/Dungeons/Queries/**` | 9 | 211 |
| **Total** | **77** | **3,252** |

Files read: **77**. Files cited in at least one record: **62**.

No hand-written file in either layer was excluded from reading. Neither project's source tree contains
generated code -- there are no EF Core migrations, no `*.Designer.cs` and no XAML source-generator
output -- so every one of the 77 files was audited against the same hand-written standard. One
category was excluded:

- `src/DungeonMaster.Domain/obj/**` and `src/DungeonMaster.Application/obj/**` (6 `.cs` files:
  `*.AssemblyInfo.cs`, `*.GlobalUsings.g.cs`, `.NETCoreApp,Version=v10.0.AssemblyAttributes.cs`) --
  MSBuild intermediate output, regenerated on every build rather than authored, and therefore not
  counted in the table above.

The following 15 files were read and found sound; they are not cited by any record:

- `src/DungeonMaster.Domain/ValueObjects/PathSegmentKind.cs` -- fully documented enum, both members carry XML docs.
- `src/DungeonMaster.Application/Features/Dungeons/Commands/AddArea/AddAreaCommand.cs` -- conforms to the documented command shape.
- `src/DungeonMaster.Application/Features/Dungeons/Commands/AddArea/AddAreaCommandValidator.cs` -- conforms to the documented validator shape.
- `src/DungeonMaster.Application/Features/Dungeons/Commands/CreateDungeon/CreateDungeonCommand.cs` -- conforms to the documented command shape.
- `src/DungeonMaster.Application/Features/Dungeons/Commands/CreateDungeon/CreateDungeonCommandValidator.cs` -- conforms to the documented validator shape.
- `src/DungeonMaster.Application/Features/Dungeons/Commands/UpdateArea/UpdateAreaCommand.cs` -- conforms to the documented command shape.
- `src/DungeonMaster.Application/Features/Dungeons/Commands/UpdateArea/UpdateAreaCommandValidator.cs` -- conforms to the documented validator shape.
- `src/DungeonMaster.Application/Features/Dungeons/Commands/UpdateDungeon/UpdateDungeonCommand.cs` -- conforms to the documented command shape.
- `src/DungeonMaster.Application/Features/Dungeons/Commands/UpdateDungeon/UpdateDungeonCommandValidator.cs` -- conforms to the documented validator shape.
- `src/DungeonMaster.Application/Features/Dungeons/Queries/GetDungeonDetail/AreaDto.cs` -- plain projection record, fully documented.
- `src/DungeonMaster.Application/Features/Dungeons/Queries/GetDungeonDetail/DungeonDetailDto.cs` -- plain projection record, fully documented.
- `src/DungeonMaster.Application/Features/Dungeons/Queries/GetDungeonDetail/FloorDto.cs` -- plain projection record, fully documented.
- `src/DungeonMaster.Application/Features/Dungeons/Queries/GetDungeonDetail/GetDungeonDetailQuery.cs` -- conforms to the documented query shape.
- `src/DungeonMaster.Application/Features/Dungeons/Queries/GetDungeons/DungeonSummaryDto.cs` -- plain projection record, fully documented.
- `src/DungeonMaster.Application/Features/Dungeons/Queries/GetDungeons/GetDungeonsQuery.cs` -- conforms to the documented query shape.

## Method

- Base commit: `45495c1bd39a52dbc942eceab897b3c1ac553ee9`.
- `dm-conventions` documents read:
  - `backend/dotnet-format.md` -- 794 lines, **read in full**.
  - `backend/backend-conventions.md` -- 251 lines, read in full.
  - `backend/dotnet-guidelines.md` -- 532 lines; sections 1 (Async / Await), 4 (Memory & Allocations)
    and 6 (Exception Handling) read in full, the HttpContext and HTTP-client sections skimmed and
    judged inapplicable to a MAUI-hosted stack with no ASP.NET Core surface.
- Microsoft Learn pages consulted (each fetched during this audit):
  - https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/names-of-namespaces
  - https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/general-naming-conventions
  - https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/enum
  - https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection-guidelines
  - https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/nullable-reference-types
  - https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/
  - https://learn.microsoft.com/en-us/dotnet/core/deploying/trimming/incompatibilities
- Tooling, all read-only: `git rev-parse` for the base SHA; `find` and `wc -l` for the scope counts;
  `grep` across `src/` to establish which Domain and Application members have production callers as
  opposed to test-only callers. No build, no test run, no formatter, no analyzer, and no write of any
  kind under `src/`.

## Summary

| Severity | Count |
|---|---|
| S1 | 0 |
| S2 | 5 |
| S3 | 16 |
| S4 | 9 |
| S5 | 3 |
| **Total** | **33** |

## Records

### CORE-001 -- Aggregate mutators silently no-op on an unknown child id and the command still reports success

```yaml
id: CORE-001
layer: CORE
severity: S2
category: correctness
authority: conventions
citation: backend/backend-conventions.md#2. CQRS Patterns
locations:
  - src/DungeonMaster.Domain/Entities/Dungeons/DungeonMethods.cs:61-84
  - src/DungeonMaster.Domain/Entities/Dungeons/DungeonMethods.cs:95-138
  - src/DungeonMaster.Domain/Entities/Dungeons/DungeonMethods.cs:155-171
  - src/DungeonMaster.Domain/Entities/Dungeons/DungeonMethods.cs:215-229
  - src/DungeonMaster.Domain/Entities/Dungeons/DungeonMethods.cs:249-271
  - src/DungeonMaster.Domain/Entities/Dungeons/DungeonMethods.cs:286-303
  - src/DungeonMaster.Domain/Entities/Dungeons/DungeonMethods.cs:392-408
  - src/DungeonMaster.Domain/Entities/Dungeons/DungeonMethods.cs:429-452
  - src/DungeonMaster.Domain/Entities/Floors/FloorMethods.cs:72-84
  - src/DungeonMaster.Domain/Entities/Floors/FloorMethods.cs:101-120
effort: M
risk: medium
independent: true
```

**What.** Every `Dungeon` mutator that takes a `floorId`, `areaId` or `objectId` returns `void` and
returns early when the lookup misses -- `RemoveFloor`, `UpdateFloor`, `RemoveArea`, `UpdateArea`,
`UpdateAreaOutline`, `RemovePlacedObject`, `UpdatePlacedObject` and the two `Floor` counterparts all do
`if (floor is null) { return; }`. `ReorderFloor` additionally returns early when `newNumber` is outside
`1..Count` (`DungeonMethods.cs:114-117`). The corresponding handlers cannot observe the miss and
unconditionally return `Result<bool>.Success(true)`.

**Why it matters.** `backend-conventions.md` section 2 states that a command "returns a typed result
wrapper indicating success or failure with an error type". Here the wrapper reports success for an
operation that did not happen. A stale floor or area id -- from a view model held across a delete, or a
reorder to an out-of-range number -- produces a green result, a `Touch()`-free aggregate and a pointless
`UpdateAsync` round-trip. Today the UI does not go on to show stale state: every dispatch call site in
`DungeonDetailViewModel.cs` discards the `Result` and immediately re-reads the aggregate via
`LoadDungeonAsync(_dungeonId)` (lines 159-163, 176-185, 197-204), so the no-op is corrected on the next
render. That is a property of the caller, not of the contract: the layer's own success signal is
unreliable, and the first caller that trusts it -- or stops reloading -- inherits the defect.

**Suggested direction.** Give the aggregate mutators a `bool` or small outcome return value, as
`Floor.RemoveObject` and `Floor.UpdateObject` already do, and have each handler map a miss to
`ErrorCategory.NotFound`. `ReorderFloor`'s range rejection is a distinct outcome from "floor not
found" and is worth distinguishing.

### CORE-002 -- AddArea throws InvalidOperationException for a missing floor while siblings return a Result

```yaml
id: CORE-002
layer: CORE
severity: S2
category: correctness
authority: conventions
citation: backend/dotnet-guidelines.md#6.1 Do Not Use Exceptions for Normal Control Flow
locations:
  - src/DungeonMaster.Domain/Entities/Dungeons/DungeonMethods.cs:196-198
  - src/DungeonMaster.Application/Features/Dungeons/Commands/AddArea/AddAreaCommandHandler.cs:31-34
effort: S
risk: low
independent: true
```

**What.** `Dungeon.AddArea` resolves the floor with `FirstOrDefault(...) ?? throw new
InvalidOperationException($"Floor {floorId} not found.")`. `AddAreaCommandHandler` calls it without a
`try`/`catch`, so the exception propagates out of the handler, out of `Dispatcher.SendAsync` and into
the caller. The neighbouring `AddPlacedObject` path returns `null` for the same condition and
`RemoveArea` / `UpdateArea` ignore it entirely.

**Why it matters.** `dotnet-guidelines.md` section 6.1 states that a predictable "not found" condition
must be expressed through `Result<T>`, not an exception. There is no dispatcher-level or
composition-root-level exception boundary in this layer, so a bad `FloorId` on `AddArea` escapes the
Result contract that all fourteen commands otherwise advertise. Three different reactions to one
condition also means no caller can write a single, correct error path.

**Suggested direction.** Make `AddArea` return `Area?` -- matching `AddPlacedObject` -- and have the
handler map `null` to `ErrorCategory.NotFound` with the floor id in the message. Then settle on one
convention for "child not found" across the whole aggregate.

### CORE-003 -- ValidationBehavior validates synchronously and never passes the cancellation token

```yaml
id: CORE-003
layer: CORE
severity: S2
category: async
authority: conventions
citation: backend/dotnet-guidelines.md#1.4 Pass CancellationToken Through the Call Stack
locations:
  - src/DungeonMaster.Application/Common/Behaviors/ValidationBehavior.cs:23-37
effort: XS
risk: low
independent: true
```

**What.** `HandleAsync` accepts a `CancellationToken ct` and then calls
`validator.Validate(context)` -- FluentValidation's synchronous entry point, which takes no token. The
token is used nowhere in the method; it is only forwarded implicitly by `await next()`.

**Why it matters.** `dotnet-guidelines.md` section 1.4 requires that a `CancellationToken` accepted by
an async method be propagated to every awaited call. Beyond the rule, the synchronous overload throws
if any registered validator ever declares an async rule (`MustAsync`, `CustomAsync`), so the first
async validation rule added to this codebase fails at runtime inside a method whose signature already
promised async execution.

**Suggested direction.** Call `await validator.ValidateAsync(context, ct)` and aggregate the results,
or iterate the validators with `await` rather than composing them through a synchronous LINQ chain.

### CORE-004 -- Entity namespaces share the name of the type they contain, forcing Floor.Floor qualification

```yaml
id: CORE-004
layer: CORE
severity: S2
category: naming
authority: microsoft
citation: https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/names-of-namespaces
locations:
  - src/DungeonMaster.Domain/Entities/Dungeon/Dungeon.cs:1-6
  - src/DungeonMaster.Domain/Entities/Floor/Floor.cs:3-8
  - src/DungeonMaster.Domain/Entities/Area/Area.cs:3-8
  - src/DungeonMaster.Domain/Entities/Dungeon/DungeonMethods.cs:44-47
  - src/DungeonMaster.Domain/Entities/Dungeon/DungeonMethods.cs:323-333
  - src/DungeonMaster.Domain/Entities/Floor/FloorMethods.cs:51-58
effort: M
risk: medium
independent: true
```

**What.** `namespace DungeonMaster.Domain.Entities.Dungeon` contains a type named `Dungeon`; the same
holds for `Floor` and `Area`. As a direct consequence, `DungeonMethods.cs` must write
`Floor.Floor`, `Floor.PlacedObject`, `Floor.ObjectKind`, `Floor.ObjectRotation`, `Floor.ObjectScale`
and `Area.Area` throughout, and `FloorMethods.cs` must write `Area.Area`.

**Why it matters.** The Microsoft namespace-naming guideline is explicit: "DO NOT use the same name
for a namespace and a type in that namespace... Several compilers require such types to be fully
qualified." That prediction is exactly what happened here -- roughly forty qualified references in
`DungeonMethods.cs` alone. The cost compounds: every new Domain type inside these folders inherits the
qualification tax, and `Floor.ObjectScale` reads as though `ObjectScale` were nested inside the `Floor`
entity when it is a sibling static class.

**Suggested direction.** Either flatten to `DungeonMaster.Domain.Entities` (keeping the per-entity
folders but not the per-entity namespace, which the `dm-conventions` folder layout does not require),
or pluralise the namespace leaf (`Entities.Floors`) so that folder structure and namespace can both
survive. Either change touches the Application and App layers' `using` lists, hence the medium risk.

**[AMENDED 6b-v, 2026-09-14 -- the flattening option's parenthetical is false; the `dm-conventions`
folder layout does require the namespace to match it. Classification: MEASUREMENT ERROR.** Previous
wording, verbatim: *"(keeping the per-entity folders but not the per-entity namespace, which the
`dm-conventions` folder layout does not require)"*. Two rules contradict it.
`dm-conventions/backend/dotnet-format.md:30` gives the convention for a namespace as *"PascalCase,
matches folder structure"*, and `dm-conventions/backend/dotnet-format.md:58` states *"Folder
structure mirrors the namespace hierarchy."* Flattening every type into
`DungeonMaster.Domain.Entities` while keeping the per-entity folders breaks both at once: the
per-entity folder would correspond to no namespace segment, so the namespace would not match the
folder structure and the folder structure would not mirror the namespace hierarchy.

**Classification evidence: MEASUREMENT ERROR, not DRIFT.** The claim was false when written rather
than overtaken by a later change to the conventions. Both rules sit in section 2, *File
Organization*, which spans `dotnet-format.md:54-61` (`## 2. File Organization` at `:54`, `## 3.
Parameter Formatting` at `:62`), and this document was written against that section: `CORE-023`
below cites `backend/dotnet-format.md#2. File Organization` and quotes its bullet at `:57`, the line
immediately above the folder-mirroring rule at `:58`. The Method block at `:57` of this document
also records `backend/dotnet-format.md` as *"794 lines, **read in full**"*, and the file is 794
lines today (`wc -l` -> `794`). The section whose requirement the parenthetical denies was therefore
both present and read at authorship.

**Outcome.** The flattening option is **REJECTED**, at this session's Gate 1 (sub-session 6b-v,
`2026-09-14-pluralise-entity-namespaces`), for breaking those two rules. The second alternative this
same paragraph offers -- **pluralise the namespace leaf** -- was taken: the three entity namespace
leaves and their three folders were pluralised together, so that both rules continue to hold. The
record's severity, category, effort and risk are unchanged, and its `locations:` block above
deliberately keeps its singular paths: it records where the defect was found when the audit observed
it, and the "What." and "Why it matters." paragraphs likewise describe the code as it then stood.**]**

### CORE-005 -- Result<T>.Value is T? on an unconstrained T, so null on failure is unreachable for value types

```yaml
id: CORE-005
layer: CORE
severity: S3
category: api-design
authority: microsoft
citation: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/nullable-reference-types
locations:
  - src/DungeonMaster.Application/Common/Results/Result.cs:9
  - src/DungeonMaster.Application/Common/Results/Result.cs:16-19
effort: S
risk: low
independent: true
```

**What.** `Result<T>` declares `public T? Value { get; init; }` with no constraint on `T`, and its XML
summary says the property is "the value returned by the operation, or `null` when the operation
failed". Every command in this layer closes `T` as `Guid` or `bool`, both value types.

**Why it matters.** The Microsoft nullable-reference-types reference states that for an unconstrained
type parameter with a value-type argument, "`T` is `int` and `T?` is also `int` -- the annotation has
no effect on value types unless the type parameter has the `struct` constraint". So on failure
`Value` is `Guid.Empty` / `false`, never `null`, and the documentation misdescribes the type for
fourteen of the sixteen dispatchable operations. Callers are pushed into sentinel checks; the App layer
already does exactly that (`ViewModels/DungeonDetailViewModel.cs:302` tests
`result.Value == Guid.Empty`), which is a null-check surrogate that would silently mis-fire the day a
command legitimately returns an empty `Guid`.

**Suggested direction.** Either constrain the story -- expose `TryGetValue(out T value)` or a
`Match`-style accessor that cannot be read on a failed result -- or, at minimum, correct the XML
summary to state that `Value` holds `default(T)` on failure and stop callers from treating the default
as a failure signal.

**[AMENDED 6b-xii, 2026-09-18 -- four of this entry's seven claims were stale or false when first
measured. The defect is real and smaller than described; the entry's core survives.** Every figure
below names the command that produced it, re-measured in the session worktree after this session's
three code commits landed. As with `CORE-004` above, the `locations:` block and the "What.", "Why it
matters." and "Suggested direction." paragraphs are left exactly as written: they record where the
defect was found and how the code then stood.

**Classification: DRIFT, not measurement error -- and the drift has a named cause.** None of these
claims went stale because anyone worked on this entry. `CORE-006`, the entry immediately below, proposed
"adding a non-generic `Result` (or a `Unit` payload) for commands with no return value, and making the
properties `private init`". **Both landed.** That single fix, aimed at a neighbour, narrowed this
entry's premise, changed its accessors, and moved its type to another file. Nothing in this audit
process re-measures an entry when the entry beside it is fixed, so `MASTER-044` sat with four wrong
claims until this session measured it. **The next `CORE-00x` fix can falsify a neighbouring entry the
same way, silently.**

*Correction 1 -- the location.* The `locations:` block cites
`src/DungeonMaster.Application/Common/Results/Result.cs:9` and `Result.cs:16-19`. The generic type is
not in that file at all. `Result.cs:6` declares the **non-generic** `public sealed record Result`, and
that type declares `IsSuccess` at `Result.cs:15` and `IsFailure` at `Result.cs:20-21` -- **not**
at `:16-19`, which holds a blank line and `IsFailure`'s doc comment and so contains neither
declaration. (Corrected post-merge by 6b-xii's orchestrator: the amendment as first written
reproduced, at this very line, the defect class it exists to correct.) `Result<T>` is declared at
**`src/DungeonMaster.Application/Common/Results/ResultOfT.cs:9`** (`grep -n "record Result<T>"`).

*Correction 2 -- the accessors.* The "What." paragraph describes `public T? Value { get; init; }`. The
unconstrained `T` holds; the accessor does not. Every property on the type is **`private init`**:
`grep -c "private init" src/DungeonMaster.Application/Common/Results/ResultOfT.cs` returns **5**
(`IsSuccess` `:18`, `Value` `:43`, `Error` `:48`, `ErrorCategory` `:53`, `ValidationErrors` `:58`), and
`grep -c "{ get; init; }"` on the same file returns **0**. This claim was already falsified by
`CORE-006`, whose fix made them private.

*Correction 3 -- the population.* The "Why it matters." paragraph says the documentation "misdescribes
the type for fourteen of the sixteen dispatchable operations". **Sixteen is still right; fourteen is
not.** Over `src/DungeonMaster.Application` with `/obj/` filtered: `": ICommand<Guid>;"` returns **4**,
`": ICommand;"` returns **10**, `": IQuery<.*;"` returns **2** -- 4 + 10 + 2 = 16. Only the **4** that
close `T` use `Result<T>` at all. The 10 non-generic commands return the non-generic `Result`, and the
two queries were never in the population: `IDispatcher.cs` declares `QueryAsync<TResult>` returning
`Task<TResult>` bare, not `Task<Result<TResult>>`, so a query never constructs a `Result<T>`. So the
figure is **4 of 16**, and it is 4 rather than 14 because of `CORE-006`'s fix.
**The near-miss this count must exclude** is the generic constraint form `where TCommand :
ICommand<TResult>`: the unqualified `": ICommand<"` returns **7**, of which **3** are that form
(`ValidationBehavior.cs:56`, `Dispatcher.cs:154`, `ICommandHandler.cs:41`). Anchoring on the trailing
semicolon excludes all three and leaves 4.

*Correction 4 -- `bool` is gone.* The "What." paragraph's "Every command in this layer closes `T` as
`Guid` or `bool`, both value types" is **half false: no command closes `T` as `bool` any more.** The
`bool` commands became the non-generic `Result` under `CORE-006`.
`grep -rn "Result<bool>\|ICommand<bool>" --include=*.cs src/`, `/obj/` and `/bin/` filtered, returns
**exactly one line** -- `src/DungeonMaster.Tests/App/AsyncDiscipline/SyncOverAsyncIsAbsentTests.cs:62`
-- and that line is the substring `IPopupResult<bool>` inside a constructed string literal in a
source-scanning test, not a use of `Result<bool>`. **Read against that one near-miss, n = 0.**
Counted a second way that could not share the first's mistake,
`grep -rhno "Result<[A-Za-z0-9_?<>, .]*>"` over the four production projects yields `Result<Guid>` (6)
and `Result<Guid>>` (4) as the **only** closed constructions; every other form is generic plumbing
(`Result<T>`, `Result<TResult>` and variants, `Result<>`). **`Result<Guid>` is the only closed
construction in production**, so `Guid.Empty` is the only failure default this defect can produce. The
two counts agree.

*Correction 5 -- the sentinel citation.* The "Why it matters." paragraph cites
`ViewModels/DungeonDetailViewModel.cs:302`. That line cannot exist: the file is **202** lines
(`b.count(b'\n')`, read in binary). The sentinel that did exist was
**`src/DungeonMaster.App/ViewModels/MapCanvasViewModel.cs:213`**, `if (addResult.Value == Guid.Empty)
{ return; }`, and this session deleted it. Whether `:302` was wrong when written or the file shrank
afterwards is **not settled here** -- this record's author could not read git history, so the
classification of this one citation is left open rather than guessed.

**The sentinel was unreachable, verified end to end rather than asserted.** It sat *after* the
`IsFailure` early return, so only the success path reached it, and on that path
`AddAreaCommandHandler.cs:33` returns `area.Id`, where `area` comes from `DungeonMethods.cs:216` ->
`FloorMethods.cs:68-71` (`new Areas.Area(Id, name, description)`; `:64` is the `AddArea(` signature), whose constructor never assigns `Id`,
leaving `Entity.cs:11`'s `= Guid.NewGuid()`. `MutateAsync<TResult>` was checked too and never
synthesises `Success(default)`. So the entry's warning that the sentinel "would silently mis-fire the
day a command legitimately returns an empty `Guid`" is right in principle; what it actually did was
return silently, reporting no error, on a branch nothing reached. The legitimate read survives at
`MapCanvasViewModel.cs:217`, passing the new area id to `UpdateAreaOutlineCommand`.

**The suite pinned the defect as a requirement, which is why closing this entry needed a test change.**
`ResultOfTTests.cs` carried a fact named `Failure_WithCategory_IsNotSuccessAndCarriesNoValue`, with an
XML summary saying "no value", whose assertion was `result.Value.Should().Be(Guid.Empty)`. The name and
the summary denied what the assertion required. It is renamed to `...CarriesTheDefaultValue`, its
summary corrected, and its assertion written as `Be(default(Guid))` so the literal states the intent.

**Outcome -- the "at minimum" half of the "Suggested direction." was taken; the first half was
considered and NOT adopted.** `TryGetValue(out T value)` and a `Match`-style accessor were both
weighed and rejected on repository evidence: `grep -rn "bool Try[A-Za-z]*(" --include=*.cs src/`, with
`/obj/` and `/bin/` filtered, returns **0 lines**. **This repository declares no `bool Try...(out T)`
accessor of its own anywhere, production or test, so adopting one here would have introduced a pattern
with no precedent** -- and `n = 0` is reported as the absence of a precedent, not as a convention
against it. The predicate was validated against a synthetic positive,
`"public bool TryGetValue(out int x)"`, which it matches, and a synthetic near-miss,
`"public bool IsSuccess(out int x)"`, which it does not. Two narrower facts, recorded so a later reader
does not over-read the `0`: `Try`-**named** members do exist (`DmInlineEditor.xaml.cs:275` declares
`public void TryCommit()`, correctly excluded as not the `bool`-returning out-parameter pattern), and
`Try`-shaped **calls** into the BCL and EF Core are common -- `grep -rn "Try[A-Za-z]*("` over the same
scope returns **30** lines, among them `Enum.TryParse` at
`src/DungeonMaster.App/Assets/ObjectAssets/ObjectPackRegistry.cs:24`.

**`T?` on an unconstrained `T` is left exactly as it is, by the user's decision at this session's Gate
2.** It is the correct C# annotation for "may be the default"; the Microsoft reference this entry cites
is describing the annotation working as specified, not a bug in it. **The defect was the prose, and the
fix is prose.** `ResultOfT.cs`'s `Value` summary now states that a failed result carries
`<see langword="default"/>` of `T` because the failure factories never assign the property, that the
default is not a failure signal and must never be read as one, that `IsSuccess` / `IsFailure` are the
only failure signals, and that for `Result<Guid>` the failure value is `Guid.Empty`, indistinguishable
from a Guid a success could return. `grep -c "when the operation failed"` on that file now returns
**0**.

**A new source-scanning fact pins the sentinel's absence**, in
`src/DungeonMaster.Tests/App/ViewModels/ResultSentinelIsAbsentTests.cs`, guarded against scanning
nothing and shown able to flag the removed form. **Its reach is narrower than its name suggests and is
recorded here so nobody mistakes it for a general guard:** it detects the single-line comparison form
this session removed, and it does **not** catch `.Equals(Guid.Empty)`, an indexer or call receiver, a
comparison against a local copy of the value, `new Guid()`, a property pattern, or any form split
across lines. It pins this defect's shape, not the class.

The record's severity, category, effort and risk are unchanged.**]**

### CORE-006 -- Result<T> permits invalid states and every state-changing command returns an always-true bool

```yaml
id: CORE-006
layer: CORE
severity: S3
category: api-design
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.Application/Common/Results/Result.cs:11-29
  - src/DungeonMaster.Application/Features/Dungeons/Commands/DeleteDungeon/DeleteDungeonCommandHandler.cs:19-23
  - src/DungeonMaster.Application/Features/Dungeons/Commands/RemoveFloor/RemoveFloorCommandHandler.cs:31-37
effort: M
risk: medium
independent: true
```

**What.** All four `Result<T>` members are `public ... { get; init; }`, so
`new Result<Guid> { IsSuccess = true, Error = "boom" }` is constructible alongside the two intended
factories. Separately, nine of the fourteen commands are declared `ICommand<bool>` and their handlers
end in `return Result<bool>.Success(true);` with no path that ever yields `Success(false)`;
`DeleteDungeonCommandHandler` returns success even when `DeleteAsync` deleted nothing.

**Why it matters.** I believe the `bool` payload is carrying no information -- it appears to be a
stand-in for "no return value", and a reader has to check each handler to discover that `false` is
unreachable. The public `init` accessors appear to leave the invariant "success implies no error"
enforced only by convention rather than by the type. Both make the error model weaker than the
discipline the rest of the layer shows.

**Suggested direction.** Consider adding a non-generic `Result` (or a `Unit` payload) for commands with
no return value, and making the properties `private init` / positional so the static factories are the
only way to construct one.

### CORE-007 -- Two ErrorCategory members are never produced and no production caller reads the category

```yaml
id: CORE-007
layer: CORE
severity: S3
category: api-design
authority: conventions
citation: backend/backend-conventions.md#2. CQRS Patterns
locations:
  - src/DungeonMaster.Application/Common/Results/ErrorCategory.cs:6-27
  - src/DungeonMaster.Application/Common/Results/Result.cs:26-29
effort: S
risk: low
independent: true
```

**What.** `ErrorCategory` declares `Validation`, `NotFound`, `Conflict` and `Unauthorized`. A
repository-wide search finds `Conflict` and `Unauthorized` produced nowhere, `Forbidden` -- listed in
the conventions' error-category set -- absent, and `Result.ErrorCategory` read only by the test
project; the three App view models that consume results read `IsSuccess` and `Error` only.

**Why it matters.** `backend-conventions.md` section 2 defines the category set as
`(Validation, NotFound, Conflict, Unauthorized, Forbidden)` and gives the category a job: the calling
layer maps it to a status. In this application nothing maps it, so the category is a write-only field
that costs a decision at every failure site and buys nothing today. Two of its four values have never
had a producer, which makes the enum look larger and more considered than it is.

**Suggested direction.** Either give the category a consumer -- a single result-to-message mapping the
view models share -- or reduce the enum to the values that are actually produced and grow it again
when a caller needs the distinction. Align the member set with the conventions either way.

### CORE-008 -- ValidationBehavior discards every validation failure except the first

```yaml
id: CORE-008
layer: CORE
severity: S3
category: api-design
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.Application/Common/Behaviors/ValidationBehavior.cs:30-44
effort: S
risk: low
independent: false
blocked-by:
  - CORE-006
```

**What.** The behavior materialises every failure from every registered validator into a `List`, then
returns `Result<TResult>.Failure(failures[0].ErrorMessage, ErrorCategory.Validation)` -- a single
string built from the first element. The remaining failures, and the `PropertyName` of every failure,
are dropped.

**Why it matters.** I believe this makes per-field error display impossible for any caller: the UI
receives one sentence with no indication of which field it belongs to, so a form with three invalid
fields must be corrected and resubmitted three times. The cost is currently masked because every
validator in the layer has exactly one rule chain, but it will surface the first time a command
carries two validated fields.

**Suggested direction.** Carry the failures as structured data on the failed result -- a
property-name-to-messages map alongside the summary string -- so the single-message path stays
available for simple callers while richer callers can bind per field.

### CORE-009 -- Ten of the fourteen commands have no validator, making ValidationBehavior a no-op for them

```yaml
id: CORE-009
layer: CORE
severity: S3
category: structure
authority: conventions
citation: backend/backend-conventions.md#2. CQRS Patterns
locations:
  - src/DungeonMaster.Application/DependencyInjection.cs:45-49
  - src/DungeonMaster.Application/Features/Dungeons/Commands/AddFloor/AddFloorCommand.cs:14-17
  - src/DungeonMaster.Application/Features/Dungeons/Commands/AddPlacedObject/AddPlacedObjectCommand.cs:28-35
  - src/DungeonMaster.Application/Features/Dungeons/Commands/DeleteDungeon/DeleteDungeonCommand.cs:11-13
  - src/DungeonMaster.Application/Features/Dungeons/Commands/RemoveArea/RemoveAreaCommand.cs:17-21
  - src/DungeonMaster.Application/Features/Dungeons/Commands/RemoveFloor/RemoveFloorCommand.cs:14-17
  - src/DungeonMaster.Application/Features/Dungeons/Commands/RemovePlacedObject/RemovePlacedObjectCommand.cs:17-21
  - src/DungeonMaster.Application/Features/Dungeons/Commands/ReorderFloor/ReorderFloorCommand.cs:17-21
  - src/DungeonMaster.Application/Features/Dungeons/Commands/UpdateAreaOutline/UpdateAreaOutlineCommand.cs:21-26
  - src/DungeonMaster.Application/Features/Dungeons/Commands/UpdateFloor/UpdateFloorCommand.cs:23-29
  - src/DungeonMaster.Application/Features/Dungeons/Commands/UpdatePlacedObject/UpdatePlacedObjectCommand.cs:28-35
effort: M
risk: low
independent: true
```

**What.** `DependencyInjection.cs` registers four validators -- for `CreateDungeon`, `UpdateDungeon`,
`AddArea` and `UpdateArea`. The other ten commands dispatch through `ValidationBehavior` with an empty
`IEnumerable<IValidator<TCommand>>`, so the behavior resolves, allocates a `ValidationContext`, finds
zero failures, and calls `next()`.

**Why it matters.** `backend-conventions.md` section 2 states that a command "has a corresponding
validator that runs before the handler". The gaps are not cosmetic: `ReorderFloorCommand.NewNumber` is
an unbounded `int` whose out-of-range values are then silently swallowed by the aggregate (CORE-001),
`AddPlacedObjectCommand.Scale` is an unbounded `float` whose only guard is an
`ArgumentOutOfRangeException` thrown deep inside `PlacedObject`, and `UpdateAreaOutlineCommand.Outline`
is an unchecked `IReadOnlyList` that may be empty or null.

**Suggested direction.** Add validators for the commands that carry real input -- at minimum the
numeric ranges on `ReorderFloor`, `AddPlacedObject` and `UpdatePlacedObject`, and non-empty checks on
`UpdateAreaOutline` -- and consider registering validators by assembly scan so a new command cannot be
added without one being noticed.

### CORE-010 -- Dispatcher is a service locator over a hand-maintained handler registration list

```yaml
id: CORE-010
layer: CORE
severity: S3
category: structure
authority: microsoft
citation: https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection-guidelines
locations:
  - src/DungeonMaster.Application/Common/Dispatch/Dispatcher.cs:9-11
  - src/DungeonMaster.Application/Common/Dispatch/Dispatcher.cs:18-34
  - src/DungeonMaster.Application/Common/Dispatch/Dispatcher.cs:59-64
  - src/DungeonMaster.Application/DependencyInjection.cs:51-101
effort: M
risk: medium
independent: true
```

**What.** `Dispatcher` takes `IServiceProvider` and calls `GetRequiredService(handlerType)` /
`GetServices(behaviorType)` on a `Type` built at run time via `MakeGenericType`. The sixteen
handler registrations it depends on are written out by hand in `DependencyInjection.cs`, each one
repeating the closed generic pair.

**Why it matters.** The Microsoft DI guidelines list "Avoid using the *service locator pattern*" among
their recommendations. The concrete cost here is that the dispatcher's dependencies are invisible: a
command whose handler registration is forgotten compiles cleanly, passes every unit test that
constructs the handler directly, and throws `InvalidOperationException` only when a user triggers that
one screen. The hand-maintained list makes the omission easy -- adding a command means editing three
files in the right order.

**Suggested direction.** Register handlers by scanning the Application assembly for closed
`ICommandHandler<,>` / `IQueryHandler<,>` implementations, and add a startup self-check that asserts
every `ICommand<>` in the assembly has exactly one registered handler. That keeps the locator inside
`Dispatcher` -- which is unavoidable for a runtime-dispatched design -- while removing the class of bug
it enables.

### CORE-011 -- Every dispatch performs reflection and dynamic binding with no caching of the closed handler type

```yaml
id: CORE-011
layer: CORE
severity: S3
category: performance
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.Application/Common/Dispatch/Dispatcher.cs:18-28
  - src/DungeonMaster.Application/Common/Dispatch/Dispatcher.cs:36-49
  - src/DungeonMaster.Application/Common/Dispatch/Dispatcher.cs:59-68
effort: M
risk: medium
independent: false
blocked-by:
  - CORE-010
```

**What.** `SendAsync` calls `MakeGenericType` twice per dispatch, resolves the handler into a
`dynamic` local, casts the command to `dynamic` at each invocation, materialises the behavior list with
`GetServices(...).Cast<dynamic>().Reverse().ToList()`, and allocates one closure per behavior. Nothing
is memoised across calls. `QueryAsync` repeats the pattern without the behavior loop.

**Why it matters.** I believe this is the main cost the hand-rolled dispatcher is paying, and it lands
on a path that the map editor drives interactively -- `UpdatePlacedObjectCommand` is dispatched while
the user drags an object. Each dispatch pays reflection, a DLR call-site setup on first use per
call-site shape, and several allocations that a compiled generic path would not need. I also believe
`dynamic` is a portability risk worth verifying for the `net10.0-maccatalyst` target, since the Native
AOT and trimming documentation both treat runtime code generation as unsupported; I could not confirm
from an official page that the C# runtime binder is affected in this specific configuration, so I state
it as a concern rather than a fact.

**Suggested direction.** Cache the closed handler and behavior types in a
`ConcurrentDictionary<Type, ...>` keyed by the command's runtime type, and store a compiled delegate
rather than re-binding through `dynamic` on every call. If AOT compatibility turns out to matter,
a source-generated dispatch table removes both reflection and `dynamic` at once.

### CORE-012 -- Queries bypass the pipeline entirely; IPipelineBehavior applies only to commands

```yaml
id: CORE-012
layer: CORE
severity: S3
category: structure
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.Application/Common/Dispatch/Dispatcher.cs:55-69
  - src/DungeonMaster.Application/Common/Dispatch/IPipelineBehavior.cs:14-16
  - src/DungeonMaster.Application/Common/Dispatch/IQueryHandler.cs:12-14
effort: M
risk: low
independent: true
```

**What.** `IPipelineBehavior<TCommand, TResult>` is constrained `where TCommand : ICommand<TResult>`,
and `QueryAsync` resolves the query handler and awaits it directly with no behavior lookup. There is no
query-side equivalent of the behavior interface.

**Why it matters.** The composition itself is correct -- I checked the ordering, and reversing the
behavior list before folding it into the pipeline does produce first-registered-outermost, with the
per-iteration `next` capture handled properly. The structural gap is elsewhere: any cross-cutting
concern that should cover reads as well as writes (logging, timing, caching, tenant filtering) has
nowhere to attach, so the first such requirement will either be duplicated into every query handler or
force a second pipeline to be built.

**Suggested direction.** I believe the cheapest correction is to generalise the behavior interface over
a request abstraction shared by `ICommand<T>` and `IQuery<T>`, keeping the `Result<T>` wrapper on the
command path only. That is a larger change than it looks, so it is worth deferring until a concern
actually needs to span both sides.

### CORE-013 -- Fourteen command handlers repeat one load / null-check / mutate / save skeleton

```yaml
id: CORE-013
layer: CORE
severity: S3
category: duplication
authority: practice
citation: Fowler, Refactoring 2e, ch. 3 "Duplicated Code"
locations:
  - src/DungeonMaster.Application/Features/Dungeons/Commands/AddArea/AddAreaCommandHandler.cs:16-41
  - src/DungeonMaster.Application/Features/Dungeons/Commands/AddFloor/AddFloorCommandHandler.cs:16-38
  - src/DungeonMaster.Application/Features/Dungeons/Commands/AddPlacedObject/AddPlacedObjectCommandHandler.cs:16-50
  - src/DungeonMaster.Application/Features/Dungeons/Commands/CreateDungeon/CreateDungeonCommandHandler.cs:16-29
  - src/DungeonMaster.Application/Features/Dungeons/Commands/DeleteDungeon/DeleteDungeonCommandHandler.cs:15-24
  - src/DungeonMaster.Application/Features/Dungeons/Commands/RemoveArea/RemoveAreaCommandHandler.cs:16-40
  - src/DungeonMaster.Application/Features/Dungeons/Commands/RemoveFloor/RemoveFloorCommandHandler.cs:16-38
  - src/DungeonMaster.Application/Features/Dungeons/Commands/RemovePlacedObject/RemovePlacedObjectCommandHandler.cs:17-41
  - src/DungeonMaster.Application/Features/Dungeons/Commands/ReorderFloor/ReorderFloorCommandHandler.cs:16-40
  - src/DungeonMaster.Application/Features/Dungeons/Commands/UpdateArea/UpdateAreaCommandHandler.cs:16-43
  - src/DungeonMaster.Application/Features/Dungeons/Commands/UpdateAreaOutline/UpdateAreaOutlineCommandHandler.cs:15-40
  - src/DungeonMaster.Application/Features/Dungeons/Commands/UpdateDungeon/UpdateDungeonCommandHandler.cs:16-41
  - src/DungeonMaster.Application/Features/Dungeons/Commands/UpdateFloor/UpdateFloorCommandHandler.cs:16-42
  - src/DungeonMaster.Application/Features/Dungeons/Commands/UpdatePlacedObject/UpdatePlacedObjectCommandHandler.cs:17-44
effort: M
risk: medium
independent: false
blocked-by:
  - CORE-001
```

**What.** Twelve of the fourteen handlers repeat the same skeleton, varying only in the aggregate call
in the middle: `LoadAsync(id, ct)`, `if (dungeon is null) return Failure($"Dungeon {id} not found.",
NotFound)`, one mutator call, `UpdateAsync(dungeon, ct)`, `Success(...)`. The not-found message string
is written out twelve times. Eleven of the twelve are identical outside the mutator call;
`AddPlacedObjectCommandHandler` adds one further guard on the returned object
(`AddPlacedObjectCommandHandler.cs:38-43`). Only `CreateDungeon` and `DeleteDungeon` differ in shape.

**Why it matters.** This is the "Duplicated Code" smell at its most literal: the load-and-guard
prologue is a policy, and it currently has twelve copies. Any change to it -- adding concurrency
handling, a domain-event dispatch after `UpdateAsync`, a transaction scope, or the outcome-aware
handling that CORE-001 requires -- has to be applied twelve times with no compiler assistance if one is
missed.

**Suggested direction.** Introduce a small abstract base or a helper (for example
`writeRepository.MutateAsync(id, dungeon => ..., ct)`) that owns load, guard, mutate and save, leaving
each handler with just its aggregate call. Land it together with CORE-001 so the shared prologue is
written once against the new outcome-returning mutators rather than twice.

### CORE-014 -- AddPlacedObject reports a missing floor as a Validation failure with an opaque message

```yaml
id: CORE-014
layer: CORE
severity: S3
category: api-design
authority: conventions
citation: backend/backend-conventions.md#2. CQRS Patterns
locations:
  - src/DungeonMaster.Application/Features/Dungeons/Commands/AddPlacedObject/AddPlacedObjectCommandHandler.cs:6-10
  - src/DungeonMaster.Application/Features/Dungeons/Commands/AddPlacedObject/AddPlacedObjectCommandHandler.cs:38-43
  - src/DungeonMaster.Domain/Entities/Dungeons/DungeonMethods.cs:363-369
effort: XS
risk: low
independent: false
blocked-by:
  - CORE-001
```

**What.** `Dungeon.AddPlacedObject` returns `null` when the floor id does not match, and the handler
turns that into `Result<Guid>.Failure("Failed to add placed object.", ErrorCategory.Validation)`. The
handler's own XML summary documents this as "a validation failure if the floor is not found". The
message names neither the floor nor the reason.

**Why it matters.** `backend-conventions.md` section 2 gives the error category a routing job -- the
calling layer maps each category onto an outcome. A not-found condition labelled `Validation` routes to
the wrong outcome and tells the user their input was malformed when in fact a referenced entity is
gone. The message carries no id, so a support report built from it is not actionable.

**Suggested direction.** Return `ErrorCategory.NotFound` with `$"Floor {command.FloorId} not found."`,
matching the dungeon-level message already used in the same method, and correct the class summary.

### CORE-015 -- Dungeon is a 453-line delegation facade that re-implements Floor's API rather than exposing it

```yaml
id: CORE-015
layer: CORE
severity: S3
category: structure
authority: practice
citation: Fowler, Refactoring 2e, ch. 3 "Middle Man"
locations:
  - src/DungeonMaster.Domain/Entities/Dungeons/DungeonMethods.cs:155-171
  - src/DungeonMaster.Domain/Entities/Dungeons/DungeonMethods.cs:191-204
  - src/DungeonMaster.Domain/Entities/Dungeons/DungeonMethods.cs:215-229
  - src/DungeonMaster.Domain/Entities/Dungeons/DungeonMethods.cs:249-271
  - src/DungeonMaster.Domain/Entities/Dungeons/DungeonMethods.cs:286-303
  - src/DungeonMaster.Domain/Entities/Dungeons/DungeonMethods.cs:356-380
  - src/DungeonMaster.Domain/Entities/Dungeons/DungeonMethods.cs:392-408
  - src/DungeonMaster.Domain/Entities/Dungeons/DungeonMethods.cs:429-452
effort: L
risk: high
independent: true
```

**What.** `DungeonMethods.cs` is the largest file in the Domain at 453 lines. Eight of its eleven
methods consist of the same three steps: `FirstOrDefault` the floor by id, return or throw if absent,
forward to the identically named `Floor` method, then `Touch()`. The forwarded methods
(`UpdateArea`, `UpdateAreaOutline`, `RemoveArea`, `AddArea`, `AddObject`, `RemoveObject`,
`UpdateObject`) already exist on `Floor` with the same parameter lists minus `floorId`.

**Why it matters.** This is the Middle Man smell: the aggregate root's public surface is mostly
pass-through, and every new `Floor` or `Area` behaviour must be mirrored upward with a hand-written
lookup-and-forward wrapper. The XML documentation is mirrored too, which is why a large share of the
file is duplicated prose. Aggregate-root gatekeeping is a legitimate reason to route through
`Dungeon`, but the current form pays the full cost of the indirection without concentrating any
invariant in it -- the wrappers add only a `Touch()` and a null guard.

**Suggested direction.** Extract the repeated `floorId` lookup into a single private
`RequireFloor(Guid floorId)` helper so each wrapper collapses to two lines, or expose a narrow
floor-scoped accessor from the root and let callers work against `Floor` directly for behaviour that
carries no cross-floor invariant. High risk because this is the aggregate's whole public surface.

**[AMENDED 6b-xiv, 2026-09-29 -- three of this entry's figures are corrected, two duplications it never
mentions are recorded, and its second suggested direction is struck as never viable. Classification:
PARTIALLY FALSIFIED.** The entry's substance stands and this session acted on its **first** suggested
direction. As with `CORE-005` and `CORE-019` in this file, the `locations:` block and the `What.`,
`Why it matters.` and `Suggested direction.` paragraphs are left exactly as written -- they record how
the code then stood -- and every correction below is recorded here rather than edited into them.
**The task that wrote this block could not invoke `git`.** Every figure attributed to `16d594b` or to
any other historical commit was measured by this session's orchestrator at Gate 1 and is recorded in
`measurement.md` sections 1 and 3 of
`.dir-sdlc/sessions/2026-09-29-dungeon-facade-lookup-extraction/`, each with the command that produced
it. Nothing here is re-derived or inferred from a neighbouring figure.

*Correction 1 -- "453 lines" was EXACT at authorship, so the figure is STALE and not an error.*
`git show 16d594b:src/DungeonMaster.Domain/Entities/Dungeon/DungeonMethods.cs | wc -l` returns **453**
at `16d594b`, the commit that authored this document on 2026-08-17. A binary `b.count(b'\n')` over
`src/DungeonMaster.Domain/Entities/Dungeons/DungeonMethods.cs` at this session's baseline `20b0b09`
returns **489**. **Note the path differs at `16d594b`** -- `Entities/Dungeon/`, singular, before the
namespace pluralisation at `070d535`. The entry was correct when it was written and has drifted since.

*Correction 2 -- "eight of its eleven methods": "eleven" was WRONG WHEN WRITTEN; "eight" is right.*
A public-method census -- a regex over `^\tpublic <return type> <Name>(`, excluding the constructor,
run against both commits -- returns **13** public methods at `16d594b` and **12** at `20b0b09`. **No
reading of either commit yields eleven.** The 12 were re-derived independently at `20b0b09` with
`grep -nE "^\tpublic "`, which returns exactly 12 lines -- `Update`, `AddFloor`, `RemoveFloor`,
`ReorderFloor`, `UpdateFloor`, `AddArea`, `RemoveArea`, `UpdateArea`, `UpdateAreaOutline`,
`AddPlacedObject`, `RemovePlacedObject`, `UpdatePlacedObject` -- and each declaration is reproduced
verbatim in `contracts/dungeon-public-surface.md` section 1 of this session's artifacts. The 13th at
`16d594b` was the second `AddPlacedObject` overload, deleted by 6b-xiii at `a9a5935`. **This
correction is a different kind from Correction 1, and the difference is the point of writing both
down:** the line count decayed with the code; this figure never held at any commit.

*Correction 3 -- the lookup duplication is TEN-fold, not eight, and the entry's undercount is not an
error in it.* The repeated `floorId` lookup occurs **10** times in `DungeonMethods.cs` at `20b0b09`,
not the 8 that the entry's "eight of its ... methods" framing implies. Measured twice, by commands
that could not share a mistake: `grep -c "FirstOrDefault"` returns **10**, and a Python read with
`newline=''` counting the byte-exact two-line expression `_floors.FirstOrDefault(floor =>` followed by
`floor.Id == floorId);` also returns **10**, as does a count of `if (floor is null)`. **Predicate
control:** `Update` and `AddFloor` are the only two methods with no lookup -- confirmed by reading
`:19-53`, where neither touches `_floors.FirstOrDefault` -- and `10 + 2 = 12` agrees with the method
count in Correction 2. **Why the entry undercounts:** `RemoveFloor` and `ReorderFloor` carry the
identical `FirstOrDefault` and null-guard but then run their own renumbering and shifting rather than
forwarding, so the entry **correctly** excludes them from its eight *forwarders* -- and thereby
undercounts the **ten** sites an extraction actually serves. The full table, with each site's
disposition when the floor is absent, is in `contracts/dungeon-public-surface.md` section 2.

*Correction 4 -- a SECOND duplication, which this entry never mentions.*
`grep -c "if (outcome is not MutationOutcome.Success)"` over the same file returns **5**: `RemoveArea`,
`UpdateArea`, `UpdateAreaOutline`, `RemovePlacedObject` and `UpdatePlacedObject` each re-check the
inner `Floor` call's outcome before touching the aggregate. **`CORE-015` does not mention it at all.**
Any design aiming at this entry's stated goal that "each wrapper collapses to two lines" has to
address this duplication and not only the lookup.

*Correction 5 -- the SECOND suggested direction was never viable as written.* The entry offers, as an
alternative to the `RequireFloor` extraction, to "expose a narrow floor-scoped accessor from the root
and let callers work against `Floor` directly". **Every `Floor` mutator is `internal`, and was
`internal` at `16d594b`.** `grep -nE "^[[:space:]]+(public|internal) [A-Za-z]"` over `FloorMethods.cs`,
`FloorObjectMethods.cs` and `Floor.cs` at `20b0b09`, and over the corresponding `Entities/Floor/` paths
at `16d594b`, returns the same set at both commits: the nine `internal` members `UpdateNumber`,
`UpdateDetails`, `AddArea`, `RemoveArea`, `UpdateArea`, `UpdateAreaOutline`, `AddObject`,
`RemoveObject` and `UpdateObject`, with `Floor`'s `public` surface being only its five scalar
properties plus the `Areas` and `Objects` read-only projections. So the second option is a
**Domain-accessibility change**, not a refactor -- and it is **not** a smaller alternative to the
first option, it is a larger one. A future session should not read the two options as comparable.

**This entry's 8 `locations:` ranges are one half of a property of the whole audit document**, which is
stated **once**, and deliberately not restated here, in the `AMENDED 6b-xiv` block on `CORE-019` below.

The record's severity, category, effort and risk are unchanged.**]**

### CORE-016 -- The Domain has no policy layer and scale validation is inline and duplicated

```yaml
id: CORE-016
layer: CORE
severity: S3
category: structure
authority: conventions
citation: backend/backend-conventions.md#5. Policy Conventions
locations:
  - src/DungeonMaster.Domain/Entities/Floors/ObjectScale.cs:3-12
  - src/DungeonMaster.Domain/Entities/Floors/PlacedObject.cs:91-100
  - src/DungeonMaster.Domain/Entities/Floors/PlacedObject.cs:28
  - src/DungeonMaster.Domain/Entities/Floors/PlacedObject.cs:70
  - src/DungeonMaster.Domain/ValueObjects/CanvasPoint.cs:42
  - src/DungeonMaster.Domain/ValueObjects/TilePoint.cs:12
  - src/DungeonMaster.Domain/ValueObjects/PathSegment.cs:25-29
effort: M
risk: low
independent: true
```

**What.** There is no `Policies/` directory. The one non-trivial domain rule -- object scale must be a
finite value in `[Min, Max]` -- is written twice, verbatim, as an inline `if` in the `PlacedObject`
constructor and again in `PlacedObject.Update`, each throwing `ArgumentOutOfRangeException` with the
same interpolated message. `ObjectScale` holds the bounds as a static class of `const float`s plus two
clamping helpers. The three value objects under `ValueObjects/` are plain positional records that
accept any input, including `float.NaN` for `CanvasPoint`.

**Why it matters.** `backend-conventions.md` section 5 requires a policy class for each domain concept
with non-trivial validation, returning a structured result and never throwing, with the value object
delegating to it. Scale is exactly that concept and has neither a policy nor a value object. The
duplication is already load-bearing: the two copies must be kept in step, and because
`AddPlacedObjectCommand` has no validator (CORE-009) an out-of-range scale reaches the entity and
surfaces as an exception rather than a validation result.

**Suggested direction.** Introduce an `ObjectScale` value object with an `ObjectScalePolicy` beside it,
have `PlacedObject` store the value object, and let the command validators call the policy directly to
produce field-level errors before construction. `PathSegment`'s `Ctrl1`/`Ctrl2` nullability rule
(control points only meaningful for `CubicBezier`) is a second candidate for the same treatment.

**[AMENDED 6b-xv, 2026-09-30 -- this entry is PART DONE, not done. Two of its remedies landed; the
policy layer it is actually about was deliberately not built. Three of its prose claims are corrected,
one of which was false when written rather than stale, and five of its six `locations:` ranges are
re-measured. Classification: PARTIALLY ACTED ON, plus DRIFT, plus one ORIGINAL ERROR.**

**Nothing below deletes any previous wording.** This register's correction convention quotes the
previous text verbatim and leaves it readable -- see the `AMENDED 6b-xiv` block on `CORE-019` below and
the `AMENDED 6b-iv` block on `MASTER-165` in `00-master-index.md:628` -- so every sentence corrected
here still stands above it, and this block is what a reader must read alongside them. The one exception
is the `locations:` block, corrected **in place**, which is this register's documented exception to
leaving an entry's original text standing (the `AMENDED 6b-xiii` block on `CORE-019` records the same
exception, as does the `MASTER-167` closure in `00-master-index.md` for `UIKIT-004`'s range).

**The task that wrote this block could not invoke `git`.** Commits are handed over: `3cef4dd` (store
behaviour pinned), `4031fc8` (validator and viewport behaviour pinned), `93a4c90` (the `CanvasPoint`
guard), `f62b1c7` (the store tests rewritten beneath the domain), `ba5666d` (the `PlacedObject`
scale-guard extraction). Every `file:line` range in this block was read from the file in the worktree
at `ba5666d` by the task that wrote it; none is carried over from the session's `measurement.md`, from
this entry's previous text, or from another task's report.

**What this session did NOT do, which is why the entry stays open.** There is still no `Policies/`
directory, no `ObjectScalePolicy`, no `PolicyResult`, no `DomainException`, and no `ObjectScale` value
object -- `ObjectScale` remains the static `const float` holder the `What.` paragraph describes.
`PlacedObject` still stores a bare `float Scale` and still throws rather than returning a structured
result. The `Suggested direction.` paragraph above therefore stands in full and unactioned. What landed
is narrower and was chosen deliberately: the duplication was removed, and the `CanvasPoint` hole the
entry names in its last sentence was closed.

*Locations correction (in place).* Previous wording, verbatim:
`PlacedObject.cs:28-34`, `PlacedObject.cs:80-86`, `CanvasPoint.cs:9`, `PathSegment.cs:18-22`. Of these,
**`CanvasPoint.cs:9` and `PathSegment.cs:18-22` were already stale before this session touched
anything**, and `PathSegment.cs:18-22` never held the record at all -- at `ba5666d` it is the
`<param name="Ctrl2">` doc block, and the record declaration is at **`:25-29`** (`grep -n "public
record PathSegment" src/DungeonMaster.Domain/ValueObjects/PathSegment.cs` returns `25:`, and the
parameter list closes at `:29`). `CanvasPoint.cs:9` is now a `<para>` line inside the type's
`<remarks>`; the record declaration is at **`:42`** (`grep -n "public record CanvasPoint"` returns
`42:`). The two `PlacedObject` ranges held the two inline `if` blocks and no longer exist as such: the
rule is one helper at **`:91-100`**, called from **`:28`** and **`:70`** (`grep -n "ValidateScale"
src/DungeonMaster.Domain/Entities/Floors/PlacedObject.cs` returns `28:`, `70:` and `91:`, the last
being the declaration, whose body closes at `:100`). `ObjectScale.cs:3-12` and `TilePoint.cs:12` were
re-read and are **unchanged and correct**.

*Correction A -- `verbatim` was FALSE when written, and `twice` is now stale.* Previous wording,
verbatim, from the `What.` paragraph above: *"is written twice, verbatim, as an inline `if` in the
`PlacedObject` constructor and again in `PlacedObject.Update`"*. **The two copies were never
verbatim.** Their conditions differed in the identifier under test -- `scale` in the constructor and
`newScale` in `Update` -- and it was the interpolated **message string** that was byte-identical. That
is a small difference and it is the reason the de-duplication needed a `paramName` parameter rather
than a bare `nameof` inside the helper. **After `ba5666d` there is one copy**, not two: a single
`private static void ValidateScale(float scale, string paramName)` at `PlacedObject.cs:91-100`, called
from the constructor at `:28` and from `Update` at `:70`. Each call site passes `nameof` **evaluated at
the call site**, so the reported `ParamName` is still `"scale"` and `"newScale"` respectively and the
observable behaviour is unchanged: same inputs rejected, same exception type, `ParamName`, `ActualValue`
and message. `Update`'s theory gained a `float.NegativeInfinity` case the constructor's already had.

*Correction B -- the no-validator clause is STRUCK as stale.* Previous wording, verbatim, from the
`Why it matters.` paragraph above: *"because `AddPlacedObjectCommand` has no validator (CORE-009) an
out-of-range scale reaches the entity and surfaces as an exception rather than a validation result"*.
**`MASTER-079` / `CORE-009` is closed** -- `00-master-index.md:267` carries `**done**` -- and both
commands now bind their scale field with `InclusiveBetween(ObjectScale.Min, ObjectScale.Max)`:
`src/DungeonMaster.Application/Features/Dungeons/Commands/AddPlacedObject/AddPlacedObjectCommandValidator.cs:38-42`
on `Scale`, and
`src/DungeonMaster.Application/Features/Dungeons/Commands/UpdatePlacedObject/UpdatePlacedObjectCommandValidator.cs:37-41`
on **`NewScale`** -- the property names differ between the two commands, so a reader grepping for
`Scale` alone will miss the second. Both reference the `ObjectScale` constants rather than restating the
bounds. So an out-of-range scale now surfaces as a validation result at the boundary, and the entity
guard is the backstop rather than the only check. The convention gap the paragraph rests on is
unaffected: a validator is not a policy, and the rule is still expressed twice in two layers.

*Correction C -- the value-object clause is STRUCK as stale.* Previous wording, verbatim, from the
`What.` paragraph above: *"The three value objects under `ValueObjects/` are plain positional records
that accept any input, including `float.NaN` for `CanvasPoint`"*. Two of the three are no longer plain.
`PathSegment` validates its control points against its `Kind` through a redeclared property initializer
(`PathSegment.cs:35`, helper at `:40-79`), added under `MASTER-170`. `CanvasPoint` now **rejects a
non-finite coordinate** (`CanvasPoint.cs:42-110`), throwing `ArgumentOutOfRangeException` whose
`ParamName` is the offending coordinate and whose `ActualValue` is the rejected value. `float.IsFinite`
is the whole predicate, so `float.MaxValue` and `float.MinValue` remain accepted. `TilePoint`
(`TilePoint.cs:12`) is still a plain positional record, and its coordinates are `int`, so the finiteness
question does not arise for it.

**The design detail worth carrying.** `CanvasPoint`'s guard fires on **both** the primary constructor
**and** `with`-expressions, through two different mechanisms -- private field initializers
(`CanvasPoint.cs:49-50`) and `init` accessors (`:63-79`) -- because a record's compiler-generated copy
constructor assigns backing fields directly and never re-runs a property initializer. An
initializer-only guard would have left `with`-expressions open. **That is exactly the carried
`MASTER-170` gap that `AreaOutlineGeometry.Translate` still walks through, and this session did not
close it.** `PathSegment` is still guarded by an initializer only. Nothing here closes or fixes
`MASTER-170`, and nothing here fixes `PathSegment`; that bypass remains a carried backlog item. This
change only avoids repeating the shape.

**What the store actually does, measured (`3cef4dd`, rewritten `f62b1c7`).**
`src/DungeonMaster.Tests/Infrastructure/Persistence/Configurations/CanvasPointNonFinitePersistenceTests.cs`
pins it. **The three non-finite values do not behave alike, and it is wrong to say SQLite rejects
non-finite floats.** It refuses exactly one of the three and stores the other two. `float.NaN` is
refused and nothing is written. **Both infinities round-trip intact**, stored and read back unchanged,
with `SELECT typeof(Position_X)` returning **`real`** -- not `NULL`, not clamped, not text. The
attribution, which must not be broadened: the `NaN` refusal is **`Microsoft.Data.Sqlite`'s ADO layer
refusing to bind the parameter**, demonstrated on a call path with no EF on it at all, throwing an
unwrapped `InvalidOperationException("Cannot store 'NaN' values.")` from
`Microsoft.Data.Sqlite.SqliteValueBinder.BindDouble`. It is **not** a SQLite constraint and **not** the
`IsRequired()` mapping; a `REAL NOT NULL` violation surfaces instead as `SqliteException` with a
constraint code, and that contrast was measured. The `REAL` column type is nonetheless why EF attempts
a floating-point bind at all, so it is not irrelevant -- a TEXT mapping would store `NaN` without
complaint. The file reaches the store **beneath** the domain, because `93a4c90`'s guard means the domain
type can no longer hold a non-finite value for EF to attempt to persist: raw-SQL write plus a direct
`SELECT` for the infinities, and a direct `Microsoft.Data.Sqlite` parameter bind for the `NaN`. It
declares **3 test methods** carrying **6 cases** (`grep -c "public async Task\|public void"` returns
`3`; `grep -c "InlineData"` returns `4`, on the one theory, plus 2 facts).

**A superseded framing still in the source, carried as a finding this task could not fix.**
`CanvasPoint.cs:16-25` describes the `NaN` refusal as *"a `DbUpdateException` wrapping
`InvalidOperationException`"* arriving *"at commit time"*. That was what the pre-`f62b1c7` test observed
through EF; `93a4c90`'s own guard made that path unreachable, so the wrapper is no longer the thing
being described. **Prefer this block's ADO-layer framing over that doc comment.** Correcting the comment
is a `src/` edit and outside this task's ownership set; it is left as a carried item.

**Whether a production path can produce a non-finite `CanvasPoint`, measured (`4031fc8`).** This matters
because the entry's severity should rest on a run result rather than on the type merely permitting one.
`src/DungeonMaster.App.Tests/Graphics/FloorMapDrawableNonFiniteTests.cs` (**6 test methods**, by
`grep -c "public void"`) pins it, and **the answer is split.** For **`ZoomLevel` there is no reachable
non-finite path on the shipping Windows TFM**: all six production writes to `ZoomLevel` / `PanOffset`
were enumerated, and the generated-code route was closed by a grep restricted to `obj/` | `bin/`
returning zero. For **`PanOffset` the question is OPEN**, recorded as an open question by explicit user
decision (escalation E2) and **deliberately not resolved in either direction**.
`DungeonDetailPage.xaml.cs` accumulates raw touch coordinates with no finiteness test, and only
`ResetViewport` sanitizes it; one non-finite touch coordinate would make `PanOffset` permanently `NaN`
and every later `ScreenToCanvas` result `NaN`. **Whether the MAUI Windows pointer pipeline can deliver
such a coordinate is a fact about an external framework's runtime, not about this repository, and
nothing in this session asserts it either way.** Also measured, and a trap worth carrying:
**`Math.Clamp` does not clamp `NaN`** -- it returns `NaN` unchanged while clamping `±Infinity` to the
bounds. So the Windows conclusion holds because of a `#elif MACCATALYST` fence, not because of the
clamp, and **the MacCatalyst pinch path is a live latent hole on that TFM.**

**`measurement.md` section 2(e) is now RUN, and its reasoning was worth nothing in either direction.**
That section reasoned about whether `InclusiveBetween` rejects `NaN` and declined to claim it.
`src/DungeonMaster.Tests/Application/Features/Dungeons/Validators/NonFiniteScaleRejectionTests.cs`
(**4 test methods**, by `grep -c "public void"`) measured it: **`InclusiveBetween` rejects all three
non-finite scale values on both command validators** -- 3 values across 2 validators, so **6
rejections** -- each a single failure carrying error code `InclusiveBetweenValidator` on the scale
property, with `ObjectScale.Default` valid on both as a control. The mechanism, measured by loading the
shipped assembly: the comparison runs through a total order that sorts `NaN` below every other value,
so a `NaN` fails the lower bound rather than being skipped as incomparable. **It is not
`Comparer<float>.Default`** -- an earlier draft of this session said so and was corrected -- and the
type that does it is `internal` to FluentValidation, so naming it here would be a citation that can
change without a public-API signal. Two parties reasoned from different implicit forms of a range check
and reached **opposite** conclusions; only the measurement settled it.

**This block moves a citation in a later entry, and says so rather than leaving it to drift.** Inserting
these paragraphs shifts every line below `CORE-016`, which includes the two in-document citations the
`AMENDED 6b-xiv` block on `CORE-019` states as positions: `:761-768` (`CORE-015`) is **unmoved**,
because it sits above this insertion point, while `:1002-1007` (`CORE-019`'s `locations:` window) has
moved. Re-run of that block's own predicate after this edit: the `CORE-019` `locations:` window is now
**`:1162-1167`**, was `:1002-1007`. This is the neighbour-falsification mechanism the `MASTER-047` closure
describes, and it is recorded here because nothing in this audit process re-measures an entry when the
document around it changes.

The record's severity, category, effort and risk are unchanged, and `MASTER-081` / `CORE-016` stays
**OPEN as part done**.**]**

**[AMENDED 20261109-1641-part-done-rows, 2026-11-09 -- `MASTER-081` is CLOSED DELIBERATELY UNCHANGED by user
decision at Gate 1, with no code change: the duplicated scale rule this entry's title names was already
removed by 6b-xv, and the policy layer it proposes was considered and declined.** The register row is
`MASTER-081` at `00-master-index.md:269`. **Nothing below deletes any previous wording.** Every figure in this
block is labelled either as measured by this session's task `T3` on the session worktree, or as told, with its
source. The contract files cited below are in `.sir/sessions/20261109-1641-part-done-rows/contracts/`.

*Closure.* `MASTER-081` (`00-master-index.md:269`) is CLOSED **DELIBERATELY UNCHANGED** by user decision at
Gate 1, with no code change (told: `.sir/sessions/20261109-1641-part-done-rows/discussion.md`, Decisions).
**The row's own subject, the duplication, is already gone** -- 6b-xv's `ValidateScale` is one helper.
Measured by `T3`: `grep -rn ValidateScale src --include=*.cs --exclude-dir=obj --exclude-dir=bin` prints
3 lines, the declaration `private static void ValidateScale(float scale, string paramName)` at
`src/DungeonMaster.Domain/Entities/Floors/PlacedObjectMethods.cs:45` (a partial of `PlacedObject`) and its two
calls, `src/DungeonMaster.Domain/Entities/Floors/PlacedObject.cs:30` in the constructor and
`PlacedObjectMethods.cs:24` in `Update`. **Considered and declined** (told: Gate 1, the same Decisions): a
policy layer, `PolicyResult`, `DomainException` and an `ObjectScale` value object. Measured by `T3`, each is
still absent or unchanged: `find src -type d -name Policies`, with `obj` and `bin` paths excluded, prints
nothing, while the same command for `Floors` prints `src/DungeonMaster.Domain/Entities/Floors`;
`grep -rnwE PolicyResult src --include=*.cs --exclude-dir=obj --exclude-dir=bin` prints nothing, and so does the
same command for `DomainException`, while the same command for `ValidateScale` prints the 3 lines above;
`grep -n ObjectScale src/DungeonMaster.Domain/Entities/Floors/ObjectScale.cs` prints `:6`
`public static class ObjectScale`, the holder of the `const float` bounds and not a value object. **What the
decision leaves, stated plainly:** the scale bounds are still checked in two layers, the entity helper and the
two command validators' `InclusiveBetween(ObjectScale.Min, ObjectScale.Max)`, which `grep -n InclusiveBetween`
places at
`src/DungeonMaster.Application/Features/Dungeons/Commands/AddPlacedObject/AddPlacedObjectCommandValidator.cs:40`
(on `Scale`) and
`src/DungeonMaster.Application/Features/Dungeons/Commands/UpdatePlacedObject/UpdatePlacedObjectCommandValidator.cs:39`
(on `NewScale`), each with `ObjectScale.Min,` and `ObjectScale.Max);` on the two lines below it -- both
referencing the `ObjectScale` constants rather than restating the numbers.

*Supersession.* The `AMENDED 6b-xv` block above ends with this sentence, quoted verbatim and joined across its
line break: "The record's severity, category, effort and risk are unchanged, and
`MASTER-081` / `CORE-016` stays **OPEN as part done**." This block supersedes that status and does not edit it.

*The `AMENDED 6b-xv` block's positions have moved, measured by `T3`.* That block places the helper at
`PlacedObject.cs:91-100`, called from `:28` and `:70`. By the `ValidateScale` grep above, the declaration is now
at `PlacedObjectMethods.cs:45` and the two calls at `PlacedObject.cs:30` (constructor) and
`PlacedObjectMethods.cs:24` (`Update`). No classification of the move is given, because `T3` cites no committed
record of the session that made it; `.sir/lessons.md:70` records that `PlacedObject`'s methods were split into
their own file (told). This entry's `locations:` list is kept unchanged as a record of the audited state.

*The `Suggested direction.` is declined, not pending.* It is left exactly as written, as the record of what was
proposed; this block, not that paragraph, is the current disposition. **The deferred "Scale value object" item
is answered by this decision** (told: `discussion.md`, Decisions, its Not-in-scope bullet); nothing is built.

*Moved citations, recorded rather than left to drift* -- the hazard the `AMENDED 6b-xv` block describes in its
penultimate paragraph. This session inserted two blocks in this file, this one (77 lines with its trailing
empty line) and the `AMENDED 20261109-1641-part-done-rows` block on `CORE-024` (72 lines with its trailing
empty line), so every line below this block's insertion point moves down 77, and every line below the
`CORE-024` block's moves down 149. CITE-IN and CITE-X (`C1-measurement.md` section 6) were run by `T3` before
the first edit and after both insertions, and each target below was re-read by `T3` at startRef and in the
post-`T3` file. Positions in this paragraph, and only here, are line numbers of this file: **20 citations
were live and correct at startRef and had their target moved**, each given as cited at (startRef, then post-`T3`
line) -> target at startRef -> target in the post-`T3` file:
`:1054` (unmoved) -> `:1162-1167` -> `:1239-1244`, the `CORE-019` `locations:` window;
`:1598` (now `:1675`) -> `:1567` -> `:1644`, the quoted `Floor.cs at 73 lines` sentence;
`:1612` (now `:1689`) -> `:1561-1563` -> `:1638-1640`, the quoted `PlacedObject is not partial at all` sentence;
`:1794` (now `:1871`) -> `:1712-1714` -> `:1789-1791`, the blocker-class sentence of `CORE-024`'s `AMENDED 6b-xvii`
block; and seven in-file citations of `:1293` -> `:1370`, the `AMENDED 6b-xiv` opener on `CORE-019`, cited at
`:1696` (now `:1773`), `:1867` (now `:2016`), `:1997` (now `:2146`), `:2123` (now `:2272`), `:2263` (now `:2412`),
`:2393` (now `:2542`) and `:2661` (now `:2810`); plus nine cross-file citations of the same `:1293` -> `:1370`, in
`03-app-shell-and-views.md` at line 208 and `04-app-controls-and-theme.md` at lines 1017, 1133, 1272, 1373, 1465,
1555, 1686 and 1798. That is 11 in-file and 9 cross-file, 20 in all. **Not listed, by count:** 6 in-file
citations were already dated at startRef, each dated by its own wording (`C1-measurement.md` section 6's
table); 10 were already stale at startRef -- 1 in this file and 8 in `04-app-controls-and-theme.md` citing
`:2156-2216`, and the 1 in `00-master-index.md`'s `MASTER-119` row citing `:2313`, which `T3` read: it holds a
sentence of the `AMENDED 6b-xvii` block on `CORE-028`, not the `MASTER-114` / `CORE-030` record that row
names; and 6 in-file citations target lines above both insertion points and did not move. None is re-derived
here. **No citation is edited** -- not in this file, and not in `03-app-shell-and-views.md` or `04-app-controls-and-theme.md`,
which no task of this session owns (decided at Gate 2). This paragraph adds citations of its own, which
CITE-IN counts after the last edit.

**No hash for this session's commit appears in this block:** no code changed for this row, and this session's
tasks run without git; the orchestrator makes the commit. The record's severity, category, effort and risk are
unchanged, and `MASTER-081` / `CORE-016` is CLOSED **DELIBERATELY UNCHANGED**.**]**

### CORE-017 -- ObjectKind has no zero value, so default(ObjectKind) silently means DoorWood

```yaml
id: CORE-017
layer: CORE
severity: S3
category: api-design
authority: microsoft
citation: https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/enum
locations:
  - src/DungeonMaster.Domain/Entities/Floors/ObjectKind.cs:3-51
effort: S
risk: medium
independent: true
```

**What.** `ObjectKind` starts at `DoorWood` with no `None` member, so the enum's zero value is a
concrete door. `ObjectRotation` and `SnapAnchor`, by contrast, do start at a neutral value
(`None`, `Center`).

**Why it matters.** The Microsoft enum design guideline is explicit: "DO provide a value of zero on
simple enums. Consider calling the value something like 'None.' If such a value is not appropriate for
this particular enum, the most common default value for the enum should be assigned the underlying
value of zero." Neither branch is satisfied here -- `DoorWood` is not a meaningful default and not the
most common kind. A default-constructed `AddPlacedObjectCommand`, a column that fails to bind, or a
deserialisation gap therefore produces a wooden door rather than an obvious error.

**Suggested direction.** Add `None = 0` as the first member and reject it at the boundary
(validator plus entity constructor). Note that the underlying numeric values of every existing member
shift by one, so any persisted integer column has to be migrated -- hence medium risk; T2 owns the
persistence side.

**AMENDED 6b-viii, 2026-09-17 -- the migration clause above is STRUCK as false.** There is no persisted
integer column. `PlacedObjectConfiguration` maps `Kind` with `.HasConversion<string>()`, so the column
stores the member NAME, and `ObjectPackRegistry` parses by name too; no ordinal is persisted or serialised
anywhere, and no `(ObjectKind)` numeric cast or `Enum.GetValues` over the type exists. Renumbering is
therefore free, which was confirmed empirically when `MASTER-073` closed: no Infrastructure file changed
and every persistence and Graphics test passed unmodified. The *suggested direction* itself -- add `None`
as a zero member and reject it at the boundary -- was followed, and is what `MASTER-073` implements.

**AMENDED 6b-ix, 2026-09-17 -- the amendment above is CORRECT and its reasoning DOES NOT GENERALISE.** Nothing
in the paragraph above is withdrawn: `ObjectKind` maps with `.HasConversion<string>()`
(`src/DungeonMaster.Infrastructure/Persistence/Configurations/PlacedObjectConfiguration.cs:26-28`), that is its
only EF property mapping --
`grep -rn "builder.Property(obj => obj.Kind)" --include=*.cs src/DungeonMaster.Infrastructure/` returns **one
line**, `PlacedObjectConfiguration.cs:26` -- so no ordinal of it is persisted, and renumbering it was free.
What is added here is the boundary of that conclusion, because the sentence "renumbering is therefore free" is
the kind of sentence a later reader carries to the next enum.
**It was true of `ObjectKind` because of `.HasConversion<string>()`, and it is false of `PathSegmentKind`.**
`PathSegmentKind` persists its **ordinal** through the `Areas.Outline` JSON column: `AreaConfiguration`
(`src/DungeonMaster.Infrastructure/Persistence/Configurations/AreaConfiguration.cs:16`) builds its options as
`private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.General);` -- the
target-typed `new` constructs a `JsonSerializerOptions` from `JsonSerializerDefaults.General` with no converter
supplied -- `grep -rn "JsonStringEnumConverter\|JsonConverter"
--include=*.cs src/` returns **zero hits** across `src/`, and `PathSegmentJsonRoundTripTests.cs:26-28` pins
`"Kind": 0` as `Line` and `"Kind": 1` as `CubicBezier`. Inserting a zero member there is a data migration on
persisted rows. The general rule is that freedom to renumber is a property of **each enum's own persistence
mapping**, established per enum, and never inherited from a sibling; see `CORE-035` for the full triage.

### CORE-018 -- Entity provides no identity-based equality, so two instances of the same entity compare unequal

```yaml
id: CORE-018
layer: CORE
severity: S3
category: api-design
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.Domain/Entities/Entity.cs:6-27
effort: S
risk: medium
independent: true
```

**What.** `Entity` exposes `Id`, `CreatedAt` and `LastModified` and a protected `Touch()`, but does not
override `Equals`, `GetHashCode` or `==`. All four entities (`Dungeon`, `Floor`, `Area`,
`PlacedObject`) therefore use reference equality, while the three value objects are `record`s with
structural equality.

**Why it matters.** I believe this is a latent trap rather than a present defect: the aggregate's
internal lookups all compare `entity.Id == someGuid` explicitly, so nothing misbehaves today. But the
model presents two entity-shaped things -- a re-loaded `Dungeon` and an in-memory one with the same
`Id` -- as different objects, which makes `Contains`, `Distinct`, `Except` and dictionary keys behave
in a way most readers of a DDD-shaped domain would not expect. The asymmetry with the value-object
records makes the surprise more likely, not less.

**Suggested direction.** Implement identity equality on `Entity` (equal when the runtime types match
and `Id` is equal), with `GetHashCode` over `Id`. Medium risk because any existing code that relies on
reference semantics for entities would change behaviour silently.

### CORE-019 -- Five Domain overloads have no production caller and are reachable only from the test assembly

```yaml
id: CORE-019
layer: CORE
severity: S3
category: dead-code
authority: practice
citation: Fowler, Refactoring 2e, ch. 3 "Dead Code"
locations:
  - src/DungeonMaster.Domain/Entities/Dungeons/DungeonMethods.cs:371-381 # corrected 6b-xiii; position at b39a65d, deleted since -- see the amendment below
  - src/DungeonMaster.Domain/Entities/Floors/FloorObjectMethods.cs:10-14
  - src/DungeonMaster.Domain/Entities/Floors/FloorObjectMethods.cs:96-113
  - src/DungeonMaster.Domain/Entities/Floors/PlacedObject.cs:13-18
  - src/DungeonMaster.Domain/Entities/Floors/PlacedObject.cs:69-71 # corrected 6b-xiii; position at b39a65d, deleted since -- see the amendment below
  - src/DungeonMaster.Domain/AssemblyInfo.cs:3
effort: S
risk: low
independent: true
```

**What.** A repository-wide search finds no reachable production entry point for the scale-less
overloads: `Dungeon.AddPlacedObject(floorId, kind, position, rotation)`, `Floor.AddObject(kind,
position, rotation)`, `Floor.UpdateObject(objectId, newPosition, newRotation)`,
`PlacedObject.Update(newPosition, newRotation)` and the four-argument `PlacedObject` constructor. Four
of the five have no caller at all outside `DungeonMaster.Tests`. The fifth,
`PlacedObject.Update(newPosition, newRotation)`, does have one production caller --
`Floor.UpdateObject(objectId, newPosition, newRotation)` at `FloorObjectMethods.cs:109` -- but that
caller is itself one of the dead overloads, so the cluster is dead as a whole rather than individually.
Every Application-layer call site passes an explicit scale. The remaining callers are in
`DungeonMaster.Tests`, reachable because `AssemblyInfo.cs` grants
`InternalsVisibleTo("DungeonMaster.Tests")`.

**Why it matters.** These overloads are kept alive by their own tests, which is the classic way dead
code survives a coverage check. Each one is a second entry point into the scale invariant discussed in
CORE-016, so any change to scale handling has to be made in twice as many places as the production
paths require, and a reader cannot tell from the code which overload is the intended one.

**Suggested direction.** Delete the scale-less overloads and update the tests to pass
`ObjectScale.Default` explicitly -- the tests then document the default rather than depending on an
otherwise unused API. If the convenience overloads are wanted for readability, keep exactly one level
of defaulting (an optional parameter) rather than a parallel overload chain at three types.

**[AMENDED 6b-xiii, 2026-09-29 -- two of this entry's six `locations:` citations were stale when
re-measured at `b39a65d`, and this session introduced a third stale range of its own before catching
it. Classification: DRIFT.** The entry's substance stands and was acted on in full: all five scale-less
overloads are deleted. As with `CORE-005` above, the `What.`, `Why it matters.` and
`Suggested direction.` paragraphs are left exactly as written -- they record how the code then stood.
Only the two stale citations are corrected in place, which is this register's documented exception to
leaving an entry's original text standing (the `MASTER-167` closure in `00-master-index.md` records the
same exception for `UIKIT-004`'s range).

*Correction 1 -- `Dungeon.AddPlacedObject`.* Previous wording, verbatim:
`src/DungeonMaster.Domain/Entities/Dungeons/DungeonMethods.cs:323-333`. That range does not hold
`AddPlacedObject` at any arity. It holds **`UpdateAreaOutline`** -- its doc-comment tail, its signature
and its opening body. Verified after this session's deletions, which removed only lines *after* `:333`
in that file and so left this range unshifted: `grep -n "UpdateAreaOutline"
src/DungeonMaster.Domain/Entities/Dungeons/DungeonMethods.cs` returns the declaration at `:326`, and
`sed -n '320,335p'` shows `:320-325` to be the tail of its `<param>` / `<returns>` block. The scale-less
four-parameter `AddPlacedObject` was at **`:371-381`** at `b39a65d`.

*Correction 2 -- `PlacedObject.Update`.* Previous wording, verbatim:
`src/DungeonMaster.Domain/Entities/Floors/PlacedObject.cs:62-63`. That range holds the **`Rotation`
property's doc comment**, not `Update` of either arity. The scale-less two-parameter `Update` was at
**`:69-71`** at `b39a65d`. Stated precisely, because it was derived rather than read: this record's
author could not read `b39a65d` directly, and located the doc comment at `:55` in the post-deletion
file, which is `:63` once the eight lines the four-parameter constructor and its blank separator
occupied are added back -- a shift independently confirmed by the `Scale` field initializer, recorded
at `:67` at `b39a65d` and now read at `:59`. On that arithmetic `:62` is the blank line above the doc
comment and the `Rotation` declaration itself sat at `:64`. The qualitative claim does not turn on the
arithmetic: no `Update` overload of either arity was within ten lines of `:62-63`.

**The drift has a named cause, and a direction.** Neither stale citation was caused by any work on
`MASTER-047`; nothing had been done to this entry between its authorship and this session. Both were
caused by the **ObjectScale feature work**, which inserted the scale-carrying overloads above and
beside the scale-less ones in all three files, leaving the interleaved doc-comment / dead-overload /
doc-comment / live-overload layout the deletions had to pick apart. The direction corroborates
insertion rather than movement: `AddPlacedObject` drifted **downward** by 48 lines, from the cited
`:323` to the measured `:371`, which is what content added above a member does to it. This is the same
mechanism `6b-xii` recorded immediately above, when `CORE-006`'s fix silently falsified four of
`CORE-005`'s claims: **a fix aimed at one entry falsifies its neighbours, in prose, at lines that may
never move.** Nothing in this audit process re-measures an entry when the code around it changes, so
these two citations sat wrong until a session convened to act on them read them.

**All three of this entry's code ranges are historical positions at `b39a65d` and are NOT navigable at
`HEAD`.** That covers the two corrected `locations:` entries above and the entry's one in-prose
citation, `FloorObjectMethods.cs:109` for the surviving production call to the two-parameter
`PlacedObject.Update` -- which **was correct at `b39a65d`**, the one in-prose citation this session's
measurement checked and found sound. All three now point at code this session deleted. **The durable
address is the symbol name**, and in this entry's case even that is gone: every one of the five members
it names has been removed. The four `locations:` entries left uncorrected -- `FloorObjectMethods.cs`
`:10-14` and `:96-113`, `PlacedObject.cs:13-18` and `AssemblyInfo.cs:3` -- were sound at `b39a65d`; the
first three addressed code now deleted, and `AssemblyInfo.cs:3` is **still current and still standing**
(`cat -n src/DungeonMaster.Domain/AssemblyInfo.cs`: 3 lines, the grant on `:3`). That grant was never
part of what this entry directed removing: it is how the rest of `DungeonMaster.Tests` reaches `Domain`
internals.

**A THIRD STALE RANGE, AND THIS SESSION AUTHORED IT.** Recorded here rather than only in the session
artifacts, because this entry's own subject is citations that no longer address what they claim, and a
session that repaired two while quietly introducing a third would leave in place exactly the defect it
convened to remove -- which is what `6b-xii` recorded of itself when it introduced two stale citations
into its own correction record. (a) The four-parameter `Dungeon.AddPlacedObject` spans
`src/DungeonMaster.Domain/Entities/Dungeons/DungeonMethods.cs:371-381` at `b39a65d`, **not `:371-377`**:
`:371-377` cuts off mid-expression, after `=> AddPlacedObject(` and `floorId,`. The signature is
`:371-375`, the delegating body `:376-381`, the doc comment `:353-370`. (b) `:371-377` was authored by
**this session's own orchestrator at Gate 1**, by inferring the span's end from two `grep` hits instead
of reading the range, and it propagated from `measurement.md` section 3 into the frozen acceptance
criteria `B1` and `C2` before anyone noticed. (c) It was caught by **`T2`'s implementer**, which
measured the range before deleting it, and was corrected by escalation `E2` before `T3` was dispatched
-- so the corrected `:371-381` is what reached this record. (d) The other four members' ranges were
verified line-for-line by `T2` and are exact.

**A SECOND UNMEASURED MECHANISM CLAIM, ALSO THIS SESSION'S OWN.** Recorded here because a reader of
this entry is precisely the reader most likely to reach for the member in question while working on
`PlacedObject`. (a) `PlacedObject`'s `Scale` field initializer, `= ObjectScale.Default`, is **not** what
sets `Scale` when EF Core materializes a `PlacedObject` from the database. EF invokes the private
parameterless constructor and **then assigns `Scale` from its mapped column**
(`PlacedObjectConfiguration.cs:34`, `builder.Property(obj => obj.Scale).HasColumnType("REAL")`), so the
initializer is overwritten on every load and is not the setter on that path. (b) Removing the
initializer is therefore **completely inert**, measured rather than argued: `T2`'s reviewer deleted it
and ran the full solution suite -- **975 passed, 0 failed, 0 skipped**. (c) The in-repo proof was
available the whole time at
`src/DungeonMaster.Tests/Infrastructure/Persistence/Configurations/PlacedObjectConfigurationTests.cs:48`,
which commits and reloads through real SQLite and asserts `reloaded.Scale` equals the **parameterised**
scale -- a `[Theory]` whose three `[InlineData]` rows are `0.25f`, `2.5f` and `4.0f`, none of them
`ObjectScale.Default` -- with `:63-64` asserting the property is ordinary data
(`TryGetDefaultValue` false, `ValueGenerated.Never`). Neither could pass if the initializer were the
setter on that path. (d) The initializer **nonetheless remains in the code**, as a defence-in-depth
default for non-EF construction paths, and **no test in the suite catches its removal** -- which is now
measured, by (b), rather than assumed. The claim that it was load-bearing on the EF path was authored
in `measurement.md` section 6 and reached frozen criterion `B6` unmeasured.

**The generalisation, which is this session's most portable finding.** The rule inherited from `6b-xii`
was *run the claim about the tool*. **That is too narrow: it is any claim about a mechanism.** Both
findings above are the same error in different clothing -- an assertion about how something behaves,
stated confidently at Gate 1, propagated into frozen criteria, and never run. One was about a line
range, which is a claim about a file; the other about EF Core's materialization path, which is a claim
about a library. Neither was a claim about a tool, and the narrower rule would have caught neither.

The record's severity, category, effort and risk are unchanged.**]**

**[AMENDED 6b-xiv, 2026-09-29 -- the `AMENDED 6b-xiii` block above carries three defects of its own:
one false claim, one causal attribution that is impossible on dates, and one sentence a backlog item
wrongly calls unsound. Classification: the 6b-xiii BLOCK is PARTIALLY FALSIFIED; this ENTRY's own
substance is untouched and stays closed.** 6b-xii recorded introducing two stale citations into its
own correction record, and 6b-xiii then did the same thing once; this block exists so that 6b-xiv does
not make it three. **Nothing below deletes any previous wording.** This register's correction
convention quotes the previous text verbatim and leaves it readable -- see the `AMENDED 6b-iv` block
on `MASTER-165` in `00-master-index.md:628`, *"Previous wording, verbatim, from this entry's
summary"* -- so every sentence corrected here still stands above it, and this block is what a reader
must read alongside them. **The task that wrote this block could not invoke `git`.** Every figure
attributed to a historical commit was measured by this session's orchestrator and handed over with
the command that produced it. Most are recorded in `measurement.md` sections 1 and 6 of
`.dir-sdlc/sessions/2026-09-29-dungeon-facade-lookup-extraction/`, measured at Gate 1. **The one
exception is the eight-row drift table below, with the closing-line range beside it**: those were
measured at Gate 3 under amendment A1 of that session's manifest, and they *replace* a
`measurement.md` figure this block goes on to show was false. Nothing here is re-derived by the task,
and no line position here is inferred from a neighbouring one.

*Correction A -- the "within ten lines" sentence is FALSE.* Previous wording, verbatim, from the
`AMENDED 6b-xiii` block above, spanning `:992-993` at this session's baseline `20b0b09`: *"The
qualitative claim does not turn on the arithmetic: no `Update` overload of either arity was within ten
lines of `:62-63`."* **The clause after the colon is false.**
`git show b39a65d:src/DungeonMaster.Domain/Entities/Floors/PlacedObject.cs | grep -n "void Update"`
returns **`70:`** and **`83:`**. At `b39a65d` that file declared the two-parameter `Update` at
**`:70`** -- **7 lines** below `:63`, squarely within ten -- and a second overload at **`:83`**. The
sentence was written as a redundant fallback, and the claim it falls back on still stands: `:62-63`
does hold the `Rotation` property's doc comment rather than `Update`, and that was verified directly.
But the false sentence sits inside the one block whose entire subject is confident unmeasured claims
about line ranges, which is why it is corrected here rather than left as a harmless surplus. **The
rest of Correction 2 above is unaffected**, including its honest marking *"Stated precisely, because
it was derived rather than read"* -- exactly the marking this sentence lacks.

*Correction B -- the "48 lines" sentence is SOUND, and the backlog item calling it unsound is the
thing that is wrong.* The sentence spanning `:999-1001` at this session's baseline -- *"The direction
corroborates insertion rather than movement: `AddPlacedObject` drifted **downward** by 48 lines, from
the cited `:323` to the measured `:371`, which is what content added above a member does to it."* --
is **accurate as written**. It is recorded as sound here because it was challenged, not because
nobody looked. (It spans three lines: it begins mid-line at `:999`, the words "48 lines" sit on
`:1000`, and both `:323` and `:371` sit on `:1001` where it ends. A citation of `:1000` alone
addresses one line of three.)

**The backlog item at `.dir-sdlc/backlog.md:427` is itself FALSE.** It asserts that *"`:323` was never
a measured position of `AddPlacedObject`"* and concludes from that *"So 48 is the gap between a stale
citation and a measured one, not a displacement anybody observed."* Both halves fail against the
authorship commit.
`git show 16d594b:src/DungeonMaster.Domain/Entities/Dungeon/DungeonMethods.cs | sed -n '323,333p'`
returns the four-parameter scale-less `AddPlacedObject` declaration **in full and nothing else** --
opening at `:323`, closing at `:333`. So `:323` **was** a measured position of that member, exactly,
and the cited range `:323-333` was that declaration and nothing else. And the displacement is real.
Tracking that one declaration across every commit that touched the file, with
`git show <commit>:<path> | grep -n "AddPlacedObject(" | grep -v "=> AddPlacedObject"`:

| Commit | 4-param `AddPlacedObject` | 5-param |
|---|---|---|
| `bb7405c` | **`:323`** | `:356` |
| `781f73b` | **`:371`** | `:404` |
| `a9a5935` | *deleted* | `:374` |

`:323` to `:371` is **+48** -- one member, two adjacent commits, a measured displacement.
**The irony is exact and belongs in the record:** the backlog item's stated reason for doubting the
figure is that nobody read the entry's authorship commit. Reading the authorship commit is what
refutes it. **`.dir-sdlc/backlog.md` is not annotated by this session** -- it lies outside the owning
task's file set -- so this paragraph is the only place the falsification is written down.

*Correction C -- the stated CAUSE of both stale citations is FALSE.* Previous wording, verbatim, from
the passage standing at `:995-997` at this session's baseline, its quoted tail running onto `:998`:
*"**The drift has a named cause, and a direction.** Neither stale citation was caused by any work on
`MASTER-047`; nothing had been done to this entry between its authorship and this session. Both were
caused by the **ObjectScale feature work**, which inserted the scale-carrying overloads above and
beside the scale-less ones in all three files"*. **The first sentence stands; the attribution is
false, for three reasons, and there are two real causes rather than one.**

**(a) The ObjectScale work predates this document by three months.**
`git log --no-walk --format='%h %ad %s' --date=short ec6c239 16d594b`:

| Commit | Date | Subject |
|---|---|---|
| `ec6c239` | **2026-05-21** | `feat(dungeons) Add object scale to placed objects` |
| `16d594b` | **2026-08-17** | `docs: add code-quality audit for domain and application` |

`git merge-base --is-ancestor ec6c239 16d594b` is **true**. A commit that landed three months
**before** this document was written cannot have moved a line the document cited afterwards. What
ObjectScale created was the scale-less / scale-carrying overload **pairs** that `CORE-019` is *about*;
it did not move the line numbers. The block above conflates the two, and they are different commits
three months apart.

**(b) There are two causes, not one, and they are different commits.**
`DungeonMethods.cs:323-333` moved `:323` -> `:371` at **`781f73b`**, *"feat(domain): report mutation
outcomes instead of silently no-opping"*, dated **2026-08-18** -- **one day** after this document was
authored, and the first of exactly three commits to touch that file after authorship.
`git diff --numstat 781f73b^ 781f73b -- "*DungeonMethods.cs"` returns `99  33`. **That one commit is
the whole +48.** `PlacedObject.cs:62-63` moved `:62` -> `:70` at **`b4e2a74`**,
*"feat(domain,application): add `ObjectKind.None` and validate placement"*, dated **2026-09-17** --
the `ObjectKind` work, not ObjectScale. The two-parameter `Update` held at `:62` through `16d594b`,
`413be78` and `070d535`, and moved only at `b4e2a74`, where the file went from **93** lines to
**101**.

**(c) One cause is claimed for "all three files", and no single commit moved both citations.**
`git diff --numstat 781f73b^ 781f73b -- "*PlacedObject.cs"` returns **nothing**.

**6b-xiii's ARITHMETIC was right and only its ATTRIBUTION was wrong**, which is worth separating
because the two are easy to condemn together. The block above reconstructs the `PlacedObject` shift as
*"the eight lines the four-parameter constructor and its blank separator occupied"*, and `b4e2a74`
took that file from 93 lines to 101 -- exactly **+8**. It derived the *size* correctly and named the
wrong commit.

**THE PROPERTY ALL OF THIS IS AN INSTANCE OF, STATED ONCE HERE AND AS A PROPERTY OF THE DOCUMENT
RATHER THAN OF ANY ONE ENTRY.** This audit document's citations were **accurate when they were
authored** and decay with the code beneath them.

- **`CORE-015` declares exactly 8 `locations:` ranges**, all naming `DungeonMethods.cs`, at `:761-768`
  at this session's baseline (`awk 'NR>=751 && NR<=776 && /^  - src\//'` returns 8; the 8 line
  numbers were then printed and read). **All 8 were exact declaration sites at `16d594b`, and all 8
  have drifted since**, each by the amount tabulated below.
- **`CORE-019` declares exactly 6 `locations:` ranges**, at `:933-938` at this session's baseline, of
  which **2** are the ones 6b-xiii corrected -- not both of this entry's locations, of which there are
  six. Both of those two were likewise **exact at `16d594b`**:
  `DungeonMethods.cs:323-333` was the four-parameter `AddPlacedObject` exactly, and in
  `PlacedObject.cs:62-63`, `:62` was the two-parameter `Update` signature and `:63` its expression
  body -- `git show 16d594b:...PlacedObject.cs | grep -n "void Update"` returns `62:` and `75:`.
- **6b-xiii's two corrections read as though this entry had mis-cited, and it had not.** Both
  corrections are true of the file at `b39a65d`, which is what they were measured against, and
  **neither of them says the citations once held.** A reader of the block above, or of the
  `MASTER-047` closure that summarises it, would reasonably conclude the 2026-08-17 audit was careless
  about line numbers. It was not.

**THE EIGHT DRIFTS, ONE ROW PER CITED LOCATION.** Drift is measured **at each declaration's opening
line**. The closing-line figures differ -- amendment A1 gives them as `+15` to `+48` -- and a bare
range cannot say
which of the two it means; that ambiguity is half of why the figure this table replaces went
unchallenged. Each row was produced by resolving the cited range against both commits, and
re-measured independently.

| Member | `CORE-015` cites | Declaration at `20b0b09` | Drift |
|---|---|---|---|
| `UpdateFloor` | `155-171` | `168` | **+13** |
| `AddArea` | `191-204` | `203` | **+12** |
| `RemoveArea` | `215-229` | `234` | **+19** |
| `UpdateArea` | `249-271` | `279` | **+30** |
| `UpdateAreaOutline` | `286-303` | `326` | **+40** |
| `AddPlacedObject` | `356-380` | `374` | **+18** |
| `RemovePlacedObject` | `392-408` | `413` | **+21** |
| `UpdatePlacedObject` | `429-452` | `459` | **+30** |

**This table replaces a false figure, and the defect that produced it is the defect this whole block
is about.** `measurement.md` section 1 claim 4 gave the range as `+13` to `+51`. **`+51` is `323` ->
`374`, the drift of the FOUR-parameter `AddPlacedObject`** -- which is a `CORE-019` citation and not
one of `CORE-015`'s eight. `CORE-015` cites `356-380`, the **five-parameter** overload, and that one
moved **+18**. The low end was wrong too: the true minimum is `AddArea`'s **+12**. A figure was
carried in from a neighbouring entry's citation and neither endpoint was measured. That row was **one
of three rows in `measurement.md` section 1 with no command named beside it** -- section 1 carries six
claims against a `Commands:` list of three bullets, and those bullets cover claims 1, 2 and 3, so
claims 4, 5 and 6 all lack one -- **and it is the only one of the three that states a figure**: the
other two state qualitative verdicts, which a reader can check by reading. The distinguishing
property was never uniqueness; it is that a figure was asserted where the document's own convention
names a command.

**So this is not one entry's defect.** Citations in this document were exact on 2026-08-17 and have
drifted with the code since, and nothing in this audit process re-measures an entry when the code
around it changes. **The durable address is the symbol name**; a `file:line` range in this document
should be read as a historical position at `16d594b` unless a later amendment re-measures it. That
applies to this block's own two in-document citations as well: `:761-768` and `:933-938` are positions
at `20b0b09`, and the `AMENDED 6b-xiv` block added to `CORE-015` above has already moved **the second
of these**: `:933-938` is now `:1002-1007`. `:761-768` is **unmoved**, because `CORE-015`'s
`locations:` block sits above this block's insertion point. (Re-run of the bullet's own predicate
after the edit: the `CORE-015` window returns `761 762 763 764 765 766 767 768`, the `CORE-019`
window `1002` through `1007`.) A statement about citation drift falsifying one of its own citations
in the act of being written, which is the
neighbour-falsification mechanism the `MASTER-047` closure describes, arriving inside a single block.

The record's severity, category, effort and risk are unchanged, and `MASTER-047` stays closed.**]**

### CORE-020 -- Common/Map holds three geometry types that no command, query or handler in the layer uses

```yaml
id: CORE-020
layer: CORE
severity: S3
category: structure
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.Application/Common/Map/MapDrawingSession.cs:30-38
  - src/DungeonMaster.Application/Common/Map/SnapZoneCalculator.cs:8-33
  - src/DungeonMaster.Application/Common/Map/TileCoordinateHelper.cs:9-52
effort: M
risk: low
independent: true
```

**What.** `MapDrawingSession`, `SnapZoneCalculator` and `TileCoordinateHelper` have no caller inside
`DungeonMaster.Application`. Their production callers are all in `DungeonMaster.App`
(`ViewModels/DungeonDetailViewModel.cs`, `Graphics/FloorMapDrawable.cs`,
`Views/DungeonDetailPage/DungeonDetailPage.xaml.cs`), which reach past the dispatcher and call them
directly. `MapDrawingSession` is stateful: the view model holds one as a mutable property.

**Why it matters.** I believe these are presentation-interaction concerns -- pixel-to-tile snapping and
in-progress drawing state -- that have been placed in the Application layer because they are
"not UI framework code", rather than because the application layer needs them. The consequence is that
the Application assembly's public surface is not the command/query contract it otherwise presents:
part of it is a utility library that the App layer consumes as a library. It also means a stateful,
non-thread-safe session object lives in a layer whose other types are all stateless per-dispatch
services.

**Suggested direction.** I believe the cleanest resolution is to move the coordinate helpers into the
App layer next to their only callers, and to decide explicitly whether `MapDrawingSession` is
application state (in which case a command should consume it) or view state (in which case it belongs
to the view model). The layer boundary itself is T6's record; this record covers only the fact that the
Application layer exposes types it does not use.

### CORE-021 -- Floor.UpdateNumber is the only entity mutator that does not call Touch

```yaml
id: CORE-021
layer: CORE
severity: S2
category: structure
authority: conventions
citation: backend/backend-conventions.md#3. Entity Conventions
locations:
  - src/DungeonMaster.Domain/Entities/Floors/FloorMethods.cs:13-14
  - src/DungeonMaster.Domain/Entities/Areas/AreaMethods.cs:19-29
  - src/DungeonMaster.Domain/Entities/Entity.cs:23-27
effort: XS
risk: low
independent: true
```

**What.** `internal void UpdateNumber(int number) => Number = number;` writes entity state without
calling `Touch()`. Every other mutator in the Domain -- `Area.Update`, `Area.SetOutline`,
`Floor.UpdateDetails`, `Floor.AddArea`, `Floor.RemoveArea`, `Floor.UpdateAreaOutline`, the three
`Floor` object methods, `PlacedObject.Update` and all of `Dungeon`'s -- does.

**Why it matters.** `backend-conventions.md` section 3 states that a mutable entity records a
last-modified timestamp updated whenever state changes, and this is the one place that rule is not
followed. There is no user-visible consequence today because `FloorDto` projects no timestamps, so the
floor's stale `LastModified` is never displayed. The inconsistency is the cost: a reader cannot rely on
"mutator implies `Touch`", and the moment a floor timestamp is surfaced, reordering will appear not to
have modified the floors it renumbered.

**Suggested direction.** Add `Touch()` to `UpdateNumber`, or -- if the omission is deliberate because
renumbering is bookkeeping rather than a user edit -- say so in the XML summary so the exception is
documented rather than inferred.

### CORE-022 -- Floor is split across three partial files where the conventions define a two-file split

```yaml
id: CORE-022
layer: CORE
severity: S4
category: structure
authority: conventions
citation: backend/dotnet-format.md#11. Domain Building Blocks in C#
locations:
  - src/DungeonMaster.Domain/Entities/Floors/Floor.cs:8
  - src/DungeonMaster.Domain/Entities/Floors/FloorMethods.cs:5
  - src/DungeonMaster.Domain/Entities/Floors/FloorObjectMethods.cs:5
  - src/DungeonMaster.Domain/Entities/Areas/Area.cs:8
  - src/DungeonMaster.Domain/Entities/Areas/AreaMethods.cs:5
  - src/DungeonMaster.Domain/Entities/Dungeons/Dungeon.cs:6
  - src/DungeonMaster.Domain/Entities/Dungeons/DungeonMethods.cs:5
  - src/DungeonMaster.Domain/Entities/Floors/PlacedObject.cs:8
effort: XS
risk: low
independent: true
```

**What.** The conventions define the entity split as exactly two files: `{Entity}.cs` for properties
and constructors, `{Entity}Methods.cs` for behaviour. `Dungeon` and `Area` follow this. `Floor` adds a
third partial, `FloorObjectMethods.cs`, holding the placed-object behaviour. `PlacedObject` is not
partial at all -- its two behaviour methods sit in `PlacedObject.cs` alongside its properties.

**Why it matters.** `dotnet-format.md` section 11 gives the split as a table of two files and states
its purpose: keeping the data shape readable at a glance while isolating behaviour. The split itself is
working as intended -- `Floor.cs` at 73 lines is genuinely readable next to 305 lines of behaviour, and
`DungeonMethods.cs` proves behaviour does grow independently. The deviation is the file count and the
inconsistency: three entities follow three different arrangements, so a reader has to look before
knowing where a method lives.

**Suggested direction.** Either accept the third file and record it in the conventions as a permitted
extension when behaviour clusters (which `FloorObjectMethods.cs` is a fair example of), or fold it back
into `FloorMethods.cs`. Whichever is chosen, apply it to `PlacedObject` too so all four entities read
the same way.

**[AMENDED 20261105-0106-core-two-row-sweep, 2026-11-05 -- `MASTER-110` is CLOSED done by this session's
task `T1`, and the line counts in this entry's `Why it matters.` paragraph are stale. Classification:
DRIFT.** The register row is `MASTER-110` at `00-master-index.md:298`, now closed **done**. **Nothing
below deletes any previous wording.** Every figure in this block is labelled either as measured by this
session's task `T4` on the session worktree after `T1`, or as told, with its source.

*Closure.* `MASTER-110` is closed **done** by this session; the remedy is its task `T1`. Gate 1 chose
"conform both": the fold-back option of the `Suggested direction.` paragraph above, applied to
`PlacedObject` too. `FloorObjectMethods.cs` was folded into `FloorMethods.cs` and deleted. `PlacedObject`
became `public sealed partial class`, split into `PlacedObject.cs` (constructors and properties) and
`PlacedObjectMethods.cs` (`Update`, `ValidateScale`). dm-conventions is unchanged. Measured by `T4`: PC
(section 10 of `.sir/sessions/20261105-0106-core-two-row-sweep/contracts/C1-measurement.md`),
`grep -n partial src/DungeonMaster.Domain/Entities/*/*.cs`, prints eight lines, two per entity, for
`Area`, `Dungeon`, `Floor` and `PlacedObject`, exactly the list that section requires after `T1`. `ls`
reports `src/DungeonMaster.Domain/Entities/Floors/FloorObjectMethods.cs` absent. M (section 8 of the
same file), its regex control re-run first, prints the startRef digest for both entities: `f3b9a065...`
over `Floor.cs` and `FloorMethods.cs`, and `ace0efdf...` over `PlacedObject.cs` and
`PlacedObjectMethods.cs`. Those startRef digests are told: measured by the architect at `11f9562` and
stated in C1.

*Correction -- the line counts in the `Why it matters.` paragraph.* Previous wording, verbatim, from
`:1567` of this file: "`Floor.cs` at 73 lines is genuinely readable next to 305 lines of behaviour". At
`11f9562`, `wc -l` gives `Floor.cs` 94, `FloorMethods.cs` 176 and `FloorObjectMethods.cs` 112, so
Floor's behaviour came to 288 lines. Those figures are told: measured by the architect and stated in
this session's C1 and C2 contracts. After `T1`, measured by `T4` with `wc -l`: `Floor.cs` 94 and
`FloorMethods.cs` 282. Both of the entry's figures were true when it was written. At the audit's own
commit `16d594b`, `git show 16d594b:<path> | wc -l` gives
`src/DungeonMaster.Domain/Entities/Floor/Floor.cs` 73, `FloorMethods.cs` 148 and `FloorObjectMethods.cs`
157, so behaviour came to 148 + 157 = 305, in a folder then named singular `Floor/`. That evidence is
told: it was supplied by the orchestrator, measured in the spec phase because tasks run without git,
and recorded in section 2 item 2 of this session's C3 contract. The code has since moved, so the
classification is DRIFT. The layer verdict at `:2940` (numbered as at `11f9562`, before this block was
inserted) restates both stale figures, and this block supersedes them there too.

*The `What.` paragraph's state claims.* "`Floor` adds a third partial" and "`PlacedObject` is not
partial at all" (`:1561-1563`) describe the pre-change state. Neither holds after `T1`.

*`locations:` is kept unchanged*, as a record of the pre-change state. After `T1`,
`FloorObjectMethods.cs:5` no longer exists. The other seven citations still name a partial declaration
on the cited line, `PlacedObject.cs:8` now included, as `T4` checked against PC's output.

*Other mentions, deliberately left unedited.* `:2302`, inside another entry's `AMENDED 6b-xvii` block,
refers to "`CORE-022`'s stale line counts, which `MASTER-110` carries"; this block is where those counts
are corrected. The layer verdict at `:2940-2941` stays as written, because it is dated audit narrative.
Both line numbers are as at `11f9562`, before this block was inserted. The record's severity, category,
effort and risk are unchanged.**]**

### CORE-023 -- RotateDirection.cs declares a type named RotationDirection

```yaml
id: CORE-023
layer: CORE
severity: S4
category: naming
authority: conventions
citation: backend/dotnet-format.md#2. File Organization
locations:
  - src/DungeonMaster.Domain/Entities/Floors/RotateDirection.cs:1-3
effort: XS
risk: low
independent: true
```

**What.** The file `RotateDirection.cs` contains `public enum RotationDirection { Clockwise,
CounterClockwise }`. The sibling `ScaleDirection.cs` declares `ScaleDirection`, matching.

**Why it matters.** `dotnet-format.md` section 2 requires that the file name match the type name
exactly. A grep or a "go to file" for `RotationDirection` misses this file, and the mismatch is the
kind that survives for years because nothing enforces it.

**Suggested direction.** Rename the file to `RotationDirection.cs`. Both callers are in
`DungeonMaster.App`, which references the type and not the file, so nothing else changes.

### CORE-024 -- Abbreviated identifiers ct, obj and CW90 are used throughout both layers

```yaml
id: CORE-024
layer: CORE
severity: S4
category: naming
authority: microsoft
citation: https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/general-naming-conventions
locations:
  - src/DungeonMaster.Application/Common/Dispatch/IDispatcher.cs:25-27
  - src/DungeonMaster.Application/Common/Dispatch/ICommandHandler.cs:29-31
  - src/DungeonMaster.Application/Common/Dispatch/IQueryHandler.cs:27-29
  - src/DungeonMaster.Application/Common/Behaviors/ValidationBehavior.cs:26
  - src/DungeonMaster.Application/Features/Dungeons/IDungeonWriteRepository.cs:22-24
  - src/DungeonMaster.Application/Features/Dungeons/IDungeonReadRepository.cs:20
  - src/DungeonMaster.Domain/Entities/Floors/FloorObjectMethods.cs:40
  - src/DungeonMaster.Domain/Entities/Floors/FloorObjectMethods.cs:65-73
  - src/DungeonMaster.Domain/Entities/Floors/ObjectRotation.cs:5-8
effort: S
risk: low
independent: true
```

**What.** The cancellation-token parameter is named `ct` on every interface, handler and repository
method in the Application layer. `FloorObjectMethods` names its local `obj`. `ObjectRotation`'s members
are `None`, `CW90`, `CW180`, `CW270`.

**Why it matters.** The Microsoft general naming conventions state "DO NOT use abbreviations or
contractions as part of identifier names" and "DO favor readability over brevity". `dotnet-format.md`
reinforces this for this specific parameter: every handler signature in sections 3, 9 and 10 spells it
`cancellationToken`, and the naming table's parameter example is `cancellationToken`. `ct` is the most
visible instance because it appears in every public signature the layer exposes.

**Suggested direction.** Rename `ct` to `cancellationToken` across the Dispatch interfaces, behaviors,
handlers and repository interfaces; rename `obj` to `placedObject` (which the surrounding lambdas
already use); and consider `Clockwise90` / `Clockwise180` / `Clockwise270` for the rotation members,
noting that persisted values are unaffected because only the names change.

**[AMENDED 6b-xvii, 2026-10-09 -- this entry's `Suggested direction.` contains a claim about persistence
that is measurably FALSE and would have rewritten stored data if acted on, and three of its `locations:`
citations are stale. Classification: ERROR for the persistence claim, DRIFT and a PROJECT MOVE for the
citations. The entry's substance -- that the three abbreviations are used throughout both layers --
stands, and two of the three were acted on in full.** The register row is `MASTER-130` at
`00-master-index.md:318`, closed **part done**. This block follows the form of the `AMENDED 6b-xvi` block
on `UI-003` at `03-app-shell-and-views.md:171-266`, which is cited **for its form only**: `AMENDED 6b-xiv`
at `:1293` of this file records that the block that one was modelled on *"carries three defects of its
own"*, and a reader following the pointer should arrive already knowing that. **Nothing below deletes any
previous wording.**

*Correction 1 -- the persistence claim in `Suggested direction.`, which is false.* Previous wording,
verbatim: *"and consider `Clockwise90` / `Clockwise180` / `Clockwise270` for the rotation members, noting
that persisted values are unaffected because only the names change."* **`ObjectRotation` is persisted by
member NAME, so renaming the members rewrites every stored value.** Read directly from
`src/DungeonMaster.Infrastructure/Persistence/Configurations/PlacedObjectConfiguration.cs`, the `Rotation`
configuration chain occupies three lines and each is named by its content rather than by a count derived
from the range: `:30` is `builder.Property(obj => obj.Rotation)`, `:31` is `.IsRequired()`, and `:32` is
`.HasConversion<string>();`. The sibling `Kind` chain's own `.HasConversion<string>();` sits at `:28`, so
`grep -n 'HasConversion<string>()'` on that file returns two lines and **only the second belongs to this
claim** -- stated here because a reader re-running that grep would otherwise have to guess which. The
column is `TEXT`: `measurement.md` section 3 records `b.Property<string>("Rotation")` with
`.HasColumnType("TEXT")` at `AppDbContextModelSnapshot.cs:141`, attributed rather than re-measured here
because that file was not read in this task. **The rename is therefore the same blocker class as the
`PathSegmentKind` row this register already parks as blocked**, and it was excluded by user decision; a
backlog item covers both renames with real migrations. The members are confirmed unchanged at
`src/DungeonMaster.Domain/Entities/Floors/ObjectRotation.cs` -- `None` `:11`, `CW90` `:16`, `CW180` `:21`,
`CW270` `:26`, located by `grep -nE 'None|CW90|CW180|CW270'` with `///` lines filtered out. The `:5-8`
range this entry cites for `ObjectRotation` was exact at the session base per `measurement.md` section 3 and has
since moved, because `CORE-025`'s remedy inserted a type summary and four member summaries above and
between those members.

*Correction 2 -- the two repository-interface citations, stale by PROJECT PATH rather than by line.*
Previous wording, verbatim: `src/DungeonMaster.Application/Features/Dungeons/IDungeonWriteRepository.cs:22-24`
and `src/DungeonMaster.Application/Features/Dungeons/IDungeonReadRepository.cs:20`. **Neither directory
holds either file.** Both interfaces now live at `src/DungeonMaster.Application/Common/Interfaces/`, where
`ls` returns exactly four entries -- `IDungeonReadRepository.cs`, `IDungeonWriteRepository.cs`,
`IUnitOfWork.cs` and `IUnitOfWorkFactory.cs`. That move is **`CORE-029`'s remedy**, and the same move is
what falsified `CORE-028` outright; the two corrections share one cause. The line numbers inside those
files are not restated here, because this session renamed the very parameter the citations point at and a
re-derived line would be a position in a file this entry no longer describes correctly.

*Correction 3 -- the `obj` citation, which points at a BLANK LINE.* Previous wording, verbatim:
`src/DungeonMaster.Domain/Entities/Floors/FloorObjectMethods.cs:40`. **Line 40 of that file holds no
`obj`; it holds nothing at all.** Read CR-safely with `cat -A` piped through `awk` -- `sed -n` and `awk`
patterns both strip CR in this shell and cannot be trusted for this -- `:40` is exactly `^M$`, a bare CRLF
separator line. Its neighbours anchor it by content: `:38` is `_objects.Add(placedObject);`, `:39` is
`Touch();`, `:41` is `return placedObject;`. **This citation is one of the four this session treats as
stable**, because the only edit the file received in 6b-xvii was a local rename that changed no line count,
so `:40` was a blank line at the session base too and the entry's figure was wrong when written rather than
drifted since. The real `obj` occurrences at the session base were nine, at `:31, :38, :41, :56, :59, :64, :95,
:98, :103` across three distinct locals, per `measurement.md` section 3. **All of them are now gone**:
`grep -nw 'obj'` on that file returns **0 lines**, and that zero discriminates rather than passing
vacuously -- the three replacement names `placedObject`, `objectToRemove` and `objectToUpdate` all
*contain* the substring `obj` and correctly fail `-w`. The replacements are at `:31` (`var placedObject`),
`:56` (`var objectToRemove`) and `:95` (`var objectToUpdate`), with `placedObject` as the lambda parameter
at `:57` and `:96`, all located by `grep -nE 'placedObject|objectToRemove|objectToUpdate'`.

*What was done, and what the residue is.* `ct` is renamed to `cancellationToken` throughout
`DungeonMaster.Application` and `DungeonMaster.Infrastructure`.
`grep -rn 'CancellationToken ct\b' src --include=*.cs`, with `/obj/` and `/bin/` excluded, returns **28**
declaration sites on the post-change tree, in **18** files by `grep -rl` -- **17** under
`src/DungeonMaster.Tests/` and **1** under `src/DungeonMaster.App/`, and **zero** in either layer this
entry is filed against. Those 28 were excluded by escalation E2's scope decision and go to the backlog.
**One measured fact about the residue changes what a future reader should do with it.**
`grep -rn 'param name="ct"' src --include=*.cs` returns **15** tags, all under `src/DungeonMaster.Tests/`,
and pairing the two commands per file shows **1 tag against 1 live `ct` declaration in each of the 15
files**. So those tags are *correct documentation of the code as it stands today*, not stale residue from a
half-finished sweep: renaming them on their own would make the documentation wrong, and they must move
with their parameters. **The authorities are verified exact**, read directly from
`dm-conventions/backend/dotnet-format.md`: `:36` gives the parameter example as `cancellationToken`, and
`:42` requires descriptive lambda and local names.

**The `Why it matters.` paragraph is left exactly as written**, and so is `What.` -- the latter describes
how the code stood when the audit ran and is a record rather than a live claim. **No commit hash appears
in this block and no placeholder for one:** a DIR task cannot invoke git, so a hash here would be copied
rather than verified. **Every line number in this block except the attributed pre-change figures was
re-derived against the post-change worktree by content search**, and the positions quoted from
`measurement.md` are historical positions at the session base that are **NOT navigable at `HEAD`**. The record's
severity, category, effort and risk are unchanged.**]**

**[AMENDED 20261105-1907-core-heavy-rows, 2026-11-05 -- the `CW90` / `CW180` / `CW270` rename this entry's
`Suggested direction.` proposes is now DONE, with the data migration the `AMENDED 6b-xvii` block above said it
needed, and that block's comparison with `PathSegmentKind` is corrected.** The register row is `MASTER-130` at
`00-master-index.md:318`, which stays **part done**. **Nothing below deletes any previous wording.** Every
figure in this block is labelled either as measured by this session's task `T4` on the session worktree, or as
told, with its source. The contract files cited below are in
`.sir/sessions/20261105-1907-core-heavy-rows/contracts/`.

*The `CW90` family, closed.* Measured by `T4`: `grep -n Clockwise` on
`src/DungeonMaster.Domain/Entities/Floors/ObjectRotation.cs` prints `:16` `Clockwise90,`, `:21` `Clockwise180,`
and `:26` `Clockwise270`, and `None` is unchanged at `:11`. `ls src/DungeonMaster.Infrastructure/Migrations/`
lists `20261105195218_RenameObjectRotationClockwiseMembers.cs` and its `.Designer.cs`. The migration's `Up`
rewrites the stored `PlacedObjects.Rotation` strings `'CW90'` to `'Clockwise90'`, `'CW180'` to `'Clockwise180'`
and `'CW270'` to `'Clockwise270'`; its `Down` is the exact inverse; `'None'` and every other value are left
untouched in both directions (`C2-rotation-migration.md` section 2, read by `T4` against the migration's two
`UPDATE` statements). The proof is the test class `RenameObjectRotationClockwiseMembersMigrationTests`. P-CW
(`C1-measurement.md` section 2) prints **0 lines**, measured by `T4` after re-running its exclusion control,
which kept only its one must-keep line. Told, not measured by `T4`: `T1`'s review record
(`.sir/sessions/20261105-1907-core-heavy-rows/reviews/T1.md`) reports the five migration tests passing, the
solution build at 0 warnings and 0 errors, and the full suite at 187, 958 and 123 passed with 0 failed. The
rename `Suggested direction.` proposes is therefore done, and done with the data migration the
`AMENDED 6b-xvii` block said it needed, not on that paragraph's premise that persisted values are unaffected.

*Correction -- the `PathSegmentKind` comparison in the `AMENDED 6b-xvii` block.* Previous wording, verbatim,
joined across its line breaks (`:1712-1714` of this file): "**The rename is therefore the same blocker class as
the `PathSegmentKind` row this register already parks as blocked**, and it was excluded by user decision; a
backlog item covers both renames with real migrations." Measured by `T4`:
`src/DungeonMaster.Domain/ValueObjects/PathSegmentKind.cs`, read in full, declares two members, `Line` at `:11`
(`grep -n 'Line,'`) and `CubicBezier` at `:16` (`grep -n CubicBezier`); neither is abbreviated, so no rename is
owed. `PathSegmentKind` is persisted by **ordinal**: it is the `Kind` of each `PathSegment` in the
`Areas.Outline` JSON column, serialised with `new(JsonSerializerDefaults.General)` at
`src/DungeonMaster.Infrastructure/Persistence/Configurations/AreaConfiguration.cs:16`, and JC
(`C1-measurement.md` section 3) prints nothing, after its control printed 2, so no JSON enum converter exists
anywhere in `src`; `PathSegmentJsonRoundTripTests.cs:28-29` stores `"Kind": 0` and `"Kind": 1` accordingly.
Renaming its members would rewrite nothing stored; renumbering them would. The blocker the register parked it
on (`MASTER-178`, `00-master-index.md:366`) concerned adding a zero member -- a renumbering, not a rename. "The
same blocker class" is therefore imprecise, and "both renames" is false: one rename was owed, and it is the one
recorded above.

*Line positions in the `AMENDED 6b-xvii` block, re-measured by `T4`.* In
`src/DungeonMaster.Infrastructure/Persistence/Configurations/PlacedObjectConfiguration.cs`, the `Rotation` chain
that block places at `:30`, `:31` and `:32` is now at `:39-42`: `:39` `builder.Property(obj =>`, `:40`
`obj.Rotation)`, `:41` `.IsRequired()`, `:42` `.HasConversion<string>();`, by `grep -n Rotation`, which prints
`:40`, and a read of the lines around it. The `Kind` chain's `.HasConversion<string>();`, which that block
places at `:28`, is now at `:37`: `grep -n 'HasConversion<string>()'` prints `:37` and `:42`, and only `:42`
belongs to the `Rotation` chain. In `ObjectRotation.cs`, the block's `:16`, `:21` and `:26` now hold
`Clockwise90`, `Clockwise180` and `Clockwise270`, as measured above.

*Why the entry stays part done.* The `ct` residue and the `obj` residue (`MASTER-130`, EXCLUSIONS 2 and 3) are
unchanged and outside this session's scope; their figures in the `AMENDED 6b-xvii` block are carried as
recorded there, not re-measured. This entry's `What.`, `Why it matters.` and `Suggested direction.` paragraphs
and its `locations:` list are left exactly as written. **No hash for this session's commit appears in this
block:** this session's tasks run without git and the orchestrator makes the commit. The record's severity,
category, effort and risk are unchanged.**]**

**[AMENDED 20261109-1641-part-done-rows, 2026-11-09 -- `MASTER-130` is CLOSED done: the `ct` residue and the
`obj` residue that the `AMENDED 20261105-1907-core-heavy-rows` block above left part done are both cleared.**
The register row is `MASTER-130` at `00-master-index.md:318`. **Nothing below deletes any previous wording.**
Every figure in this block is labelled either as measured by this session's task `T3` on the session worktree,
or as told, with its source. The contract files cited below are in
`.sir/sessions/20261109-1641-part-done-rows/contracts/`, and the review records in
`.sir/sessions/20261109-1641-part-done-rows/reviews/`.

*Closure.* `MASTER-130` (`00-master-index.md:318`) is CLOSED done by this session. The `CW90` family was
cleared by `20261105-1907-core-heavy-rows`, as the block above records; it is not re-measured here. Measured
by `T3`, each instrument re-run after the fixture control of `C1-measurement.md` section 4.0 gave its stated
result: **`ct` (EXCLUSION 2) is cleared** -- CT-DECL (`C1-measurement.md` section 4.1) prints 0 lines and TAG
(section 4.2) prints 0, where the startRef figures in section 4.1 and 4.2 are 28 lines in 18 files and 15
(told); CT-NEW prints 28 lines in 18 files and TAG-NEW 15 lines in 15 files, the 18 being exactly the files
`T1` changed and the 15 lying within them; CT-WORD prints exactly one line,
`src/DungeonMaster.App/Controls/NotebookView/NotebookView.xaml.cs:151`, a comment naming the argument of a
rejected `Task.Delay` alternative and not a parameter, the one residue, left deliberately. **`obj`
(EXCLUSION 3) is cleared** -- OBJ-WORD (section 4.3), the instrument EXCLUSION 3 used, prints only the four
exempt `src/DungeonMaster.Domain/Entities/Entity.cs` lines, `:65`, `:69`, `:71` and `:72`, and OBJ-HEAD prints
0 (startRef 11, told). The eleven lambda parameters in `FloorConfiguration.cs`, `PlacedObjectConfiguration.cs`
and `DungeonReadRepository.cs` are named `placedObject`, the name this entry's `Suggested direction.` gives:
`grep -rnwE "placedObject =>"` over `src/DungeonMaster.Infrastructure`, with `--include=*.cs` and `obj` and
`bin` directories excluded, prints 11 lines. The two `DungeonReadRepository.cs` comments naming the parameter
were renamed with it, `:91` and `:133` (`grep -n 'placedObject.Position'` on that file prints them beside the
code line `:45`). **The record's "17 lines", settled** (told: orchestrator measurement in the spec phase,
section 4.3's unit settlement): the record counted lines; at this session's startRef the same instrument gave
28 non-exempt lines, 18 + 8 + 2, with still eleven lambdas -- DRIFT, not error. **Model and behaviour, told**
from `reviews/T2.md`: `dotnet ef migrations has-pending-model-changes` prints `No changes have been made to the
model since the last migration.` after the rename, and printed `Changes have been made to the model since the
last migration. Add a new migration.` under `T2`'s injected must-fail control; the solution build is at
0 warnings and 0 errors and the full suite passes at 217, 976 and 137 with 0 failed and 0 skipped, as
`reviews/T1.md` also reports after the `ct` rename.

*Line contents quoted by the earlier blocks now read differently, by rename rather than drift.* The
`AMENDED 6b-xvii` block's quotation of the `Rotation` chain at `PlacedObjectConfiguration.cs:30`, and the
`AMENDED 20261105-1907-core-heavy-rows` block's quotations of that file's `:39` and `:40`, give the lambda
parameter its old name. Read by `T3` after `T2`, in
`src/DungeonMaster.Infrastructure/Persistence/Configurations/PlacedObjectConfiguration.cs`, `:39` is
`builder.Property(placedObject =>` and `:40` is `placedObject.Rotation)` -- the same lines, now with
`placedObject`. Those quotations are records of the pre-rename text and are not edited: QOBJ
(`C1-measurement.md` section 4.5), re-run by `T3` before its first edit, prints 15 lines of audit text quoting
the old lambda code, across `00-master-index.md`, this file and `02-infrastructure.md`, and none is edited.

*`FloorObjectMethods.cs`, measured.* The `AMENDED 6b-xvii` block locates its `obj` clearance in
`src/DungeonMaster.Domain/Entities/Floors/FloorObjectMethods.cs`, which `ls src/DungeonMaster.Domain/Entities/Floors/`
no longer lists.
`grep -rnw placedObject src/DungeonMaster.Domain --include=*.cs --exclude-dir=obj --exclude-dir=bin` finds the
names that block describes in `src/DungeonMaster.Domain/Entities/Floors/FloorMethods.cs`: the construction
local at `:201` (`var placedObject = new PlacedObject(`) and the lambda parameters at `:226`
(`var objectToRemove = _objects.FirstOrDefault(placedObject =>`) and `:265`
(`var objectToUpdate = _objects.FirstOrDefault(placedObject =>`); `grep -cw obj` on that file prints 0. No
classification of the move is given, because `T3` cites no committed record of the session that made it;
`.sir/lessons.md:70` records the fold of `FloorObjectMethods.cs` into `FloorMethods.cs` (told).

*What remains outside the row.* Measured by `T3` with OBJ-WORD's flags over the projects outside the row's
recorded population of Application, Infrastructure and Domain (`C1-measurement.md` section 4.3): `obj` still
names locals and parameters in `src/DungeonMaster.App` (32 lines in 2 files) and `src/DungeonMaster.Tests`
(46 lines in 7 files, two of the lines `obj` as a directory name inside a string), and `src/DungeonMaster.App.Tests`
holds 1 line in 1 file; `src/DungeonMaster.App.Core`, `src/DungeonMaster.App.UnitTests` and
`src/DungeonMaster.App.UiTests` hold 0. They are not a residue of `MASTER-130` and are recorded so that the
closure is not read as covering them. Gate 2 left this population for later work: it is carried to
`.sir/backlog.md` as a new item added by the orchestrator at Gate 3 (told: this session's `spec.md`, Scope).

*Moved citations.* This block's insertion moves every line below it. The moved-citation paragraph of the
`AMENDED 20261109-1641-part-done-rows` block on `CORE-016` records the citations this session's two insertions
moved, this block's included; no citation is edited.

This entry's `What.`, `Why it matters.` and `Suggested direction.` paragraphs and its `locations:` list are left
exactly as written. **No hash for this session's commit appears in this block:** this session's tasks run
without git and the orchestrator makes the commit. The record's severity, category, effort and risk are
unchanged.**]**

### CORE-025 -- Six public Domain types carry no XML documentation

```yaml
id: CORE-025
layer: CORE
severity: S4
category: docs
authority: conventions
citation: backend/dotnet-format.md#9. XML Documentation Comments
locations:
  - src/DungeonMaster.Domain/Entities/Floors/ObjectKind.cs:3
  - src/DungeonMaster.Domain/Entities/Floors/ObjectRotation.cs:3
  - src/DungeonMaster.Domain/Entities/Floors/ObjectScale.cs:3-11
  - src/DungeonMaster.Domain/Entities/Floors/SnapAnchor.cs:3
  - src/DungeonMaster.Domain/Entities/Floors/RotateDirection.cs:3
  - src/DungeonMaster.Domain/Entities/Floors/ScaleDirection.cs:3
effort: S
risk: low
independent: true
```

**What.** `ObjectKind` (37 members), `ObjectRotation`, `SnapAnchor`, `RotationDirection` and
`ScaleDirection` have no `<summary>` on the type or on any member. `ObjectScale` documents neither the
class nor its four constants nor its two methods. Everything else in both layers is documented, often
thoroughly -- `PathSegmentKind` documents both of its members.

**Why it matters.** `dotnet-format.md` section 9 states that all public types, members and parameters
must have XML documentation comments. The gap is concentrated in exactly the types where a reader most
needs prose: `ObjectKind`'s semantics are carried entirely by section comments drawn with box-drawing
characters, `SnapAnchor`'s members do not explain what a "North" anchor snaps to, and `ObjectScale`'s
`StepUp`/`StepDown` clamping behaviour is discoverable only by reading the expression body.

**Suggested direction.** Add type-level summaries to all six, member summaries to `SnapAnchor`,
`ObjectRotation`, `RotationDirection` and `ScaleDirection`, and constant/method summaries to
`ObjectScale`. `ObjectKind`'s 37 members can reasonably keep their grouping comments with a type-level
summary that points at the grouping.

**[AMENDED 6b-xvii, 2026-10-09 -- `ObjectKind`'s member count is wrong in two places in this entry, and
the entry's central claim is not true of `ObjectKind`. Classification: ERROR for the count, PARTIALLY
FALSIFIED for the claim; the entry's substance holds for the other five types and was acted on in full.**
The register row is `MASTER-131` at `00-master-index.md:319`, closed **done**. This block follows the form
of the `AMENDED 6b-xvi` block on `UI-003` at `03-app-shell-and-views.md:171-266`, cited **for its form
only** -- `AMENDED 6b-xiv` at `:1293` of this file records that the block that one was modelled on
*"carries three defects of its own"*. **Nothing below deletes any previous wording.**

*Correction 1 -- the member count, which appears twice.* Previous wording, verbatim: *"`ObjectKind` (37
members)"* in the `What.` paragraph, and *"`ObjectKind`'s 37 members can reasonably keep their grouping
comments"* in `Suggested direction.`. **The true count is 38.** **Both independent predicates agreed on
38, and both were re-run against the post-change tree** rather than taken from `measurement.md`:
`grep -cE '^[[:space:]]+[A-Za-z][A-Za-z0-9]*,?$'` on
`src/DungeonMaster.Domain/Entities/Floors/ObjectKind.cs` returns **38**, and the comma-required form
`grep -cE '^[[:space:]]+[A-Za-z][A-Za-z0-9]*,$'` also returns **38**. A third form, tab-anchored, agrees:
with `TAB=$(printf '\t')`, `grep -cE "^${TAB}[A-Za-z][A-Za-z0-9]*,?$"` returns **38**. **That third form
is written with a shell-expanded tab deliberately: `\t` inside `grep -E` in this shell matches a literal
`t`, not a tab, and the naive `^\t...` form returns 0** -- a confident wrong number rather than an error,
and it is on this project's standing instrument-trap list. **The predicate was validated before its total
was trusted**, by naming one case that must match and three near-misses that must not: `None,` at `:13`
matches; `public enum ObjectKind` at `:7`, every `/// <summary>` line, and the five box-drawing grouping
comments all correctly do not. **The total was then reached a fourth time by an instrument that could not
share the line-shape predicates' mistake** -- arithmetic over the groups those comments delimit, which
counts boundaries rather than line shapes: 1 (`None`, `:13`) + 5 (doors, `:16-20`) + 12 (furniture,
`:23-34`) + 8 (storage, `:37-44`) + 5 (lighting, `:47-51`) + 7 (decoration, `:54-60`) = **38**. The five
grouping comments are at `:15`, `:22`, `:36`, `:46` and `:53`. The first member is `None` at `:13` and the
last is `WeaponRack` at `:60`. The discrepancy is consistent with `None` having been added by the
enum-zero work after this audit was written, which `CORE-035` records.

*Correction 2 -- the `What.` paragraph's claim that `ObjectKind` has no documentation, which is false of
that file.* Previous wording, verbatim: *"`ObjectKind` (37 members), `ObjectRotation`, `SnapAnchor`,
`RotationDirection` and `ScaleDirection` have no `<summary>` on the type or on any member."* **`ObjectKind`
already documented its `None` member before this session touched it.** `grep -c '<summary>'` on that file
returns **2** now; `measurement.md` section 3 records **1** at the session base, with the `None` member's summary
at `:5` and the type declaration at `:3` undocumented. So the clause was correct about the *type* and
wrong about *"or on any member"* for this one file; it holds for the other four named types, each of which
`measurement.md` section 3 measured at **0**. Post-change, the pre-existing summary block sits at `:9-12`
with its member at `:13`, and the type declaration is at `:7` by
`grep -n 'public enum ObjectKind'` -- the `+4` is the type summary this session's remedy inserted above
it. **That pre-existing text was preserved byte-for-byte**, not rewritten: the reviewer compared it with
`cmp` against the pre-change blob and got identity at **176 bytes**, with an offset control that differed,
so the comparison was not vacuous. **The scope of the remedy on this file is therefore narrower than the
entry's `Suggested direction.` reads:** `ObjectKind` received a **type-level summary only**, no per-member
summaries, and its grouping comments were kept, which is the one disposition the entry does explicitly
allow.

*Correction 3 -- `ObjectScale` is not an enum, and its two methods needed more than summaries.* Previous
wording, verbatim: *"`ObjectScale` documents neither the class nor its four constants nor its two
methods"* in `What.`, and *"constant/method summaries to `ObjectScale`"* in `Suggested direction.`. The
`What.` sentence is right about what is missing and right to call it a class; the correction is to the
remedy. `grep -n 'public static class ObjectScale'` puts the declaration at `:6` of
`src/DungeonMaster.Domain/Entities/Floors/ObjectScale.cs`; it holds four `public const float` members --
`Min` `:11`, `Max` `:16`, `Step` `:21`, `Default` `:26` -- and two static methods, `StepUp` `:37` and
`StepDown` `:48`, each taking one `float current`. **Because `dotnet-format.md:372` covers PARAMETERS as
well as types and members, those two methods required `<param name="current">` and `<returns>` tags**,
which "constant/method summaries" understates. Both were added. **The authority is verified exact**, read
directly from `dm-conventions/backend/dotnet-format.md:372`: "All public types, members, and parameters
must have XML documentation comments."

*One shape fact that changed the remedy for two of the five enums, recorded because it is not visible from
the entry.* `ScaleDirection` and `RotationDirection` were each declared as a **single-line body** --
`measurement.md` section 3 reads `ScaleDirection.cs:3` as `public enum ScaleDirection { Up, Down }` -- so
documenting their members required expanding both bodies to a braced multi-line form rather than merely
inserting comments. The entry's `Suggested direction.` does not mention that, and it is the reason those
two files grew from 3-line bodies to 17 lines each.

**A PRE-EXISTING deviation inside this entry's own preserved text is filed separately rather than folded
in here.** `ObjectKind.cs:10` breaks a sentence across two `///` lines -- `:10` ends
`is a <see cref="PlacedObject"/>,` and the sentence finishes on `:11` -- violating
`dotnet-format.md:378`. **It could not have been fixed by this entry's remedy without breaking that
remedy's own byte-for-byte preservation requirement**, so it is filed as `MASTER-180` at
`00-master-index.md:368`, with `CORE-036` in this file as its source entry.

**The `Why it matters.` paragraph is left exactly as written.** **No commit hash appears in this block and
no placeholder for one:** a DIR task cannot invoke git, so a hash here would be copied rather than
verified. **Every line number in this block except the attributed pre-change figures was re-derived against
the post-change worktree by content search**, and the positions quoted from `measurement.md` -- `:3`, `:5`
and `:5-8` for `ObjectKind`, `:3` for `ScaleDirection` -- are historical positions at the session base that are
**NOT navigable at `HEAD`**, because this session's own remedy inserted documentation above them. **All six
`locations:` ranges in the YAML block are stale for the same reason, and here that universal is MEASURED
rather than asserted**, each type's declaration located at `HEAD` by
`grep -nE '^(public|internal) (enum|static class|class|readonly struct|struct) '` on its own file, run over
all six in one pass against the post-change worktree. **Every one of the six cites `:3` and not one of them
is at `:3` any longer:** `ObjectKind.cs:3` is now `:7`, and `ObjectRotation.cs:3`, `ObjectScale.cs:3-11`,
`SnapAnchor.cs:3`, `RotateDirection.cs:3` and `ScaleDirection.cs:3` are each now `:6`. `sed -n '3p'` on each
of the six returns `/// <summary>` -- the first line of the documentation this session inserted -- which is
what makes the shared reason a reading of the files rather than an inference from one of them.
**The displacement is NOT a single constant, which is why it is enumerated:** `ObjectKind` moved +4 and the
other five +3, because its inserted type summary runs to four lines where theirs run to three. The fifth
range, `RotateDirection.cs:3`, is additionally stale by **file name**: `CORE-023`'s remedy renamed that file
to `RotationDirection.cs` in this same session, which is why the grep above had to be run against
`RotationDirection.cs` for that one. The record's severity, category, effort and risk are
unchanged.**]**

### CORE-026 -- Two files indent XML documentation continuation lines with spaces instead of two tabs

```yaml
id: CORE-026
layer: CORE
severity: S4
category: docs
authority: conventions
citation: backend/dotnet-format.md#9. XML Documentation Comments
locations:
  - src/DungeonMaster.Domain/ValueObjects/CanvasPoint.cs:3-8
  - src/DungeonMaster.Application/Features/Dungeons/Queries/GetDungeonDetail/PlacedObjectDto.cs:6-15
  - src/DungeonMaster.Domain/Entities/Floors/PlacedObject.cs:12
  - src/DungeonMaster.Domain/Entities/Floors/PlacedObject.cs:43-59
effort: XS
risk: low
independent: true
```

**What.** `CanvasPoint.cs` and `PlacedObjectDto.cs` write `///     Represents...` with five spaces
where every other file writes `///` followed by two tabs. Both files also put `<param>` descriptions on
the same line as the tag (`/// <param name="X">Horizontal pixel offset from the area origin.</param>`),
as does `PlacedObject.cs` for several of its summaries.

**Why it matters.** `dotnet-format.md` section 9 requires that tag descriptions go on a new line
indented with two tabs from the `///` marker. With no `.editorconfig` in the repository -- T6 owns that
record -- nothing normalises this, so the two styles will keep interleaving as files are edited in
different editors.

**Suggested direction.** Re-indent the affected comment blocks to the two-tab form and expand the
single-line `<summary>` and `<param>` tags in `PlacedObject.cs` and `PlacedObjectDto.cs` to the
multi-line shape the rest of both layers uses.

**[AMENDED 6b-xvii, 2026-10-09 -- this entry's title UNDERSTATES its own population by a factor of four,
its single-line-tag census was wrong in three ways, and a second deviation against the same conventions
section was found while reading the authority for it. Classification: the TITLE is FALSE as a population
claim; the census figures are ERRORS, two of them from a BLIND PREDICATE rather than a miscount; the
entry's substance stands and was acted on in full for the files it names.** The register row is
`MASTER-112` at `00-master-index.md:300`, closed **done** on the files this entry names, with the
remainder filed as `MASTER-181` at `00-master-index.md:369`. This block follows the form of the
`AMENDED 6b-xvi` block on `UI-003` at `03-app-shell-and-views.md:171-266`, cited **for its form only** --
`AMENDED 6b-xiv` at `:1293` of this file records that the block that one was modelled on *"carries three
defects of its own"*. **Nothing below deletes any previous wording.**

*Correction 1 -- the title's population claim.* Previous wording, verbatim: *"**Two** files indent XML
documentation continuation lines with spaces instead of two tabs"*, repeated in `What.` as *"`CanvasPoint.cs`
and `PlacedObjectDto.cs` write `///     Represents...` with five spaces where every other file writes
`///` followed by two tabs."* **The clause "every other file" is the false half, and the two named files
were 2 of 8.** One command over the tree,
`grep -rnE '^[[:space:]]*/// {2,}' src --include=*.cs` with `/bin/` and `/obj/` excluded: the orchestrator
ran it at the session base and measured **163 lines across 8 files**; this task re-ran the identical command on
the post-change tree and measured **159 lines across 6 files**. **The two censuses reconcile exactly**,
163 - 4 = 159 and 8 - 2 = 6, so the whole delta is the four lines in the two named files and the figures
are one measurement rather than two assertions. **The predicate is not blind in either direction:** it
returns **0** on the conforming control
`src/DungeonMaster.Application/Features/Dungeons/Queries/GetDungeons/GetDungeonsQueryHandler.cs` and
non-zero on each of the six survivors. The six, with per-file line counts from the same run:
`src/DungeonMaster.App/Graphics/FloorMapDrawable.cs` **116**,
`src/DungeonMaster.Infrastructure/Migrations/20260818232300_UtcDateTimeTimestampsAndScaleWithoutStoreDefault.cs`
**35**, `src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/FloorPersistenceTests.cs` **3**,
`src/DungeonMaster.App/Assets/ObjectAssets/ObjectAssetCatalogue.cs` **2**,
`src/DungeonMaster.App/Assets/ObjectAssets/ObjectKindInfo.cs` **2**, and
`src/DungeonMaster.Infrastructure/Persistence/Configurations/PlacedObjectConfiguration.cs` **1**. **None is
in the CORE layer**, which is why they are filed to a new row instead of widening this `XS` one.
`measurement.md` section 3 marks the repo-wide count **NOT MEASURED** at decomposition time, the per-file
census having timed out at 120 seconds; the single tree-wide `grep -r` above is the form that completes,
and the figure stops being unmeasured here.

*Correction 2 -- `PlacedObjectDto.cs`'s single-line tag positions.* Previous wording: this entry's
`locations:` block cites the range `:6-15`, and `MASTER-112`'s own working note named the four tags at
`:9, :10, :13, :14`. **The true positions at the session base were `:9, :10, :14, :15`.** `:13` is the
`/// </param>` **closer** of the `Position` tag that was already in conforming three-line form spanning
`:11-13`, so it was never a single-line tag; and `:15` was omitted. **The count of 4 was right and two of
the four positions were wrong**, which is the signature of a range read by eye rather than by predicate.

*Correction 3 -- `PlacedObject.cs`'s single-line summary count, wrong because the PREDICATE was wrong.*
Previous wording: *"as does `PlacedObject.cs` for several of its summaries"*, quantified in
`MASTER-112`'s working note as **5**. **The true count at the session base was 6, at `:12, :37, :40, :49, :52,
:55`**, with a seventh single-line tag, a `<param>`, at `:81`. The first predicate was
`<summary>[^<]+</summary>`, whose character class **cannot cross a nested tag**, so it silently skipped
`:12` -- whose summary contains `<see cref="PlacedObject"/>`. **This is the row worth carrying out of this
block: the count was wrong because the predicate was wrong, and re-running it would have returned 5 every
time.** A census validates its predicate, not only its total. The predicate that gets it right is
`grep -nE '///[[:space:]]*<(param|summary|returns)[^>]*>.*</(param|summary|returns)>'`, which returns
**0** on the conforming control `GetDungeonsQueryHandler.cs`.

*Correction 4 -- three single-line tags in `CanvasPoint.cs` that this entry does not mention at all.*
Previous wording: the `locations:` block cites only `CanvasPoint.cs:3-8`. **Three further single-line tags
sat at `:34`, `:35` and `:84`** at the session base, unenumerated because only the file's first 16 lines were
read before it was reported as censused. **All three sit OUTSIDE the file's type-level `<remarks>` block**,
which closed at `:33`, so expanding them did not disturb the block this session left deliberately
untouched -- the partition was checked by set membership rather than by subtracting one count from
another.

*The state after the remedy.* Both predicates return **0** on all three files --
`src/DungeonMaster.Domain/ValueObjects/CanvasPoint.cs`,
`src/DungeonMaster.Application/Features/Dungeons/Queries/GetDungeonDetail/PlacedObjectDto.cs` and
`src/DungeonMaster.Domain/Entities/Floors/PlacedObject.cs` -- re-run by this task against the post-change
worktree. **Both zeros were shown able to be non-zero**: the space-indent predicate returns 159 over the
rest of the tree in the same run, and the single-line-tag predicate returns **1** on a hand-written
`/// <summary>Whole thing.</summary>` CRLF fixture. **The authority is verified exact**, read directly
from `dm-conventions/backend/dotnet-format.md:375`: "Tag descriptions go on a **new line**, indented with
**two tabs** from the `///` marker".

**A SECOND deviation against the same conventions section was found while reading the authority for this
entry, and its status has since changed -- which is recorded here rather than glossed.** `dotnet-format.md:378`
also requires "**Line breaks occur only at sentence boundaries.** Never break a sentence across multiple
`///` lines." `CanvasPoint.cs` breaks sentences mid-clause across `///` lines, on **both sides** of its
type-level `<remarks>` block, and so does `PlacedObject.cs`. **At decomposition this was surfaced as an
UNFILED finding and deliberately not acted on**, so that an `XS` row's scope would not quietly absorb a
second deviation. **It is no longer unfiled:** by user decision at Gate 2 it became `MASTER-180` at
`00-master-index.md:368`, with `CORE-036` in this file as its source entry, and the enumerated line list
and the predicate behind it live there. It was still **not fixed** -- only filed. **The reason it needed
its own row rather than a note is that the deviation is wider than the single `<remarks>` block it was
first scoped to**, which the T3 review raised as a `contradicts` finding, and **the census behind it is
predicate-dependent: four predicates produced four different totals for one population**, which is why
`MASTER-180` carries an enumerated line list and no total at all.

**The `Why it matters.` and `Suggested direction.` paragraphs are left exactly as written**, including
`Why it matters.`'s parenthetical that T6 owns the `.editorconfig` record. **No commit hash appears in
this block and no placeholder for one:** a DIR task cannot invoke git, so a hash here would be copied
rather than verified. **The line positions in corrections 2, 3 and 4 are historical positions at the session base
and are NOT navigable at `HEAD`**: this session's own remedy expanded every one of those tags from one
line to three, which displaces everything below each of them, and the post-change state of those files is
recorded above as a pair of zeros rather than as a new set of positions, because the constructs those
numbers named no longer exist. **The four `locations:` ranges in the YAML block are NOT all stale, and they
are NOT stale "for the same reason" -- the previous wording here, verbatim, was *"All four `locations:`
ranges in the YAML block are stale for the same reason"*, attributing all four to this session's expansion
of every tag from one line to three displacing everything below.** That reason cannot reach two of them, so
the four are enumerated below, each boundary named by its line **and by what sits on that line at `HEAD`**
rather than by any count derived from a range. The contents were read directly from the post-change
worktree with `sed -n` ranges over each file, plus `cat -A` on `CanvasPoint.cs` so the indentation is
visible rather than inferred. **A line number that moved and a construct that changed shape are different
facts, and this entry no longer collapses them.**

*(1) `PlacedObjectDto.cs:6-15` -- end boundary stale, start boundary UNMOVED.* `:6` is still
`/// <summary>`. `:15` is now `/// <param name="Position">`, which is mid-block: the doc block's true end at
`HEAD` is `:23`, a `/// </param>`, with the declaration `public record PlacedObjectDto(` at `:24`. So the
range no longer bounds the construct, but nothing displaced its first line.

*(2) `PlacedObject.cs:43-59` -- displaced, which is the one range the original reason does fit.* `:43` is
now blank and the summary block there begins at `:44`, so the range's start no longer sits on a tag at all.

*(3) `CanvasPoint.cs:3-8` -- UNMOVED, and nothing in this file could have displaced it.* At `HEAD` `:3` is
`/// <summary>`, `:6` is `/// </summary>`, `:7` is `/// <remarks>` and `:8` is `///` followed by two tabs
and `<para>`. The orchestrator compared the same lines against the session-base blob and found the two
boundaries and everything between them identical, the only change being the indentation of `:4-5` from five
spaces to two tabs, with **no change in line count**; that comparison needs git history, which a DIR task
may not invoke, so it is attributed to the orchestrator rather than claimed as measured here. **Correction
4 above puts this file's three expanded tags at `:34`, `:35` and `:84` -- all BELOW `:3-8`** -- so the
displacement this block offered as the reason runs in the wrong direction for this range.

*(4) `PlacedObject.cs:12` -- number UNMOVED, construct CHANGED SHAPE.* `:12` is still `/// <summary>`: it
is the first expanded tag in the file, so there was nothing above it to push it down. What changed is the
shape of what it opens -- the summary now runs `:12-14` in the conforming three-line form, closing at `:14`
with `/// </summary>` above the `internal PlacedObject(` declaration at `:15`. The citation is therefore
still navigable and still lands on a `<summary>`, while the single-line tag the row was describing no
longer exists. The record's severity, category, effort and risk are unchanged.**]**

**[AMENDED 6b-xviii, 2026-10-09 -- `MASTER-181`, the row this entry's own remainder was filed to, is
CLOSED AS A DUPLICATE rather than carried forward, and its recorded layer is wrong in its first
element. Classification: the ROW is a DUPLICATE, merged by a user decision at Gate 1, not falsified;
this ENTRY's substance stands and stays closed, and NO CODE IS ATTRIBUTED TO `MASTER-181`.** The
register row is `MASTER-181` at `00-master-index.md:369`, closed **done AS A DUPLICATE of
`MASTER-122`**; `MASTER-122` is at `00-master-index.md:310` and carries the remedy. This block follows
the form of the `AMENDED 6b-xvii` block on `CORE-028`, re-derived by content search to **`:2156-2216`**
of this file after this block's own insertion moved it (it was `:2090-2125` before), cited **for its
form only** -- `AMENDED 6b-xiv` at `:1293` of this file records that the block that one was in turn
modelled on *"carries three defects of its own"*. **Nothing below deletes any previous wording.**

*Correction 1 -- the disposition of this entry's remainder.* Previous wording, verbatim, from the
`AMENDED 6b-xvii` block above: *"remainder filed as `MASTER-181` at `00-master-index.md:369`."*
**That remainder is not an independent row and never needed to be.** `MASTER-181` and `MASTER-122`
name the same defect under the same authority, and **116 of `MASTER-181`'s 159 lines are
`src/DungeonMaster.App/Graphics/FloorMapDrawable.cs`, which is `MASTER-122`'s own largest location** --
73% of the population by one file. The user's Gate 1 decision was to merge into `MASTER-122`, widen
that row to the measured population, and close this one as a duplicate. Measured on the post-change
tree by this task, per file, with `grep -cE '^[[:space:]]*/// {2,}'`: **0** on each of
`FloorMapDrawable.cs`, `src/DungeonMaster.App/Assets/ObjectAssets/ObjectAssetCatalogue.cs`,
`src/DungeonMaster.App/Assets/ObjectAssets/ObjectKindInfo.cs`,
`src/DungeonMaster.Infrastructure/Persistence/Configurations/PlacedObjectConfiguration.cs` and
`src/DungeonMaster.Tests/Infrastructure/Persistence/Repositories/FloorPersistenceTests.cs`, while the
conforming tab form `grep -cE "^[[:space:]]*///${TAB}"` with `TAB=$(printf '\t')` returns
**116 + 2 + 2 + 1 + 3 = 124** on the same five files -- a measured gain rather than a standing zero.
**POSITIVE CONTROL beside those five zeros**, because five zeros with no control are indistinguishable
from a blind predicate: the identical space predicate returns **35** on
`src/DungeonMaster.Infrastructure/Migrations/20260818232300_UtcDateTimeTimestampsAndScaleWithoutStoreDefault.cs`,
and the tab predicate returns **21** on `src/DungeonMaster.App/Controls/NotebookAffordance.cs` where the
`\t`-inside-`grep -E` spelling returns a false **0** on that same file -- the instrument already in this
register's inherited list, re-confirmed here by a third party. **The predicate was validated in both
directions** on a purpose-built CRLF fixture: a line carrying ONE space after `///` does not match, and
lines carrying two and five do, 2 of 4. **And one whole-tree pass closes the arithmetic**:
`grep -rnE '^[[:space:]]*/// {2,}' src --include=*.cs --exclude-dir=bin --exclude-dir=obj` returns
**35 matching lines in 1 file** post-change, the migration alone, against the orchestrator's **159 lines
in 6 files** at the session base. `159 - 124 = 35`, so the census and the per-file figures are one
measurement rather than two assertions. Note the `--exclude-dir` form: applying the `bin`/`obj` filter
to the matched TEXT rather than to PATHS is what produced the discarded artefact figure of 167.

*Correction 2 -- `MASTER-181`'s recorded layer.* Previous wording, verbatim, the layer cell of that row:
*"UI, INFRA, TEST"*. **It is wrong in its first element, and only in its first element.** The merged
population spans **UIKIT, INFRA and TEST**: three of the six files are in `DungeonMaster.App`'s control
kit and graphics (`FloorMapDrawable.cs`, `ObjectAssetCatalogue.cs`, `ObjectKindInfo.cs`), which this
register classifies `UIKIT` and not `UI`; two are `INFRA` (`PlacedObjectConfiguration.cs` and the
migration) and one is `TEST` (`FloorPersistenceTests.cs`). The distinction is not cosmetic here: it is
the whole reason the row belonged with the `UIKIT` batch this session selected by layer concentration,
and a reader filtering the register on `UIKIT` would not have found it.

*Correction 3 -- the judgment call this entry's remainder row raised and declined to make.* Previous
wording, verbatim, from `MASTER-181`'s own row text: *"the migration's 35 lines also need a judgment
call this row does not make, since migrations are generated artefacts and may be exempt"*. **That
question is now answered, and the answer is a standing exemption rather than deferred work.** The user
decided at Gate 2 that generated EF scaffolder output is **EXEMPT** from `dotnet-format.md` section 9's
two-tab rule as a standing convention of this repository. The full statement of that convention, with
its evidence, is recorded on `UIKIT-022` in `04-app-controls-and-theme.md`; recorded here because this
is the entry whose remainder raised it. **`124 remedied + 35 exempt = 159`, the full measured
population, and NO OPEN ROW COVERS THE 35 LINES AFTER THIS SESSION -- deliberately, because nothing is
owed.**

*What this block does NOT measure, stated so nothing downstream reads it as evidence.* **Which session
converted `FloorMapDrawable.cs` from spaces to tabs is NOT MEASURED by this session.** The file is
measured here at **0** leading-space lines against **722** leading-tab lines of **898**
(`grep -c '^ '`, `grep -c "^$TAB"`, `wc -l`, post-change), which falsifies `MASTER-122`'s file-level
census outright, but attributing the conversion to a session needs git archaeology nobody ran, and no
row claims it.**]**

### CORE-027 -- Five files indent with four spaces and two column-align assignments where the rest use tabs

```yaml
id: CORE-027
layer: CORE
severity: S4
category: style
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.Domain/Entities/Floors/ObjectKind.cs:5-50
  - src/DungeonMaster.Domain/Entities/Floors/ObjectRotation.cs:5-8
  - src/DungeonMaster.Domain/Entities/Floors/ObjectScale.cs:5-11
  - src/DungeonMaster.Domain/Entities/Floors/SnapAnchor.cs:5-9
  - src/DungeonMaster.Domain/Entities/Floors/PlacedObject.cs:36-40
  - src/DungeonMaster.Application/Features/Dungeons/Queries/GetDungeonDetail/PlacedObjectDto.cs:17-21
effort: XS
risk: low
independent: true
```

**What.** Sixty-five of the seventy-seven files in scope indent with tabs and seven contain no indented
line at all. The remaining five -- `ObjectKind.cs`, `ObjectRotation.cs`, `ObjectScale.cs`,
`SnapAnchor.cs` and `PlacedObjectDto.cs` -- indent with four spaces. Two files additionally pad
assignments into columns: `public const float Min     = 0.25f;` in `ObjectScale.cs`, and
`FloorId  = floorId; Kind     = kind;` in `PlacedObject.cs`, which is tab-indented and is cited above
for the alignment alone.

**Why it matters.** `dotnet-format.md` does not state an indent-character rule, so I am not asserting a
violation -- I believe the dominant convention in this repository is tabs, and these five files depart
from it. The practical cost is diff noise: a single-character edit inside a column-aligned block
re-aligns the whole block, and mixed indentation makes whitespace-only churn hard to distinguish from
real change in review.

**Suggested direction.** Normalise the five files to tabs and drop the column alignment in both. This
is one of the things an `.editorconfig` would settle permanently; that file's absence is T6's record.

### CORE-028 -- Twelve handlers carry a redundant using for their own enclosing namespace, and four do not

```yaml
id: CORE-028
layer: CORE
severity: S4
category: style
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.Application/Features/Dungeons/Commands/AddArea/AddAreaCommandHandler.cs:3
  - src/DungeonMaster.Application/Features/Dungeons/Commands/AddFloor/AddFloorCommandHandler.cs:3
  - src/DungeonMaster.Application/Features/Dungeons/Commands/CreateDungeon/CreateDungeonCommandHandler.cs:3
  - src/DungeonMaster.Application/Features/Dungeons/Commands/DeleteDungeon/DeleteDungeonCommandHandler.cs:3
  - src/DungeonMaster.Application/Features/Dungeons/Commands/RemoveArea/RemoveAreaCommandHandler.cs:3
  - src/DungeonMaster.Application/Features/Dungeons/Commands/RemoveFloor/RemoveFloorCommandHandler.cs:3
  - src/DungeonMaster.Application/Features/Dungeons/Commands/ReorderFloor/ReorderFloorCommandHandler.cs:3
  - src/DungeonMaster.Application/Features/Dungeons/Commands/UpdateArea/UpdateAreaCommandHandler.cs:3
  - src/DungeonMaster.Application/Features/Dungeons/Commands/UpdateDungeon/UpdateDungeonCommandHandler.cs:3
  - src/DungeonMaster.Application/Features/Dungeons/Commands/UpdateFloor/UpdateFloorCommandHandler.cs:3
  - src/DungeonMaster.Application/Features/Dungeons/Queries/GetDungeonDetail/GetDungeonDetailQueryHandler.cs:2
  - src/DungeonMaster.Application/Features/Dungeons/Queries/GetDungeons/GetDungeonsQueryHandler.cs:2
effort: XS
risk: low
independent: true
```

**What.** Twelve handlers declare `using DungeonMaster.Application.Features.Dungeons;` even though
their own file-scoped namespace is nested beneath it, so the repository interfaces are already in
scope. The four object-related handlers (`AddPlacedObject`, `RemovePlacedObject`, `UpdatePlacedObject`,
`UpdateAreaOutline`) omit it and compile identically.

**Why it matters.** I believe this is dead syntax rather than a defect -- the build is clean, and no
analyzer is configured to flag it (T6 owns the analyzer record). The cost is that the two conventions
sit side by side in the same folder tree, so a reader adding a handler has no way to tell which form is
intended, and the redundant line implies a dependency that does not exist.

**Suggested direction.** Remove the twelve redundant directives so all sixteen handlers read the same
way.

**[AMENDED 6b-xvii, 2026-10-09 -- this entry is FALSIFIED. The directive it is about exists nowhere under
`src`, so neither the twelve nor the four is a real population and there is nothing to remove.
Classification: FALSIFIED, by a sibling remedy rather than by an error in the original reading. No code
changed.** The register row is `MASTER-113` at `00-master-index.md:301`, closed **FALSIFIED**. This block
follows the form of the `AMENDED 6b-xvi` block on `UI-003` at `03-app-shell-and-views.md:171-266`, cited
**for its form only** -- `AMENDED 6b-xiv` at `:1293` of this file records that the block that one was
modelled on *"carries three defects of its own"*. **Nothing below deletes any previous wording.**

*Correction 1 -- the `What.` paragraph, both halves.* Previous wording, verbatim: *"Twelve handlers declare
`using DungeonMaster.Application.Features.Dungeons;` even though their own file-scoped namespace is nested
beneath it, so the repository interfaces are already in scope. The four object-related handlers
(`AddPlacedObject`, `RemovePlacedObject`, `UpdatePlacedObject`, `UpdateAreaOutline`) omit it and compile
identically."* **Zero of the sixteen handlers carry that directive, and it appears nowhere under `src`.**
Three commands, all run against the post-change worktree:

1. **Anchored, per handler.**
   `grep -rln '^using DungeonMaster.Application.Features.Dungeons;$' src/DungeonMaster.Application/Features/Dungeons --include=*Handler.cs | wc -l`
   returns **0**. The denominator is **16**, from
   `find src/DungeonMaster.Application/Features/Dungeons -name '*Handler.cs'` with `/obj/` and `/bin/`
   pruned, piped to `wc -l` -- so the ratio is 0 of 16, and **the 16 is measured here rather than carried
   from this entry**, which is the one figure the entry happened to get right.
2. **Unanchored, repository-wide.**
   `grep -rn 'using DungeonMaster.Application.Features.Dungeons;' src --include=*.cs`, with `/bin/` and
   `/obj/` excluded, returns **0**. This exists to exclude the possibility that the `^...$` anchor is what
   missed, which the per-handler form alone could not rule out.
3. **POSITIVE CONTROL.** `grep -c '^using DungeonMaster.Application.Common.Dispatch;$'` on
   `src/DungeonMaster.Application/Features/Dungeons/Commands/AddArea/AddAreaCommandHandler.cs` returns
   **1**.

**The control is what makes the two zeros measurements rather than a blind predicate**, and it is the only
reason the falsification can be stated as a fact: the same anchored form, applied to the same files,
demonstrably sees a `using` directive that is present. Without it, `0` and `0` are indistinguishable from
a predicate reading nothing -- the failure mode this register keeps meeting, most recently where `grep -P`
errored in this locale **and printed `0` in the same breath**, and in `CORE-026`'s own corrected census,
where a character class that could not cross a nested tag returned a confident wrong total every time it
was run.

*The cause, read from the tree rather than inferred.* The repository interfaces now live at
`src/DungeonMaster.Application/Common/Interfaces/`, where `ls` returns exactly four entries --
`IDungeonReadRepository.cs`, `IDungeonWriteRepository.cs`, `IUnitOfWork.cs` and `IUnitOfWorkFactory.cs`.
**That move is `CORE-029`'s remedy**, the very next entry in this file, and it took the interfaces out of
`Features/Dungeons` along with the only reason the directive existed. The same move is what stranded two
of `CORE-024`'s citations; the two findings share one cause. **This entry was therefore resolved by a
sibling change before 6b-xvii reached it, and it closes on evidence with no code change** -- which also
means the `Suggested direction.` has no work left in it, rather than having work that was declined.

*What this entry got right, recorded so the falsification is not read as a blanket dismissal.* The
`Why it matters.` paragraph's belief that this is "dead syntax rather than a defect" is consistent with
everything measured: the solution builds with **`0 Warning(s)`** and **`0 Error(s)`**, read from those two
summary lines of `dotnet build src/DungeonMaster.sln` (and deliberately not from
`grep -icE "warning|error"`, which returns **2** on a clean build because the summary lines contain the
words). And the four-handler asymmetry it describes is the kind of thing worth filing even when the
specific directive has since gone: a reader adding a handler still has no in-file signal about which form
is intended.

**The `Why it matters.` and `Suggested direction.` paragraphs are left exactly as written**, including
`Why it matters.`'s parenthetical that T6 owns the analyzer record. **All twelve `locations:` citations are
stale**, for a reason stronger than drift: the lines they name do not hold the directive they were cited
for, in any of the twelve files. **No commit hash appears in this block and no placeholder for one** -- a
DIR task cannot invoke git -- **and none is owed, because no code changed for this entry.** The record's
severity, category, effort and risk are unchanged.**]**

### CORE-029 -- Repository interfaces sit under Features/Dungeons rather than the conventions' Common/Interfaces

```yaml
id: CORE-029
layer: CORE
severity: S4
category: structure
authority: conventions
citation: backend/backend-conventions.md#1. Project Structure
locations:
  - src/DungeonMaster.Application/Features/Dungeons/IDungeonReadRepository.cs:9
  - src/DungeonMaster.Application/Features/Dungeons/IDungeonWriteRepository.cs:8
effort: S
risk: low
independent: true
```

**What.** `IDungeonReadRepository` and `IDungeonWriteRepository` live at the root of
`Features/Dungeons/`. The conventions' Application layout places application service interfaces in
`Common/Interfaces/` and shows `Features/{Feature}/` containing only `Commands/`, `Queries/` and
`DTOs/`. There is no `Common/Interfaces/` directory in the project.

**Why it matters.** `backend-conventions.md` section 1 gives the layer layout explicitly, and this is a
departure from it. The cost is small and mostly navigational, but it is the kind of drift that makes
the documented structure stop being a reliable map of the code.

**Suggested direction.** Either move the two interfaces to `Common/Interfaces/`, or -- since
feature-owned repository contracts are a defensible choice that keeps the read and write models beside
the feature that defines them -- amend the conventions to permit feature-local interfaces and say why.
The deviation being undocumented is the part worth fixing, not necessarily the placement.

### CORE-030 -- Every collection property allocates a fresh ReadOnlyCollection wrapper on each access

```yaml
id: CORE-030
layer: CORE
severity: S4
category: performance
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.Domain/Entities/Dungeons/Dungeon.cs:48-49
  - src/DungeonMaster.Domain/Entities/Floors/Floor.cs:65-66
  - src/DungeonMaster.Domain/Entities/Floors/Floor.cs:71-72
  - src/DungeonMaster.Domain/Entities/Areas/Area.cs:60-61
  - src/DungeonMaster.Application/Common/Map/MapDrawingSession.cs:37-38
effort: XS
risk: low
independent: true
```

**What.** `Floors`, `Areas`, `Objects`, `Outline` and `Vertices` are all expression-bodied properties of
the form `_backing.AsReadOnly()`, so each read constructs a new `ReadOnlyCollection<T>` around the same
list rather than returning a cached wrapper.

**Why it matters.** I believe the allocation is measurable only because of who reads these properties:
the map renderer in the App layer walks floors, areas, outlines and objects on every draw pass, so a
per-frame allocation per collection per entity accumulates in gen-0. The nearest stated rule,
`dotnet-guidelines.md` section 4.2 "Avoid Frequent Large Object Allocations on Hot Paths", does not
reach this: these wrappers are small objects, not Large Object Heap allocations, and the section is
written about request handling rather than rendering. So I state the concern as a belief rather than a
rule violation.

**Suggested direction.** Cache the wrapper in a `readonly` field initialised beside the backing list
(`private readonly ReadOnlyCollection<Floor> _floorsView;`) and return it, or expose the property as
`IReadOnlyList<T>` returning `_floors` directly, which is already read-only through the interface.

**[AMENDED 6b-xvii, 2026-10-09 -- two of this entry's five `locations:` citations are stale by a constant
offset and a third is stale by project, and the remedy the `Suggested direction.` prescribes FAILS at one
of the five sites. Classification: DRIFT for the two `Floor.cs` ranges, a PROJECT MOVE for the fifth
citation, and an ERROR in the suggested remedy. The entry's substance stands and was acted on at all five
sites.** The register row is `MASTER-114` at `00-master-index.md:302`, closed **done**. This block follows
the form of the `AMENDED 6b-xvi` block on `UI-003` at `03-app-shell-and-views.md:171-266`, cited **for its
form only** -- `AMENDED 6b-xiv` at `:1293` of this file records that the block that one was modelled on
*"carries three defects of its own"*. **Nothing below deletes any previous wording.**

*Correction 1 -- the two `Floor.cs` ranges, which are DRIFT and not mis-identification.* Previous wording,
verbatim: `src/DungeonMaster.Domain/Entities/Floors/Floor.cs:65-66` and
`src/DungeonMaster.Domain/Entities/Floors/Floor.cs:71-72`. `measurement.md` section 3 measured the true
positions at the session base as **`:74-75`** and **`:80-81`**. **The arithmetic, stated so it can be rechecked:**
`74 - 65 = 9` and `80 - 71 = 9`, and each cited span's own length is right -- `65-66` is 2 lines and
`74-75` is 2 lines, `71-72` is 2 lines and `80-81` is 2 lines. **Equal-length spans displaced by ONE
SHARED CONSTANT is the signature of content inserted above the first citation after the entry was written,
not of a different defect being re-identified** -- nine lines of it -- and it is the same signal behind
`CORE-022`'s stale line counts, which `MASTER-110` carries. **This session did not locate the inserting
commit**, so the offset is attributed and its cause inferred from its shape rather than read from history.
**The pre-change halves of that arithmetic were NOT re-measured here**: establishing them needs git history
and a DIR task may not invoke git, so they are attributed to `measurement.md` section 3 and written out in
full for whoever can. The other two citations, `Dungeon.cs:48-49` and `Area.cs:60-61`, were **exact** at
the session base per the same section.

*Correction 2 -- the fifth citation, stale by PROJECT.* Previous wording, verbatim:
`src/DungeonMaster.Application/Common/Map/MapDrawingSession.cs:37-38`. **No such directory exists:** `ls`
on `src/DungeonMaster.Application/Common/Map/` reports `No such file or directory`, and
`find src -name 'MapDrawingSession.cs'` with `/obj/` and `/bin/` pruned returns **exactly one** path,
`src/DungeonMaster.App.Core/Map/MapDrawingSession.cs`. **That is outside the CORE layer this entry is
filed under**, so one of the five sites this CORE-layer entry covers is not a CORE-layer site at all. The
move is consistent with `CORE-020`, which records `Common/Map` as holding three geometry types with no
caller inside the Application layer.

*Correction 3 -- the `Suggested direction.`'s first option, which FAILS at `Area.Outline`.* Previous
wording, verbatim: *"Cache the wrapper in a `readonly` field initialised beside the backing list
(`private readonly ReadOnlyCollection<Floor> _floorsView;`) and return it."* **That form makes
`Area.Outline` read back EMPTY FOREVER.** EF Core installs the outline value converter's list onto
`_outline` **after** `private Area()` has run, so a wrapper built in the constructor wraps the
pre-materialisation list and never sees the materialised one. The remedy is correct at the other four
sites and was used there; `Area.Outline` uses a **lazy** field instead. The second option the paragraph
offers -- "expose the property as `IReadOnlyList<T>` returning `_floors` directly" -- is not what was
built either: all five properties already declared `IReadOnlyList<T>`, so returning the bare `List<T>`
would have handed out a mutable list behind a read-only interface. **This correction is recorded because
the remedy as written is wrong for one of the five sites the entry itself enumerates**, and a future
reader following it would reintroduce the bug.

*The five sites as they now stand, every line re-derived against the post-change worktree.* Located by
`grep -rn 'AsReadOnly()' src --include=*.cs` with `/obj/` and `/bin/` excluded, plus a `grep -n` on each
property's and each field's own declaration text:

| Property | Wrapper field | Built at | Property | Body |
|---|---|---|---|---|
| `Dungeon.Floors`, `Entities/Dungeons/Dungeon.cs` | `_floorsView` `:14` | `:17`, in `private Dungeon()` | `:56` | `:57` |
| `Floor.Areas`, `Entities/Floors/Floor.cs` | `_areasView` `:17` | `:22` | `:86` | `:87` |
| `Floor.Objects`, same file | `_objectsView` `:18` | `:23` | `:92` | `:93` |
| `MapDrawingSession.Vertices`, `App.Core/Map/MapDrawingSession.cs` | `_verticesView` `:43` | `:49` | `:54` | `:55` |
| `Area.Outline`, `Entities/Areas/Area.cs` | `_outlineView` `:25`, nullable | lazily, `:76` | `:75` | `:76` |

The four `readonly` sites return a bare field read, so reference equality across two reads is a language
guarantee visible in the declaration rather than a behaviour needing a test to establish; the regression
guard for all five is `src/DungeonMaster.Tests/Domain/CollectionWrapperCachingTests.cs`.

*Two residuals, recorded rather than closed.* The lazy `_outlineView ??= ...` form is **not thread-safe**,
so two concurrent first reads can each build a wrapper and break reference equality between them; nothing
in `src` reads `Outline` concurrently and the domain entities are not documented as thread-safe, so this
is a wording hazard rather than a live defect. And the lazy cache **goes permanently stale if `_outline` is
ever reassigned after a first read** -- demonstrated by reflection, not reasoned: swapping `_outline` for a
new list after a read left `Outline` returning the wrapper over the old one. It is unreachable today, since
`grep -rn '\.Reload()\|CurrentValues.SetValues'` over `src` returns no hits and
`grep -rnE '_(vertices|floors|areas|objects|outline)\s*=[^=]'` returns only the five field initialisers,
but no existing guard would see it if a future `Entry.Reload()` introduced it. **The invariant the comment
at `Area.cs` depends on is "`_outline` is assigned at most once per instance", and that is precisely what
such a call would break.** A read-mutate-reread assertion would pin it; there is none, and the gap is
carried to the backlog rather than claimed covered.

*One structural consequence worth recording, because it inverts an earlier finding.* `OutlineComparer`'s
role has flipped. Before this change, its absence caused **spurious writes**; after it, its absence
**silently drops outline edits**, because EF compares the same cached wrapper instance against itself and
sees no change. The comparer is therefore load-bearing in a new way and must not be removed as redundant.
It is declared at
`src/DungeonMaster.Infrastructure/Persistence/Configurations/AreaConfiguration.cs:37` and wired at `:110`,
both re-derived here by `grep -n 'OutlineComparer'` on that file. **Those two lines are stated because the
implementing task's own report named them as `:28` and `:101`, which is wrong against the post-change
tree** -- the same drift class this entry's own citations suffer, caught in a report this time rather than
in a register cell.

**The `Why it matters.` paragraph is left exactly as written**, including its explicit framing of the
allocation concern as a belief rather than a rule violation, which this session did not test and does not
endorse either way: no allocation measurement was taken, and the change was made for the shape rather than
for a measured frame cost. **No commit hash appears in this block and no placeholder for one:** a DIR task
cannot invoke git, so a hash here would be copied rather than verified. **The the session base positions quoted
above are historical and NOT navigable at `HEAD`** -- this session's own remedy added a wrapper field and
a constructor assignment to four of the five files, which displaces everything below each insertion -- and
all five `locations:` ranges in the YAML block are stale for that reason on top of the three defects
corrected above. The record's severity, category, effort and risk are unchanged.**]**

### CORE-031 -- The hand-rolled dispatcher is 233 lines replacing a mediator library and earns its keep

```yaml
id: CORE-031
layer: CORE
severity: S5
category: structure
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.Application/Common/Dispatch/Dispatcher.cs:9-69
  - src/DungeonMaster.Application/Common/Dispatch/IDispatcher.cs:8-46
  - src/DungeonMaster.Application/Common/Dispatch/ICommand.cs:9
  - src/DungeonMaster.Application/Common/Dispatch/IQuery.cs:9
effort: L
risk: high
independent: true
```

**What.** The `Common/Dispatch` folder is seven files and 233 lines: two marker interfaces, two handler
interfaces, a behavior interface, the dispatcher facade and its implementation. It provides
command/query separation, one open-generic behavior pipeline, and nothing else -- no notifications, no
streaming, no request pre/post processors, no ordering control beyond registration order.

**Why it matters.** I believe the hand-rolled dispatcher is currently carrying its weight, and it is
worth saying so explicitly rather than treating "no MediatR" as a defect. The interfaces are small and
correct, the behavior composition order is right, the `Result<T>` return type is baked into the command
contract in a way a general-purpose library would not give for free, and there is no licensing or
version-coupling exposure. Its real costs are already filed separately: the service-locator
registration surface (CORE-010), the uncached reflection and `dynamic` (CORE-011), and the missing
query-side pipeline (CORE-012). If any of those three grows -- particularly a second or third behavior
with ordering requirements -- the balance shifts.

**Suggested direction.** Keep it, and treat CORE-010 through CORE-012 as the maintenance price of
keeping it. Revisit only if the pipeline needs ordered behaviors, notifications, or query-side
cross-cutting concerns, at which point a library becomes cheaper than growing this one.

**[AMENDED 20261105-1907-core-heavy-rows, 2026-11-05 -- `MASTER-157` is CLOSED DELIBERATELY UNCHANGED by
user decision at Gate 1, and the size this entry's `What.` paragraph gives is stale. Classification:
DRIFT.** The register row is `MASTER-157` at `00-master-index.md:345`. **Nothing below deletes any previous
wording.** Every figure in this block is labelled either as measured by this session's task `T2` on the
session worktree, or as told, with its source. The contract files cited below are in
`.sir/sessions/20261105-1907-core-heavy-rows/contracts/`.

*Closure.* `MASTER-157` (`00-master-index.md:345`) is CLOSED **DELIBERATELY UNCHANGED** by user decision at
Gate 1: the user chose to keep the hand-rolled dispatcher, which is this entry's own `Suggested direction.`
("Keep it"). No code changed.

*The stale size.* Previous wording, verbatim, from `What.` (`:2447` of this file at this session's startRef):
"The `Common/Dispatch` folder is seven files and 233 lines". Measured by `T2`:
`wc -l src/DungeonMaster.Application/Common/Dispatch/*.cs` lists 8 files and 437 lines in total,
`Dispatcher.cs` 209; the eighth file is `IRequest.cs` (9 lines). Told, measured by the orchestrator in the
spec phase and recorded in `C4-register.md` section 1.3: `16d594b` ("docs: add code-quality audit for domain
and application", 2026-08-17) is the only commit `git log -S 'seven files and 233 lines'` prints over this
file, and at that commit `Common/Dispatch/` held seven files -- `Dispatcher.cs` 70, `ICommand.cs` 9,
`ICommandHandler.cs` 32, `IDispatcher.cs` 47, `IPipelineBehavior.cs` 36, `IQuery.cs` 9, `IQueryHandler.cs`
30 -- totalling 233, by `git show 16d594b:<path>` piped to `wc -l` per file; the same method at `62ba566`
prints 8 files and 437 lines. Classification **DRIFT**: exact at its commit, moved since.

*The three costs.* `Why it matters.` names them as `CORE-010`, `CORE-011` and `CORE-012`. Their register
rows, `MASTER-089` (`00-master-index.md:277`), `MASTER-090` (`:278`) and `MASTER-080` (`:268`), each read
**done**: that is what the register says (told), and `T2` verified each in code rather than take it from
there. Paths below are under `src/DungeonMaster.Application/`.

*`CORE-011`, uncached reflection and `dynamic`.* DYN (`C1-measurement.md` section 4) prints nothing, its
control re-run first. `Common/Dispatch/Dispatcher.cs:18` declares
`private static readonly ConcurrentDictionary<Type, object> Invokers`. `grep -n 'Invokers.GetOrAdd'` on
`Dispatcher.cs` prints `:31`, `:48` and `:67` (the two `SendAsync` overloads and `QueryAsync`), and
`grep -n MakeGenericType` on it prints `:35`, `:52` and `:71`, each inside the `static` value factory of the
`GetOrAdd` above it: the closed invoker type is built once per request type and cached.

*`CORE-010`, the hand-maintained registration list.* `grep -n 'AddScoped<' DependencyInjection.cs` prints
one line, `:27`, `services.AddScoped<IDispatcher, Dispatcher>();`: no handler is registered by name.
`grep -n ScannedTypes DependencyInjection.cs` prints `:29` (the registration loop), `:99` (the self-check
loop) and `:137` (the property, an assembly scan of the Application assembly). `ValidateHandlerRegistrations`
(`DependencyInjection.cs:93`) asserts exactly one marker and exactly one handler per request type; it is
exercised by `ApplicationServiceRegistrationTests.cs` (`:29`, `:46`) and is not called at app startup:
`grep -rn ValidateHandlerRegistrations src/DungeonMaster.App --include=*.cs --exclude-dir=obj
--exclude-dir=bin`, run from the worktree root, prints nothing.

*`CORE-012`, queries bypassing the pipeline.* `Common/Dispatch/IQuery.cs:9-10` declares
`public interface IQuery<TResult>` `: IRequest<TResult>`. `grep -n 'BuildPipeline<'` on `Dispatcher.cs`
prints `:84` (the definition) and `:133`, `:166`, `:199`; `:199` is inside `TypedQueryInvoker`, so a query
passes through the same `IPipelineBehavior` composition as a command. No behavior is registered for
queries, by design: `ValidationBehavior` is registered in the two command branches
(`DependencyInjection.cs:48-54`, whose comment at `:48-49` reads "so that no query can ever acquire a
validation behavior", and `:64-70`). The pipeline is reachable by queries and empty for them.

*The residue.* The grep of `C4-register.md` section 1.2 D, run by `T2` on `Dispatcher.cs`, prints `:92`,
`:131`, `:164` and `:197`: handlers and behaviors are still resolved from `IServiceProvider` at dispatch
time. That is the service-location shape `MASTER-089`'s title names. Its hand-maintained-list half is gone;
its service-location half remains, by design, inherent in the dispatcher Gate 1 kept. The three costs are
therefore not all fully gone, and this block does not claim they are.

This entry's `What.`, `Why it matters.` and `Suggested direction.` paragraphs are left exactly as written,
and its `locations:` list is kept unchanged as a record of the audited state; this block does not
re-derive those lines. **No hash for this session's commit appears in this block** (the two hashes above
are the orchestrator's, told): this session's tasks run without git and the orchestrator makes the
commit; no code changed for this row in any case. The record's severity,
category, effort and risk are unchanged.**]**

### CORE-032 -- Both query handlers wrap a single passthrough call in an async state machine

```yaml
id: CORE-032
layer: CORE
severity: S5
category: async
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.Application/Features/Dungeons/Queries/GetDungeons/GetDungeonsQueryHandler.cs:14-17
  - src/DungeonMaster.Application/Features/Dungeons/Queries/GetDungeonDetail/GetDungeonDetailQueryHandler.cs:14-19
effort: XS
risk: low
independent: true
```

**What.** Both query handlers are expression-bodied `async` methods whose entire body is
`await readRepository.Get...Async(...)` -- the awaited task is returned unchanged, with no work before
or after the await.

**Why it matters.** I believe returning the task directly would remove the state machine and one
allocation per query with no behavioural difference here, since neither method holds a `using`, a
`try`, or anything else that depends on the continuation running inside the method. I also believe the
current form is defensible: eliding `async` moves exceptions from the await point to the call point and
makes stack traces harder to read, which is why many teams keep the `await` deliberately. This is a
note, not a deviation -- no authority I read prescribes either form.

**Suggested direction.** If the elision is wanted, drop `async`/`await` and return the task; otherwise
leave it and be consistent, since the command handlers all have genuine post-await work and must stay
`async` regardless.

### CORE-033 -- The only persistence-shaped concession in the Domain is the private parameterless constructors

```yaml
id: CORE-033
layer: CORE
severity: S5
category: boundary
authority: judgment
citation: n/a
locations:
  - src/DungeonMaster.Domain/Entities/Areas/Area.cs:12
  - src/DungeonMaster.Domain/Entities/Dungeons/Dungeon.cs:10
  - src/DungeonMaster.Domain/Entities/Floors/Floor.cs:13
  - src/DungeonMaster.Domain/Entities/Floors/PlacedObject.cs:10
  - src/DungeonMaster.Domain/Entities/Areas/Area.cs:45
  - src/DungeonMaster.Domain/Entities/Floors/PlacedObject.cs:53
effort: XS
risk: low
independent: true
```

**What.** Each of the four entities declares an unused `private Entity() { }` constructor, and two
non-nullable reference properties are initialised with `= default!` (`Area.Name`,
`PlacedObject.Position`). Nothing else in the 20 Domain files references a framework type beyond the
BCL: no attributes, no `DbContext`, no `virtual` navigation properties, no interfaces borrowed from
Application, and the `.csproj` has zero package and zero project references.

**Why it matters.** I checked domain purity specifically and believe it holds. These six lines exist to
let an ORM materialise the entities, which is an inward-facing accommodation rather than a leak -- the
Domain names nothing about persistence and would compile unchanged if the Infrastructure project were
deleted. I record it so the concession is visible rather than accidental, and because `= default!`
silences the nullable analyser at exactly the two places where a materialisation bug would be hardest
to see. The model is behaviour-rich, not anaemic: the aggregate owns renumbering, outline replacement
and scale bounds rather than exposing setters.

**Suggested direction.** Nothing required. If the concession is ever worth removing, EF Core can
construct via the full constructor for these shapes; a one-line comment on each private constructor
naming its purpose would be cheaper and would stop a future reader deleting them as dead code.

**[AMENDED 6b-xvii, 2026-10-09 -- two of this entry's figures are wrong and one of its two `= default!`
citations is stale, but its CONCLUSION -- that domain purity holds -- is CORRECT and is corroborated by
the conventions. Classification: ERROR for both counts, DRIFT for the citation; the entry's substance and
its verdict both stand. No code changed.** The register row is `MASTER-146` at `00-master-index.md:334`,
closed **done** as an evidence-only close. This block follows the form of the `AMENDED 6b-xvi` block on
`UI-003` at `03-app-shell-and-views.md:171-266`, cited **for its form only** -- `AMENDED 6b-xiv` at
`:1293` of this file records that the block that one was modelled on *"carries three defects of its own"*.
**Nothing below deletes any previous wording.**

*Correction 1 -- there are THREE `= default!` properties, not two.* Previous wording, verbatim: *"and two
non-nullable reference properties are initialised with `= default!` (`Area.Name`, `PlacedObject.Position`)"*,
with the same figure implied by `Why it matters.`'s *"`= default!` silences the nullable analyser at
exactly the two places where a materialisation bug would be hardest to see"*.
`grep -rn '= default!' src/DungeonMaster.Domain --include=*.cs` returns **three** lines, every one
re-derived against the post-change worktree: `Area.Name` at
`src/DungeonMaster.Domain/Entities/Areas/Area.cs:60`, **`Dungeon.Name`** at
`src/DungeonMaster.Domain/Entities/Dungeons/Dungeon.cs:41`, and `PlacedObject.Position` at
`src/DungeonMaster.Domain/Entities/Floors/PlacedObject.cs:53`. **`Dungeon.Name` is not cited anywhere in
this entry**; `measurement.md` section 3 puts it at `Dungeon.cs:33` at the session base. The count 6 in
`Why it matters.`'s *"These six lines"* is 4 constructors + 2 properties and becomes **7** under the
corrected figure.

*Correction 2 -- one of the two cited `= default!` lines was stale, and it is now accidentally RIGHT,
which is why it needs saying out loud.* Previous wording, verbatim:
`src/DungeonMaster.Domain/Entities/Floors/PlacedObject.cs:53`. `measurement.md` section 3 measured the true
line at the session base as **`:47`**, so the citation was wrong by 6 when written. **`CORE-026`'s remedy in this
same session then expanded that file's doc comments and pushed the property down to `:53`** -- the exact
line the entry had guessed. A reader checking the entry against `HEAD` would therefore find the citation
correct and conclude it always had been. The other cited line drifted the opposite way in the same
session: `Area.cs:45` is now `:60`, and the uncited `Dungeon.cs:33` is now `:41`, both displaced by
`CORE-030`'s added wrapper fields and constructor bodies. **None of the three session-base figures is
navigable at `HEAD`**, and the agreement at `:53` is coincidence rather than correctness.

*Correction 3 -- the Domain holds 21 `.cs` files, not 20.* Previous wording, verbatim: *"Nothing else in
the 20 Domain files references a framework type beyond the BCL"*, with the same figure in the
`Layer verdict` section's *"on reading all twenty files"*.
`find src/DungeonMaster.Domain -name '*.cs'` with `/obj/` and `/bin/` pruned, piped to `wc -l`, returns
**21**.

*The conclusion is CORRECT, and neither corrected figure disturbs it.* Three `= default!` properties across
21 files is the same kind of fact as two across 20. The four private parameterless constructors remain the
only other concession and are confirmed by
`grep -rnE 'private (Area|Dungeon|Floor|PlacedObject)\(\)'`: `Area.cs:27`, `Dungeon.cs:16`, `Floor.cs:20`,
`PlacedObject.cs:10`. **The conventions corroborate the verdict directly rather than merely permitting
it**, which this entry does not note: the canonical entity example in `dm-conventions/backend/dotnet-format.md`
contains **both** concessions within fifteen lines of each other -- `private Dungeon() { }` at **`:574`**
and `public string Name { get; private set; } = default!;` at **`:584`** -- both read directly from that
file. So the shape this entry records as a concession is the shape the conventions prescribe, and the
`Suggested direction.`'s "Nothing required" is the right call for a stronger reason than the entry gives.

*One divergence from that canonical example, new in this session and measured rather than assumed.*
`Dungeon.cs:16` now reads `private Dungeon() =>` rather than the empty `private Dungeon() { }` of `:574`,
because `CORE-030`'s remedy gave that constructor a body: it assigns the cached `_floorsView` wrapper. The
constructor is still private and still parameterless, so the concession is unchanged **in kind**, but it is
no longer empty and no longer byte-matches the example -- which matters for the `Suggested direction.`'s
closing idea of "a one-line comment on each private constructor naming its purpose", since one of the four
now has a purpose beyond materialisation.

**The `Why it matters.` and `Suggested direction.` paragraphs are left exactly as written** apart from the
figures corrected above, which are measurements rather than records of how the code then stood. **No
commit hash appears in this block and no placeholder for one** -- a DIR task cannot invoke git -- **and
none is owed, because no code changed for this entry.** **FOUR of the six `locations:` citations are stale
at `HEAD` and TWO are correct there.** The split is enumerated rather than totalled, which is the form
`MASTER-180` and `CORE-036` use and for this same reason: the universal this block first carried -- *"All
six `locations:` citations in the YAML block are stale at `HEAD`"* -- was false for the two that happen to
be right, and it contradicted the grep printed four paragraphs above it and the whole of Correction 2. The
two locating commands are the two already in this block, both re-run against the post-change worktree:
`grep -rnE 'private (Area|Dungeon|Floor|PlacedObject)\(\)' src/DungeonMaster.Domain --include=*.cs` for the
four private constructors and `grep -rn '= default!' src/DungeonMaster.Domain --include=*.cs` for the
properties. **Stale -- cited line, then the line the command returns at `HEAD`:** `Area.cs:12` is now
`:27`; `Dungeon.cs:10` is now `:16`; `Floor.cs:13` is now `:20`; `Area.cs:45` is now `:60`. **Correct at
`HEAD`:** `PlacedObject.cs:10`, which is exactly where the first command puts `private PlacedObject() { }`,
and `PlacedObject.cs:53`, which is exactly where the second puts `PlacedObject.Position`. The second of
those two is correct **by coincidence** and not because the citation was right when written, which is the
point Correction 2 above establishes and the reason the two cannot simply be folded into the stale four.
The record's severity, category, effort and risk are unchanged.**]**

### CORE-034 -- default(MutationOutcome) was Success, and ToResult's `_ =>` arm passed it through instead of catching it

```yaml
id: CORE-034
layer: CORE
severity: S2
category: correctness
authority: microsoft
citation: https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/enum
locations:
  - src/DungeonMaster.Domain/Entities/MutationOutcome.cs:6-28
  - src/DungeonMaster.Application/Common/Handlers/MutationOutcomeExtensions.cs:61-87
effort: S
risk: low
independent: true
```

**What.** `MutationOutcome` declared `{ Success, NotFound, OutOfRange }` in that order with no explicit
values, so `default(MutationOutcome)` was `MutationOutcome.Success`. A mutation nobody performed reported
that it worked.

**Why it matters.** The guard that exists for this enum could not catch it. `MutationOutcomeExtensions.ToResult`
switches on the outcome and closes with a `_ =>` arm that throws `ArgumentOutOfRangeException`. That arm
catches an undefined value such as `(MutationOutcome)99`, and it **could not catch the CLR default, because
the CLR default IS a recognised member** -- `Success` -- so the default fell into the `MutationOutcome.Success`
arm and was mapped straight to `Result.Success()`. The failure was reported to the Application layer as a
success, and the two mechanisms that might have observed it did not: the `_ =>` arm at
`MutationOutcomeExtensions.cs:61-87` could not fire on a recognised member, and `CA1008` does not fire at this
solution's `AnalysisMode=Recommended` (`Directory.Build.props:15`, measured by build probe in session 6b-viii).
No claim is made here about what the test suite did or did not cover before this session.

**This is the same defect class as `MASTER-073`, with the mechanism inverted, and the inversion is the point.**
`MASTER-073`'s zero value (`ObjectKind.DoorWood`) was **absorbed** by a `_ =>` arm -- the arm fired, and the
wrong value was stopped by accident. This one is **passed through** by a `_ =>` arm -- the arm never fires,
because a named arm above it claims the value first. A reviewer who has internalised `MASTER-073` will look for
a catch-all arm, find one, and conclude the enum is guarded. It is not. The presence of a `_ =>` arm is
evidence about undefined values only, and says nothing about the zero member.

**The defect was latent, and the search that established that is a weak instrument.** No
`default(MutationOutcome)`, `= default`, `new MutationOutcome[]`, or uninitialised field or property of that
type was found; every production site returns an explicit member. That figure and its command are recorded in
`.dir-sdlc/sessions/2026-09-17-enum-zero-values/measurement.md` §2, which also states the limit of the
instrument: it matches source patterns only and cannot see reachability through a generic `default(T)`, a
`FirstOrDefault`, a struct field, a mocking framework's unconfigured return, or a persistence round-trip.
Where it is silent, that silence is not evidence. `MASTER-073` was latent on the same terms and was closed
anyway.

**Resolved in session 6b-ix.** `None` was added at zero and `ToResult` rejects it through its **own** switch
arm, separate from the arm rejecting undefined values, so that either rejection can be removed without
disabling the other. The rejection is member-specific rather than an `Enum.IsDefined` check, because
`Enum.IsDefined(MutationOutcome.None)` is `true` and a definedness check would admit it. The XML `<exception>`
wording on both overloads -- **two** `ToResult` declarations and no more, by
`grep -n "public static Result ToResult(" src/DungeonMaster.Application/Common/Handlers/MutationOutcomeExtensions.cs`,
which returns lines `34` and `61` (the third `ToResult` hit in that file, at `:37`, is a call site, not a
declaration) -- previously read "not a recognised `MutationOutcome` member", which became false for
the `None` case once `None` was declared, and was corrected on both.

### CORE-035 -- Triage of the fourteen production enums with a concrete member at zero

```yaml
id: CORE-035
layer: CORE
severity: S4
category: api-design
authority: microsoft
citation: https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/enum
locations:
  - src/DungeonMaster.Domain/Entities/MutationOutcome.cs:6-28
  - src/DungeonMaster.App.Core/Map/MapDrawingSession.cs:8-24
  - src/DungeonMaster.Domain/ValueObjects/PathSegment.cs:35
  - src/DungeonMaster.App/Assets/ObjectAssets/ObjectKindInfo.cs:24-28
  - src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:140
  - src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:151
  - src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:167
  - src/DungeonMaster.Application/Common/Results/Result.cs:31
  - src/DungeonMaster.Application/Common/Results/ResultOfT.cs:39
effort: M
risk: medium
independent: true
```

**What.** `CORE-017` and `CORE-034` each filed one enum. This entry records the census that asked how many
others there are, and triages the answer, so that the twelve this session does not change -- the fourteen in
the table below less its two class A rows, 14 - 2 = 12 (table census, defined under that table) -- are visible
as decisions rather than as oversights.

**The figure is 14 of 17 as the tree stood at session start, before this session's own two changes**, measured
in `.dir-sdlc/sessions/2026-09-17-enum-zero-values/measurement.md` §1 by a
Python walk of `src/` that strips block and line comments from each file before parsing: **17 production enum
declarations** (18 under `src/`, of which one, `AsyncVoidUnitKind`, is in the test project), of which
**14 have a concrete member at zero** and 3 have `None` at zero.

**After this session's two class A changes the same census returns `12 of 17`**, with **five** enums carrying
`None` at zero -- `MutationOutcome`, `DrawSessionResult`, `ObjectKind`, `ObjectRotation`, `FloorDropOwner`.
That second figure is not the session-start measurement above; it is a **re-run of the same census on the
post-change worktree**, performed after this entry's triage was written, and it moves because `MutationOutcome`
and `DrawSessionResult` left the concrete-at-zero bucket when they gained `None`. Predicate check on the re-run:
`MutationOutcome` must now fall in the `None` bucket and does, `PathSegmentKind` must still fall in the
concrete bucket (`Line`) and does. **A later reader re-running the census on current `main` should expect 12,
not 14** -- both figures are correct, of different trees, and the triage table below is the session-start
fourteen.

**The inherited figure of "14 of 16" was wrong in its denominator and is corrected here.** It reconciles
exactly: `ObjectKind.None` was added by 6b-viii, moving `ObjectKind` out of the concrete-at-zero bucket, so the
census that produced `14 of 16` was taken over a tree where the true figure was `15 of 17` -- derived, not
measured: the measured `14 of 17` plus the one enum 6b-viii moved out of the bucket, 14 + 1 = 15 over the same
17 declarations. That census therefore missed one concrete-at-zero enum, and which one is not recoverable from
the claim. The figure to use for the as-found tree is
`14 of 17`. The correction is recorded rather than quietly applied because "14 of 16" was an **uncited count**
that survived into a session artifact and had to be found by re-measurement; measurement.md §1 also records
that the first run of the corrected census was itself wrong and was discarded, having split enum bodies on `,`
before stripping doc comments, which corrupted every enum whose `<summary>` contains a comma.

**The fourteen.** The Microsoft guideline admits two compliant shapes -- `None = 0`, or the *most common
default value* at zero -- and most of these sit in the second branch legitimately. Class A is measured; class
B is a design question; class C is a judgment about intent, qualified below.

| Enum | Zero member | Class |
|---|---|---|
| `MutationOutcome` | `Success` | A -- the zero value asserts a consequential outcome |
| `DrawSessionResult` | `Added` | A |
| `RotationDirection` | `Clockwise` | B -- the zero value silently picks one of several equally valid options |
| `ScaleDirection` | `Up` | B |
| `PathSegmentKind` | `Line` | B |
| `SnapAnchor` | `Center` | C -- **reclassified from B by this entry**; see below |
| `MapTool` | `Draw` | C -- most common default, compliant under the guideline's second branch |
| `DrawState` | `Idle` | C |
| `NotebookKind` | `Dungeon` | C |
| `NotebookPane` | `Description` | C |
| `CanvasCursorShape` | `Arrow` | C |
| `BehaviorCursorShape` | `Hand` | C |
| `FloorListBandKind` | `FloorCard` | C |
| `ErrorCategory` | `Validation` | C |

**Every class count in the rest of this entry is read off the table above, by this command.** Run in the
repository root, over the line range the table occupies in this file
(`grep -n '^| \`MutationOutcome\`\|^| \`ErrorCategory\`' docs/audit/01-domain-application.md` gives the range):

```
sed -n '<first>,<last>p' docs/audit/01-domain-application.md | wc -l          # 14 rows
sed -n '<first>,<last>p' docs/audit/01-domain-application.md | grep -c '| A'  # 2
sed -n '<first>,<last>p' docs/audit/01-domain-application.md | grep -c '| B'  # 3
sed -n '<first>,<last>p' docs/audit/01-domain-application.md | grep -c '| C'  # 9
```

The `| ` prefix in the pattern is load-bearing and the predicate was checked both ways before the totals were
used: the `SnapAnchor` row must **not** match `| B` even though its text reads "reclassified from B", and it
must match `| C`; `grep '| B'` over the range prints exactly `RotationDirection`, `ScaleDirection` and
`PathSegmentKind`, and 2 + 3 + 9 = 14 reconciles with the row count. **Every count below cites this block as
`(table census)`.**

**`SnapAnchor` is reclassified from class B to class C on repository evidence.** `ObjectKindInfo.GetDefaultAnchor`
(`src/DungeonMaster.App/Assets/ObjectAssets/ObjectKindInfo.cs:24-28`) tests for wall objects and wall-adjacent
objects and then returns `SnapAnchor.Center` as its **terminal fallback**. `Center` is therefore the repository's
own most-common-default for this enum, stated in code, which places it inside the guideline's second branch
rather than in the "silently picks one of several equally valid options" shape it was first read as.

**`PathSegmentKind` is blocked, and the blocker is a data migration rather than a renumbering.** Here, inserting
a zero member is not free. (No claim is made about the remaining thirteen rows: each one's freedom to renumber
is a property of its own persistence mapping, and only `PathSegmentKind` and `ObjectKind` were checked.)

- `AreaConfiguration` builds its serializer options as, verbatim from
  `src/DungeonMaster.Infrastructure/Persistence/Configurations/AreaConfiguration.cs:16`:

  ```csharp
  private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.General);
  ```

  The `new` is target-typed, so the constructed type is `JsonSerializerOptions` and the single argument is
  `JsonSerializerDefaults.General` -- no converter is supplied.
- `grep -rn "JsonStringEnumConverter\|JsonConverter" --include=*.cs src/` returns **zero hits** across the whole
  of `src/`, so no converter is registered anywhere else either.
- `System.Text.Json` therefore writes the enum as its **ordinal** into the `Areas.Outline` TEXT column.
- `PathSegmentJsonRoundTripTests.cs:26-28` pins that persisted form: `"Kind": 0` deserialises to `Line` and
  `"Kind": 1` to `CubicBezier`.

Inserting `None = 0` would reinterpret every already-persisted outline -- every stored `Line` would read back as
`None`, every `CubicBezier` as `Line`. That is a **data migration on a JSON column, not a renumbering**, and it
is out of scope for this session.

**For `RotationDirection` and `ScaleDirection`, adding a zero member does not on its own make a leaked default
detectable.** Both enums are consumed by boolean comparison, not by a switch. Measured:
`grep -rn "RotationDirection\." --include=*.cs src/` returns **4 lines** and
`grep -rn "ScaleDirection\." --include=*.cs src/` returns **3 lines**; of those seven, four are *producers* that
raise an event with a member (`ControlsPaletteView.xaml.cs:14`, `:15`, `:16`, `:17`) and the remaining **three
are the only consuming reads**, all of them `==` comparisons:

- `_editor.GhostRotation = dir == RotationDirection.Clockwise` (`DungeonDetailPage.xaml.cs:140`)
- `await RotateObjectAsync(dir == RotationDirection.Clockwise);` (`:151`)
- `var down = dir == ScaleDirection.Down;` (`:167`)

No `switch` on either type appears in those seven lines.

A new `None` member would compare unequal at each site, so the **else-branch would absorb it and the operation
would still be performed, in the opposite direction** -- a leaked default would rotate counter-clockwise or
scale up rather than being rejected. Adding the member without also changing these three sites buys nothing,
which is why these two are recorded as a design question rather than scheduled as a fix.

**Class C is a judgment about intent supported by usage, not a measurement** -- with one exception.
**`ErrorCategory` carries measured evidence**: it is stored as `ErrorCategory?` at
`src/DungeonMaster.Application/Common/Results/Result.cs:31` and
`src/DungeonMaster.Application/Common/Results/ResultOfT.cs:39`, so absence of a category is already modelled by
`null` rather than by a zero member, and the zero value is not reachable as "no category supplied". The other
**eight** class C entries (nine class C rows less `ErrorCategory`; table census) rest on reading how each enum
is used and could be argued with.

**`CA1008` will not flag anything this triage leaves unchanged**, which is the only analyser measured here; no
claim is made about analysers not enabled in this solution. `CA1008` (*Enums should have zero value*) does
**not** fire at this solution's `AnalysisMode=Recommended`, measured by build probe in session 6b-viii; the two
settings that probe rests on are at `Directory.Build.props:15` (`<AnalysisMode>Recommended</AnalysisMode>`) and
`Directory.Build.props:5` (`<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`), both read by
`grep -n "AnalysisMode\|TreatWarningsAsErrors" Directory.Build.props`. The **twelve** enums left alone here --
the fourteen rows of the table above less its two class A rows, 14 - 2 = 12 (table census) -- are therefore
left alone silently: no `CA1008` warning is emitted for them whether or not they are ever changed, so this
record is the only place their status is visible.

**Suggested direction.** Nothing is required for class C. **Class B is three enums after the `SnapAnchor`
reclassification above** (table census), and three separate design decisions; of those three, **one**
(`PathSegmentKind`) is blocked above, leaving **two** -- 3 - 1 = 2, the class B count from the table census
less the single blocked row named in this entry -- and neither of those two has a named defect the way `MutationOutcome` did. `RotationDirection` and `ScaleDirection`
need their three comparison sites reconsidered before a zero member is worth adding, and `PathSegmentKind`
needs a persistence decision first. (`SnapAnchor`, now class C, is no longer in this list.) The cheapest
durable fix for `PathSegmentKind` is to register a `JsonStringEnumConverter` and migrate the stored outlines to names, after
which its ordinals stop being load-bearing and a zero member becomes free; that is a session of its own.

**[AMENDED 20261105-1907-core-heavy-rows, 2026-11-05 -- `MASTER-178` is CLOSED DELIBERATELY UNCHANGED: each of
the twelve enums this entry leaves with a concrete member at zero now has a recorded disposition, and none is
owed a change.** The register row is `MASTER-178` at `00-master-index.md:366`.
**Nothing below deletes any previous wording.** Every figure in this block is labelled either as measured by
this session's task `T3` on the session worktree, or as told, with its source. The contract files cited below are in
`.sir/sessions/20261105-1907-core-heavy-rows/contracts/`.

*Closure.* `MASTER-178` (`00-master-index.md:366`) is CLOSED **DELIBERATELY UNCHANGED** by this session. The
rule applied is Gate 1's (told, `C3-enum-census.md` section 4): where `default(T)` is actually reachable
**and** is a defect, add a `None` member, with a data migration if the enum is persisted; otherwise close the
enum deliberately unchanged with a stated reason. No enum of the twelve meets both conditions, so no `None`
member and no data migration was added, and no code changed.

*The validated census.* The instrument is the Python census in `C3-enum-census.md` section 1, recreated by
`T3` byte for byte as `census.py`; `sha256sum census.py` printed
`2a10830e5ac1569f5f542c99ae17802a805290e4f4e1e1fae5500d3aadb5b05a`, the digest that section states. It walks
`src/`, skips `obj/`, `bin/` and the four test projects, strips comments and string literals from each `.cs`
file, honours explicit `= n`, and reports the member at zero of each enum. Its control ran first, on the
fixture of `C3-enum-census.md` section 2 (the sha256 of both fixture files matched that section):
`python -I census.py <fixture>` printed these six lines (tab-separated; tabs shown here as two spaces), whose
CR-stripped sha256 is `db60df1b3f0d35513572960c642ee4a555a21ac2c7becad99f20586059864758`:

```
Explicit  zero=None  explicit=True  src/Prod/F.cs
ExplicitZeroLater  zero=B  explicit=True  src/Prod/F.cs
Hidden  zero=First  explicit=True  src/Prod/F.cs
MustFlag  zero=Alpha  explicit=False  src/Prod/F.cs
MustNotFlag  zero=None  explicit=False  src/Prod/F.cs
TOTAL 5 NONE_AT_ZERO 1 CONCRETE_AT_ZERO 3 NO_ZERO 1
```

On the session worktree, `python -I census.py C:/dm-wt/20261105-1907-core-heavy-rows` prints 17 enum rows
ending `TOTAL 17 NONE_AT_ZERO 5 CONCRETE_AT_ZERO 12 NO_ZERO 0`, CR-stripped sha256
`8d1d455164802c1cc7495043f0983bf0b7c00de2e4d7734d03a5b9a26470d929`, the output `C3-enum-census.md` section 3
records. The five with `None` at zero are `DrawSessionResult`, `FloorDropOwner`, `MutationOutcome`,
`ObjectKind` and `ObjectRotation`; the twelve with a concrete member at zero are the twelve rows of the table
below. The second predicate, the line-based `grep` of `C3-enum-census.md` section 3, which cannot share the
parser's mistakes, prints **17** lines, one per enum the census names, the nested `private enum NotebookPane`
included. Its exclusion control (the same pipeline with `grep -v` replaced by `grep`) prints exactly one line,
`src/DungeonMaster.Tests/App/AsyncDiscipline/AsyncDisciplineScanner.cs:288:public enum AsyncVoidUnitKind`, so
the test-project exclusion fires and removes only that line. The two counts agree. **Reading the census
rows:** the script prints Python's `None` for an enum with no member at zero, so a row reading `zero=None` is
ambiguous between "a member named `None` sits at zero" and "no member is zero" -- the fixture's `Explicit` row
is the second case. The TOTAL line is not ambiguous (it compares against the string `'None'`), and on the
worktree it prints `NO_ZERO 0`, so every `zero=None` row there is a member named `None`. The 12 of 17 agrees
with the post-change figure this entry and `MASTER-178` already record for 6b-ix.

*Dispositions.* One row per enum the census prints with a concrete member at zero, and none for the five with
`None` at zero. Every citation was re-read by `T3` on the session worktree; the reasons are those of
`C3-enum-census.md` section 4 (told), checked against that re-read. File names are short; their paths under
`src/` are listed after the table.

| # | Enum | Zero member | Disposition | Reason and evidence |
| --- | --- | --- | --- | --- |
| 1 | `PathSegmentKind` | `Line` | deliberately unchanged | `PathSegment` is a `record` class (`PathSegment.cs:28`) whose redeclared `Kind` initializer (`:38`) validates every value in `ValidateControlPoints` (`:43-88`), whose final `else` arm throws `ArgumentOutOfRangeException` for an undefined kind. It is persisted by ordinal in the `Areas.Outline` JSON column (`AreaConfiguration.cs:16`, `new(JsonSerializerDefaults.General)`); JC (`C1-measurement.md` section 3) prints nothing, its control re-run first. `PathSegmentJsonRoundTripTests.cs:28-29` are the two raw-JSON lines pinning `"Kind": 0` as `Line` and `"Kind": 1` as `CubicBezier`. `Line` is the most common segment and a valid one, so a `0` read from storage is correct data, not a leaked default; adding `None = 0` would reinterpret every stored outline (a JSON data migration) to fix no defect. |
| 2 | `RotationDirection` | `Clockwise` | deliberately unchanged | Raised only at `ControlsPaletteView.xaml.cs:27` (`CounterClockwise`) and `:28` (`Clockwise`), both explicit; consumed by `==` at `DungeonDetailPage.xaml.cs:139` and `:150`. No field or property stores the type: `grep -rn RotationDirection` over production `src/` prints only the event declaration (`ControlsPaletteView.xaml.cs:14`), those four lines and the enum's own declaration. `default` is never produced; every value comes from an explicit producer. |
| 3 | `ScaleDirection` | `Up` | deliberately unchanged | Raised only at `ControlsPaletteView.xaml.cs:29` (`Down`) and `:30` (`Up`); consumed by `==` at `DungeonDetailPage.xaml.cs:166`. No stored field: the same grep for `ScaleDirection` prints only the event declaration (`ControlsPaletteView.xaml.cs:19`), those three lines and the enum's own declaration. Unchanged for the same reason as row 2. |
| 4 | `SnapAnchor` | `Center` | deliberately unchanged | The only production producer is `ObjectKindInfo.GetDefaultAnchor` (`ObjectKindInfo.cs:40`), whose every path returns an explicit member (`:52` `East`, `:56` `South`, `:58` `Center` as the terminal fallback). `grep -rn 'GetDefaultAnchor(' src --include=*.cs --exclude-dir=obj --exclude-dir=bin` prints that declaration and three lines of `DungeonMaster.App.UnitTests/Assets/ObjectAssets/ObjectKindInfoTests.cs` (`:108`, `:116`, `:173`), and nothing else: it has no production caller. `Center` is the repository's own stated fallback (this entry's class C reclassification) and no `default` is produced. |
| 5 | `MapTool` | `Draw` | deliberately unchanged | The only stored `MapTool` is `MapCanvasViewModel.ActiveTool`, initialised `= MapTool.Pan` (`MapCanvasViewModel.cs:47`); `MapEditorState.SetActiveTool` takes it as a parameter (`MapEditorState.cs:148`) and stores none. The default is never observed: the one stored field is initialised explicitly. |
| 6 | `DrawState` | `Idle` | deliberately unchanged | `public DrawState DrawState { get; private set; }` has no initializer (`MapEditorState.cs:42`), so `default` **is** reached at construction; it is then set explicitly (`:154-156`, `:162`, `:188-190`). The reachable default is `Idle`, which is the correct initial state, so this is the guideline's "most common default" branch, not a defect. |
| 7 | `NotebookKind` | `Dungeon` | deliberately unchanged | Both carriers are `record` classes (`NotebookTarget.cs:15`, `NotebookSaveArgs.cs:14`), so their default is `null`, not a zero `Kind`; every producer passes an explicit kind (`DungeonDetailPage.xaml.cs:673` `Dungeon`, `:988` `Floor`, `:997` `Area`). `default` is never produced. |
| 8 | `NotebookPane` | `Description` | deliberately unchanged | A `private enum` (`NotebookView.xaml.cs:17`); every call passes an explicit pane (`:159`, `:203`, `:346` `Description`; `:349` `Notes`), and no field stores the type. Private, never defaulted, and `Description` is the pane the view opens on (`:159`, `:203`). |
| 9 | `CanvasCursorShape` | `Arrow` | deliberately unchanged | No stored field. `DungeonDetailPage.xaml.cs:1129` maps every unlisted tool to `CanvasCursorShape.Arrow` (the `_ =>` arm), and the Windows mapping falls back to the system arrow (`PlatformCanvasCursor.Windows.cs`, the `_ =>` arm of `ToSystemShape`, `:71` in the method at `:64`). `Arrow` is the ordinary cursor and the code's own fallback, so a zero `Arrow` is the most common default. |
| 10 | `BehaviorCursorShape` | `Hand` | deliberately unchanged | `HandCursorBehavior.Shape` is initialised `= BehaviorCursorShape.Hand` (`HandCursorBehavior.cs:16`). The property is initialised explicitly, and `Hand` is the behaviour's namesake default. |
| 11 | `FloorListBandKind` | `FloorCard` | deliberately unchanged | `FloorListBand` is a `readonly record struct` (`FloorReorderNearest.cs:88`), so `default` is constructible; it has `Height` 0, and `Resolve` skips every band with `Height <= 0` in both passes (`:166`, `:187`). The one producer sets the kind explicitly (`DungeonDetailPage.xaml.cs:2412-2413`). A defaulted band is ignored by construction, so its zero kind is never acted on. |
| 12 | `ErrorCategory` | `Validation` | deliberately unchanged | Modelled as `ErrorCategory?` (`Result.cs:31`, `ResultOfT.cs:49`), so "no category" is `null`, not zero; failures are built with an explicit category. Absence is `null`, so the zero value never stands for "unset". |

Paths under `src/`: `PathSegment.cs` and `PathSegmentKind.cs` in `DungeonMaster.Domain/ValueObjects/`;
`AreaConfiguration.cs` in `DungeonMaster.Infrastructure/Persistence/Configurations/`;
`PathSegmentJsonRoundTripTests.cs` in `DungeonMaster.Tests/Infrastructure/Persistence/Configurations/`;
`ControlsPaletteView.xaml.cs` and `DungeonDetailPage.xaml.cs` in `DungeonMaster.App/Views/DungeonDetailPage/`;
`ObjectKindInfo.cs` in `DungeonMaster.App/Assets/ObjectAssets/`; `MapCanvasViewModel.cs` in
`DungeonMaster.App/ViewModels/`; `MapEditorState.cs` and `FloorReorderNearest.cs` in
`DungeonMaster.App.Core/Editor/`; `NotebookTarget.cs`, `NotebookSaveArgs.cs` and `NotebookView.xaml.cs` in
`DungeonMaster.App/Controls/NotebookView/`; `PlatformCanvasCursor.Windows.cs` in
`DungeonMaster.App/Platforms/Windows/`; `HandCursorBehavior.cs` in `DungeonMaster.App/Behaviors/`; `Result.cs`
and `ResultOfT.cs` in `DungeonMaster.Application/Common/Results/`.

*Superseded framings.* This entry's body records `RotationDirection` and `ScaleDirection` as an open **class B
design question**, and its body and `Suggested direction.` record `PathSegmentKind` as **blocked**. Both are
superseded. The class B design question is decided by reachability: a zero member is owed only where the
default is reachable and wrong, and rows 2 and 3 above show neither enum's default is ever produced. The
`PathSegmentKind` blocker was real for *adding* a zero member, which needs a data migration of the stored
outlines; row 1 shows no zero member is owed, so nothing is blocked. The `Suggested direction.`'s
`JsonStringEnumConverter` route is therefore not needed for this entry.

*Drifted citations,* each re-measured by `T3` with a read of the line on the session worktree:
`ResultOfT.cs:39`, this entry's `ErrorCategory` citation, now holds `public T? Value { get; private init; }`,
and the `ErrorCategory?` property is at `:49`. `PathSegmentJsonRoundTripTests.cs:26-28` now opens the raw
literal at `:26`, and the two raw-JSON `"Kind"` lines are `:28-29`. `DungeonDetailPage.xaml.cs:140`, `:151` and
`:167` are now `:139`, `:150` and `:166` for the three `==` reads quoted above. The `ControlsPaletteView.xaml.cs`
producers cited as `:14`-`:17` are at `:27-30`; `:14` and `:19` are the two event declarations. This entry's
`What.` paragraph, the rest of its body and its `Suggested direction.` are left exactly as written, and its
`locations:` list is kept unchanged as a record of the audited state; this block does not re-derive those
lines. **No hash for this session's commit appears in this block:** no code changed for this row, and this
session's tasks run without git; the orchestrator makes the commit. The record's severity, category, effort
and risk are unchanged.**]**

### CORE-036 -- CanvasPoint's remarks block breaks sentences mid-clause across /// lines

```yaml
id: CORE-036
layer: CORE
severity: S4
category: docs
authority: conventions
citation: backend/dotnet-format.md:378
locations:
  # Enumerated, NOT compressed into ranges: a range here would silently include
  # conforming lines. CanvasPoint.cs:16-30 as a range, for instance, would sweep in
  # :19, :22, :25, :26 and :27, none of which violates the rule.
  - src/DungeonMaster.Domain/ValueObjects/CanvasPoint.cs:4
  - src/DungeonMaster.Domain/ValueObjects/CanvasPoint.cs:9,10,11,12
  - src/DungeonMaster.Domain/ValueObjects/CanvasPoint.cs:16,17,18,20,21,23,24,28,29,30
  - src/DungeonMaster.Domain/ValueObjects/CanvasPoint.cs:41,42,43
  - src/DungeonMaster.Domain/ValueObjects/CanvasPoint.cs:60,61,62,63,64
  - src/DungeonMaster.Domain/ValueObjects/CanvasPoint.cs:92
  - src/DungeonMaster.Domain/Entities/Floors/PlacedObject.cs:50,97,98,102
  - src/DungeonMaster.Domain/Entities/Floors/ObjectKind.cs:10
effort: XS
risk: low
independent: true
```

**Found by session 6b-xvii on 2026-10-09 while reading the authority for `CORE-026`, not by the 2026-08-17
audit.** The register row is `MASTER-180` at `00-master-index.md:368`, filed **open**. It was deliberately
**not fixed**: `CORE-026` is an `XS` row and folding a second deviation into it would have taken it past
the scope it was estimated against, so this is surfaced as its own entry instead.

**What.** `src/DungeonMaster.Domain/ValueObjects/CanvasPoint.cs` breaks sentences mid-clause across `///`
lines, in its type-level `<summary>`, throughout its type-level `<remarks>` block, and in three member
comments below it. `src/DungeonMaster.Domain/Entities/Floors/PlacedObject.cs` does the same at four lines,
and `src/DungeonMaster.Domain/Entities/Floors/ObjectKind.cs` at one. **The deviation is deliberately NOT
scoped to the type-level `<remarks>` block it was first noticed in** -- it falls on **both sides** of that
block in `CanvasPoint.cs`, as the inside/outside partition below sets out, and in **two further files
entirely**. This is why the `locations:` list above is wider than `MASTER-180`'s title, which names only
`CanvasPoint`'s `<remarks>` block: the title records where the finding was first seen and this list records
the population, and the T3 review raised the discrepancy between them as a `contradicts` finding.

**Why it matters.** `dm-conventions/backend/dotnet-format.md:378` states, read directly from that file:
"**Line breaks occur only at sentence boundaries.** Never break a sentence across multiple `///` lines.
Each new `///` continuation line begins a new sentence." This is a rule in the same numbered section
`CORE-026` cites, and it is **not** what `CORE-026` filed -- that entry is about *indentation* of
continuation lines, this one about *where they fall*. A file can satisfy one and violate the other, and
`CanvasPoint.cs` now does exactly that: `CORE-026`'s remedy made every continuation line two-tab indented
while leaving every sentence boundary where it was.

**The population is PREDICATE-DEPENDENT, so this entry carries an enumerated line list and deliberately
carries NO TOTAL.** Four predicates produced four different totals for it -- `measurement.md`'s review
record for T3 tabulates all four, returning 25, 15, 9 and 24 for `CanvasPoint.cs` -- and **a figure written
here would be a fact about a predicate masquerading as a fact about the code.** The predicate behind the
`locations:` list, stated so it can be re-run: *a `///` line counts when its text after `///` is non-empty
and is not a tag-only line, does not end in `.`, `!`, `?` or `>`, and the next line is also a `///` line
with non-empty non-tag text.* It was implemented as an `awk` program and **validated before its output was
trusted**, on a hand-written CRLF fixture carrying one case that must match -- a description line ending
`,` whose sentence finishes on the next `///` line -- and three near-misses that must not: an opening
tag-only line, a closing tag-only line, and a period-terminated description. It returned the one matching
line and nothing else.

The lines, repeated here grouped by whether they fall inside or outside the type-level `<remarks>` block,
which the `locations:` block above does not record: `CanvasPoint.cs` **inside** its type-level
`<remarks>` block, which spans `:7-33` --
`grep -n '<remarks>\|</remarks>'` on that file returns **six** lines, three opener/closer pairs at
`:7`/`:33`, `:59`/`:66` and `:76`/`:78`, and the type-level block is the first pair; the other two are
member-level and are named here so the citation cannot be mistaken for the whole grep's output -- **`9, 10, 11, 12, 16, 17, 18, 20, 21, 23, 24, 28, 29, 30`**;
`CanvasPoint.cs` **outside** that block -- **`4, 41, 42, 43, 60, 61, 62, 63, 64, 92`**; `PlacedObject.cs`
-- **`50, 97, 98, 102`**; and
`src/DungeonMaster.Application/Features/Dungeons/Queries/GetDungeonDetail/PlacedObjectDto.cs` -- **none**,
which is worth recording as a measured negative because that file is one of the two `CORE-026` names and a
reader might reasonably expect it here too. The partition between inside and outside was checked by set
membership, not by subtracting one count from another.

**The ten lines outside the block are the only part of this census that reproduced independently**, reached
separately by the T3 implementer and the orchestrator; this task's own run then reproduced all four lists
line-for-line, which is a third party agreeing rather than one party re-running its own command. **A
re-run cannot catch a wrong predicate** -- it reproduces the wrong answer exactly -- which is why the
agreement across parties, and the fixture validation, are what the list rests on.

**One of the listed lines is PRE-EXISTING and could not have been fixed by the session that read it.**
`ObjectKind.cs:10` ends `is a <see cref="PlacedObject"/>,` with the sentence finishing on `:11`. That text
sits inside the `None` member summary **`CORE-025`'s remedy was required to preserve verbatim**, and it was
preserved -- byte-identical at 176 bytes against the pre-change blob, verified with `cmp` against an offset
control that differed. So `CORE-025` could not have repaired it without violating its own preservation
requirement, and it is recorded here rather than left as an apparent oversight in that entry.

**Suggested direction.** Re-break each listed line at a sentence boundary, changing no words. The
indentation is already conforming after `CORE-026`'s remedy, so this is a pure line-break change and the
predicate above doubles as its verification: it should return no lines in all three files afterwards. Note
that `dotnet-format.md:377` explicitly allows multi-sentence descriptions, so the fix is to move the
breaks, not to split the prose into more tags. `ObjectKind.cs:10` is only safe to touch once `CORE-025` is
closed, which it now is.

**[AMENDED 20261103-1947-core-doc-comments, 2026-11-03 -- `MASTER-180` is CLOSED done by this session,
and one claim in this entry's `locations:` comment is false: `CanvasPoint.cs:19` at `d2b84a2` does
violate the rule. Classification: MEASUREMENT ERROR, by a predicate blind to the case.** The register
row is `MASTER-180` at `00-master-index.md:368`, now closed **done**. **Nothing below deletes any
previous wording.** Every `CanvasPoint.cs` line number in this block is labelled with the commit it was
measured at, because this session's task `T1` re-broke that file's `///` lines and moved them.

*Correction -- the claim that `CanvasPoint.cs:19` at `d2b84a2` conforms.* Previous wording, verbatim, from the `locations:` comment at
`:2801-2803` of this file at `d2b84a2`, each line's `  # ` prefix left out: *"Enumerated, NOT
compressed into ranges: a range here would silently include conforming lines. CanvasPoint.cs:16-30 as a
range, for instance, would sweep in :19, :22, :25, :26 and :27, none of which violates the rule."* Every
`CanvasPoint.cs` line number in that quotation refers to the file at `d2b84a2`. **`:19` at `d2b84a2` is wrong.** At
`d2b84a2`, `CanvasPoint.cs:19` ends `while <see cref="float.PositiveInfinity"/>`, and its sentence
continues on `CanvasPoint.cs:20` at `d2b84a2` -- a sentence broken across two `///` lines, which
`backend/dotnet-format.md:378` forbids. This entry's own predicate cannot see it, because it exempts
every line ending `>`, and an inline tag such as `<see cref="..."/>` ends in `>`. The same predicate
without that exemption, recorded verbatim as `P2` in section 2 of
`.sir/sessions/20261103-1947-core-doc-comments/contracts/C1-measurement.md`, reports the line: run on
`CanvasPoint.cs` at `d2b84a2` it prints this entry's `CanvasPoint.cs` list plus `19`, while this entry's
predicate (`P1` in section 1 of that file) prints the list alone. Neither predicate reports
`CanvasPoint.cs:22`, `:25`, `:26` or `:27` at `d2b84a2`, so the rest of the quoted claim stands.

*Closure.* `MASTER-180` is closed **done** by this session; the remedy is its task `T1`. Measured on the
session worktree after `T1`, `P2` prints nothing on `src/DungeonMaster.Domain/ValueObjects/CanvasPoint.cs`,
`src/DungeonMaster.Domain/Entities/Floors/PlacedObject.cs` and
`src/DungeonMaster.Domain/Entities/Floors/ObjectKind.cs`, so the `Suggested direction.` paragraph's
verification -- no lines in all three files afterwards -- holds under the stronger predicate as well as
under this entry's own. `P2`'s controls were re-run first, on the two CRLF fixtures that section
records: it printed only line 2 of `fixture1`, a sentence broken mid-clause, and only line 1 of
`fixture2`, a sentence broken after `<see cref="X"/>`, which this entry's predicate passes over. The
record's severity, category, effort and risk are unchanged, and its `locations:` block above
deliberately keeps its line numbers: they describe `CanvasPoint.cs` at `d2b84a2`, before `T1`.**]**

## Layer verdict

Both layers hold their shape. The Domain has no project or package references and, on reading all
twenty files, no conceptual leak either: no framework types, no persistence vocabulary, and a
behaviour-rich model where the aggregate owns floor renumbering, outline replacement and scale bounds
rather than exposing setters. The four private parameterless constructors are the only ORM
accommodation and they name nothing about persistence. The Application layer likewise depends only on
Domain, keeps commands, queries, handlers and validators in the shape the conventions prescribe, and
its thirty-odd triplets are strikingly consistent -- consistent enough that their shared skeleton is
now duplication rather than convention (CORE-013).

The internal seams are where the layers are weakest, and the fault line is the same in both: failure
information is discarded at the seam. `Dungeon`'s mutators return `void` and no-op on an unknown child
id, so a handler cannot tell success from silent nothing and returns `Success(true)` regardless
(CORE-001); one path throws instead (CORE-002); one miscategorises (CORE-014); and `ErrorCategory`,
the type that exists to carry this distinction, has no production consumer at all (CORE-007). Fixing
the aggregate's return types is the single change that unlocks most of the rest.

Three named areas are otherwise sound. The dispatcher composes its pipeline correctly -- reversal and
per-iteration capture both check out -- and I judge the hand-rolled design as earning its keep today
(CORE-031), with its costs filed as CORE-010 through CORE-012. `Result`/`ErrorCategory` is used
consistently and never unwrapped carelessly; its problems are design-level, not discipline-level. The
partial-entity split works: `Floor.cs` at 73 lines stays readable beside 305 lines of behaviour. Its
only issue is that three entities arrange it three ways (CORE-022).

The boundary that does need attention is Application-to-App: `Common/Map` holds three geometry types
with no caller inside the layer (CORE-020). T6 owns the boundary record itself.

## Attestation

- No file under `src/` was created, modified, or deleted by this task: yes
- Every record cites at least one file path with a line number: yes
- Every record names its authority, and every non-`judgment` record carries a real citation: yes
