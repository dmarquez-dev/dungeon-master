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
	///		Identifies the <see cref="FocusedAreaId"/> bindable property.
	/// </summary>
	public static readonly BindableProperty FocusedAreaIdProperty =
		BindableProperty.Create(
			nameof(FocusedAreaId),
			typeof(Guid?),
			typeof(FloorSidebarItem),
			null);

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
	///		Raised on every drag-over of this item's drop zone, carrying the platform's drag
	///		arguments so the page can read the pointer position and the dragged floor's payload.
	/// </summary>
	public event EventHandler<DragEventArgs>? DragOverReceived;

	/// <summary>
	///		Raised first thing on every drop on this item, before the reorder is decided and
	///		whatever the decision is, so the page can end the drag's visual state.
	/// </summary>
	public event EventHandler? DropReceived;

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
	///		Gets a value indicating whether any focusable descendant of this floor card currently
	///		holds keyboard focus — the chevron, the notebook and delete buttons, the name entry, or
	///		anything in the area list below them.
	///		<para>
	///		Walked on every read off each descendant's own IsFocused, so it cannot go stale against
	///		them. A stored copy would be two owners of one fact — the defect MASTER-098 took out of
	///		this file — and the frontend discipline requires derived values to be computed, never
	///		stored. DmCard tracks the same fact with a counter fed by Focused/Unfocused events; that
	///		shape is deliberately NOT copied here, because the counter is the stored copy this
	///		control must not have.
	///		</para>
	///		<para>
	///		<c>DungeonDetailPage</c> reads this to gate its Ctrl+Up / Ctrl+Down reorder route, which
	///		fires only while exactly one floor card reports focus. The gate is the CARD and not the
	///		drag handle because the handle is a Label again (contract C1 amendment A1) and a Label
	///		takes no keyboard focus on WinUI; MASTER-164 already made every card reachable by Tab
	///		through ExpandToggle, which is what the route rests on now.
	///		</para>
	/// </summary>
	public bool ContainsKeyboardFocus => HasFocusedDescendant(this);

	/// <summary>
	///		Gets or sets a value indicating whether this floor's area list is shown.
	///		Expanding is the only route to a floor's areas, so this is also what the page means by
	///		selecting a floor. Nothing in this control writes it: <c>DungeonDetailPage</c>'s floor
	///		row template binds it one way to whether this floor is the editor state's selected floor,
	///		and a local write would displace that binding. A tap or a chevron click raises
	///		<see cref="SelectRequested"/> instead and lets the owner decide.
	/// </summary>
	public bool IsExpanded
	{
		get => (bool)GetValue(IsExpandedProperty);
		set => SetValue(IsExpandedProperty, value);
	}

	/// <summary>
	///		Gets or sets the identifier of the area the editor state has focused, or
	///		<see langword="null"/> when none is. Each area row's <see cref="AreaSidebarItem.IsSelected"/>
	///		is bound one way to whether its own area carries this identifier, so this value is the
	///		only input to that appearance and no method here pushes it into a row.
	/// </summary>
	public Guid? FocusedAreaId
	{
		get => (Guid?)GetValue(FocusedAreaIdProperty);
		set => SetValue(FocusedAreaIdProperty, value);
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

		// The label is pinned to one line and tail-truncates a name longer than its column, so the
		// tooltip is how the full name is read. It is set from the label's own text after both arms
		// above, so the placeholder and every rename reach it without a second copy of either.
		ToolTipProperties.SetText(
			FloorNameLabel,
			FloorNameLabel.Text);

		// Colour AND description together — see NotebookAffordance for why the pair is one call
		// and not two copies.
		NotebookAffordance.Apply(
			NotebookButton,
			floor.HasNotebookContent);
	}

	/// <summary>
	///		Moves keyboard focus to this floor card's reorder affordance — the expand chevron, which
	///		is the element on this card that reliably takes keyboard focus on WinUI.
	///		Focus is an ACTION, not appearance state, so it is a method and not a bindable property —
	///		and the chevron lives inside this control's template, so the page has no other handle on
	///		it. It is the one public method this control keeps, and it does not reopen MASTER-098,
	///		whose direction is that APPEARANCE state be bindable rather than driven by imperative
	///		setters: it moves focus and sets no appearance state.
	///		<para>
	///		The page calls this after a keyboard reorder completes. A reorder is a full reload, which
	///		empties and refills the floor list's templated BindableLayout, so every card is a new
	///		instance and the element that held focus is destroyed by the very reorder the user just
	///		performed; without this the route would work exactly once. It worked on repeat in
	///		observation pass O1 and must not regress.
	///		</para>
	///		<para>
	///		It targets the chevron and NOT the drag handle, which contract C1 amendment A1 reverted
	///		to a Label. A1 discloses the behaviour change this carries: a user who pressed Ctrl+arrow
	///		with focus on the delete button lands on the chevron afterwards rather than back on
	///		delete. Predictable and announced beats focus lost to the top of the page.
	///		</para>
	/// </summary>
	public void FocusReorderAffordance()
	{
		// THE TYPE ANNOTATION IS A GUARD, not a style choice, and it is the same guard
		// ApplyExpanded puts on ExpandToggle for the same reason. ExpandToggle must stay a Button:
		// as a Label it takes no keyboard focus on WinUI (MASTER-164), so this restoration would
		// silently do nothing and the keyboard reorder route would work exactly once — and nothing
		// about a Label would fail to compile against the call below. Written out as a Button,
		// reverting the markup is CS0029 and the build stops.
		Button toggle = ExpandToggle;

		toggle.Focus();
	}

	// The walk behind ContainsKeyboardFocus. It is recursive and unmemoised on purpose: the result
	// is derived from the descendants' own IsFocused and is never stored, so there is nothing to go
	// stale. The visual-tree walk is the shape DmCard.ForEachVisualElement already uses in this
	// project. The root itself is skipped because a ContentView takes no focus and the question is
	// about DESCENDANTS.
	private static bool HasFocusedDescendant(Element root)
	{
		foreach (var child in ((IVisualTreeElement)root).GetVisualChildren())
		{
			if (child is not Element childElement)
			{
				continue;
			}

			if (childElement is VisualElement { IsFocused: true })
			{
				return true;
			}

			if (HasFocusedDescendant(childElement))
			{
				return true;
			}
		}

		return false;
	}

	// The glyph IS the expanded state, so the description moves with it; both are CONTENT rather
	// than appearance, which is why they stay here while the colours live in the XAML's
	// FloorStates group. The three IsVisible toggles this method used to perform are bindings on
	// IsExpanded now.
	private void ApplyExpanded()
	{
		var expanded = IsExpanded;

		// THE TYPE ANNOTATION IS A GUARD, not a style choice. ExpandToggle must stay a Button:
		// as a Label it takes no keyboard focus on WinUI, and expanding is the only route to a
		// floor's areas, so the entire area list went keyboard-unreachable (MASTER-164). Nothing
		// about a Label would fail to compile against the two statements below, so reverting the
		// markup would be a silent regression — exactly the way this defect arrived. Written out
		// as a Button, the revert is CS0029 and the build stops. Same reason the four *State
		// constants at the top of this file are named rather than inlined.
		Button toggle = ExpandToggle;

		toggle.Text = expanded ? "▾" : "▸";

		SemanticProperties.SetDescription(
			toggle,
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

	// A card tap only ASKS to be selected. The page's SelectRequested handler decides what that
	// means — select a collapsed floor, exit area focus, or toggle an already-selected floor off —
	// and writes the result to the editor state, which IsExpanded is bound to. Writing IsExpanded
	// here as well would be two owners of one fact, and as a local value it would displace that
	// binding for good (MASTER-098 / UIKIT-011).
	// Delete and notebook suppress this to avoid unexpected expansion on hover-revealed controls.
	private void OnCardTapped(object? sender, TappedEventArgs e)
	{
		if (_suppressCardTap)
		{
			_suppressCardTap = false;
			return;
		}

		SelectRequested?.Invoke(this, EventArgs.Empty);
	}

	// The chevron does exactly what a card tap does, and that is the point: it is the KEYBOARD
	// route to the same behaviour, not a second behaviour. It deliberately does not decide the
	// expanded state for itself — the page owns that decision through the editor state the
	// template binds IsExpanded to — so writing it here would be two mechanisms driving one
	// fact, which is the exact defect MASTER-098 took out of this file.
	//
	// IT ALSO DOES NOT SET _suppressCardTap, and that is measured rather than reasoned. A Clicked
	// on this Button does NOT reach the card's VerticalStackLayout TapGestureRecognizer, so the
	// flag had nothing to suppress and stayed set — and the next card-body tap was swallowed by
	// it. Observed 2026-09-11 against the built app driven through UIA: chevron expand, chevron
	// collapse, then a card-body click that did nothing, where the same card-body click expanded
	// the floor in a control run with no chevron click before it.
	private void OnExpandToggleClicked(object? sender, EventArgs e) =>
		SelectRequested?.Invoke(this, EventArgs.Empty);

	// THE DRAG HANDLE'S TAP HANDLER IS DELIBERATELY GONE, along with the tap recogniser that named
	// it, and it is NOT coming back now that the handle is a Label again. It set _suppressCardTap,
	// and when the handle was briefly a Button its press stopped reaching the card's
	// VerticalStackLayout tap recogniser — the same measured behaviour OnExpandToggleClicked's
	// comment above records — so the flag had nothing to suppress, stayed set, and swallowed the
	// next card-body tap. Restoring it would be restoring a compensation for an element this file
	// no longer has: contract C1 amendment A1 keeps it deleted. _suppressCardTap itself stays:
	// OnDeleteClicked and OnNotebookClicked still set it.

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
		DragOverReceived?.Invoke(this, e);

		_isDragOver = true;
		UpdateVisualState();
	}

	private void OnDragLeave(object? sender, DragEventArgs e)
	{
		// Only this card's own hover state ends here. The page's insertion line is not hidden on a
		// card leave: the pointer usually leaves one card for the seam or the next card while the
		// drag goes on, and the list's own drag-over and drag-leave own the line there.
		_isDragOver = false;
		UpdateVisualState();
	}

	private void OnDrop(object? sender, DropEventArgs e)
	{
		DropReceived?.Invoke(this, EventArgs.Empty);

		_isDragOver = false;
		UpdateVisualState();

		// The four silent returns of contract C1 section 1 live in FloorReorderDrop.Decide, which
		// the page's floor-list drop target calls too, so the two drop targets cannot disagree.
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
