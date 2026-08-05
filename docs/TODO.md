# TODO

Deferred work items with enough context to pick up cold. Newest first.

---

## Evaluate the dungeon-detail data refresh path

**Raised:** 2026-08-05, during the floor-level ownership refactor
**Area:** `DungeonMaster.App` — `DungeonDetailViewModel`, `DungeonDetailPage`
**Type:** performance / code health — no known user-visible bug

The designer reloads the entire dungeon aggregate far more often than it needs to, and some of those reloads are redundant with each other. Worth measuring before optimising: the app is local-SQLite and single-user, so the real cost may be small. The point of this item is to find out, not to assume.

### What to measure first

Instrument `GetDungeonDetailQuery` with a stopwatch and a call counter, then exercise the designer normally — create a floor, draw a few areas, place a dozen objects, drag some around, rename an area, delete an area. Record query count and total time. That number decides whether the rest of this item is worth doing.

A dungeon with several floors, many areas, and hundreds of placed objects is the case that would hurt, since `GetDetailAsync` projects the whole tree every time. Consider seeding a large fixture to see where it starts to bite.

### Known redundancies

1. **Double round-trips on several page handlers.** `DeleteAreaAsync` calls `RemoveAreaCommand` — whose ViewModel method already ends in `LoadDungeonAsync` — and then calls `RefreshFloorAsync()`, which reloads again. Two full `GetDungeonDetailQuery` executions per delete. `EraseAreaAsync` has the same shape, and it is worth auditing every `RefreshFloorAsync()` call site for the same pattern.

2. **`LoadDungeonAsync` rebuilds the sidebar O(n) times per load.** It does `Floors.Clear()` then `Floors.Add(floor)` per floor. Each mutation raises `CollectionChanged`, and the page's handler calls `RebuildFloorList()`, which tears down and recreates every `FloorSidebarItem` and every nested `AreaSidebarItem`. A dungeon with 10 floors rebuilds the whole sidebar 11 times per reload. Batching the update — or replacing the collection wholesale and rebuilding once — would collapse that.

3. **Whole-aggregate reload for single-field edits.** Renaming an area reloads the entire dungeon so one label can change. A targeted DTO update, or a narrower query, would avoid it. Note the trade-off: the whole-reload approach is what makes cached DTOs reliably fresh, and hand-patching DTOs is exactly what caused the rename-propagation bug fixed on 2026-08-05. Any change here must keep every cached `AreaDto`/`FloorDto` reader consistent — do not reintroduce partial patching without a plan for that.

### Dead code to remove

- `AreaSidebarItem.UpdateAreaName(string)` and `FloorSidebarItem.UpdateAreaName(Guid, string)` — both became unreachable when area renaming moved to the reload path. They mirror `UpdateFloorName`, so decide whether to keep them as deliberate control API or delete them.
- Re-check for others after the lifecycle fix settles; the refactor removed several call sites.

### Related

The page lifecycle defect fixed on 2026-08-05 (a modal popup fired `OnNavigatedFrom` and permanently tore down `Floors.CollectionChanged`) means the sidebar previously stopped rebuilding at all after any confirm dialog. Any refresh-path work should re-verify that subscriptions survive popups, and should not reintroduce a single-subscribe-in-constructor pattern.

---

## Metadata editing UX for floors and areas

**Raised:** 2026-08-05
**Area:** `DungeonMaster.App` — designer chrome
**Type:** design question, not yet specified

Floor names are edited in a toolbar field; area names in a second toolbar field that appears only while an area is focused; areas and floors are deleted from the right sidebar. So the same object is renamed in one place and deleted in another.

`Area` also carries `Description` and `Notes` that the UI barely surfaces, and a DM writing room descriptions is a core use case a rename-only affordance cannot serve.

Options worth surveying before building:

- **Inline edit in the sidebar row** — the tree-UI convention (Figma layers, VS Code explorer). Puts every operation on an object in one place.
- **A properties panel** — select an object, its metadata appears in a dedicated pane. Scales to description and notes.
- **Keep contextual toolbar editing** but make floors and areas consistent with each other.

Blocked on a design pass, not on implementation.

---

## Reintroduce area outline redraw

**Raised:** 2026-08-05
**Area:** `DungeonMaster.App` — designer
**Type:** feature, roughly specified

The Redraw button was removed with the area-context toolbar during the floor-level ownership refactor. Re-drawing an existing area's outline in place is currently impossible; the workaround is to erase the area and draw a new one, which loses its name, description, and notes.

Proposed behaviour: if an area is focused, clicking the Draw tool enters redraw mode for that area; otherwise Draw creates a new area.

Caveat to design around: that makes one control do two different things based on selection state, which is the kind of modality the refactor deliberately removed. Selection is now explicit and visible, so it is defensible — but the in-progress drawing state needs to make it obvious that an existing outline is being replaced rather than a new area drawn. Sequence this after the metadata UX decision, since where selection lives affects how redraw should feel.
