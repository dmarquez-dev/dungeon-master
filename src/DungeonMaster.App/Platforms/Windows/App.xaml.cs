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

		// Text selection highlight — amber/parchment instead of system purple.
		resources["TextControlSelectionHighlightColor"] = ToBrush(DmTheme.SurfaceTextSelection);

		// Scrollbar thumb — gold palette. Written at the application level AND into every theme
		// dictionary below, because the two halves of this are separate faults and either one
		// alone leaves the thumb painting the WinUI default.
		Merge(
			resources,
			ScrollbarEntries());

		// Parchment ComboBox/Picker theme — same values for Light and Dark so the dropdown
		// always matches the palette regardless of system theme.
		//
		// MERGED, not replaced. An assignment to ThemeDictionaries[theme] discards whatever was
		// already there, and WinUI's own generic dictionaries are already there — so replacing
		// "Light" with a ComboBox-only dictionary silently removed every other Light entry for
		// any element whose RequestedTheme is Light. ObjectPaletteView.xaml.cs sets exactly that
		// on the palette's ScrollViewer, which is why the scrollbar override reached nothing even
		// once the key names were corrected.
		MergeTheme(
			resources,
			"Light",
			ComboBoxTheme(),
			ScrollbarEntries());

		MergeTheme(
			resources,
			"Dark",
			ComboBoxTheme(),
			ScrollbarEntries());

		// HighContrast gets the shared entries only, omitting the pressed, disabled and
		// unselected-item entries so the system can supply its own high-contrast values. The
		// scrollbar is left to the system there for the same reason.
		MergeTheme(
			resources,
			"HighContrast",
			ComboBoxSharedEntries());
	}

	/// <summary>
	///		The WinUI scrollbar keys.
	/// </summary>
	/// <remarks>
	///		<para>
	///		The thumb names are <c>ScrollBarThumbFill*</c>. They were
	///		<c>ScrollBarThumbFillBrush*</c> here and in <c>Platforms/Windows/App.xaml</c> before
	///		it, and no such key exists in WinUI — verified against the shipped string tables in
	///		<c>Microsoft.UI.Xaml.Controls.pri</c> and <c>Microsoft.ui.xaml.resources.19h1.dll</c>,
	///		where all four <c>…FillBrush…</c> forms miss and all four <c>…Fill…</c> forms hit.
	///		</para>
	///		<para>
	///		WHAT ACTUALLY PAINTS, MEASURED: the resting scrollbar — the thin bar a user sees
	///		almost all of the time — is the ScrollBar's PANNING thumb, and it reads
	///		<c>ScrollBarPanningThumbBackground</c>. That is the pixel every previous measurement
	///		of this defect sampled: the object palette's bar at (231, 800) was
	///		<c>0x6F5F42</c>, which is neither <c>DmScrollbarThumb</c> nor anything derived from it
	///		— it is the WinUI default, black at ~45% over the parchment. Setting
	///		<c>ScrollBarPanningThumbBackground</c> here was proved to reach it by writing a
	///		saturated magenta and sampling the same pixel.
	///		</para>
	///		<para>
	///		THE EXPANDED THUMB IS STILL NOT REACHED, and the four <c>ScrollBarThumbFill*</c>
	///		entries below are currently inert. Hovering expands the bar and the thumb paints
	///		<c>0x87847D</c> on a <c>0xF4EEE1</c> track — the WinUI defaults — and it stays those
	///		values when <c>ScrollBarThumbFill</c> is set to pure red at ALL THREE scopes: the
	///		application dictionary, <c>ThemeDictionaries["Light"]</c>, and the palette
	///		ScrollViewer's OWN <c>Resources</c>. An element-level entry is the highest-precedence
	///		scope there is, so the expanded thumb in this Windows App SDK build does not read
	///		<c>ScrollBarThumbFill</c> at all. The correct names are kept because they ARE the
	///		correct names and because the entries cost nothing; what is NOT claimed is that they
	///		do anything. Do not delete this paragraph without re-measuring.
	///		</para>
	/// </remarks>
	private static ResourceDictionary ScrollbarEntries()
	{
		return new ResourceDictionary
		{
			// Proved to paint.
			["ScrollBarPanningThumbBackground"] = ToBrush(DmTheme.ScrollbarThumb),
			["ScrollBarPanningThumbBackgroundDisabled"] = ToBrush(DmTheme.ScrollbarThumbDisabled),

			// Correctly named, currently inert — see the remarks above.
			["ScrollBarThumbFill"] = ToBrush(DmTheme.ScrollbarThumb),
			["ScrollBarThumbFillPointerOver"] = ToBrush(DmTheme.Gold),
			["ScrollBarThumbFillPressed"] = ToBrush(DmTheme.TextTavern),
			["ScrollBarThumbFillDisabled"] = ToBrush(DmTheme.ScrollbarThumbDisabled),
		};
	}

	/// <summary>
	///		Copies every entry of each source into the named theme dictionary, creating it only if
	///		it is absent. Nothing already present under that theme is discarded.
	/// </summary>
	private static void MergeTheme(
		ResourceDictionary resources,
		string theme,
		params ResourceDictionary[] sources)
	{
		if (!resources.ThemeDictionaries.TryGetValue(
				theme,
				out var existing)
			|| existing is not ResourceDictionary target)
		{
			target = new ResourceDictionary();
			resources.ThemeDictionaries[theme] = target;
		}

		Merge(
			target,
			sources);
	}

	private static void Merge(
		ResourceDictionary target,
		params ResourceDictionary[] sources)
	{
		foreach (var source in sources)
		{
			foreach (var entry in source)
			{
				target[entry.Key] = entry.Value;
			}
		}
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
