# App follow-ups

Close the five App-layer follow-up rows that session `20261102-2225-app-correctness` appended to `.sir/backlog.md` (lines 194-198, F1-F5).

## Context

Every fact below was re-measured in the worktree `C:/dm-wt/20261103-2239-app-follow-ups` at `2a7d27b`, on branch `agent/20261103-2239-app-follow-ups`. All of them agree with the Gate 1 spot-checks, with one correction under F1.

- **F1.** In `DungeonListPage.xaml`, `StartSessionButton` (:104) carries `ToolTipProperties.Text="Start session"` at :107. That tooltip sits beneath the overlay `BoxView` tooltip "Sessions are not available yet" at :118. The detail page's `StartSessionButton` (:124) has **no** tooltip of its own, only the overlay one at :152. The stacking therefore exists on the list page only. Command: `grep -n ToolTipProperties` on both XAML files.
- **F2.** Ctrl+0 is handled at `DungeonDetailPage.xaml.cs:484-488` (`KeyChord.IsControlOnly`). The user accepted it as-is at Gate 1, so nothing changes.
- **F3.** `ZoomLevelGuard.Next(current, raw)` returns `current` when `raw` is non-finite. Otherwise it snaps `raw` to 1.0 within 0.05 and clamps it to [0.25, 4.0]. So `Next(x, 1.0f) == 1.0f` for every `x`. A census of production `ZoomLevel` writes finds exactly 2, with `grep -rnE "ZoomLevel[[:space:]]*(=([^=]\|$)\|\+=\|-=\|\*=\|/=)" src/DungeonMaster.App --include=*.cs --exclude-dir=obj --exclude-dir=bin`:
  - `:520` is guarded.
  - `:527`, `= 1.0f;` inside `ResetViewport`, is unguarded.
- **F4.** No consumer was found.
  - `grep -rIn "GreaterThanZeroConverter" src` (bin and obj excluded) prints 5 lines: `App.xaml:21`, the class at `:10`, and the test file at `:8`, `:15` and `:17`.
  - The case-insensitive `greaterthanzero` prints 8: those 5 plus 3 `TileSizeNotGreaterThanZero` Core test methods. That is the near miss.
  - Positive control: `StaticResource NotNullConverter` finds `CreateDungeonPopup.xaml:71`.
  - The test class holds 18 `[Fact]` and no theories.
  - The audit records naming the converter are UIKIT-004 (`04-app-controls-and-theme.md:224`), UIKIT-006 (`:291`) and MASTER-167 (`00-master-index.md:355`, `:666-674`).
- **F5.** `src/DungeonMaster.App.UiTests/README.md:101-104` says `FloorExpandToggle` is "reached at press 4: `StartSessionButton`, …". That walk was measured on 2026-09-14. The button is now `IsEnabled="False"` unconditionally. The test asserts membership in the walk, not a press number, so the test itself is unaffected.
- **Line endings.** All nine target files are all-CRLF with 0 lone CR, by a `perl -0777` byte census. Its fixture control `printf 'a\r\nb\rc\n'` printed `2 1 2`. `Edit` is safe on every one of them.
- **Verification probe.** `dotnet build src/DungeonMaster.sln` gives 0 warnings and 0 errors. `dotnet test --no-build` passes 187 + 943 + 139. `git status` was clean after the build.

## Definition of done

- [ ] D1. The list page's `StartSessionButton` carries no `ToolTipProperties` of its own. Its `SemanticProperties.Description`/`Hint` and the overlay tooltip are unchanged.
- [ ] D2. Hovering the start-session icon on a list card shows exactly one tooltip, "Sessions are not available yet". This is a human observation at Gate 3; no command can show which tooltip surfaces.
- [ ] D3. F2 is closed on the recorded user decision (only plain Ctrl+0 resets), and the `case '0':` handler is unchanged.
- [ ] D4. Every production write to `ZoomLevel`, including the one in `ResetViewport`, takes its value from `ZoomLevelGuard.Next`.
- [ ] D5. A reset still yields zoom 1.0, pan zero, label "100%" and a redraw, read from source.
- [ ] D6. Ctrl+0 after zooming still shows 100% and returns the map to its origin. This is a human observation at Gate 3.
- [ ] D7. A test pins `ZoomLevelGuard.Next(NaN, 1.0f) == 1.0f`. It is shown to fail under an injected defect while a control test stays green.
- [ ] D8. A controlled census, re-run immediately before deletion, finds no consumer of `GreaterThanZeroConverter`.
- [ ] D9. The converter, its `App.xaml` registration and its 18 tests are deleted, and nothing under `src` names it.
- [ ] D10. UIKIT-004, UIKIT-006 and MASTER-167 each carry an `AMENDED` block recording the deletion. No previous wording is deleted.
- [ ] D11. The UiTests README walk no longer lists `StartSessionButton` as a stop. It states press 3, says that count is inferred from source plus the platform rule rather than re-measured, and cites the rule.
- [ ] D12. The `FloorMapDrawableNonFiniteTests` remarks describe the guarded `ZoomLevel` state. Per E1, the scope is narrow.
- [ ] D13. After each building task, the solution builds with 0 warnings and every test passes.
- [ ] D14. Every edited file stays all-CRLF with 0 lone CR.
- [ ] D15. Backlog rows 194-198 are removed at Gate 3, each with its disposition.

## Scope

There are five tasks. The build chain runs T1 → T2 → T3 → T5, all serial, because they share one `obj/`/`bin/`. T4 builds nothing and is free to run alongside any of them.

- **T1, F1.** Delete the one tooltip line from the list XAML.
- **T2, F3.** Route the reset through the guard and add 2 guard tests (a NaN-current test and its control). The injection window in `ZoomLevelGuard.cs` must end byte-identical.
- **T3, F4.** Run the census, delete the converter, its registration and its tests, and add three audit amendment blocks.
- **T4, F5.** Correct the README bullet.
- **T5.** Correct the `ZoomLevel` half of the `FloorMapDrawableNonFiniteTests` remarks. E1 kept this narrow at Gate 2.

### Not in scope

- Every other backlog row, including line 199 (filing the well-formed-`Guid` defect as an audit entry).
- Any behaviour change beyond F1's tooltip removal and F3's guard routing.
- The detail page's `StartSessionButton`, which has no stacked tooltip.
- Binding Ctrl+Shift+0 or any other modified chord (F2).
- Everything under `.dir-sdlc/`, and the MASTER-167 table row at `00-master-index.md:355`.
- Re-running the Appium suite.
- The non-`ZoomLevel` line citations in `FloorMapDrawableNonFiniteTests` remarks. E1 kept T5 narrow, and this item is carried to the backlog at Gate 3.

## Assumptions

| Question | Answer | Rung | Evidence |
|---|---|---|---|
| Does F1 touch the detail page too? | No. Its `StartSessionButton` has no tooltip of its own. | repo | `DungeonDetailPage.xaml:124-139`; `grep -n ToolTipProperties` |
| How does the reset reach the guard? | Either `ZoomLevelGuard.Next(_drawable.ZoomLevel, 1.0f)` or a call to `ApplyZoom(1.0f)`. The implementer picks; the result is identical. | repo | `ZoomLevelGuard.cs:43-53`, where `Next(x, 1.0f) == 1.0f` |
| How is F3 evidenced? | A static census, plus one guard-level test with an injected defect. The live reset is a human observation at Gate 3, because `ResetViewport` is private on a page that needs a host. | judgment | none |
| Which audit text does F4 amend? | The UIKIT-004, UIKIT-006 and MASTER-167 narratives, as appended `AMENDED` blocks that quote the old wording. The table row and `.dir-sdlc/` stay untouched. | repo | Census above. Deleted-location precedent: `05-tests.md:1032`. Opener form: `01-domain-application.md:2885` |
| What is F5's corrected press count? | 3, inferred, not measured. | repo + external | Measured walk (README :102-104); `DungeonDetailPage.xaml:131`; MS Learn keyboard-accessibility: "Disabled controls are automatically removed from tab order." |
| Does the README hold a list-page walk? | No. | repo | `grep -n StartSessionButton README.md` finds only :103 |
| Are contracts needed? | No. No task calls code that another task produces. | judgment | none |
| Can two tasks build at once? | No. The chain is T1→T2→T3→T5 and T4 runs free. | repo | One worktree, so one shared `obj/` and `bin/` |
| Where do worktrees live? | `C:/dm-wt/<id>/`, now recorded as `git.worktreeRoot` in `.sir/config.json`. | repo | `.sir/lessons.md`: the MAX_PATH trap |

## Contracts

None. `manifest.json` declares `contracts: []`.

## Open questions

None. E1 was resolved at Gate 2: keep T5 narrow.
