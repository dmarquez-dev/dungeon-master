using DungeonMaster.Application.Features.Dungeons.Queries.GetDungeonDetail;

namespace DungeonMaster.App.Controls;

/// <summary>
///		A sidebar list item that represents a dungeon floor, supporting inline name editing, area management,
///		expand/collapse toggling, and drag-and-drop reordering.
/// </summary>
public partial class FloorSidebarItem : ContentView
{
	private static readonly Color _surfaceNormal = Color.FromArgb("#2E8B5E1A"); // DMCardSurface
	private static readonly Color _surfaceHover  = Color.FromArgb("#528B5E1A"); // DMCardSurfaceHover
	private static readonly Color _surfaceActive  = Color.FromArgb("#6A8B5E1A"); // slightly more opaque for expanded state

	private static readonly SolidColorBrush _borderNormal = new(Color.FromArgb("#4DC4862A")); // DMCardBorder
	private static readonly SolidColorBrush _borderActive = new(Color.FromArgb("#A6C4862A")); // DMCardBorderHover

	/// <summary>
	///		Identifies the <see cref="Floor"/> bindable property.
	/// </summary>
	public static readonly BindableProperty FloorProperty =
		BindableProperty.Create(
			nameof(Floor),
			typeof(FloorDto),
			typeof(FloorSidebarItem),
			null,
			propertyChanged: (bindable, _, newValue) =>
				((FloorSidebarItem)bindable).Populate((FloorDto?)newValue));

	/// <summary>
	///		Raised when the user activates this floor item, signalling the page to deselect all others.
	/// </summary>
	public event EventHandler? SelectRequested;

	/// <summary>
	///		Raised when a drop completes and a floor reorder is required.
	/// </summary>
	public event EventHandler<(Guid DraggedFloorId, int TargetNumber)>? ReorderRequested;

	/// <summary>
	///		Raised when a drag operation begins on this item.
	/// </summary>
	public event EventHandler? DragStarted;

	/// <summary>
	///		Raised when a drag operation ends on this item.
	/// </summary>
	public event EventHandler? DragEnded;

	/// <summary>
	///		Raised when a dragged item enters this item's drop zone, requesting a placeholder insertion.
	/// </summary>
	public event EventHandler? PlaceholderRequested;

	/// <summary>
	///		Raised when the drag placeholder should be removed.
	/// </summary>
	public event EventHandler? PlaceholderCleared;

	/// <summary>
	///		Raised when the user requests deletion of this floor.
	/// </summary>
	public event EventHandler<FloorDto>? DeleteRequested;

	/// <summary>
	///		Raised when the user commits a new name for this floor.
	/// </summary>
	public event EventHandler<string?>? NameCommitted;

	/// <summary>
	///		Raised when the user requests a new area to be added to this floor.
	/// </summary>
	public event EventHandler? AddAreaRequested;

	/// <summary>
	///		Raised when the user commits a new name for an area on this floor.
	/// </summary>
	public event EventHandler<(AreaDto Area, string Name)>? AreaNameCommitted;

	/// <summary>
	///		Raised when the user requests deletion of an area on this floor.
	/// </summary>
	public event EventHandler<AreaDto>? AreaDeleteRequested;

	private bool _isExpanded;
	private bool _suppressCardTap;

	/// <summary>
	///		Gets or sets the floor data displayed and edited by this item.
	/// </summary>
	public FloorDto? Floor
	{
		get => (FloorDto?)GetValue(FloorProperty);
		set => SetValue(FloorProperty, value);
	}

	/// <summary>
	///		Initializes a new instance of <see cref="FloorSidebarItem"/>, loads the XAML component tree,
	///		and subscribes to the floor name editor's label-tapped event.
	/// </summary>
	public FloorSidebarItem()
	{
		InitializeComponent();
		FloorNameEditor.LabelTapped += OnFloorNameEditorLabelTapped;
	}

	private void Populate(FloorDto? floor)
	{
		if (floor is null)
		{
			return;
		}

		FloorNumberLabel.Text = $"Floor {floor.Number} -";
		FloorNameEditor.Text = floor.Name ?? string.Empty;
		FloorNameEditor.Placeholder = "name";

		RebuildAreas(floor);
	}

	private void RebuildAreas(FloorDto floor)
	{
		AreaContainer.Children.Clear();

		foreach (var area in floor.Areas)
		{
			var item = new AreaSidebarItem { Area = area };
			item.DeleteRequested += (_, deletedArea) =>
				AreaDeleteRequested?.Invoke(this, deletedArea);
			item.NameCommitted += (_, name) =>
				AreaNameCommitted?.Invoke(this, (area, name));
			AreaContainer.Children.Add(item);
		}
	}

	/// <summary>
	///		Auto-expands this item and focuses the floor name editor.
	///		Called after a new floor is added to the list.
	/// </summary>
	public void FocusNameEditor()
	{
		if (!_isExpanded)
		{
			SetExpanded(true);
		}

		SelectRequested?.Invoke(this, EventArgs.Empty);
		FloorNameEditor.ActivateEditor();
	}

	/// <summary>
	///		Collapses this item without firing <see cref="SelectRequested"/>.
	///		Called by the page to enforce single-selection behaviour.
	/// </summary>
	public void Deselect() =>
		SetExpanded(false);

	/// <summary>
	///		Restores this item to the expanded state after a list rebuild.
	/// </summary>
	public void Select() =>
		SetExpanded(true);

	/// <summary>
	///		Commits any in-progress edits in the floor name editor and all child area editors.
	/// </summary>
	public void TryCommitActiveEditor()
	{
		FloorNameEditor.TryCommit();

		foreach (var child in AreaContainer.Children)
		{
			if (child is AreaSidebarItem area)
			{
				area.TryCommitActiveEditor();
			}
		}
	}

	private void SetExpanded(bool expanded)
	{
		_isExpanded = expanded;
		ExpandToggle.Text = expanded ? "▾" : "▸";
		AreaContainer.IsVisible = expanded;
		AddAreaButton.IsVisible = expanded;

		FloorBorder.BackgroundColor = expanded ? _surfaceActive : _surfaceNormal;
		FloorBorder.Stroke = expanded ? _borderActive : _borderNormal;
	}

	// Card tap only expands — collapsing happens via single-select (clicking another floor).
	// Delete and drag handle suppress this to avoid unexpected expansion on hover-revealed controls.
	private void OnCardTapped(object? sender, TappedEventArgs e)
	{
		if (_suppressCardTap)
		{
			_suppressCardTap = false;
			return;
		}

		if (!_isExpanded)
		{
			SetExpanded(true);
			SelectRequested?.Invoke(this, EventArgs.Empty);
		}
	}

	private void OnDragHandleTapped(object? sender, TappedEventArgs e) =>
		_suppressCardTap = true;

	private void OnFloorNameEditorLabelTapped(object? sender, EventArgs e)
	{
		if (!_isExpanded)
		{
			SetExpanded(true);
			SelectRequested?.Invoke(this, EventArgs.Empty);
		}
	}

	private void OnPointerEntered(object? sender, PointerEventArgs e)
	{
		if (!_isExpanded)
		{
			FloorBorder.BackgroundColor = _surfaceHover;
		}

		DeleteButton.IsVisible = true;
		DragHandle.IsVisible = true;
	}

	private void OnPointerExited(object? sender, PointerEventArgs e)
	{
		if (!_isExpanded)
		{
			FloorBorder.BackgroundColor = _surfaceNormal;
		}

		DeleteButton.IsVisible = false;
		DragHandle.IsVisible = false;
	}

	private void OnDeleteClicked(object? sender, EventArgs e)
	{
		_suppressCardTap = true;

		if (Floor is not null)
		{
			DeleteRequested?.Invoke(this, Floor);
		}
	}

	private void OnFloorNameCommitted(object? sender, string? name) =>
		NameCommitted?.Invoke(this, name);

	private void OnAddAreaClicked(object? sender, EventArgs e) =>
		AddAreaRequested?.Invoke(this, EventArgs.Empty);

	private void OnDragStarting(object? sender, DragStartingEventArgs e)
	{
		if (Floor is not null)
		{
			e.Data.Properties["FloorId"] = Floor.Id.ToString();
			DragStarted?.Invoke(this, EventArgs.Empty);
		}
	}

	private void OnDropCompleted(object? sender, DropCompletedEventArgs e) =>
		DragEnded?.Invoke(this, EventArgs.Empty);

	private void OnDragOver(object? sender, DragEventArgs e)
	{
		e.AcceptedOperation = DataPackageOperation.Copy;
		PlaceholderRequested?.Invoke(this, EventArgs.Empty);
		FloorBorder.Stroke = _borderActive;
	}

	private void OnDragLeave(object? sender, DragEventArgs e)
	{
		// Do NOT clear the placeholder here. Removing it while the cursor moves
		// into the adjacent placeholder triggers DragOver → DragLeave oscillation = stutter.
		// The placeholder is cleared when the drag ends (DropCompleted).
		FloorBorder.Stroke = _isExpanded ? _borderActive : _borderNormal;
	}

	private void OnDrop(object? sender, DropEventArgs e)
	{
		PlaceholderCleared?.Invoke(this, EventArgs.Empty);
		FloorBorder.Stroke = _isExpanded ? _borderActive : _borderNormal;

		if (Floor is null)
		{
			return;
		}

		if (!e.Data.Properties.TryGetValue("FloorId", out var obj))
		{
			return;
		}

		if (obj is not string idStr || !Guid.TryParse(idStr, out var draggedId))
		{
			return;
		}

		if (draggedId == Floor.Id)
		{
			return;
		}

		ReorderRequested?.Invoke(this, (draggedId, Floor.Number));
	}
}
