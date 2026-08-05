using DungeonMaster.Application.Features.Dungeons.Queries.GetDungeonDetail;
using DungeonMaster.App.Theme;

namespace DungeonMaster.App.Controls;

/// <summary>
///		A sidebar list item that represents a dungeon floor, supporting inline name editing, area management,
///		expand/collapse toggling, and drag-and-drop reordering.
/// </summary>
public partial class FloorSidebarItem : ContentView
{
	private static readonly Color _surfaceNormal = DmColors.FloorSurfaceNormal;
	private static readonly Color _surfaceHover  = DmColors.FloorSurfaceHover;
	private static readonly Color _surfaceActive  = DmColors.FloorSurfaceActive;

	private static readonly SolidColorBrush _borderNormal = DmColors.FloorBorderNormal;
	private static readonly SolidColorBrush _borderActive = DmColors.FloorBorderActive;

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
	///		Raised when the user requests the notebook for this floor.
	/// </summary>
	public event EventHandler<FloorDto>? NotebookRequested;

	/// <summary>
	///		Raised when the user requests a new area to be added to this floor.
	/// </summary>
	public event EventHandler? AddAreaRequested;

	/// <summary>
	///		Raised when the user taps an area item to enter area edit context.
	/// </summary>
	public event EventHandler<AreaDto>? AreaSelectRequested;

	/// <summary>
	///     Raised when the pointer enters an area item in this floor.
	/// </summary>
	public event EventHandler<AreaDto>? AreaHoverStarted;

	/// <summary>
	///     Raised when the pointer exits an area item in this floor.
	/// </summary>
	public event EventHandler<AreaDto>? AreaHoverEnded;

	/// <summary>
	///     Raised when the user requests deletion of an area in this floor.
	/// </summary>
	public event EventHandler<AreaDto>? AreaDeleteRequested;

	/// <summary>
	///     Raised when the user requests the notebook for an area in this floor.
	/// </summary>
	public event EventHandler<AreaDto>? AreaNotebookRequested;

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
	///		Initializes a new instance of <see cref="FloorSidebarItem"/> and loads the XAML component tree.
	/// </summary>
	public FloorSidebarItem()
	{
		InitializeComponent();
	}

	private void Populate(FloorDto? floor)
	{
		if (floor is null)
		{
			return;
		}

		FloorNumberLabel.Text = $"Floor {floor.Number} -";
		FloorNameLabel.Text = floor.Name ?? string.Empty;

		var hasContent =
			!string.IsNullOrWhiteSpace(floor.Description) ||
			!string.IsNullOrWhiteSpace(floor.Notes);

		NotebookButton.TextColor = hasContent
			? DmColors.ScrollGoldLight
			: DmColors.InkMuted;

		RebuildAreas(floor);
	}

	private void RebuildAreas(FloorDto floor)
	{
		AreaContainer.Children.Clear();

		foreach (var area in floor.Areas)
		{
			var item = new AreaSidebarItem { Area = area };
			item.SelectRequested += (_, selectedArea) =>
				AreaSelectRequested?.Invoke(this, selectedArea);
			item.HoverStarted += (_, hoveredArea) =>
				AreaHoverStarted?.Invoke(this, hoveredArea);
			item.HoverEnded += (_, hoveredArea) =>
				AreaHoverEnded?.Invoke(this, hoveredArea);
			item.DeleteRequested += (_, areaToDelete) =>
				AreaDeleteRequested?.Invoke(this, areaToDelete);
			item.NotebookRequested += (_, areaForNotebook) =>
				AreaNotebookRequested?.Invoke(this, areaForNotebook);
			AreaContainer.Children.Add(item);
		}
	}

	/// <summary>
	///		Auto-expands this item and fires <see cref="SelectRequested"/>.
	///		Called after a new floor is added, so the new row is open and selected; naming it
	///		happens in the notebook, which the user opens from the row.
	/// </summary>
	public void FocusNameEditor()
	{
		if (!_isExpanded)
		{
			SetExpanded(true);
		}

		SelectRequested?.Invoke(this, EventArgs.Empty);
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
	///		No-op — floor and area name editing moved to the notebook.
	/// </summary>
	public void TryCommitActiveEditor() { }

	/// <summary>
	///		Sets the editing visual state for a child area item by ID.
	/// </summary>
	public void SetAreaEditing(Guid areaId, bool editing)
	{
		foreach (var child in AreaContainer.Children)
		{
			if (child is AreaSidebarItem item && item.Area?.Id == areaId)
			{
				item.SetEditing(editing);
				return;
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

	// Card tap expands when collapsed, or fires SelectRequested when already expanded.
	// The page uses SelectRequested to exit area context when the floor card is re-tapped.
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
		}
		SelectRequested?.Invoke(this, EventArgs.Empty);
	}

	private void OnDragHandleTapped(object? sender, TappedEventArgs e) =>
		_suppressCardTap = true;

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

	private void OnNotebookClicked(object? sender, EventArgs e)
	{
		_suppressCardTap = true;

		if (Floor is not null)
		{
			NotebookRequested?.Invoke(this, Floor);
		}
	}

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
