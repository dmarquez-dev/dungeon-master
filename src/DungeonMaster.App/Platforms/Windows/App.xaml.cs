using DungeonMaster.App.Theme;
using Microsoft.UI.Xaml;

// Both MAUI and WinUI define these names and both namespaces are in scope in this file.
using ResourceDictionary = Microsoft.UI.Xaml.ResourceDictionary;
using SolidColorBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DungeonMaster.App.WinUI;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : MauiWinUIApplication
{
	/// <summary>
	/// Initializes the singleton application object.  This is the first line of authored code
	/// executed, and as such is the logical equivalent of main() or WinMain().
	/// </summary>
	public App()
	{
		this.InitializeComponent();
	}

	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

	/// <summary>
	///		Applies the theme resource overrides before the first window is created.
	///		This cannot run in the constructor: <c>Application.Resources</c> is not yet
	///		available there and its getter fails with <c>E_FAIL</c>.
	/// </summary>
	/// <param name="args">
	///		The launch arguments forwarded to the base implementation.
	/// </param>
	protected override void OnLaunched(LaunchActivatedEventArgs args)
	{
		ApplyThemeResources();

		base.OnLaunched(args);
	}

	/// <summary>
	///		Overrides the WinUI system brushes that the parchment theme replaces.
	///		These live in code rather than in App.xaml because that file is parsed by WinUI:
	///		MAUI's merged theme dictionaries are not in scope there, so the tokens have to
	///		arrive through <see cref="DmColors"/> and <see cref="DmColors.ToPlatform"/>.
	/// </summary>
	private void ApplyThemeResources()
	{
		// Scrollbar thumb — gold palette.
		this.Resources["ScrollBarThumbFillBrush"] = Brush(DmColors.ScrollbarThumb);
		this.Resources["ScrollBarThumbFillBrushPointerOver"] = Brush(DmColors.Gold);
		this.Resources["ScrollBarThumbFillBrushPressed"] = Brush(DmColors.TextTavern);
		this.Resources["ScrollBarThumbFillBrushDisabled"] = Brush(DmColors.ScrollbarThumbDisabled);

		// Text selection highlight — amber/parchment instead of system purple.
		this.Resources["TextControlSelectionHighlightColor"] = Brush(DmColors.TextSelection);

		// Parchment ComboBox/Picker theme — same values for Light and Dark so the dropdown
		// always matches the palette regardless of system theme. HighContrast deliberately
		// omits the pressed, disabled and unselected-item entries so the system can supply
		// its own high-contrast values for them.
		this.Resources.ThemeDictionaries["Light"] = ComboBoxTheme();
		this.Resources.ThemeDictionaries["Dark"] = ComboBoxTheme();
		this.Resources.ThemeDictionaries["HighContrast"] = ComboBoxHighContrastTheme();
	}

	private static ResourceDictionary ComboBoxTheme()
	{
		var dictionary = ComboBoxSharedEntries();

		dictionary["ComboBoxItemForegroundPressed"] = Brush(DmColors.TextToolPanel);
		dictionary["ComboBoxItemForegroundDisabled"] = Brush(DmColors.TextComboItemDisabled);
		dictionary["ComboBoxItemBackground"] = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
		dictionary["ComboBoxItemBackgroundPressed"] = Brush(DmColors.SurfaceComboItemPressed);

		return dictionary;
	}

	private static ResourceDictionary ComboBoxHighContrastTheme()
	{
		return ComboBoxSharedEntries();
	}

	private static ResourceDictionary ComboBoxSharedEntries()
	{
		return new ResourceDictionary
		{
			["ComboBoxDropDownBackground"] = Brush(DmColors.SurfaceToolPanel),
			["ComboBoxDropDownBorderBrush"] = Brush(DmColors.BorderToolChip),
			["ComboBoxDropDownGlyphForeground"] = Brush(DmColors.TextToolPanel),
			["ComboBoxDropDownGlyphForegroundFocused"] = Brush(DmColors.TextToolPanel),
			["ComboBoxItemForeground"] = Brush(DmColors.TextToolPanel),
			["ComboBoxItemForegroundPointerOver"] = Brush(DmColors.TextToolPanel),
			["ComboBoxItemForegroundSelected"] = Brush(DmColors.TextToolPanel),
			["ComboBoxItemForegroundSelectedUnfocused"] = Brush(DmColors.TextToolPanel),
			["ComboBoxItemBackgroundPointerOver"] = Brush(DmColors.SurfaceComboItemHover),
			["ComboBoxItemBackgroundSelected"] = Brush(DmColors.SurfaceToolChip),
			["ComboBoxItemBackgroundSelectedPointerOver"] = Brush(DmColors.SurfaceComboItemHover),
			["ComboBoxItemBackgroundSelectedUnfocused"] = Brush(DmColors.SurfaceToolChip),
		};
	}

	private static SolidColorBrush Brush(Microsoft.Maui.Graphics.Color color)
	{
		return new SolidColorBrush(DmColors.ToPlatform(color));
	}
}
