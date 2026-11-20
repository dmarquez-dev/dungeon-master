# Coverage — 2026-08-28-the-control-kit

Gate 2 columns written 2026-08-28, before the approval that freezes `manifest.json`. End state written at Gate 3.

## Definition of done

| Item | Covering task | Gate 2 | End state |
|---|---|---|---|
| 1. `NotNullConverter` rejects the unresolved-binding sentinel (`MASTER-007`) | T1 | covered |  met |
| 2. Converters return null from `ConvertBack` (`MASTER-034`) | T1 | covered |  met |
| 3. Dungeon-list Edit/Delete reachable, mechanism demonstrated (`MASTER-159`) | T2 | covered |  met — found already fixed; no code changed |
| 4. Touch-target minimum resolves from one token, ratchet guards it (`MASTER-036`) | T3 | covered |  met |
| 5. `Gold` and `ScrollGoldLight` resolve distinct keys (`MASTER-037`) | T3 | covered |  met |
| 6. `PaletteThumb` reads `StrokeHairline`; migration list and its doc comment corrected (backlog) | T3 | covered |  met |
| 7. `DmSearchBar` focus through visual states (`MASTER-060`) | T4 | covered |  met |
| 8. `NotebookView` theme lookup through an accessor, missing key diagnosable (`MASTER-061`) | T5 | covered |  met |
| 9. Sidebar visual state through bindable properties (`MASTER-098`) | T6 | covered |  **not met in part** — criterion 11's DragOver observation unobtainable; MASTER-098 records part done |
| 10. `FloorSidebarItem` stops forwarding `AreaSidebarItem`'s events (`MASTER-099`) | T6 | covered |  met |
| 11. `FloorSidebarItem` card body carries `HandCursorBehavior` (`MASTER-161`) | T6 | covered |  **not met** — remedy shipped, user observed no hand cursor 2026-09-01; MASTER-161 open |
| 12. `DrawAreasViewMode`/`DrawAreasEditMode` collapsed (`MASTER-035`) | T7 | covered |  met |
| 13. `FloorMapDrawable` exposes read-only collections (`MASTER-059`) | T7 | covered |  met |
| 14. `Draw` allocation removed, measured before/after (`MASTER-097`) | T7 | covered |  met |
| 15. `DmInlineEditor` responsibilities split, `ClearFocusGlow` made explicit (`MASTER-105`) | T8 | covered |  met |
| 16. No `propertyChanged` handler writes an already-bound property (`MASTER-077`) | T8 | covered |  met |
| 17. Index records every Session 5 row; `094`/`108` partial, never done | T9 | covered |  met |
| 18. Build 0/0 both TFMs; both test assemblies green, reported per assembly | T1–T9 | covered |  met |
| `CreateDungeonPopup.xaml` per-binding `x:DataType={x:Null}` + falsified comment at `:15-47` | T10 | uncovered — became a task at Gate 2 (E1) |  met — 6 of 7 bindings compiled, red step both directions; comment corrected |
| Dead-by-coupling guard, `DungeonDetailPage.xaml.cs:984-992` | — | uncovered — accepted out of scope at Gate 2 (E1) |  out of scope |
| Silent `Guid.Empty` exit, `MapCanvasViewModel.cs:213-216` | — | uncovered — accepted out of scope at Gate 2 (E1) |  out of scope |
| Lost `` Was `_field` `` provenance, `contracts/app-core-editor.cs` | — | uncovered — accepted out of scope at Gate 2 (E1) |  out of scope |
| Unreachable `n < 3` guard, `AreaOutlineGeometry.cs:148` (record-only; "do not fix") | — | uncovered — accepted out of scope at Gate 2 (E1) |  out of scope |

The five rows above are the inherited backlog items the decomposition brief named as required reading but did not scope — escalation **E1**, resolved by the user at Gate 2. One became **T10**; the other four stay open in `.dir-sdlc/backlog.md` and T9 records them as still-open rather than closed.

## Contract operations

| Operation | Implementing task | Consuming task | Gate 2 | End state |
|---|---|---|---|---|
| `DmTheme.StrokeHairline` | T3 | T3 (`PaletteThumb.CreateShell`) | covered |  **not called** — the only call site, `PaletteThumb.cs:95`, is inside the defining task's own ownership set, so this check cannot confirm the application reaches it. See the note below. |
| `DmTheme.ColorByKey(string)` | T3 | T5 (`NotebookView` ribbon brush) | covered |  **called** — `Controls/NotebookView/NotebookView.xaml.cs:477`, owned by T5, a different task from the definition site's owner |
| Token `DmTouchTargetMinimum` | T3 | T3 (XAML setter sites) | covered |  **called** — `Controls/DmSearchBar/DmSearchBar.xaml:36`, which T4 owns independently of T3; the other 11 binding sites are all inside T3's own ownership |
| New distinct gold token for `ScrollGoldLight` | T3 | T3 | covered |  **called** — `Controls/FloorSidebarItem/FloorSidebarItem.xaml.cs:246` and `Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:590`, both owned by T6 |
| dm-card invariants 1–2 — buttons reachable, handlers run | T2 | T2, re-verified by T8 | covered |  **called** — re-verified by T8's reviewer with real clicks: Edit reached the Dungeon Editor, Delete raised the confirm popup |
| dm-card invariant 3 — `TrailingContent` renders in the `DataTemplate` | T2 | T2 (`DungeonListPage.xaml`) | covered |  **called** — `Views/DungeonListPage/DungeonListPage.xaml` DataTemplate, unmodified since T2 |
| dm-card invariant 4 — three `AutomationId`s present | T2 | T2, T8 | covered |  **called** — all three `AutomationId` strings present in `DungeonListPage.xaml`, verified by T8's reviewer |
| dm-card invariant 5 — Start-Session button stays unwired | T2 | *negative invariant, no caller by design* | covered |  *negative invariant* — `StartSessionButton` still carries no `Clicked` or `Command`; correctly no caller |
| audit-status §2 vocabulary and §5 record corrections | T9 | T9 | covered |  **called** — T9 wrote only §2 forms; reviewer's `Counter` over all 166 rows found no sixth form |
| audit-status §6 `AUDIT OUTCOMES` block | T9 | T9 (produced by T1–T8) | covered |  **called** — 29 rows written from producer blocks in `reviews/T1.md`–`T10.md` |

`Implementing task` is empty at Gate 2 by design; it is derivable only once Gate 3's search locates each definition site.

**Disclosed weakness, not a gap.** `ColorByKey`'s error cases (a) and (c) are argued rather than observed; only case (b) carries a demanded observation. Making (a) observable would require a read before `Application.Current` is set, which this session cannot stage.


## Gate 3 note on `not called`

`DmTheme.StrokeHairline` records `not called`, and the reason is the check working as designed rather than a defect. Its only call site is `Views/DungeonDetailPage/PaletteThumb.cs:95`, which T3 owns alongside the definition site `Theme/DmTheme.cs` — so the search discards it, per the rule that a sibling-file call inside the implementing task is not evidence the application reaches the operation.

**The operation is in fact reached**: `PaletteThumb.CreateShell` is called by the palette, and T3's reviewer independently confirmed the token's declared type matches (`Tokens.xaml:574`, `x:Double`), closing the runtime-only failure mode. The `not called` verdict records that *this session's coverage check cannot prove it*, which is a weaker and more honest claim than the alternative.

Two other rows carry a related weakness, stated rather than hidden: `DmTouchTargetMinimum` survives the discard only because `DmSearchBar.xaml` is owned by T4 as well as T3, and the remaining eleven binding sites are all inside T3's own ownership.
