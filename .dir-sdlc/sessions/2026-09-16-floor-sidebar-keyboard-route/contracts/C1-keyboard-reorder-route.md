# Contract C1 — the floor keyboard reorder route

Draft for Gate 2. Written by `dir-architect` in decomposition mode. Produced by `T1` (the decision
type) and `T3` (the control and the page); guarded by `T4`.

This file's absolute main-checkout path is
`C:/Users/dmarq/Development/Repos/dungeon-master/.dir-sdlc/sessions/2026-09-16-floor-sidebar-keyboard-route/contracts/C1-keyboard-reorder-route.md`.

---

## 0. The measurement this contract rests on, and the one it does NOT

`measurement.md` §3 left exactly one question open: **does attaching `Plugin.Maui.KeyListener`'s
`KeyboardBehavior` to a control scope its key events to that control?**

**It does not. It cannot.** Measured three ways on 2026-09-16, none a restatement of another:

1. The package's own Android doc comment, in
   `C:/Users/dmarq/.nuget/packages/plugin.maui.keylistener/1.0.0-preview4/lib/net10.0-android36.0/Plugin.Maui.KeyListener.xml`,
   on `M:Plugin.Maui.KeyListener.KeyboardBehavior.GetParentLayout`:
   *"Similarly to the Apple and Windows implementations, **find the outermost layout** to connect
   the key events to."*
2. The Windows assembly's own metadata. Extracting printable strings from
   `.../lib/net10.0-windows10.0.19041/Plugin.Maui.KeyListener.dll` yields `OnAttachedTo`,
   `get_XamlRoot`, `get_Content`, `FrameworkElement` and `add_KeyDown` — the shape of a walk to the
   window root, not a subscription on the attached view.
3. The upstream source of `KeyboardBehavior.Windows.cs`
   (`davidortinau/Plugin.Maui.KeyListener`), whose `OnAttachedTo` reads:

   ```csharp
   protected override void OnAttachedTo(VisualElement bindable, FrameworkElement platformView)
   {
       base.OnAttachedTo(bindable, platformView);
       if (platformView.XamlRoot?.Content is FrameworkElement content)
       {
           content.KeyDown += OnKeyDown;
           content.KeyUp += OnKeyUp;
       }
   }
   ```

**Consequence, and it is binding.** A second `KeyboardBehavior` attached to the drag handle would
subscribe to the *same* `XamlRoot.Content` the page already subscribes to — a second global
listener, not a scoped one, and it would double-fire. **No task may add a `KeyboardBehavior`
anywhere.** The route in §3 is therefore the explicit focus gate, the second of the two shapes
`measurement.md` §3 named.

**What is NOT measured, and must not be asserted by anyone as if it were:** whether WinUI delivers a
`Ctrl`+arrow `KeyDown` to `XamlRoot.Content` while a MAUI `Button` holds focus. Nothing in this
repository or in the package establishes it. It is settled by the human observation pass (§6), and
no task may write a criterion that presumes the answer.

---

## 1. The drag handle becomes a `Button` — FROZEN

`FloorSidebarItem.xaml:237-255` currently declares the handle as a `Label`. It becomes a `Button`.

The justification is already written in that file, by 6b-iv, at `FloorSidebarItem.xaml:100-108` and
again at `FloorSidebarItem.xaml.cs:290-300`:

> As a Label it took no keyboard focus on WinUI — only Control-derived classes do — and the card
> body it points at is a VerticalStackLayout, which takes no focus either and cannot be made to:
> IsTabStop/TabIndex/ITabStopElement are absent from .NET MAUI 10's public API and MAUI 9 removed
> layout focus deliberately.

The drag handle is a `Label`. That sentence applies to it unchanged. `ExpandToggle` is the worked
precedent in the same file, for the same reason, closing `MASTER-164`.

**Frozen properties of the conversion:**

| Item | Requirement | Why |
| --- | --- | --- |
| Element type | `Button` | Only `Control`-derived types take WinUI keyboard focus. Tab reaching the handle is itself half of `MASTER-176`'s remedy. |
| `x:Name` | `DragHandle` (new) | The code-behind must reach it for §3 and §4.3. |
| `AutomationId` | `FloorDragHandle` — **unchanged** | `src/DungeonMaster.App.UiTests/FloorSidebarAutomationTests.cs:85` and `src/DungeonMaster.Tests/App/Layout/FloorDragHandleTests.cs:45` both address it by this string. |
| `Padding` | `8,0` — **unchanged** | The grab area. `FloorDragHandleTests.TheHandleIsWidenedByPaddingRatherThanByTheGlyph` asserts a non-empty `Padding`. Do **not** substitute `DmPaddingCompact`; that is `ExpandToggle`'s value, not this one's. |
| `BackgroundColor` | `Transparent` — **unchanged** | Load-bearing, not cosmetic: WinUI does not hit-test a null `Background`, so without it the padding is inert. Asserted by the same test. |
| `FontFamily` / `FontSize` | `DmFontBody` / `DmIconSizeLarge` — **unchanged** | Growing the glyph is the route that grows the header row, which 5c's contract C3 §2 forbids. `FontSize` asserted by the same test. |
| `HeightRequest`, `MinimumHeightRequest` | **absent** | Asserted `BeNull` by the same test; either one raises the header row. |
| `BorderWidth` | `0` | `ExpandToggle`'s precedent — leaves nothing of the button drawn. |
| `Margin` | `0` | Padding is the mechanism; margin is not. `ExpandToggle`'s precedent. |
| `Text` | `&#x22EE;` — unchanged | |
| `TextColor` | `{StaticResource DmTextTavernMuted}` — unchanged | |
| `VerticalOptions` | `Fill` — unchanged | The handle takes the row's full height rather than 24 of its 44. |

### 1.1 The drag gesture rehosts, and this changes a previous session's decision

`FloorSidebarItem.xaml:235-236` records 6b-iv's contract C1 §3: *"The DragGestureRecognizer stays on
THIS Label and its two handler names are unchanged."* **That contract expired with 6b-iv and binds
nothing now**, and this session changes the host it named. Stated here so the change is legible
rather than silent.

- The `<DragGestureRecognizer DragStarting="OnDragStarting" DropCompleted="OnDropCompleted" />`
  moves onto the `Button`. **Both handler names stay exactly as they are** — the payload contract's
  only writer is `OnDragStarting` — and there must remain **exactly one** `DragGestureRecognizer` in
  the file.
- The comment block at `:235-236` must be rewritten in place to record that the host is now a
  `Button` and why. Leaving the old sentence there would be a false claim about the file, which is
  the error class this project has recorded fourteen times across 6b-iv and 6b-v.

### 1.2 `OnDragHandleTapped` is REMOVED

The `<TapGestureRecognizer Tapped="OnDragHandleTapped" />` on the handle, and the method
`OnDragHandleTapped` (`FloorSidebarItem.xaml.cs:363-364`), are both deleted.

This is not tidying. `FloorSidebarItem.xaml.cs` already records the measured reason in
`OnExpandToggleClicked`'s comment:

> IT ALSO DOES NOT SET `_suppressCardTap`, and that is measured rather than reasoned. A `Clicked` on
> this `Button` does NOT reach the card's `VerticalStackLayout` `TapGestureRecognizer`, so the flag
> had nothing to suppress and stayed set — and the next card-body tap was swallowed by it. Observed
> 2026-09-11 against the built app driven through UIA.

Once the handle is a `Button`, pressing it no longer reaches the card's tap recognizer, so
`OnDragHandleTapped` has nothing to suppress — and a `_suppressCardTap` left set is exactly the
swallowed-card-tap defect recorded above.

**The `_suppressCardTap` field itself stays**: `OnDeleteClicked` and `OnNotebookClicked` still set it.

### 1.3 The revert guard

`ApplyExpanded` defends `ExpandToggle` by naming its type (`Button toggle = ExpandToggle;`) so a
revert to `Label` fails as `CS0029` rather than regressing silently. **The same guard is required for
`DragHandle`**: some statement in `FloorSidebarItem.xaml.cs` must bind `DragHandle` to a
`Microsoft.Maui.Controls.Button`-typed local or field, with a comment saying why. §4.3's
`FocusDragHandle` is the site.

---

## 2. The key binding — FROZEN

**`Ctrl`+`UpArrow` moves the focused floor one position up. `Ctrl`+`DownArrow` moves it one position
down.** No other key, no other modifier.

**Why these and not others, measured:**

- `KeyboardKeys` declares `UpArrow` and `DownArrow`; `KeyboardModifiers` declares
  `None, Shift, Command, Control, Alt`. Both read from
  `C:/Users/dmarq/.nuget/packages/plugin.maui.keylistener/1.0.0-preview4/lib/net10.0/Plugin.Maui.KeyListener.dll`
  with a `System.Reflection.Metadata` type/field dump on 2026-09-16.
- `KeyPressedEventArgs` exposes exactly four properties: `Modifiers`, `Key`, `KeyChar`, `Handled`
  (same dump). **`Key` is the property to switch on for an arrow; `KeyChar` is not** — the existing
  `switch (char.ToLower(e.KeyChar))` at `DungeonDetailPage.xaml.cs:377` cannot see an arrow key.
- **`Control` is the only modifier proven to reach this handler in this application.**
  `DungeonDetailPage.xaml.cs:446` already reads `e.Modifiers.HasFlag(KeyboardModifiers.Control)` for
  `Ctrl`+`0`, and that is shipped, working behaviour.
- `Alt`+arrow is the more common "move item up/down" convention, and it is **rejected on evidence**:
  on WinUI, `Alt`-modified presses are routed as system keys, and I could not confirm from the
  package, from this repository, or from a cited source that they raise `UIElement.KeyDown` at all.
  Adopting `Alt` would rest the whole remedy on an unverified platform claim. Recorded as belief,
  not fact.
- **No collision.** `OnKeyDown`'s switch covers `d m h p s e r [ ] 0` only
  (`DungeonDetailPage.xaml.cs:377-450`), and nothing in the file reads an arrow key:
  `grep -c "UpArrow\|DownArrow\|LeftArrow\|RightArrow" src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs`
  returns `0`, run in the worktree at `7483b57`.

---

## 3. The focus gate — FROZEN

The reorder branch fires **only** when a floor card's drag handle currently holds keyboard focus.
This is the explicit gate `measurement.md` §3 named as the alternative to a scoped listener, and §0
establishes that a scoped listener does not exist.

**`FloorSidebarItem` exposes the fact, computed and never stored:**

```csharp
/// <summary>
///		Gets a value indicating whether this floor's reorder handle currently holds keyboard focus.
///		Computed on every read off the Button's own IsFocused, so it cannot go stale against it.
/// </summary>
public bool IsDragHandleFocused => DragHandle.IsFocused;
```

Frozen: the member name `IsDragHandleFocused`, its type `bool`, that it is get-only, and that it is a
computed expression over `DragHandle.IsFocused` with **no backing field**. A stored copy is two
owners of one fact — the defect `MASTER-098` took out of this file, and a rule of the frontend
discipline ("Derived values are computed, never stored").

**The page reads it and owns everything else.** `DungeonDetailPage` already enumerates
`FloorListContainer.Children` for `FloorSidebarItem` at `:2186`, `:2202` and `:2267`. The reorder
branch finds the single child whose `IsDragHandleFocused` is true. If there is none, it does nothing
and falls through — **silently, with no error message**: a `Ctrl`+arrow pressed anywhere else on the
page is not a failure.

---

## 4. The decision, the call, and the focus restoration — FROZEN

### 4.1 The decision is a pure function in `DungeonMaster.App.Core` (`T1`)

New file `src/DungeonMaster.App.Core/Editor/FloorReorderKeys.cs`, modelled on the existing
`src/DungeonMaster.App.Core/Editor/FloorReorderDrop.cs` for the reason that file gives in its own
words: *"so the decision is testable in this project while the MAUI-touching work stays in the
view."*

```csharp
namespace DungeonMaster.App.Core.Editor;

/// <summary>What a keyboard reorder keystroke means.</summary>
public readonly record struct FloorKeyboardReorderDecision(bool ShouldReorder, int TargetFloorNumber)
{
    public static FloorKeyboardReorderDecision None => default;
}

public static class FloorReorderKeys
{
    public static FloorKeyboardReorderDecision Decide(int focusedFloorNumber, int floorCount, bool moveUp);
}
```

Frozen: the namespace, both type names, the member names `ShouldReorder`, `TargetFloorNumber`, `None`
and `Decide`, and `Decide`'s parameter order, names and types.

`Decide` returns `None` — a silent no-op, never an error — in every one of these cases:

| Case | Returns |
| --- | --- |
| `floorCount < 2` | `None` |
| `focusedFloorNumber < 1` | `None` |
| `focusedFloorNumber > floorCount` | `None` |
| `moveUp` and `focusedFloorNumber == 1` (already the top floor) | `None` |
| `!moveUp` and `focusedFloorNumber == floorCount` (already the bottom floor) | `None` |

Otherwise it returns
`new FloorKeyboardReorderDecision(true, focusedFloorNumber + (moveUp ? -1 : 1))`.

**`TargetFloorNumber` is a 1-based floor `Number`**, never an index into a collection and never an
index into a visual-tree child collection — the same distinction `FloorReorderDecision`'s own doc
comment draws, for the same reason. `FloorItem.Number` is the 1-based display number
(`src/DungeonMaster.App.Core/Presentation/FloorItem.cs:30`).

`FloorReorderKeys` must not reference `Plugin.Maui.KeyListener`, `Microsoft.Maui.*`, or any key type.
It takes a `bool moveUp`; mapping `KeyboardKeys` onto that bool is the page's job. This keeps the
type in a plain `net10.0` project that `DungeonMaster.Tests` already references
(`src/DungeonMaster.Tests/DungeonMaster.Tests.csproj:24`).

### 4.2 The call — the page reuses the command, not a new one

The reorder branch calls
`_viewModel.FloorList.ReorderFloorCommand.ExecuteAsync(new ReorderFloorArgs(focusedFloor.Id, decision.TargetFloorNumber))`
— the same command the three existing drop-driven call sites use
(`DungeonDetailPage.xaml.cs:861`, `:2306`, `:2426`). **No new command, no new handler, and no work
below the App layer**: `measurement.md` §3 establishes that `ReorderFloorCommand` / `Handler` /
`Validator` and `Dungeon.ReorderFloor` are built and tested already. This route adds a **fourth**
call site, taking the production App-code total to four.

Placement inside the existing `OnKeyDown` (`DungeonDetailPage.xaml.cs:362`):

- **after** the existing `IsPageInteractive()` and `IsTextInputActive()` guards, which stay first;
- **inside** the existing `try`, which is opened before the first `await` because the method is
  `async void` and an escaping exception would take the process down — the file says so in place;
- **before** the `switch (char.ToLower(e.KeyChar))`, because an arrow key carries no meaningful
  `KeyChar` (§2).

It must not introduce a second `catch`. On failure the existing `catch` sets `_viewModel.ErrorMessage`.

### 4.3 Focus restoration — REQUIRED, not optional

`RebuildFloorList()` (`DungeonDetailPage.xaml.cs:753`) clears `FloorListContainer.Children` and
constructs a fresh `FloorSidebarItem` per floor, so **the `Button` that held focus is destroyed by
the very reorder the user just performed.** Without restoration, one keypress moves the floor and
throws focus to the top of the page, and a second keypress does nothing — a keyboard route that
works exactly once. The frontend discipline's accessibility floor requires that focus move
predictably after an action that changes what is on screen.

`FloorSidebarItem` exposes one thin imperative method:

```csharp
/// <summary>
///		Moves keyboard focus to this floor's reorder handle.
///		Focus is an ACTION, not appearance state, so it is a method and not a bindable property —
///		and the Button lives inside this control's template, so the page has no other handle on it.
///		The Button-typed local is the same revert guard ApplyExpanded uses on ExpandToggle: as a
///		Label this element takes no keyboard focus on WinUI and the keyboard reorder route dies
///		silently, so a revert must fail as CS0029 rather than compile.
/// </summary>
public void FocusDragHandle()
{
    Button handle = DragHandle;
    handle.Focus();
}
```

Frozen: the member name `FocusDragHandle`, that it takes no parameters, returns `void`, and that it
carries the `Button`-typed local required by §1.3.

**This method does not reopen `MASTER-098`.** `UIKIT-011`'s direction is that *appearance state* be
bindable rather than driven by imperative setters. Focus is an action, not appearance state, and this
method is the same justified shape as the surviving `SetAreaEditing`: the element lives inside this
control and the page cannot reach it.

After the `await` completes, the page re-finds the `FloorSidebarItem` in `FloorListContainer.Children`
whose `Floor?.Id` equals the moved floor's id and calls `FocusDragHandle()` on it. If no such child
exists, it does nothing — silently.

---

## 5. Announced semantics — FROZEN

The handle's `SemanticProperties.Description` stays **`Reorder floor`**.

Its `SemanticProperties.Hint` changes to name both routes, because the keyboard route is otherwise
invisible and the hint is the only place a screen-reader user learns it. Frozen verbatim:

> `Drag this handle onto another floor card, or press Control plus Up Arrow or Control plus Down Arrow, to change the floor order.`

"Control plus Up Arrow" is spelled out rather than "Ctrl+Up" because a screen reader reads the string
as written.

`ToolTipProperties.Text` is **added** to the handle, matching `ExpandToggle`'s precedent of carrying
one. Frozen verbatim:

> `Reorder floor (Ctrl+Up / Ctrl+Down)`

**No task adds a `SemanticProperties.Description` stating a position** ("3 of 7"). It would need
rewriting on every reorder, add and delete, it has no owner in this design, and a stale position
announced confidently is worse than none.

---

## 6. What only observation can settle — NOT a static claim

These four are settled by the human observation pass, and **no criterion may assert any of them from
a source read.** Each names what must be observed.

1. **`Ctrl`+`UpArrow` / `Ctrl`+`DownArrow` reach `OnKeyDown` while the handle holds focus, and move
   the floor.** §0's open question. If they do not arrive, the route is inert and the session
   escalates rather than shipping something that reads as applied.
2. **Pointer drag-reorder still works after the gesture rehosts onto a `Button`.** A WinUI `Button`
   captures the pointer press, and whether MAUI's `DragGestureRecognizer` still starts a drag from
   one is not established anywhere in this repository. This is a regression risk of the same class
   as the inert-padding one 5c's C3 records, and it is the most likely way this change ships broken.
3. **The header row's height is unchanged.** Measured at 44 DIU by 6b-iv. A `Button` renders larger
   than a `Label` — that is what session 5c's escalation E5 refusal rested on. If the row grows, the
   conversion is reverted, not argued.
4. **The floor card's width and the floor name are not squeezed.** The `Label` measured 23 DIU wide;
   `ExpandToggle` measured 44x44. A handle that widens its `Auto` column takes that width from the
   floor name beside it.

Observations 1 and 2 are **blocking**. Observations 3 and 4 are reported to the user for a decision.


---

# AMENDMENT A1 — 2026-09-16, after observation O1. Resolves escalation E1.

**Sections 1, 1.1, 1.2, 1.3 and 3 above are SUPERSEDED by this amendment.** They are left in place unedited
because they record what was frozen at Gate 2 and what was built against it; this amendment records what
replaced it and why. Sections 2, 4, 5 and 6 are UNCHANGED and still bind.

## What observation found

`observations/O1-keyboard-and-drag.md`, run with the user against the worktree build at `52e6213`:

- **The keyboard route works, including on repeat.** Section 0's open question is settled YES — the keys
  reach `OnKeyDown` carrying `e.Key` while the handle holds focus.
- **The pointer drag is BROKEN.** A WinUI `Button` consumes the press and MAUI's `DragGestureRecognizer`
  does not start a drag from one. This is section 6 observation 2, which is **blocking**.
- **The floor name is squeezed** — `MAIN TAV…` became `MAIN TA…`. Section 6 observation 4.

**Both failures have one cause**: the `Button`'s rendered footprint and its press handling. Microsoft's own
drag-and-drop documentation attaches `DragGestureRecognizer` only to non-interactive elements — `Image`,
`Rectangle`, `Path` — and exposes no property that makes a `Button` a drag source. There is no documented
fix that keeps the `Button`.

## The user's decision at escalation E1

**Revert the handle to a `Label`, and gate the reorder keys on focus anywhere within the floor card.**

## 1A. The drag handle returns to a `Label` — FROZEN

The element carrying `AutomationId="FloorDragHandle"` is a **`Label`** again, restored to its shape at
commit `7483b57`: `Padding="8,0"`, `BackgroundColor="Transparent"`, the glyph `&#x22EE;` unchanged,
`FontFamily="{StaticResource DmFontBody}"`, `FontSize="{StaticResource DmIconSizeLarge}"`,
`TextColor="{StaticResource DmTextTavernMuted}"`, `HorizontalTextAlignment="Center"`,
`VerticalTextAlignment="Center"`, `VerticalOptions="Fill"`.

`HorizontalTextAlignment` and `VerticalTextAlignment` **return with it** — they are `Label` members that
`Button` does not expose, which is why T3 correctly dropped them and why they must come back now.

The `DragGestureRecognizer` sits on that `Label` with `DragStarting="OnDragStarting"` and
`DropCompleted="OnDropCompleted"`, as it did at `7483b57`. **This restores the exact element 6b-iv observed
starting a drag on 2026-09-11.**

`OnDragHandleTapped` and its `TapGestureRecognizer` **stay deleted.** They were removed because a `Button`
press does not reach the card's tap recognizer; a `Label` tap does, so restoring the suppression would be
restoring a behaviour whose only purpose was to compensate for the `Button`. `_suppressCardTap` remains, set
by `OnDeleteClicked` and `OnNotebookClicked` only.

**The revert guard inverts.** `ApplyExpanded`'s `Button`-typed local for `ExpandToggle` stays as it is — that
element is still a `Button` and `MASTER-164` still depends on it. But `FocusDragHandle()`'s `Button`-typed
local is **removed along with the method**, per 3A.

## 3A. The focus gate moves from the handle to the card — FROZEN

`IsDragHandleFocused` and `FocusDragHandle()` are **removed.** A `Label` cannot hold focus on WinUI, so a
property reporting whether it does would be permanently false and the route would be inert.

`FloorSidebarItem` instead exposes:

```csharp
public bool ContainsKeyboardFocus { get; }
```

get-only, **computed on every read with no backing field** — the same rule section 3 imposed on
`IsDragHandleFocused`, and for the same reason: a stored copy is two owners of one fact, the defect
`MASTER-098` took out of this file. It reports whether **any focusable descendant of this card currently
holds keyboard focus.**

**This works because `MASTER-164` already made the card keyboard-reachable.** `ExpandToggle` has been a
focusable `Button` since 6b-iv, and the notebook, delete and add-area buttons are focusable too. `Tab`
already stops on every floor card; the user presses `Ctrl`+arrow exactly as observation O1 confirmed, with
focus on the chevron rather than on the handle.

The page's gate reads `ContainsKeyboardFocus` instead of `IsDragHandleFocused`. Everything else in section 4
is unchanged: still exactly one matching card or nothing happens, still `FloorReorderKeys.Decide`, still the
existing `ReorderFloorCommand`, still silent no-ops.

### Focus restoration — still REQUIRED, and it changes target

Section 4.3's reason is untouched: `RebuildFloorList()` clears `FloorListContainer.Children` and destroys
whatever held focus, so without restoration the route works exactly once. Observation O1 confirmed the repeat
case working, and that must not regress.

`FocusDragHandle()` is replaced by **`FocusReorderAffordance()`**, which focuses the card's
**`ExpandToggle`** — the element that reliably takes focus on this card. It binds `ExpandToggle` to a
`Button`-typed local before calling `Focus()`, preserving the `CS0029` revert guard that `MASTER-164`
depends on.

**This is a deliberate, disclosed behaviour change**: a user who pressed `Ctrl`+arrow while focus was on the
delete button lands on the chevron afterwards rather than back on delete. Predictable and announced beats
focus lost to the page, which is what happens with no restoration at all.

## 5A. The announced semantics — one string changes

Section 5's `Description` (`"Reorder floor"`) and `ToolTipProperties.Text` are **unchanged**.

The `Hint` **keeps naming the keys** — it is still the only place a screen-reader user learns the route
exists — but it must no longer imply the handle itself is the thing to focus, because it is a `Label` again
and focus never rests on it. The frozen string becomes:

> `Drag this handle onto another floor card to change the floor order. With a floor card focused, press Control plus Up Arrow or Control plus Down Arrow to move it.`

## What does NOT change

- Section 2's key binding: `Ctrl`+`UpArrow` / `Ctrl`+`DownArrow`, read from `e.Key` and `e.Modifiers`,
  branch placed before the `KeyChar` switch. **Proven working by observation O1.**
- Section 4.1's `FloorReorderKeys` — T1's file is untouched by this amendment.
- Section 4.2's reuse of the existing command, and the silent no-op rules.
- The drop placeholder stays out of scope.

## The cost, stated plainly

The handle stops being its own tab stop, so a screen-reader user meets its `Hint` as **static text** rather
than as a focusable control. That is strictly weaker than what the `Button` gave, and it is the price of a
working pointer drag. `MASTER-176`'s requirement is that *a keyboard-only user can reorder floors* — which
this satisfies, and which observation O1 proved is reachable by `Tab` today.
