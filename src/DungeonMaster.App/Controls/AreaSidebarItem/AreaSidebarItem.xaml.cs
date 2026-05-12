using DungeonMaster.Application.Features.Dungeons.Queries.GetDungeonDetail;

namespace DungeonMaster.App.Controls;

/// <summary>
///		A sidebar list item that represents a single dungeon area, supporting inline name editing and delete actions.
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
	///		Raised when the user requests deletion of this area.
	/// </summary>
	public event EventHandler<AreaDto>? DeleteRequested;

	/// <summary>
	///		Raised when the user commits a new name for the area.
	/// </summary>
	public event EventHandler<string>? NameCommitted;

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

		AreaNameEditor.Text = area.Name;
	}

	private void OnPointerEntered(object? sender, PointerEventArgs e) =>
		DeleteButton.IsVisible = true;

	private void OnPointerExited(object? sender, PointerEventArgs e) =>
		DeleteButton.IsVisible = false;

	private void OnDeleteClicked(object? sender, EventArgs e)
	{
		if (Area is not null)
		{
			DeleteRequested?.Invoke(this, Area);
		}
	}

	/// <summary>
	///		Commits any in-progress name edit without requiring user interaction.
	/// </summary>
	public void TryCommitActiveEditor() =>
		AreaNameEditor.TryCommit();

	private void OnAreaNameCommitted(object? sender, string? name)
	{
		if (!string.IsNullOrWhiteSpace(name))
		{
			NameCommitted?.Invoke(this, name);
		}
	}
}
