using DungeonMaster.App.Core.Presentation;
using DungeonMaster.App.Theme;

namespace DungeonMaster.App.Controls;

/// <summary>
///		A sidebar list item that represents a single dungeon area, supporting name display and select/hover interactions.
/// </summary>
public partial class AreaSidebarItem : ContentView
{
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
	///		Raised when the user taps this area to enter area edit context.
	/// </summary>
	public event EventHandler<AreaItem>? SelectRequested;

	/// <summary>
	///     Raised when the pointer enters this area item.
	/// </summary>
	public event EventHandler<AreaItem>? HoverStarted;

	/// <summary>
	///     Raised when the pointer exits this area item.
	/// </summary>
	public event EventHandler<AreaItem>? HoverEnded;

	/// <summary>
	///		Raised when the user requests deletion of this area.
	/// </summary>
	public event EventHandler<AreaItem>? DeleteRequested;

	/// <summary>
	///		Raised when the user requests the notebook for this area.
	/// </summary>
	public event EventHandler<AreaItem>? NotebookRequested;

	private bool _isSelected;

	/// <summary>
	///		Gets or sets the area data displayed and edited by this item.
	/// </summary>
	public AreaItem? Area
	{
		get => (AreaItem?)GetValue(AreaProperty);
		set => SetValue(AreaProperty, value);
	}

	/// <summary>
	///		Initializes a new instance of <see cref="AreaSidebarItem"/> and loads the XAML component tree.
	/// </summary>
	public AreaSidebarItem()
	{
		InitializeComponent();
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
		if (!_isSelected)
		{
			ItemBorder.BackgroundColor = DmTheme.GoldHoverTint;
		}

		if (Area is not null)
		{
			HoverStarted?.Invoke(this, Area);
		}
	}

	private void OnHoverExited(object? sender, EventArgs e)
	{
		if (!_isSelected)
		{
			ItemBorder.BackgroundColor = Colors.Transparent;
		}

		if (Area is not null)
		{
			HoverEnded?.Invoke(this, Area);
		}
	}

	private void OnItemTapped(object? sender, TappedEventArgs e)
	{
		if (Area is not null)
		{
			SelectRequested?.Invoke(this, Area);
		}
	}

	private void OnDeleteClicked(object? sender, EventArgs e)
	{
		if (Area is not null)
		{
			DeleteRequested?.Invoke(this, Area);
		}
	}

	private void OnNotebookClicked(object? sender, EventArgs e)
	{
		if (Area is not null)
		{
			NotebookRequested?.Invoke(this, Area);
		}
	}

	/// <summary>
	///		Sets the selected/editing visual state (gold border and subtle background).
	/// </summary>
	public void SetEditing(bool editing)
	{
		_isSelected = editing;

		if (editing)
		{
			ItemBorder.Stroke = new SolidColorBrush(DmTheme.AreaSelectedBorder);
			ItemBorder.StrokeThickness = 1;
			ItemBorder.BackgroundColor = DmTheme.AreaSelectedTint;
		}
		else
		{
			ItemBorder.Stroke = new SolidColorBrush(Colors.Transparent);
			ItemBorder.StrokeThickness = 0;
			ItemBorder.BackgroundColor = Colors.Transparent;
		}
	}

}
