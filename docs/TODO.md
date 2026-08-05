# TODO

Deferred work items with enough context to pick up cold. Newest first.

---

## macCatalyst has no text-input guard for keyboard shortcuts

**Raised:** 2026-08-05, during the notebook metadata work
**Area:** `DungeonMaster.App` — `DungeonDetailPage.IsTextInputActive`
**Type:** cross-platform gap — blocks shipping macCatalyst, harmless on Windows

`OnKeyDown` gates the designer's single-letter tool shortcuts (D for draw, and the rest) behind `IsTextInputActive()`. On Windows that method asks WinUI's `FocusManager` whether the focused element is a `TextBox`, which correctly covers the notebook's `Editor` panes. On macCatalyst it returns `false` unconditionally, so **typing a description into the notebook will also fire tool shortcuts**.

This is not a regression introduced by deleting `_isTextInputFocused` with the Lore drawer. That flag was only ever maintained by the four editors the notebook replaced; nothing would have set it for `NotebookView` either. The gap arrived with the notebook and would have existed regardless.

Two ways to close it, either is fine:

- Walk `UIResponder.IsFirstResponder` over the page's platform view and return true when the first responder is a `UITextView` or `UITextField`.
- Have `NotebookView` surface focus/blur events — it already tracks editor focus internally for its autosave-on-unfocus — and let the page hold a flag again, this time fed by the control that actually owns the input.

The second is probably better, since it keeps the page out of platform APIs and would also cover any future control that hosts text.

Windows is unaffected and needs no change.

---

## Designer async-lifecycle hardening

**Raised:** 2026-08-05, during the investigation behind the popup lifecycle fix
**Area:** `DungeonMaster.App` — `DungeonDetailPage`
**Type:** latent bugs — same family as the defect already fixed, none confirmed reproducible yet

The fixed defect was a page handler that kept operating on state the page no longer owned after a modal popup tore down its subscriptions. Auditing for that shape turned up five more places where the designer mutates UI state across an `await` without re-checking that the state it captured is still live. None of them has a confirmed repro, so they were deliberately left out of that fix and collected here for a dedicated refinement branch rather than being folded into an unrelated change.

The common failure mode to keep in mind while reading these: every write path ends in a full `LoadDungeonAsync`, which clears and refills `Floors`, which rebuilds the entire sidebar. Anything holding a `FloorDto`, an `AreaDto`, a sidebar item, or an object id from before that reload is holding something detached.

### 1. Fire-and-forget handlers mutate UI state after awaits

`EraseAreaAsync`, `PlaceObjectAsync`, `FinishDrawing`, `MoveObjectAsync`, and `DeleteSelectedObjectAsync` are all `async void` or otherwise fire-and-forget. Each awaits a command and then writes page state — `_drawable`, canvas invalidation, toolbar state — without re-validating that `_selectedFloorId` and `_drawable.Floor` still refer to live data. A floor deletion, a floor switch, or a second overlapping operation landing during the await leaves the continuation writing into a floor that is no longer selected. Being `async void` also means any exception thrown after the first await escapes to the platform unhandled rather than surfacing.

Fix shape: capture the floor id before the await, re-check it against `_selectedFloorId` after, and bail out of the UI mutation if it changed — the same guard now used in `OnAreaNameToolbarCommitted`.

### 2. `RefreshFloorAsync` does not re-validate object selection

`RefreshFloorAsync` reloads the dungeon and re-points `_drawable.Floor` at the refreshed projection, but leaves `_selectedObjectId` and `_hoveredObjectId` untouched. If the reload dropped the object those ids name — deleted elsewhere, or removed with its floor — the ids survive as references to nothing. `RefreshMap` already does the equivalent check for `_focusedAreaId` and clears it when the area is absent from the refreshed floor; `RefreshFloorAsync` should do the same for object ids.

### 3. Popup awaits inside closures owned by a rebuilt sidebar item

Highest risk of the three, and the same shape as the defect already fixed.

`DeleteAreaAsync` and the floor-delete handler registered in `RebuildFloorList` both `await Navigation.ShowConfirmPopupAsync(...)` from inside a closure captured by a `FloorSidebarItem`. Showing the popup can trigger navigation lifecycle events on the page, and any reload during the await runs `RebuildFloorList`, which clears `FloorListContainer` and constructs entirely new sidebar items. The closure keeps running on the old, detached item — and then mutates page state (`_selectedFloorId`, `_drawable`, toolbar visibility) from it, referencing a `FloorDto` captured before the rebuild.

Fix shape: hoist the confirm-and-mutate work out of the sidebar item's closure into a page-level method that takes only ids, and re-resolve the floor and area from `_viewModel.Floors` after the popup returns rather than trusting the captured DTOs.

### 4. Keyboard behaviour attached before the page has a handler

`Behaviors.Add(_keyboardBehavior)` runs in the page constructor, before a platform handler exists. It happens to work because the behaviour defers its platform wiring until the handler is created, but nothing states that requirement and nothing enforces it — the ordering is implicit. Either attach it from `OnHandlerChanged`, or document the dependency at the attach site so a future reorder does not silently break keyboard shortcuts.

### 5. `OnNotebookSaveRequested` is a timer-driven `async void` write path

`DungeonDetailPage.OnNotebookSaveRequested` is a sixth handler in this family, but it differs in kind from the other five: every other one is user-gesture-driven and therefore serialised by the user's own input, whereas this one fires on the notebook's 900ms debounce timer. `Dispatcher` resolves handlers from the root `IServiceProvider`, and `AppDbContext` is registered with `AddDbContext` — scoped, but effectively a singleton given MAUI's single root scope. If a debounce commit is mid-`await` and the user then closes the notebook, or blurs an editor after typing more, a second write could start on the same `DbContext` — "A second operation started on this context; a second operation cannot be started while the first is not complete." Not reproduced statically; would need the app running with fast typing followed immediately by Close to confirm.

Worth noting this handler is otherwise the best-behaved of the family: it re-resolves `_drawable.Floor` by id rather than reusing a captured DTO, so the fix here is narrower than for 1–3 — likely serialising the debounce-triggered save against a concurrent close/blur commit rather than the broader re-validate-after-await pattern used elsewhere in this section.

---

## `_suppressCardTap` latch could strand on a platform where `Button.Clicked` does not bubble

**Raised:** 2026-08-05, during the final notebook-metadata review
**Area:** `DungeonMaster.App` — `FloorSidebarItem`
**Type:** cross-platform gap, unconfirmed off Windows

`FloorSidebarItem.xaml.cs:277` sets `_suppressCardTap` before a child button's own click handling runs, and relies on the subsequent bubbled `OnCardTapped` to clear it — the pattern used to stop a click meant for the button (notebook, delete, etc.) from also being read as a click on the card underneath it. Verified working on Windows, where `Button.Clicked` reliably bubbles to the containing card's tap recognizer.

On a platform where the click does not bubble the same way, `OnCardTapped` never runs, the latch stays set, and it silently swallows the user's very next genuine card tap — a stuck state that only clears itself on the next successful bubble.

This is the same shape as the pre-existing delete-button suppression path elsewhere in the file, but that one is lower risk: it always ends in a confirm popup, which gives the user an obvious "did that work?" signal and a natural place to retry. This latch has no such backstop, so it is the more likely of the two to strand a user without an obvious cause.

Needs a real device/platform check (iOS/Android/macCatalyst) to confirm before deciding on a fix; if it does strand, the likely fix is clearing the latch on a short timer or on the card's own `PointerExited` rather than depending solely on the bubble.

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

- `AreaSidebarItem.UpdateAreaName(string)` and `FloorSidebarItem.UpdateAreaName(Guid, string)` — both became unreachable when area renaming moved to the reload path.
- `FloorSidebarItem.UpdateFloorName(string)` — joined them once floor renaming moved to the reload path too, so the whole in-place label-patching API is now unused. Decide whether to keep the three as deliberate control API or delete them together.
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
