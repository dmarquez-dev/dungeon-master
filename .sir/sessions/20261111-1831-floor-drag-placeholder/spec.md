# Floor drag placeholder

Replace the floor sidebar's sized drag placeholder with an insertion line that marks where the
reorder will really land, dim the dragged card instead of hiding it, and so remove the self-drop gap.

## Context

Dragging a floor card in `DungeonDetailPage` hides the dragged row (`Opacity = 0`,
`DungeonDetailPage.xaml.cs:982` at `12ef199`) and inserts a sized `Border` into
`FloorListContainer.Children` by index (`:2598`, `:2658`). That container is now
BindableLayout-bound (`DungeonDetailPage.xaml:693-698`). Three backlog rows (20261106-2050-detail-page-trio
twice, 2026-09-11-make-the-floor-sidebar-operable once; see `discussion.md`) report:

- the self-drop gap: drop a card on its own slot and a card-height empty space shows, with no placeholder
- the index-based insert into a bound container
- the open question of how the indicator can show the true landing position without changing
  the visual tree under the pointer

A restore-on-drag-end fix was tried and discarded in 20261106-2050-detail-page-trio
(`reviews/T3.md` round 2). The architect's diagnosis (contract C1 section 1) rates this cause
**probable**: the gap is the hidden dragged card itself during the drag, which no post-drop
restore can cure. The design does not depend on that cause being right. No automated test can
start a real WinUI drag (`FloorSidebarAutomationTests.cs:26-31`), so a person confirms the
behaviour at Gate 3.

## Definition of done

1. Dropping a floor on its own slot leaves no empty space, during or after the drag (Gate 3 human
   check H1).
2. The drop indicator is a line outside `FloorListContainer`, and no drag handler inserts, removes
   or reorders that container's children.
3. The dragged card is dimmed to 0.4, never hidden. Every terminal path of a drag, and the next
   drag start, restores every row to full opacity (contract C1 V1).
4. The line's position comes from one pure App.Core function. It is tested against the drop
   resolver `FloorReorderNearest` and against the domain's `Dungeon.ReorderFloor`.
5. The page builds its drop bands in one place, used by both the indicator and the list drop.
6. The line is hidden on every terminal path of a drag (contract C1 V6).
7. The placeholder machinery is gone: its fields, methods, card events, and
   `FloorListBandKind.TransientDropTarget`.
8. What a drop means is unchanged, and the existing drop tests still pass.
9. Every comment listed in contract C1 section 4 is true of the new code.
10. `dotnet build src/DungeonMaster.sln` reports 0 warnings and 0 errors.
11. All three test assemblies pass. Each total is reconciled against its base total (Assumptions
    row 1).
12. No file a task writes contains `.sir/` or cites a backlog line number.
13. A normal reorder, up or down, lands where the line showed, including a drop on a seam (H2, H3,
    H5).
14. The line tracks the landing position and no card moves (H4).
15. Leaving the list or cancelling the drag clears the line (H6).

## Scope

The floor drag-and-drop interaction in `DungeonDetailPage` and `FloorSidebarItem`, and the App.Core
types the drop decision lives in (`FloorReorderDrop`, `FloorReorderNearest`, new
`FloorDropIndicator` and `FloorDropLanding`). Two tasks run in series: T1 builds the App.Core
resolver, and T2 builds the view.

### Not in scope

- Removing the band-kind concept entirely: `FloorListBand` keeps its `Kind` parameter.
- Renaming the `DmDragPlaceholderStroke` and `DmDragPlaceholderFill` tokens. After this session
  `DmDragPlaceholderFill` and `DmTheme.DragPlaceholderFill` are unreferenced by product code.
- Whether the new `Grid` changes the element that `FloorSidebarAutomationTests` reaches through its
  `ScrollViewportXPath` (`:95`). UiTests are outside the solution and are not run.
- Auto-scrolling the list during a drag.
- The other floor-sidebar rows: the truncated floor name, drag-handle focusability, the
  `AreaSidebarItem` hover, and the expand-icon spacing.

## Assumptions

| Question | Answer | Rung | Evidence |
|---|---|---|---|
| What are the base test totals that C2-24 reconciles against? | DungeonMaster.Tests 976, DungeonMaster.App.Tests 137, DungeonMaster.App.UnitTests 217. All passing, 0 failed | repo | Orchestrator probe at `12ef199`: `dotnet test C:/dm-wt/20261111-1831-floor-drag-placeholder/src/DungeonMaster.sln --no-build` (`state.json` `verification`) |
| What is the self-drop gap? | Probably the hidden dragged card during the drag. Rated probable, not established. The design removes it whichever way | repo + external | C1 section 1, E1-E8 |
| How should the dragged card look during a drag? | It stays in its slot, dimmed to 0.4, with an insertion line in the middle of the gap. This was the user's Gate 2 decision D1, taken after a best-practice review | external | Atlassian Pragmatic drag and drop design guidelines (https://atlassian.design/components/pragmatic-drag-and-drop/design-guidelines): the item "should stay in place and dim to 40% opacity", and the line sits "in the middle of the gap". NN/g (https://www.nngroup.com/articles/drag-drop/) prefers items moving aside, which the Gate 1 no-mutation decision excludes |
| How does T1 test the "band is not a FloorCard" row once T2 deletes `TransientDropTarget`? | It builds the band from `(FloorListBandKind)int.MaxValue` and never names the deleted member (Gate 2 decision D2) | repo | `FloorReorderNearest.cs:7-20` (the enum's two members at `12ef199`); C1-2, C2-19 |
| Do the exact-count needles count documentation lines? | No. C1-7 and C2-15 count code lines only, through `grep -v '///'` (Gate 2 decision D3) | repo | C1-12 and C2-26 require XML docs on the same members |
| Can two tasks build at once in one worktree? | No: they share `obj/` and `bin/`, and DungeonMaster.Tests builds App.Core. T2 runs after T1 | repo | `src/DungeonMaster.Tests/DungeonMaster.Tests.csproj:24` |
| Can a peer read an injected defect? | No. T1 injects only into its own `FloorDropIndicator.cs` and `FloorReorderDrop.cs`, T2 only into its own page file, and the chain is serial | repo | manifest `dependsOn` |
| Does a self-drop show a line? | No. Dropping a floor on itself is already a silent no-op | repo | `FloorReorderDrop.cs` self-drop row (`draggedFloorId == targetId`); C1 section 2.1 |
| Delete the band-kind concept, or only `TransientDropTarget`? | Only `TransientDropTarget`. The enum keeps `FloorCard`, so every commit builds | assumption (architect) | C1 V8; the kind appears at 4 sites at `12ef199` (`grep -rn TransientDropTarget src --include=*.cs --exclude-dir=obj --exclude-dir=bin`) |
| What colour does the line use? | The `DmDragPlaceholderStroke` token. Both placeholder tokens stay | repo | `src/DungeonMaster.App/Resources/Theme/Tokens.xaml:251-252`; `ThemeDictionaryTests.cs:43` |
| Does a XAML binding for the line's inset need its own `x:DataType`? | Yes. Under `TreatWarningsAsErrors`, a miss in a compiled binding fails the build | repo | `DungeonDetailPage.xaml:49-63` |
| Does the repo-wide test gate run UiTests? | No. The solution holds 8 projects and UiTests is not one of them | repo | `grep -c 'Project(' src/DungeonMaster.sln` prints 8; `src/DungeonMaster.App.UiTests/README.md` |
| Which MAUI version are the platform facts read against? | `Microsoft.Maui.Controls` 10.0.41 | repo | `Directory.Packages.props:26` |

## Contracts

- [`contracts/C1-floor-drop-indicator.md`](contracts/C1-floor-drop-indicator.md). Section 1 is the
  diagnosis. Section 2 is the App.Core interface: T1 declares it and T2 calls it. Section 3 is the
  view rules V1-V8. Section 4 lists the comments that become false. Section 5 is the Gate 3 human
  checks H1-H6.

## Open questions

- Does the card stay hidden after release, as well as during the drag? This is undetermined (C1
  section 1). Gate 3 human check H1 settles it, and the user makes it at Gate 3.
