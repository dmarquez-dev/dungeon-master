using DungeonMaster.Domain.Entities.Floors;

namespace DungeonMaster.App.Views.DungeonDetail;

/// <summary>
///		The secondary object palette, which lists every object of a single category as a grid of chips, five to a row.
/// </summary>
public partial class SecondaryPaletteView : ContentView
{
	/// <summary>
	///		Raised when one of the object chips is tapped, carrying the object kind that chip stands for.
	/// </summary>
	public event EventHandler<ObjectKind>? ObjectSelected;

	/// <summary>
	///		Raised when the panel's close button is tapped.
	/// </summary>
	public event EventHandler? CloseRequested;

	/// <summary>
	///		Initialises a new instance of <see cref="SecondaryPaletteView"/>.
	///		The title is blank and the grid is empty until <see cref="Populate"/> is called.
	/// </summary>
	public SecondaryPaletteView()
	{
		InitializeComponent();
	}

	private void OnCloseClicked(object? sender, EventArgs e) =>
		CloseRequested?.Invoke(this, EventArgs.Empty);

	/// <summary>
	///		Titles the panel for the given category and fills its grid with one chip per object, five to a row.
	///		The grid's children and row definitions are cleared first, so a repeat call replaces the contents rather than adding to them.
	///		Nothing here throws if the call is skipped; the panel simply shows an empty grid under a blank title.
	/// </summary>
	/// <param name="category">
	///		The category name shown in the panel's title.
	/// </param>
	/// <param name="objects">
	///		The objects to show, in the order they should appear.
	/// </param>
	public void Populate(string category, IReadOnlyList<ObjectKind> objects)
	{
		TitleLabel.Text = $"All {category}";
		ObjectGrid.Children.Clear();
		ObjectGrid.RowDefinitions.Clear();

		var rows = (int)Math.Ceiling(objects.Count / 5.0);
		for (var r = 0; r < rows; r++)
		{
			ObjectGrid.RowDefinitions.Add(new RowDefinition { Height = 48 });
		}

		for (var i = 0; i < objects.Count; i++)
		{
			// The chip is PaletteThumb's; the only part that is this view's own is that a tap is
			// reported to whoever opened the palette rather than acted on here.
			var border = PaletteThumb.Build(
				objects[i],
				kind =>
					ObjectSelected?.Invoke(this, kind));

			ObjectGrid.Add(border, i % 5, i / 5);
		}
	}
}
