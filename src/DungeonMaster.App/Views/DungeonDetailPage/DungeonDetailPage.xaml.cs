using DungeonMaster.App.Assets.ObjectAssets;
using Plugin.Maui.KeyListener;
using DungeonMaster.App.Behaviors;
using DungeonMaster.App.Controls;
using DungeonMaster.App.Core.Editor;
using DungeonMaster.App.Core.Map;
using DungeonMaster.App.Core.Timing;
using DungeonMaster.App.Extensions;
using DungeonMaster.App.Graphics;
using DungeonMaster.App.Services.Platform;
using DungeonMaster.App.Theme;
using DungeonMaster.App.ViewModels;
using DungeonMaster.App.Views.DungeonDetail;
using DungeonMaster.App.Core.Presentation;
using DungeonMaster.Domain.Entities.Floors;
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

	// The editor's interaction state and its transitions. Every fact the page used to hold in a
	// private field — which floor is selected, which area is focused, where the draw state
	// machine stands, the area move drag, the pan anchor, the ghost rotation, and the whole
	// object hover/select/press/drag group — lives here, in a type with no MAUI dependency that
	// DungeonMaster.Tests exercises directly. See MapEditorState for the field-by-field mapping.
	private readonly MapEditorState _editor = new();

	// Floor list drag-and-drop plumbing. This is view work and stays: it is the placeholder Border
	// this page inserts into its own sidebar and the item it is currently dragging.
	private View? _dragPlaceholder;
	private FloorSidebarItem? _draggingItem;
	private FloorSidebarItem? _currentPlaceholderTarget;
	private bool _placeholderAfterTarget;

	// Constructed in the page constructor rather than initialized here: its timer is built on this
	// page's Dispatcher, which is an instance member (UIKIT-017 / MASTER-062).
	private readonly CanvasLongPressDetector _longPressDetector;

	private KeyboardBehavior? _keyboardBehavior;

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

		// The tick arrives on this page's dispatcher thread, so the detector's press flag is
		// written and read on one thread and no MainThread hop is needed inside the handler.
		_longPressDetector = new CanvasLongPressDetector(new DispatcherLongPressTimer(Dispatcher));

		BindingContext = viewModel;

		MapCanvas.Drawable = _drawable;

		// The drawable owns its draw state after MASTER-059, so it is the only thing that knows a
		// mutation happened. One subscription here is what UIKIT-008 asked for: a forgotten
		// Invalidate() after a Set.../Add.../Clear... call can no longer leave a stale frame. The
		// handler is never detached because the drawable is this page's alone and dies with it,
		// so it cannot outlive the page it captures.
		_drawable.RedrawRequested += (_, _) =>
			MapCanvas.Invalidate();

		// Wire object palette events
		ObjectPalette.ActiveObjectChanged += OnPaletteObjectChanged;

		// ObjectPaletteView has no view model of its own; its guarded handlers report here.
		ObjectPalette.LoadFailed += (_, message) =>
			_viewModel.ErrorMessage = message;
		ObjectPalette.CloseRequested += (_, _) =>
			SetActiveTool(MapTool.Pan);
		ObjectPalette.SecondaryPaletteRequested += OnSecondaryPaletteRequested;
		ObjectPalette.SecondaryPaletteDismissed += OnSecondaryPaletteDismissed;
		ObjectPalette.PackChanged += async (_, _) =>
		{
			// async void lambda: an escaping exception is UNOBSERVED and is rethrown on the
			// synchronisation context, so the try opens before the first await.
			try
			{
				_drawable.ClearObjectImageCache();
				await _drawable.PreloadObjectImagesAsync();
				MapCanvas.Invalidate();
			}
			catch (Exception exception)
			{
				_viewModel.ErrorMessage = $"Could not load the images for that object pack: {exception.Message}";
			}
		};

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

		ControlsPalette.RotateRequested += async (_, dir) =>
		{
			// async void: an escaping exception is UNOBSERVED and is rethrown on the
			// synchronisation context, which takes the process down. The try opens before the
			// first await for that reason, not as defensive style.
			try
			{
				if (ObjectPalette.ActiveObject is not null)
				{
					_editor.GhostRotation = dir == RotationDirection.Clockwise
						? (ObjectRotation)(((int)_editor.GhostRotation + 1) % 4)
						: (ObjectRotation)(((int)_editor.GhostRotation + 3) % 4);
					_drawable.GhostRotation = _editor.GhostRotation;
					MapCanvas.Invalidate();
				}
				else
				{
					_editor.SelectedObjectId ??= _editor.HoveredObjectId;
					if (_editor.SelectedObjectId is not null)
					{
						await RotateObjectAsync(dir == RotationDirection.Clockwise);
					}
				}
			}
			catch (Exception exception)
			{
				_viewModel.ErrorMessage = $"Could not rotate that object: {exception.Message}";
			}
		};
		ControlsPalette.ScaleRequested += async (_, dir) =>
		{
			// async void: an escaping exception is UNOBSERVED and is rethrown on the
			// synchronisation context, which takes the process down. The try opens before the
			// first await for that reason, not as defensive style.
			try
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
					_editor.SelectedObjectId ??= _editor.HoveredObjectId;
					if (_editor.SelectedObjectId is not null)
					{
						await ScaleObjectAsync(down);
					}
				}
			}
			catch (Exception exception)
			{
				_viewModel.ErrorMessage = $"Could not resize that object: {exception.Message}";
			}
		};

		AttachRuntimeHandlers();
	}

	// ─── Page lifecycle ─────────────────────────────────────────────────────────
	//
	// Subscriptions are split into two groups:
	//
	//  * One-time construction wiring (above) — palette events and popup button behaviors. Every
	//    one of those targets a child control this page owns outright, so they live and die with
	//    the page and never need detaching.
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
		_viewModel.FloorList.Items.CollectionChanged += OnFloorsChanged;

		MapCanvas.HandlerChanged += OnMapCanvasHandlerChanged;
		_longPressDetector.LongPressed += OnObjectLongPressed;
		Notebook.SaveRequested += OnNotebookSaveRequested;

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
		_viewModel.FloorList.Items.CollectionChanged -= OnFloorsChanged;

		MapCanvas.HandlerChanged -= OnMapCanvasHandlerChanged;
		_longPressDetector.LongPressed -= OnObjectLongPressed;
		Notebook.SaveRequested -= OnNotebookSaveRequested;

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
	/// <remarks>
	///		ApplyQueryAttributes starts the load and holds it on <c>LoadTask</c>; this is where the
	///		page observes it. Without the await the load is a task nobody holds: a faulted one is
	///		never seen, and the page sits empty with nothing anywhere saying why.
	/// </remarks>
	protected override async void OnNavigatedTo(NavigatedToEventArgs args)
	{
		// async void: an escaping exception is UNOBSERVED and is rethrown on the
		// synchronisation context, which takes the process down. The try opens before the
		// first await for that reason, not as defensive style.
		try
		{
			base.OnNavigatedTo(args);
			AttachRuntimeHandlers();

			await _viewModel.LoadTask;
		}
		catch (Exception exception)
		{
			_viewModel.ErrorMessage = $"Could not load that dungeon: {exception.Message}";
		}
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

			// UIKIT-001 / MASTER-013: the drawable owns every decoded object image, each of which
			// wraps a native bitmap. The page created it and nothing else holds it, so this is
			// where the handles are released. Dispose() is idempotent and never throws, and the
			// drawable stays drawable afterwards - it paints placeholders rather than failing if
			// the framework drives it again.
			_drawable.Dispose();
		}
	}

	// ─── Keyboard shortcuts ─────────────────────────────────────────────────────

	private bool IsTextInputActive()
	{
#if WINDOWS
		// Check native focus — more reliable than MAUI Focused events because DmInlineEditor
		// focuses an internal TextBox that doesn't bubble to the ContentView.Focused event.
		// The notebook's Editor panes are TextBoxes on WinUI too, so they are covered here.
		if (Window?.Handler?.PlatformView is Microsoft.UI.Xaml.Window winUiWindow)
		{
			var focused = Microsoft.UI.Xaml.Input.FocusManager.GetFocusedElement(winUiWindow.Content.XamlRoot);
			return focused is Microsoft.UI.Xaml.Controls.TextBox;
		}
#elif MACCATALYST
		// TODO(macOS): UIKit does not expose a global FocusManager equivalent, and this page no
		// longer owns any text input directly — the only ones left are inside the flag banner and
		// the notebook, neither of which reports focus to the page. macCatalyst therefore has no
		// guard at all: add a UIResponder.IsFirstResponder walk over the page's platform view, or
		// surface a focus event from NotebookView, before shipping that platform.
#endif
		return false;
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

	private async void OnKeyDown(object? sender, KeyPressedEventArgs e)
	{
		if (!IsPageInteractive())
			return;

		if (IsTextInputActive())
			return;

		// async void: an escaping exception is UNOBSERVED and is rethrown on the
		// synchronisation context, which takes the process down. The try opens before the
		// first await for that reason, not as defensive style.
		try
		{
			switch (char.ToLower(e.KeyChar))
			{
				case 'd':
					if (_viewModel.Canvas.ActiveTool == MapTool.Draw && _editor.DrawState == DrawState.Drawing)
						AbortInProgressDraw();   // pressing D while drawing cancels in-progress vertices
					else
						ToggleOrSetTool(MapTool.Draw);
					break;
				case 'm': ToggleOrSetTool(MapTool.Move); break;
				case 'h': ToggleOrSetTool(MapTool.Pan); break;
				case 'p': ToggleOrSetTool(MapTool.Place); break;
				case 's': ToggleOrSetTool(MapTool.Select); break;
				case 'e': ToggleOrSetTool(MapTool.Erase); break;
				case 'r':
					if (_viewModel.Canvas.ActiveTool == MapTool.Place)
					{
						if (ObjectPalette.ActiveObject is not null)
						{
							// Rotate ghost preview.
							_editor.GhostRotation = (ObjectRotation)(((int)_editor.GhostRotation + 1) % 4);
							_drawable.GhostRotation = _editor.GhostRotation;
							MapCanvas.Invalidate();
						}
						else
						{
							// Rotate selected or hovered placed object.
							_editor.SelectedObjectId ??= _editor.HoveredObjectId;
							if (_editor.SelectedObjectId is not null)
								await RotateObjectAsync(clockwise: true);
						}
					}
					break;
				case '[':
					if (_viewModel.Canvas.ActiveTool == MapTool.Place)
					{
						if (ObjectPalette.ActiveObject is not null)
						{
							_drawable.GhostScale = ObjectScale.StepDown(_drawable.GhostScale);
							ControlsPalette.UpdateScale(_drawable.GhostScale);
							MapCanvas.Invalidate();
						}
						else
						{
							_editor.SelectedObjectId ??= _editor.HoveredObjectId;
							if (_editor.SelectedObjectId is not null)
							{
								await ScaleObjectAsync(down: true);
							}
						}
					}
					break;
				case ']':
					if (_viewModel.Canvas.ActiveTool == MapTool.Place)
					{
						if (ObjectPalette.ActiveObject is not null)
						{
							_drawable.GhostScale = ObjectScale.StepUp(_drawable.GhostScale);
							ControlsPalette.UpdateScale(_drawable.GhostScale);
							MapCanvas.Invalidate();
						}
						else
						{
							_editor.SelectedObjectId ??= _editor.HoveredObjectId;
							if (_editor.SelectedObjectId is not null)
							{
								await ScaleObjectAsync(down: false);
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
		catch (Exception exception)
		{
			_viewModel.ErrorMessage = $"Could not apply that keyboard shortcut: {exception.Message}";
		}
	}

	// If tool is already active, revert to Pan (deactivate); otherwise activate.
	private void ToggleOrSetTool(MapTool tool)
	{
		if (_viewModel.Canvas.ActiveTool == tool)
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

	private async void OnMapCanvasHandlerChanged(object? sender, EventArgs e)
	{
		DetachScrollZoom();
		AttachScrollZoom();

		// HandlerChanged also fires with a null handler on teardown; there is nothing to
		// preload into then.
		if (MapCanvas.Handler is null)
		{
			return;
		}

		// async void: an escaping exception is UNOBSERVED and is rethrown on the
		// synchronisation context, which takes the process down. The try opens before the
		// first await for that reason, not as defensive style.
		try
		{
			// First point at which the platform view — and on Windows the Win2D resource
			// creator — exists, so this is where the object images are decoded. Draw no longer
			// loads them itself; a kind still missing from the cache renders the placeholder.
			await _drawable.PreloadObjectImagesAsync();
			MapCanvas.Invalidate();
		}
		catch (Exception exception)
		{
			_viewModel.ErrorMessage = $"Could not load the object images: {exception.Message}";
		}
	}

	private void AttachScrollZoom()
	{
#if WINDOWS
		// UI-018 / MASTER-055: the native element is captured at subscribe time so the unsubscribe
		// below cannot depend on MapCanvas.Handler still resolving. Nothing orders the canvas
		// handler's disconnect against the page's own, and a re-resolve that comes back null
		// unsubscribes nothing while the element goes on holding the delegate.
		_scrollZoomSubscription ??= new DungeonMaster.App.Core.Interop.CapturedSubscription<Microsoft.UI.Xaml.FrameworkElement>(
			() => MapCanvas.Handler?.PlatformView as Microsoft.UI.Xaml.FrameworkElement,
			element => element.PointerWheelChanged += OnMapScrollWheel,
			element => element.PointerWheelChanged -= OnMapScrollWheel);

		_scrollZoomSubscription.Attach();
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
		// Unsubscribes from the captured element and clears it. Deliberately does NOT read
		// MapCanvas.Handler?.PlatformView: that re-resolve is the defect UI-018 records.
		_scrollZoomSubscription?.Detach();
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
	// The one scroll-zoom subscription, created on first attach and reused across the
	// detach/attach pairs the page runs whenever the canvas handler changes.
	private DungeonMaster.App.Core.Interop.CapturedSubscription<Microsoft.UI.Xaml.FrameworkElement>? _scrollZoomSubscription;

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
			TitleBanner.Title = _viewModel.Header.DungeonName;
			RefreshDungeonNotebookButton();
		}
	}

	/// <summary>
	///		Re-applies the dungeon row's notebook icon colour, matching how the floor and area rows
	///		colour theirs when they repopulate: gold once the entity has authored text, dim before.
	/// </summary>
	private void RefreshDungeonNotebookButton()
	{
		var hasContent =
			!string.IsNullOrWhiteSpace(_viewModel.Header.DungeonDescription) ||
			!string.IsNullOrWhiteSpace(_viewModel.Header.DungeonNotes);

		DungeonNotebookButton.TextColor = hasContent
			? DmTheme.ScrollGoldLight
			: DmTheme.InkMuted;
	}

	private void OnDungeonNotebookClicked(object? sender, EventArgs e)
	{
		if (_viewModel.Dungeon is null)
		{
			return;
		}

		Notebook.Open(new NotebookTarget(
			_viewModel.Dungeon.Id,
			null,
			NotebookKind.Dungeon,
			_viewModel.Header.DungeonName,
			_viewModel.Header.DungeonDescription,
			_viewModel.Header.DungeonNotes));
	}

	// Every write path below ends in a full reload that clears Floors and rebuilds every sidebar
	// item, so no FloorItem or AreaItem may be held across the await — only identifiers survive it.
	//
	// All three branches follow one rule: description and notes are written exactly as typed, but a
	// blank name falls back to the entity's last known good one. Each level punishes a blank name
	// differently and all of them badly — UpdateAreaCommandValidator rejects an empty area name and
	// drops the entire write, description and notes included, while a floor name has no validator
	// at all and would persist blank. Clearing a name is not something a description edit should be
	// able to do in passing, so the fallback is applied uniformly rather than per level.
	private async void OnNotebookSaveRequested(object? sender, NotebookSaveArgs e)
	{
		// async void: an escaping exception is UNOBSERVED and is rethrown on the
		// synchronisation context, which takes the process down. The try opens before the
		// first await for that reason, not as defensive style.
		try
		{
			switch (e.Kind)
			{
				case NotebookKind.Dungeon:
					{
						if (!string.IsNullOrWhiteSpace(e.Name))
						{
							_viewModel.Header.DungeonName = e.Name;
						}

						_viewModel.Header.DungeonDescription = e.Description;
						_viewModel.Header.DungeonNotes = e.Notes;

						// The header's save now reloads the detail on success, like every other
						// mutation. The page therefore no longer chases it with a second load of
						// its own: the banner and the sidebar row repaint from that reload.
						await _viewModel.Header.SaveCommand.ExecuteAsync(null);

						// A blank name never overwrote DungeonName above, so it still holds the last known
						// good one. The notebook does not know that — it is still showing what the user
						// typed — so tell it, unless it has since been retargeted to a different entity.
						if (_viewModel.Header.DungeonName != e.Name && Notebook.CurrentId == e.Id)
						{
							Notebook.SyncName(_viewModel.Header.DungeonName);
						}

						break;
					}

				case NotebookKind.Floor:
					{
						// Resolved before the await, because the reload it triggers replaces this instance.
						var lastGoodName = _viewModel.FloorList.Items
							.FirstOrDefault(floorItem => floorItem.Id == e.Id)?.Name;

						var floorName = string.IsNullOrWhiteSpace(e.Name) ? lastGoodName : e.Name;

						await _viewModel.FloorList.UpdateFloorCommand.ExecuteAsync(
							new UpdateFloorArgs(e.Id, floorName, e.Description, e.Notes));

						// Same fallback-visibility gap as the dungeon case above.
						if (floorName != e.Name && Notebook.CurrentId == e.Id)
						{
							Notebook.SyncName(floorName ?? string.Empty);
						}

						break;
					}

				case NotebookKind.Area:
					{
						// An area is addressed through its owning floor; without one there is nothing to save.
						if (e.FloorId is null)
						{
							return;
						}

						var lastGoodName = _viewModel.FloorList.Items
							.FirstOrDefault(floorItem => floorItem.Id == e.FloorId.Value)?.Areas
							.FirstOrDefault(areaItem => areaItem.Id == e.Id)?.Name;

						var areaName = string.IsNullOrWhiteSpace(e.Name) ? lastGoodName : e.Name;

						// No name typed and no row left to borrow one from: the validator would reject the
						// write, so abandon it here rather than sending something that cannot succeed.
						if (string.IsNullOrWhiteSpace(areaName))
						{
							return;
						}

						await _viewModel.Canvas.UpdateAreaCommand.ExecuteAsync(
							new UpdateAreaArgs(e.FloorId.Value, e.Id, areaName, e.Description, e.Notes));

						// Same fallback-visibility gap as the dungeon case above.
						if (areaName != e.Name && Notebook.CurrentId == e.Id)
						{
							Notebook.SyncName(areaName);
						}

						break;
					}
			}

			// The reload replaced every floor projection, so the drawable is still pointing at the
			// pre-save instance. Re-resolve it by id rather than reusing the captured DTO.
			_drawable.Floor = _viewModel.FloorList.Items.FirstOrDefault(floorItem => floorItem.Id == _editor.SelectedFloorId);
			RefreshDungeonNotebookButton();
			MapCanvas.Invalidate();
		}
		catch (Exception exception)
		{
			_viewModel.ErrorMessage = $"Could not save that notebook entry: {exception.Message}";
		}
	}

	private void OnFloorsChanged(
		object? sender,
		System.Collections.Specialized.NotifyCollectionChangedEventArgs e) =>
		RebuildFloorList();

	private void RebuildFloorList()
	{
		FloorListContainer.Children.Clear();

		foreach (var floor in _viewModel.FloorList.Items)
		{
			var item = new FloorSidebarItem { Floor = floor };

			// Each floor row is a heading in the sidebar's outline, one level below the FLOORS
			// caption the XAML carries. Without it a screen reader reads the sidebar as an
			// undifferentiated run of controls and offers no way to jump between floors.
			SemanticProperties.SetHeadingLevel(item, SemanticHeadingLevel.Level3);

			item.DeleteRequested += async (_, floorToDelete) =>
			{
				// async void: an escaping exception is UNOBSERVED and is rethrown on the
				// synchronisation context, which takes the process down. The try opens before the
				// first await for that reason, not as defensive style.
				try
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
					var wasSelected = _editor.SelectedFloorId == floorToDelete.Id;
					if (wasSelected)
					{
						ClearAreaFocus();
						_editor.SelectedFloorId = null;
						_drawable.Floor = null;
						_drawable.HoveredAreaId = null;
						ResetDrawState();
						SetMapToolbarShown(false);
					}

					await _viewModel.FloorList.RemoveFloorCommand.ExecuteAsync(floorToDelete.Id);

					// The deleted floor, or any of its areas, may be what the notebook is open on.
					CloseNotebookIfOrphaned();

					if (wasSelected)
					{
						UpdateToolbarButtonState();
						MapCanvas.Invalidate();
					}
				}
				catch (Exception exception)
				{
					_viewModel.ErrorMessage = $"Could not delete that floor: {exception.Message}";
				}
			};

			item.SelectRequested += (_, _) =>
			{
				// Re-tapping the floor card while an area is focused clears the focus first;
				// a further tap falls through to the deselect branch below.
				if (_editor.FocusedAreaId is not null && _editor.SelectedFloorId == floor.Id)
				{
					ClearAreaFocus();
					return;
				}

				// Re-tapping the already-selected floor toggles it off: collapse it and route
				// back through RefreshMap so the resulting state is identical to the page's
				// initial no-floor-selected state rather than a hand-rolled second copy of it.
				if (_editor.SelectedFloorId == floor.Id)
				{
					_editor.SelectedFloorId = null;
					item.IsExpanded = false;
					ResetDrawState();
					RefreshMap();

					// Collapsing the floor also closes its notebook if one is open on it or on
					// one of its areas — the entity still exists, but its row is no longer shown
					// selected/expanded, so keeping the notebook pinned to it reads as stale.
					if (Notebook.CurrentId is Guid notebookId
						&& (notebookId == floor.Id || floor.Areas.Any(areaItem => areaItem.Id == notebookId)))
					{
						Notebook.Close();
					}

					return;
				}

				_editor.SelectedFloorId = floor.Id;
				CollapseAllExcept(item);
				RefreshMap();
			};

			item.ReorderRequested += async (_, args) =>
			{
				// async void: an escaping exception is UNOBSERVED and is rethrown on the
				// synchronisation context, which takes the process down. The try opens before the
				// first await for that reason, not as defensive style.
				try
				{
					_editor.SelectedFloorId = args.DraggedFloorId;
					await _viewModel.FloorList.ReorderFloorCommand.ExecuteAsync(
						new ReorderFloorArgs(args.DraggedFloorId, args.TargetNumber));
				}
				catch (Exception exception)
				{
					_viewModel.ErrorMessage = $"Could not reorder the floors: {exception.Message}";
				}
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
			{
				// async void lambda: the try opens before the first await so nothing escapes.
				try
				{
					await _viewModel.Canvas.AddAreaCommand.ExecuteAsync(floor.Id);
				}
				catch (Exception exception)
				{
					_viewModel.ErrorMessage = $"Could not add an area: {exception.Message}";
				}
			};

			// The five area commands are set on the floor item and read straight off it by each
			// area row's template binding (MASTER-099). They used to be five subscriptions to
			// five FloorSidebarItem events that existed only to re-raise the area row's own.
			item.AreaSelectCommand = new Command<AreaItem>(area =>
			{
				if (_editor.FocusedAreaId == area.Id)
				{
					ClearAreaFocus();
					return;
				}

				FocusArea(floor.Id, area);
			});

			item.AreaHoverStartedCommand = new Command<AreaItem>(area =>
			{
				_drawable.HoveredAreaId = area.Id;
				MapCanvas.Invalidate();
			});

			item.AreaHoverEndedCommand = new Command<AreaItem>(_ =>
			{
				_drawable.HoveredAreaId = null;
				MapCanvas.Invalidate();
			});

			// Sidebar delete is the only way to remove an area with an empty outline —
			// the Erase tool needs outline geometry to hit-test against.
			item.AreaDeleteCommand = new Command<AreaItem>(async areaToDelete =>
			{
				// async void lambda: the try opens before the first await so nothing escapes.
				try
				{
					await DeleteAreaAsync(floor.Id, areaToDelete);
				}
				catch (Exception exception)
				{
					_viewModel.ErrorMessage = $"Could not delete that area: {exception.Message}";
				}
			});

			item.NotebookRequested += (_, floorForNotebook) =>
				Notebook.Open(new NotebookTarget(
					floorForNotebook.Id,
					floorForNotebook.Id,
					NotebookKind.Floor,
					floorForNotebook.Name ?? string.Empty,
					floorForNotebook.Description,
					floorForNotebook.Notes));

			item.AreaNotebookCommand = new Command<AreaItem>(areaForNotebook =>
				Notebook.Open(new NotebookTarget(
					areaForNotebook.Id,
					floor.Id,
					NotebookKind.Area,
					areaForNotebook.Name,
					areaForNotebook.Description,
					areaForNotebook.Notes)));

			FloorListContainer.Children.Add(item);

			if (floor.Id == _editor.SelectedFloorId)
			{
				item.IsExpanded = true;
			}
		}

		// A rebuild recreates the area items, so re-apply the focused area's selected visual.
		UpdateActiveAreaSidebarHighlight();
	}

	// Confirms and removes an area from the sidebar. Placed objects belong to the floor,
	// so they survive area deletion; only the annotation is removed.
	private async Task DeleteAreaAsync(Guid floorId, AreaItem area)
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

		if (_editor.FocusedAreaId == area.Id)
		{
			ClearAreaFocus();
		}

		_drawable.HoveredAreaId = null;

		await _viewModel.Canvas.RemoveAreaCommand.ExecuteAsync(
			new RemoveAreaArgs(floorId, area.Id));
		await RefreshFloorAsync();
		CloseNotebookIfOrphaned();
		MapCanvas.Invalidate();
	}

	// ─── Area focus ─────────────────────────────────────────────────────────────

	// Focuses an area so the canvas dims every other area. Display only — it gates nothing.
	private void FocusArea(Guid floorId, AreaItem area)
	{
		if (floorId != _editor.SelectedFloorId)
		{
			return;
		}

		// Guard against a stale sidebar row whose area no longer exists in the current data.
		var currentFloor = _viewModel.FloorList.Items.FirstOrDefault(floorItem => floorItem.Id == floorId);
		if (currentFloor?.Areas.Any(areaItem => areaItem.Id == area.Id) != true)
		{
			ClearAreaFocus();
			return;
		}

		if (_editor.DrawState == DrawState.Drawing)
		{
			AbortInProgressDraw();
		}

		// Abort any in-progress move drag
		if (_editor.IsMoveDragging)
		{
			_editor.EndMoveDrag();
			_drawable.DragPreviewAreaId = null;
			_drawable.DragPreviewDelta = null;
		}

		_editor.FocusedAreaId = area.Id;
		_drawable.ActiveAreaId = area.Id;

		UpdateActiveAreaSidebarHighlight();
		UpdateToolbarButtonState();
		MapCanvas.Invalidate();
	}

	// Clears the area focus so every area renders at full opacity again.
	private void ClearAreaFocus()
	{
		_editor.FocusedAreaId = null;
		_drawable.ActiveAreaId = null;

		UpdateActiveAreaSidebarHighlight();
		MapCanvas.Invalidate();
	}

	// The MAUI-touching half of ending an in-progress draw: drop the vertices the session was
	// collecting and the canvas preview painted from them. It deliberately does NOT move the draw
	// state, because the two callers want it moved to different places and neither of them
	// computes the value — MapEditorState does.
	private void ClearInProgressDraw()
	{
		_viewModel.Canvas.DrawingSession.Reset();
		_drawable.ClearInProgressVertices();
		_drawable.PhantomEnd = null;
		_drawable.ActiveAreaId = _editor.FocusedAreaId;
	}

	// Abandons an in-progress draw while STAYING on the Draw tool, so the tool re-arms and a new
	// outline can be started. A tool change does not come through here: it ends at Idle, and
	// MapEditorState.SetActiveTool has already put the state there by the time it reports that an
	// abort is owed.
	private void AbortInProgressDraw()
	{
		_editor.ResetDrawState(_editor.SelectedFloorId is not null);
		ClearInProgressDraw();
	}

	// The page names the cursor shape the tool implies; each platform maps that shape to its own
	// native cursor behind PlatformCanvasCursor. No WinUI or AppKit type is named here, and the
	// ProtectedCursor reflection that used to sit in this method now lives in the Windows file and
	// is resolved once for the process rather than on every call.
	private void SetCanvasCursor(MapTool tool, bool isDragging = false)
	{
		var shape = tool switch
		{
			MapTool.Draw => CanvasCursorShape.Crosshair,
			MapTool.Erase => CanvasCursorShape.Crosshair,
			MapTool.Move => CanvasCursorShape.Move,
			MapTool.Pan => isDragging
				? CanvasCursorShape.Grabbing
				: CanvasCursorShape.Grab,
			_ => CanvasCursorShape.Arrow,
		};

		PlatformCanvasCursor.SetShape(MapCanvas.Handler?.PlatformView, shape);
	}

	// ─── Toolbar handlers ────────────────────────────────────────────────────────


	// The state half of a tool change belongs to MapEditorState, which also clears the whole object
	// interaction group and reports back what the view still owes. Everything below the call is a
	// MAUI-touching consequence of that decision and nothing here computes a DrawState.
	private void SetActiveTool(MapTool tool)
	{
		_viewModel.Canvas.ActiveTool = tool;

		var change = _editor.SetActiveTool(tool, _viewModel.Canvas.DrawingSession.IsActive);

		// ClearInProgressDraw, not AbortInProgressDraw: a tool change ends at Idle and the state
		// machine has already put it there. Re-arming here would silently leave the Draw tool
		// primed on a tool the user has just left.
		if (change.AbortInProgressDraw)
		{
			ClearInProgressDraw();
		}

		// Clear canvas hover highlight when leaving tools that show it.
		if (change.ClearHoveredArea)
		{
			_drawable.HoveredAreaId = null;
		}

		// Dismiss object popup and clear any hover highlight when switching tools. The state side
		// of this was done by SetActiveTool above; these are the drawable and the popup control.
		_drawable.HoveredObjectId = null;
		_drawable.DraggingObjectId = null;
		_drawable.DraggingObjectTargetCanvas = null;
		ObjectActionPopup.IsVisible = false;

		ObjectPalette.IsVisible = tool == MapTool.Place;
		ControlsPalette.IsVisible = tool == MapTool.Place && ObjectPalette.ActiveObject is not null;
		if (change.LeavingPlaceTool)
		{
			OnSecondaryPaletteDismissed(null, EventArgs.Empty);
			_drawable.GhostObjectKind = null;
			_drawable.GhostCanvasPosition = null;
			_drawable.GhostRotation = ObjectRotation.None;
			_editor.GhostRotation = ObjectRotation.None;
			_drawable.GhostScale = ObjectScale.Default;
			ControlsPalette.ResetScale();
			MapCanvas.Invalidate();
		}

		SetCanvasCursor(tool);
		UpdateToolbarButtonState();
	}

	private void OnToolDrawClicked(object? sender, EventArgs e) =>
		ToggleOrSetTool(MapTool.Draw);

	private void OnToolMoveClicked(object? sender, EventArgs e) =>
		ToggleOrSetTool(MapTool.Move);

	private void OnToolSelectClicked(object? sender, EventArgs e) =>
		ToggleOrSetTool(MapTool.Select);

	private void OnToolPanClicked(object? sender, EventArgs e) =>
		ToggleOrSetTool(MapTool.Pan);

	private void OnToolPlaceClicked(object? sender, EventArgs e) =>
		ToggleOrSetTool(MapTool.Place);

	private void OnToolEraseClicked(object? sender, EventArgs e) =>
		ToggleOrSetTool(MapTool.Erase);

	private async Task EraseAreaAsync(Guid floorId, AreaItem area)
	{
		_drawable.HoveredAreaId = null;
		MapCanvas.Invalidate();
		await _viewModel.Canvas.RemoveAreaCommand.ExecuteAsync(
			new RemoveAreaArgs(floorId, area.Id));
		await RefreshFloorAsync();
		CloseNotebookIfOrphaned();
		MapCanvas.Invalidate();
	}

	// ─── Object palette handlers ─────────────────────────────────────────────────

	private async Task PlaceObjectAsync(
		Guid floorId,
		ObjectKind kind,
		CanvasPoint position,
		ObjectRotation rotation,
		float scale)
	{
		await _viewModel.Canvas.AddPlacedObjectCommand.ExecuteAsync(
			new PlaceObjectArgs(floorId, kind, position, rotation, scale));
		// Reload only the floor data — do NOT call RefreshMap(), which would reset tool/context state.
		await RefreshFloorAsync();
		MapCanvas.Invalidate();
	}

	private void OnPaletteObjectChanged(object? sender, ObjectKind? kind)
	{
		_drawable.GhostObjectKind = kind;
		_editor.GhostRotation = ObjectRotation.None;
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

	/// <summary>
	///		Shows or hides the map toolbar without letting it leave the layout.
	/// </summary>
	/// <param name="shown">
	///		<see langword="true"/> once a floor is selected and the drawing tools apply.
	/// </param>
	/// <remarks>
	///		Opacity rather than <c>IsVisible</c>, on purpose. The toolbar and the sidebar dungeon
	///		card share one <c>Auto</c> row of the designer's grid, which is what makes their bottom
	///		edges agree without a measured height token; a collapsed element contributes nothing to
	///		an <c>Auto</c> row, so hiding this one with <c>IsVisible</c> would shrink that row to
	///		the card and then jump the whole map down when the first floor was picked.
	///		<c>InputTransparent</c> is what keeps it genuinely un-clickable in the meantime.
	/// </remarks>
	private void SetMapToolbarShown(bool shown)
	{
		MapToolbar.Opacity = shown ? 1 : 0;
		MapToolbar.InputTransparent = !shown;
	}

	private void UpdateToolbarButtonState()
	{
		var isDrawActive = _viewModel.Canvas.ActiveTool == MapTool.Draw;
		var isMoveActive = _viewModel.Canvas.ActiveTool == MapTool.Move;
		var isPanActive = _viewModel.Canvas.ActiveTool == MapTool.Pan;
		var isPlaceActive = _viewModel.Canvas.ActiveTool == MapTool.Place;
		var isSelectActive = _viewModel.Canvas.ActiveTool == MapTool.Select;
		var isEraseActive = _viewModel.Canvas.ActiveTool == MapTool.Erase;

		ToolDrawTile.IsActive = isDrawActive;
		ToolMoveTile.IsActive = isMoveActive;
		ToolSelectTile.IsActive = isSelectActive;
		ToolEraseTile.IsActive = isEraseActive;
		ToolPlaceTile.IsActive = isPlaceActive;
		ToolPanTile.IsActive = isPanActive;

		var hasVertices = _viewModel.Canvas.DrawingSession.Vertices.Count > 0;
		UndoButton.IsEnabled = hasVertices;
		UndoButton.Opacity = hasVertices ? 1.0 : 0.35;
	}

	private void OnUndoClicked(object? sender, EventArgs e)
	{
		if (_viewModel.Canvas.DrawingSession.UndoLastVertex())
		{
			if (!_viewModel.Canvas.DrawingSession.IsActive)
			{
				// Undoing the last vertex leaves the Draw tool armed rather than idle, which is
				// exactly what MapEditorState.ResetDrawState reports for a selected floor.
				_editor.ResetDrawState(_editor.SelectedFloorId is not null);
			}

			_drawable.SetInProgressVertices(_viewModel.Canvas.DrawingSession.Vertices);
			var hasVertices = _viewModel.Canvas.DrawingSession.Vertices.Count > 0;
			UndoButton.IsEnabled = hasVertices;
			UndoButton.Opacity = hasVertices ? 1.0 : 0.35;
			MapCanvas.Invalidate();
		}
	}

	// ─── Auto-save helpers ───────────────────────────────────────────────────────

	// Reloads floor data from the VM after any auto-save operation and syncs the drawable.
	// The dungeon identifier is the editing context's, never the page's: ReloadAsync re-runs the
	// detail query for the dungeon the view model already holds, so nothing here re-takes an id
	// through a null-forgiving read of Dungeon.
	private async Task RefreshFloorAsync()
	{
		if (_editor.SelectedFloorId is null) return;
		await _viewModel.ReloadAsync();
		_drawable.Floor = _viewModel.FloorList.Items.FirstOrDefault(floorItem => floorItem.Id == _editor.SelectedFloorId);
	}

	private void ResetDrawState()
	{
		_viewModel.Canvas.DrawingSession.Reset();

		// The draw state and the area-move drag move together, and MapEditorState owns both
		// transitions; the page computes neither value.
		_editor.ResetDrawState(_editor.SelectedFloorId is not null);

		_drawable.ClearInProgressVertices();
		_drawable.PhantomEnd = null;
		_drawable.ClearPendingMoveOffsets();
		_drawable.ClearGhostAreas();
		_drawable.DragPreviewAreaId = null;
		_drawable.DragPreviewDelta = null;
	}

	// ─── Map canvas gesture handlers ────────────────────────────────────────────

	private async void OnMapStartInteraction(object? sender, TouchEventArgs e)
	{
		// GraphicsView's StartInteraction fires for all pointer buttons including right-click, so
		// the secondary button is suppressed here; right-click triggers Undo/Deselect through the
		// separate OnMapSecondaryTapped handler instead. Each platform answers the question in its
		// own file behind PlatformPointer, including the macCatalyst gap the TODO(macOS) note
		// there records.
		if (PlatformPointer.IsSecondaryButtonPressed())
		{
			return;
		}

		// async void: an escaping exception is UNOBSERVED and is rethrown on the
		// synchronisation context, which takes the process down. The try opens before the
		// first await for that reason, not as defensive style.
		try
		{
			var touchPoint = new CanvasPoint((float)e.Touches[0].X, (float)e.Touches[0].Y);
			_editor.LastTouchPoint = touchPoint;

			// GraphicsView on WinUI consumes all pointer events, so overlaid MAUI buttons never
			// fire Clicked. Instead we manually hit-test the popup rect and dispatch the action.
			if (ObjectActionPopup.IsVisible
				&& _editor.PopupBounds is CanvasRect popupBounds
				&& popupBounds.Contains(touchPoint.X, touchPoint.Y))
			{
				_longPressDetector.TouchStarted();
				var relX = touchPoint.X - popupBounds.X;
				var zoneW = popupBounds.Width / 5f;
				if (relX < zoneW)
					await RotateObjectAsync(clockwise: false);
				else if (relX < zoneW * 2)
					await RotateObjectAsync(clockwise: true);
				else if (relX < zoneW * 3)
					await ScaleObjectAsync(down: true);
				else if (relX < zoneW * 4)
					await ScaleObjectAsync(down: false);
				else
					await DeleteSelectedObjectAsync();
				return;
			}

			_longPressDetector.TouchStarted();

			if (_viewModel.Canvas.ActiveTool == MapTool.Pan)
			{
				_editor.PanAnchorScreen = touchPoint;
				SetCanvasCursor(MapTool.Pan, isDragging: true);
				return;
			}

			var rawPoint = new PointF((float)e.Touches[0].X, (float)e.Touches[0].Y);
			var canvasHitPoint = _drawable.ScreenToCanvas(rawPoint);

			// Object interaction: the Select tool, or Place mode with no active palette item.
			// Left-click with a palette item = place; otherwise inspect/drag existing objects.
			if (_viewModel.Canvas.ActiveTool == MapTool.Select
				|| (_viewModel.Canvas.ActiveTool == MapTool.Place && ObjectPalette.ActiveObject is null))
			{
				var hitObjectId = GetObjectAtCanvasPoint(canvasHitPoint);
				if (hitObjectId is not null)
				{
					// Enter pending state — we disambiguate click vs drag in EndInteraction.
					// BeginObjectPress records the id and the drag anchor and clears the
					// confirmation flag; the long-press flag is cleared alongside it.
					_editor.BeginObjectPress(hitObjectId.Value, touchPoint);
					_editor.ObjectLongPressHandled = false;
					_editor.HoveredObjectId = hitObjectId;
					_drawable.HoveredObjectId = hitObjectId;
					MapCanvas.Invalidate();
					return;
				}

				// Click on empty canvas — dismiss popup and clear hover highlight. Releasing the
				// pending press is what ClearObjectInteraction is for, and every other field it
				// clears is already null or false at the start of an interaction on empty canvas.
				_editor.ClearObjectInteraction();
				_drawable.HoveredObjectId = null;
				if (ObjectActionPopup.IsVisible)
				{
					ObjectActionPopup.IsVisible = false;
				}
				MapCanvas.Invalidate();
				// Don't return — let the click fall through to tool logic.
			}

			if (_viewModel.Canvas.ActiveTool == MapTool.Erase
				&& _editor.SelectedFloorId is not null)
			{
				var hit = HitTestArea(canvasHitPoint);
				if (hit is not null)
				{
					await EraseAreaAsync(_editor.SelectedFloorId.Value, hit);
				}
				return;
			}

			if (_viewModel.Canvas.ActiveTool == MapTool.Place
				&& ObjectPalette.ActiveObject is ObjectKind placedKind
				&& _editor.SelectedFloorId is not null)
			{
				var placeRaw = new PointF((float)e.Touches[0].X, (float)e.Touches[0].Y);
				var placeCanvas = _drawable.ScreenToCanvas(placeRaw);

				await PlaceObjectAsync(
					_editor.SelectedFloorId.Value,
					placedKind,
					new CanvasPoint(placeCanvas.X, placeCanvas.Y),
					_editor.GhostRotation,
					_drawable.GhostScale);
				return;
			}

			var canvasPoint = canvasHitPoint;

			if (_viewModel.Canvas.ActiveTool == MapTool.Move && _editor.SelectedFloorId is not null)
			{
				var hit = HitTestArea(canvasPoint);
				if (hit is not null)
				{
					_editor.BeginMoveDrag(
						hit.Id,
						TileCoordinateHelper.SnapToGrid(canvasPoint.X, canvasPoint.Y, _drawable.TileSize));
					_drawable.DragPreviewAreaId = hit.Id;
					_drawable.DragPreviewDelta = new TilePoint(0, 0);
					MapCanvas.Invalidate();
				}
				return;
			}

			if (_viewModel.Canvas.ActiveTool != MapTool.Draw || _editor.SelectedFloorId is null)
			{
				return;
			}

			var snapped = TileCoordinateHelper.SnapToGrid(canvasPoint.X, canvasPoint.Y, _drawable.TileSize);
			var result = _viewModel.Canvas.DrawingSession.PlaceVertex(snapped);

			if (result == DrawSessionResult.Added)
			{
				if (_editor.DrawState == DrawState.DrawingReady)
				{
					// The first vertex arms the Draw tool into Drawing. That is the same
					// transition MapEditorState makes when the Draw tool is asserted with a live
					// session, and it is the only frozen operation that produces the value — the
					// page must not compute a DrawState of its own. The tool is unchanged, so
					// _viewModel.Canvas.ActiveTool is deliberately not reassigned.
					_editor.SetActiveTool(MapTool.Draw, drawingSessionIsActive: true);
				}

				_drawable.SetInProgressVertices(_viewModel.Canvas.DrawingSession.Vertices);
				UpdateToolbarButtonState();
				MapCanvas.Invalidate();
			}
			else if (result == DrawSessionResult.Closed)
			{
				await FinishDrawing();
			}
		}
		catch (Exception exception)
		{
			_viewModel.ErrorMessage = $"Could not complete that map action: {exception.Message}";
		}
	}

	private void OnMapMoveHover(object? sender, TouchEventArgs e)
	{
		var point = e.Touches.FirstOrDefault();
		var canvasPoint = _drawable.ScreenToCanvas(new PointF(point.X, point.Y));
		var needsInvalidate = false;

		if (_viewModel.Canvas.ActiveTool == MapTool.Place && _drawable.GhostObjectKind is not null)
		{
			_drawable.GhostCanvasPosition = canvasPoint;
			MapCanvas.Invalidate();
			return;
		}

		if (_editor.DrawState == DrawState.Drawing)
		{
			_drawable.PhantomEnd = canvasPoint;
			needsInvalidate = true;
		}

		// Canvas hover highlighting — for Move, Select, and Erase tools
		if (_editor.SelectedFloorId is not null
			&& (_viewModel.Canvas.ActiveTool == MapTool.Move || _viewModel.Canvas.ActiveTool == MapTool.Select || _viewModel.Canvas.ActiveTool == MapTool.Erase))
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
		if (_viewModel.Canvas.ActiveTool == MapTool.Place || _viewModel.Canvas.ActiveTool == MapTool.Select)
		{
			var cp = _drawable.ScreenToCanvas(new PointF((float)e.Touches.FirstOrDefault().X, (float)e.Touches.FirstOrDefault().Y));
			var newHovered = GetObjectAtCanvasPoint(cp);
			if (newHovered != _editor.HoveredObjectId)
			{
				_editor.HoveredObjectId = newHovered;
				// Only update the drawable's hover if the popup isn't pinned to a selected object.
				if (_editor.SelectedObjectId is null)
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

		if (_viewModel.Canvas.ActiveTool == MapTool.Place)
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
		if (_editor.HoveredObjectId is not null)
		{
			_editor.HoveredObjectId = null;
			if (_editor.SelectedObjectId is null)
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
		if (_editor.PendingObjectClickId is not null
			&& (_viewModel.Canvas.ActiveTool == MapTool.Place || _viewModel.Canvas.ActiveTool == MapTool.Select))
		{
			var current = new PointF((float)e.Touches[0].X, (float)e.Touches[0].Y);
			// The distance test and the threshold live in MapEditorState; the page only reacts to
			// the promotion. The id is read before the call because the state machine keeps it.
			var draggingObjectId = _editor.PendingObjectClickId;
			var wasAlreadyConfirmed = _editor.IsObjectDragConfirmed;

			if (_editor.ConfirmObjectDragIfPastThreshold(new CanvasPoint(current.X, current.Y)))
			{
				if (!wasAlreadyConfirmed)
				{
					_longPressDetector.TouchEnded(); // cancel the long-press timer
					_editor.SelectedObjectId = null;
					ObjectActionPopup.IsVisible = false;
					_drawable.DraggingObjectId = draggingObjectId;
				}

				var canvasPoint = _drawable.ScreenToCanvas(current);

				_drawable.DraggingObjectTargetCanvas = canvasPoint;
				_editor.ObjectDragTargetCanvas = new CanvasPoint(canvasPoint.X, canvasPoint.Y);

				MapCanvas.Invalidate();
			}
			return;
		}

		if (_viewModel.Canvas.ActiveTool == MapTool.Pan && _editor.PanAnchorScreen is CanvasPoint anchor)
		{
			var current = new PointF((float)e.Touches[0].X, (float)e.Touches[0].Y);
			var delta = new PointF(current.X - anchor.X, current.Y - anchor.Y);
			_drawable.PanOffset = new PointF(
				_drawable.PanOffset.X + delta.X,
				_drawable.PanOffset.Y + delta.Y);
			_editor.PanAnchorScreen = new CanvasPoint(current.X, current.Y);
			MapCanvas.Invalidate();
			return;
		}

		if (_viewModel.Canvas.ActiveTool == MapTool.Move && _editor.IsMoveDragging
			&& _editor.MoveGrabTile is not null && _editor.DraggingAreaId is not null)
		{
			var point = e.Touches.FirstOrDefault();
			var canvasPoint = _drawable.ScreenToCanvas(new PointF(point.X, point.Y));
			var currentTile = TileCoordinateHelper.SnapToGrid(canvasPoint.X, canvasPoint.Y, _drawable.TileSize);
			_drawable.DragPreviewDelta = new TilePoint(
				currentTile.Col - _editor.MoveGrabTile.Col,
				currentTile.Row - _editor.MoveGrabTile.Row);
			MapCanvas.Invalidate();
			return;
		}

		if (_editor.DrawState == DrawState.Drawing)
		{
			var point = e.Touches.FirstOrDefault();
			var canvasPoint = _drawable.ScreenToCanvas(new PointF(point.X, point.Y));
			_drawable.PhantomEnd = canvasPoint;
			MapCanvas.Invalidate();
		}
	}

	private async void OnMapEndInteraction(object? sender, TouchEventArgs e)
	{
		// async void: an escaping exception is UNOBSERVED and is rethrown on the
		// synchronisation context, which takes the process down. The try opens before the
		// first await for that reason, not as defensive style.
		try
		{
			_longPressDetector.TouchEnded();

			// Resolve pending object click vs drag.
			if (_editor.PendingObjectClickId is not null)
			{
				// Everything the decision below needs is read BEFORE the press is released:
				// ClearObjectInteraction is the only way to release it, and it clears the whole
				// group in one step — which is also exactly what both branches want afterwards.
				var clickId = _editor.PendingObjectClickId.Value;
				var wasDrag = _editor.IsObjectDragConfirmed;
				var targetPos = _editor.ObjectDragTargetCanvas;
				var longPressHandled = _editor.ObjectLongPressHandled;
				var previouslySelectedId = _editor.SelectedObjectId;

				_editor.ClearObjectInteraction();

				if (wasDrag)
				{
					// Was a drag — commit the move to the dropped floor-absolute position.
					_drawable.DraggingObjectId = null;
					_drawable.DraggingObjectTargetCanvas = null;
					if (targetPos is not null)
					{
						await MoveObjectAsync(clickId, targetPos);
					}
					return;
				}

				// Was a click — show/toggle popup (unless long press already handled it).
				if (!longPressHandled)
				{
					if (previouslySelectedId == clickId && ObjectActionPopup.IsVisible)
					{
						_drawable.HoveredObjectId = null;
						ObjectActionPopup.IsVisible = false;
					}
					else
					{
						_editor.SelectedObjectId = clickId;
						_editor.HoveredObjectId = clickId;
						_drawable.HoveredObjectId = clickId;
						UpdateObjectPopup();
					}
					MapCanvas.Invalidate();
				}
				return;
			}

			if (_viewModel.Canvas.ActiveTool == MapTool.Pan)
			{
				_editor.PanAnchorScreen = null;
				SetCanvasCursor(MapTool.Pan, isDragging: false);
				return;
			}

			if (_viewModel.Canvas.ActiveTool == MapTool.Move && _editor.IsMoveDragging
				&& _editor.DraggingAreaId is not null && _editor.SelectedFloorId is not null)
			{
				var drag = _drawable.DragPreviewDelta ?? new TilePoint(0, 0);
				var areaId = _editor.DraggingAreaId.Value;
				var floorId = _editor.SelectedFloorId.Value;
				var area = _drawable.Floor?.Areas.FirstOrDefault(areaItem => areaItem.Id == areaId);

				_drawable.DragPreviewAreaId = null;
				_drawable.DragPreviewDelta = null;
				_editor.EndMoveDrag();

				if (area is not null && (drag.Col != 0 || drag.Row != 0))
				{
					// Keep visual preview via pending offset while the DB write completes
					_drawable.SetPendingMoveOffset(areaId, drag);
					MapCanvas.Invalidate();

					// The offset that gets persisted. Translating an outline is geometry, so it
					// lives in App.Core where it is tested against the value the command writes.
					var newOutline = AreaOutlineGeometry.Translate(area.Outline, drag);

					await _viewModel.Canvas.MoveAreaOutlineCommand.ExecuteAsync(
						new MoveAreaOutlineArgs(floorId, areaId, newOutline));

					// MASTER-166, same shape as FinishDrawing: the pending offset is the
					// optimistic visual here, so it survives until the reload has replaced it.
					await OptimisticGhostSequence.RefreshThenClearAsync(
						RefreshFloorAsync,
						_drawable.ClearPendingMoveOffsets);
				}

				UpdateToolbarButtonState();
				MapCanvas.Invalidate();
			}
		}
		catch (Exception exception)
		{
			_viewModel.ErrorMessage = $"Could not complete that map action: {exception.Message}";
		}
	}

	private async Task FinishDrawing()
	{
		var vertices = _viewModel.Canvas.DrawingSession.Vertices;
		if (vertices.Count < 3)
		{
			AbortInProgressDraw();
			return;
		}

		var outline = _viewModel.Canvas.DrawingSession.BuildOutline();

		// Show optimistic visual immediately, then auto-save and reload
		if (_editor.SelectedFloorId is not null)
		{
			_drawable.AddGhostArea(outline);
			MapCanvas.Invalidate();
			await _viewModel.Canvas.AddAreaWithOutlineCommand.ExecuteAsync(
				new AddAreaWithOutlineArgs(_editor.SelectedFloorId.Value, outline));
		}

		_viewModel.Canvas.DrawingSession.Reset();

		// A completed outline leaves the Draw tool armed for the next one, which is what
		// MapEditorState.ResetDrawState reports while a floor is selected.
		_editor.ResetDrawState(_editor.SelectedFloorId is not null);
		_drawable.ClearInProgressVertices();
		_drawable.PhantomEnd = null;

		// MASTER-166: the ghost is cleared only AFTER the reload that replaces it, so no
		// frame can show neither the ghost nor the persisted area.
		await OptimisticGhostSequence.RefreshThenClearAsync(
			RefreshFloorAsync,
			_drawable.ClearGhostAreas);

		// Restore the focus dim if an area was focused before the draw
		if (_editor.FocusedAreaId is not null)
		{
			_drawable.ActiveAreaId = _editor.FocusedAreaId;
			UpdateActiveAreaSidebarHighlight();
		}

		UpdateToolbarButtonState();
		MapCanvas.Invalidate();
	}

	private async void OnMapDoubleTapped(object? sender, TappedEventArgs e)
	{
		// async void: an escaping exception is UNOBSERVED and is rethrown on the
		// synchronisation context, which takes the process down. The try opens before the
		// first await for that reason, not as defensive style.
		try
		{
			if (_editor.DrawState == DrawState.Drawing && _viewModel.Canvas.DrawingSession.Vertices.Count >= 3)
			{
				_drawable.PhantomEnd = null;
				await FinishDrawing();
			}
		}
		catch (Exception exception)
		{
			_viewModel.ErrorMessage = $"Could not finish that area outline: {exception.Message}";
		}
	}

	private async void OnMapSecondaryTapped(object? sender, TappedEventArgs e)
	{
		// async void: an escaping exception is UNOBSERVED and is rethrown on the
		// synchronisation context, which takes the process down. The try opens before the
		// first await for that reason, not as defensive style.
		try
		{
			if (_viewModel.Canvas.ActiveTool == MapTool.Place)
			{
				if (ObjectPalette.ActiveObject is not null)
				{
					// Deselect the active palette item.
					ObjectPalette.ClearActiveObject();
				}
				else if (_editor.HoveredObjectId is not null)
				{
					// Right-click on a hovered placed object (inspect mode) deletes it.
					_editor.SelectedObjectId = _editor.HoveredObjectId;
					await DeleteSelectedObjectAsync();
				}
				return;
			}

			OnUndoClicked(sender, e);
		}
		catch (Exception exception)
		{
			_viewModel.ErrorMessage = $"Could not delete that object: {exception.Message}";
		}
	}

	// ─── Hit testing ────────────────────────────────────────────────────────────

	// The polygon construction, the ray-casting containment test, the shoelace area and the
	// smallest-wins tie-break all live in MapHitTester, where they are testable without a canvas.
	// The page's remaining job is to say WHICH areas to test and at what tile size, and to pass on
	// the drag preview so hit testing follows the dragged position.
	private AreaItem? HitTestArea(PointF pixelPoint)
	{
		if (_drawable.Floor?.Areas is not { } areas)
		{
			return null;
		}

		return MapHitTester.HitTestArea(
			areas,
			pixelPoint.X,
			pixelPoint.Y,
			_drawable.TileSize,
			_drawable.DragPreviewAreaId,
			_drawable.DragPreviewDelta);
	}

	// ─── Object popup helpers ───────────────────────────────────────────────────

	// Smallest-object-wins bounding-box hit testing, in MapHitTester for the same reason as
	// HitTestArea. ObjectKindInfo is passed as a delegate rather than called there: it lives in the
	// App layer, which App.Core cannot reference, and it is deliberately not moved this session.
	private Guid? GetObjectAtCanvasPoint(PointF canvasPoint)
	{
		if (_drawable.Floor?.Objects is not { } objects)
		{
			return null;
		}

		return MapHitTester
			.HitTestObject(
				objects,
				canvasPoint.X,
				canvasPoint.Y,
				_drawable.TileSize,
				ObjectKindInfo.GetObjectSize)
			?.Id;
	}

	private void OnObjectLongPressed(object? sender, EventArgs e)
	{
		if (_editor.LastTouchPoint is not CanvasPoint lastTouch)
		{
			return;
		}

		var canvasPoint = _drawable.ScreenToCanvas(new PointF(lastTouch.X, lastTouch.Y));
		var objectId = GetObjectAtCanvasPoint(canvasPoint);
		if (objectId is not null)
		{
			// The pending press is released first, because a long press has already decided this
			// is not a click; ClearObjectInteraction is what releases it.
			_editor.ClearObjectInteraction();
			_editor.ObjectLongPressHandled = true; // prevent EndInteraction from firing click logic again
			_editor.SelectedObjectId = objectId;
			_editor.HoveredObjectId = objectId;
			_drawable.HoveredObjectId = objectId;
			UpdateObjectPopup();
			MapCanvas.Invalidate();
		}
	}

	private void UpdateObjectPopup()
	{
		if (_editor.SelectedObjectId is null || _drawable.Floor is null)
		{
			_editor.PopupBounds = null;
			ObjectActionPopup.IsVisible = false;
			return;
		}

		var obj = _drawable.Floor.Objects.FirstOrDefault(placedObj => placedObj.Id == _editor.SelectedObjectId);

		if (obj is null)
		{
			_editor.PopupBounds = null;
			ObjectActionPopup.IsVisible = false;
			return;
		}

		// Position above the object center in screen space (5 x 32px buttons + 2 dividers + padding ~= 182x40)
		var screenX = obj.Position.X * _drawable.ZoomLevel + _drawable.PanOffset.X;
		var screenY = obj.Position.Y * _drawable.ZoomLevel + _drawable.PanOffset.Y - 48;

		const float PopupWidth = 182f;
		const float PopupHeight = 40f;

		// The hit-zone guard OnMapStartInteraction dispatches through. A CanvasRect rather than a
		// RectF because it is interaction state and lives on MapEditorState with the rest of it.
		_editor.PopupBounds = new CanvasRect(screenX, screenY, PopupWidth, PopupHeight);

		ObjectActionPopup.Margin = new Thickness(screenX, screenY, 0, 0);
		ObjectActionPopup.HorizontalOptions = LayoutOptions.Start;
		ObjectActionPopup.VerticalOptions = LayoutOptions.Start;
		ObjectActionPopup.IsVisible = true;
	}

	// Properties, not static readonly fields: see DmTheme's remarks and the guard test
	// NoEagerDmThemeRead.
	private static Color PopupBtnHoverGold =>
		DmTheme.PopupButtonHover;

	private static Color PopupBtnHoverRed =>
		DmTheme.PopupButtonDestructiveHover;

	private static Color PopupBtnPressGold =>
		DmTheme.PopupButtonPressed;

	private static Color PopupBtnPressRed =>
		DmTheme.PopupButtonDestructivePressed;

	// Not static: the guarded tap handler below routes a failure to _viewModel.ErrorMessage.
	private void AttachPopupButtonBehavior(Border btn, bool goldHover, Func<Task> action)
	{
		var hoverColor = goldHover ? PopupBtnHoverGold : PopupBtnHoverRed;
		var pressColor = goldHover ? PopupBtnPressGold : PopupBtnPressRed;
		var isHovered = false;

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
			// async void: an escaping exception is UNOBSERVED and is rethrown on the
			// synchronisation context, which takes the process down. The try opens before the
			// first await for that reason, not as defensive style.
			try
			{
				btn.BackgroundColor = pressColor;
				await action();
				btn.BackgroundColor = isHovered ? hoverColor : Colors.Transparent;
			}
			catch (Exception exception)
			{
				_viewModel.ErrorMessage = $"Could not complete that object action: {exception.Message}";
			}
		};
		btn.GestureRecognizers.Add(tap);

		btn.Behaviors.Add(new HandCursorBehavior());
	}

	private async Task RotateObjectAsync(bool clockwise)
	{
		if (_editor.SelectedObjectId is null || _editor.SelectedFloorId is null || _drawable.Floor is null)
		{
			return;
		}

		var obj = _drawable.Floor.Objects.FirstOrDefault(placedObj => placedObj.Id == _editor.SelectedObjectId);

		if (obj is null)
		{
			return;
		}

		var newRotation = clockwise
			? (ObjectRotation)(((int)obj.Rotation + 1) % 4)
			: (ObjectRotation)(((int)obj.Rotation + 3) % 4);

		await _viewModel.Canvas.UpdatePlacedObjectCommand.ExecuteAsync(
			new UpdatePlacedObjectArgs(
				_editor.SelectedFloorId.Value,
				_editor.SelectedObjectId.Value,
				obj.Position,
				newRotation,
				obj.Scale));

		await RefreshFloorAsync();
		// Re-pin popup to the refreshed object position.
		if (_editor.SelectedObjectId is not null)
		{
			UpdateObjectPopup();
		}

		MapCanvas.Invalidate();
	}

	private async Task ScaleObjectAsync(bool down)
	{
		if (_editor.SelectedObjectId is null || _editor.SelectedFloorId is null || _drawable.Floor is null)
		{
			return;
		}

		var obj = _drawable.Floor.Objects.FirstOrDefault(placedObj =>
			placedObj.Id == _editor.SelectedObjectId.Value);

		if (obj is null)
		{
			return;
		}

		var newScale = down
			? ObjectScale.StepDown(obj.Scale)
			: ObjectScale.StepUp(obj.Scale);

		await _viewModel.Canvas.UpdatePlacedObjectCommand.ExecuteAsync(
			new UpdatePlacedObjectArgs(
				_editor.SelectedFloorId.Value,
				_editor.SelectedObjectId.Value,
				obj.Position,
				obj.Rotation,
				newScale));

		await RefreshFloorAsync();
		if (_editor.SelectedObjectId is not null)
		{
			UpdateObjectPopup();
		}

		MapCanvas.Invalidate();
	}

	private async Task MoveObjectAsync(Guid objectId, CanvasPoint newPosition)
	{
		if (_editor.SelectedFloorId is null || _drawable.Floor is null)
		{
			return;
		}

		var obj = _drawable.Floor.Objects.FirstOrDefault(placedObj => placedObj.Id == objectId);

		if (obj is null || obj.Position == newPosition)
		{
			return;
		}

		_editor.HoveredObjectId = null;
		_drawable.HoveredObjectId = null;
		MapCanvas.Invalidate();

		await _viewModel.Canvas.UpdatePlacedObjectCommand.ExecuteAsync(
			new UpdatePlacedObjectArgs(
				_editor.SelectedFloorId.Value,
				objectId,
				newPosition,
				obj.Rotation,
				obj.Scale));

		await RefreshFloorAsync();
		MapCanvas.Invalidate();
	}

	private async Task DeleteSelectedObjectAsync()
	{
		if (_editor.SelectedObjectId is null || _editor.SelectedFloorId is null || _drawable.Floor is null)
		{
			return;
		}

		if (!_drawable.Floor.Objects.Any(placedObj => placedObj.Id == _editor.SelectedObjectId))
		{
			return;
		}

		var deleteId = _editor.SelectedObjectId.Value;
		_editor.SelectedObjectId = null;
		_editor.HoveredObjectId = null;
		_drawable.HoveredObjectId = null;
		ObjectActionPopup.IsVisible = false;
		MapCanvas.Invalidate();

		await _viewModel.Canvas.RemovePlacedObjectCommand.ExecuteAsync(
			new RemovePlacedObjectArgs(_editor.SelectedFloorId.Value, deleteId));

		await RefreshFloorAsync();
		MapCanvas.Invalidate();
	}

	// ─── Map refresh ────────────────────────────────────────────────────────────

	private void RefreshMap()
	{
		var floor = _viewModel.FloorList.Items.FirstOrDefault(floorItem => floorItem.Id == _editor.SelectedFloorId);
		_drawable.Floor = floor;

		// The focused area belongs to a floor we just left — drop the focus so nothing stays dimmed.
		if (_editor.FocusedAreaId is not null
			&& floor?.Areas.Any(areaItem => areaItem.Id == _editor.FocusedAreaId) != true)
		{
			ClearAreaFocus();
		}

		// Same reasoning one level up: the notebook may be open on an entity the reload removed.
		CloseNotebookIfOrphaned();

		if (_editor.DrawState == DrawState.Idle)
		{
			_drawable.ClearInProgressVertices();
			_drawable.PhantomEnd = null;
		}

		SetMapToolbarShown(_editor.SelectedFloorId is not null);

		if (_editor.SelectedFloorId is not null && _editor.DrawState == DrawState.Idle)
			SetActiveTool(MapTool.Pan);

		UpdateToolbarButtonState();
		MapCanvas.Invalidate();
	}

	/// <summary>
	///		Closes the notebook when the entity it is open on no longer exists after a reload,
	///		so it cannot keep offering to author something that has been deleted.
	/// </summary>
	private void CloseNotebookIfOrphaned()
	{
		if (Notebook.CurrentId is not Guid notebookId)
		{
			return;
		}

		var isLive =
			_viewModel.Dungeon?.Id == notebookId
			|| _viewModel.FloorList.Items.Any(floorItem =>
				floorItem.Id == notebookId
				|| floorItem.Areas.Any(areaItem => areaItem.Id == notebookId));

		if (!isLive)
		{
			Notebook.Close();
		}
	}

	// ─── Sidebar helpers ────────────────────────────────────────────────────────

	private void UpdateActiveAreaSidebarHighlight()
	{
		foreach (var child in FloorListContainer.Children)
		{
			if (child is not FloorSidebarItem floorItem || floorItem.Floor is not { } floor || floor.Id != _editor.SelectedFloorId)
				continue;

			foreach (var area in floor.Areas)
			{
				floorItem.SetAreaEditing(area.Id, area.Id == _editor.FocusedAreaId);
			}

			break;
		}
	}

	private void CollapseAllExcept(FloorSidebarItem except)
	{
		foreach (var child in FloorListContainer.Children)
		{
			if (child is FloorSidebarItem other && !ReferenceEquals(other, except))
			{
				other.IsExpanded = false;
			}
		}
	}

	// ─── The floor list as a drop target ────────────────────────────────────────

	/// <summary>
	///		Accepts a floor drag anywhere over the floor list, including the gaps between cards.
	/// </summary>
	/// <remarks>
	///		<c>Copy</c> is the only non-<c>None</c> member MAUI's <c>DataPackageOperation</c> has —
	///		<c>Move</c> does not exist there — and it is what both of the other two drop targets
	///		accept. Contract C1 section 4 records this as a refuted candidate: a task must not
	///		"fix" the reorder by reaching for <c>.Move</c>.
	/// </remarks>
	private void OnFloorListDragOver(object? sender, DragEventArgs e) =>
		e.AcceptedOperation = DataPackageOperation.Copy;

	/// <summary>
	///		Resolves a drop that missed every floor card to the card it meant, and reorders.
	/// </summary>
	/// <remarks>
	///		<para>
	///		ESCALATION E6. This is the end of the silent discard. <c>FloorListContainer</c> carried
	///		no drop target at all, so a release on the 4-DIU seam between two cards, or on any space
	///		no handler owned, was discarded with no handler, no error and no trace — and because the
	///		reorder itself works once the drop lands on a card, that silence is what made the whole
	///		feature read as broken.
	///		</para>
	///		<para>
	///		THE BANDS ARE BUILT BEFORE <see cref="ClearDragPlaceholder"/> RUNS, and the order is
	///		load-bearing: the placeholder is itself a drop target occupying a band, so a list built
	///		after clearing it would report the placeholder's strip as unowned space and resolve a
	///		drop the placeholder had already handled.
	///		</para>
	///		<para>
	///		async void: an escaping exception is UNOBSERVED and is rethrown on the synchronisation
	///		context, which takes the process down. The try opens before the first await for that
	///		reason, not as defensive style — the same shape as the other two drop handlers.
	///		</para>
	/// </remarks>
	private async void OnFloorListDrop(object? sender, DropEventArgs e)
	{
		try
		{
			var position = e.GetPosition(FloorListContainer);

			if (position is not Point point)
			{
				return;
			}

			var bands = new List<FloorListBand>();
			var cards = new List<FloorSidebarItem?>();

			foreach (var child in FloorListContainer.Children)
			{
				if (child is not View view)
				{
					continue;
				}

				var item = view as FloorSidebarItem;
				var target = DropTargetWithin(view);

				bands.Add(new FloorListBand(
					item is null ? FloorListBandKind.TransientDropTarget : FloorListBandKind.FloorCard,
					view.Y + (ReferenceEquals(target, view) ? 0 : target.Y),
					target.Height));

				cards.Add(item);
			}

			var resolution = FloorReorderNearest.Resolve(bands, point.Y);

			// ExistingTarget means the release landed inside a card or the placeholder, both of
			// which carry a handler of their own. Declining here is what keeps the reorder running
			// exactly once regardless of whether the platform bubbles the drop up to this handler.
			if (resolution.Owner != FloorDropOwner.FloorList)
			{
				return;
			}

			var nearest = cards[resolution.BandIndex];

			ClearDragPlaceholder();

			// The same decision the other two drop targets make, from the same owner, so the three
			// cannot drift apart. e.Data is null-conditional because MAUI hands these args a
			// nullable package; see FloorReorderDrop's remarks.
			var decision = FloorReorderDrop.Decide(
				e.Data?.Properties,
				nearest?.Floor?.Id,
				nearest?.Floor?.Number ?? 0);

			if (!decision.ShouldReorder)
			{
				return;
			}

			_editor.SelectedFloorId = decision.DraggedFloorId;
			await _viewModel.FloorList.ReorderFloorCommand.ExecuteAsync(
				new ReorderFloorArgs(decision.DraggedFloorId, decision.TargetFloorNumber));
		}
		catch (Exception exception)
		{
			_viewModel.ErrorMessage = $"Could not reorder the floors: {exception.Message}";
		}
	}

	/// <summary>
	///		The element inside <paramref name="child"/> that actually carries a drop target, or
	///		<paramref name="child"/> itself when none does.
	/// </summary>
	/// <remarks>
	///		A floor card is a <c>ContentView</c> 58 DIU tall whose root <c>Border</c> — the element
	///		carrying the <c>DropGestureRecognizer</c> — is 54, the remaining 4 being that border's
	///		bottom margin. THOSE 4 DIU ARE THE SEAM THIS WHOLE FIX IS ABOUT, so measuring the band
	///		from the child's 58 would swallow the gap and report it as a hit on the card. Reading
	///		the recogniser's own host rather than subtracting a hard-coded 4 keeps the two facts
	///		from drifting if the card's margin ever changes.
	/// </remarks>
	private static View DropTargetWithin(View child)
	{
		if (child.GestureRecognizers.OfType<DropGestureRecognizer>().Any())
		{
			return child;
		}

		if (child is ContentView content
			&& content.Content is View inner
			&& inner.GestureRecognizers.OfType<DropGestureRecognizer>().Any())
		{
			return inner;
		}

		return child;
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
			// async void: an escaping exception is UNOBSERVED and is rethrown on the
			// synchronisation context, which takes the process down. The try opens before the
			// first await for that reason, not as defensive style.
			try
			{
				ClearDragPlaceholder();

				// The same decision the floor card's own drop target makes, from the same owner, so
				// the two targets cannot drift apart. dropArgs.Data is null-conditional because
				// MAUI hands these args a nullable package; see FloorReorderDrop's remarks.
				var decision = FloorReorderDrop.Decide(
					dropArgs.Data?.Properties,
					capturedTarget.Floor?.Id,
					capturedTarget.Floor?.Number ?? 0);

				if (!decision.ShouldReorder)
				{
					return;
				}

				_editor.SelectedFloorId = decision.DraggedFloorId;
				await _viewModel.FloorList.ReorderFloorCommand.ExecuteAsync(
					new ReorderFloorArgs(decision.DraggedFloorId, decision.TargetFloorNumber));
			}
			catch (Exception exception)
			{
				_viewModel.ErrorMessage = $"Could not reorder the floors: {exception.Message}";
			}
		};

		_dragPlaceholder = new Border
		{
			HeightRequest = 44,
			Stroke = new SolidColorBrush(DmTheme.DragPlaceholderStroke),
			StrokeThickness = 2,
			StrokeShape = new RoundRectangle { CornerRadius = 3 },
			BackgroundColor = DmTheme.DragPlaceholderFill,
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

	private async void OnAddFloorClicked(object? sender, EventArgs e)
	{
		// async void: an escaping exception is UNOBSERVED and is rethrown on the
		// synchronisation context, which takes the process down. The try opens before the
		// first await for that reason, not as defensive style.
		try
		{
			await _viewModel.FloorList.AddFloorCommand.ExecuteAsync(null);

			Dispatcher.DispatchDelayed(
				TimeSpan.FromMilliseconds(80),
				() =>
				{
					if (FloorListContainer.Children.LastOrDefault() is FloorSidebarItem lastItem)
					{
						lastItem.FocusNameEditor();
					}
				});
		}
		catch (Exception exception)
		{
			_viewModel.ErrorMessage = $"Could not add a floor: {exception.Message}";
		}
	}

}
