# CONTRACT — DmCard's trailing-content reveal

Frozen at Gate 2, session `2026-08-28-the-control-kit`.

**PRODUCER:** T2 — closes `MASTER-159` (S2, user-facing) and owns `src/DungeonMaster.App/Controls/DmCard/**`
plus the sole consumer, `src/DungeonMaster.App/Views/DungeonListPage/DungeonListPage.xaml`.
**CONSUMER:** T8 — later edits `Controls/DmCard/**` for `MASTER-077`'s subtitle clause and must not
regress what T2 established.

This contract exists for one reason: `MASTER-159` is the only S2 in the session and the only defect
here that a user can see, and the task that fixes it is not the last task to touch the file. An
implementer needing this changed **stops and escalates**.

---

## 1. What is NOT frozen — deliberately

**The mechanism of the defect is not stated here, and T2 must not be given one.** The audit record
(`docs/audit/00-master-index.md:469`) asserts that the `TrailingContent` `propertyChanged` callback
"does not run" for a value set inside the list's `DataTemplate`. That is a hypothesis, not an
observation: the observation behind the entry is only that hover fires, `PointerOver` applies, and
neither button appears in a 19-descendant UIA tree or a full-window screenshot.

An earlier handoff concluded from `DmCard.xaml.cs:46` that the entry's headline is false and the entry
should be closed as already-fixed. **That conclusion is rejected.** It reads the index's one-line row,
not the full entry, which already states the assignment at `:46` exists and makes its claim about the
callback anyway.

T2 therefore **reproduces first and establishes the mechanism empirically**, and only then fixes it.
A fix that changes code without a reproduction is not acceptable here, because nothing in this session
could then tell whether it worked.

## 2. Facts about the current tree, measured 2026-08-28

Stated so T2 does not re-derive them and T8 does not contradict them.

- `TrailingPresenter` is a bare `ContentView` at `Controls/DmCard/DmCard.xaml:52-55`, in
  `Grid ColumnDefinitions="*,Auto"` (`:37`), column 1.
- Its `Content` is assigned only from `TrailingContentProperty`'s `propertyChanged` at
  `DmCard.xaml.cs:42-48`, the assignment itself at `:46`.
- `TrailingPresenter.IsVisible` is set `false` in the constructor at `:84`, `true` at `:104`
  (`OnHoverEntered`), `false` at `:110` (`OnHoverExited`).
- The **only** consumer is the `CollectionView` `DataTemplate` at
  `Views/DungeonListPage/DungeonListPage.xaml:42-143`, `x:DataType="dto:DungeonSummaryDto"`. The
  trailing content is a `HorizontalStackLayout` (`:50`) holding the Edit button (`:53`,
  `AutomationId="EditDungeonButton"`, `Clicked="OnEditClicked"`), an unwired Start-Session button
  (`:87`, `AutomationId="StartSessionButton"`), and the Delete button (`:121`,
  `AutomationId="DeleteDungeonButton"`, `Clicked="OnDeleteClicked"`).
- **The reveal is pointer-only, on paper as well as in practice.** `DmCard.WireHover()` (`:87-99`)
  subscribes `HoverEntered` and `HoverExited` and nothing else. `HoverOrLongPressBehavior` also
  raises `LongPressed` (`Behaviors/HoverOrLongPressBehavior.cs:13`, raised at `:62`), and **nothing
  anywhere in the repository subscribes it**. Measured with
  `grep -rn 'LongPressed' src/ --include=*.cs --include=*.xaml` — the only subscriber hits
  (`DungeonDetailPage.xaml.cs:217`, `:247`) are on `CanvasLongPressDetector`, a different type.
  `Controls/AreaSidebarItem/AreaSidebarItem.xaml:92-96` wires the same behavior the same partial way.

## 3. What T2 must leave true, and T8 must keep true

These are the invariants. Everything else about `DmCard` is negotiable.

1. **The Edit and Delete buttons are reachable by a user on a dungeon-list card.** Reachable means a
   human, or a UIA driver, can get them on screen and activate them — not that they exist in the
   visual tree.
2. `DungeonListPage.OnEditClicked` and `OnDeleteClicked` are reachable from those buttons. Session 4
   recorded both handlers as guarded but **never seen running**, precisely because of this defect
   (`00-master-index.md:415`, the `MASTER-019` qualification). Closing `MASTER-159` is what makes
   that observation possible; T2 should expect its reviewer to ask for it.
3. `DmCard` keeps a public `TrailingContent` bindable property of type `View`, settable from XAML as
   a property element. The property's **name, type and public accessibility are frozen**; its
   `propertyChanged` callback, default value, and the presenter's internals are not.
4. `AutomationId="EditDungeonButton"`, `"StartSessionButton"` and `"DeleteDungeonButton"` keep those
   exact strings. They are how any UIA observation addresses the buttons, and Session 4's record
   refers to them.
5. The Start-Session button stays **unwired**. `DungeonListPage.xaml:72-85` explains at length why,
   and its `SemanticProperties.Hint` deliberately says the action is unavailable. Wiring it is a
   feature. If T2's fix makes the button visible for the first time, that comment stays accurate and
   must not be softened.

## 4. Boundaries

- **T2 may edit `Behaviors/HoverOrLongPressBehavior.cs`** if the mechanism is there — it is in T2's
  ownership set for that reason. **T2 must NOT fix `MASTER-006`** (that behavior never detaches the
  pan recogniser it adds, `OnDetachingFrom` at `:28-43` removes `_pointer` but not the
  `PanGestureRecognizer` added at `:25`). `MASTER-006` is rescheduled to Session 6
  (`00-master-index.md:194`, status `rescheduled to Session 6`) because it needs evidence this
  session cannot produce. If T2 touches that file, it says so and says explicitly that it left
  `MASTER-006` alone.
- **T2 must NOT touch `DmCard`'s `Subtitle` mechanism.** `SubtitleProperty`'s `propertyChanged`
  (`DmCard.xaml.cs:24-31`) and `UpdateSubtitleVisibility()` (`:113-114`) are `MASTER-077`'s, and
  `MASTER-077` belongs to T8 in whole. Two tasks editing that pair is how a partial fix gets recorded
  as a whole one.
- **T8 must NOT weaken invariants 1–5.** If `MASTER-077`'s remedy for the subtitle appears to require
  it, that is an escalation.
- If T2 establishes that the mechanism lies outside its ownership set — in the `CollectionView`
  virtualisation, in a MAUI platform behaviour, or in a file it does not own — it **escalates with
  the reproduction attached** rather than reaching outside. The reproduction is the deliverable
  either way.
