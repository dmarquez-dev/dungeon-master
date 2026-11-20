using System.Windows.Input;
using DungeonMaster.App.Core.Presentation;

namespace DungeonMaster.App.Controls;

/// <summary>
///		A sidebar list item that represents a single dungeon area, supporting name display and select/hover interactions.
/// </summary>
public partial class AreaSidebarItem : ContentView
{
	// The visual states declared on ItemBorder in AreaSidebarItem.xaml. Named once here so a
	// rename in the markup breaks the build rather than silently going to a state that does not
	// exist — VisualStateManager.GoToState returns false for an unknown name and paints nothing.
	private const string NormalState = "Normal";
	private const string HoveredState = "Hovered";
	private const string SelectedState = "Selected";

	/// <summary>
	///		Identifies the <see cref="Area"/> bindable property.
	/// </summary>
	public static readonly BindableProperty AreaProperty =
		BindableProperty.Create(
			nameof(Area),
			typeof(AreaItem),
			typeof(AreaSidebarItem),
			null,
			propertyChanged: (bindable, _, newValue) =>
				((AreaSidebarItem)bindable).Populate((AreaItem?)newValue));

	/// <summary>
	///		Identifies the <see cref="IsSelected"/> bindable property.
	///		This replaces the private <c>_isSelected</c> field the appearance used to hang off
	///		(MASTER-098): a bindable property is a valid binding target, is settable through a style,
	///		and is monitorable for change, which is what lets the SelectionStates group in the XAML
	///		own the selected appearance instead of six assignments in code.
	/// </summary>
	public static readonly BindableProperty IsSelectedProperty =
		BindableProperty.Create(
			nameof(IsSelected),
			typeof(bool),
			typeof(AreaSidebarItem),
			false,
			propertyChanged: (bindable, _, _) =>
				((AreaSidebarItem)bindable).UpdateVisualState());

	/// <summary>
	///		Identifies the <see cref="SelectCommand"/> bindable property.
	/// </summary>
	public static readonly BindableProperty SelectCommandProperty =
		BindableProperty.Create(
			nameof(SelectCommand),
			typeof(ICommand),
			typeof(AreaSidebarItem),
			null);

	/// <summary>
	///		Identifies the <see cref="HoverStartedCommand"/> bindable property.
	/// </summary>
	public static readonly BindableProperty HoverStartedCommandProperty =
		BindableProperty.Create(
			nameof(HoverStartedCommand),
			typeof(ICommand),
			typeof(AreaSidebarItem),
			null);

	/// <summary>
	///		Identifies the <see cref="HoverEndedCommand"/> bindable property.
	/// </summary>
	public static readonly BindableProperty HoverEndedCommandProperty =
		BindableProperty.Create(
			nameof(HoverEndedCommand),
			typeof(ICommand),
			typeof(AreaSidebarItem),
			null);

	/// <summary>
	///		Identifies the <see cref="DeleteCommand"/> bindable property.
	/// </summary>
	public static readonly BindableProperty DeleteCommandProperty =
		BindableProperty.Create(
			nameof(DeleteCommand),
			typeof(ICommand),
			typeof(AreaSidebarItem),
			null);

	/// <summary>
	///		Identifies the <see cref="NotebookCommand"/> bindable property.
	/// </summary>
	public static readonly BindableProperty NotebookCommandProperty =
		BindableProperty.Create(
			nameof(NotebookCommand),
			typeof(ICommand),
			typeof(AreaSidebarItem),
			null);

	private bool _isHovered;

	/// <summary>
	///		Gets or sets the area data displayed and edited by this item.
	/// </summary>
	public AreaItem? Area
	{
		get => (AreaItem?)GetValue(AreaProperty);
		set => SetValue(AreaProperty, value);
	}

	/// <summary>
	///		Gets or sets a value indicating whether this area is the one currently being edited,
	///		which the SelectionStates group paints with the gold border and the selected tint.
	/// </summary>
	public bool IsSelected
	{
		get => (bool)GetValue(IsSelectedProperty);
		set => SetValue(IsSelectedProperty, value);
	}

	/// <summary>
	///		Gets or sets the command invoked with this item's <see cref="Area"/> when the user taps it.
	///		These five commands are what replaced the five events this control used to raise and
	///		<see cref="FloorSidebarItem"/> used to republish unchanged (MASTER-099): the owner sets
	///		them straight through the area list's item template, so nothing in between re-raises them.
	/// </summary>
	public ICommand? SelectCommand
	{
		get => (ICommand?)GetValue(SelectCommandProperty);
		set => SetValue(SelectCommandProperty, value);
	}

	/// <summary>
	///		Gets or sets the command invoked with this item's <see cref="Area"/> when the pointer enters it.
	/// </summary>
	public ICommand? HoverStartedCommand
	{
		get => (ICommand?)GetValue(HoverStartedCommandProperty);
		set => SetValue(HoverStartedCommandProperty, value);
	}

	/// <summary>
	///		Gets or sets the command invoked with this item's <see cref="Area"/> when the pointer exits it.
	/// </summary>
	public ICommand? HoverEndedCommand
	{
		get => (ICommand?)GetValue(HoverEndedCommandProperty);
		set => SetValue(HoverEndedCommandProperty, value);
	}

	/// <summary>
	///		Gets or sets the command invoked with this item's <see cref="Area"/> when the user requests its deletion.
	/// </summary>
	public ICommand? DeleteCommand
	{
		get => (ICommand?)GetValue(DeleteCommandProperty);
		set => SetValue(DeleteCommandProperty, value);
	}

	/// <summary>
	///		Gets or sets the command invoked with this item's <see cref="Area"/> when the user requests its notebook.
	/// </summary>
	public ICommand? NotebookCommand
	{
		get => (ICommand?)GetValue(NotebookCommandProperty);
		set => SetValue(NotebookCommandProperty, value);
	}

	/// <summary>
	///		Initializes a new instance of <see cref="AreaSidebarItem"/> and loads the XAML component tree.
	/// </summary>
	public AreaSidebarItem()
	{
		InitializeComponent();

		// Enter the resting state explicitly. The attributes on ItemBorder already paint it, but
		// entering Normal here is what gives the group a state to leave, so the first transition
		// out of it restores these values rather than whatever it last saw.
		UpdateVisualState();
	}

	private void Populate(AreaItem? area)
	{
		if (area is null)
		{
			return;
		}

		AreaNameLabel.Text = area.Name;

		// Colour AND description together — see NotebookAffordance for why the pair is one call
		// and not two copies.
		NotebookAffordance.Apply(
			NotebookButton,
			area.HasNotebookContent);
	}

	private void OnHoverEntered(object? sender, EventArgs e)
	{
		_isHovered = true;
		UpdateVisualState();

		if (Area is not null)
		{
			Invoke(HoverStartedCommand, Area);
		}
	}

	private void OnHoverExited(object? sender, EventArgs e)
	{
		_isHovered = false;
		UpdateVisualState();

		if (Area is not null)
		{
			Invoke(HoverEndedCommand, Area);
		}
	}

	private void OnItemTapped(object? sender, TappedEventArgs e)
	{
		if (Area is not null)
		{
			Invoke(SelectCommand, Area);
		}
	}

	private void OnDeleteClicked(object? sender, EventArgs e)
	{
		if (Area is not null)
		{
			Invoke(DeleteCommand, Area);
		}
	}

	private void OnNotebookClicked(object? sender, EventArgs e)
	{
		if (Area is not null)
		{
			Invoke(NotebookCommand, Area);
		}
	}

	/// <summary>
	///		Selects the visual state that the current selection and hover flags together mean.
	///		This method is the ONE owner of that decision: the states themselves say what each looks
	///		like, and nothing else in this control writes a colour.
	/// </summary>
	private void UpdateVisualState() =>
		VisualStateManager.GoToState(
			ItemBorder,
			IsSelected
				? SelectedState
				: _isHovered
					? HoveredState
					: NormalState);

	private static void Invoke(ICommand? command, AreaItem area)
	{
		if (command?.CanExecute(area) == true)
		{
			command.Execute(area);
		}
	}
}
