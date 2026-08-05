using DungeonMaster.App.Assets.TextureAssets;
using DungeonMaster.Domain.Entities.Area;

namespace DungeonMaster.App.Views.DungeonDetail;

public partial class TexturePaletteView : ContentView
{
	public event EventHandler<(string Category, TextureKind? Texture, StructureKind? Structure)>? SelectionChanged;
	public event EventHandler? PackChanged;
	public event EventHandler<TextureSecondaryPaletteView>? SecondaryPaletteRequested;
	public event EventHandler? SecondaryPaletteDismissed;

	public TextureKind?    ActiveTexture   { get; private set; }
	public StructureKind?  ActiveStructure { get; private set; }

	private const int Cols         = 4;
	private const int PrimaryCount = Cols * 2 - 1; // 7 — fits 2 rows with the last slot for the expander

	private static readonly Color           _thumbNormal   = Color.FromArgb("#C8AC78");
	private static readonly Color           _thumbHover    = Color.FromArgb("#E8CC9A");
	private static readonly SolidColorBrush _strokeNormal   = new(Color.FromArgb("#9A7820"));
	private static readonly SolidColorBrush _strokeHover    = new(Color.FromArgb("#C8A040"));
	private static readonly SolidColorBrush _strokeSelected = new(Color.FromArgb("#2C1A08"));

	private Border? _selectedBorder;

	private TextureSecondaryPaletteView? _secondaryPalette;
	private string? _currentSecondaryCategory;

	public TexturePaletteView()
	{
		InitializeComponent();

		foreach (var (_, displayName) in TexturePackRegistry.AvailablePacks)
			PackPicker.Items.Add(displayName);

#if WINDOWS
		TextureScrollView.HandlerChanged += (_, _) =>
		{
			if (TextureScrollView.Handler?.PlatformView is
				Microsoft.UI.Xaml.Controls.ScrollViewer sv)
			{
				sv.RequestedTheme = Microsoft.UI.Xaml.ElementTheme.Light;
			}
		};
		PackPicker.HandlerChanged += (_, _) =>
		{
			if (PackPicker.Handler?.PlatformView is not Microsoft.UI.Xaml.Controls.ComboBox cb)
				return;

			cb.RequestedTheme = Microsoft.UI.Xaml.ElementTheme.Light;
			cb.MinWidth = 0;
			cb.Margin = new Microsoft.UI.Xaml.Thickness(0, 0, 2, 0);
			cb.BorderThickness = new Microsoft.UI.Xaml.Thickness(1);
			cb.HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Stretch;
			cb.BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(
				Windows.UI.Color.FromArgb(255, 154, 120, 32));
			cb.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
				Windows.UI.Color.FromArgb(255, 200, 172, 120));
			cb.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
				Windows.UI.Color.FromArgb(255, 92, 62, 16));

			cb.Loaded += (_, _) =>
			{
				try
				{
					var border  = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 154, 120,  32));
					var bg      = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 200, 172, 120));
					var popup   = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 237, 216, 160));
					var text    = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255,  92,  62,  16));
					var hover   = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 214, 190, 130));
					var pressed = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 184, 154,  96));
					var transp  = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);

					var d = new Microsoft.UI.Xaml.ResourceDictionary();
					d["ComboBoxDropDownBackground"]                = popup;
					d["ComboBoxDropDownBorderBrush"]               = border;
					d["ComboBoxDropDownGlyphForeground"]           = text;
					d["ComboBoxDropDownGlyphForegroundFocused"]    = text;
					d["ComboBoxItemPillFillBrush"]                 = border;
					d["ComboBoxItemForeground"]                    = text;
					d["ComboBoxItemForegroundPointerOver"]         = text;
					d["ComboBoxItemForegroundSelected"]            = text;
					d["ComboBoxItemForegroundSelectedUnfocused"]   = text;
					d["ComboBoxItemForegroundPressed"]             = text;
					d["ComboBoxItemBackground"]                    = transp;
					d["ComboBoxItemBackgroundPointerOver"]         = hover;
					d["ComboBoxItemBackgroundPressed"]             = pressed;
					d["ComboBoxItemBackgroundSelected"]            = bg;
					d["ComboBoxItemBackgroundSelectedPointerOver"] = hover;
					d["ComboBoxItemBackgroundSelectedUnfocused"]   = bg;

					cb.Resources.ThemeDictionaries["Light"] = d;
				}
				catch { /* non-critical visual styling — fail silently */ }
			};
		};
#endif

		this.Loaded += OnLoaded;
	}

	private async void OnLoaded(object? sender, EventArgs e)
	{
		if (TexturePackRegistry.AvailablePacks.Count == 0)
			return;

		PackPicker.SelectedIndex = 0;
		await LoadPackAsync(TexturePackRegistry.AvailablePacks[0].Id);
	}

	// ── Grid population ──────────────────────────────────────────────────────

	private void RebuildGrids(
		IReadOnlyDictionary<TextureKind, string>   texturePaths,
		IReadOnlyDictionary<StructureKind, string> structurePaths)
	{
		_selectedBorder = null;

		var floorKinds = Enum.GetValues<TextureKind>()
			.Where(k => TextureKindInfo.GetCategory(k) == "Floor" && texturePaths.ContainsKey(k))
			.ToList();
		var wallKinds = Enum.GetValues<TextureKind>()
			.Where(k => TextureKindInfo.GetCategory(k) == "Wall" && texturePaths.ContainsKey(k))
			.ToList();
		var structureKinds = Enum.GetValues<StructureKind>()
			.Where(k => structurePaths.ContainsKey(k))
			.ToList();

		PopulateTextureGrid(FloorGrid, floorKinds, texturePaths, "Floor");
		PopulateTextureGrid(WallGrid,  wallKinds,  texturePaths, "Wall");
		PopulateStructureGrid(StructureGrid, structureKinds, structurePaths);
	}

	private void PopulateTextureGrid(
		Grid grid,
		IReadOnlyList<TextureKind> kinds,
		IReadOnlyDictionary<TextureKind, string> paths,
		string category)
	{
		grid.Children.Clear();
		grid.RowDefinitions.Clear();

		var showExpander = kinds.Count > PrimaryCount;
		var primary      = kinds.Take(PrimaryCount).ToList();
		var totalSlots   = primary.Count + (showExpander ? 1 : 0);
		var rowCount     = Math.Max(1, (int)Math.Ceiling(totalSlots / (double)Cols));
		for (var r = 0; r < rowCount; r++)
			grid.RowDefinitions.Add(new RowDefinition { Height = 40 });

		for (var i = 0; i < primary.Count; i++)
			grid.Add(BuildTextureTile(primary[i], paths[primary[i]]), i % Cols, i / Cols);

		if (showExpander)
		{
			var allKinds = kinds;
			grid.Add(
				BuildExpanderButton(() => OpenTextureSecondary(category, allKinds, paths)),
				PrimaryCount % Cols, PrimaryCount / Cols);
		}
	}

	private void PopulateStructureGrid(
		Grid grid,
		IReadOnlyList<StructureKind> kinds,
		IReadOnlyDictionary<StructureKind, string> paths)
	{
		grid.Children.Clear();
		grid.RowDefinitions.Clear();

		var showExpander = kinds.Count > PrimaryCount;
		var primary      = kinds.Take(PrimaryCount).ToList();
		var totalSlots   = primary.Count + (showExpander ? 1 : 0);
		var rowCount     = Math.Max(1, (int)Math.Ceiling(totalSlots / (double)Cols));
		for (var r = 0; r < rowCount; r++)
			grid.RowDefinitions.Add(new RowDefinition { Height = 40 });

		for (var i = 0; i < primary.Count; i++)
			grid.Add(BuildStructureTile(primary[i], paths[primary[i]]), i % Cols, i / Cols);

		if (showExpander)
		{
			var allKinds = kinds;
			grid.Add(
				BuildExpanderButton(() => OpenStructureSecondary("Structure", allKinds, paths)),
				PrimaryCount % Cols, PrimaryCount / Cols);
		}
	}

	// ── Tile factories ────────────────────────────────────────────────────────

	private Border BuildTextureTile(TextureKind kind, string path)
	{
		var border = new Border
		{
			BackgroundColor = _thumbNormal,
			Stroke          = _strokeNormal,
			StrokeThickness = 1,
			StrokeShape     = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 2 },
			HeightRequest   = 40,
			Content         = new Image { Source = path, Aspect = Aspect.AspectFill }
		};
		ToolTipProperties.SetText(border, TextureKindInfo.GetDisplayName(kind));

		border.GestureRecognizers.Add(new TapGestureRecognizer
		{
			Command = new Command(() => SelectTexture(kind, border))
		});
		AttachThumbnailHover(border);
		return border;
	}

	private Border BuildStructureTile(StructureKind kind, string path)
	{
		var border = new Border
		{
			BackgroundColor = _thumbNormal,
			Stroke          = _strokeNormal,
			StrokeThickness = 1,
			StrokeShape     = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 2 },
			HeightRequest   = 40,
			Content         = new Image { Source = path, Aspect = Aspect.AspectFit }
		};
		ToolTipProperties.SetText(border, StructureKindInfo.GetDisplayName(kind));

		border.GestureRecognizers.Add(new TapGestureRecognizer
		{
			Command = new Command(() => SelectStructure(kind, border))
		});
		AttachThumbnailHover(border);
		return border;
	}

	private Border BuildExpanderButton(Action onTapped)
	{
		var border = new Border
		{
			BackgroundColor = _thumbNormal,
			Stroke          = _strokeNormal,
			StrokeThickness = 1,
			StrokeShape     = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 2 },
			HeightRequest   = 40,
			Content         = new Label
			{
				Text              = "···",
				FontSize          = 18,
				TextColor         = Color.FromArgb("#7A5030"),
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions   = LayoutOptions.Center
			}
		};

		var tap = new TapGestureRecognizer();
		tap.Tapped += (_, _) => onTapped();
		border.GestureRecognizers.Add(tap);
		AttachThumbnailHover(border);
		return border;
	}

	private void AttachThumbnailHover(Border border)
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

	// ── Selection ─────────────────────────────────────────────────────────────

	private void SelectTexture(TextureKind kind, Border border)
	{
		ClearSelection();
		_selectedBorder        = border;
		border.Stroke          = _strokeSelected;
		border.StrokeThickness = 2;
		ActiveTexture          = kind;
		ActiveStructure        = null;
		SelectionChanged?.Invoke(this, (TextureKindInfo.GetCategory(kind), kind, null));
	}

	private void SelectStructure(StructureKind kind, Border border)
	{
		ClearSelection();
		_selectedBorder        = border;
		border.Stroke          = _strokeSelected;
		border.StrokeThickness = 2;
		ActiveStructure        = kind;
		ActiveTexture          = null;
		SelectionChanged?.Invoke(this, (StructureKindInfo.GetCategory(kind), null, kind));
	}

	private void SelectTextureFromSecondary(string category, TextureKind kind)
	{
		ClearSelection();
		ActiveTexture   = kind;
		ActiveStructure = null;
		SelectionChanged?.Invoke(this, (category, kind, null));
	}

	private void SelectStructureFromSecondary(string category, StructureKind kind)
	{
		ClearSelection();
		ActiveStructure = kind;
		ActiveTexture   = null;
		SelectionChanged?.Invoke(this, (category, null, kind));
	}

	private void ClearSelection()
	{
		if (_selectedBorder is null)
			return;
		_selectedBorder.Stroke          = _strokeNormal;
		_selectedBorder.StrokeThickness = 1;
		_selectedBorder.BackgroundColor = _thumbNormal;
		_selectedBorder = null;
	}

	// ── Secondary palette ─────────────────────────────────────────────────────

	private void OpenTextureSecondary(
		string category,
		IReadOnlyList<TextureKind> kinds,
		IReadOnlyDictionary<TextureKind, string> paths)
	{
		if (_secondaryPalette?.IsVisible == true && _currentSecondaryCategory == category)
		{
			SecondaryPaletteDismissed?.Invoke(this, EventArgs.Empty);
			_currentSecondaryCategory = null;
			return;
		}

		EnsureSecondaryPalette();
		_secondaryPalette!.PopulateTextures(category, kinds, paths);
		_currentSecondaryCategory = category;
		SecondaryPaletteRequested?.Invoke(this, _secondaryPalette);
	}

	private void OpenStructureSecondary(
		string category,
		IReadOnlyList<StructureKind> kinds,
		IReadOnlyDictionary<StructureKind, string> paths)
	{
		if (_secondaryPalette?.IsVisible == true && _currentSecondaryCategory == category)
		{
			SecondaryPaletteDismissed?.Invoke(this, EventArgs.Empty);
			_currentSecondaryCategory = null;
			return;
		}

		EnsureSecondaryPalette();
		_secondaryPalette!.PopulateStructures(category, kinds, paths);
		_currentSecondaryCategory = category;
		SecondaryPaletteRequested?.Invoke(this, _secondaryPalette);
	}

	private void EnsureSecondaryPalette()
	{
		if (_secondaryPalette is not null)
			return;

		_secondaryPalette = new TextureSecondaryPaletteView();
		_secondaryPalette.CloseRequested += (_, _) =>
		{
			SecondaryPaletteDismissed?.Invoke(this, EventArgs.Empty);
			_currentSecondaryCategory = null;
		};
		_secondaryPalette.SelectionChanged += (_, args) =>
		{
			var (cat, texture, structure) = args;
			if (texture is not null)
				SelectTextureFromSecondary(cat, texture.Value);
			else if (structure is not null)
				SelectStructureFromSecondary(cat, structure.Value);
			SecondaryPaletteDismissed?.Invoke(this, EventArgs.Empty);
			_currentSecondaryCategory = null;
		};
	}

	// ── Pack loading ──────────────────────────────────────────────────────────

	private async void OnPackPickerChanged(object? sender, EventArgs e)
	{
		var idx = PackPicker.SelectedIndex;
		if (idx < 0 || idx >= TexturePackRegistry.AvailablePacks.Count)
			return;

		await LoadPackAsync(TexturePackRegistry.AvailablePacks[idx].Id);
	}

	private async Task LoadPackAsync(string packId)
	{
		var result = await TexturePackRegistry.LoadPackAsync(packId);
		if (result is null)
			return;

		TextureAssetCatalogue.SetPaths(result.TexturePaths);
		StructureAssetCatalogue.SetPaths(result.StructurePaths);

		RebuildGrids(result.TexturePaths, result.StructurePaths);

		PackChanged?.Invoke(this, EventArgs.Empty);
	}
}
