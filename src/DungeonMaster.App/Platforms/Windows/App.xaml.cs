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
	///		Overrides the WinUI system brushes that the parchment theme replaces.
	///		These live in code rather than in App.xaml because that file is parsed by WinUI:
	///		MAUI's merged theme dictionaries are not in scope there, so the tokens have to
	///		arrive through <see cref="DmTheme"/> and <see cref="DmTheme.ToPlatform"/>.
	/// </summary>
	/// <remarks>
	///		CALL SITE IS DELIBERATE. This runs from the MAUI <c>App</c> constructor, and the
	///		window either side of that point is narrow. Earlier — in this class's constructor
	///		— <c>Application.Resources</c> is not yet available and its getter fails with
	///		<c>E_FAIL</c>. Earlier than the MAUI <c>App</c> constructor in any form, including
	///		the top of <c>OnLaunched</c> where this used to sit, <c>DmTheme</c> cannot resolve:
	///		MAUI's <c>Application.Current</c> and its merged dictionaries are created inside
	///		<c>MauiWinUIApplication.OnLaunched</c>, so every <c>DmTheme</c> read would throw.
	///		Later, the first window has already been created. The MAUI <c>App</c> constructor,
	///		after <c>InitializeComponent</c>, is the one point that satisfies both ends.
	/// </remarks>
	internal static void ApplyThemeResources()
	{
		var resources = Microsoft.UI.Xaml.Application.Current.Resources;

		// Scrollbar thumb — gold palette.
		resources["ScrollBarThumbFillBrush"] = Brush(DmTheme.ScrollbarThumb);
		resources["ScrollBarThumbFillBrushPointerOver"] = Brush(DmTheme.Gold);
		resources["ScrollBarThumbFillBrushPressed"] = Brush(DmTheme.TextTavern);
		resources["ScrollBarThumbFillBrushDisabled"] = Brush(DmTheme.ScrollbarThumbDisabled);

		// Text selection highlight — amber/parchment instead of system purple.
		resources["TextControlSelectionHighlightColor"] = Brush(DmTheme.TextSelection);

		// Parchment ComboBox/Picker theme — same values for Light and Dark so the dropdown
		// always matches the palette regardless of system theme. HighContrast deliberately
		// omits the pressed, disabled and unselected-item entries so the system can supply
		// its own high-contrast values for them.
		resources.ThemeDictionaries["Light"] = ComboBoxTheme();
		resources.ThemeDictionaries["Dark"] = ComboBoxTheme();
		resources.ThemeDictionaries["HighContrast"] = ComboBoxHighContrastTheme();
	}

	private static ResourceDictionary ComboBoxTheme()
	{
		var dictionary = ComboBoxSharedEntries();

		dictionary["ComboBoxItemForegroundPressed"] = Brush(DmTheme.TextToolPanel);
		dictionary["ComboBoxItemForegroundDisabled"] = Brush(DmTheme.TextComboItemDisabled);
		dictionary["ComboBoxItemBackground"] = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
		dictionary["ComboBoxItemBackgroundPressed"] = Brush(DmTheme.SurfaceComboItemPressed);

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
			["ComboBoxDropDownBackground"] = Brush(DmTheme.SurfaceToolPanel),
			["ComboBoxDropDownBorderBrush"] = Brush(DmTheme.BorderToolChip),
			["ComboBoxDropDownGlyphForeground"] = Brush(DmTheme.TextToolPanel),
			["ComboBoxDropDownGlyphForegroundFocused"] = Brush(DmTheme.TextToolPanel),
			["ComboBoxItemForeground"] = Brush(DmTheme.TextToolPanel),
			["ComboBoxItemForegroundPointerOver"] = Brush(DmTheme.TextToolPanel),
			["ComboBoxItemForegroundSelected"] = Brush(DmTheme.TextToolPanel),
			["ComboBoxItemForegroundSelectedUnfocused"] = Brush(DmTheme.TextToolPanel),
			["ComboBoxItemBackgroundPointerOver"] = Brush(DmTheme.SurfaceComboItemHover),
			["ComboBoxItemBackgroundSelected"] = Brush(DmTheme.SurfaceToolChip),
			["ComboBoxItemBackgroundSelectedPointerOver"] = Brush(DmTheme.SurfaceComboItemHover),
			["ComboBoxItemBackgroundSelectedUnfocused"] = Brush(DmTheme.SurfaceToolChip),
		};
	}

	private static SolidColorBrush Brush(Microsoft.Maui.Graphics.Color color)
	{
		return new SolidColorBrush(DmTheme.ToPlatform(color));
	}
}
