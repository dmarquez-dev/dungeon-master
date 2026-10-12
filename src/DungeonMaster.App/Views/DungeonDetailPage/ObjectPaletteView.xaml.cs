using DungeonMaster.App.Assets.ObjectAssets;
using DungeonMaster.App.Theme;
using DungeonMaster.Domain.Entities.Floors;

namespace DungeonMaster.App.Views.DungeonDetail;

/// <summary>
///		The primary object palette, which offers the available object packs in a picker and the objects of the loaded pack as rows of category chips.
/// </summary>
public partial class ObjectPaletteView : ContentView
{
	private IReadOnlyList<string> _categoryOrder = [];
	private IReadOnlyDictionary<string, IReadOnlyList<ObjectKind>> _categories =
		new Dictionary<string, IReadOnlyList<ObjectKind>>();

	/// <summary>
	///		Gets the object kind the palette currently has selected, or <see langword="null"/> when nothing is selected.
	/// </summary>
	public ObjectKind? ActiveObject { get; private set; }

	/// <summary>
	///		Raised whenever <see cref="ActiveObject"/> changes, carrying the new selection or <see langword="null"/> when the selection has been cleared.
	///		The selection is cleared from two places: <see cref="ClearActiveObject"/> and the handler behind the palette's own close button.
	/// </summary>
	public event EventHandler<ObjectKind?>? ActiveObjectChanged;

	/// <summary>
	///		Raised when the palette's close button is tapped, after the active object has been cleared.
	/// </summary>
	public event EventHandler? CloseRequested;

	/// <summary>
	///		Raised when a category's expander chip is tapped, carrying the secondary palette the host should show.
	///		That palette has already been populated for the category by the time this is raised.
	/// </summary>
	public event EventHandler<SecondaryPaletteView>? SecondaryPaletteRequested;

	/// <summary>
	///		Raised when the secondary palette should be hidden.
	/// </summary>
	public event EventHandler? SecondaryPaletteDismissed;

	/// <summary>
	///		Raised after an object pack has finished loading and its asset paths have been published to <see cref="ObjectAssetCatalogue"/>.
	/// </summary>
	public event EventHandler? PackChanged;

	/// <summary>
	///		Raised when loading an object pack fails. This view has no view model of its own,
	///		so the message is handed to <c>DungeonDetailPage</c>, which routes it to
	///		<c>DungeonDetailViewModel.ErrorMessage</c>.
	/// </summary>
	public event EventHandler<string>? LoadFailed;

	/// <summary>
	///		Initialises a new instance of <see cref="ObjectPaletteView"/>, filling the pack picker with the display names of the known object packs.
	///		Loading a pack's objects is deferred until the view is loaded.
	/// </summary>
	public ObjectPaletteView()
	{
		InitializeComponent();

		// Populate the pack picker with known packs.
		foreach (var (_, displayName) in ObjectPackRegistry.AvailablePacks)
		{
			PackPicker.Items.Add(displayName);
		}

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
				var border = new Microsoft.UI.Xaml.Media.SolidColorBrush(DmTheme.ToPlatform(DmTheme.BorderToolChip));
				var bg = new Microsoft.UI.Xaml.Media.SolidColorBrush(DmTheme.ToPlatform(DmTheme.SurfaceToolChip));
				var popup = new Microsoft.UI.Xaml.Media.SolidColorBrush(DmTheme.ToPlatform(DmTheme.SurfaceToolPanel));
				var text = new Microsoft.UI.Xaml.Media.SolidColorBrush(DmTheme.ToPlatform(DmTheme.TextToolPanel));
				var hover = new Microsoft.UI.Xaml.Media.SolidColorBrush(DmTheme.ToPlatform(DmTheme.SurfaceComboItemHover));
				var pressed = new Microsoft.UI.Xaml.Media.SolidColorBrush(DmTheme.ToPlatform(DmTheme.SurfaceComboItemPressed));
				var transp = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);

				var d = new Microsoft.UI.Xaml.ResourceDictionary();
				d["ComboBoxDropDownBackground"] = popup;
				d["ComboBoxDropDownBorderBrush"] = border;
				d["ComboBoxDropDownGlyphForeground"] = text;
				d["ComboBoxDropDownGlyphForegroundFocused"] = text;
				d["ComboBoxItemPillFillBrush"] = border; // accent pill → gold
				d["ComboBoxItemForeground"] = text;
				d["ComboBoxItemForegroundPointerOver"] = text;
				d["ComboBoxItemForegroundSelected"] = text;
				d["ComboBoxItemForegroundSelectedUnfocused"] = text;
				d["ComboBoxItemForegroundPressed"] = text;
				d["ComboBoxItemBackground"] = transp;
				d["ComboBoxItemBackgroundPointerOver"] = hover;
				d["ComboBoxItemBackgroundPressed"] = pressed;
				d["ComboBoxItemBackgroundSelected"] = bg;
				d["ComboBoxItemBackgroundSelectedPointerOver"] = hover;
				d["ComboBoxItemBackgroundSelectedUnfocused"] = bg;

				// Only the theme-dictionary attachment can fail in a way we choose to absorb: the
				// WinUI resource system rejects a duplicate or ill-typed key rather than degrading.
				// Anything wider — an OOM, or a bug in the tree walk below — must surface.
				try
				{
					cb.Resources.ThemeDictionaries["Light"] = d;
				}
				catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or System.Runtime.InteropServices.COMException)
				{
					System.Diagnostics.Debug.WriteLine($"ObjectPaletteView: failed to attach the parchment Light theme dictionary to the pack ComboBox. {ex}");
				}

				// Walk the closed-state visual tree to set ellipsis trimming on the header TextBlock.
				SetTextTrimmingOnChildren(cb, depth: 0);
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
		// async void: an escaping exception is UNOBSERVED and is rethrown on the
		// synchronisation context, which takes the process down. The try therefore opens
		// before the first await.
		try
		{
			if (ObjectPackRegistry.AvailablePacks.Count == 0)
			{
				return;
			}

			PackPicker.SelectedIndex = 0;
			await LoadPackAsync(ObjectPackRegistry.AvailablePacks[0].Id);
		}
		catch (Exception exception)
		{
			// No view model here, so the failure leaves on LoadFailed and DungeonDetailPage
			// puts it on DungeonDetailViewModel.ErrorMessage.
			LoadFailed?.Invoke(this, $"Could not load the object palette: {exception.Message}");
		}
	}

	private async void OnPackPickerChanged(object? sender, EventArgs e)
	{
		// async void: see OnLoaded. The try opens before the first await.
		try
		{
			var idx = PackPicker.SelectedIndex;
			if (idx < 0 || idx >= ObjectPackRegistry.AvailablePacks.Count)
			{
				return;
			}

			await LoadPackAsync(ObjectPackRegistry.AvailablePacks[idx].Id);
		}
		catch (Exception exception)
		{
			// No view model here, so the failure leaves on LoadFailed and DungeonDetailPage
			// puts it on DungeonDetailViewModel.ErrorMessage.
			LoadFailed?.Invoke(this, $"Could not switch object pack: {exception.Message}");
		}
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

	/// <summary>
	///		The chip for one object in the primary palette. The chip itself is
	///		<see cref="PaletteThumb"/>'s; this palette supplies only what is its own, which is that
	///		a tap selects the object directly rather than raising an event.
	/// </summary>
	private View BuildThumbButton(ObjectKind kind) =>
		PaletteThumb.Build(kind, SelectObject);

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

		// The same chip frame the object thumbnails use, so the expander cannot drift away from
		// the row it sits in. Everything below it is the expander's own and is not shared.
		var border = PaletteThumb.CreateShell(label);

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

	/// <summary>
	///		Makes the given object kind the active selection and raises <see cref="ActiveObjectChanged"/> with it.
	/// </summary>
	/// <param name="kind">
	///		The object kind to select.
	/// </param>
	public void SelectObject(ObjectKind kind)
	{
		ActiveObject = kind;
		ActiveObjectChanged?.Invoke(this, kind);
	}

	/// <summary>
	///		Clears the active selection and raises <see cref="ActiveObjectChanged"/> with <see langword="null"/>.
	/// </summary>
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
