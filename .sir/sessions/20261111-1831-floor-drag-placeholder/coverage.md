# Coverage

| Item | Covering task | Gate 2 | End state |
|---|---|---|---|
| 1. Self-drop leaves no empty space (H1) | orchestrator | covered (orchestrator). No command can start a real drag (C1 E8), so Gate 3 human check H1 decides it, recorded in `report.md`. T2's C2-1 and C2-3 are structural proxies and do not close this row | met: user reported human check H1 passed at Gate 3, observed in the app built from the session worktree (`dotnet build C:/dm-wt/20261111-1831-floor-drag-placeholder/src/DungeonMaster.App/DungeonMaster.App.csproj -t:Run -f net10.0-windows10.0.19041.0`) |
| 2. Indicator is a line outside the container, no child mutation | T2 (C2-2, C2-7, C2-10) | covered | met (T2 C2-2, C2-7, C2-10) |
| 3. Dragged card dimmed to 0.4, never hidden; every terminal path and the next drag start restore full opacity | T2 (C2-1, C2-3 to C2-6, C2-8, C2-28) | covered. This is the structural half; whether the card is visible on screen is H1 and H4 at Gate 3 | met (T2 C2-1, C2-3 to C2-6, C2-8, C2-28); on-screen visibility is H1 and H4 |
| 4. Line position from one pure App.Core function, tested against resolver and domain | T1 (C1-2 to C1-5, C1-9) | covered | met (T1 C1-2 to C1-5, C1-9) |
| 5. One band builder for indicator and list drop | T2 (C2-14) | covered | met (T2 C2-14) |
| 6. Line hidden on every terminal path | T2 (C2-4 to C2-6, C2-17, C2-18) | covered | met (T2 C2-4 to C2-6, C2-17, C2-18) |
| 7. Placeholder machinery and `TransientDropTarget` gone | T2 (C2-9, C2-19) | covered | met (T2 C2-9, C2-19) |
| 8. Drop meaning unchanged, existing drop tests pass | T1 (C1-8), T2 (C2-22) | covered | met (T1 C1-8, T2 C2-22) |
| 9. C1 section 4 comments true | T1 (C1-11), T2 (C2-20, C2-21) | covered | met (T1 C1-11, T2 C2-20, C2-21) |
| 10. Solution builds 0 warnings 0 errors | T2 (C2-23) | covered | met (T2 C2-23) |
| 11. Three test assemblies pass, totals reconciled | T2 (C2-24) | covered | met (T2 C2-24) |
| 12. No `.sir/` path or backlog line number in written files | T1 (C1-14), T2 (C2-27) | covered | met (T1 C1-14, T2 C2-27) |
| 13. Reorder up, down and seam lands where the line showed (H2, H3, H5) | orchestrator | covered (orchestrator). Gate 3 human checks H2, H3 and H5, recorded in `report.md` | met: user reported human check H2, H3, H5 passed at Gate 3, observed in the app built from the session worktree (`dotnet build C:/dm-wt/20261111-1831-floor-drag-placeholder/src/DungeonMaster.App/DungeonMaster.App.csproj -t:Run -f net10.0-windows10.0.19041.0`) |
| 14. Line tracks landing, no card moves (H4) | orchestrator | covered (orchestrator). Gate 3 human check H4, recorded in `report.md` | met: user reported human check H4 passed at Gate 3, observed in the app built from the session worktree (`dotnet build C:/dm-wt/20261111-1831-floor-drag-placeholder/src/DungeonMaster.App/DungeonMaster.App.csproj -t:Run -f net10.0-windows10.0.19041.0`) |
| 15. Leave or cancel clears the line (H6) | orchestrator | covered (orchestrator). Gate 3 human check H6, recorded in `report.md` | met: user reported human check H6 passed at Gate 3, observed in the app built from the session worktree (`dotnet build C:/dm-wt/20261111-1831-floor-drag-placeholder/src/DungeonMaster.App/DungeonMaster.App.csproj -t:Run -f net10.0-windows10.0.19041.0`) |

| Operation | Implementing task | Consuming task | Gate 2 | End state |
|---|---|---|---|---|
| `FloorDropIndicator.Locate` | T1 | T2 (the page's show method, C2-15) | covered; predicted called | called, `DungeonDetailPage.xaml.cs:2638`; prediction confirmed |
| `FloorDropLanding` | T1 | T2 (C2-15) | covered; predicted called | called, `FloorDropIndicator.cs:77` (repo-wide fallback search; not present at `12ef199`); the page reads the value through `var landing` at `DungeonDetailPage.xaml.cs:2638`, which a name search cannot see; prediction confirmed |
| `FloorReorderDrop.TryReadDraggedFloorId` | T1 | T2 (the page's show method, C2-15); also `Decide` (C1-7) | covered; predicted called | called, `DungeonDetailPage.xaml.cs:2630`; prediction confirmed |
