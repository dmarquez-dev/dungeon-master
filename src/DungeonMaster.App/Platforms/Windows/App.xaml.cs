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
		resources["ScrollBarThumbFillBrush"] = ToBrush(DmTheme.ScrollbarThumb);
		resources["ScrollBarThumbFillBrushPointerOver"] = ToBrush(DmTheme.Gold);
		resources["ScrollBarThumbFillBrushPressed"] = ToBrush(DmTheme.TextTavern);
		resources["ScrollBarThumbFillBrushDisabled"] = ToBrush(DmTheme.ScrollbarThumbDisabled);

		// Text selection highlight — amber/parchment instead of system purple.
		resources["TextControlSelectionHighlightColor"] = ToBrush(DmTheme.SurfaceTextSelection);

		// Parchment ComboBox/Picker theme — same values for Light and Dark so the dropdown
		// always matches the palette regardless of system theme.
		//
		// These assignments REPLACE any existing entry rather than merging into it. That is safe
		// only because Platforms/Windows/App.xaml is deliberately empty and nothing else
		// populates ThemeDictionaries; if a WinUI control library is ever added, merge instead.
		resources.ThemeDictionaries["Light"] = ComboBoxTheme();
		resources.ThemeDictionaries["Dark"] = ComboBoxTheme();

		// HighContrast gets the shared entries only, omitting the pressed, disabled and
		// unselected-item entries so the system can supply its own high-contrast values.
		resources.ThemeDictionaries["HighContrast"] = ComboBoxSharedEntries();
	}

	private static ResourceDictionary ComboBoxTheme()
	{
		var dictionary = ComboBoxSharedEntries();

		dictionary["ComboBoxItemForegroundPressed"] = ToBrush(DmTheme.TextToolPanel);
		dictionary["ComboBoxItemForegroundDisabled"] = ToBrush(DmTheme.TextComboItemDisabled);
		dictionary["ComboBoxItemBackground"] = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
		dictionary["ComboBoxItemBackgroundPressed"] = ToBrush(DmTheme.SurfaceComboItemPressed);

		return dictionary;
	}

	private static ResourceDictionary ComboBoxSharedEntries()
	{
		return new ResourceDictionary
		{
			["ComboBoxDropDownBackground"] = ToBrush(DmTheme.SurfaceToolPanel),
			["ComboBoxDropDownBorderBrush"] = ToBrush(DmTheme.BorderToolChip),
			["ComboBoxDropDownGlyphForeground"] = ToBrush(DmTheme.TextToolPanel),
			["ComboBoxDropDownGlyphForegroundFocused"] = ToBrush(DmTheme.TextToolPanel),
			["ComboBoxItemForeground"] = ToBrush(DmTheme.TextToolPanel),
			["ComboBoxItemForegroundPointerOver"] = ToBrush(DmTheme.TextToolPanel),
			["ComboBoxItemForegroundSelected"] = ToBrush(DmTheme.TextToolPanel),
			["ComboBoxItemForegroundSelectedUnfocused"] = ToBrush(DmTheme.TextToolPanel),
			["ComboBoxItemBackgroundPointerOver"] = ToBrush(DmTheme.SurfaceComboItemHover),
			["ComboBoxItemBackgroundSelected"] = ToBrush(DmTheme.SurfaceToolChip),
			["ComboBoxItemBackgroundSelectedPointerOver"] = ToBrush(DmTheme.SurfaceComboItemHover),
			["ComboBoxItemBackgroundSelectedUnfocused"] = ToBrush(DmTheme.SurfaceToolChip),
		};
	}

	private static SolidColorBrush ToBrush(Microsoft.Maui.Graphics.Color color)
	{
		return new SolidColorBrush(DmTheme.ToPlatform(color));
	}
}
