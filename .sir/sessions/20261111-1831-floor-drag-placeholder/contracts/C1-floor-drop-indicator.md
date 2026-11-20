<!-- frozen: 20261111-1831-floor-drag-placeholder, Gate 2 -->

# C1 - Floor drop indicator

One contract for the session. Section 2 is the only cross-task interface (T1 declares, T2 calls).
Sections 1, 3, 4 and 5 are the diagnosis and the view design T2 builds against; they declare no
operation another task calls and carry no coverage row of their own.

All `file:line` citations are `grep -n` at `12ef199` in the session worktree.

## 1. Diagnosis of the self-drop gap

**Conclusion.** The card-height empty space the user saw is, with confidence **probable**, the
dragged card itself, made invisible at drag start and still occupying its slot while the pointer
is over it, with no indicator marking that slot. That is a DURING-drag state, which is why no
post-drop restore path (the base's or the discarded revision's) could cure it. Whether the card
also stays invisible AFTER the release is **undetermined**: by source reading it should not.
The design in section 3 removes both readings by construction, so it depends on neither.

Evidence:

| # | Fact | Where |
|---|------|-------|
| E1 | Drag start sets the dragged row's `Opacity = 0`; the only writer of 1.0 is the drag-end handler. | `DungeonDetailPage.xaml.cs:981-982`, `:985-995` |
| E2 | `ShowDragPlaceholder` returns immediately when the hovered card is the dragged card, neither placing a placeholder nor removing one shown earlier at another card (T3 review A6). Hovering your own slot therefore shows nothing there. | `DungeonDetailPage.xaml.cs:2553-2558` |
| E3 | WinUI: "An Opacity value of 0 does not exclude an object from hit testing." The invisible card still receives `DragOver`/`Drop`, so the pointer sits over an empty-looking region that is a live drop target. | external: https://learn.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.uielement.opacity (section "Opacity and hit-testing") |
| E4 | The user's words were about the drag in progress: "No placeholder appears, and there is just a card-height empty space that never disappears when dragging". | `.sir/sessions/20261106-2050-detail-page-trio/reviews/T3.md:121` |
| E5 | Microsoft.Maui.Controls is pinned at 10.0.41 (`grep -n "Microsoft.Maui.Controls\"" Directory.Packages.props` -> `:26`). At that tag MAUI subscribes the source's WinUI `DropCompleted` whenever `CanDrag` is true (`GesturePlatformManager.Windows.cs:920-926`) and relays it (`:201-205`) to `DragGestureRecognizer.SendDropCompleted`, which forwards it once per drag, gated on `_isDragActive` that `SendDragStarting` sets (`DragGestureRecognizer.cs:94-109`, `:128`). So the base's drag-end restore is expected to run on every drag whose `DragStarting` ran. Nothing in this repository's code or MAUI's explains a card that stays invisible after the release. | external: https://raw.githubusercontent.com/dotnet/maui/10.0.41/src/Controls/src/Core/Platform/GestureManager/GesturePlatformManager.Windows.cs ; https://raw.githubusercontent.com/dotnet/maui/10.0.41/src/Controls/src/Core/DragAndDrop/DragGestureRecognizer.cs |
| E6 | On the card, `HandleDrop` and `HandleDragOver` do not set `Handled` (`GesturePlatformManager.Windows.cs:180-235`), so a card drop also bubbles to the list. The list then resolves `ExistingTarget` and returns early with no teardown (`DungeonDetailPage.xaml.cs:2490-2493`). | external (as E5); repo |
| E7 | The discarded revision restored Opacity from the card's declined drop, from drag start, from drag end and after the reorder await. Its tests passed, and the user re-observed the gap. That is consistent with E1-E4, where the gap exists before any of those paths runs. | `.sir/sessions/20261106-2050-detail-page-trio/reviews/T3.md:123-128`, `:154` |
| E8 | No automated test can start a real WinUI drag in this repo, so no step of this diagnosis can be observed by a command. | `src/DungeonMaster.App.UiTests/FloorSidebarAutomationTests.cs:26-31` |

A web search for a MAUI issue about `DropCompleted` not being raised on a self-drop on Windows
found none.

**Residual risk.** Suppose WinUI really does skip both the card's `Drop` and the source's
`DropCompleted` on a self-drop. Then section 3 leaves exactly one piece of drag state behind: the
insertion line and a card dimmed to 0.4 (Gate 2 decision D1). Neither is an empty space, and the
next drag start clears both (rules V1 and V6). Gate 3 human check H1 is the only instrument that
can tell.

## 2. App.Core interface (T1 declares, T2 calls)

Source-tree homes, owned by T1. Consumers import these and nothing else:

- `src/DungeonMaster.App.Core/Editor/FloorDropLanding.cs` declares `FloorDropLanding`.
- `src/DungeonMaster.App.Core/Editor/FloorDropIndicator.cs` declares `FloorDropIndicator`.
- `src/DungeonMaster.App.Core/Editor/FloorReorderDrop.cs` gains `TryReadDraggedFloorId`.

Namespace: `DungeonMaster.App.Core.Editor`. There is one type per file (`dotnet-format.md` section 2).

```csharp
public readonly record struct FloorDropLanding(int TargetIndex, int GapIndex, double LineY)
{
	public static FloorDropLanding None { get; }   // (FloorReorderNearest.NoBand, FloorReorderNearest.NoBand, double.NaN)
	public bool HasLanding { get; }                 // TargetIndex != FloorReorderNearest.NoBand (computed, not stored)
}

public static class FloorDropIndicator
{
	public static FloorDropLanding Locate(
		IReadOnlyList<FloorListBand>? bands,
		double y,
		int draggedIndex);
}

public static class FloorReorderDrop   // existing type; one member added
{
	public static bool TryReadDraggedFloorId(
		IReadOnlyDictionary<string, object>? payload,
		out Guid draggedFloorId);
}
```

### 2.1 `FloorDropIndicator.Locate`

Inputs are the bands in visual order, in `FloorListContainer` coordinates, the pointer's y in the
same coordinates, and the dragged card's index among those bands.

| Case | Result |
|------|--------|
| `bands` null or empty | `None` |
| `y` not finite | `None` |
| `draggedIndex < 0` or `>= bands.Count` | `None` |
| `FloorReorderNearest.Resolve(bands, y).Owner == None` | `None` |
| resolved band index `t` equals `draggedIndex` (dropping in place is a no-op) | `None` |
| resolved band `t` is not a `FloorListBandKind.FloorCard` | `None` |
| `draggedIndex < t` (moving down) | `TargetIndex = t`, `GapIndex = t + 1` |
| `draggedIndex > t` (moving up) | `TargetIndex = t`, `GapIndex = t` |

`t` is `Resolve(bands, y).BandIndex` for BOTH `ExistingTarget` and `FloorList` owners. This is
the one-owner rule: the indicator names exactly the card that the card's own `Drop` (pointer
inside a card band) or the list's `Drop` (near-miss) would hand to `FloorReorderDrop.Decide`.

`LineY` is the y of gap `g = GapIndex`:

- `g == 0`: `bands[0].Top`
- `g == bands.Count`: `bands[^1].Bottom`
- otherwise: `(bands[g - 1].Bottom + bands[g].Top) / 2`, the middle of the seam

The neighbour bands are used whatever their height.

**Why the gap is the true landing position.** `Dungeon.ReorderFloor(floorId, newNumber)` gives
the dragged floor `newNumber` and shifts every floor between its old and new numbers by one
(`src/DungeonMaster.Domain/Entities/Dungeons/DungeonMethods.cs:102-145`, the shifts at `:125-140`).
A drop on card `t` sends `newNumber = t + 1`, so the dragged floor ends at 0-based position `t`.
Gap `t + 1` (moving down) and gap `t` (moving up) are the slots where that position lies in the
list as the user sees it during the drag. That identity is T1's cross-check against the domain.

There are no exceptions. Every unusable input returns `None`.

The row "resolved band `t` is not a `FloorCard`" stays a defence after T2 deletes
`TransientDropTarget` (V8). T1's test for it builds the kind as `(FloorListBandKind)int.MaxValue`
and never names the deleted member (Gate 2 decision D2).

### 2.2 `FloorReorderDrop.TryReadDraggedFloorId`

It returns true and sets `draggedFloorId` only when `payload` is non-null, holds `FloorIdKey`, and
that value is a `string` that `Guid.TryParse` accepts. In every other case it returns false with
`draggedFloorId = Guid.Empty`, and it throws nothing. `Decide` must obtain the dragged id through
this method, so the payload has one parser. `Decide`'s five rows are unchanged.

## 3. View design (binding on T2)

V1. **The dragged card is dimmed, never hidden** (Gate 2 decision D1, after a best-practice
review: the source item stays in its slot at reduced opacity, Atlassian Pragmatic drag and drop
design guidelines giving 40%). The page declares `private const double DraggedFloorCardOpacity =
0.4;`. On source `DragStarted` the page first resets every row, then sets the sender's `Opacity` to
`DraggedFloorCardOpacity`. The reset sets `Opacity = 1` on every `FloorSidebarItem` child of
`FloorListContainer` by walking the children, never through a stored reference to the dragged row,
so a row re-bound mid-drag cannot be missed. It runs on every terminal path in V6 except the
list's `DragLeave`, where the drag continues. No drag handler writes `IsVisible` or a visual state
on any row, and no other `Opacity` value is written on a row.

V2. **No child of `FloorListContainer` is inserted, removed or reordered by any drag handler.**
`BindableLayout` is the container's only writer. The placeholder and every member serving it are
deleted: `_dragPlaceholder`, `_draggingItem`, `_currentPlaceholderTarget`,
`_placeholderAfterTarget`, `ShowDragPlaceholder`, `ClearDragPlaceholder`,
`OnFloorPlaceholderRequested`, `OnFloorPlaceholderCleared`, and the card events
`PlaceholderRequested` and `PlaceholderCleared`.

V3. **The indicator is an insertion line.** It is a sibling of `FloorListContainer`, never a
child. The `ScrollView` at `DungeonDetailPage.xaml:692` gets a `Grid` content hosting
`FloorListContainer` and, declared after it, an element named `FloorDropIndicatorLine`. That name
is not `FloorDropIndicator`, which would shadow the App.Core type inside the page. The element:

- is a `BoxView`, `HeightRequest` 2, `VerticalOptions="Start"`, `InputTransparent="True"`,
  `Opacity="0"` at rest
- takes its colour from the `DmDragPlaceholderStroke` token
- is inset on the right by `FloorListContainer`'s right padding, taken from that padding (binding
  or construction-time read) rather than restated as a literal. A XAML binding needs its own
  `x:DataType` under the page's root `x:DataType` (precedent: `DungeonDetailPage.xaml:49-63`,
  where `TreatWarningsAsErrors` turns a compiled-binding miss into a build error).

During a drag the page writes exactly two of its properties: `TranslationY` and `Opacity`. Neither
invalidates measure, so the line moves without re-laying-out anything under the pointer. Show sets
`TranslationY = clamp(LineY - 1, 0, max(0, FloorListContainer.Height - 2))` and `Opacity = 1`.
Hide sets `Opacity = 0`.

V4. **One band builder.** One private page method measures `FloorListContainer`'s children into
`FloorListBand`s (the shape at `DungeonDetailPage.xaml.cs:2464-2482`, every band now
`FloorCard`) and the parallel card list. The list drop and the indicator both call it, and nothing
else constructs a `FloorListBand`.

V5. **Show path.** A drag over a card or over the list calls one page method with the
`DragEventArgs`. That method:

- reads `e.GetPosition(FloorListContainer)`
- reads the dragged id through `FloorReorderDrop.TryReadDraggedFloorId(e.Data?.View.Properties, ...)` (amended A2: on a `DragEventArgs`, `Data` is a `DataPackage` whose `Properties` has no dictionary interface at MAUI 10.0.41; its `View` is the read-only dictionary)
- finds `draggedIndex` as the index of the card whose `Floor.Id` equals that id
- calls `FloorDropIndicator.Locate`, then shows or hides the line

A null position, an unreadable payload, a dragged floor not in the list, or a `None` landing
hides the line. The method is idempotent, so the bubbled double call (E6) is harmless. Page state
holds no dragged item. The payload is the one owner of which floor is being dragged, so a row
re-bound mid-drag cannot leave stale state.

The card raises the drag-over through a new event, `DragOverReceived` (`EventHandler<DragEventArgs>`),
declared in `FloorSidebarItem.xaml.cs` and raised from `OnDragOver`. It also raises `DropReceived`
(`EventHandler`) as the first statement of `OnDrop`, before `Decide`, for every outcome. These
replace `PlaceholderRequested` and `PlaceholderCleared`. The handler names in the card's XAML
(`OnDragStarting`, `OnDropCompleted`, `OnDragOver`, `OnDragLeave`, `OnDrop`) do not change.

V6. **Hide on every terminal path.** Each of these hides the line, and each except the list's
`DragLeave` also runs the V1 full-opacity reset:

- source `DragStarted`, which resets any line left by a lost drag
- source `DragEnded` (`DropCompleted`)
- card `DropReceived`
- every return path of `OnFloorListDrop`, including the null-position return, the `None` and
  `ExistingTarget` declines, and the `ShouldReorder == false` return
- the list's new `DragLeave` handler (`OnFloorListDragLeave`), when the leave position is null or
  outside `[0, Width) x [0, Height)` of `FloorListContainer`. A leave bubbled from a card while the
  pointer is still inside the list keeps the line.

`OnFloorListDrop` stays `async void` with its `try` opening before the first `await`
(`AsyncVoidUnitsAreGuardedTests`).

V7. **Drop meaning is unchanged.** A card drop still decides with that card's `Floor`. A list
near-miss still resolves through `FloorReorderNearest.Resolve` and then `Decide`. Both still call
`ReorderFloorCommand` with the target's 1-based `Number`.

V8. **`FloorListBandKind.TransientDropTarget` is deleted.** No placeholder band can exist any more.
The enum keeps `FloorCard`, so the `FloorListBand` constructor and `Resolve` keep their shape. Removing the
kind concept entirely is left to a later session.

## 4. Comments that become false and must be rewritten or removed

These were located at `12ef199`. A task touching one of these files leaves no sentence describing a
placeholder element, a placeholder drop target, or an Opacity-hidden dragged card.

| File | Lines at `12ef199` | Owner |
|------|--------------------|-------|
| `src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml` | `:686-691`, `:700-724`, `:731-739` | T2 |
| `src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs` | `:46-47`, `:2422-2425` ("both of the other two drop targets"), `:2440-2446`, `:2448-2451` ("the other two drop handlers"), `:2487-2489`, `:2499-2501` | T2 |
| `src/DungeonMaster.App/Controls/FloorSidebarItem/FloorSidebarItem.xaml.cs` | `:130-138`, `:508-515`, `:524-525` | T2 |
| `src/DungeonMaster.App.Core/Editor/FloorReorderNearest.cs` | `:14-20`, `:115-123`, `:178-179` | T2 |
| `src/DungeonMaster.Tests/App/Layout/FloorDragHandleTests.cs` | `:252-255` | T2 |
| `src/DungeonMaster.Tests/App/Layout/FloorDragHandleDropTargetTests.cs` | `:56` ("the third drop target"), `:145-186`, `:239-244` | T2 |
| `src/DungeonMaster.App.Core/Editor/FloorReorderDrop.cs` | `:35-39` | T1 |
| `src/DungeonMaster.App.Core/Editor/MapEditorState.cs` | `:14-15` ("placeholder insertion") | T1 |

These are not floor-drag placeholders and must stay:

- `DungeonDetailPage.xaml.cs:376` and `:606` (map drawable and object-cache placeholders)
- `FloorSidebarItem.xaml:151` (unnamed-floor "Floor N" text)
- `src/DungeonMaster.App.Core/Timing/OptimisticGhostSequence.cs` (object ghost)

## 5. Gate 3 human checks (orchestrator; no command can make these, per E8)

**Launch.** Run
`dotnet build C:/dm-wt/20261111-1831-floor-drag-placeholder/src/DungeonMaster.App/DungeonMaster.App.csproj -t:Run -f net10.0-windows10.0.19041.0`.
If that exits with no `DungeonMaster` process running (check with `tasklist`), launch
`C:/dm-wt/20261111-1831-floor-drag-placeholder/src/DungeonMaster.App/bin/Debug/net10.0-windows10.0.19041.0/win-x64/DungeonMaster.App.exe`
directly.

**Setup.** Open a dungeon with at least 4 floors, all collapsed (no floor selected).

- **H1, self-drop.**
  1. Press the drag handle (the vertical dots at the right of the card) on floor 2.
  2. Move the pointer a few pixels inside floor 2's card and back.
  3. Release over floor 2's own card.

  Pass: at no moment during or after the drag is there an empty card-sized space. Floor 2's card
  stays drawn in its slot, dimmed but clearly visible, throughout the drag. After release it is
  back at full strength and the floor order is unchanged.
- **H2, reorder down.**
  1. Drag floor 1 by its handle over floor 3's card.
  2. Note where the line is.
  3. Release.

  Pass: the line sits in the seam directly below floor 3, and floor 1 lands in that slot (it is now
  third).
- **H3, reorder up.**
  1. Drag floor 4 over floor 2's card.
  2. Note the line.
  3. Release.

  Pass: the line sits in the seam directly above floor 2, and the floor lands there (it is now
  second).
- **H4, tracking without shifting.**
  1. Drag floor 1 and move slowly down over every card and seam to the last card, without
     releasing.

  Pass: no card moves or changes size at any point during the drag, and floor 1 stays visible,
  dimmed, in its slot. Over floor 1 itself no line shows. Over each other card the line sits
  directly below that card.
- **H5, seam drop.**
  1. Drag floor 1.
  2. Release on the thin gap between floor 3 and floor 4.

  Pass: the line shown just before release marked the slot floor 1 then lands in.
- **H6, leave and cancel.**
  1. Drag floor 2 out of the sidebar onto the map.
  2. Release there.
  3. Start another drag and press Esc.

  Pass: after each, no line remains, every card is back at full strength (none left dimmed), and
  the order is unchanged.
