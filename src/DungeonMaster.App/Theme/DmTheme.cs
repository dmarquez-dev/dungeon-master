namespace DungeonMaster.App.Theme;

/// <summary>
///		Provides typed access to the theme resources defined under Resources/Theme.
///		Every member resolves its value from the application's merged resource dictionaries
///		at the moment it is read, so the dictionaries remain the single source of truth.
///		Resolution throws on a missing or mistyped key rather than falling back, because a
///		silent fallback would render a transparent brush that is hard to trace to its cause.
/// </summary>
/// <remarks>
///		LIFETIME: every member here reads <c>Application.Current.Resources</c>. That object
///		does not exist until the MAUI <c>App</c> constructor has run <c>InitializeComponent</c>,
///		so no static field initialiser, static constructor or <c>BindableProperty.Create</c>
///		default may call into this class. Those sites run at type-initialisation time, which
///		can precede application startup, and they will throw with a message that reads like a
///		missing key when the real fault is the ordering. Read these properties from runtime
///		code only.
/// </remarks>
public static class DmTheme
{
	/// <summary>
	///		Gets the active tool and focus highlight gold.
	///		Also used as the hover stroke on object-palette thumbnails and as the WinUI
	///		scrollbar thumb fill on pointer-over. <see cref="ScrollGoldLight"/> resolves the same
	///		token; see the note there before trying to move the two apart.
	/// </summary>
	public static Color Gold =>
		Resolve<Color>("DmAccentGoldLight");

	/// <summary>
	///		Gets the inactive tool and muted label colour on tavern surfaces.
	/// </summary>
	public static Color GoldMuted =>
		Resolve<Color>("DmTextTavernMuted");

	/// <summary>
	///		Gets the gold used for scroll and border glow effects.
	/// </summary>
	public static Color ScrollGold =>
		Resolve<Color>("DmAccentGold");

	/// <summary>
	///		Gets the subtle gold hover tint background.
	/// </summary>
	public static Color GoldHoverTint =>
		Resolve<Color>("DmGoldHoverTint");

	/// <summary>
	///		Gets the selected area border gold.
	/// </summary>
	public static Color AreaSelectedBorder =>
		Resolve<Color>("DmAreaSelectedBorder");

	/// <summary>
	///		Gets the selected area background tint.
	/// </summary>
	public static Color AreaSelectedTint =>
		Resolve<Color>("DmAreaSelectedTint");

	/// <summary>
	///		Gets the floor card surface colour in its normal state.
	/// </summary>
	public static Color FloorSurfaceNormal =>
		Resolve<Color>("DmSurfaceTavernRaised");

	/// <summary>
	///		Gets the floor card surface colour in its hover state.
	/// </summary>
	public static Color FloorSurfaceHover =>
		Resolve<Color>("DmSurfaceTavernRaisedHover");

	/// <summary>
	///		Gets the floor card surface colour in its expanded state.
	/// </summary>
	public static Color FloorSurfaceActive =>
		Resolve<Color>("DmSurfaceTavernRaisedActive");

	/// <summary>
	///		Gets the floor card border brush in its normal state.
	/// </summary>
	public static Brush FloorBorderNormal =>
		Resolve<Brush>("DmBorderTavernBrush");

	/// <summary>
	///		Gets the floor card border brush in its active state.
	/// </summary>
	public static Brush FloorBorderActive =>
		Resolve<Brush>("DmBorderTavernHoverBrush");

	/// <summary>
	///		Gets the save action border and stroke green.
	/// </summary>
	public static Color SaveGreen =>
		Resolve<Color>("DmSave");

	/// <summary>
	///		Gets the save action text and icon green.
	/// </summary>
	public static Color SaveGreenText =>
		Resolve<Color>("DmSaveText");

	/// <summary>
	///		Gets the discard action border and stroke red.
	/// </summary>
	public static Color DiscardRed =>
		Resolve<Color>("DmDiscard");

	/// <summary>
	///		Gets the discard action text and icon red.
	/// </summary>
	public static Color DiscardRedText =>
		Resolve<Color>("DmDiscardText");

	/// <summary>
	///		Gets the drag-and-drop placeholder stroke colour.
	/// </summary>
	public static Color DragPlaceholderStroke =>
		Resolve<Color>("DmDragPlaceholderStroke");

	/// <summary>
	///		Gets the drag-and-drop placeholder background tint.
	/// </summary>
	public static Color DragPlaceholderFill =>
		Resolve<Color>("DmDragPlaceholderFill");

	/// <summary>
	///		Gets the focus and hover glow tint used by inline editors.
	/// </summary>
	public static Color EditorGlowTint =>
		Resolve<Color>("DmEditorGlowTint");

	/// <summary>
	///		Gets the notebook icon gold shown when an entity has authored content.
	///		This resolves the SAME token as <see cref="Gold"/>, not merely the same value, so the
	///		two cannot be moved apart by editing a value: splitting them means adding a token to
	///		Tokens.xaml and repointing this member at it, the way DmMapObjectPlaceholder was added
	///		for <see cref="MapObjectPlaceholder"/>. The second name is kept because the call sites
	///		read differently — this one is a content indicator, <see cref="Gold"/> is the active
	///		tool and focus highlight — but the seam is nominal until that token exists.
	/// </summary>
	public static Color ScrollGoldLight =>
		Resolve<Color>("DmAccentGoldLight");

	/// <summary>
	///		Gets the muted parchment ink used for de-emphasised glyphs and labels.
	///		Its uses are the notebook icon when an entity has no authored content, the
	///		notebook close button at rest, and the object-palette expander ellipsis.
	/// </summary>
	public static Color InkMuted =>
		Resolve<Color>("DmTextParchmentMuted");

	/// <summary>
	///		Gets the dark-tavern primary text gold.
	///		Also used as the WinUI scrollbar thumb fill in its pressed state, which is the
	///		brightest step of that control's three-stop ramp.
	/// </summary>
	public static Color TextTavern =>
		Resolve<Color>("DmTextTavern");

	/// <summary>
	///		Gets the destructive action red.
	/// </summary>
	public static Color Destructive =>
		Resolve<Color>("DmDestructive");

	/// <summary>
	///		Gets the faint red wash behind a hovered destructive icon button.
	/// </summary>
	public static Color DestructiveHoverSurface =>
		Resolve<Color>("DmDestructiveHoverSurface");

	/// <summary>
	///		Gets the notebook's "saved" stamp green.
	/// </summary>
	public static Color SavedStamp =>
		Resolve<Color>("DmSavedStamp");

	/// <summary>
	///		Gets the parchment ink used for text on a tool-panel surface.
	/// </summary>
	public static Color TextParchment =>
		Resolve<Color>("DmTextParchment");

	/// <summary>
	///		Gets the tool-palette chip surface in its normal state.
	/// </summary>
	public static Color SurfaceToolChip =>
		Resolve<Color>("DmSurfaceToolChip");

	/// <summary>
	///		Gets the tool-palette chip surface in its hover state.
	/// </summary>
	public static Color SurfaceToolChipHover =>
		Resolve<Color>("DmSurfaceToolChipHover");

	/// <summary>
	///		Gets the tool-palette chip border in its normal state.
	/// </summary>
	public static Color BorderToolChip =>
		Resolve<Color>("DmBorderToolChip");

	/// <summary>
	///		Gets the tool-palette panel surface.
	/// </summary>
	public static Color SurfaceToolPanel =>
		Resolve<Color>("DmSurfaceToolPanel");

	/// <summary>
	///		Gets the text colour used on a tool-panel surface.
	/// </summary>
	public static Color TextToolPanel =>
		Resolve<Color>("DmTextToolPanel");

	/// <summary>
	///		Gets the WinUI scrollbar thumb fill.
	///		Shares its value with <see cref="TextSelection"/> today, but is deliberately a
	///		separate member: the scrollbar thumb and the text-selection wash are independent
	///		design decisions that a future restyle may want to move apart. Tokens.xaml lists
	///		the pair under "colours shared across tokens".
	/// </summary>
	public static Color ScrollbarThumb =>
		Resolve<Color>("DmScrollbarThumb");

	/// <summary>
	///		Gets the WinUI scrollbar thumb fill in its disabled state.
	/// </summary>
	public static Color ScrollbarThumbDisabled =>
		Resolve<Color>("DmScrollbarThumbDisabled");

	/// <summary>
	///		Gets the WinUI text selection highlight.
	///		Shares its value with <see cref="ScrollbarThumb"/> today, but is deliberately a
	///		separate member for the same reason given there.
	/// </summary>
	public static Color TextSelection =>
		Resolve<Color>("DmTextSelection");

	/// <summary>
	///		Gets the WinUI combo item surface in its hover state.
	/// </summary>
	public static Color SurfaceComboItemHover =>
		Resolve<Color>("DmSurfaceComboItemHover");

	/// <summary>
	///		Gets the WinUI combo item surface in its pressed state.
	/// </summary>
	public static Color SurfaceComboItemPressed =>
		Resolve<Color>("DmSurfaceComboItemPressed");

	/// <summary>
	///		Gets the WinUI combo item text colour in its disabled state.
	/// </summary>
	public static Color TextComboItemDisabled =>
		Resolve<Color>("DmTextComboItemDisabled");

	/// <summary>
	///		Gets the object action popup button gold wash in its hover state.
	/// </summary>
	public static Color PopupButtonHover =>
		Resolve<Color>("DmPopupButtonHover");

	/// <summary>
	///		Gets the object action popup button gold wash in its pressed state.
	/// </summary>
	public static Color PopupButtonPressed =>
		Resolve<Color>("DmPopupButtonPressed");

	/// <summary>
	///		Gets the object action popup button destructive wash in its hover state.
	/// </summary>
	public static Color PopupButtonDestructiveHover =>
		Resolve<Color>("DmPopupButtonDestructiveHover");

	/// <summary>
	///		Gets the object action popup button destructive wash in its pressed state.
	/// </summary>
	public static Color PopupButtonDestructivePressed =>
		Resolve<Color>("DmPopupButtonDestructivePressed");

	/// <summary>
	///		Gets the map canvas grid line colour.
	/// </summary>
	public static Color MapGrid =>
		Resolve<Color>("DmMapGrid");

	/// <summary>
	///		Gets the map canvas committed area outline colour.
	///		Shares its value with <see cref="Gold"/>, <see cref="ScrollGoldLight"/> and
	///		<see cref="MapObjectPlaceholder"/> today, but is deliberately a separate member:
	///		the map's area colour and the UI accent gold are independent design decisions
	///		that a future restyle may want to move apart.
	/// </summary>
	public static Color MapArea =>
		Resolve<Color>("DmMapArea");

	/// <summary>
	///		Gets the map canvas outline vertex handle colour.
	/// </summary>
	public static Color MapVertex =>
		Resolve<Color>("DmMapVertex");

	/// <summary>
	///		Gets the fill of the area currently being drawn on the map canvas.
	/// </summary>
	public static Color MapAreaFillActive =>
		Resolve<Color>("DmMapAreaFillActive");

	/// <summary>
	///		Gets the stroke of the area currently being drawn on the map canvas.
	/// </summary>
	public static Color MapAreaStrokeActive =>
		Resolve<Color>("DmMapAreaStrokeActive");

	/// <summary>
	///		Gets the stand-in rendering colour for an object with no drawable image.
	///		It paints the "?" fallback glyph and outline and the dashed ghost-placement
	///		outline. Shares its value with <see cref="MapArea"/> and the UI golds today, but
	///		is deliberately separate because these are object-placeholder renderings, not
	///		area outlines or UI affordances, and a restyle may want to move them apart.
	/// </summary>
	public static Color MapObjectPlaceholder =>
		Resolve<Color>("DmMapObjectPlaceholder");

	/// <summary>
	///		Gets the body font family.
	/// </summary>
	public static string FontBody =>
		Resolve<string>("DmFontBody");

	/// <summary>
	///		Gets the body ramp's default size.
	/// </summary>
	public static double FontSizeBody =>
		Resolve<double>("DmFontSizeBody");

	/// <summary>
	///		Gets the body ramp's caption size.
	/// </summary>
	public static double FontSizeCaption =>
		Resolve<double>("DmFontSizeCaption");

	/// <summary>
	///		Gets the body ramp's metadata size.
	/// </summary>
	public static double FontSizeMeta =>
		Resolve<double>("DmFontSizeMeta");

	/// <summary>
	///		Gets the smallest size on the type scale.
	/// </summary>
	public static double FontSizeMicro =>
		Resolve<double>("DmFontSizeMicro");

#if WINDOWS
	/// <summary>
	///		Converts a MAUI colour to the WinUI colour type used by native resource dictionaries.
	/// </summary>
	/// <param name="color">
	///		The MAUI colour to convert.
	/// </param>
	/// <returns>
	///		The equivalent <see cref="Windows.UI.Color"/>.
	/// </returns>
	/// <remarks>
	///		Each channel is clamped to 0..1 and rounded to nearest, so a channel that does not
	///		land on k/255 converts to its nearest 8-bit neighbour: an alpha of 0.5f yields 128
	///		rather than the 127 a truncating cast would produce. Every colour in the theme
	///		dictionaries is parsed from 6- or 8-digit hex and so already lands on k/255, but the
	///		helper is also handed derived colours such as <c>WithAlpha</c> results, which do not.
	/// </remarks>
	public static Windows.UI.Color ToPlatform(Color color)
	{
		return Windows.UI.Color.FromArgb(
			Channel(color.Alpha),
			Channel(color.Red),
			Channel(color.Green),
			Channel(color.Blue));
	}

	private static byte Channel(float value)
	{
		return (byte)Math.Round(
			Math.Clamp(value, 0f, 1f) * 255f,
			MidpointRounding.AwayFromZero);
	}
#endif

	/// <summary>
	///		Resolves a keyed resource from the application's merged dictionaries.
	/// </summary>
	/// <param name="key">
	///		The resource key to look up.
	/// </param>
	/// <exception cref="InvalidOperationException">
	///		Thrown when the application resources are not yet available, when the key is absent,
	///		or when the key is bound to a value of a different type.
	/// </exception>
	private static T Resolve<T>(string key)
	{
		// Fully qualified: the DungeonMaster.Application namespace also binds "Application" here.
		var resources = Microsoft.Maui.Controls.Application.Current?.Resources;

		if (resources?.TryGetValue(key, out var value) is true && value is T typed)
		{
			return typed;
		}

		throw new InvalidOperationException(
			$"Theme resource '{key}' was not found as a {typeof(T).Name}. "
			+ "Either the key is missing from Resources/Theme, or this read ran before "
			+ "Application.Current.Resources existed — see the remarks on DmTheme.");
	}
}
