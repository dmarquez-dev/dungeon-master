sir-sdlc 1.0.44 — Gate 3 report — Small

**Decisions needed:** none.

## 2. Acceptance criteria

- **C1** · met: `grep -rn -F '…_IsDeclared' src --include=*.cs` printed nothing; the remark now cites `<c>DmThemeKeysResolveTests.EveryKeyDmThemeResolvesIsDeclaredAsTheTypeItIsResolvedAs</c>`, declared at DmThemeKeysResolveTests.cs:149, with the `///` two-tab indent kept; the rewrap is forced (paragraph max width 103 with tabs as 4, the neighbour `<c>` line is 100, so appending "and" makes 104), and the sentence text is unchanged.
- **C2** · met: `grep -rn -F 'names that one' src --include=*.cs` printed nothing; the because-strings now name `EveryIconOnlyControlHasASemanticDescription` (SemanticDescriptionTests.cs:123), `EveryDmFontIconTextIsInTheIconFontsCmap` (IconFontCoverageTests.cs:75) and `EveryRegisteredExceptionStillExistsAndStillOffends` (TokenNamingTests.cs:346), all three existing methods; the TokenNamingTests choice is the only fact that resolves both registers against `ColourTokens()` (:263) and is the one the guard's own summary cref (:323) names; the hunks touch only string literals and comments.
- **C3** · met: `git diff a942e8f --stat -- src` lists exactly the four files; Python byte counts of CRLF, LF and CR are equal in each, with no BOM: 346, 566, 220 and 674, moved from 344, 566, 221 and 675 by +2, 0, -1 and -1, matching each hunk's line counts.
- **C4** · met: build-after.log reads `0 Warning(s)` and `0 Error(s)`; test-before.log and test-after.log give identical Totals of 217, 1008 and 146 with 0 failed (from the implementer's saved logs; the reviewer read them and did not re-run).
- **C5** · applied after approval: the two backlog rows from 20261104-1726-test-hygiene-sweep, "Replace the elided old test name" and "Reword the carried because strings", are removed and no other backlog line changes.

**Human checks:** none for this session. Three human checks are still open in `.sir/backlog.md`. The commands below were read from the repository's own documentation and not run, because each one opens the app on your desktop.

- From 2026-09-16-floor-sidebar-keyboard-route, "Take a DIU measurement of the floor-name column in the next observation pass": launch with `dotnet build src/DungeonMaster.App/DungeonMaster.App.csproj -t:Run -f net10.0-windows10.0.19041.0` from the repository root, then measure the floor-name column.
- From 2026-09-16-floor-sidebar-keyboard-route, "Run `FloorDragHandleRightEdgeClearsTheScrollViewportRightEdgeByAtLeast14`": start `appium --address 127.0.0.1 --port 4723`, then run `dotnet test src/DungeonMaster.App.UiTests/DungeonMaster.App.UiTests.csproj --filter FloorDragHandleRightEdgeClearsTheScrollViewportRightEdgeByAtLeast14 --logger "console;verbosity=detailed"` (from src/DungeonMaster.App.UiTests/README.md, sections 2 and 3).
- From 20261112-0153-floor-name-tooltip, "Run `FloorExpandToggleMeasuresAtLeast23HighAndAtMost40Wide` ... and record its logged size": the same Appium server, then the same `dotnet test` with `--filter FloorExpandToggleMeasuresAtLeast23HighAndAtMost40Wide`.

## 3. What ran

- **T1** · sir-test-engineer, then sir-reviewer · tests · sonnet / sonnet · 1 review · 0 rounds · uncommitted until approval
- Change surface: `git diff --stat HEAD -- <the four test files> .sir/backlog.md` shows 4 files changed, 13 insertions and 13 deletions. `git status --short` over the same list shows only those four modified and no untracked file. The new session directory `.sir/sessions/20261117-2333-test-wording-fixes/` is this session's record. `git diff HEAD -- <that file list>` prints the full diff.
- Files: `src/DungeonMaster.App.UnitTests/Theme/DmThemeResolutionTests.cs`, `src/DungeonMaster.Tests/App/Accessibility/SemanticDescriptionTests.cs`, `src/DungeonMaster.Tests/App/Theme/IconFontCoverageTests.cs`, `src/DungeonMaster.Tests/App/Theme/TokenNamingTests.cs`.

## 4. Advisory findings

- cosmetics: 1 (in-thread review). The DmThemeResolutionTests.cs remark now has a short `and` line and a short `early all throw out of` line, which the width forces as shown under C1.
