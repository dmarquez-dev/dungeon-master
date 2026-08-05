using DungeonMaster.App.Assets.TextureAssets;
using DungeonMaster.Domain.Entities.Area;

namespace DungeonMaster.App.Views.DungeonDetail;

public sealed class TextureSecondaryPaletteView : ContentView
{
	public event EventHandler<(string Category, TextureKind? Texture, StructureKind? Structure)>? SelectionChanged;
	public event EventHandler? CloseRequested;

	private readonly Label _titleLabel;
	private readonly Grid  _itemGrid;
	private Border? _selectedBorder;

	private static readonly Color           _thumbNormal   = Color.FromArgb("#C8AC78");
	private static readonly Color           _thumbHover    = Color.FromArgb("#E8CC9A");
	private static readonly SolidColorBrush _strokeNormal   = new(Color.FromArgb("#9A7820"));
	private static readonly SolidColorBrush _strokeHover    = new(Color.FromArgb("#C8A040"));
	private static readonly SolidColorBrush _strokeSelected = new(Color.FromArgb("#2C1A08"));

	private const int Cols = 4;

	public TextureSecondaryPaletteView()
	{
		_titleLabel = new Label
		{
			FontFamily       = "EBGaramondRegular",
			FontSize         = 13,
			TextColor        = Color.FromArgb("#5C3E10"),
			TextTransform    = TextTransform.Uppercase,
			CharacterSpacing = 0.8,
			VerticalOptions  = LayoutOptions.Center
		};

		var closeBtn = new Button
		{
			Text            = "✕",
			FontSize        = 12,
			TextColor       = Color.FromArgb("#7A5030"),
			BackgroundColor = Colors.Transparent,
			Padding         = 0,
			WidthRequest    = 22,
			HeightRequest   = 22
		};
		closeBtn.Clicked += (_, _) => CloseRequested?.Invoke(this, EventArgs.Empty);

		var header = new Grid
		{
			ColumnDefinitions = [
				new ColumnDefinition { Width = GridLength.Star },
				new ColumnDefinition { Width = GridLength.Auto }
			]
		};
		header.Add(_titleLabel, 0, 0);
		header.Add(closeBtn,    1, 0);

		_itemGrid = new Grid { ColumnSpacing = 4, RowSpacing = 4 };
		for (var i = 0; i < Cols; i++)
			_itemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = 48 });

		var scroll = new ScrollView
		{
			Orientation                 = ScrollOrientation.Vertical,
			VerticalScrollBarVisibility = ScrollBarVisibility.Always,
			MaximumHeightRequest        = 200,
			Content                     = _itemGrid
		};

		Content = new Border
		{
			BackgroundColor = Color.FromArgb("#EDD8A0"),
			Stroke          = new SolidColorBrush(Color.FromArgb("#8B5E1A")),
			StrokeThickness = 1,
			StrokeShape     = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 4 },
			Padding         = new Thickness(8),
			WidthRequest    = 224,
			HorizontalOptions = LayoutOptions.Start,
			Content         = new VerticalStackLayout
			{
				Spacing  = 4,
				Children = { header, scroll }
			}
		};
	}

	public void PopulateTextures(
		string category,
		IReadOnlyList<TextureKind> kinds,
		IReadOnlyDictionary<TextureKind, string> paths)
	{
		_selectedBorder = null;
		_titleLabel.Text = $"All {category}";
		PopulateGrid(kinds.Select(k => (
			Path:    paths.TryGetValue(k, out var p) ? p : null,
			Tooltip: TextureKindInfo.GetDisplayName(k),
			OnSelect: (Action)(() =>
				SelectionChanged?.Invoke(this, (TextureKindInfo.GetCategory(k), k, null)))
		)).ToList());
	}

	public void PopulateStructures(
		string category,
		IReadOnlyList<StructureKind> kinds,
		IReadOnlyDictionary<StructureKind, string> paths)
	{
		_selectedBorder = null;
		_titleLabel.Text = $"All {category}";
		PopulateGrid(kinds.Select(k => (
			Path:    paths.TryGetValue(k, out var p) ? p : null,
			Tooltip: StructureKindInfo.GetDisplayName(k),
			OnSelect: (Action)(() =>
				SelectionChanged?.Invoke(this, (StructureKindInfo.GetCategory(k), null, (StructureKind?)k)))
		)).ToList());
	}

	private void PopulateGrid(IReadOnlyList<(string? Path, string Tooltip, Action OnSelect)> items)
	{
		_itemGrid.Children.Clear();
		_itemGrid.RowDefinitions.Clear();

		var rows = (int)Math.Ceiling(items.Count / (double)Cols);
		for (var r = 0; r < rows; r++)
			_itemGrid.RowDefinitions.Add(new RowDefinition { Height = 48 });

		for (var i = 0; i < items.Count; i++)
		{
			var (path, tooltip, onSelect) = items[i];
			_itemGrid.Add(BuildTile(path, tooltip, onSelect), i % Cols, i / Cols);
		}
	}

	private Border BuildTile(string? path, string tooltip, Action onSelect)
	{
		View content = path is not null
			? new Image { Source = path, Aspect = Aspect.AspectFit, Margin = 3 }
			: new Label
			{
				Text              = tooltip.Length > 0 ? tooltip[..1] : "?",
				FontSize          = 11,
				TextColor         = Color.FromArgb("#2C1A08"),
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions   = LayoutOptions.Center
			};

		var border = new Border
		{
			WidthRequest    = 48,
			HeightRequest   = 48,
			Padding         = 0,
			BackgroundColor = _thumbNormal,
			Stroke          = _strokeNormal,
			StrokeThickness = 1,
			StrokeShape     = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 2 },
			Content         = content
		};
		ToolTipProperties.SetText(border, tooltip);

		var tap = new TapGestureRecognizer();
		tap.Tapped += (_, _) =>
		{
			SelectBorder(border);
			onSelect();
		};
		border.GestureRecognizers.Add(tap);
		AttachHover(border);
		return border;
	}

	private void SelectBorder(Border border)
	{
		if (_selectedBorder is not null)
		{
			_selectedBorder.Stroke          = _strokeNormal;
			_selectedBorder.StrokeThickness = 1;
		}
		_selectedBorder         = border;
		border.Stroke           = _strokeSelected;
		border.StrokeThickness  = 2;
	}

	private void AttachHover(Border border)
	{
		var ptr = new PointerGestureRecognizer();
		ptr.PointerEntered += (_, _) =>
		{
			border.BackgroundColor = _thumbHover;
			if (border != _selectedBorder)
				border.Stroke = _strokeHover;
		};
		ptr.PointerExited += (_, _) =>
		{
			border.BackgroundColor = _thumbNormal;
			border.Stroke          = border == _selectedBorder ? _strokeSelected : _strokeNormal;
			if (border == _selectedBorder)
				border.StrokeThickness = 2;
		};
		border.GestureRecognizers.Add(ptr);
	}
}
