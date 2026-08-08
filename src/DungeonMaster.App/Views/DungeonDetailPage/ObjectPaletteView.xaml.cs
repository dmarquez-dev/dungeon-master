using DungeonMaster.App.Assets.ObjectAssets;
using DungeonMaster.App.Theme;
using DungeonMaster.Domain.Entities.Floor;

namespace DungeonMaster.App.Views.DungeonDetail;

public partial class ObjectPaletteView : ContentView
{
    private IReadOnlyList<string> _categoryOrder = [];
    private IReadOnlyDictionary<string, IReadOnlyList<ObjectKind>> _categories =
        new Dictionary<string, IReadOnlyList<ObjectKind>>();

    public ObjectKind? ActiveObject { get; private set; }

    public event EventHandler<ObjectKind?>? ActiveObjectChanged;
    public event EventHandler? CloseRequested;
    public event EventHandler<SecondaryPaletteView>? SecondaryPaletteRequested;
    public event EventHandler? SecondaryPaletteDismissed;
    public event EventHandler? PackChanged;

    public ObjectPaletteView()
    {
        InitializeComponent();

        // Populate the pack picker with known packs.
        foreach (var (_, displayName) in ObjectPackRegistry.AvailablePacks)
            PackPicker.Items.Add(displayName);

        // Apply parchment theme overrides to platform-native controls.
        // Windows: RequestedTheme + ResourceDictionary injection on the WinUI ComboBox/ScrollViewer.
        // macCatalyst: The Picker maps to a UIPickerView/UITextField. Colours set in XAML
        //   (DmSurfaceToolChip background, DmTextToolPanel text) are sufficient for the control itself.
        //   The ScrollViewer gold-thumb override lives in Platforms/Windows/App.xaml.cs
        //   (ApplyThemeResources), which writes the ScrollBarThumbFill* keys at the application
        //   level AND into the Light and Dark theme dictionaries — the RequestedTheme=Light below
        //   sends this ScrollViewer's ThemeResource lookup to the Light dictionary, so an
        //   application-level entry alone would not reach it. On macCatalyst the default
        //   scroll-indicator style (dark-on-light) is acceptable against the parchment surface.
        // TODO(macOS): If the macCatalyst Picker popup (UIPickerView) appears illegible, add a
        //   #if MACCATALYST HandlerChanged block here to set UIPickerView.BackgroundColor and
        //   apply UILabel foreground styling via a custom UIPickerViewDelegate.
#if WINDOWS
        CategoryScrollView.HandlerChanged += (_, _) =>
        {
            if (CategoryScrollView.Handler?.PlatformView is
                Microsoft.UI.Xaml.Controls.ScrollViewer sv)
            {
                sv.RequestedTheme = Microsoft.UI.Xaml.ElementTheme.Light;
            }
        };
        PackPicker.HandlerChanged += (_, _) =>
        {
            if (PackPicker.Handler?.PlatformView is not Microsoft.UI.Xaml.Controls.ComboBox cb)
            {
                return;
            }

            cb.RequestedTheme = Microsoft.UI.Xaml.ElementTheme.Light;
            cb.MinWidth = 0;
            cb.Margin = new Microsoft.UI.Xaml.Thickness(0, 0, 2, 0); // right gap keeps border visible
            cb.BorderThickness = new Microsoft.UI.Xaml.Thickness(1);
            cb.HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Stretch;
            cb.BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                DmTheme.ToPlatform(DmTheme.BorderToolChip));
            cb.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                DmTheme.ToPlatform(DmTheme.SurfaceToolChip));
            cb.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                DmTheme.ToPlatform(DmTheme.TextToolPanel));

            // Build a fresh parchment ResourceDictionary and attach it at the control level
            // so the dropdown popup (isolated visual tree) picks it up via ThemeResource lookup.
            cb.Loaded += (_, _) =>
            {
                try
                {
                    var border  = new Microsoft.UI.Xaml.Media.SolidColorBrush(DmTheme.ToPlatform(DmTheme.BorderToolChip));
                    var bg      = new Microsoft.UI.Xaml.Media.SolidColorBrush(DmTheme.ToPlatform(DmTheme.SurfaceToolChip));
                    var popup   = new Microsoft.UI.Xaml.Media.SolidColorBrush(DmTheme.ToPlatform(DmTheme.SurfaceToolPanel));
                    var text    = new Microsoft.UI.Xaml.Media.SolidColorBrush(DmTheme.ToPlatform(DmTheme.TextToolPanel));
                    var hover   = new Microsoft.UI.Xaml.Media.SolidColorBrush(DmTheme.ToPlatform(DmTheme.SurfaceComboItemHover));
                    var pressed = new Microsoft.UI.Xaml.Media.SolidColorBrush(DmTheme.ToPlatform(DmTheme.SurfaceComboItemPressed));
                    var transp  = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);

                    var d = new Microsoft.UI.Xaml.ResourceDictionary();
                    d["ComboBoxDropDownBackground"]                = popup;
                    d["ComboBoxDropDownBorderBrush"]               = border;
                    d["ComboBoxDropDownGlyphForeground"]           = text;
                    d["ComboBoxDropDownGlyphForegroundFocused"]    = text;
                    d["ComboBoxItemPillFillBrush"]                 = border; // accent pill → gold
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

                    // Walk the closed-state visual tree to set ellipsis trimming on the header TextBlock.
                    SetTextTrimmingOnChildren(cb, depth: 0);
                }
                catch { /* non-critical visual styling — fail silently */ }
            };
        };
#endif

        this.Loaded += OnLoaded;
    }

#if WINDOWS
    private static void SetTextTrimmingOnChildren(Microsoft.UI.Xaml.DependencyObject parent, int depth)
    {
        if (depth > 8)
        {
            return;
        }

        var count = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(parent);
        for (var i = 0; i < count; i++)
        {
            var child = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChild(parent, i);
            if (child is Microsoft.UI.Xaml.Controls.TextBlock tb)
            {
                tb.TextTrimming = Microsoft.UI.Xaml.TextTrimming.CharacterEllipsis;
            }

            SetTextTrimmingOnChildren(child, depth + 1);
        }
    }
#endif

    private async void OnLoaded(object? sender, EventArgs e)
    {
        if (ObjectPackRegistry.AvailablePacks.Count == 0)
        {
            return;
        }

        PackPicker.SelectedIndex = 0;
        await LoadPackAsync(ObjectPackRegistry.AvailablePacks[0].Id);
    }

    private async void OnPackPickerChanged(object? sender, EventArgs e)
    {
        var idx = PackPicker.SelectedIndex;
        if (idx < 0 || idx >= ObjectPackRegistry.AvailablePacks.Count)
        {
            return;
        }

        await LoadPackAsync(ObjectPackRegistry.AvailablePacks[idx].Id);
    }

    private async Task LoadPackAsync(string packId)
    {
        var result = await ObjectPackRegistry.LoadPackAsync(packId);
        if (result is null)
        {
            return;
        }

        _categoryOrder = result.CategoryOrder;
        _categories = result.Categories;
        ObjectAssetCatalogue.SetPaths(result.Paths);

        PackChanged?.Invoke(this, EventArgs.Empty);

        RebuildCategoryRows();
    }

    private void RebuildCategoryRows()
    {
        CategoriesContainer.Children.Clear();

        foreach (var category in _categoryOrder)
        {
            if (!_categories.TryGetValue(category, out var objects))
            {
                continue;
            }

            var row = BuildCategoryRow(category, objects);
            if (row is not null)
            {
                CategoriesContainer.Children.Add(row);
            }
        }
    }

    private View? BuildCategoryRow(string categoryName, IReadOnlyList<ObjectKind> objects)
    {
        const int cols = 4;
        const int primaryCount = cols * 2 - 1; // 7 — leaves one slot per two rows for the expander

        var available = objects.Where(kind => ObjectAssetCatalogue.Paths.ContainsKey(kind)).ToList();
        if (available.Count == 0)
        {
            return null;
        }

        var label = new Label
        {
            Text = categoryName.ToUpperInvariant(),
            FontFamily = DmTheme.FontBody,
            FontSize = DmTheme.FontSizeCaption,
            TextColor = DmTheme.TextToolPanel,
            CharacterSpacing = 0.5
        };

        var grid = new Grid { ColumnSpacing = DmTheme.SpaceXS, RowSpacing = DmTheme.SpaceXS };
        for (var i = 0; i < cols; i++)
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = 48 });
        }

        grid.RowDefinitions.Add(new RowDefinition { Height = 48 });
        grid.RowDefinitions.Add(new RowDefinition { Height = 48 });

        var slots = available.Take(primaryCount).ToList();
        for (var i = 0; i < slots.Count; i++)
        {
            grid.Add(BuildThumbButton(slots[i]), i % cols, i / cols);
        }

        if (available.Count > primaryCount)
        {
            grid.Add(BuildExpanderButton(categoryName, available), primaryCount % cols, primaryCount / cols);
        }

        // Was a bare 3, which is not a step on the spacing scale; snapped up to DmSpaceXS (4),
        // so the category caption now sits 1 DIU further from its thumbnail grid.
        return new VerticalStackLayout { Spacing = DmTheme.SpaceXS, Children = { label, grid } };
    }

    private View BuildThumbButton(ObjectKind kind)
    {
        var border = new Border
        {
            WidthRequest = 48,
            HeightRequest = 48,
            Padding = 0,
            BackgroundColor = DmTheme.SurfaceToolChip,
            Stroke = new SolidColorBrush(DmTheme.BorderToolChip),
            StrokeThickness = 1,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
            {
                CornerRadius = DmTheme.RadiusTight
            }
        };

        var displayName = ObjectKindInfo.GetDisplayName(kind);

        // The tooltip is a hover affordance and reaches nobody who is not holding a mouse; the
        // chip's only other content is an image, so the description is its one text alternative.
        ToolTipProperties.SetText(border, displayName);
        SemanticProperties.SetDescription(border, displayName);

        if (ObjectAssetCatalogue.Paths.TryGetValue(kind, out var path))
        {
            border.Content = new Image
            {
                Source = path,
                Aspect = Aspect.AspectFit,
                Margin = 2
            };
        }
        else
        {
            border.Content = new Label
            {
                Text = displayName.Length > 0 ? displayName[..1] : "?",
                FontSize = DmTheme.FontSizeMeta,
                TextColor = DmTheme.TextParchment,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
        }

        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) =>
            SelectObject(kind);
        border.GestureRecognizers.Add(tap);
        PaletteThumb.AttachHover(border);
        return border;
    }

    private Border BuildExpanderButton(string category, IReadOnlyList<ObjectKind> allObjects)
    {
        var label = new Label
        {
            Text = "···",
            FontSize = DmTheme.FontSizeBody,
            TextColor = DmTheme.InkMuted,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };

        var border = new Border
        {
            WidthRequest = 48,
            HeightRequest = 48,
            Padding = 0,
            BackgroundColor = DmTheme.SurfaceToolChip,
            Stroke = new SolidColorBrush(DmTheme.BorderToolChip),
            StrokeThickness = 1,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
            {
                CornerRadius = DmTheme.RadiusTight
            },
            Content = label
        };

        // The label is an ellipsis and nothing else, so the description is the whole of what a
        // reader gets; it names the category the expander opens.
        SemanticProperties.SetDescription(border, $"More {category}");
        SemanticProperties.SetHint(
            border,
            "Opens the full list of objects in this category.");

        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) =>
            OpenSecondaryPalette(category, allObjects);
        border.GestureRecognizers.Add(tap);
        PaletteThumb.AttachHover(border);
        return border;
    }

    private SecondaryPaletteView? _secondaryPalette;
    private EventHandler<ObjectKind>? _secondaryPaletteHandler;
    private string? _currentSecondaryCategory;

    private void OpenSecondaryPalette(string category, IReadOnlyList<ObjectKind> objects)
    {
        if (_secondaryPalette?.IsVisible == true && _currentSecondaryCategory == category)
        {
            SecondaryPaletteDismissed?.Invoke(this, EventArgs.Empty);
            _currentSecondaryCategory = null;
            return;
        }

        if (_secondaryPalette is null)
        {
            _secondaryPalette = new SecondaryPaletteView();
            _secondaryPalette.CloseRequested += (_, _) =>
            {
                SecondaryPaletteDismissed?.Invoke(this, EventArgs.Empty);
                _currentSecondaryCategory = null;
            };
        }
        else if (_secondaryPaletteHandler is not null)
        {
            _secondaryPalette.ObjectSelected -= _secondaryPaletteHandler;
        }

        _secondaryPaletteHandler = (_, kind) =>
        {
            SelectObject(kind);
            SecondaryPaletteDismissed?.Invoke(this, EventArgs.Empty);
            _currentSecondaryCategory = null;
        };
        _secondaryPalette.ObjectSelected += _secondaryPaletteHandler;
        _secondaryPalette.Populate(category, objects);
        _currentSecondaryCategory = category;
        SecondaryPaletteRequested?.Invoke(this, _secondaryPalette);
    }

    public void SelectObject(ObjectKind kind)
    {
        ActiveObject = kind;
        ActiveObjectChanged?.Invoke(this, kind);
    }

    public void ClearActiveObject()
    {
        ActiveObject = null;
        ActiveObjectChanged?.Invoke(this, null);
    }

    private void OnClosePalette(object? sender, EventArgs e)
    {
        ActiveObject = null;
        ActiveObjectChanged?.Invoke(this, null);
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }
}
