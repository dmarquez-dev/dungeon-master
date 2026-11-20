# App follow-ups

Close the five follow-up rows that session `20261102-2225-app-correctness` appended to
`.sir/backlog.md` at its Gate 3 (backlog lines 194-198). All five are in the App layer
(`src/DungeonMaster.App`, its test projects and the UiTests README). Two are decisions the
user settled at Gate 1. Three are code or documentation changes.

## Context

The repo-audit arc on `chore/repo-audit` schedules its work from `.sir/backlog.md`. At Gate 1
that file held 159 entries by `grep -c '^20' .sir/backlog.md`, and also 159 by the second
predicate `grep -c ' -- source: ' .sir/backlog.md`. The previous session was
`20261103-1947-core-doc-comments`. Gate 1 offered four units: App follow-ups, test strength
part 2 (backlog lines 82, 100, 201, 202), the TEST style sweep (MASTER-126/128/137/139/143/144)
and UI doc/comment fixes (lines 68, 71-73, 203). The orchestrator recommended **App
follow-ups**, and the user chose it. Options 2-4 were not re-measured.

The selected rows are numbered by their line in `.sir/backlog.md` as read at Gate 1. The
backlog line is authoritative:

| # | Backlog line | Source | Row (abridged) |
|---|---|---|---|
| F1 | 194 | reviews/T3.md A-T3-1 | Decide which tooltip a hovered `StartSessionButton` card on the list page shows, and drop the button's own `ToolTipProperties.Text` if it can surface. |
| F2 | 195 | reviews/T4.md A-T4-1 | Confirm that Ctrl+Shift+0 (and any Control-plus-modifier+0) not resetting the viewport is acceptable, or bind those chords deliberately. |
| F3 | 196 | reviews/T5.md A-T5-2 | Route `ResetViewport` in `DungeonDetailPage.xaml.cs` through `ZoomLevelGuard`, so every zoom write shares the non-finite guard. |
| F4 | 197 | reviews/T3.md A-T3-2 | Delete `GreaterThanZeroConverter` (registered at `App.xaml:21`) and its tests, now that it has no XAML consumer. Count the consumers first. |
| F5 | 198 | reviews/T3.md A-T3-3 | Correct `src/DungeonMaster.App.UiTests/README.md:103`, which still lists `StartSessionButton` in the tab walk although the button is disabled and skipped on both pages. |

The "Source" column refers to files under `.sir/sessions/20261102-2225-app-correctness/`.

These are the orchestrator's Gate 1 spot-checks, taken from the main checkout at `2a7d27b`.
They are not measurements, and the spec phase must re-measure each one:

- F1: `src/DungeonMaster.App/Views/DungeonListPage/DungeonListPage.xaml:107` reads
  `ToolTipProperties.Text="Start session"` on the button with `AutomationId="StartSessionButton"`
  (`:104`) and `IsEnabled="False"` (`:108`). A transparent `BoxView` overlay at `:116-119`
  carries `ToolTipProperties.Text="Sessions are not available yet"` (`:118`). The button also
  has `SemanticProperties.Description` (`:105`) and `SemanticProperties.Hint` (`:106`). It was
  not checked whether `DungeonDetailPage` has its own `StartSessionButton` with the same
  stacking.
- F2: `src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs` handles `'0'`
  with `if (KeyChord.IsControlOnly(e.Modifiers))` (`:485`) and then calls `ResetViewport()`
  (`:486`).
- F3: in the same file, `ApplyZoom` writes `_drawable.ZoomLevel = ZoomLevelGuard.Next(...)`
  (`:520`). `ResetViewport()` (`:525`) writes `_drawable.ZoomLevel = 1.0f;` directly (`:527`).
  The signature and semantics of `ZoomLevelGuard.Next(current, raw)` were not read.
- F4: `grep -rn "GreaterThanZero" src --include=*.xaml` returned only the registration at
  `src/DungeonMaster.App/App.xaml:21`. The `.cs` hits (from `grep -rn "GreaterThanZeroConverter" src --include=*.cs`)
  were `src/DungeonMaster.App/Converters/GreaterThanZeroConverter.cs:10` and
  `src/DungeonMaster.App.Tests/Converters/GreaterThanZeroConverterTests.cs`. That test file's
  doc comment at `:8` says "UIKIT-004 asserts this converter", so the deletion may touch an
  audit-record claim, which the spec must check. The searches did not cover every
  consumption form (code-behind `Resources[...]` lookups, `{StaticResource}` in non-`.xaml`
  files, and audit docs or other tests that name the type). The spec must count all of
  them with a census that is checked against a control.
- F5: `src/DungeonMaster.App.UiTests/README.md:103` reads
  `` reached at press 4: `StartSessionButton`, `DungeonNotebookButton`, `AddFloorButton`, ``.
  The rest of that sentence and its press counts were not read.

## Decisions

- Track: **Standard**. There are five separate rows, and each closing claim can be checked on
  its own.
- Session name: `app-follow-ups`.
- **F1, decided by the user: overlay only.** Remove the button's own
  `ToolTipProperties.Text="Start session"` so that only "Sessions are not available yet" can
  surface. Keep its `SemanticProperties.Description` and `SemanticProperties.Hint` unchanged.
  If the spec finds the same stacking on another page's `StartSessionButton`, apply the same
  decision there.
- **F2, decided by the user: accept as-is.** Only plain Ctrl+0 resets the viewport, which
  matches the exact-modifier rule. This needs **no code change**. The row closes on the
  recorded decision, and the backlog row is removed at Gate 3 with this decision as its
  disposition.
- F3: the reset must write zoom through `ZoomLevelGuard`. Whether that means
  `ZoomLevelGuard.Next(current, 1.0f)` or something else depends on the guard's semantics,
  which the spec reads. The observable result of a reset stays 100% zoom and zero pan.
- F4: delete the converter, its `App.xaml` registration and its tests, but only after a
  controlled census finds no consumer. Any audit-record text the deletion falsifies (such as
  the UIKIT-004 claim) is in scope to amend. A census that finds a live consumer stops F4,
  and that becomes an escalation, not a workaround.
- F5: correct the README tab-walk text so it matches the current tab order on both pages,
  using what the source shows. The Appium suite (`src/DungeonMaster.App.UiTests`) is outside
  `src/DungeonMaster.sln`, and no session command runs it. So a corrected press count rests on
  source evidence, and the spec must say so.
- Out of scope: every other backlog row, including line 199 (filing the well-formed-`Guid`
  defect as an audit entry) from the same source session. No behaviour change beyond F1's
  tooltip and F3's guard routing.
- Standing hazards to carry into every dispatch, from `.sir/lessons.md`: never `sed -i`, and
  never `Write` an existing CRLF file. `Edit` strips trailing whitespace from `new_string`.
  Create the worktree under a short root such as `C:\dm-wt\`. Verify in the main checkout
  after the merge. `dotnet test --no-build` in an unrestored worktree proves nothing.
