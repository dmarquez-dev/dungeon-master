# MASTER-044 -- `Result<T>.Value` documents a `null` that `Result<Guid>` cannot produce

Close audit register entry `MASTER-044` by making `Result<T>`'s documentation, its one sentinel
consumer, and its own test say what is actually true -- and amend `CORE-005` with the four claims the
measurement falsified.

## Context

`MASTER-044` (`docs/audit/00-master-index.md:232`, source `CORE-005` at
`docs/audit/01-domain-application.md:264`) has **never been measured by any session**. The
measurement pass run this session against `42ec7e9` -- `measurement.md`, which this spec sits on top
of -- found four of its seven claims stale or false.

They did not go stale because anyone worked on `MASTER-044`. They went stale because **`CORE-006`,
the entry immediately below it at `:299`, was fixed**: a non-generic `Result` landed for the commands
with no return value, and the accessors became `private init`. That narrowed `MASTER-044`'s premise
from fourteen of sixteen dispatchable operations to four, and moved `Result<T>` into a file the entry
does not name.

What survives is narrow and real:

- `ResultOfT.cs:27` says `Value` is "`null` when the operation failed". For the only closed
  construction production uses -- `Result<Guid>`, `n = 4` command types -- `T?` on an unconstrained
  `T` is just `T`, so the failure value is `Guid.Empty` and `null` is unreachable.
- `MapCanvasViewModel.cs:213` is the sentinel that mistake produces: `if (addResult.Value ==
  Guid.Empty) return;`, sitting **after** the `IsFailure` early return, so it can only be reached on
  the success path -- where `AddAreaCommandHandler` returns `area.Id` and `Entity.Id` is initialised
  to `Guid.NewGuid()`. It is unreachable, and if it were reached it would return silently with no
  error shown.
- `ResultOfTTests.cs:36` is named `Failure_WithCategory_IsNotSuccessAndCarriesNoValue` with a summary
  saying "no value", and asserts `result.Value.Should().Be(Guid.Empty)`. **The suite pins the defect
  as a requirement**, which is why the entry cannot be closed without going through it.

## Definition of done

1. The `Value` XML documentation in `ResultOfT.cs` no longer claims the property is `null` on
   failure. `grep -c "when the operation failed"` on that file returns `0`.
2. That documentation states the failure value is the **default** of `T`, using
   `<see langword="default"/>`.
3. That documentation names the consequence for `Result<Guid>` -- the failure default is
   `Guid.Empty`, indistinguishable from a `Guid` a success could return.
4. That documentation names `IsSuccess`/`IsFailure` as the only failure signal, and says the default
   must not be read as one.
5. `MapCanvasViewModel.cs` contains no comparison of a dispatch result's `Value` against
   `Guid.Empty`. `grep -n "Value == Guid.Empty"` on that file prints nothing.
6. The legitimate read survives: the area id from the successful `AddAreaCommand` is still passed to
   `UpdateAreaOutlineCommand`, and is that file's only read of `addResult.Value`.
7. No test name or XML summary in `ResultOfTTests.cs` claims a failed `Result<Guid>` carries no
   value.
8. That file contains a `[Fact]` naming the **default** behaviour, asserting against `default(Guid)`
   written literally rather than `Guid.Empty`, with a summary stating the default is not a failure
   signal.
9. A new source-scanning fact pins the sentinel's absence across all four dungeon-detail view models,
   is shown able to flag the removed sentinel form, is shown **not** to flag three named near-misses,
   and is guarded against scanning nothing.
10. `MASTER-044`'s status cell in `00-master-index.md` is non-empty and marks the entry done.
11. `CORE-005` carries an amendment block recording the corrected location, the `private init`
    accessors, the corrected population (`4` of `16`, `Result<Guid>` only), that no command closes
    `T` as `bool`, the corrected sentinel citation, and that the `TryGetValue`/`Match` half of its
    suggested direction was considered and not adopted, with the evidence.
12. No regression: `dotnet test src/DungeonMaster.sln` reports `Failed: 0` on all three test-project
    lines and exits `0`, with `DungeonMaster.Tests` reporting **strictly more** than its 850
    baseline.

## Scope

Four files change, plus one new test file:

- `src/DungeonMaster.Application/Common/Results/ResultOfT.cs` -- documentation only
- `src/DungeonMaster.App/ViewModels/MapCanvasViewModel.cs` -- delete the sentinel
- `src/DungeonMaster.Tests/Application/Common/Results/ResultOfTTests.cs` -- repair the test
- `src/DungeonMaster.Tests/App/ViewModels/ResultSentinelIsAbsentTests.cs` -- **new**
- `docs/audit/00-master-index.md`, `docs/audit/01-domain-application.md` -- the closure

### Not in scope

- **Changing `T?` to a constrained `T`.** `T?` on an unconstrained `T` is the correct C# annotation
  for "may be the default"; the defect is the prose. Pinned as a preserve criterion (`A5`) with the
  reasoning recorded, and recorded in the audit (`D8`). **This is the one decision available for Gate
  2 to overturn** -- see Open questions.
- **Adding `TryGetValue(out T)` or a `Match` accessor**, which `CORE-005`'s suggested direction
  floats. Repository evidence is against it: `n = 0` `Try`-pattern method declarations anywhere in
  `src/`, production or test.
- The `public init` accessors, the `bool` commands, and the queries. All three are parts of the entry
  that are no longer true -- already `private init`, already gone, and never used `Result<T>`
  respectively. They are corrected in the amendment, not in code.
- `CORE-006` itself, and every other open register entry.
- The 8 lone `CR` characters in `.dir-sdlc/backlog.md`, outstanding since 6b-iv.

## Assumptions

| Question | Answer | Rung | Evidence |
|---|---|---|---|
| Which shape should the fix take -- document, constrain, or add a safe accessor? | Document. Correct the prose, delete the consumer sentinel it produced, repair the test. | 1 -- repo evidence | `Try`-pattern declarations in `src/`: **n = 0** (`grep -rn "bool Try[A-Za-z]*("`, obj/bin filtered; validated against a synthetic positive and a synthetic near-miss). Production generic constraints: **n = 1**. Throwing members in Application+Domain+Infrastructure: **n = 1**. **Two of these are single instances and are reported as such, not as conventions.** |
| Does this session need a frozen contract? | No. Ownership sets are disjoint and nothing crosses a task boundary as an interface. | 1 -- repo evidence | `manifest.json` records `"contracts": []`. The preceding session declared none on the same grounds. |
| How does this repository pin a source-level fact? | A source-scanning test reading files through `RepoPaths`. | 1 -- repo evidence | **n = 18** consumer files under `src/DungeonMaster.Tests`. **This one is a convention.** |
| How does this repository record a falsified audit claim? | An `**[AMENDED ...` block appended inside the entry, body left as written. | 1 -- repo evidence | **n = 41** (`01`:1, `04`:2, `05`:38). `01-domain-application.md:260-262` states the rule in its own words for `CORE-004`. |
| Is the sentinel at `MapCanvasViewModel.cs:213` reachable? | No. It sits after the `IsFailure` return, so only the success path reaches it, and that path returns a freshly created id. | 1 -- repo evidence | `MapCanvasViewModel.cs:206-215`; `AddAreaCommandHandler.cs:33`; `Entity.cs:11`. |
| Will removing it disturb the existing source-scanning guard on that file? | No. `FailureIsSurfacedTests` requires the `IsFailure` branch and the `ReportFailure` routing, both of which precede the sentinel and survive. | 1 -- repo evidence | `FailureIsSurfacedTests.cs`; pinned as `B2`/`B3` and measured empirically by `B7`. |
| Can the new test file land inside an existing scan population? | No. `RepoPaths.AppSourceFiles` scans under `src/DungeonMaster.App` only; the new file is under `src/DungeonMaster.Tests`. | 1 -- repo evidence | `RepoPaths.cs:58-70`; all 6 `AppSourceFiles("*.cs")` consumers checked by the orchestrator. |
| What is the no-regression baseline? | 850 / 23 / 102 = **975 passed, 0 failed**. | 1 -- repo evidence | Measured **twice this session, independently**: by the orchestrator in the main checkout and by the architect in the worktree, both at `42ec7e9`. |

## Contracts

**None.** `manifest.json` records `"contracts": []`. There is therefore no contract-operation table in
`coverage.md` and no contract-operation search at Gate 3.

## Open questions

| Question | Who resolves it | When |
|---|---|---|
| Should `T?` be left unconstrained and documented (chosen), or should the type be narrowed instead? Definition-of-done items 1-4 take the documenting route; `measurement.md` §3 fact 2 states the annotation as a live fact, and the architect reports it as a decision rather than a closure. | **The user** | **Gate 2.** Overturning it re-opens `T1` before the freeze; leaving it stands as specified. |
