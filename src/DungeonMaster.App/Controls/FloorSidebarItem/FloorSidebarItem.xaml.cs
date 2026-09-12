using System.Windows.Input;
using DungeonMaster.App.Core.Editor;
using DungeonMaster.App.Core.Presentation;
using DungeonMaster.App.Theme;

namespace DungeonMaster.App.Controls;

/// <summary>
///		A sidebar list item that represents a dungeon floor, supporting inline name editing, area management,
///		expand/collapse toggling, and drag-and-drop reordering.
/// </summary>
public partial class FloorSidebarItem : ContentView
{
	// The visual states declared on FloorBorder in FloorSidebarItem.xaml. Named once here so a
	// rename in the markup breaks the build rather than silently going to a state that does not
	// exist — VisualStateManager.GoToState returns false for an unknown name and paints nothing.
	private const string NormalState = "Normal";
	private const string HoveredState = "Hovered";
	private const string ExpandedState = "Expanded";
	private const string DragOverState = "DragOver";

	/// <summary>
	///		Identifies the <see cref="Floor"/> bindable property.
	/// </summary>
	public static readonly BindableProperty FloorProperty =
		BindableProperty.Create(
			nameof(Floor),
			typeof(FloorItem),
			typeof(FloorSidebarItem),
			null,
			propertyChanged: (bindable, _, newValue) =>
				((FloorSidebarItem)bindable).Populate((FloorItem?)newValue));

	/// <summary>
	///		Identifies the <see cref="IsExpanded"/> bindable property.
	///		This replaces the private <c>_isExpanded</c> field the appearance used to hang off
	///		(MASTER-098): a bindable property is a valid binding target, is settable through a style,
	///		and is monitorable for change, which is what lets the FloorStates group in the XAML own
	///		the expanded appearance and the three IsVisible bindings own the disclosure.
	/// </summary>
	public static readonly BindableProperty IsExpandedProperty =
		BindableProperty.Create(
			nameof(IsExpanded),
			typeof(bool),
			typeof(FloorSidebarItem),
			false,
			propertyChanged: (bindable, _, _) =>
				((FloorSidebarItem)bindable).ApplyExpanded());

	/// <summary>
	///		Identifies the <see cref="AreaSelectCommand"/> bindable property.
	/// </summary>
	public static readonly BindableProperty AreaSelectCommandProperty =
		BindableProperty.Create(
			nameof(AreaSelectCommand),
			typeof(ICommand),
			typeof(FloorSidebarItem),
			null);

	/// <summary>
	///		Identifies the <see cref="AreaHoverStartedCommand"/> bindable property.
	/// </summary>
	public static readonly BindableProperty AreaHoverStartedCommandProperty =
		BindableProperty.Create(
			nameof(AreaHoverStartedCommand),
			typeof(ICommand),
			typeof(FloorSidebarItem),
			null);

	/// <summary>
	///		Identifies the <see cref="AreaHoverEndedCommand"/> bindable property.
	/// </summary>
	public static readonly BindableProperty AreaHoverEndedCommandProperty =
		BindableProperty.Create(
			nameof(AreaHoverEndedCommand),
			typeof(ICommand),
			typeof(FloorSidebarItem),
			null);

	/// <summary>
	///		Identifies the <see cref="AreaDeleteCommand"/> bindable property.
	/// </summary>
	public static readonly BindableProperty AreaDeleteCommandProperty =
		BindableProperty.Create(
			nameof(AreaDeleteCommand),
			typeof(ICommand),
			typeof(FloorSidebarItem),
			null);

	/// <summary>
	///		Identifies the <see cref="AreaNotebookCommand"/> bindable property.
	/// </summary>
	public static readonly BindableProperty AreaNotebookCommandProperty =
		BindableProperty.Create(
			nameof(AreaNotebookCommand),
			typeof(ICommand),
			typeof(FloorSidebarItem),
			null);

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
	public event EventHandler<FloorItem>? DeleteRequested;

	/// <summary>
	///		Raised when the user requests the notebook for this floor.
	/// </summary>
	public event EventHandler<FloorItem>? NotebookRequested;

	/// <summary>
	///		Raised when the user requests a new area to be added to this floor.
	/// </summary>
	public event EventHandler? AddAreaRequested;

	private bool _suppressCardTap;
	private bool _isHovered;
	private bool _isDragOver;

	/// <summary>
	///		Gets or sets the floor data displayed and edited by this item.
	/// </summary>
	public FloorItem? Floor
	{
		get => (FloorItem?)GetValue(FloorProperty);
		set => SetValue(FloorProperty, value);
	}

	/// <summary>
	///		Gets or sets a value indicating whether this floor's area list is shown.
	///		Expanding is the only route to a floor's areas, so this is also what the page means by
	///		selecting a floor.
	/// </summary>
	public bool IsExpanded
	{
		get => (bool)GetValue(IsExpandedProperty);
		set => SetValue(IsExpandedProperty, value);
	}

	/// <summary>
	///		Gets or sets the command each area row invokes with its own area when the user taps it.
	///		These five commands are what replaced the five Area* events this control used to declare
	///		(MASTER-099). Each was a pass-through republication of an identically-shaped event on
	///		<see cref="AreaSidebarItem"/>, re-wired as five closures per child on every
	///		<see cref="Floor"/> assignment. The area rows now read these straight off this control
	///		through the item template, so nothing here forwards anything.
	/// </summary>
	public ICommand? AreaSelectCommand
	{
		get => (ICommand?)GetValue(AreaSelectCommandProperty);
		set => SetValue(AreaSelectCommandProperty, value);
	}

	/// <summary>
	///		Gets or sets the command each area row invokes with its own area when the pointer enters it.
	/// </summary>
	public ICommand? AreaHoverStartedCommand
	{
		get => (ICommand?)GetValue(AreaHoverStartedCommandProperty);
		set => SetValue(AreaHoverStartedCommandProperty, value);
	}

	/// <summary>
	///		Gets or sets the command each area row invokes with its own area when the pointer exits it.
	/// </summary>
	public ICommand? AreaHoverEndedCommand
	{
		get => (ICommand?)GetValue(AreaHoverEndedCommandProperty);
		set => SetValue(AreaHoverEndedCommandProperty, value);
	}

	/// <summary>
	///		Gets or sets the command each area row invokes with its own area when the user requests its deletion.
	/// </summary>
	public ICommand? AreaDeleteCommand
	{
		get => (ICommand?)GetValue(AreaDeleteCommandProperty);
		set => SetValue(AreaDeleteCommandProperty, value);
	}

	/// <summary>
	///		Gets or sets the command each area row invokes with its own area when the user requests its notebook.
	/// </summary>
	public ICommand? AreaNotebookCommand
	{
		get => (ICommand?)GetValue(AreaNotebookCommandProperty);
		set => SetValue(AreaNotebookCommandProperty, value);
	}

	/// <summary>
	///		Initializes a new instance of <see cref="FloorSidebarItem"/> and loads the XAML component tree.
	/// </summary>
	public FloorSidebarItem()
	{
		InitializeComponent();

		// Enter the resting state explicitly. The attributes on FloorBorder already paint it, but
		// entering Normal here is what gives the group a state to leave, so the first transition
		// out of it restores these values rather than whatever it last saw.
		ApplyExpanded();
	}

	private void Populate(FloorItem? floor)
	{
		if (floor is null)
		{
			return;
		}

		if (string.IsNullOrWhiteSpace(floor.Name))
		{
			FloorNameLabel.Text = $"Floor {floor.Number}";
			FloorNameLabel.TextColor = DmTheme.GoldMuted;
		}
		else
		{
			FloorNameLabel.Text = floor.Name;
			FloorNameLabel.TextColor = DmTheme.ScrollGoldLight;
		}

		// Colour AND description together — see NotebookAffordance for why the pair is one call
		// and not two copies.
		NotebookAffordance.Apply(
			NotebookButton,
			floor.HasNotebookContent);
	}

	/// <summary>
	///		Expands this item and fires <see cref="SelectRequested"/>.
	///		Called after a new floor is added, so the new row is open and selected; naming it
	///		happens in the notebook, which the user opens from the row.
	/// </summary>
	public void FocusNameEditor()
	{
		IsExpanded = true;
		SelectRequested?.Invoke(this, EventArgs.Empty);
	}

	/// <summary>
	///		Sets the selected visual state for a child area row by ID.
	///		A thin wrapper over the child's own <see cref="AreaSidebarItem.IsSelected"/>: the rows are
	///		generated by the area list's item template, so the page has no handle on them and this
	///		control is the only thing that can reach one by area ID.
	/// </summary>
	public void SetAreaEditing(Guid areaId, bool editing)
	{
		foreach (var child in AreaContainer.Children)
		{
			if (child is AreaSidebarItem item && item.Area?.Id == areaId)
			{
				item.IsSelected = editing;
				return;
			}
		}
	}

	// The glyph IS the expanded state, so the description moves with it; both are CONTENT rather
	// than appearance, which is why they stay here while the colours live in the XAML's
	// FloorStates group. The three IsVisible toggles this method used to perform are bindings on
	// IsExpanded now.
	private void ApplyExpanded()
	{
		var expanded = IsExpanded;

		ExpandToggle.Text = expanded ? "▾" : "▸";

		SemanticProperties.SetDescription(
			ExpandToggle,
			expanded
				? "Expanded"
				: "Collapsed");

		UpdateVisualState();
	}

	/// <summary>
	///		Selects the visual state that the current expanded, drag-over and hover flags together
	///		mean. This method is the ONE owner of that decision: the states themselves say what each
	///		looks like, and nothing else in this control writes a colour.
	/// </summary>
	private void UpdateVisualState() =>
		VisualStateManager.GoToState(
			FloorBorder,
			IsExpanded
				? ExpandedState
				: _isDragOver
					? DragOverState
					: _isHovered
						? HoveredState
						: NormalState);

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

		IsExpanded = true;
		SelectRequested?.Invoke(this, EventArgs.Empty);
	}

	private void OnDragHandleTapped(object? sender, TappedEventArgs e) =>
		_suppressCardTap = true;

	private void OnPointerEntered(object? sender, PointerEventArgs e)
	{
		_isHovered = true;
		UpdateVisualState();
	}

	private void OnPointerExited(object? sender, PointerEventArgs e)
	{
		_isHovered = false;
		UpdateVisualState();
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

		_isDragOver = true;
		UpdateVisualState();
	}

	private void OnDragLeave(object? sender, DragEventArgs e)
	{
		// Do NOT clear the placeholder here. Removing it while the cursor moves
		// into the adjacent placeholder triggers DragOver then DragLeave oscillation = stutter.
		// The placeholder is cleared when the drag ends (DropCompleted).
		_isDragOver = false;
		UpdateVisualState();
	}

	private void OnDrop(object? sender, DropEventArgs e)
	{
		PlaceholderCleared?.Invoke(this, EventArgs.Empty);

		_isDragOver = false;
		UpdateVisualState();

		// The four silent returns of contract C1 section 1 live in FloorReorderDrop.Decide, which
		// the page's placeholder drop target calls too, so the two drop targets cannot disagree.
		//
		// e.Data is reached through a null-conditional because MAUI's Windows gesture manager
		// builds these args from a NULLABLE package (GesturePlatformManager.Windows.cs, HandleDrop,
		// `datapackage?.View`). At Microsoft.Maui.Controls 10.0.41 DropEventArgs' internal
		// constructor substitutes `new DataPackageView(new DataPackage())` for a null, so the guard
		// is defence against a contract, not against a path reachable today - and the substitution
		// is worth knowing about for its own sake: when the platform cannot recover the source
		// package it hands this handler an EMPTY one, which is indistinguishable here from a drop
		// that carried no FloorId, and is therefore a silent return by design.
		var decision = FloorReorderDrop.Decide(e.Data?.Properties, Floor?.Id, Floor?.Number ?? 0);

		if (!decision.ShouldReorder)
		{
			return;
		}

		ReorderRequested?.Invoke(this, (decision.DraggedFloorId, decision.TargetFloorNumber));
	}
}
