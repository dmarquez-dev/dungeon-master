# Coverage — 2026-08-26-settle-the-app-boundaries

Written at Gate 2, before the approval that froze `manifest.json`. The `Gate 2` columns are complete;
the `End state` columns and the `Implementing task` column were filled at Gate 3.

## A deviation in how the contract-operation end state was derived — read before the table

The prescribed search discards, in order: the definition site; **every hit in a file owned by the same
task that owns the definition site**; and hits inside test files. Applied literally here, **every one
of the 18 operations would read `not called`.**

That is not a finding about the work; it is the rule meeting a decomposition it does not fit. The
discard assumes a greenfield split where one task implements an operation and a *different* task
wires it up. This session is a **move refactor**: T2 relocated `TileCoordinateHelper`,
`MapDrawingSession` and `TileDistance` into `App.Core/Map/**` **and** owned the three App files that
already called them, because updating a caller's `using` line is inseparable from moving the type.
T3 has the same shape for `Presentation/**` and the two sidebar controls. The call sites are not new
wiring this session might have forgotten — they pre-date it, and criteria 12 and 15 of T2 and
criterion 13 of T3 each require behaviour to be unchanged, verified by the reviewers against
normalised diffs.

So the end state below records `called` with the file, and this note records what the literal rule
would have said. The purpose the discard serves — catching an operation with an implementer and no
consumer, where every task passes and the product still cannot do the thing — is not in play for
operations whose consumers are older than the session.

**What a call site still does not prove.** It proves something calls the operation. It does not prove
a user can reach it, and nothing below is recorded as reachable.

## Definition of done

| Item | Covering task | Gate 2 | End state |
|---|---|---|---|
| D1 — the three geometry types no longer live in `DungeonMaster.Application` (`MASTER-082`) | T2 | covered | met |
| D2 — Application's public surface is stated in a committed document, and what is not that surface has moved (`MASTER-087`) | T2 | covered | met |
| D3 — a `net10.0` `DungeonMaster.App.Core` exists, referenced by App and by Tests, with no MAUI package (`MASTER-026` half a) | T1 | covered | met |
| D4 — a test project referencing `DungeonMaster.App` **executes** MAUI-typed App code, with demonstrated discrimination (`MASTER-026` half b) | T1 | covered | met |
| D5 — the kit stops naming `AreaDto` / `FloorDto` / `PlacedObjectDto`, binds its own contracts, mapped at one boundary; feet-per-tile leaves the render method (`MASTER-027`) | T3 for the retyping; T2 for the feet arithmetic | covered | met |
| D6 — `docs/architecture-decisions.md` holds `ADR-001` recording macCatalyst build-verified / not run-verified, the five `TODO(macOS)` paths, `UI-036` and the solution platform accepted unchanged, and a revisit condition (`MASTER-030`) | T4 | covered | met |
| D7 — no regression: solution 0 warnings, both App TFMs 0 warnings, `DungeonMaster.Tests` `Failed: 0` | T1, T2, T3, T4 | covered | met |

7 items, 7 covered, 0 uncovered.

## Contract operations

| Operation | Implementing task | Consuming task | Gate 2 | End state |
|---|---|---|---|---|
| `TileCoordinateHelper.SnapToGrid` | T2 | T2 | covered | called — `FloorMapDrawable.cs:544`; `DungeonDetailPage.xaml.cs:1429,1443,1613` |
| `TileCoordinateHelper.ToPixel` | T2 | T2 | covered | called — `FloorMapDrawable.cs:304,312,328,329,518,523,534,541`; `DungeonDetailPage.xaml.cs:1860,1868` |
| `MapDrawingSession.Vertices` | T2 | T2 | covered | called — `DungeonDetailPage.xaml.cs:1250,1264,1265,1453,1742,1785` |
| `MapDrawingSession.IsActive` | T2 | T2 | covered | called — `DungeonDetailPage.xaml.cs:1089,1259` |
| `MapDrawingSession.PlaceVertex` | T2 | T2 | covered | called — `DungeonDetailPage.xaml.cs:1444` |
| `MapDrawingSession.UndoLastVertex` | T2 | T2 | covered | called — `DungeonDetailPage.xaml.cs:1257` |
| `MapDrawingSession.Reset` | T2 | T2 | covered | called — `DungeonDetailPage.xaml.cs:1022,1284,1759` |
| `MapDrawingSession.BuildOutline` | T2 | T2 | covered | called — `DungeonDetailPage.xaml.cs:1749` |
| `DrawSessionResult` (3 members) | T2 | T2 | covered | called — `DungeonDetailPage.xaml.cs:1446,1457` |
| `TileDistance.InFeet` | T2 | T2 | covered | called — `FloorMapDrawable.cs:555` |
| `SnapZoneCalculator.ToTile` | — (deleted) | — | uncovered — **deleted at Gate 2**: zero production callers (`grep -rn "SnapZoneCalculator" src --include=*.cs` → 7 lines, 2 files, all non-production), and it is the type's only member, so the type and `SnapZoneCalculatorTests.cs` go with it | deleted — the type, its file and its 5 tests are gone |
| `MapDrawingSession.LastVertex` | — (deleted) | — | uncovered — **deleted at Gate 2**: zero production callers, one test assertion at `MapDrawingSessionTests.cs:72` rewritten to `Vertices[^1]` | deleted — one test assertion rewritten to `Vertices[^1]` |
| `AreaItem.HasNotebookContent` | T3 | T3 | covered | called — `AreaSidebarItem.xaml.cs:80` |
| `FloorItem.HasNotebookContent` | T3 | T3 | covered | called — `FloorSidebarItem.xaml.cs:153` |
| `DungeonDetailPresentationMapper.ToItems` (public) | T3 | T3 | covered | called — `DungeonDetailViewModel.cs:179` |
| `DungeonDetailPresentationMapper.ToItem(AreaDto)` (internal) | T3 | T3 | uncovered by an external caller — **made `internal` at Gate 2**; called by `ToItem(FloorDto)` and directly tested via `InternalsVisibleTo` | called in-assembly by `ToItem(FloorDto)`; directly tested. No external caller by design |
| `DungeonDetailPresentationMapper.ToItem(PlacedObjectDto)` (internal) | T3 | T3 | uncovered by an external caller — **made `internal` at Gate 2**; called by `ToItem(FloorDto)` and directly tested | called in-assembly by `ToItem(FloorDto)`; directly tested. No external caller by design |
| `DungeonDetailPresentationMapper.ToItem(FloorDto)` (internal) | T3 | T3 | uncovered by an external caller — **made `internal` at Gate 2**; called by `ToItems` and directly tested | called in-assembly by `ToItems`; directly tested. No external caller by design |

**End state: 18 operations. 13 `called` with a named external call site; 3 called in-assembly only
by design (the `internal` overloads narrowed at Gate 2, each directly tested); 2 deleted at Gate 2.
No operation is `not called`, and none was left without a consumer.**

The 13 external call sites were located by whole-word content search over the session's changed
files, `git diff --name-only 702ff76bd73e71f115600406855a2c6bb0821e4a..dir/2026-08-26-settle-the-app-boundaries`,
excluding `bin/`, `obj/` and `src/DungeonMaster.Tests/`. See the deviation note at the top of this
file for why the owns-scoped discard was not applied.

**One consequence the Gate 2 decision did not anticipate, recorded here rather than discovered later.**
`MapDrawingSession.LastVertex` has zero *production* callers but one test caller, so its deletion is
not free. T2's criteria prescribe the exact rewrite. Separately, `UndoLastVertex` is a different
member with a real production caller at `DungeonDetailPage.xaml.cs:1256` and is explicitly not
deleted — the two names differ by one character and a careless grep conflates them.

**The `internal` decision required touching T1.** `internal` members of `DungeonMaster.App.Core` are
invisible to `DungeonMaster.Tests` without `[assembly: InternalsVisibleTo("DungeonMaster.Tests")]`.
That file belongs at the project root, which is T1's ownership set, not T3's. T1 creates it; T3
verifies it exists and reports a blocked dependency rather than creating it or widening the overloads
back to `public`.
