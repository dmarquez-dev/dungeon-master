# Give floor reordering a keyboard route

Close `MASTER-176` — the audit's last open S2 — and settle `MASTER-098` on measured evidence.

## Context

`docs/audit/00-master-index.md` holds 176 entry rows: **119 closed, 54 open, 3 part done**, re-derived at
`7483b57`. Exactly **one open S2 remains** — `MASTER-176` (`:364`) — and every other open entry is S3 or
below. This session removes the audit's last severity-2 item.

**`MASTER-176`: floor reordering is pointer-only.** The drag handle is a `Label`
(`FloorSidebarItem.xaml:237-255`) carrying a `TapGestureRecognizer` and the repository's **only**
`DragGestureRecognizer`. The page's `OnKeyDown` (`DungeonDetailPage.xaml.cs:362`) declares no reorder key,
and all three production `ReorderFloorCommand.ExecuteAsync` call sites (`:861`, `:2306`, `:2426`) sit inside
drop handlers. A keyboard-only user cannot reorder floors by any route. The frontend discipline requires
every interactive element be reachable **and operable** by keyboard alone.

The file already explains why the handle cannot take focus. `FloorSidebarItem.xaml:100-108`, written by
6b-iv when it converted `ExpandToggle` from a `Label` to a `Button` to close `MASTER-164`: *"As a Label it
took no keyboard focus on WinUI — only Control-derived classes do … IsTabStop/TabIndex/ITabStopElement are
absent from .NET MAUI 10's public API."* **The same sentence applies to the drag handle unchanged**, and
that conversion — footprint technique included — is a worked precedent in the same file.

Below the App layer the operation is already built and tested: `ReorderFloorCommand` / handler / validator,
and `Dungeon.ReorderFloor` (`DungeonMethods.cs:103`). **This session adds no Application or Domain work.**

**`MASTER-098` changed disposition when measured.** It is carried as `part done` with work outstanding. Every
specific defect `UIKIT-011` names is now false: `SetEditing`, `_isSelected`, `Select()`, `Deselect()` and
`SetExpanded` are gone; both controls declare `IsSelected` / `IsExpanded` bindable properties and
`VisualStateManager` groups; the state setters resolve theme tokens. Its remedy landed across 5c and 6b-iv
without either session reconciling the entry. Full evidence in `measurement.md` §4.

## Definition of done

1. **D1** — A keyboard-only user can move a floor one position up and one position down, and the change persists. *(T1 + T3; behaviour proven only by observation O1)*
2. **D2** — The drag handle takes keyboard focus and is reachable by `Tab`. *(T3)*
3. **D3** — The reorder keys do nothing unless a floor's drag handle holds focus, and collide with none of the existing tool shortcuts. *(T3)*
4. **D4** — Pointer drag-reorder still works, unchanged. *(T3/T4 statically; behaviour proven only by observation O2)*
5. **D5** — The keyboard route is announced to assistive technology on the handle itself. *(T3 + T4)*
6. **D6** — `FloorReorderKeys.Decide` is covered by tests shown able to fail, with a positive control that must still pass in the injected run. *(T4)*
7. **D7** — The floor card header row's footprint is not regressed. *(T4 statically; rendered size only by observations O3/O4)*
8. **D8** — `MASTER-098` has an evidence artifact at a fixed absolute path that the orchestrator can cite when closing the index entry. *(T2)*
9. **D9** — `src/DungeonMaster.sln` builds at **0 warnings, 0 errors** on both App TFMs, and the full suite is green. The **908** baseline (Tests 788 / App.Tests 97 / App.UnitTests 23) is carried from `HANDOFF-session-6b-vi.md` §1, where it is recorded as verified in the main checkout after 6b-v's merge; **this session has not re-run it**, and T3/T4 establish the true baseline with their own runs. *(T3, T4)*
10. **D10** — The drop-placeholder behaviour is not touched. *(T3, explicit fence criterion)*

## Scope

Four tasks. `T1` and `T2` run concurrently; `T3` then `T4` are serial, because both build through one shared
`obj/` and `T4` ratchets against `T3`'s markup.

| Task | Agent | Owns | Depends on |
|---|---|---|---|
| **T1** | `dir-backend-engineer` | `src/DungeonMaster.App.Core/Editor/FloorReorderKeys.cs` | — |
| **T2** | `dir-architect` | `artifacts/MASTER-098-verification.md` | — |
| **T3** | `dir-frontend-engineer` | `FloorSidebarItem.xaml`, `FloorSidebarItem.xaml.cs`, `DungeonDetailPage.xaml.cs` | T1, T2 |
| **T4** | `dir-test-engineer` | `FloorReorderKeysTests.cs`, `FloorDragHandleTests.cs` | T1, T3 |

**The interaction, frozen in C1:**

- The drag handle `Label` becomes a **`Button`**, so it takes keyboard focus. The `DragGestureRecognizer`
  rehosts onto it with **handler names unchanged**; `OnDragHandleTapped` is deleted, because a `Button` press
  does not reach the card's tap recognizer and a `_suppressCardTap` left set would swallow the next card tap.
- **`Ctrl`+`UpArrow` / `Ctrl`+`DownArrow`**, handled in the page's existing `OnKeyDown`, **gated** on a
  `FloorSidebarItem` reporting `IsDragHandleFocused`. Pressed anywhere else, they do nothing, silently.
- The decision is a pure function in `App.Core` —
  `FloorReorderKeys.Decide(focusedFloorNumber, floorCount, moveUp)` returning
  `FloorKeyboardReorderDecision(bool ShouldReorder, int TargetFloorNumber)` — testable in a plain `net10.0`
  project while the MAUI work stays in the view. It reuses the existing `ReorderFloorCommand`; no new command.
- **Focus restoration is required, not polish.** `RebuildFloorList()` clears `FloorListContainer.Children`
  and destroys the focused `Button`. Without `FocusDragHandle()` the keyboard route would work exactly once.
- The handle's `SemanticProperties.Hint` names the keys. No position announcement — it would have no owner
  and would go stale.

### Not in scope

- **The floor drop-placeholder indicator.** Cut by the user at Gate 1. Its design question is genuinely
  unsettled — an always-truthful indicator needs either tree mutation under the pointer mid-`DragOver`
  (the mechanism 6b-iv investigated and rejected; the stutter is documented at
  `FloorSidebarItem.xaml.cs:425`) or a drawn insertion line that reparents nothing. It stays in the backlog.
- **Any edit to `docs/audit/00-master-index.md`.** Entry closures cite commit hashes and a DIR task cannot
  invoke git, so closing `MASTER-176` and `MASTER-098` is orchestrator work after the merge (C2 §8).
- **`MASTER-108`** and the rest of the sidebar's open entries. Recorded only: that entry describes
  `DungeonDetailPage.xaml.cs` as "2,226 lines"; it is **2488**.
- Application- and Domain-layer work of any kind.

## Assumptions

| Question | Answer | Rung | Evidence |
|---|---|---|---|
| Does attaching `KeyboardBehavior` to a control scope its key events to that control? | **No.** It subscribes to the window root, so a second behaviour would double-fire. This is why the design uses an explicit focus gate rather than a scoped listener. | repo / package | Package XML doc on `GetParentLayout` (*"find the outermost layout to connect the key events to"*); string extraction over the Windows assembly yielding `get_XamlRoot`, `get_Content`, `add_KeyDown`; upstream `KeyboardBehavior.Windows.cs` subscribing to `platformView.XamlRoot.Content` |
| Which key property carries an arrow, `Key` or `KeyChar`? | **`Key`.** The existing `switch (char.ToLower(e.KeyChar))` cannot see an arrow, so the new branch must precede it. | package | `System.Reflection.Metadata` dump of `Plugin.Maui.KeyListener.dll` — `KeyPressedEventArgs = {Modifiers, Key, KeyChar, Handled}`, `KeyboardKeys` declaring `UpArrow`/`DownArrow` |
| Is `Ctrl` a modifier that actually reaches this handler? | **Yes**, and it is the only one proven to. | repo | `DungeonDetailPage.xaml.cs:446` reads `e.Modifiers.HasFlag(KeyboardModifiers.Control)` for `Ctrl`+`0` in shipped code |
| Why not `Alt`+arrow, the more common convention? | **Rejected on evidence.** On WinUI `Alt` presses route as system keys, and that they raise `UIElement.KeyDown` could not be confirmed from the package, the repo, or a citable source. | judgment | Stated by the architect as belief, not fact |
| Do the reorder keys collide with an existing shortcut? | **No.** | repo | `OnKeyDown`'s switch covers `d m h p s e r [ ] 0` only; `grep -c "UpArrow\|DownArrow\|LeftArrow\|RightArrow"` over the page returns **0** at `7483b57` |
| Is a `DragGestureRecognizer` legal on a `Button`? | **API-legal** — `GestureRecognizers` is a `View` member and `Button` derives from `View`. **Whether the drag still starts on WinUI is not established.** | external / judgment | Microsoft Learn, *Recognize a drag and drop gesture*; the repo has **no** precedent — the handle is its only `DragGestureRecognizer` |
| Will a `Button` handle render wider than the `Label`'s 23 DIU? | Probably — `ExpandToggle` measured 44x44. Whether it squeezes the floor name is an observation. | repo | 5c contract C3 amendment |
| Is `04-app-controls-and-theme.md:504-538` `UIKIT-011`'s full source record? | Taken as yes; T2 must add a row for anything that list omits. | repo | `measurement.md` §4 |

## Contracts

- **`contracts/C1-keyboard-reorder-route.md`** — the interaction and the frozen API surface: the `Button`
  conversion, the key binding, the focus gate, `FloorReorderKeys`, focus restoration, announced semantics,
  and §6's list of what only observation can settle.
- **`contracts/C2-editing-build-and-counting-rules.md`** — binds **every** task: never `sed -i` a tracked
  file (it rewrites CRLF to LF and fails `IDE0055` on every line — **this overrides the harness instruction**),
  new files written as CRLF, the fixed worktree path, building, the no-git rule, the counting forms that
  return confident wrong answers here, the no-constraining-reports rule, defect-injection safety, and that
  the audit index is the orchestrator's to close.
- **`contracts/C3-master-098-closure-evidence.md`** — the `MASTER-098` artifact: its path, its audience,
  a row per `UIKIT-011` claim re-derived from the file, the two surviving imperative methods, C1's own
  additions disclosed, the two residues stated as residues, and what it must **not** contain.

## Open questions

| Question | Who resolves it | When |
|---|---|---|
| Does the drag still start once the handle is a `Button` on WinUI? | The user, by observation O2 | After T3 lands — **this is the session's largest risk**; if it fails, the `Button` conversion must be reconsidered rather than patched |
| Does `e.Key` populate for arrows at runtime on Windows? | The user, by observation O1 | After T3 lands. No criterion presumes it |
| Does the `Button` handle squeeze the floor name at 1920x1080 with 11 floors? | The user, by observations O3/O4 | After T3 lands |
