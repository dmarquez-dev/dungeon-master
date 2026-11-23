# test-strength-rows

Close five test-strength rows from `.sir/backlog.md` by strengthening five existing tests or fixtures in `src/DungeonMaster.Tests`. Each one is shown able to fail by injecting its defect.

## Context

Each of the five rows names a test that passes against a defect it exists to catch. All five were re-measured at `e951be1` during discuss (see `discussion.md`) and again by the architect at Gate 2:

1. `IconFontFixture`'s `ElementTag` regex has a lazy `[^>]*?` attribute run that stops at the first `>` inside an attribute value. An icon-font element whose `FontFamily` comes after such a `>` is never reported.
2. `TrueTypeCmapTests.EverySubtableFormatInOneCmapIsRead` feeds format 0 only `0x41`, which the format-4 segment also covers. With format 0's loop broken, the test still passes (architect's scratch run: `Failed: 1, Passed: 10`, only the format-0 unit fact failing).
3. `GetDungeonDetailQueryHandlerTests.HandleAsync_WithExistingId_ReturnsDtoFromRepository` arranges `[]` for `Floors` and checks only `BeSameAs`, so it does not catch a handler that mutates the list in place.
4. `ThemeSource.QualifiedElementName` returns a bare local name for any namespace other than XAML. The two element-type assertions built on it (`FloorDragHandleTests` "Label" and `FloorExpandToggleTests` "Button") therefore accept an element of that name in any namespace. Both still pass with a `clr-namespace` injected on the element (architect's scratch run, 11/11).
5. `DungeonLastModifiedOnFailedMutationTests` depends on the wall clock advancing, through `SpinWait.SpinUntil`.

## Definition of done

- [ ] D1 `IconFontFixture.cs` no longer contains `ElementTag`, and its scan is an `XDocument` walk loaded with line info. A new fact passes inline XAML that has `>` inside an attribute value before the icon-font attributes, and it is reported. The fact fails under the old regex scan. The real application tree's population stays at the 13 declarations measured at `e951be1`.
- [ ] D2 `EverySubtableFormatInOneCmapIsRead` feeds format 0 a code below `0x41`. It fails when `TrueTypeCmap.ReadFormat0`'s loop is injected to read nothing, and passes once that is reverted.
- [ ] D3 `HandleAsync_WithExistingId_ReturnsDtoFromRepository` arranges a non-empty `List<FloorDto>`. It fails against an injected handler that removes one element in place, and against one that replaces an element in place with a value-equal copy.
- [ ] D4 The two element-type assertions built on `QualifiedElementName`, `FloorDragHandleTests.cs:94-101` and `FloorExpandToggleTests.cs:66-73` at `e951be1`, require the MAUI default namespace. Each fails when its element in `FloorSidebarItem.xaml` is injected into a non-MAUI namespace. The other 17 call sites of the helper are unchanged.
- [ ] D5 `git grep -n SpinUntil -- 'src/DungeonMaster.Tests*'` prints nothing. Each of the five `..._ReturnsNotFoundAndLeavesLastModifiedUnchanged` facts fails when a failed mutation advances `LastModified`. The sixth fact, `UpdateArea_WithExistingFloorAndExistingArea_ReturnsSuccessAndAdvancesLastModified`, is a success-path fact, so it fails when a successful mutation does not advance `LastModified`.
- [ ] D6 Test-only: `git diff --name-only main...<tip>`, run in the worktree, lists only paths under `src/DungeonMaster.Tests/`.
- [ ] D7 At the session tip, `dotnet build src/DungeonMaster.sln` reports `0 Warning(s)` and `0 Error(s)`, and `dotnet test src/DungeonMaster.sln --no-build` passes App.UnitTests 217, DungeonMaster.Tests 1014 and App.Tests 146.
- [ ] D8 Gate 3 removes exactly the five rows from `.sir/backlog.md`, identified by session and subject: 2026-09-02-app-sub-suite-rebuild `IconFontFixture` tag scan; 2026-09-02-app-sub-suite-rebuild `TrueTypeCmapTests` format-0 (MASTER-101); 2026-09-08-fixtures-and-de-duplication `GetDungeonDetailQueryHandlerTests` (MASTER-069); 2026-09-16-floor-sidebar-keyboard-route `QualifiedElementName`; 20261118-2110-decided-test-rows `CaptureLastModifiedAndWaitForClockToAdvance`.

## Scope

The five rows above, as five serial tasks (T1 to T5), each owning its test file plus the one file it injects its defect into. Every injection is reverted, and the file is hashed back to its `e951be1` working-tree sha256.

### Not in scope

- Production code changes of any kind. A row that turns out to need one is escalated. This is carried by the end-hash criteria C2.4, C3.4, C4.5 and C5.5, and by D6.
- Changing `ThemeSource.QualifiedElementName` itself, or its other 17 call sites. Gate 2 decision D1 chose to strengthen the two assertions instead. The latent over-inclusion at the 8 selection-filter sites (a foreign `Style`, `Setter` or `Label` would be selected as a MAUI one) is routed to the backlog at Gate 3, to be named in that gate's ask.
- `docs/audit/00-master-index.md`'s MASTER-101 and MASTER-069 entries.
- Any other backlog row.

## Assumptions

| Question | Answer | Rung | Evidence |
|---|---|---|---|
| Can the tasks build concurrently? | No. They form a serial chain, T1 to T5. | repo | All build `DungeonMaster.Tests.csproj` with its one obj/bin and its ProjectReferences (`DungeonMaster.Tests.csproj:21-24`); lock failures are recorded at `.sir/lessons.md:74` |
| Which tasks break a file while running? | All five, each in a file it owns: T1 its own seam; T2 `TrueTypeCmap.cs:141`; T3 `GetDungeonDetailQueryHandler.cs:14-19`; T4 `FloorSidebarItem.xaml:139` and `:289`; T5 `DungeonMethods.cs:464` and `:467` | repo | The criteria C1.5, C2.2-C2.4, C3.2-C3.4, C4.2-C4.5 and C5.3-C5.5 |
| Is a temporary App XAML injection safe? | Yes, if tests run `--no-build` and nothing builds the App while the injection is live. DungeonMaster.Tests reads the XAML as text and does not reference the App project. | repo | `DungeonMaster.Tests.csproj:21-24` |
| What does the cmap row's "records undetermined" mean? | It is the 20261029-1708 migration's certainty marker. Re-measurement at `e951be1` supersedes it, and the task need do nothing about it. | repo | `.sir/sessions/20261029-1708-dir-backlog-to-sir/contracts/triage-file.md:52`; the architect's injection run |
| QualifiedElementName: change the helper or the assertions? | The assertions (2 sites), leaving the helper and its other 17 call sites alone. The user chose this at Gate 2 (D1). | repo + judgment | 19 call sites (`git grep -n "QualifiedElementName(" e951be1 -- src`, with doc comments and the declaration filtered out). Only 2 are `.Should()` assertions. 3 TokenBindingFixture sites and 2 attribute sites need the bare name. |
| Does the sixth LastModified fact fail when a failed mutation advances LastModified? | No. It is a success-path fact, so it gets the opposite injection (C5.4). The row's check "each of the six" holds for five. | repo | `DungeonLastModifiedOnFailedMutationTests.cs:138-163`; architect's run, `Failed: 5, Passed: 1` |
| Which backdate precedent applies? | `BackdateLastModified` by reflection to `new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc)` | repo | `EntityEqualityTests.cs:171`, `:196-199`; `FloorMethodsTests.cs:17`, `:31-34`; private setter `Entity.cs:27`, all at `e951be1` |
| Can T3's in-place mutation be exercised with `[]`? | No: `[]` arrives as `FloorDto[]`, so a mutating handler throws instead of mutating. Arrange a `List<FloorDto>`. | repo | Architect's scratch run, InvalidCastException |
| Test-count chain? | DungeonMaster.Tests goes from 1012 to 1014 at T1 (+2 facts), then +0 for T2-T5. App.UnitTests stays 217 and App.Tests 146. | repo | `state.json` verification probe at `e951be1` |
| Which discipline rules are encoded as criteria? | Coverage (`disciplines/test.md:35`) in every discrimination criterion; Naming (`:16-18`) in C1.4; no wall clock and no sleeping (`:24`, `:28`) in C5.2 and C5.6 | docs | The composed `disciplines/test.md` |

## Contracts

None. The tasks share no declarations or interfaces.

## Open questions

None. Gate 2 decision D1 is answered: strengthen the assertions.
