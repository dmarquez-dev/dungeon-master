using DungeonMaster.Domain.Entities.Floors;

namespace DungeonMaster.App.Views.DungeonDetail;

public partial class SecondaryPaletteView : ContentView
{
	public event EventHandler<ObjectKind>? ObjectSelected;
	public event EventHandler? CloseRequested;

	public SecondaryPaletteView()
	{
		InitializeComponent();
	}

	private void OnCloseClicked(object? sender, EventArgs e) =>
		CloseRequested?.Invoke(this, EventArgs.Empty);

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
