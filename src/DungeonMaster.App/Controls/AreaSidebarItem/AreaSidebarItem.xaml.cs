using DungeonMaster.Application.Features.Dungeons.Queries.GetDungeonDetail;
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
			typeof(AreaDto),
			typeof(AreaSidebarItem),
			null,
			propertyChanged: (bindable, _, newValue) =>
				((AreaSidebarItem)bindable).Populate((AreaDto?)newValue));

	/// <summary>
	///		Raised when the user taps this area to enter area edit context.
	/// </summary>
	public event EventHandler<AreaDto>? SelectRequested;

	/// <summary>
	///     Raised when the pointer enters this area item.
	/// </summary>
	public event EventHandler<AreaDto>? HoverStarted;

	/// <summary>
	///     Raised when the pointer exits this area item.
	/// </summary>
	public event EventHandler<AreaDto>? HoverEnded;

	/// <summary>
	///		Raised when the user requests deletion of this area.
	/// </summary>
	public event EventHandler<AreaDto>? DeleteRequested;

	/// <summary>
	///		Raised when the user requests the notebook for this area.
	/// </summary>
	public event EventHandler<AreaDto>? NotebookRequested;

	private bool _isSelected;

	/// <summary>
	///		Gets or sets the area data displayed and edited by this item.
	/// </summary>
	public AreaDto? Area
	{
		get => (AreaDto?)GetValue(AreaProperty);
		set => SetValue(AreaProperty, value);
	}

	/// <summary>
	///		Initializes a new instance of <see cref="AreaSidebarItem"/> and loads the XAML component tree.
	/// </summary>
	public AreaSidebarItem()
	{
		InitializeComponent();
	}

	private void Populate(AreaDto? area)
	{
		if (area is null)
		{
			return;
		}

		AreaNameLabel.Text = area.Name;

		var hasContent =
			!string.IsNullOrWhiteSpace(area.Description) ||
			!string.IsNullOrWhiteSpace(area.Notes);

		NotebookButton.TextColor = hasContent
			? DmColors.ScrollGoldLight
			: DmColors.InkMuted;
	}

	private void OnHoverEntered(object? sender, EventArgs e)
	{
		if (!_isSelected)
		{
			ItemBorder.BackgroundColor = DmColors.GoldHoverTint;
		}

		DeleteButton.IsVisible = true;

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

		DeleteButton.IsVisible = false;

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
	///		No-op — area name editing moved to the notebook.
	/// </summary>
	public void TryCommitActiveEditor() { }

	/// <summary>
	///		Sets the selected/editing visual state (gold border and subtle background).
	/// </summary>
	public void SetEditing(bool editing)
	{
		_isSelected = editing;

		if (editing)
		{
			ItemBorder.Stroke = new SolidColorBrush(DmColors.AreaSelectedBorder);
			ItemBorder.StrokeThickness = 1;
			ItemBorder.BackgroundColor = DmColors.AreaSelectedTint;
		}
		else
		{
			ItemBorder.Stroke = new SolidColorBrush(Colors.Transparent);
			ItemBorder.StrokeThickness = 0;
			ItemBorder.BackgroundColor = Colors.Transparent;
		}
	}

}
