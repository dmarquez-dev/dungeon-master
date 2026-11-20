# floor-drag-placeholder

Fix the floor drag-and-drop placeholder in `DungeonDetailPage`: cure the self-drop gap, make the
drop indicator robust to the BindableLayout-bound container, and settle how the indicator shows the
true landing position without mutating the visual tree mid-`DragOver`.

## Context

The backlog's five scheduled headings (`.sir/backlog.md:5`, `:7`, `:9`, `:11`, `:13`) are all
closed in the register: MASTER-126, 128, 137, 139, 143, 110, 180, 157, 178, 142, 081 and 130 read
done or DELIBERATELY UNCHANGED in `docs/audit/00-master-index.md`. The only register row of 181 with
an empty status cell, MASTER-158 (`00-master-index.md:346`), is permanently deferred at
`.sir/backlog.md:156`. The next unit therefore came from the unscheduled rows; this is the most
recent user-visible App defect.

The three backlog rows, re-checked against `12ef199` at Gate 1 (line numbers are
`.sir/backlog.md` at that commit):

- `:175` (20261106-2050-detail-page-trio, xref MASTER-108) -- dragging a floor card and dropping it
  on its own position leaves the card invisible (a card-height gap that never clears) and shows no
  placeholder. `OnFloorDragStarted` (`src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:974`)
  sets the dragged row to Opacity 0. The same single-restorer logic exists at `442b6f5:917-922`, so
  the defect predates that session. A tested revision restoring the card on a declined drop and at
  every drag end did NOT cure it on re-observation and was discarded
  (`.sir/sessions/20261106-2050-detail-page-trio/reviews/T3.md`, round 2): the cause is not the
  missing `DropCompleted` alone, and is still undetermined.
- `:176` (same session, T3 review A4) -- `ShowDragPlaceholder` (`DungeonDetailPage.xaml.cs:2553`)
  computes `insertIdx = FloorListContainer.Children.IndexOf(target)` (`:2598`) and calls
  `FloorListContainer.Children.Insert(insertIdx, _dragPlaceholder)` (`:2658`) into a container that
  is now BindableLayout-bound; a floor Replace during a drag could land the placeholder at the wrong
  child index. The row says to take it with `:175`.
- `:35` (2026-09-11-make-the-floor-sidebar-operable) -- settle, before building, how the floor drop
  indicator can always show the resolver's true landing position without mutating the visual tree
  under the pointer mid-`DragOver`, for example an insertion line instead of a sized placeholder.

Observe the running app with
`dotnet run --project C:/Users/dmarq/Development/Repos/dungeon-master/src/DungeonMaster.App/DungeonMaster.App.csproj -f net10.0-windows10.0.19041.0`
(from `:175`).

## Decisions

- Track: Standard (user, Gate 1).
- Scope: the three rows above, as one concern -- the placeholder mechanism, its indicator design, and
  the self-drop defect it plausibly causes. All three rows close with this session.
- Diagnose the self-drop defect's root cause before choosing a fix; the previous restore-on-drag-end
  fix is known not to cure it.
- The indicator design (`:35`) is settled at spec time, with the insertion-line approach as the
  row's own suggested option; the chosen design must not mutate `FloorListContainer`'s children
  under the pointer during `DragOver`, or the spec must say why it still must.
- Done is observed, not inferred: Gate 3 carries human checks in the running app for the self-drop
  (no gap, card visible), a normal reorder still landing where the indicator showed, and the
  indicator tracking the true landing position.
- Out of scope: the other floor-sidebar rows (truncated floor name, drag-handle
  focusability, `AreaSidebarItem` hover, expand-icon spacing) and anything outside floor drag and drop.
