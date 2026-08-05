using DungeonMaster.App.Assets.ObjectAssets;
using Plugin.Maui.KeyListener;
using DungeonMaster.App.Behaviors;
using DungeonMaster.App.Controls;
using DungeonMaster.App.Extensions;
using DungeonMaster.App.Graphics;
using DungeonMaster.App.Theme;
using DungeonMaster.App.ViewModels;
using DungeonMaster.App.Views.DungeonDetail;
using DungeonMaster.Application.Common.Map;
using DungeonMaster.Application.Features.Dungeons.Queries.GetDungeonDetail;
using DungeonMaster.Domain.Entities.Floor;
using DungeonMaster.Domain.ValueObjects;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace DungeonMaster.App.Views;

/// <summary>
///		Displays the detail view for a single dungeon, including floors, areas, and the map canvas.
///		Supports inline editing, floor reordering via drag-and-drop, area management,
///		and a floor-level editing state machine for drawing and moving area outlines.
/// </summary>
public partial class DungeonDetailPage : ContentPage
{
	private readonly DungeonDetailViewModel _viewModel;
	private readonly FloorMapDrawable _drawable;

	// Floor list state
	private Guid? _selectedFloorId;
	private View? _dragPlaceholder;
	private FloorSidebarItem? _draggingItem;
	private FloorSidebarItem? _currentPlaceholderTarget;
	private bool _placeholderAfterTarget;

	// Area focus — drives the canvas dim only. Never gates input.
	private Guid? _focusedAreaId;

	// Draw state machine
	private enum DrawState { Idle, DrawingReady, Drawing }
	private DrawState _drawState = DrawState.Idle;

	private bool _loreOpen;


	// Current drag state
	private bool _isMoveDragging;
	private Guid? _draggingAreaId;
	private TilePoint? _moveGrabTile;

	// Pan tool anchor (screen space)
	private PointF? _panAnchorScreen;

	// Ghost placement rotation state (cycles when R pressed in Place mode)
	private ObjectRotation _ghostRotation = ObjectRotation.None;

	// Object action popup state
	private Guid? _hoveredObjectId;   // canvas highlight (follows cursor)
	private Guid? _selectedObjectId;  // popup target (set on click/long-press)
	private RectF _popupBounds;       // hit-zone guard for popup button dispatch
	private PointF _lastTouchPoint;
	private readonly CanvasLongPressDetector _longPressDetector = new();

	// Object drag-to-move state
	private const float ObjectDragThreshold = 6f;
	private Guid? _pendingObjectClickId;    // touch down on object, waiting to disambiguate click vs drag
	private PointF _objectDragAnchorScreen;
	private bool _isObjectDragConfirmed;
	private CanvasPoint? _objectDragTargetCanvas;   // floor-absolute pixel position being previewed
	private bool _objectLongPressHandled;            // prevents EndInteraction from re-triggering click after long press

	// Keyboard guard — suppresses tool shortcuts while a text field is focused.
	private bool _isTextInputFocused;

	private KeyboardBehavior? _keyboardBehavior;

#if WINDOWS
	[System.Runtime.InteropServices.DllImport("user32.dll")]
	private static extern short GetAsyncKeyState(int vKey);
#endif

	private const float ZoomStep = 0.1f;
	private const float ZoomSnapThreshold = 0.05f;

	/// <summary>
	///		Initializes a new instance of <see cref="DungeonDetailPage"/>, wires the map drawable,
	///		and subscribes to view model property and floor collection changes.
	/// </summary>
	public DungeonDetailPage(DungeonDetailViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		_drawable = new FloorMapDrawable();
		BindingContext = viewModel;

		MapCanvas.Drawable = _drawable;

		// Wire object palette events
		ObjectPalette.ActiveObjectChanged += OnPaletteObjectChanged;
		ObjectPalette.CloseRequested += (_, _) =>
			SetActiveTool(MapTool.Pan);
		ObjectPalette.SecondaryPaletteRequested += OnSecondaryPaletteRequested;
		ObjectPalette.SecondaryPaletteDismissed += OnSecondaryPaletteDismissed;
		ObjectPalette.PackChanged += (_, _) =>
		{
			_drawable.ClearObjectImageCache();
			MapCanvas.Invalidate();
		};

		// Suppress keyboard shortcuts while any inline editor is focused.
		// Standard MAUI Editor/Entry: use Focused/Unfocused.
		// DmInlineEditor wraps an internal Entry — use its dedicated events.
		DescriptionEditor.Focused += (_, _) =>
			_isTextInputFocused = true;
		DescriptionEditor.Unfocused += (_, _) =>
			_isTextInputFocused = false;
		NotesEditor.Focused += (_, _) =>
			_isTextInputFocused = true;
		NotesEditor.Unfocused += (_, _) =>
			_isTextInputFocused = false;
		FloorNameToolbarEditor.EditingStarted += (_, _) =>
			_isTextInputFocused = true;
		FloorNameToolbarEditor.EditingEnded += (_, _) =>
			_isTextInputFocused = false;
		AreaNameToolbarEditor.EditingStarted += (_, _) =>
			_isTextInputFocused = true;
		AreaNameToolbarEditor.EditingEnded += (_, _) =>
			_isTextInputFocused = false;

		AttachPopupButtonBehavior(
			PopupBtnCCW,
			goldHover: true,
			() =>
				RotateObjectAsync(clockwise: false));
		AttachPopupButtonBehavior(
			PopupBtnCW,
			goldHover: true,
			() =>
				RotateObjectAsync(clockwise: true));
		AttachPopupButtonBehavior(
			PopupBtnDelete,
			goldHover: false,
			() =>
				DeleteSelectedObjectAsync());
		AttachPopupButtonBehavior(
			PopupBtnScaleDown,
			goldHover: true,
			() => ScaleObjectAsync(down: true));
		AttachPopupButtonBehavior(
			PopupBtnScaleUp,
			goldHover: true,
			() => ScaleObjectAsync(down: false));

		ControlsPalette.RotateRequested += (_, dir) =>
		{
			if (ObjectPalette.ActiveObject is not null)
			{
				_ghostRotation = dir == RotationDirection.Clockwise
					? (ObjectRotation)(((int)_ghostRotation + 1) % 4)
					: (ObjectRotation)(((int)_ghostRotation + 3) % 4);
				_drawable.GhostRotation = _ghostRotation;
				MapCanvas.Invalidate();
			}
			else
			{
				_selectedObjectId ??= _hoveredObjectId;
				if (_selectedObjectId is not null)
				{
					_ = RotateObjectAsync(dir == RotationDirection.Clockwise);
				}
			}
		};
		ControlsPalette.ScaleRequested += (_, dir) =>
		{
			var down = dir == ScaleDirection.Down;
			if (ObjectPalette.ActiveObject is not null)
			{
				_drawable.GhostScale = down
					? ObjectScale.StepDown(_drawable.GhostScale)
					: ObjectScale.StepUp(_drawable.GhostScale);
				ControlsPalette.UpdateScale(_drawable.GhostScale);
				MapCanvas.Invalidate();
			}
			else
			{
				_selectedObjectId ??= _hoveredObjectId;
				if (_selectedObjectId is not null)
				{
					_ = ScaleObjectAsync(down);
				}
			}
		};

		AttachRuntimeHandlers();
	}

	// ─── Page lifecycle ─────────────────────────────────────────────────────────
	//
	// Subscriptions are split into two groups:
	//
	//  * One-time construction wiring (above) — palette events, popup button behaviors and
	//    inline-editor focus handlers. Every one of those targets a child control this page
	//    owns outright, so they live and die with the page and never need detaching.
	//
	//  * Runtime handlers (below) — the view model subscriptions, the canvas handler hook,
	//    the long-press detector and the keyboard behavior. These used to be torn down in
	//    OnNavigatedFrom and never restored. CommunityToolkit's ShowPopupAsync presents its
	//    popup as a modal page, which raises OnNavigatedFrom on the page underneath, so the
	//    first confirm dialog permanently crippled the page.
	//
	// Both the page and its view model are registered transient, so the page is the sole owner
	// of everything it subscribes to and unsubscribing on navigation buys nothing. The runtime
	// handlers therefore stay attached for the page's whole lifetime and are only released on
	// genuine teardown (handler disconnect). AttachRuntimeHandlers is idempotent and is also
	// invoked from OnAppearing/OnNavigatedTo so the page self-heals if it is ever reconnected.
	//
	// Keyboard shortcuts are instead gated at dispatch time by IsPageInteractive(), which
	// suppresses them while a modal/popup is up or another page is on top — the behaviour the
	// old teardown was reaching for, without the permanent breakage.

	private bool _runtimeHandlersAttached;

	private void AttachRuntimeHandlers()
	{
		if (_runtimeHandlersAttached)
		{
			return;
		}

		_runtimeHandlersAttached = true;

		_viewModel.PropertyChanged += OnViewModelPropertyChanged;
		_viewModel.Floors.CollectionChanged += OnFloorsChanged;

		MapCanvas.HandlerChanged += OnMapCanvasHandlerChanged;
		_longPressDetector.LongPressed += OnObjectLongPressed;

		// Plugin.Maui.KeyListener: fires regardless of which element has focus,
		// solving the GraphicsView focus-sink problem on Windows.
		if (_keyboardBehavior is null)
		{
			_keyboardBehavior = new KeyboardBehavior();
			_keyboardBehavior.KeyDown += OnKeyDown;
			Behaviors.Add(_keyboardBehavior);
		}

		// Re-run the native scroll-zoom hook in case the canvas handler already exists.
		DetachScrollZoom();
		AttachScrollZoom();
	}

	private void DetachRuntimeHandlers()
	{
		if (!_runtimeHandlersAttached)
		{
			return;
		}

		_runtimeHandlersAttached = false;

		_viewModel.PropertyChanged -= OnViewModelPropertyChanged;
		_viewModel.Floors.CollectionChanged -= OnFloorsChanged;

		MapCanvas.HandlerChanged -= OnMapCanvasHandlerChanged;
		_longPressDetector.LongPressed -= OnObjectLongPressed;

		if (_keyboardBehavior is not null)
		{
			_keyboardBehavior.KeyDown -= OnKeyDown;
			Behaviors.Remove(_keyboardBehavior);
			_keyboardBehavior = null;
		}

		DetachScrollZoom();
	}

	/// <inheritdoc/>
	protected override void OnAppearing()
	{
		base.OnAppearing();
		AttachRuntimeHandlers();
	}

	/// <inheritdoc/>
	protected override void OnNavigatedTo(NavigatedToEventArgs args)
	{
		base.OnNavigatedTo(args);
		AttachRuntimeHandlers();
	}

	/// <inheritdoc/>
	protected override void OnHandlerChanged()
	{
		base.OnHandlerChanged();

		// A null handler means the page has left the visual tree for good — this is the only
		// genuine teardown point, and the only place the long-press detector may be disposed.
		if (Handler is null)
		{
			DetachRuntimeHandlers();
			_longPressDetector.Dispose();
		}
	}

	// ─── Keyboard shortcuts ─────────────────────────────────────────────────────

	private bool IsTextInputActive()
	{
#if WINDOWS
		// Check native focus — more reliable than MAUI Focused events because DmInlineEditor
		// focuses an internal TextBox that doesn't bubble to the ContentView.Focused event.
		if (Window?.Handler?.PlatformView is Microsoft.UI.Xaml.Window winUiWindow)
		{
			var focused = Microsoft.UI.Xaml.Input.FocusManager.GetFocusedElement(winUiWindow.Content.XamlRoot);
			return focused is Microsoft.UI.Xaml.Controls.TextBox;
		}
#elif MACCATALYST
		// TODO(macOS): UIKit does not expose a global FocusManager equivalent. The fallback
		// _isTextInputFocused flag is populated via MAUI Focused/Unfocused events and the
		// DmInlineEditor.EditingStarted/EditingEnded events wired in the constructor.
		// If DmInlineEditor's internal UITextField fires focus events that don't surface through
		// those delegates, add a UITextFieldDelegate or UIResponder.IsFirstResponder check here.
#endif
		return _isTextInputFocused;
	}

	// The keyboard behavior stays attached for the page's lifetime, so shortcuts are gated here
	// instead: they are ignored while a popup/modal is displayed or another page is on top.
	private bool IsPageInteractive()
	{
		if (Navigation?.ModalStack.Count > 0)
			return false;

		var current = Shell.Current?.CurrentPage;
		return current is null || ReferenceEquals(current, this);
	}

	private void OnKeyDown(object? sender, KeyPressedEventArgs e)
	{
		if (!IsPageInteractive())
			return;

		if (IsTextInputActive())
			return;

		switch (char.ToLower(e.KeyChar))
		{
			case 'd':
				if (_viewModel.ActiveTool == MapTool.Draw && _drawState == DrawState.Drawing)
					AbortInProgressDraw();   // pressing D while drawing cancels in-progress vertices
				else
					ToggleOrSetTool(MapTool.Draw);
				break;
			case 'm': ToggleOrSetTool(MapTool.Move);   break;
			case 'h': ToggleOrSetTool(MapTool.Pan);    break;
			case 'p': ToggleOrSetTool(MapTool.Place);  break;
			case 's': ToggleOrSetTool(MapTool.Select); break;
			case 'e': ToggleOrSetTool(MapTool.Erase); break;
			case 'r':
				if (_viewModel.ActiveTool == MapTool.Place)
				{
					if (ObjectPalette.ActiveObject is not null)
					{
						// Rotate ghost preview.
						_ghostRotation = (ObjectRotation)(((int)_ghostRotation + 1) % 4);
						_drawable.GhostRotation = _ghostRotation;
						MapCanvas.Invalidate();
					}
					else
					{
						// Rotate selected or hovered placed object.
						_selectedObjectId ??= _hoveredObjectId;
						if (_selectedObjectId is not null)
							_ = RotateObjectAsync(clockwise: true);
					}
				}
				break;
			case '[':
				if (_viewModel.ActiveTool == MapTool.Place)
				{
					if (ObjectPalette.ActiveObject is not null)
					{
						_drawable.GhostScale = ObjectScale.StepDown(_drawable.GhostScale);
						ControlsPalette.UpdateScale(_drawable.GhostScale);
						MapCanvas.Invalidate();
					}
					else
					{
						_selectedObjectId ??= _hoveredObjectId;
						if (_selectedObjectId is not null)
						{
							_ = ScaleObjectAsync(down: true);
						}
					}
				}
				break;
			case ']':
				if (_viewModel.ActiveTool == MapTool.Place)
				{
					if (ObjectPalette.ActiveObject is not null)
					{
						_drawable.GhostScale = ObjectScale.StepUp(_drawable.GhostScale);
						ControlsPalette.UpdateScale(_drawable.GhostScale);
						MapCanvas.Invalidate();
					}
					else
					{
						_selectedObjectId ??= _hoveredObjectId;
						if (_selectedObjectId is not null)
						{
							_ = ScaleObjectAsync(down: false);
						}
					}
				}
				break;
			case '0':
				if (e.Modifiers.HasFlag(KeyboardModifiers.Control))
					ResetViewport();
				break;
		}
	}

	// If tool is already active, revert to Pan (deactivate); otherwise activate.
	private void ToggleOrSetTool(MapTool tool)
	{
		if (_viewModel.ActiveTool == tool)
			SetActiveTool(MapTool.Pan);
		else
			SetActiveTool(tool);
	}

	// ─── Zoom controls ──────────────────────────────────────────────────────────

	private void OnZoomInClicked(object? sender, EventArgs e) =>
		ApplyZoom(_drawable.ZoomLevel + ZoomStep);

	private void OnZoomOutClicked(object? sender, EventArgs e) =>
		ApplyZoom(_drawable.ZoomLevel - ZoomStep);

	private void ApplyZoom(float raw)
	{
		if (Math.Abs(raw - 1.0f) < ZoomSnapThreshold)
			raw = 1.0f;
		_drawable.ZoomLevel = Math.Clamp(raw, 0.25f, 4.0f);
		ZoomPercentLabel.Text = $"{(int)Math.Round(_drawable.ZoomLevel * 100)}%";
		MapCanvas.Invalidate();
	}

	private void ResetViewport()
	{
		_drawable.ZoomLevel = 1.0f;
		_drawable.PanOffset = PointF.Zero;
		ZoomPercentLabel.Text = "100%";
		MapCanvas.Invalidate();
	}

	private void OnMapCanvasHandlerChanged(object? sender, EventArgs e)
	{
		DetachScrollZoom();
		AttachScrollZoom();
	}

	private void AttachScrollZoom()
	{
#if WINDOWS
		if (MapCanvas.Handler?.PlatformView is Microsoft.UI.Xaml.FrameworkElement el)
			el.PointerWheelChanged += OnMapScrollWheel;
#elif MACCATALYST
		// On macCatalyst the MAUI PinchGestureRecognizer is mapped to a UIPinchGestureRecognizer
		// and fires on both trackpad pinch and mouse scroll-wheel zoom (treated as pinch by UIKit).
		// We attach it to the MAUI view rather than the native view so that MAUI can dispatch it.
		// TODO(macOS): MAUI's PinchGestureRecognizer does not expose the pinch centroid, so zoom
		// always scales around the canvas center rather than the cursor position. A native
		// UIPinchGestureRecognizer on the platform view could be used to get the centroid if needed.
		if (_pinchGesture is null)
		{
			_pinchGesture = new PinchGestureRecognizer();
			_pinchGesture.PinchUpdated += OnMapPinchUpdated;
			MapCanvas.GestureRecognizers.Add(_pinchGesture);
		}
#endif
	}

	private void DetachScrollZoom()
	{
#if WINDOWS
		if (MapCanvas.Handler?.PlatformView is Microsoft.UI.Xaml.FrameworkElement el)
			el.PointerWheelChanged -= OnMapScrollWheel;
#elif MACCATALYST
		if (_pinchGesture is not null)
		{
			_pinchGesture.PinchUpdated -= OnMapPinchUpdated;
			MapCanvas.GestureRecognizers.Remove(_pinchGesture);
			_pinchGesture = null;
		}
#endif
	}

#if WINDOWS
	private void OnMapScrollWheel(object sender,
		Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
	{
		var delta = e.GetCurrentPoint((Microsoft.UI.Xaml.UIElement)sender)
					  .Properties.MouseWheelDelta;
		var direction = delta > 0 ? 1f : -1f;
		ApplyZoom(_drawable.ZoomLevel + direction * ZoomStep);
	}
#elif MACCATALYST
	private PinchGestureRecognizer? _pinchGesture;
	private float _pinchStartZoom = 1f;

	private void OnMapPinchUpdated(object? sender, PinchGestureUpdatedEventArgs e)
	{
		switch (e.Status)
		{
			case GestureStatus.Started:
				_pinchStartZoom = _drawable.ZoomLevel;
				break;
			case GestureStatus.Running:
				ApplyZoom(_pinchStartZoom * (float)e.Scale);
				break;
		}
	}
#endif

	private void OnViewModelPropertyChanged(
		object? sender,
		System.ComponentModel.PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(DungeonDetailViewModel.Dungeon) && _viewModel.Dungeon is not null)
		{
			TitleBanner.Title = _viewModel.DungeonName;
			DescriptionEditor.Text = _viewModel.DungeonDescription;
			NotesEditor.Text = _viewModel.DungeonNotes;
		}
	}

	private void OnFloorsChanged(
		object? sender,
		System.Collections.Specialized.NotifyCollectionChangedEventArgs e) =>
		RebuildFloorList();

	private void RebuildFloorList()
	{
		FloorListContainer.Children.Clear();

		foreach (var floor in _viewModel.Floors)
		{
			var item = new FloorSidebarItem { Floor = floor };

			item.DeleteRequested += async (_, floorToDelete) =>
			{
				var confirmed = await Navigation.ShowConfirmPopupAsync(
					"Delete Floor",
					$"Delete Floor {floorToDelete.Number}? All areas on this floor will be removed.",
					"DELETE",
					"CANCEL",
					isDestructive: true);

				if (!confirmed)
				{
					return;
				}

				// Deleting the selected floor must drop every piece of state that points at it,
				// otherwise the drawable keeps rendering the deleted floor's projection.
				// Deleting any other floor leaves the current selection untouched.
				var wasSelected = _selectedFloorId == floorToDelete.Id;
				if (wasSelected)
				{
					ClearAreaFocus();
					_selectedFloorId = null;
					_drawable.Floor = null;
					_drawable.HoveredAreaId = null;
					ResetDrawState();
					FloorNameToolbarEditor.Text = string.Empty;
					MapToolbar.IsVisible = false;
				}

				await _viewModel.RemoveFloorCommand.ExecuteAsync(floorToDelete.Id);

				if (wasSelected)
				{
					UpdateToolbarButtonState();
					MapCanvas.Invalidate();
				}
			};

			item.SelectRequested += (_, _) =>
			{
				// Re-tapping the floor card while an area is focused clears the focus.
				if (_focusedAreaId is not null && _selectedFloorId == floor.Id)
				{
					ClearAreaFocus();
					return;
				}

				_selectedFloorId = floor.Id;
				CollapseAllExcept(item);
				RefreshMap();
			};

			item.ReorderRequested += async (_, args) =>
			{
				_selectedFloorId = args.DraggedFloorId;
				await _viewModel.ReorderFloorCommand.ExecuteAsync((args.DraggedFloorId, args.TargetNumber));
			};

			item.DragStarted += (_, _) =>
			{
				_draggingItem = item;
				item.Opacity = 0;
			};

			item.DragEnded += (_, _) =>
			{
				_draggingItem = null;
				item.Opacity = 1.0;
				ClearDragPlaceholder();
			};

			item.PlaceholderRequested += (_, _) =>
				ShowDragPlaceholder(item);

			item.PlaceholderCleared += (_, _) =>
				ClearDragPlaceholder();

			item.AddAreaRequested += async (_, _) =>
				await _viewModel.AddAreaCommand.ExecuteAsync(floor.Id);

			item.AreaSelectRequested += (_, area) =>
			{
				if (_focusedAreaId == area.Id)
				{
					ClearAreaFocus();
					return;
				}

				FocusArea(floor.Id, area);
			};

			item.AreaHoverStarted += (_, area) =>
			{
				_drawable.HoveredAreaId = area.Id;
				MapCanvas.Invalidate();
			};

			item.AreaHoverEnded += (_, _) =>
			{
				_drawable.HoveredAreaId = null;
				MapCanvas.Invalidate();
			};

			// Sidebar delete is the only way to remove an area with an empty outline —
			// the Erase tool needs outline geometry to hit-test against.
			item.AreaDeleteRequested += async (_, areaToDelete) =>
				await DeleteAreaAsync(floor.Id, areaToDelete);

			FloorListContainer.Children.Add(item);

			if (floor.Id == _selectedFloorId)
			{
				item.Select();
			}
		}

		// A rebuild recreates the area items, so re-apply the focused area's selected visual.
		UpdateActiveAreaSidebarHighlight();
	}

	// Confirms and removes an area from the sidebar. Placed objects belong to the floor,
	// so they survive area deletion; only the annotation is removed.
	private async Task DeleteAreaAsync(Guid floorId, AreaDto area)
	{
		var confirmed = await Navigation.ShowConfirmPopupAsync(
			"Delete Area",
			$"Delete area \"{area.Name}\"? Objects placed on this floor are not affected.",
			"DELETE",
			"CANCEL",
			isDestructive: true);

		if (!confirmed)
		{
			return;
		}

		if (_focusedAreaId == area.Id)
		{
			ClearAreaFocus();
		}

		_drawable.HoveredAreaId = null;

		await _viewModel.RemoveAreaCommand.ExecuteAsync((floorId, area.Id));
		await RefreshFloorAsync();
		MapCanvas.Invalidate();
	}

	// ─── Lore drawer ────────────────────────────────────────────────────────────

	private void OnLoreBookmarkTapped(object? sender, EventArgs e)
	{
		_loreOpen = !_loreOpen;
		LoreDrawer.IsVisible = _loreOpen;
		LoreBookmarkLabel.Text = _loreOpen
			? "LORE & NOTES  ▴"
			: "LORE & NOTES  ▾";
	}

	private void OnLoreDrawerTapped(object? sender, TappedEventArgs e)
	{
		TitleBanner.TryCommit();
		DescriptionEditor.TryCommit();
		NotesEditor.TryCommit();
	}

	// ─── Area focus ─────────────────────────────────────────────────────────────

	// Focuses an area so the canvas dims every other area. Display only — it gates nothing.
	private void FocusArea(Guid floorId, AreaDto area)
	{
		if (floorId != _selectedFloorId)
		{
			return;
		}

		// Guard against a stale sidebar row whose area no longer exists in the current data.
		var currentFloor = _viewModel.Floors.FirstOrDefault(floorItem => floorItem.Id == floorId);
		if (currentFloor?.Areas.Any(areaItem => areaItem.Id == area.Id) != true)
		{
			ClearAreaFocus();
			return;
		}

		if (_drawState == DrawState.Drawing)
		{
			AbortInProgressDraw();
		}

		// Abort any in-progress move drag
		if (_isMoveDragging)
		{
			_isMoveDragging = false;
			_draggingAreaId = null;
			_moveGrabTile = null;
			_drawable.DragPreviewAreaId = null;
			_drawable.DragPreviewDelta = null;
		}

		_focusedAreaId = area.Id;
		_drawable.ActiveAreaId = area.Id;

		AreaNameToolbarEditor.Text = area.Name ?? string.Empty;
		AreaNameToolbarEditor.IsVisible = true;

		UpdateActiveAreaSidebarHighlight();
		UpdateToolbarButtonState();
		MapCanvas.Invalidate();
	}

	// Clears the area focus so every area renders at full opacity again.
	private void ClearAreaFocus()
	{
		_focusedAreaId = null;
		_drawable.ActiveAreaId = null;

		AreaNameToolbarEditor.Text = string.Empty;
		AreaNameToolbarEditor.IsVisible = false;

		UpdateActiveAreaSidebarHighlight();
		MapCanvas.Invalidate();
	}

	private void AbortInProgressDraw()
	{
		_viewModel.DrawingSession.Reset();
		_drawState = DrawState.DrawingReady;
		_drawable.InProgressVertices = [];
		_drawable.PhantomEnd = null;
		_drawable.ActiveAreaId = _focusedAreaId;
	}

	private void SetCanvasCursor(MapTool tool, bool isDragging = false)
	{
#if WINDOWS
		if (MapCanvas.Handler?.PlatformView is Microsoft.UI.Xaml.UIElement element)
		{
			var shape = tool switch
			{
				MapTool.Draw   => Microsoft.UI.Input.InputSystemCursorShape.Cross,
				MapTool.Move   => Microsoft.UI.Input.InputSystemCursorShape.SizeAll,
				MapTool.Pan    => isDragging
					? Microsoft.UI.Input.InputSystemCursorShape.SizeAll
					: Microsoft.UI.Input.InputSystemCursorShape.Hand,
				MapTool.Erase  => Microsoft.UI.Input.InputSystemCursorShape.Cross,
				_              => Microsoft.UI.Input.InputSystemCursorShape.Arrow
			};

			typeof(Microsoft.UI.Xaml.UIElement)
				.GetProperty(
					"ProtectedCursor",
					System.Reflection.BindingFlags.Instance |
					System.Reflection.BindingFlags.NonPublic |
					System.Reflection.BindingFlags.Public)
				?.SetValue(element, Microsoft.UI.Input.InputSystemCursor.Create(shape));
		}
#elif MACCATALYST
		// On macCatalyst the GraphicsView platform view is a UIView; cursor shape is set via NSCursor.
		// UIHoverGestureRecognizer fires continuously while the pointer is over the view, so we push
		// the cursor once here and rely on the next hover event (or tool switch) to pop it.
		// TODO(macOS): NSCursor does not have a SizeAll/crosshair that maps 1:1 to WinUI's SizeAll.
		// The closest available cursors are used below. Consider caching the hover recognizer on the
		// canvas view and updating its action delegate rather than re-adding it each time.
		switch (tool)
		{
			case MapTool.Draw:
			case MapTool.Erase:
				AppKit.NSCursor.CrosshairCursor.Set();
				break;
			case MapTool.Move:
				AppKit.NSCursor.OpenHandCursor.Set();
				break;
			case MapTool.Pan:
				(isDragging ? AppKit.NSCursor.ClosedHandCursor : AppKit.NSCursor.OpenHandCursor).Set();
				break;
			default:
				AppKit.NSCursor.ArrowCursor.Set();
				break;
		}
#endif
	}

	// ─── Toolbar handlers ────────────────────────────────────────────────────────


	private void SetActiveTool(MapTool tool)
	{
		_viewModel.ActiveTool = tool;

		switch (tool)
		{
			case MapTool.Draw:
				_drawState = _viewModel.DrawingSession.IsActive ? DrawState.Drawing : DrawState.DrawingReady;
				break;
			case MapTool.Move:
			case MapTool.Pan:
			case MapTool.Place:
			case MapTool.Select:
			case MapTool.Erase:
				if (_drawState == DrawState.Drawing)
				{
					AbortInProgressDraw();
				}
				_drawState = DrawState.Idle;
				break;
		}

		// Clear canvas hover highlight when leaving tools that show it.
		if (tool != MapTool.Move && tool != MapTool.Select && tool != MapTool.Erase)
		{
			_drawable.HoveredAreaId = null;
		}

		// Dismiss object popup and clear any hover highlight when switching tools.
		_selectedObjectId = null;
		_hoveredObjectId = null;
		_drawable.HoveredObjectId = null;
		_pendingObjectClickId = null;
		_isObjectDragConfirmed = false;
		_drawable.DraggingObjectId = null;
		_drawable.DraggingObjectTargetCanvas = null;
		ObjectActionPopup.IsVisible = false;

		ObjectPalette.IsVisible = tool == MapTool.Place;
		ControlsPalette.IsVisible = tool == MapTool.Place && ObjectPalette.ActiveObject is not null;
		if (tool != MapTool.Place)
		{
			OnSecondaryPaletteDismissed(null, EventArgs.Empty);
			_drawable.GhostObjectKind = null;
			_drawable.GhostCanvasPosition = null;
			_drawable.GhostRotation = ObjectRotation.None;
			_ghostRotation = ObjectRotation.None;
			_drawable.GhostScale = ObjectScale.Default;
			ControlsPalette.ResetScale();
			MapCanvas.Invalidate();
		}

		SetCanvasCursor(tool);
		UpdateToolbarButtonState();
	}

	private void OnToolDrawClicked(object? sender, TappedEventArgs e) =>
		ToggleOrSetTool(MapTool.Draw);

	private void OnToolMoveClicked(object? sender, TappedEventArgs e) =>
		ToggleOrSetTool(MapTool.Move);

	private void OnToolSelectClicked(object? sender, TappedEventArgs e) =>
		ToggleOrSetTool(MapTool.Select);

	private void OnToolPanClicked(object? sender, TappedEventArgs e) =>
		ToggleOrSetTool(MapTool.Pan);

	private void OnToolPlaceClicked(object? sender, TappedEventArgs e) =>
		ToggleOrSetTool(MapTool.Place);

	private void OnToolEraseClicked(object? sender, TappedEventArgs e) =>
		ToggleOrSetTool(MapTool.Erase);

	private async void EraseAreaAsync(Guid floorId, AreaDto area)
	{
		_drawable.HoveredAreaId = null;
		MapCanvas.Invalidate();
		await _viewModel.RemoveAreaCommand.ExecuteAsync((floorId, area.Id));
		await RefreshFloorAsync();
		MapCanvas.Invalidate();
	}

	// ─── Object palette handlers ─────────────────────────────────────────────────

	private async void PlaceObjectAsync(
		Guid floorId,
		ObjectKind kind,
		CanvasPoint position,
		ObjectRotation rotation,
		float scale)
	{
		await _viewModel.AddPlacedObjectCommand.ExecuteAsync(
			new DungeonDetailViewModel.PlaceObjectArgs(floorId, kind, position, rotation, scale));
		// Reload only the floor data — do NOT call RefreshMap(), which would reset tool/context state.
		await RefreshFloorAsync();
		MapCanvas.Invalidate();
	}

	private void OnPaletteObjectChanged(object? sender, ObjectKind? kind)
	{
		_drawable.GhostObjectKind = kind;
		_ghostRotation = ObjectRotation.None;
		_drawable.GhostRotation = ObjectRotation.None;
		_drawable.GhostScale = ObjectScale.Default;
		ControlsPalette.ResetScale();
		ControlsPalette.IsVisible = kind is not null;
		MapCanvas.Invalidate();
	}

	private void OnSecondaryPaletteRequested(object? sender, SecondaryPaletteView palette)
	{
		if (!MapOverlay.Children.Contains(palette))
		{
			palette.HorizontalOptions = LayoutOptions.Start;
			palette.VerticalOptions = LayoutOptions.End;
			// Main palette: 8px left margin + 216px wide = 224px right edge; 8px gap before secondary.
			palette.Margin = new Thickness(232, 0, 0, 8);
			MapOverlay.Children.Add(palette);
		}
		palette.IsVisible = true;
	}

	private void OnSecondaryPaletteDismissed(object? sender, EventArgs e)
	{
		foreach (var child in MapOverlay.Children.OfType<SecondaryPaletteView>().ToList())
		{
			child.IsVisible = false;
		}
	}

	private async void OnFloorNameToolbarCommitted(object? sender, string? name)
	{
		if (_selectedFloorId is null || string.IsNullOrWhiteSpace(name))
		{
			return;
		}

		var floor = _viewModel.Floors.FirstOrDefault(floorItem => floorItem.Id == _selectedFloorId);
		if (floor is null || name == floor.Name)
		{
			return;
		}

		await _viewModel.UpdateFloorCommand.ExecuteAsync((_selectedFloorId.Value, name));

		foreach (var child in FloorListContainer.Children)
		{
			if (child is FloorSidebarItem item && item.Floor?.Id == _selectedFloorId)
			{
				item.UpdateFloorName(name);
				break;
			}
		}
	}

	private async void OnAreaNameToolbarCommitted(object? sender, string? name)
	{
		if (_selectedFloorId is null || _focusedAreaId is null || string.IsNullOrWhiteSpace(name))
		{
			return;
		}

		var floor = _viewModel.Floors.FirstOrDefault(floorItem => floorItem.Id == _selectedFloorId);
		var area = floor?.Areas.FirstOrDefault(areaItem => areaItem.Id == _focusedAreaId);

		// The focused area is gone from the refreshed projection — the toolbar is pointing at a
		// phantom row. Drop the stale focus rather than silently no-op'ing the rename.
		if (area is null)
		{
			ClearAreaFocus();
			return;
		}

		if (name == area.Name)
		{
			return;
		}

		await _viewModel.UpdateAreaNameCommand.ExecuteAsync((_selectedFloorId.Value, _focusedAreaId.Value, name));

		// The command reloads the detail, so the sidebar has already rebuilt from fresh
		// projections. Re-point the drawable at the new floor instance and keep the
		// toolbar editor showing the persisted name.
		_drawable.Floor = _viewModel.Floors.FirstOrDefault(floorItem => floorItem.Id == _selectedFloorId);
		AreaNameToolbarEditor.Text = name;
		MapCanvas.Invalidate();
	}

	private void UpdateToolbarButtonState()
	{
		var isDrawActive = _viewModel.ActiveTool == MapTool.Draw;
		var isMoveActive = _viewModel.ActiveTool == MapTool.Move;
		var isPanActive = _viewModel.ActiveTool == MapTool.Pan;
		var isPlaceActive = _viewModel.ActiveTool == MapTool.Place;
		var isSelectActive = _viewModel.ActiveTool == MapTool.Select;
		var isEraseActive = _viewModel.ActiveTool == MapTool.Erase;

		var activeColor = DmColors.Gold;
		var mutedColor = DmColors.GoldMuted;
		var activeBrush = new SolidColorBrush(activeColor);
		var transparentBrush = new SolidColorBrush(Colors.Transparent);

		ToolDrawBorder.Stroke = isDrawActive ? activeBrush : transparentBrush;
		ToolDrawIcon.TextColor = isDrawActive ? activeColor : mutedColor;
		ToolDrawLabel.TextColor = isDrawActive ? activeColor : mutedColor;

		ToolMoveBorder.Stroke = isMoveActive ? activeBrush : transparentBrush;
		ToolMoveIcon.TextColor = isMoveActive ? activeColor : mutedColor;
		ToolMoveLabel.TextColor = isMoveActive ? activeColor : mutedColor;

		ToolSelectBorder.Stroke = isSelectActive ? activeBrush : transparentBrush;
		ToolSelectIcon.TextColor = isSelectActive ? activeColor : mutedColor;
		ToolSelectLabel.TextColor = isSelectActive ? activeColor : mutedColor;

		ToolEraseBorder.Stroke = isEraseActive ? activeBrush : transparentBrush;
		ToolEraseIcon.TextColor = isEraseActive ? activeColor : mutedColor;
		ToolEraseLabel.TextColor = isEraseActive ? activeColor : mutedColor;

		ToolPlaceBorder.Stroke = isPlaceActive ? activeBrush : transparentBrush;
		ToolPlaceIcon.TextColor = isPlaceActive ? activeColor : mutedColor;
		ToolPlaceLabel.TextColor = isPlaceActive ? activeColor : mutedColor;

		ToolPanBorder.Stroke = isPanActive ? activeBrush : transparentBrush;
		ToolPanIcon.TextColor = isPanActive ? activeColor : mutedColor;
		ToolPanLabel.TextColor = isPanActive ? activeColor : mutedColor;

		var hasVertices = _viewModel.DrawingSession.Vertices.Count > 0;
		UndoButton.IsEnabled = hasVertices;
		UndoButton.Opacity = hasVertices ? 1.0 : 0.35;
	}

	private void OnUndoClicked(object? sender, EventArgs e)
	{
		if (_viewModel.DrawingSession.UndoLastVertex())
		{
			if (!_viewModel.DrawingSession.IsActive)
			{
				_drawState = DrawState.DrawingReady;
			}

			_drawable.InProgressVertices = [.. _viewModel.DrawingSession.Vertices];
			var hasVertices = _viewModel.DrawingSession.Vertices.Count > 0;
			UndoButton.IsEnabled = hasVertices;
			UndoButton.Opacity = hasVertices ? 1.0 : 0.35;
			MapCanvas.Invalidate();
		}
	}

	// ─── Auto-save helpers ───────────────────────────────────────────────────────

	// Reloads floor data from the VM after any auto-save operation and syncs the drawable.
	private async Task RefreshFloorAsync()
	{
		if (_selectedFloorId is null) return;
		await _viewModel.LoadDungeonCommand.ExecuteAsync(_viewModel.Dungeon!.Id);
		_drawable.Floor = _viewModel.Floors.FirstOrDefault(floorItem => floorItem.Id == _selectedFloorId);
	}

	private void ResetDrawState()
	{
		_viewModel.DrawingSession.Reset();
		_drawState = _selectedFloorId is not null ? DrawState.DrawingReady : DrawState.Idle;

		_isMoveDragging = false;
		_draggingAreaId = null;
		_moveGrabTile = null;

		_drawable.InProgressVertices = [];
		_drawable.PhantomEnd = null;
		_drawable.PendingMoveOffsets.Clear();
		_drawable.PendingOutlines.Clear();
		_drawable.GhostAreas.Clear();
		_drawable.DragPreviewAreaId = null;
		_drawable.DragPreviewDelta = null;
	}

	// ─── Map canvas gesture handlers ────────────────────────────────────────────

	private void OnMapStartInteraction(object? sender, TouchEventArgs e)
	{
#if WINDOWS
		// GraphicsView's StartInteraction fires for all pointer buttons including right-click.
		// GetAsyncKeyState polls the right mouse button state synchronously so we can suppress
		// StartInteraction when the secondary button is pressed (right-click triggers Undo/Deselect
		// via the separate OnMapSecondaryTapped handler instead).
		const int VK_RBUTTON = 0x02;
		if ((GetAsyncKeyState(VK_RBUTTON) & 0x8000) != 0)
		{
			return;
		}
		// TODO(macOS): On macCatalyst there is no GetAsyncKeyState equivalent. The platform maps
		// a right-click / two-finger-tap to the SecondaryTapped event, which already routes to
		// OnMapSecondaryTapped. However, if the native UIGestureRecognizer fires StartInteraction
		// before SecondaryTapped on macCatalyst (which can happen), a spurious StartInteraction
		// will execute draw/place logic. Verify on a real Mac that right-click-only does NOT
		// trigger OnMapStartInteraction; if it does, add a UIEvent.IsSecondaryButton check here
		// using the platform view's gesture recognizers.
#endif

		_lastTouchPoint = new PointF((float)e.Touches[0].X, (float)e.Touches[0].Y);

		// GraphicsView on WinUI consumes all pointer events, so overlaid MAUI buttons never
		// fire Clicked. Instead we manually hit-test the popup rect and dispatch the action.
		if (ObjectActionPopup.IsVisible && _popupBounds.Contains(_lastTouchPoint.X, _lastTouchPoint.Y))
		{
			_longPressDetector.TouchStarted();
			var relX  = _lastTouchPoint.X - _popupBounds.X;
			var zoneW = _popupBounds.Width / 5f;
			if (relX < zoneW)
				_ = RotateObjectAsync(clockwise: false);
			else if (relX < zoneW * 2)
				_ = RotateObjectAsync(clockwise: true);
			else if (relX < zoneW * 3)
				_ = ScaleObjectAsync(down: true);
			else if (relX < zoneW * 4)
				_ = ScaleObjectAsync(down: false);
			else
				_ = DeleteSelectedObjectAsync();
			return;
		}

		_longPressDetector.TouchStarted();

		if (_viewModel.ActiveTool == MapTool.Pan)
		{
			_panAnchorScreen = new PointF((float)e.Touches[0].X, (float)e.Touches[0].Y);
			SetCanvasCursor(MapTool.Pan, isDragging: true);
			return;
		}

		var rawPoint = new PointF((float)e.Touches[0].X, (float)e.Touches[0].Y);
		var canvasHitPoint = _drawable.ScreenToCanvas(rawPoint);

		// Object interaction: the Select tool, or Place mode with no active palette item.
		// Left-click with a palette item = place; otherwise inspect/drag existing objects.
		if (_viewModel.ActiveTool == MapTool.Select
			|| (_viewModel.ActiveTool == MapTool.Place && ObjectPalette.ActiveObject is null))
		{
			var hitObjectId = GetObjectAtCanvasPoint(canvasHitPoint);
			if (hitObjectId is not null)
			{
				// Enter pending state — we disambiguate click vs drag in EndInteraction.
				_pendingObjectClickId = hitObjectId;
				_objectDragAnchorScreen = _lastTouchPoint;
				_isObjectDragConfirmed = false;
				_objectLongPressHandled = false;
				_hoveredObjectId = hitObjectId;
				_drawable.HoveredObjectId = hitObjectId;
				MapCanvas.Invalidate();
				return;
			}

			// Click on empty canvas — dismiss popup and clear hover highlight.
			_pendingObjectClickId = null;
			_selectedObjectId = null;
			_hoveredObjectId = null;
			_drawable.HoveredObjectId = null;
			if (ObjectActionPopup.IsVisible)
			{
				ObjectActionPopup.IsVisible = false;
			}
			MapCanvas.Invalidate();
			// Don't return — let the click fall through to tool logic.
		}

		if (_viewModel.ActiveTool == MapTool.Erase
			&& _selectedFloorId is not null)
		{
			var hit = HitTestArea(canvasHitPoint);
			if (hit is not null)
			{
				EraseAreaAsync(_selectedFloorId.Value, hit);
			}
			return;
		}

		if (_viewModel.ActiveTool == MapTool.Place
			&& ObjectPalette.ActiveObject is ObjectKind placedKind
			&& _selectedFloorId is not null)
		{
			var placeRaw = new PointF((float)e.Touches[0].X, (float)e.Touches[0].Y);
			var placeCanvas = _drawable.ScreenToCanvas(placeRaw);

			PlaceObjectAsync(
				_selectedFloorId.Value,
				placedKind,
				new CanvasPoint(placeCanvas.X, placeCanvas.Y),
				_ghostRotation,
				_drawable.GhostScale);
			return;
		}

		var canvasPoint = canvasHitPoint;

		if (_viewModel.ActiveTool == MapTool.Move && _selectedFloorId is not null)
		{
			var hit = HitTestArea(canvasPoint);
			if (hit is not null)
			{
				_draggingAreaId = hit.Id;
				_moveGrabTile = TileCoordinateHelper.SnapToGrid(canvasPoint.X, canvasPoint.Y, _drawable.TileSize);
				_drawable.DragPreviewAreaId = hit.Id;
				_drawable.DragPreviewDelta = new TilePoint(0, 0);
				_isMoveDragging = true;
				MapCanvas.Invalidate();
			}
			return;
		}

		if (_viewModel.ActiveTool != MapTool.Draw || _selectedFloorId is null)
		{
			return;
		}

		var snapped = TileCoordinateHelper.SnapToGrid(canvasPoint.X, canvasPoint.Y, _drawable.TileSize);
		var result = _viewModel.DrawingSession.PlaceVertex(snapped);

		if (result == DrawSessionResult.Added)
		{
			if (_drawState == DrawState.DrawingReady)
			{
				_drawState = DrawState.Drawing;
			}

			_drawable.InProgressVertices = [.. _viewModel.DrawingSession.Vertices];
			UpdateToolbarButtonState();
			MapCanvas.Invalidate();
		}
		else if (result == DrawSessionResult.Closed)
		{
			FinishDrawing();
		}
	}

	private void OnMapMoveHover(object? sender, TouchEventArgs e)
	{
		var point = e.Touches.FirstOrDefault();
		var canvasPoint = _drawable.ScreenToCanvas(new PointF(point.X, point.Y));
		var needsInvalidate = false;

		if (_viewModel.ActiveTool == MapTool.Place && _drawable.GhostObjectKind is not null)
		{
			_drawable.GhostCanvasPosition = canvasPoint;
			MapCanvas.Invalidate();
			return;
		}

		if (_drawState == DrawState.Drawing)
		{
			_drawable.PhantomEnd = canvasPoint;
			needsInvalidate = true;
		}

		// Canvas hover highlighting — for Move, Select, and Erase tools
		if (_selectedFloorId is not null
			&& (_viewModel.ActiveTool == MapTool.Move || _viewModel.ActiveTool == MapTool.Select || _viewModel.ActiveTool == MapTool.Erase))
		{
			var hit = HitTestArea(canvasPoint);
			var newId = hit?.Id;
			if (newId != _drawable.HoveredAreaId)
			{
				_drawable.HoveredAreaId = newId;
				needsInvalidate = true;
			}
		}

		// Hover highlight for placed objects (canvas glow only — popup is click-driven).
		if (_viewModel.ActiveTool == MapTool.Place || _viewModel.ActiveTool == MapTool.Select)
		{
			var cp = _drawable.ScreenToCanvas(new PointF((float)e.Touches.FirstOrDefault().X, (float)e.Touches.FirstOrDefault().Y));
			var newHovered = GetObjectAtCanvasPoint(cp);
			if (newHovered != _hoveredObjectId)
			{
				_hoveredObjectId = newHovered;
				// Only update the drawable's hover if the popup isn't pinned to a selected object.
				if (_selectedObjectId is null)
				{
					_drawable.HoveredObjectId = newHovered;
				}
				needsInvalidate = true;
			}
		}

		if (needsInvalidate)
		{
			MapCanvas.Invalidate();
		}
	}

	private void OnMapEndHover(object? sender, EventArgs e)
	{
		var needsInvalidate = false;

		if (_viewModel.ActiveTool == MapTool.Place)
		{
			_drawable.GhostCanvasPosition = null;
			needsInvalidate = true;
		}

		if (_drawable.PhantomEnd is not null)
		{
			_drawable.PhantomEnd = null;
			needsInvalidate = true;
		}

		if (_drawable.HoveredAreaId is not null)
		{
			_drawable.HoveredAreaId = null;
			needsInvalidate = true;
		}

		// Clear hover highlight when cursor leaves the canvas.
		// The selected-object popup stays until explicitly dismissed.
		if (_hoveredObjectId is not null)
		{
			_hoveredObjectId = null;
			if (_selectedObjectId is null)
			{
				_drawable.HoveredObjectId = null;
			}

			needsInvalidate = true;
		}

		if (needsInvalidate)
		{
			MapCanvas.Invalidate();
		}
	}

	private void OnMapDragInteraction(object? sender, TouchEventArgs e)
	{
		// Object drag-to-move: active whenever a touch started on an object in Place mode.
		if (_pendingObjectClickId is not null
			&& (_viewModel.ActiveTool == MapTool.Place || _viewModel.ActiveTool == MapTool.Select))
		{
			var current = new PointF((float)e.Touches[0].X, (float)e.Touches[0].Y);
			var dist = MathF.Sqrt(
				MathF.Pow(current.X - _objectDragAnchorScreen.X, 2) +
				MathF.Pow(current.Y - _objectDragAnchorScreen.Y, 2));

			if (!_isObjectDragConfirmed && dist > ObjectDragThreshold)
			{
				_isObjectDragConfirmed = true;
				_longPressDetector.TouchEnded(); // cancel the long-press timer
				_selectedObjectId = null;
				ObjectActionPopup.IsVisible = false;
				_drawable.DraggingObjectId = _pendingObjectClickId;
			}

			if (_isObjectDragConfirmed)
			{
				var canvasPoint = _drawable.ScreenToCanvas(current);

				_drawable.DraggingObjectTargetCanvas = canvasPoint;
				_objectDragTargetCanvas = new CanvasPoint(canvasPoint.X, canvasPoint.Y);

				MapCanvas.Invalidate();
			}
			return;
		}

		if (_viewModel.ActiveTool == MapTool.Pan && _panAnchorScreen is PointF anchor)
		{
			var current = new PointF((float)e.Touches[0].X, (float)e.Touches[0].Y);
			var delta = new PointF(current.X - anchor.X, current.Y - anchor.Y);
			_drawable.PanOffset = new PointF(
				_drawable.PanOffset.X + delta.X,
				_drawable.PanOffset.Y + delta.Y);
			_panAnchorScreen = current;
			MapCanvas.Invalidate();
			return;
		}

		if (_viewModel.ActiveTool == MapTool.Move && _isMoveDragging
			&& _moveGrabTile is not null && _draggingAreaId is not null)
		{
			var point = e.Touches.FirstOrDefault();
			var canvasPoint = _drawable.ScreenToCanvas(new PointF(point.X, point.Y));
			var currentTile = TileCoordinateHelper.SnapToGrid(canvasPoint.X, canvasPoint.Y, _drawable.TileSize);
			_drawable.DragPreviewDelta = new TilePoint(
				currentTile.Col - _moveGrabTile.Col,
				currentTile.Row - _moveGrabTile.Row);
			MapCanvas.Invalidate();
			return;
		}

		if (_drawState == DrawState.Drawing)
		{
			var point = e.Touches.FirstOrDefault();
			var canvasPoint = _drawable.ScreenToCanvas(new PointF(point.X, point.Y));
			_drawable.PhantomEnd = canvasPoint;
			MapCanvas.Invalidate();
		}
	}

	private async void OnMapEndInteraction(object? sender, TouchEventArgs e)
	{
		_longPressDetector.TouchEnded();

		// Resolve pending object click vs drag.
		if (_pendingObjectClickId is not null)
		{
			var clickId = _pendingObjectClickId.Value;
			_pendingObjectClickId = null;

			if (_isObjectDragConfirmed)
			{
				// Was a drag — commit the move to the dropped floor-absolute position.
				var targetPos = _objectDragTargetCanvas;
				_isObjectDragConfirmed = false;
				_objectDragTargetCanvas = null;
				_drawable.DraggingObjectId = null;
				_drawable.DraggingObjectTargetCanvas = null;
				if (targetPos is not null)
				{
					_ = MoveObjectAsync(clickId, targetPos);
				}
				return;
			}

			// Was a click — show/toggle popup (unless long press already handled it).
			_isObjectDragConfirmed = false;
			_objectDragTargetCanvas = null;
			if (!_objectLongPressHandled)
			{
				if (_selectedObjectId == clickId && ObjectActionPopup.IsVisible)
				{
					_selectedObjectId = null;
					_hoveredObjectId = null;
					_drawable.HoveredObjectId = null;
					ObjectActionPopup.IsVisible = false;
				}
				else
				{
					_selectedObjectId = clickId;
					_hoveredObjectId = clickId;
					_drawable.HoveredObjectId = clickId;
					UpdateObjectPopup();
				}
				MapCanvas.Invalidate();
			}
			_objectLongPressHandled = false;
			return;
		}

		if (_viewModel.ActiveTool == MapTool.Pan)
		{
			_panAnchorScreen = null;
			SetCanvasCursor(MapTool.Pan, isDragging: false);
			return;
		}

		if (_viewModel.ActiveTool == MapTool.Move && _isMoveDragging
			&& _draggingAreaId is not null && _selectedFloorId is not null)
		{
			var drag = _drawable.DragPreviewDelta ?? new TilePoint(0, 0);
			var areaId = _draggingAreaId.Value;
			var floorId = _selectedFloorId.Value;
			var area = _drawable.Floor?.Areas.FirstOrDefault(areaItem => areaItem.Id == areaId);

			_drawable.DragPreviewAreaId = null;
			_drawable.DragPreviewDelta = null;
			_isMoveDragging = false;
			_draggingAreaId = null;
			_moveGrabTile = null;

			if (area is not null && (drag.Col != 0 || drag.Row != 0))
			{
				// Keep visual preview via pending offset while the DB write completes
				_drawable.PendingMoveOffsets[areaId] = drag;
				MapCanvas.Invalidate();

				var newOutline = area.Outline
					.Select(segment => segment with
					{
						End = new TilePoint(segment.End.Col + drag.Col, segment.End.Row + drag.Row),
						Ctrl1 = segment.Ctrl1 is null ? null : new TilePoint(segment.Ctrl1.Col + drag.Col, segment.Ctrl1.Row + drag.Row),
						Ctrl2 = segment.Ctrl2 is null ? null : new TilePoint(segment.Ctrl2.Col + drag.Col, segment.Ctrl2.Row + drag.Row),
					})
					.ToList();

				await _viewModel.MoveAreaOutlineAsync(
					floorId,
					areaId,
					(IReadOnlyList<DungeonMaster.Domain.ValueObjects.PathSegment>)newOutline);

				_drawable.PendingMoveOffsets.Clear();
				await RefreshFloorAsync();
			}

			UpdateToolbarButtonState();
			MapCanvas.Invalidate();
		}
	}

	private async void FinishDrawing()
	{
		var vertices = _viewModel.DrawingSession.Vertices;
		if (vertices.Count < 3)
		{
			AbortInProgressDraw();
			return;
		}

		var outline = _viewModel.DrawingSession.BuildOutline();

		// Show optimistic visual immediately, then auto-save and reload
		if (_selectedFloorId is not null)
		{
			_drawable.GhostAreas.Add(outline);
			MapCanvas.Invalidate();
			await _viewModel.AddAreaWithOutlineCommand.ExecuteAsync((_selectedFloorId.Value, outline));
		}

		_viewModel.DrawingSession.Reset();
		_drawState = DrawState.DrawingReady;
		_drawable.InProgressVertices = [];
		_drawable.PhantomEnd = null;
		_drawable.GhostAreas.Clear();
		_drawable.PendingOutlines.Clear();

		await RefreshFloorAsync();
		// Restore the focus dim if an area was focused before the draw
		if (_focusedAreaId is not null)
		{
			_drawable.ActiveAreaId = _focusedAreaId;
			UpdateActiveAreaSidebarHighlight();
		}

		UpdateToolbarButtonState();
		MapCanvas.Invalidate();
	}

	private void OnMapDoubleTapped(object? sender, TappedEventArgs e)
	{
		if (_drawState == DrawState.Drawing && _viewModel.DrawingSession.Vertices.Count >= 3)
		{
			_drawable.PhantomEnd = null;
			FinishDrawing();
		}
	}

	private void OnMapSecondaryTapped(object? sender, TappedEventArgs e)
	{
		if (_viewModel.ActiveTool == MapTool.Place)
		{
			if (ObjectPalette.ActiveObject is not null)
			{
				// Deselect the active palette item.
				ObjectPalette.ClearActiveObject();
			}
			else if (_hoveredObjectId is not null)
			{
				// Right-click on a hovered placed object (inspect mode) deletes it.
				_selectedObjectId = _hoveredObjectId;
				_ = DeleteSelectedObjectAsync();
			}
			return;
		}

		OnUndoClicked(sender, e);
	}

	// ─── Hit testing ────────────────────────────────────────────────────────────

	private AreaDto? HitTestArea(PointF pixelPoint)
	{
		if (_drawable.Floor?.Areas is null)
		{
			return null;
		}

		AreaDto? bestMatch = null;
		var bestArea = float.MaxValue;

		foreach (var area in _drawable.Floor.Areas)
		{
			if (area.Outline.Count == 0)
			{
				continue;
			}

			var outline = area.Outline;

			// The polygon is closed, so the final segment's end point is also its start point.
			var start = outline[^1].End;

			// Account for an active drag preview so hit-testing follows the dragged position
			var dragDelta = area.Id == _drawable.DragPreviewAreaId ? _drawable.DragPreviewDelta : null;
			var visualOrigin = dragDelta is null
				? start
				: new TilePoint(start.Col + dragDelta.Col, start.Row + dragDelta.Row);

			// Build pixel-space polygon vertices from the area outline
			var verts = new List<PointF>(outline.Count + 1);
			var (ox, oy) = TileCoordinateHelper.ToPixel(visualOrigin, _drawable.TileSize);
			verts.Add(new PointF(ox, oy));

			foreach (var seg in outline)
			{
				var visualEnd = dragDelta is null
					? seg.End
					: new TilePoint(seg.End.Col + dragDelta.Col, seg.End.Row + dragDelta.Row);
				var (ex, ey) = TileCoordinateHelper.ToPixel(visualEnd, _drawable.TileSize);
				verts.Add(new PointF(ex, ey));
			}

			// The last segment closes back to origin — remove the duplicate closing vertex
			if (verts.Count > 1)
			{
				verts.RemoveAt(verts.Count - 1);
			}

			if (!IsPointInPolygon(pixelPoint, verts))
			{
				continue;
			}

			// When multiple areas overlap at the cursor, prefer the smallest (most specific) target
			var polyArea = PolygonArea(verts);
			if (polyArea < bestArea)
			{
				bestArea = polyArea;
				bestMatch = area;
			}
		}

		return bestMatch;
	}

	private static bool IsPointInPolygon(PointF point, List<PointF> polygon)
	{
		var n = polygon.Count;
		var inside = false;
		for (int i = 0, j = n - 1; i < n; j = i++)
		{
			var xi = polygon[i].X; var yi = polygon[i].Y;
			var xj = polygon[j].X; var yj = polygon[j].Y;
			if (((yi > point.Y) != (yj > point.Y)) &&
				point.X < (xj - xi) * (point.Y - yi) / (yj - yi) + xi)
				inside = !inside;
		}
		return inside;
	}

	private static float PolygonArea(List<PointF> polygon)
	{
		var n = polygon.Count;
		var sum = 0f;
		for (int i = 0, j = n - 1; i < n; j = i++)
			sum += (polygon[j].X + polygon[i].X) * (polygon[j].Y - polygon[i].Y);
		return MathF.Abs(sum * 0.5f);
	}

	// ─── Object popup helpers ───────────────────────────────────────────────────

	private Guid? GetObjectAtCanvasPoint(PointF canvasPoint)
	{
		if (_drawable.Floor is null)
			return null;

		// Pick the smallest object whose bounding box contains the hit point (topmost = smallest area).
		PlacedObjectDto? best = null;
		var bestArea = float.MaxValue;
		foreach (var obj in _drawable.Floor.Objects)
		{
			var (wTiles, hTiles) = ObjectKindInfo.GetObjectSize(obj.Kind);
			var halfW = wTiles * _drawable.TileSize / 2f;
			var halfH = hTiles * _drawable.TileSize / 2f;

			if (canvasPoint.X >= obj.Position.X - halfW && canvasPoint.X <= obj.Position.X + halfW &&
			    canvasPoint.Y >= obj.Position.Y - halfH && canvasPoint.Y <= obj.Position.Y + halfH)
			{
				var area = wTiles * hTiles;
				if (area < bestArea)
				{
					bestArea = area;
					best = obj;
				}
			}
		}
		return best?.Id;
	}

	private void OnObjectLongPressed(object? sender, EventArgs e)
	{
		var canvasPoint = _drawable.ScreenToCanvas(_lastTouchPoint);
		var objectId = GetObjectAtCanvasPoint(canvasPoint);
		if (objectId is not null)
		{
			_objectLongPressHandled = true; // prevent EndInteraction from firing click logic again
			_pendingObjectClickId = null;
			_selectedObjectId = objectId;
			_hoveredObjectId = objectId;
			_drawable.HoveredObjectId = objectId;
			UpdateObjectPopup();
			MapCanvas.Invalidate();
		}
	}

	private void UpdateObjectPopup()
	{
		if (_selectedObjectId is null || _drawable.Floor is null)
		{
			ObjectActionPopup.IsVisible = false;
			return;
		}

		var obj = _drawable.Floor.Objects.FirstOrDefault(placedObj => placedObj.Id == _selectedObjectId);

		if (obj is null)
		{
			ObjectActionPopup.IsVisible = false;
			return;
		}

		// Position above the object center in screen space (5 x 32px buttons + 2 dividers + padding ~= 182x40)
		var screenX = obj.Position.X * _drawable.ZoomLevel + _drawable.PanOffset.X;
		var screenY = obj.Position.Y * _drawable.ZoomLevel + _drawable.PanOffset.Y - 48;

		const float PopupWidth  = 182f;
		const float PopupHeight = 40f;
		_popupBounds = new RectF(screenX, screenY, PopupWidth, PopupHeight);

		ObjectActionPopup.Margin = new Thickness(screenX, screenY, 0, 0);
		ObjectActionPopup.HorizontalOptions = LayoutOptions.Start;
		ObjectActionPopup.VerticalOptions = LayoutOptions.Start;
		ObjectActionPopup.IsVisible = true;
	}

	private static readonly Color PopupBtnHoverGold    = Color.FromArgb("#33C8A040");
	private static readonly Color PopupBtnHoverRed     = Color.FromArgb("#33CC3030");
	private static readonly Color PopupBtnPressGold    = Color.FromArgb("#66C8A040");
	private static readonly Color PopupBtnPressRed     = Color.FromArgb("#66CC3030");

	private static void AttachPopupButtonBehavior(Border btn, bool goldHover, Func<Task> action)
	{
		var hoverColor = goldHover ? PopupBtnHoverGold : PopupBtnHoverRed;
		var pressColor = goldHover ? PopupBtnPressGold : PopupBtnPressRed;
		var isHovered  = false;

		var ptr = new PointerGestureRecognizer();
		ptr.PointerEntered += (_, _) =>
		{
			isHovered = true;
			btn.BackgroundColor = hoverColor;
		};
		ptr.PointerExited += (_, _) =>
		{
			isHovered = false;
			btn.BackgroundColor = Colors.Transparent;
		};
		btn.GestureRecognizers.Add(ptr);

		var tap = new TapGestureRecognizer();
		tap.Tapped += async (_, _) =>
		{
			btn.BackgroundColor = pressColor;
			await action();
			btn.BackgroundColor = isHovered ? hoverColor : Colors.Transparent;
		};
		btn.GestureRecognizers.Add(tap);

		btn.Behaviors.Add(new HandCursorBehavior());
	}

	private async Task RotateObjectAsync(bool clockwise)
	{
		if (_selectedObjectId is null || _selectedFloorId is null || _drawable.Floor is null)
		{
			return;
		}

		var obj = _drawable.Floor.Objects.FirstOrDefault(placedObj => placedObj.Id == _selectedObjectId);

		if (obj is null)
		{
			return;
		}

		var newRotation = clockwise
			? (ObjectRotation)(((int)obj.Rotation + 1) % 4)
			: (ObjectRotation)(((int)obj.Rotation + 3) % 4);

		await _viewModel.UpdatePlacedObjectAsync(
			_viewModel.Dungeon!.Id,
			_selectedFloorId.Value,
			_selectedObjectId.Value,
			obj.Position,
			newRotation,
			obj.Scale);

		await RefreshFloorAsync();
		// Re-pin popup to the refreshed object position.
		if (_selectedObjectId is not null)
		{
			UpdateObjectPopup();
		}

		MapCanvas.Invalidate();
	}

	private async Task ScaleObjectAsync(bool down)
	{
		if (_selectedObjectId is null || _selectedFloorId is null || _drawable.Floor is null)
		{
			return;
		}

		var obj = _drawable.Floor.Objects.FirstOrDefault(placedObj =>
			placedObj.Id == _selectedObjectId.Value);

		if (obj is null)
		{
			return;
		}

		var newScale = down
			? ObjectScale.StepDown(obj.Scale)
			: ObjectScale.StepUp(obj.Scale);

		await _viewModel.UpdatePlacedObjectAsync(
			_viewModel.Dungeon!.Id,
			_selectedFloorId.Value,
			_selectedObjectId.Value,
			obj.Position,
			obj.Rotation,
			newScale);

		await RefreshFloorAsync();
		if (_selectedObjectId is not null)
		{
			UpdateObjectPopup();
		}

		MapCanvas.Invalidate();
	}

	private async Task MoveObjectAsync(Guid objectId, CanvasPoint newPosition)
	{
		if (_selectedFloorId is null || _drawable.Floor is null)
		{
			return;
		}

		var obj = _drawable.Floor.Objects.FirstOrDefault(placedObj => placedObj.Id == objectId);

		if (obj is null || obj.Position == newPosition)
		{
			return;
		}

		_hoveredObjectId = null;
		_drawable.HoveredObjectId = null;
		MapCanvas.Invalidate();

		await _viewModel.UpdatePlacedObjectAsync(
			_viewModel.Dungeon!.Id,
			_selectedFloorId.Value,
			objectId,
			newPosition,
			obj.Rotation,
			obj.Scale);

		await RefreshFloorAsync();
		MapCanvas.Invalidate();
	}

	private async Task DeleteSelectedObjectAsync()
	{
		if (_selectedObjectId is null || _selectedFloorId is null || _drawable.Floor is null)
		{
			return;
		}

		if (!_drawable.Floor.Objects.Any(placedObj => placedObj.Id == _selectedObjectId))
		{
			return;
		}

		var deleteId = _selectedObjectId.Value;
		_selectedObjectId = null;
		_hoveredObjectId = null;
		_drawable.HoveredObjectId = null;
		ObjectActionPopup.IsVisible = false;
		MapCanvas.Invalidate();

		await _viewModel.RemovePlacedObjectAsync(
			_viewModel.Dungeon!.Id,
			_selectedFloorId.Value,
			deleteId);

		await RefreshFloorAsync();
		MapCanvas.Invalidate();
	}

	// ─── Map refresh ────────────────────────────────────────────────────────────

	private void RefreshMap()
	{
		var floor = _viewModel.Floors.FirstOrDefault(floorItem => floorItem.Id == _selectedFloorId);
		_drawable.Floor = floor;

		FloorNameToolbarEditor.Text = floor?.Name ?? string.Empty;

		// The focused area belongs to a floor we just left — drop the focus so nothing stays dimmed.
		if (_focusedAreaId is not null
			&& floor?.Areas.Any(areaItem => areaItem.Id == _focusedAreaId) != true)
		{
			ClearAreaFocus();
		}

		if (_drawState == DrawState.Idle)
		{
			_drawable.InProgressVertices = [];
			_drawable.PhantomEnd = null;
		}

		MapToolbar.IsVisible = _selectedFloorId is not null;

		if (_selectedFloorId is not null && _drawState == DrawState.Idle)
			SetActiveTool(MapTool.Pan);

		UpdateToolbarButtonState();
		MapCanvas.Invalidate();
	}

	// ─── Sidebar helpers ────────────────────────────────────────────────────────

	private void UpdateActiveAreaSidebarHighlight()
	{
		foreach (var child in FloorListContainer.Children)
		{
			if (child is not FloorSidebarItem floorItem || floorItem.Floor is not { } floor || floor.Id != _selectedFloorId)
				continue;

			foreach (var area in floor.Areas)
			{
				floorItem.SetAreaEditing(area.Id, area.Id == _focusedAreaId);
			}

			break;
		}
	}

	private void OnSidebarTapped(object? sender, TappedEventArgs e) =>
		CommitAllFloorEditors();

	private void CommitAllFloorEditors()
	{
		foreach (var child in FloorListContainer.Children)
		{
			if (child is FloorSidebarItem item)
			{
				item.TryCommitActiveEditor();
			}
		}
	}

	private void CollapseAllExcept(FloorSidebarItem except)
	{
		foreach (var child in FloorListContainer.Children)
		{
			if (child is FloorSidebarItem other && !ReferenceEquals(other, except))
			{
				other.Deselect();
			}
		}
	}

	// ─── Drag-and-drop placeholder ──────────────────────────────────────────────

	private void ShowDragPlaceholder(FloorSidebarItem target)
	{
		if (ReferenceEquals(target, _draggingItem))
		{
			return;
		}

		int sourceIdx = -1, targetIdx = -1, n = 0;

		foreach (var child in FloorListContainer.Children)
		{
			if (ReferenceEquals(child, _dragPlaceholder))
				continue;

			if (ReferenceEquals(child, _draggingItem))
				sourceIdx = n;

			if (ReferenceEquals(child, target))
				targetIdx = n;

			n++;
		}

		bool afterTarget = sourceIdx >= 0 && targetIdx >= 0 && sourceIdx < targetIdx;

		if (ReferenceEquals(_currentPlaceholderTarget, target) && _placeholderAfterTarget == afterTarget)
		{
			return;
		}

		_currentPlaceholderTarget = target;
		_placeholderAfterTarget = afterTarget;

		if (_dragPlaceholder is not null)
		{
			FloorListContainer.Children.Remove(_dragPlaceholder);
			_dragPlaceholder = null;
		}

		var insertIdx = FloorListContainer.Children.IndexOf(target);

		if (insertIdx < 0)
		{
			return;
		}

		if (afterTarget)
		{
			insertIdx++;
		}

		var capturedTarget = target;
		var dropGesture = new DropGestureRecognizer();

		dropGesture.DragOver += (_, dragArgs) =>
			dragArgs.AcceptedOperation = DataPackageOperation.Copy;

		dropGesture.Drop += async (_, dropArgs) =>
		{
			ClearDragPlaceholder();

			if (capturedTarget.Floor is null)
			{
				return;
			}

			if (!dropArgs.Data.Properties.TryGetValue("FloorId", out var obj))
			{
				return;
			}

			if (obj is not string idStr || !Guid.TryParse(idStr, out var draggedId))
			{
				return;
			}

			if (draggedId == capturedTarget.Floor.Id)
			{
				return;
			}

			_selectedFloorId = draggedId;
			await _viewModel.ReorderFloorCommand.ExecuteAsync((draggedId, capturedTarget.Floor.Number));
		};

		_dragPlaceholder = new Border
		{
			HeightRequest = 44,
			Stroke = new SolidColorBrush(DmColors.DragPlaceholderStroke),
			StrokeThickness = 2,
			StrokeShape = new RoundRectangle { CornerRadius = 3 },
			BackgroundColor = DmColors.DragPlaceholderFill,
			Margin = new Thickness(0, 0, 0, 4)
		};
		_dragPlaceholder.GestureRecognizers.Add(dropGesture);
		FloorListContainer.Children.Insert(insertIdx, _dragPlaceholder);
	}

	private void ClearDragPlaceholder()
	{
		_currentPlaceholderTarget = null;

		if (_dragPlaceholder is null)
		{
			return;
		}

		FloorListContainer.Children.Remove(_dragPlaceholder);
		_dragPlaceholder = null;
	}

	// ─── Nav and dungeon-level editors ──────────────────────────────────────────

	private void OnBackClicked(object? sender, EventArgs e) =>
		Shell.Current.GoToAsync("..");

	private async void OnAddFloorClicked(object? sender, EventArgs e)
	{
		await _viewModel.AddFloorCommand.ExecuteAsync(null);

		Dispatcher.DispatchDelayed(
			TimeSpan.FromMilliseconds(80),
			() =>
			{
				if (FloorListContainer.Children.LastOrDefault() is FloorSidebarItem lastItem)
				{
					lastItem.FocusNameEditor();
				}
				FloorNameToolbarEditor.ActivateEditor();
			});
	}

	private async void OnDungeonNameCommitted(object? sender, string? name)
	{
		if (!string.IsNullOrWhiteSpace(name) && name != _viewModel.DungeonName)
		{
			_viewModel.DungeonName = name;
			await _viewModel.SaveDungeonCommand.ExecuteAsync(null);
		}
	}

	private async void OnDescriptionCommitted(object? sender, string? value)
	{
		if (value != _viewModel.DungeonDescription)
		{
			_viewModel.DungeonDescription = value;
			await _viewModel.SaveDungeonCommand.ExecuteAsync(null);
		}
	}

	private async void OnNotesCommitted(object? sender, string? value)
	{
		if (value != _viewModel.DungeonNotes)
		{
			_viewModel.DungeonNotes = value;
			await _viewModel.SaveDungeonCommand.ExecuteAsync(null);
		}
	}
}
