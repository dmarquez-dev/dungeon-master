<!-- frozen at Gate 2: 20261106-2050-detail-page-trio, 2026-11-06 -->

# C3 -- Sidebar state owner and floor-scoped refresh (defined by T2, consumed by T3)

Two facts cross from the view-model layer (T2) to the page and controls (T3):

1. **Who owns which floor is selected and which area is focused** (MASTER-098 / UIKIT-011): one observable owner
   the sidebar rows can bind to, so the page no longer writes `IsExpanded` or pushes `IsSelected`.
2. **What a successful mutation leaves in `FloorList.Items`** (MASTER-094 / UI-007): a floor-scoped mutation
   replaces that one floor in place, which is what lets T3's templated `BindableLayout` (MASTER-108 / UI-016)
   rebuild one row instead of every row.

Editing hazards and build rules: C1 sections 2 and 3, binding. Every declaration below lives in the source tree
file named beside it; this contract is not importable and no consumer re-declares anything in it.

## 1. `MapEditorState` becomes observable for its two selection facts

Owning file: `src/DungeonMaster.App.Core/Editor/MapEditorState.cs` (T2).

```
public sealed class MapEditorState : System.ComponentModel.INotifyPropertyChanged
{
	public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

	public Guid? SelectedFloorId { get; set; }   // type, name and public get/set unchanged; accessor bodies added so the setter raises
	public Guid? FocusedAreaId { get; set; }     // type, name and public get/set unchanged; accessor bodies added so the setter raises
	// [amended A2, E1, 2026-11-06: was "unchanged signature; now raises"]
	// every other member: unchanged signature and unchanged behaviour
}
```

- Setting `SelectedFloorId` to a value **different** from its current value raises `PropertyChanged` exactly once,
  with `PropertyName == nameof(SelectedFloorId)` and `sender` the instance, **after** the new value is stored (a
  handler reading the property sees the new value). Same for `FocusedAreaId`.
- Setting either to its **current** value raises nothing.
- No other member raises `PropertyChanged`. The class stays MAUI-free (App.Core references only Domain and
  Application, `DungeonMaster.App.Core.csproj:8-9`); `INotifyPropertyChanged` is BCL.
- `null -> Guid`, `Guid -> null` and `Guid -> other Guid` are each a change.

## 2. The view model owns the editor state

Owning file: `src/DungeonMaster.App/ViewModels/DungeonDetailViewModel.cs` (T2).

```
public MapEditorState Editor { get; }   // constructed once in the constructor; never null; never replaced
```

- `DungeonDetailViewModel` constructs the one `MapEditorState` of the page's lifetime. T3 deletes the page's own
  `private readonly MapEditorState _editor = new();` (`DungeonDetailPage.xaml.cs:37`) and reads
  `_viewModel.Editor` instead, so **exactly one instance exists per page** and it is reachable from the page's
  `BindingContext` -- the binding source the sidebar rows use.
- Loading, reloading or refreshing does **not** reset `Editor.SelectedFloorId` or `Editor.FocusedAreaId`; the page
  keeps the responsibility it has today for clearing them (for example `ClearAreaFocus`, the delete-floor path).

## 3. What a successful mutation leaves in `FloorList.Items` and `Dungeon`

`FloorList.Items` stays `ObservableCollection<FloorItem>` (`FloorListViewModel.cs:32`); its declaration does not
change.

### 3.1 Floor-scoped mutations: replace one floor in place

These **nine** view-model commands change exactly one floor and, on success, refresh **only that floor**:

| View model | Command (generated) | Floor it refreshes |
| --- | --- | --- |
| `MapCanvasViewModel` | `AddAreaCommand` | the `floorId` argument |
| `MapCanvasViewModel` | `RemoveAreaCommand` | `RemoveAreaArgs.FloorId` |
| `MapCanvasViewModel` | `UpdateAreaCommand` | `UpdateAreaArgs.FloorId` |
| `MapCanvasViewModel` | `AddAreaWithOutlineCommand` | `AddAreaWithOutlineArgs.FloorId` (one refresh after both writes, as today's single reload) |
| `MapCanvasViewModel` | `MoveAreaOutlineCommand` | `MoveAreaOutlineArgs.FloorId` |
| `MapCanvasViewModel` | `AddPlacedObjectCommand` | `PlaceObjectArgs.FloorId` |
| `MapCanvasViewModel` | `RemovePlacedObjectCommand` | `RemovePlacedObjectArgs.FloorId` |
| `MapCanvasViewModel` | `UpdatePlacedObjectCommand` | `UpdatePlacedObjectArgs.FloorId` |
| `FloorListViewModel` | `UpdateFloorCommand` | `UpdateFloorArgs.FloorId` |

After such a command completes successfully, and the floor-scoped read (C2) returned a `FloorDto` for that floor:

- `FloorList.Items` holds the same number of elements; the element at the refreshed floor's index is a **new**
  `FloorItem` mapped from the returned `FloorDto`; **every other element is reference-identical** to what it was
  before the command.
- `FloorList.Items` raised **exactly one** `CollectionChanged`, with `Action == Replace`, `NewStartingIndex ==
  OldStartingIndex ==` that index. No `Reset`, `Remove` or `Add` is raised.
- `Dungeon` is replaced by a copy whose `Floors` element at that index is the returned `FloorDto` and whose other
  elements are reference-identical, so `Dungeon.Floors` and `FloorList.Items` keep describing the same floors.
  (At startRef the only code in the App reading a detail's `Floors` is `FloorListViewModel.cs:61`, `dto.Floors`:
  `grep -rnE --include=*.cs --include=*.xaml --exclude-dir=obj --exclude-dir=bin '\.Floors\b' src/DungeonMaster.App | grep -v 'Entities\.Floors;'`
  prints that line and the comment at `:57`, and nothing else. It must still not be left stale.)
- `GetDungeonDetailQuery` was **not** dispatched.

**Error cases -- each falls back to the full reload (`ReloadAsync`, behaviour unchanged) and raises no `Replace`:**

| Case | Outcome |
| --- | --- |
| The floor-scoped read returns `null` (the floor or the dungeon was removed between the write and the read) | full reload; its own not-found handling applies (`LoadDungeonAsync`, `DungeonDetailViewModel.cs:183-203`) |
| No element of `FloorList.Items` has the floor's `Id` | full reload |
| `Dungeon` is `null` | full reload |

A **failed** command (its `Result.IsFailure`) refreshes nothing and reloads nothing, exactly as today: it reports
through `ReportFailure` and returns.

### 3.2 Membership- and header-shaped mutations: unchanged full reload

These **four** keep awaiting the full reload, each for a stated reason that T2 records at its declaration
(`IDungeonEditingContext.cs:56-58` already requires a documented reason for any exception to the policy):

| View model | Command | Why it reloads the whole detail |
| --- | --- | --- |
| `FloorListViewModel` | `AddFloorCommand` | changes the floor list's membership |
| `FloorListViewModel` | `RemoveFloorCommand` | changes membership and renumbers every later floor |
| `FloorListViewModel` | `ReorderFloorCommand` | renumbers every floor between the two positions |
| `DungeonHeaderViewModel` | `SaveCommand` | changes the dungeon-level projection itself (`Dungeon`, the header) |

Each of these leaves `FloorList.Items` exactly as `Populate` does today: `Clear()` then one `Add` per floor
(`Populate`, `FloorListViewModel.cs:42-65`).

## 4. Operations and their consumers

| Operation | Defined by | Consumed by |
| --- | --- | --- |
| `MapEditorState.PropertyChanged` for `SelectedFloorId` / `FocusedAreaId` | T2 | T3 -- the sidebar rows' `IsExpanded` / `IsSelected` bindings |
| `DungeonDetailViewModel.Editor` | T2 | T3 -- the page (replacing its own `_editor`) and the row bindings' source |
| Section 3.1 replace-in-place semantics of the nine commands | T2 | T3 -- the page's `RefreshFloorAsync` stops re-querying and only re-resolves `_drawable.Floor`; the templated floor list rebuilds one row |
| Section 3.2 full-reload semantics of the four commands | T2 (preserved) | T3 -- the keyboard-reorder focus restoration (`DungeonDetailPage.xaml.cs:2315-2321`) still runs after a full rebuild |

The floor-refresh member T2 adds to `IDungeonEditingContext` (for the child view models to call) is **not** in
this table: its only callers are T2's own `MapCanvasViewModel` and `FloorListViewModel`, so it is not a boundary
between tasks; T2's criteria cover it.
