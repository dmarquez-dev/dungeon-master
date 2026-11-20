# Evidence handover to T5

What T1–T4 produced, as they reported it and as their reviewers verified it. **T5 cites this file and
`measurement.md`; it invents no evidence of its own about code it did not write.** Every figure here was
produced by a named command in this session, against the fixed measurement commit `1407090` or a named
later commit.

Session commits, in order (`git log --oneline 1407090..HEAD` in `C:/dm-wt/6b-iii`):

| Commit | Task | Subject |
|---|---|---|
| `6871900` | T1 | `test(handlers): add cancellation-token forwarding proof` |
| `aeac7b7` | T2 | `test(domain): add boundary and overload coverage for MASTER-086 re-justification` |
| `925fb55` | T3 | `fix(app): guard non-int values in GreaterThanZeroConverter.Convert` |
| `5347813` | T4 | `build(deps): remove unused coverlet.collector package` |

`aeac7b7` is an amend of `027f3b5`; the tree is byte-identical and only the message changed, correcting a
mis-cited audit commit (`850a10b` → `413be78`). **`413be78` (2026-08-17) is the audit commit.** Do not cite
`850a10b` as the audit commit — it is dated 2026-09-08 and belongs to session 6b-ii.

---

## T1.evidence(MASTER-070)

**Seam.** `DungeonMaster.Tests.Application.Common.Handlers.CommandHandlerTestBase<THandler>.InvokeWithTokenAsync`,
signature `protected abstract Task InvokeWithTokenAsync(CancellationToken ct)`, at
`src/DungeonMaster.Tests/Application/Common/Handlers/CommandHandlerTestBase.cs:62`. Accessor
`protected IUnitOfWorkFactory UnitOfWorkFactory` at `:49`.

**New facts, by the port each proves.** Every assertion names the token value; none uses
`Arg.Any<CancellationToken>()`.

| Port method | Fact | Declared at | Executions |
|---|---|---|---|
| `IUnitOfWorkFactory.CreateAsync` | `CommandHandlerTestBase\`1.HandleAsync_WithACancellationToken_PassesThatSameTokenToUnitOfWorkFactoryCreateAsync` | `CommandHandlerTestBase.cs:69` | 14 (once per leaf) |
| `IDungeonWriteRepository.LoadAsync` | `DungeonNotFoundHandlerTestBase\`1.HandleAsync_WithACancellationToken_PassesThatSameTokenToWriteRepositoryLoadAsync` | `DungeonNotFoundHandlerTestBase.cs:34` | 12 (once per loading leaf) |
| `IUnitOfWork.CommitAsync` | `CreateDungeonCommandHandlerTests.HandleAsync_WithACancellationToken_PassesThatSameTokenToUnitOfWorkCommitAsync` | `CreateDungeonCommandHandlerTests.cs:109` | 1 |
| `IDungeonWriteRepository.RemoveAsync` | `DeleteDungeonCommandHandlerTests.HandleAsync_WithACancellationToken_PassesThatSameTokenToWriteRepositoryRemoveAsync` | `DeleteDungeonCommandHandlerTests.cs:115` | 1 |
| `IDungeonReadRepository.GetSummariesAsync` | `GetDungeonsQueryHandlerTests.HandleAsync_WithACancellationToken_PassesThatSameTokenToGetSummariesAsync` | `GetDungeonsQueryHandlerTests.cs:83` | 1 |
| `IDungeonReadRepository.GetDetailAsync` | `GetDungeonDetailQueryHandlerTests.HandleAsync_WithACancellationToken_PassesThatSameTokenToGetDetailAsync` | `GetDungeonDetailQueryHandlerTests.cs:61` | 1 |

30 executions total (14 + 12 + 1 + 1 + 1 + 1), by
`dotnet test … --filter "FullyQualifiedName~PassesThatSameToken"` → `Passed: 30, Failed: 0, Total: 30`.

**Infrastructure cancellation tests.** Four, each `await cts.CancelAsync()` before the call, each asserting
`OperationCanceledException`, each over `SqliteInMemoryTestBase`:

| Method cancelled | Test | Line |
|---|---|---|
| `DungeonReadRepository.GetSummariesAsync` | `DungeonReadRepositoryTests.GetSummariesAsync_WithAnAlreadyCancelledToken_ThrowsOperationCanceledException` | `:72` |
| `DungeonReadRepository.GetDetailAsync` | `DungeonReadRepositoryTests.GetDetailAsync_WithAnAlreadyCancelledToken_ThrowsOperationCanceledException` | `:98` |
| `DungeonWriteRepository.LoadAsync` | `DungeonWriteRepositoryTests.LoadAsync_WithAnAlreadyCancelledToken_ThrowsOperationCanceledException` | `:231` |
| `UnitOfWork.CommitAsync` | `UnitOfWorkTests.CommitAsync_WithAnAlreadyCancelledToken_ThrowsOperationCanceledExceptionAndWritesNothing` | `:115` |

Reviewer confirmed each seeds its row first, so with a live token the call returns data rather than throwing
— **the cancellation is what discriminates, not a missing row.**

**Injected defect.** `CreateDungeonCommandHandler.cs:20`, `CreateAsync(ct)` → `CreateAsync(CancellationToken.None)`.
Command `dotnet test … --filter "FullyQualifiedName~…Features.Dungeons.Commands"` → `Failed: 1, Passed: 74, Total: 75`.
Sole failure: `CreateDungeonCommandHandlerTests.HandleAsync_WithACancellationToken_PassesThatSameTokenToUnitOfWorkFactoryCreateAsync`.
Controls green in the same run: `…CreateDungeonCommandHandlerTests…PassesThatSameTokenToUnitOfWorkCommitAsync`
and `…AddAreaCommandHandlerTests…PassesThatSameTokenToUnitOfWorkFactoryCreateAsync`. Reverted; production
byte-identical. Reviewer independently reproduced the injection: `Failed: 1, Passed: 29`, then `Failed: 0, Passed: 30`.

**Post-revert filter totals** (`dotnet test src/DungeonMaster.Tests/DungeonMaster.Tests.csproj --filter "FullyQualifiedName~<X>"`),
all `Failed: 0`: `…Application.Common.Handlers` 14 · `…Features.Dungeons.Commands` 75 · `…Features.Dungeons.Queries` 6 ·
`…Infrastructure.Persistence.Repositories` 23. Build proof for full leaf override:
`dotnet build src/DungeonMaster.Tests/DungeonMaster.Tests.csproj` → `0 Warning(s), 0 Error(s)` (a missed override is CS0534).

**Not covered by T1, do not let MASTER-070's closure claim it:** the `Dispatcher` itself. T1 covers the handler
and repository boundaries only.

---

## T2.evidence(MASTER-086)

**New tests.** Four `[Theory]` (3 `InlineData` each — `null`, `""`, `"   "` — asserting `ArgumentException`):

| Guard proven | Test | Line |
|---|---|---|
| `Dungeon.cs:25` | `DungeonAggregateTests.Constructor_WithNullOrWhitespaceName_ThrowsArgumentException` | `:341` |
| `DungeonMethods.cs:24` | `DungeonAggregateTests.Update_WithNullOrWhitespaceName_ThrowsArgumentException` | `:354` |
| `Area.cs:31` | `DungeonAggregateTests.AddArea_WithNullOrWhitespaceNameOnExistingFloor_ThrowsArgumentException` | `:373` |
| `AreaMethods.cs:24` | `DungeonAggregateTests.UpdateArea_WithNullOrWhitespaceNameOnExistingArea_ThrowsArgumentException` | `:393` |

Reviewer verified the crux: `DungeonMethods.AddArea` returns `null` at its floor lookup (`:208-214`) and
`UpdateArea` returns `NotFound` at its floor lookup **before** the name guard is reached — the tests pass a
real `floor.Id` and a real `area.Id`, so the guard is what throws.

Plus five `[Fact]`: `ReorderFloor_WithNegativeNumber_ReturnsOutOfRangeAndLeavesNumberUnchanged` (`:413`);
`AddPlacedObject_WithoutScale_ReturnsObjectWithDefaultScaleAndGivenKindPositionAndRotation` (`:431`);
`AddPlacedObject_WithoutScaleOnUnknownFloorId_ReturnsNull` (`:455`);
`UpdateObject_WithoutScaleOnExistingObject_ReturnsSuccessAndAppliesPositionAndRotation` (`FloorObjectMethodsTests.cs:129`);
`UpdateObject_WithoutScaleOnUnknownId_ReturnsNotFound` (`:152`).

Reviewer confirmed overload binding by arity: `DungeonMethods.cs:371` (4-arg) vs `:404`, and
`FloorObjectMethods.cs:96` (3-arg) vs `:134`, all arity-distinct with no optional parameters — so the new
tests hit the previously untested overloads, not the ones already covered at `FloorObjectMethodsTests.cs:76`/`:98`.

**Injected defect.** Removed `ArgumentException.ThrowIfNullOrWhiteSpace(name);` from `Area.cs:31`.
`dotnet test … --filter "FullyQualifiedName~DungeonMaster.Tests.Domain"` → `Failed: 3, Passed: 66, Total: 69`;
the three failures were the three cases of `AddArea_WithNullOrWhitespaceNameOnExistingFloor_ThrowsArgumentException`,
message `Expected a <System.ArgumentException> to be thrown, but no exception was thrown.` Named green control:
`UpdateArea_WithNullOrWhitespaceNameOnExistingArea_ThrowsArgumentException` — it routes through a **different**
guard (`AreaMethods.cs:24`), so it legitimately stays green. Restored; reviewer verified `Area.cs` byte-identical
to base (normalized blob `fe5b905b…`, CRLF intact: 1788 bytes on disk vs 1726 in the blob = 62 CR bytes, 62 lines).

**Filter total:** `Failed: 0, Passed: 69, Skipped: 0, Total: 69`, reproduced by the reviewer.

**Deliberately not written, per escalation E2** — record this as a decision, not an omission: no
`Area.SetOutline` degenerate/null-outline test, no `Floor` number zero/negative test, no `PathSegment`
CubicBezier null-control-point test. Each would require a guard that does not exist.

---

## T3.evidence(MASTER-167)

**Guard.** `src/DungeonMaster.App/Converters/GreaterThanZeroConverter.cs:46-49`:

```csharp
if (value is not int n)
{
	return false;
}
```

It is the **first statement** in `Convert`; the `Invert` block is now at `:53`. XML doc updated at `:15-20`
and `:37-38`. Wider than `MASTER-167` prescribed, per escalation **E3**.

**Red step — the entry's central evidence.** Run against the **unmodified** converter:
`dotnet test src/DungeonMaster.App.Tests/DungeonMaster.App.Tests.csproj --filter "FullyQualifiedName~GreaterThanZeroConverterTests"`
→ `Failed: 5, Passed: 13, Total: 18`. The five failures were the `UnsetValue`, `null`, `double`, `long` and
`string` rows at source lines 207/223/239/255/271, each `Expected: False / Actual: True`. Named green control:
`ConvertReturnsTrueForZeroWhenInverted` (`:103`). After the guard: `Failed: 0, Passed: 18, Total: 18`,
reproduced by the reviewer. Red artifact preserved at the T3 scratch directory as `red.txt` (5,639 bytes).

**Five failures, not one, is the finding.** `MASTER-167` describes the sentinel row only; the defect covers
every non-`int`. A sentinel-only guard would have left four of the five rows broken.

**The pre-existing test is not evidence.** `ConvertReturnsFalseForTheUnresolvedBindingSentinel` (`:24`) passes
`parameter = null`, so the negation never runs. It was green against the defective converter and is green now.
Reviewer verified it is absent from the red run's five failures. **Say this explicitly in the amendment** — a
future reader seeing a sentinel test in this file could otherwise conclude the case was already covered.

**Consumers — cite these by `file:line`, never by a grep line count.** There are **7 XAML sites**: the resource
declaration at `App.xaml:21`, and six bindings at `DungeonDetailPage.xaml:125`, `:131`, `:151` and
`DungeonListPage.xaml:100`, `:106`, `:115`. Exactly **two** pass `ConverterParameter=Invert` —
`DungeonDetailPage.xaml:151` and `DungeonListPage.xaml:115`. Both are transparent `BoxView` overlays carrying
the identical tooltip "Add at least one floor before starting a session". None was edited.

> **Contradicts finding, disposition adopted.** `contracts/converter-behaviour.md:72-74` cites
> `grep -rn "GreaterThanZeroConverter" src/DungeonMaster.App/` as yielding **7 lines**. Verified in the
> worktree it yields **15**; `--exclude-dir=obj` yields 10; excluding `obj` and `bin` yields 8; XML-only with
> both exclusions yields 7. The substance is right, the instrument is not — the count swings with build-artifact
> state. **T5 enumerates the sites by `file:line` and cites no count-producing command for them.** See
> `reviews/T3.md`.

---

## T4.evidence(MASTER-155)

**Four removal sites**, all confirmed at the contract's lines before editing:

| File | Removed line |
|---|---|
| `src/DungeonMaster.Tests/DungeonMaster.Tests.csproj:9` | `<PackageReference Include="coverlet.collector" />` |
| `src/DungeonMaster.App.Tests/DungeonMaster.App.Tests.csproj:26` | `<PackageReference Include="coverlet.collector" />` |
| `src/DungeonMaster.App.UnitTests/DungeonMaster.App.UnitTests.csproj:20` | `<PackageReference Include="coverlet.collector" />` |
| `Directory.Packages.props:20` | `<PackageVersion Include="coverlet.collector" Version="6.0.4" />` |

**Lock regeneration by tooling.** `dotnet restore src/DungeonMaster.sln --force-evaluate`; no mismatch reported,
no lock file deleted. Of nine `packages.lock.json`: **3 content-changed** (`DungeonMaster.Tests`, `App.Tests`,
`App.UnitTests`), **6 unchanged**. Each changed file lost exactly one six-line `coverlet.collector` `Direct`
block and nothing else — **no other `resolved` version moved**. `App.UiTests` never referenced coverlet and is
excluded from the sln, so it needed no separate restore. Reviewer re-ran with `--locked-mode --force-evaluate`,
which fails if the committed locks disagree with the tooling: it restored clean and left `git status` at the
same 7 files.

**Build:** `dotnet build src/DungeonMaster.sln` → `Build succeeded. 0 Warning(s), 0 Error(s)`, all eight projects.

**Execution unaffected:** `dotnet test src/DungeonMaster.App.UnitTests/DungeonMaster.App.UnitTests.csproj` →
`Failed: 0, Passed: 23, Total: 23`; `dotnet test src/DungeonMaster.Tests/DungeonMaster.Tests.csproj --filter
"FullyQualifiedName~DungeonMaster.Tests.Application.Common.Results"` → `Failed: 0, Passed: 22, Total: 22`.

**CPM intact:** `grep -rn 'PackageReference[^>]*Version=' --include=*.csproj --include=*.props --include=*.targets`
returns no hits. `Directory.Packages.props` now holds **15** `PackageVersion` entries, down from 16.

**Nothing else adopted:** no `.runsettings`, no `xunit.runner.json`, no `.github/` directory or workflow file,
no `[assembly: CollectionBehavior]` anywhere.

> **Cite `git status`, not mtimes.** T4's own criterion-7 evidence attributed some changed mtimes to
> "concurrent peers T2/T3". This session's chain is serial; those were T2's and T3's already-committed work.
> The conclusion was right, the reasoning was not. The orchestrator confirmed scope by `git status --porcelain`
> (7 files, all inside owns). Use that.

---

## Session-level facts T5 may cite

- **Four self-inflicted citation errors were caught this session**, each by someone downstream of whoever wrote
  the number: the falsified `RotateDirection` P4 row (grepped a file name, caught by the architect); the
  mis-cited audit commit in T2's first commit message (caught by the orchestrator, amended); the converter
  consumer-count framing (caught by the reviewer); and T4's mtime misattribution (caught by the reviewer).
  This is the same class of defect the session was convened to correct in the inherited records.
- **Escalations settled:** E1 remove coverlet · E2 file P1–P3 rather than fix · E3 guard any non-`int` ·
  E4 file `MASTER-171` as a file-name/type-name mismatch and record the falsification.
- **Only four of `05-tests.md`'s 32 records were re-measured** this pass: TEST-001, TEST-019, TEST-020,
  TEST-032. The other 28 remain unchecked rather than confirmed, and the preamble must say so.
