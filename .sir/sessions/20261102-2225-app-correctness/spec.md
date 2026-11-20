# App correctness

Close six user-visible correctness gaps in `DungeonMaster.App` (backlog rows A1-A6), each proven by a test that fails on an injected revert.

## Context

Gate 1 picked six App rows from `.sir/backlog.md` (lines 56, 57, 64, 68, 75, 77). The spec phase re-checked every one at base `18329d4`, in the worktree `C:\dm-wt\app-correctness`. **All six are still live.** Two of them carried wrong details:

- **A3 is on both pages.** The row places the button "in `DmCard`'s trailing region". That is `DungeonListPage.xaml:96`. Gate 1's table had it on the detail page. `DungeonDetailPage.xaml:118` has the same defect.
- **A6 line numbers.** The `try` is at `:548`, not `:547`, and the method spans `:533-560`.

What is live, row by row:

- **A1.** `DungeonDetailViewModel.LoadDungeonAsync` (`:172-181`) sets `_dungeonId` before the query runs. It hands a null DTO to `Repopulate` without checking it. So an id that names no dungeon shows an empty page with no error. Every editing command's `NoDungeonIsLoaded()` gate (`:131-141`) then lets saves through.
- **A2.** `CreateDungeonViewModel` assigns `ErrorMessage` only on failure (`:70`). It never clears it.
- **A3.** Both `StartSessionButton`s bind `IsEnabled` to `FloorCount > 0`. Neither has a handler. On a dungeon with floors, each is an enabled tab stop that does nothing.
- **A4.** The floor reorder chord at `DungeonDetailPage.xaml.cs:396` uses `HasFlag(Control)`. Ctrl+0 at `:480` uses the same pattern.
- **A5.** `ApplyZoom` (`:514-523`) passes a NaN zoom through `Math.Clamp` unchanged. The pinch path (`:632`) can produce one.
- **A6.** `OnMapCanvasHandlerChanged` calls `DetachScrollZoom(); AttachScrollZoom();` at `:535-536`, above its `try`.

Verification probe in the worktree:

- `dotnet build src/DungeonMaster.sln`: Build succeeded, 0 Warning(s), 0 Error(s).
- `dotnet test --no-build`: 187 + 932 + 108 tests passed, 0 failed.
- `git status` was clean after the build.

## Definition of done

- [ ] **D1.** Navigating to a well-formed id that names no dungeon leaves `DungeonId == Guid.Empty` and puts a NotFound `ErrorMessage` on screen, routed through `ResultMessage`.
- [ ] **D2.** In that state, a save dispatches no command.
- [ ] **D3.** If the dungeon is deleted between commands, so the reload after a successful save returns null, the result is the same as D1 and D2.
- [ ] **D4.** A create clears the previous `ErrorMessage` when it starts. After a later success, no error is left showing.
- [ ] **D5.** On a dungeon with floors, the detail page's `StartSessionButton` reports `IsEnabled == false` at Opacity 0.35. Its tooltip and comments are still true.
- [ ] **D6.** *(Gate 2 decision)* The list page's `StartSessionButton` behaves the same as D5. The `DmCard.xaml.cs:202` focus-ring comment is corrected.
- [ ] **D7.** The floor reorder chord fires for exactly `Control` + Up/Down, and rejects Control combined with any other modifier.
- [ ] **D8.** *(Gate 2 decision)* Ctrl+0 uses the same exact-Control predicate.
- [ ] **D9.** A non-finite requested zoom (NaN, +inf or -inf) leaves the zoom level unchanged. Finite values keep their exact current snap and clamp behaviour.
- [ ] **D10.** `OnMapCanvasHandlerChanged` runs no statement before its `try`. The catch message is true of everything the `try` covers.
- [ ] **D11.** *(Gate 2 decision)* `DungeonDetailPage.xaml.cs:616` is indented with tabs only. No line in the file has a space in its leading whitespace.
- [ ] **D12.** Every changed guard has a test that fails under a named injection that was actually run, while a control passes. The logs are saved and cited.
- [ ] **D13.** At each task's final state, `dotnet build src/DungeonMaster.sln` is clean, and the touched test projects pass with their counts named. Every touched file is CRLF.

## Scope

A1-A6, as above, plus the three Gate 2 decisions.

Five tasks, all `sir-frontend-engineer`, **run strictly in order T1→T2→T3→T4→T5**:

| Task | Covers |
|---|---|
| T1 | A1 |
| T2 | A2 |
| T3 | A3 |
| T4 | A4, plus Ctrl+0 if kept |
| T5 | A5, A6, plus the indent fix if kept |

The architect drafted T1-T4 to run in parallel. The orchestrator serialized them. All five tasks build and test in one worktree, and parallel `dotnet build` runs collide on the shared `obj/` directories. T5's A6 injection also rewrites `DungeonDetailPage.xaml.cs` while T3's tests compile that page.

### Not in scope

- **Wiring Start Session.** Sessions do not exist yet. The button is disabled, not wired.
- **Filing a new audit entry for A1.** This is a Gate 3 note.
- **The single-letter tool shortcuts.** `d`, `m`, `h`, `p`, `s`, `e` and `r` fire whatever modifiers are held. The rows do not name them.
- **`GreaterThanZeroConverter`.** If both A3 pages are done, it is left with no XAML consumer. Deleting it goes to the backlog.
- **`src/DungeonMaster.App.UiTests/README.md:103`.** It says the tab walk reaches `StartSessionButton`. That project is not in `DungeonMaster.sln` and no task owns the file, so it goes to the backlog unless Gate 2 adds it to T3.
- **Hand-built `ErrorMessage` strings in the page code-behind** (`:489` and `:558`). `FailureIsSurfacedTests` does not scan them.

## Assumptions

| Question | Answer | Rung | Evidence |
|---|---|---|---|
| What does "stop saves" mean in A1? | Clear `_dungeonId` to `Guid.Empty`. Every editing command already refuses through `NoDungeonIsLoaded()`. | 1 | `DungeonDetailViewModel.cs:131-141`; `DungeonHeaderViewModel.cs:74-79`; `FloorListViewModel.cs:78,113,149,187`; `MapCanvasViewModel.cs:74…364` |
| How is A1's error surfaced? | A new static NotFound `Result` beside `UnusableNavigationIdResult`, routed through `ResultMessage.ForFailure` | 1 | `DungeonDetailViewModel.cs:22-24,122`; `FailureIsSurfacedTests.cs` (assignments must route through `ResultMessage`) |
| After a refused save, which message shows? | `NoDungeonLoadedResult`'s text replaces the NotFound text, because the save calls `ClearError()` first. This is existing behaviour and is accepted. | 1 | `DungeonHeaderViewModel.cs:74-79` |
| Where do the A1 and A2 tests live? | `DungeonMaster.App.Tests`. It is the only test project with internals access, and it already substitutes `AppDispatcher`. | 1 | `DungeonMaster.App/AssemblyInfo.cs:3`; `FloorReorderCommandArgsTests.cs:92` |
| Which mechanism disables A3? | Local `IsEnabled="False"` plus `Opacity="0.35"`, the overlay always visible, and tooltip text saying sessions are not available yet. No `Disabled` VisualState. | 1 | `UndoButton`, `DungeonDetailPage.xaml:301-302`; `Implicit.xaml:215-224` |
| Must the reorder chord accept extra modifiers? | No. `KeyboardModifiers` is `[Flags]` with exactly None, Shift, Command, Control and Alt, and has no lock-state members. So exact equality cannot break under NumLock or CapsLock. | 1 | Reflection over `Plugin.Maui.KeyListener` 1.0.0-preview4 `lib/net10.0` (orchestrator); T4 re-measures with `Enum.GetValues` |
| Where does the A5 guard live? | In `ApplyZoom`, through an extracted `internal static ZoomLevelGuard`. It covers the buttons, the wheel and the pinch, and it is compiled and testable on Windows. | 1 | `DrawSessionResultGuard.cs:3,10` (seam precedent); `CanvasPoint.cs`; `FloorReorderNearest.cs:155` |
| Does `Math.Clamp` pass ±∞ unchanged? | No. Only NaN passes through. ±∞ clamps to a bound, so rejecting it changes behaviour, as Gate 1 directed. | 4 | .NET `Math.Clamp` semantics (architect judgment, not executed); T5's tests discriminate either way |
| Can A6's guarding be observed? | No. Nothing makes `AttachScrollZoom` throw, so the evidence is a labelled structural source scan. Human checks cover regression only. | 1 | `DungeonDetailPage.xaml.cs:562-604`; `AsyncVoidUnitsAreGuardedTests.cs` (G1 sees only the first `await`) |

## Contracts

None. Each new helper (`KeyChord`, `ZoomLevelGuard`) is used only by its own task.

## Open questions

None. The user resolved all three Gate 2 decisions at 2026-11-02T23:14Z:

1. The list page is included in A3 (D6).
2. Ctrl+0 is included in A4 (D8).
3. The mixed-indent fix at `:616` is included (D11). This closes backlog line 58.

Every task tagged `[GATE-2 SEPARABLE]` is kept. T4's `HasFlag` count criterion was settled to zero sites, with a base-copy control of 2 hits.
