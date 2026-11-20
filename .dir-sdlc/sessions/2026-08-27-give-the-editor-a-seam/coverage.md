# Coverage — 2026-08-27-give-the-editor-a-seam

Gate 2 columns written 2026-08-27, before the approval freezing `manifest.json`. End state written at Gate 3.

## Definition of done

| Item | Covering task | Gate 2 | End state |
|---|---|---|---|
| 1. `AreaOutlineGeometry` + `MapHitTester` in App.Core, MAUI-free (MASTER-095) | T1, T5, T2 | covered | **met** |
| 2. `MapEditorState` owns the editor transitions (MASTER-108, kernel) | T1, T5, T2 | covered | **met** |
| 3. Page drops the 18 interaction-state fields, keeps the 11 view-state ones (MASTER-108, page) | T5 | covered | **met** |
| 4. `DungeonDetailViewModel` becomes a composition root over three children (MASTER-106) | T3 | covered | **met** |
| 5. Reload policy uniform and deviations documented (MASTER-094, uniform half) | T3 | covered | **met** |
| 6. Four loose async methods become commands, no `dungeonId` parameter (MASTER-052) | T3 | covered | **met** |
| 7. Cursor behind a platform seam, resolution failure reported (MASTER-096) | T5 | covered | **met** |
| 8. `GetAsyncKeyState` out of the page, behind a platform seam (MASTER-056) | T5 | covered | **met** |
| 9. `DungeonListViewModel` takes `INavigationService`, no `Shell.Current` (MASTER-053) | T4 | covered | **met** |
| 10. `DungeonListPage` takes `Func<CreateDungeonPopup>`, popup still per click (MASTER-054) | T4 | covered | **met** |
| 11. `CreateDungeonViewModel` reports its outcome through one channel (MASTER-076) | T4 | covered | **met** |
| 12. Popup declares `x:DataType` or records the blocking diagnostic (MASTER-083) | T4 | covered | **met** |
| 13. Canvas + FLOORS caption + floor rows carry accessibility semantics (MASTER-084) | T6, T5 | covered | **met** |
| 14. Back action explicitly owned by the page (MASTER-058, reachable half) | T6 | covered | **met** |
| 15. Palette chip constructed in exactly one place (MASTER-057) | T7 | covered | **met** |
| 16. Executing tests for geometry, hit testing and editor state; per-assembly count above 506 | T2 | covered | **met** |
| 17. Dead `GetDungeonDetail` import removed after confirming it is unused (5a backlog 1) | T5 | covered | **met** |
| 18. 5a's two false "4-space indented" criterion texts corrected (5a backlog 2) | T8 | covered | **met** |
| 19. Unchecked-`int` limit at `FloorMapDrawable.cs:553` recorded in code (5a backlog 3) | T8 | covered | **met** |
| 20. Throwing behaviour at `DungeonDetailViewModel.cs:179` documented (5a backlog 4) | T3 | covered | **met** |
| 21. Solution builds 0 Warning(s)/0 Error(s); `net10.0-maccatalyst` still compiles | T1–T8 | covered | **met** |
| UI-016 second remedy — `RebuildFloorList` → templated `BindableLayout` (page `:729-940`, 212 lines) | — | **uncovered** — accepted out of scope at Gate 2: deferred to sub-session 5c via `.dir-sdlc/backlog.md` | **out of scope** |
| UI-007 second remedy — patch the projection instead of re-querying the aggregate | — | **uncovered** — accepted out of scope at Gate 2: deferred to sub-session 5c via `.dir-sdlc/backlog.md` | **out of scope** |
| UI-033 first remedy — verify the back affordance on macCatalyst | — | **uncovered** — accepted out of scope: unreachable under ADR-001, recorded in the spec's `### Not in scope` | **out of scope** |

## Contract operations

`Implementing task` written at Gate 3 from the definition-site search. `End state` from a whole-word content
search over the session's touched files (`git diff --name-only d33ec23..HEAD`), discarding the definition site,
files owned by the task owning that definition site, and test files.

| Operation | Implementing task | Consuming task | Gate 2 | End state |
|---|---|---|---|---|
| `AreaOutlineGeometry.Translate` | T1 | T5 | covered | **called** — `DungeonDetailPage.xaml.cs` (also `FloorMapDrawable.cs`) |
| `AreaOutlineGeometry.ToPolygon` | T1 | T1 | covered | **called** — `MapHitTester.cs:86`, inside the implementing task; reached transitively because `HitTestArea` has an external caller |
| `AreaOutlineGeometry.ContainsPoint` | T1 | T1 | covered | **called** — `MapHitTester.cs:88`, same transitive reach as `ToPolygon` |
| `AreaOutlineGeometry.Area` | T1 | T1 | covered | **called** — tie-break inside `MapHitTester`, same transitive reach |
| `MapHitTester.HitTestArea` | T1 | T5 | covered | **called** — `DungeonDetailPage.xaml.cs:1808` |
| `MapHitTester.HitTestObject` | T1 | T5 | covered | **called** — `DungeonDetailPage.xaml.cs:1829` |
| `MapTool` | T1 | T3, T5 | covered | **called** — `MapCanvasViewModel.cs`, `DungeonDetailPage.xaml.cs` |
| `DrawState` | T1 | T5 | covered | **called** — `DungeonDetailPage.xaml.cs` |
| `CanvasRect.Contains` | T1 | T5 | covered | **called** — `DungeonDetailPage.xaml.cs:1298` popup hit-zone guard |
| `ToolChange` | T1 | T5 | covered | **called** — `DungeonDetailPage.xaml.cs:1060` via `var change = _editor.SetActiveTool(...)`; the type name itself does not appear, so a whole-word search misses it |
| `MapEditorState.SetActiveTool` | T1 | T5 | covered | **called** — `DungeonDetailPage.xaml.cs:1060` |
| `MapEditorState.ResetDrawState` | T1 | T5 | covered | **called** — `DungeonDetailPage.xaml.cs:1256-1262` |
| `MapEditorState.BeginMoveDrag` / `EndMoveDrag` | T1 | T5 | covered | **called** — `DungeonDetailPage.xaml.cs:1395`, `:1674` |
| `MapEditorState.BeginObjectPress` | T1 | T5 | covered | **called** — `DungeonDetailPage.xaml.cs` |
| `MapEditorState.ConfirmObjectDragIfPastThreshold` | T1 | T5 | covered | **called** — `DungeonDetailPage.xaml.cs` |
| `MapEditorState.ClearObjectInteraction` | T1 | T5 | covered | **called** — `DungeonDetailPage.xaml.cs:1351`, `:1623`, `:1852` |
| `IDungeonEditingContext` (5 members) | T3 | T3 | covered | **called** — implemented on the root, consumed by all three children, which is the declared design. `context.ReportFailure` 14 sites (1+4+9); `NoDungeonIsLoaded` 13 sites (1+4+8) |
| `DungeonDetailViewModel.Dungeon` / `ErrorMessage` | T3 | T6 | covered | **called** — `DungeonDetailPage.xaml`, 3 distinct expressions over 7 occurrences |
| `DungeonDetailViewModel.Header` / `.FloorList` / `.Canvas` | T3 | T5 | covered | **called** — `DungeonDetailPage.xaml.cs`; `FloorList.Items` 10 hits, `Canvas.ActiveTool` 30, `Canvas.DrawingSession` 14 |
| `DungeonDetailViewModel.LoadTask` | T3 | T5 | covered | **called** — `DungeonDetailPage.xaml.cs:283`. **Newly added by T5**; the page never awaited it before. See `contradicts` C1 in `reviews/T5.md` |
| `DungeonDetailViewModel.LoadDungeonCommand` | T3 | T3 | covered | **called** — `ApplyQueryAttributes`, inside the implementing task by design |
| `DungeonDetailViewModel.GoBackCommand` | T3 | T6 | covered | **called** — `DungeonDetailPage.xaml:58` via `Shell.BackButtonBehavior` |
| `DungeonHeaderViewModel.SaveCommand` | T3 | T5 | covered | **called** — `DungeonDetailPage.xaml.cs:633` |
| `DungeonHeaderViewModel.Populate` / `FloorListViewModel.Populate` | T3 | T3 | covered | **called** — composition root during load, inside the implementing task by design |
| `FloorListViewModel.Items` + 4 commands | T3 | T5 | covered | **called** — `DungeonDetailPage.xaml.cs`, incl. `CollectionChanged` at `:214`/`:244` |
| `MapCanvasViewModel` properties + 8 commands + arg records | T3 | T5 | covered | **called** — `DungeonDetailPage.xaml.cs` throughout |
| `INavigationService.GoToAsync` | T4 | T4 | covered | **called** — `DungeonListViewModel.cs:104` |
| `INavigationService.GoBackAsync` | T4 | T3 | covered | **called** — `DungeonDetailViewModel.cs:190` |
| `Func<CreateDungeonPopup>` | T4 | T4 | covered | **called** — `DungeonListPage.xaml.cs.OnCreateClicked`, registered `MauiProgram.cs:138-139` |

**No operation is `not called`.** Six returned no external hit on the first mechanical pass; each was resolved
by inspection rather than recorded as a gap: `ToPolygon`, `ContainsPoint` and `Area` are called inside
`MapHitTester` and reached through `HitTestArea`, which has an external caller; `ToolChange` is consumed
through `var` so its type name never appears; and `IDungeonEditingContext`, `ReportFailure` and
`NoDungeonIsLoaded` are consumed by the three children inside T3, which is the consuming task the Gate 2
mapping declared.

A call site proves something calls the operation. It does **not** prove a user can reach it, and none of the
above is recorded as reachable.
