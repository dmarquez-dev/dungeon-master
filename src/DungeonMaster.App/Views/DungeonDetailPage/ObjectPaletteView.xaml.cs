using DungeonMaster.App.Assets.ObjectAssets;
using DungeonMaster.App.Behaviors;
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
        //   The ScrollViewer gold-thumb override lives in Windows App.xaml only; on macCatalyst the
        //   default scroll-indicator style (dark-on-light) is acceptable against the parchment surface.
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
                DmColors.ToPlatform(DmColors.BorderToolChip));
            cb.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                DmColors.ToPlatform(DmColors.SurfaceToolChip));
            cb.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                DmColors.ToPlatform(DmColors.TextToolPanel));

            // Build a fresh parchment ResourceDictionary and attach it at the control level
            // so the dropdown popup (isolated visual tree) picks it up via ThemeResource lookup.
            cb.Loaded += (_, _) =>
            {
                try
                {
                    var border  = new Microsoft.UI.Xaml.Media.SolidColorBrush(DmColors.ToPlatform(DmColors.BorderToolChip));
                    var bg      = new Microsoft.UI.Xaml.Media.SolidColorBrush(DmColors.ToPlatform(DmColors.SurfaceToolChip));
                    var popup   = new Microsoft.UI.Xaml.Media.SolidColorBrush(DmColors.ToPlatform(DmColors.SurfaceToolPanel));
                    var text    = new Microsoft.UI.Xaml.Media.SolidColorBrush(DmColors.ToPlatform(DmColors.TextToolPanel));
                    var hover   = new Microsoft.UI.Xaml.Media.SolidColorBrush(DmColors.ToPlatform(DmColors.SurfaceComboItemHover));
                    var pressed = new Microsoft.UI.Xaml.Media.SolidColorBrush(DmColors.ToPlatform(DmColors.SurfaceComboItemPressed));
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
            FontFamily = "EBGaramondRegular",
            FontSize = 13,
            TextColor = DmColors.TextToolPanel,
            CharacterSpacing = 0.5
        };

        var grid = new Grid { ColumnSpacing = 4, RowSpacing = 4 };
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

        return new VerticalStackLayout { Spacing = 3, Children = { label, grid } };
    }

    private View BuildThumbButton(ObjectKind kind)
    {
        var border = new Border
        {
            WidthRequest = 48,
            HeightRequest = 48,
            Padding = 0,
            BackgroundColor = DmColors.SurfaceToolChip,
            Stroke = new SolidColorBrush(DmColors.BorderToolChip),
            StrokeThickness = 1,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 2 }
        };

        var displayName = ObjectKindInfo.GetDisplayName(kind);
        ToolTipProperties.SetText(border, displayName);

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
                FontSize = 11,
                TextColor = DmColors.TextParchment,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
        }

        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) =>
            SelectObject(kind);
        border.GestureRecognizers.Add(tap);
        AttachThumbHover(border);
        return border;
    }

    private Border BuildExpanderButton(string category, IReadOnlyList<ObjectKind> allObjects)
    {
        var label = new Label
        {
            Text = "···",
            FontSize = 18,
            TextColor = DmColors.InkMuted,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };

        var border = new Border
        {
            WidthRequest = 48,
            HeightRequest = 48,
            Padding = 0,
            BackgroundColor = DmColors.SurfaceToolChip,
            Stroke = new SolidColorBrush(DmColors.BorderToolChip),
            StrokeThickness = 1,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 2 },
            Content = label
        };

        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) =>
            OpenSecondaryPalette(category, allObjects);
        border.GestureRecognizers.Add(tap);
        AttachThumbHover(border);
        return border;
    }

    private static readonly Color ThumbNormal = DmColors.SurfaceToolChip;
    private static readonly Color ThumbHover   = DmColors.SurfaceToolChipHover;
    private static readonly SolidColorBrush ThumbStrokeNormal = new(DmColors.BorderToolChip);
    private static readonly SolidColorBrush ThumbStrokeHover  = new(DmColors.Gold);

    private static void AttachThumbHover(Border border)
    {
        border.Behaviors.Add(new HandCursorBehavior());
        var ptr = new PointerGestureRecognizer();
        ptr.PointerEntered += (_, _) =>
        {
            border.BackgroundColor = ThumbHover;
            border.Stroke = ThumbStrokeHover;
        };
        ptr.PointerExited += (_, _) =>
        {
            border.BackgroundColor = ThumbNormal;
            border.Stroke = ThumbStrokeNormal;
        };
        border.GestureRecognizers.Add(ptr);
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
