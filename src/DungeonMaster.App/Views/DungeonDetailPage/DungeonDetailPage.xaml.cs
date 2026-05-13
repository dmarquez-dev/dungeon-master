using DungeonMaster.App.Controls;
using DungeonMaster.App.Extensions;
using DungeonMaster.App.Graphics;
using DungeonMaster.App.Theme;
using DungeonMaster.App.ViewModels;
using DungeonMaster.Application.Common.Map;
using DungeonMaster.Application.Features.Dungeons.Queries.GetDungeonDetail;
using DungeonMaster.Domain.ValueObjects;
using Microsoft.Maui.Controls.Shapes;

namespace DungeonMaster.App.Views;

/// <summary>
///		Displays the detail view for a single dungeon, including floors, areas, and the map canvas.
///		Supports inline editing, floor reordering via drag-and-drop, area management,
///		and a floor-level editing state machine for drawing and moving area outlines.
/// </summary>
public partial class DungeonDetailPage : ContentPage
{
	private readonly DungeonDetailViewModel _viewModel;
	private readonly FloorMapDrawable _drawable = new();

	// Floor list state
	private Guid? _selectedFloorId;
	private View? _dragPlaceholder;
	private FloorSidebarItem? _draggingItem;
	private FloorSidebarItem? _currentPlaceholderTarget;
	private bool _placeholderAfterTarget;

	// Active draw target — null means drawing a new area
	private Guid? _activeAreaId;
	private bool _isDrawingNewArea;

	// Draw state machine
	private enum DrawState { Idle, DrawingReady, Drawing }
	private DrawState _drawState = DrawState.Idle;

	// Toolbar context state machine
	private enum ToolbarContext { Floor, Area }
	private ToolbarContext _currentContext = ToolbarContext.Floor;
	private bool _loreOpen;

	// Pending floor-level changes (committed to DB only on Save)
	private readonly Dictionary<Guid, TilePoint> _pendingMoveOffsets = new();
	private readonly Dictionary<Guid, (TilePoint Origin, IReadOnlyList<DungeonMaster.Domain.ValueObjects.PathSegment> Outline)> _pendingOutlines = new();
	private readonly List<(TilePoint Origin, IReadOnlyList<DungeonMaster.Domain.ValueObjects.PathSegment> Outline)> _pendingNewAreas = [];

	// Current drag state (transient — committed into _pendingMoveOffsets on mouse-release)
	private bool _isMoveDragging;
	private Guid? _draggingAreaId;
	private TilePoint? _moveGrabTile;

	// Pan tool anchor (screen space)
	private PointF? _panAnchorScreen;

	// Keyboard guard — suppresses tool shortcuts while a text field is focused.
	// Value is read only inside #if WINDOWS; the pragma suppresses the CS0414 on other targets.
#pragma warning disable CS0414
	private bool _isTextInputFocused;
#pragma warning restore CS0414

#if WINDOWS
	[System.Runtime.InteropServices.DllImport("user32.dll")]
	private static extern short GetAsyncKeyState(int vKey);
#endif

	private const float ZoomStep = 0.1f;
	private const float ZoomSnapThreshold = 0.05f;

	private bool HasPendingChanges =>
		_pendingMoveOffsets.Count > 0
		|| _pendingOutlines.Count > 0
		|| _pendingNewAreas.Count > 0;

	/// <summary>
	///		Initializes a new instance of <see cref="DungeonDetailPage"/>, wires the map drawable,
	///		and subscribes to view model property and floor collection changes.
	/// </summary>
	public DungeonDetailPage(DungeonDetailViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = viewModel;

		MapCanvas.Drawable = _drawable;

		viewModel.PropertyChanged += OnViewModelPropertyChanged;
		viewModel.Floors.CollectionChanged += OnFloorsChanged;

		// Suppress keyboard shortcuts while any inline editor is focused
		DescriptionEditor.Focused += (_, _) => _isTextInputFocused = true;
		DescriptionEditor.Unfocused += (_, _) => _isTextInputFocused = false;
		NotesEditor.Focused += (_, _) => _isTextInputFocused = true;
		NotesEditor.Unfocused += (_, _) => _isTextInputFocused = false;
		FloorNameToolbarEditor.Focused += (_, _) => _isTextInputFocused = true;
		FloorNameToolbarEditor.Unfocused += (_, _) => _isTextInputFocused = false;
		AreaNameToolbarEditor.Focused += (_, _) => _isTextInputFocused = true;
		AreaNameToolbarEditor.Unfocused += (_, _) => _isTextInputFocused = false;
	}

	/// <inheritdoc/>
	protected override void OnAppearing()
	{
		base.OnAppearing();
		MapCanvas.HandlerChanged += OnMapCanvasHandlerChanged;
		AttachKeyDown();
	}

	/// <inheritdoc/>
	protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
	{
		base.OnNavigatedFrom(args);
		DetachKeyDown();
		MapCanvas.HandlerChanged -= OnMapCanvasHandlerChanged;
		DetachScrollZoom();
		_viewModel.PropertyChanged -= OnViewModelPropertyChanged;
		_viewModel.Floors.CollectionChanged -= OnFloorsChanged;
	}

	// ─── Keyboard shortcuts ─────────────────────────────────────────────────────

	private void AttachKeyDown()
	{
#if WINDOWS
		if (Window?.Handler?.PlatformView is Microsoft.UI.Xaml.Window winUiWindow)
		{
			winUiWindow.Content.KeyDown += OnWindowKeyDown;
		}
#endif
	}

	private void DetachKeyDown()
	{
#if WINDOWS
		if (Window?.Handler?.PlatformView is Microsoft.UI.Xaml.Window winUiWindow)
		{
			winUiWindow.Content.KeyDown -= OnWindowKeyDown;
		}
#endif
	}

#if WINDOWS
	private void OnWindowKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
	{
		if (_isTextInputFocused)
		{
			return;
		}

		switch (e.Key)
		{
			case Windows.System.VirtualKey.D:
				SetActiveTool(MapTool.Draw);
				break;
			case Windows.System.VirtualKey.M:
				SetActiveTool(MapTool.Move);
				break;
			case Windows.System.VirtualKey.H:
				SetActiveTool(MapTool.Pan);
				break;
			case Windows.System.VirtualKey.P:
				SetActiveTool(MapTool.Place);
				break;
		}

		if (e.Key == Windows.System.VirtualKey.Number0)
		{
			var ctrlState = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(
				Windows.System.VirtualKey.Control);
			if (ctrlState.HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down))
			{
				ResetViewport();
			}
		}
	}
#endif

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
		MapCanvas.Invalidate();
	}

	private void ResetViewport()
	{
		_drawable.ZoomLevel = 1.0f;
		_drawable.PanOffset = PointF.Zero;
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
#endif
	}

	private void DetachScrollZoom()
	{
#if WINDOWS
		if (MapCanvas.Handler?.PlatformView is Microsoft.UI.Xaml.FrameworkElement el)
			el.PointerWheelChanged -= OnMapScrollWheel;
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

				if (confirmed)
				{
					await _viewModel.RemoveFloorCommand.ExecuteAsync(floorToDelete.Id);
				}
			};

			item.SelectRequested += (_, _) =>
			{
				if (floor.Id != _selectedFloorId && HasPendingChanges)
				{
					DiscardPendingChanges();
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

			item.AreaDeleteRequested += async (_, area) =>
				await _viewModel.RemoveAreaCommand.ExecuteAsync((floor.Id, area.Id));

			item.AreaSelectRequested += (_, area) =>
				SelectAreaForDrawing(floor.Id, area);

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

			FloorListContainer.Children.Add(item);

			if (floor.Id == _selectedFloorId)
			{
				item.Select();
			}
		}
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

	// ─── Area draw targeting ────────────────────────────────────────────────────

	private void SelectAreaForDrawing(Guid floorId, AreaDto area)
	{
		if (floorId != _selectedFloorId)
		{
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

		_activeAreaId = area.Id;
		_isDrawingNewArea = false;
		_drawable.ActiveAreaId = area.Id;

		AreaNameToolbarEditor.Text = area.Name ?? string.Empty;
		SetToolbarContext(ToolbarContext.Area);

		UpdateActiveAreaSidebarHighlight();
		UpdateToolbarButtonState();
		MapCanvas.Invalidate();
	}

	private void AbortInProgressDraw()
	{
		_viewModel.DrawingSession.Reset();
		_drawState = DrawState.DrawingReady;
		_isDrawingNewArea = false;
		_drawable.InProgressVertices = [];
		_drawable.PhantomEnd = null;
		_drawable.ActiveAreaId = _activeAreaId;
	}

	private void SetToolbarContext(ToolbarContext context)
	{
		_currentContext = context;
		FloorContextToolbar.IsVisible = context == ToolbarContext.Floor;
		AreaContextToolbar.IsVisible = context == ToolbarContext.Area;
	}

	// ─── Toolbar handlers ────────────────────────────────────────────────────────

	private async void OnSaveClicked(object? sender, EventArgs e) =>
		await SavePendingChangesAsync();

	private void OnDiscardClicked(object? sender, EventArgs e) =>
		DiscardPendingChanges();

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
				if (_drawState == DrawState.Drawing)
				{
					AbortInProgressDraw();
				}
				_drawState = DrawState.Idle;
				break;
		}

		UpdateToolbarButtonState();
	}

	private void OnToolDrawClicked(object? sender, TappedEventArgs e) =>
		SetActiveTool(MapTool.Draw);

	private void OnToolMoveClicked(object? sender, TappedEventArgs e) =>
		SetActiveTool(MapTool.Move);

	private void OnToolPanClicked(object? sender, TappedEventArgs e) =>
		SetActiveTool(MapTool.Pan);

	private void OnToolPlaceClicked(object? sender, TappedEventArgs e) =>
		SetActiveTool(MapTool.Place);

	private void OnAreaContextBackClicked(object? sender, EventArgs e)
	{
		if (_drawState == DrawState.Drawing)
		{
			AbortInProgressDraw();
		}

		_activeAreaId = null;
		_drawable.ActiveAreaId = null;
		_viewModel.ActiveTool = MapTool.Draw;
		_drawState = DrawState.DrawingReady;

		SetToolbarContext(ToolbarContext.Floor);
		UpdateActiveAreaSidebarHighlight();
		UpdateToolbarButtonState();
		MapCanvas.Invalidate();
	}

	private void OnToolRedrawClicked(object? sender, EventArgs e)
	{
		if (_activeAreaId is null)
		{
			return;
		}

		_viewModel.ActiveTool = MapTool.Draw;
		_drawState = DrawState.DrawingReady;
		_drawable.ActiveAreaId = _activeAreaId;

		UpdateToolbarButtonState();
		MapCanvas.Invalidate();
	}

	private async void OnFloorNameToolbarCommitted(object? sender, string? name)
	{
		if (_selectedFloorId is null || string.IsNullOrWhiteSpace(name))
		{
			return;
		}

		var floor = _viewModel.Floors.FirstOrDefault(f => f.Id == _selectedFloorId);
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
		if (_selectedFloorId is null || _activeAreaId is null || string.IsNullOrWhiteSpace(name))
		{
			return;
		}

		var floor = _viewModel.Floors.FirstOrDefault(f => f.Id == _selectedFloorId);
		var area = floor?.Areas.FirstOrDefault(a => a.Id == _activeAreaId);
		if (area is null || name == area.Name)
		{
			return;
		}

		await _viewModel.UpdateAreaNameCommand.ExecuteAsync((_selectedFloorId.Value, _activeAreaId.Value, name));

		foreach (var child in FloorListContainer.Children)
		{
			if (child is FloorSidebarItem floorItem && floorItem.Floor?.Id == _selectedFloorId)
			{
				floorItem.UpdateAreaName(_activeAreaId.Value, name);
				break;
			}
		}
	}

	private void UpdateToolbarButtonState()
	{
		var isDrawActive = _viewModel.ActiveTool == MapTool.Draw;
		var isMoveActive = _viewModel.ActiveTool == MapTool.Move;
		var isPanActive = _viewModel.ActiveTool == MapTool.Pan;
		var isPlaceActive = _viewModel.ActiveTool == MapTool.Place;

		var activeColor = DmColors.Gold;
		var mutedColor = DmColors.GoldMuted;
		var activeBrush = new SolidColorBrush(activeColor);
		var transparentBrush = new SolidColorBrush(Colors.Transparent);

		if (_currentContext == ToolbarContext.Floor)
		{
			ToolDrawBorder.Stroke = isDrawActive ? activeBrush : transparentBrush;
			ToolDrawIcon.TextColor = isDrawActive ? activeColor : mutedColor;
			ToolDrawLabel.TextColor = isDrawActive ? activeColor : mutedColor;

			ToolMoveBorder.Stroke = isMoveActive ? activeBrush : transparentBrush;
			ToolMoveIcon.TextColor = isMoveActive ? activeColor : mutedColor;
			ToolMoveLabel.TextColor = isMoveActive ? activeColor : mutedColor;

			ToolPanBorder.Stroke = isPanActive ? activeBrush : transparentBrush;
			ToolPanLabel.TextColor = isPanActive ? activeColor : mutedColor;

			ToolPlaceBorder.Stroke = isPlaceActive ? activeBrush : transparentBrush;
			ToolPlaceLabel.TextColor = isPlaceActive ? activeColor : mutedColor;
		}
		else
		{
			// In area context, Redraw highlights when draw is active
			ToolRedrawBorder.Stroke = isDrawActive ? activeBrush : transparentBrush;
			ToolRedrawIcon.TextColor = isDrawActive ? activeColor : mutedColor;
			ToolRedrawLabel.TextColor = isDrawActive ? activeColor : mutedColor;
		}

		var hasVertices = _viewModel.DrawingSession.Vertices.Count > 0;
		UndoButton.IsEnabled = hasVertices;
		UndoButton.Opacity = hasVertices ? 1.0 : 0.35;

		var greenBrush = new SolidColorBrush(DmColors.SaveGreen);
		var greenText = DmColors.SaveGreenText;
		SaveButton.Stroke = HasPendingChanges ? greenBrush : transparentBrush;
		SaveIcon.TextColor = HasPendingChanges ? greenText : mutedColor;
		SaveLabel.TextColor = HasPendingChanges ? greenText : mutedColor;
		SaveButton.Opacity = HasPendingChanges ? 1.0 : 0.4;
		SaveButton.InputTransparent = !HasPendingChanges;

		var redBrush = new SolidColorBrush(DmColors.DiscardRed);
		var redText = DmColors.DiscardRedText;
		DiscardButton.Stroke = HasPendingChanges ? redBrush : transparentBrush;
		DiscardIcon.TextColor = HasPendingChanges ? redText : mutedColor;
		DiscardLabel.TextColor = HasPendingChanges ? redText : mutedColor;
		DiscardButton.Opacity = HasPendingChanges ? 1.0 : 0.4;
		DiscardButton.InputTransparent = !HasPendingChanges;
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

	// ─── Save / Discard ──────────────────────────────────────────────────────────

	private async Task SavePendingChangesAsync()
	{
		if (_selectedFloorId is null || !HasPendingChanges)
		{
			return;
		}

		var floorId = _selectedFloorId.Value;

		// Collect all area IDs that need an outline or move update
		var areaIdsToSave = new HashSet<Guid>();
		foreach (var id in _pendingOutlines.Keys) areaIdsToSave.Add(id);
		foreach (var id in _pendingMoveOffsets.Keys) areaIdsToSave.Add(id);

		foreach (var areaId in areaIdsToSave)
		{
			var area = _drawable.Floor?.Areas.FirstOrDefault(a => a.Id == areaId);
			if (area is null)
			{
				continue;
			}

			// Use pending outline if one was drawn; otherwise keep the stored outline
			var (baseOrigin, baseOutline) = _pendingOutlines.TryGetValue(areaId, out var pendingOutline)
				? pendingOutline
				: (area.Origin, area.Outline);

			// Apply any pending move offset on top
			if (_pendingMoveOffsets.TryGetValue(areaId, out var moveOffset)
				&& (moveOffset.Col != 0 || moveOffset.Row != 0))
			{
				var newOrigin = new TilePoint(baseOrigin.Col + moveOffset.Col, baseOrigin.Row + moveOffset.Row);
				var newOutline = baseOutline
					.Select(s => s with
					{
						End = new TilePoint(s.End.Col + moveOffset.Col, s.End.Row + moveOffset.Row),
						Ctrl1 = s.Ctrl1 is null ? null : new TilePoint(s.Ctrl1.Col + moveOffset.Col, s.Ctrl1.Row + moveOffset.Row),
						Ctrl2 = s.Ctrl2 is null ? null : new TilePoint(s.Ctrl2.Col + moveOffset.Col, s.Ctrl2.Row + moveOffset.Row)
					})
					.ToList();

				await _viewModel.MoveAreaCommand.ExecuteAsync(
					(floorId, areaId, newOrigin, (IReadOnlyList<DungeonMaster.Domain.ValueObjects.PathSegment>)newOutline));
			}
			else
			{
				await _viewModel.MoveAreaCommand.ExecuteAsync(
					(floorId, areaId, baseOrigin, baseOutline));
			}
		}

		// Add and outline each new area from draw-to-spawn
		foreach (var (origin, outline) in _pendingNewAreas)
		{
			await _viewModel.AddAreaWithOutlineCommand.ExecuteAsync((floorId, origin, outline));
		}

		await _viewModel.LoadDungeonCommand.ExecuteAsync(_viewModel.Dungeon!.Id);

		DiscardPendingChanges();
		_drawable.Floor = _viewModel.Floors.FirstOrDefault(f => f.Id == floorId);
		MapCanvas.Invalidate();
	}

	private void DiscardPendingChanges()
	{
		// Abort any in-progress draw
		_viewModel.DrawingSession.Reset();
		_isDrawingNewArea = false;
		_activeAreaId = null;
		_drawState = _selectedFloorId is not null ? DrawState.DrawingReady : DrawState.Idle;

		// Clear all pending data
		_pendingMoveOffsets.Clear();
		_pendingOutlines.Clear();
		_pendingNewAreas.Clear();

		// Reset drag state
		_isMoveDragging = false;
		_draggingAreaId = null;
		_moveGrabTile = null;

		// Sync drawable
		_drawable.ActiveAreaId = null;
		_drawable.InProgressVertices = [];
		_drawable.PhantomEnd = null;
		_drawable.PendingMoveOffsets.Clear();
		_drawable.PendingOutlines.Clear();
		_drawable.GhostAreas.Clear();
		_drawable.DragPreviewAreaId = null;
		_drawable.DragPreviewDelta = null;

		// Return to floor context and reset default tool
		_viewModel.ActiveTool = MapTool.Draw;
		SetToolbarContext(ToolbarContext.Floor);

		UpdateActiveAreaSidebarHighlight();
		UpdateToolbarButtonState();
		MapCanvas.Invalidate();
	}

	// ─── Map canvas gesture handlers ────────────────────────────────────────────

	private void OnMapStartInteraction(object? sender, TouchEventArgs e)
	{
#if WINDOWS
		const int VK_RBUTTON = 0x02;
		if ((GetAsyncKeyState(VK_RBUTTON) & 0x8000) != 0)
		{
			return;
		}
#endif

		if (_viewModel.ActiveTool == MapTool.Pan)
		{
			_panAnchorScreen = new PointF((float)e.Touches[0].X, (float)e.Touches[0].Y);
			return;
		}

		var point = e.Touches.FirstOrDefault();
		var canvasPoint = _drawable.ScreenToCanvas(new PointF(point.X, point.Y));

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
				_isDrawingNewArea = _activeAreaId is null;
				_drawable.ActiveAreaId = _activeAreaId; // null = no dimming for new areas
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

		if (_drawState == DrawState.Drawing)
		{
			_drawable.PhantomEnd = canvasPoint;
			needsInvalidate = true;
		}

		// Canvas hover highlighting — only for Move tool in floor context
		if (_currentContext == ToolbarContext.Floor
			&& _selectedFloorId is not null
			&& _viewModel.ActiveTool == MapTool.Move)
		{
			var hit = HitTestArea(canvasPoint);
			var newId = hit?.Id;
			if (newId != _drawable.HoveredAreaId)
			{
				_drawable.HoveredAreaId = newId;
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

		if (_drawable.PhantomEnd is not null)
		{
			_drawable.PhantomEnd = null;
			needsInvalidate = true;
		}

		if (_drawable.HoveredAreaId is not null && _currentContext == ToolbarContext.Floor)
		{
			_drawable.HoveredAreaId = null;
			needsInvalidate = true;
		}

		if (needsInvalidate)
		{
			MapCanvas.Invalidate();
		}
	}

	private void OnMapDragInteraction(object? sender, TouchEventArgs e)
	{
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
			var dragDelta = new TilePoint(
				currentTile.Col - _moveGrabTile.Col,
				currentTile.Row - _moveGrabTile.Row);
			var pending = _pendingMoveOffsets.GetValueOrDefault(_draggingAreaId.Value) ?? new TilePoint(0, 0);
			_drawable.DragPreviewDelta = new TilePoint(
				pending.Col + dragDelta.Col - pending.Col,
				pending.Row + dragDelta.Row - pending.Row);

			// The drawable computes effectiveOffset = pending + dragDelta.
			// Store only the delta relative to the committed pending offset so GetEffectiveOffset adds correctly.
			_drawable.DragPreviewDelta = dragDelta;
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

	private void OnMapEndInteraction(object? sender, TouchEventArgs e)
	{
		if (_viewModel.ActiveTool == MapTool.Pan)
		{
			_panAnchorScreen = null;
			return;
		}

		if (_viewModel.ActiveTool == MapTool.Move && _isMoveDragging && _draggingAreaId is not null)
		{
			// Fold the live drag delta into the persistent pending offset
			var pending = _pendingMoveOffsets.GetValueOrDefault(_draggingAreaId.Value) ?? new TilePoint(0, 0);
			var drag = _drawable.DragPreviewDelta ?? new TilePoint(0, 0);
			var accumulated = new TilePoint(pending.Col + drag.Col, pending.Row + drag.Row);

			if (accumulated.Col != 0 || accumulated.Row != 0)
			{
				_pendingMoveOffsets[_draggingAreaId.Value] = accumulated;
			}
			else
			{
				_pendingMoveOffsets.Remove(_draggingAreaId.Value);
			}

			_drawable.PendingMoveOffsets[_draggingAreaId.Value] = accumulated;
			_drawable.DragPreviewAreaId = null;
			_drawable.DragPreviewDelta = null;
			_isMoveDragging = false;
			_draggingAreaId = null;
			_moveGrabTile = null;

			UpdateToolbarButtonState();
			MapCanvas.Invalidate();
		}
	}

	private void FinishDrawing()
	{
		var vertices = _viewModel.DrawingSession.Vertices;
		if (vertices.Count < 3)
		{
			AbortInProgressDraw();
			return;
		}

		var origin = vertices[0];
		var outline = _viewModel.DrawingSession.BuildOutline();

		if (_isDrawingNewArea)
		{
			_pendingNewAreas.Add((origin, outline));
			_drawable.GhostAreas.Add((origin, outline));
		}
		else if (_activeAreaId is Guid activeId)
		{
			_pendingOutlines[activeId] = (origin, outline);
			_drawable.PendingOutlines[activeId] = (origin, outline);
			// Stay in area context — user can inspect the result, rename, or redraw again
			UpdateActiveAreaSidebarHighlight();
		}

		_viewModel.DrawingSession.Reset();
		_drawState = DrawState.DrawingReady;
		_isDrawingNewArea = false;
		_drawable.InProgressVertices = [];
		_drawable.PhantomEnd = null;

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

	private void OnMapSecondaryTapped(object? sender, TappedEventArgs e) =>
		OnUndoClicked(sender, e);

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

			var (origin, outline) = _pendingOutlines.TryGetValue(area.Id, out var po)
				? po
				: (area.Origin, area.Outline);

			_pendingMoveOffsets.TryGetValue(area.Id, out var pendingOffset);

			var visualOrigin = pendingOffset is null
				? origin
				: new TilePoint(origin.Col + pendingOffset.Col, origin.Row + pendingOffset.Row);

			// Build pixel-space polygon vertices from the area outline
			var verts = new List<PointF>(outline.Count + 1);
			var (ox, oy) = TileCoordinateHelper.ToPixel(visualOrigin, _drawable.TileSize);
			verts.Add(new PointF(ox, oy));

			foreach (var seg in outline)
			{
				var visualEnd = pendingOffset is null
					? seg.End
					: new TilePoint(seg.End.Col + pendingOffset.Col, seg.End.Row + pendingOffset.Row);
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

	// ─── Map refresh ────────────────────────────────────────────────────────────

	private void RefreshMap()
	{
		var floor = _viewModel.Floors.FirstOrDefault(f => f.Id == _selectedFloorId);
		_drawable.Floor = floor;

		FloorNameToolbarEditor.Text = floor?.Name ?? string.Empty;

		if (_drawState == DrawState.Idle)
		{
			_drawable.InProgressVertices = [];
			_drawable.PhantomEnd = null;
		}

		MapToolbar.IsVisible = _selectedFloorId is not null;

		if (_selectedFloorId is not null && _drawState == DrawState.Idle)
		{
			_viewModel.ActiveTool = MapTool.Draw;
			_drawState = DrawState.DrawingReady;
		}

		SetToolbarContext(ToolbarContext.Floor);
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
				floorItem.SetAreaEditing(area.Id, area.Id == _activeAreaId);
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
