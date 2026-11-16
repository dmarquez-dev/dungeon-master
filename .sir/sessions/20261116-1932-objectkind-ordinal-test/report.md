sir-sdlc 1.0.41 — Gate 3 report — Small

**Decisions needed:** none — E1, raised at Gate 1, was answered there (retire the ordinal fact, keep the reason) with no deferred action.

## 2. Acceptance criteria

T1 met all four of its criteria, as judged by the T1 review. C5 is the orchestrator's backlog edit.

- **C1** · met: `git diff 0ffc4bc -- src` removes exactly the `[Fact]` plus the 12-line `DoorWoodIsOne_DoorIronIsTwo_AndWeaponRackIsThirtySeven` method and one blank line (4 insertions, 15 deletions per `git diff --stat`), and no hunk touches any other fact
- **C2** · met: one sentence added at `ObjectKindGuardTests.cs:9-12` (`grep -n "///"`; `cat -A` shows the `///^I^I` form) saying only `ObjectKind.None`'s ordinal (0, the default) is relied on and the others are not persisted because `PlacedObjectConfiguration` stores `Kind` by name; summary lines are 83/82/75/82/61 characters (Python `len`), so none exceeds line 8's 83; the only cref is `ObjectKind.None`, already used on line 8. The reviewer checked the claim independently: `PlacedObjectConfiguration.cs:37` is `.HasConversion<string>()`, the model snapshot declares `b.Property<string>("Kind")`, and a production census (casts, `Convert.ToInt`, ordering, comparisons, JSON enum attributes, `Enum.GetValues`/`GetNames`) found no ordinal use; its positive control was `ObjectPackRegistry.cs:26`, `Enum.TryParse<ObjectKind>(entry.Kind, ...)`, a by-name parse
- **C3** · met · reviewer-reproduced: Python `-I` bytes give CRLF 109 = LF 109 = CR 109 (120 at 0ffc4bc, less 15, plus 4), BOM false, 0 space-indented lines; `git diff --name-only 0ffc4bc` lists only this file
- **C4** · met · reviewer-reproduced: `dotnet build src/DungeonMaster.sln` reads 0 Warning(s) and 0 Error(s); `dotnet test src/DungeonMaster.sln --no-build` exits 0 with totals App.UnitTests 217, DungeonMaster.Tests 1004, App.Tests 146, all passed, against the orchestrator's 0ffc4bc baseline of 217 / 1005 / 146; full output saved before filtering in the session scratch `T1/` and `T1-review/` directories
- **C5** · applied after approval: the orchestrator removes the backlog row `2026-11-03 20261103-1654-test-strength Strengthen the ObjectKindGuardTests ordinal test` after approval and before the commit, and no other backlog line changes

**Human checks:** none for this session. The change is test-only, and C3 and C4 settle it.

Human checks still open in `.sir/backlog.md`, all against the main checkout. These commands come from the project's own build and test layout. They were not run, because each one opens a window on your desktop or needs the Appium server:
- 2026-09-16-floor-sidebar-keyboard-route: take a DIU measurement of the floor-name column. Launch the built app with `C:\Users\dmarq\Development\Repos\dungeon-master\src\DungeonMaster.App\bin\Debug\net10.0-windows10.0.19041.0\win-x64\DungeonMaster.App.exe` (present at the time of this report).
- 2026-09-16-floor-sidebar-keyboard-route: with the Appium server up, run `dotnet test src/DungeonMaster.App.UiTests/DungeonMaster.App.UiTests.csproj --filter FullyQualifiedName~FloorDragHandleRightEdgeClearsTheScrollViewportRightEdgeByAtLeast14`.
- 20261112-0153-floor-name-tooltip: with the Appium server up, run `dotnet test src/DungeonMaster.App.UiTests/DungeonMaster.App.UiTests.csproj --filter FullyQualifiedName~FloorExpandToggleMeasuresAtLeast23HighAndAtMost40Wide`, and record its logged size.

## 3. What ran

- **T1** · sir-test-engineer, then sir-reviewer · tests · sonnet / sonnet · 1 review · 0 rounds · uncommitted: the test engineer made the change with Edit and ran the solution build and tests. The reviewer then reviewed it once and returned PASS with 0 blocking findings and 2 advisories.
- Scope versus the backlog row: the row cites `ObjectKindGuardTests.cs:21-26`, which had drifted; at 0ffc4bc the fact is at `:25-37`.
- Correction: the Gate 1 card and E1's question cited `PlacedObjectConfiguration.cs:36` for the string conversion, but `grep -n 'HasConversion<string>'` on that file prints `:37` (`:35` holds `placedObject.Kind)`), so the line number was off by one. The claim itself is unchanged.
- Change surface: `git diff --stat HEAD -- src/DungeonMaster.Tests/Domain/Entities/Floor/ObjectKindGuardTests.cs` (repository root) shows 1 file changed, 4 insertions and 15 deletions. `git status --short` on the same path lists it as modified (` M`), with no untracked file. `git diff HEAD -- src/DungeonMaster.Tests/Domain/Entities/Floor/ObjectKindGuardTests.cs` shows the full diff.

## 4. Advisory findings

- **contract gaps** · 1, from the in-thread T1 review: the summary at `ObjectKindGuardTests.cs:9-12` states that ordinals are not persisted, and no test pins it. If `PlacedObjectConfiguration.cs:37` were changed from a string to an int conversion, nothing would fail. The reviewer notes that this is the trade accepted at Gate 1.
- **cosmetics** · 1, from the in-thread T1 review: `<c>PlacedObjectConfiguration</c>` could be a `<see cref>`. The reviewer judged plain `<c>` acceptable, since it has precedent in the repository and avoids a cross-layer cref.
