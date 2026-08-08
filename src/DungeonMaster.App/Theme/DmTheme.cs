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
///		so nothing that runs at type-initialisation time may call into this class. Those sites
///		throw with a message that reads like a missing key when the real fault is the ordering.
///		The eager shapes, all four of which C# runs before startup can be guaranteed, are:
///		a static field initialiser; a static auto-property initialiser
///		(<c>static Color X { get; } = …</c>, which the compiler emits as a backing-field
///		initialiser); a <c>BindableProperty.Create</c> positional default, which is itself a
///		static field initialiser; and a static constructor body. Read these properties from
///		runtime code only — an expression-bodied member, a property with a <c>get</c> body, or a
///		<c>defaultValueCreator</c> lambda all defer the read to first use and are safe.
///		<c>DmThemeKeysResolveTests.NoEagerDmThemeRead</c> enforces this list;
///		if a shape is added here it must be added there too.
///		THREAD SAFETY: <c>ResourceDictionary</c> is not thread-safe, so these members are safe only
///		because nothing mutates the merged dictionaries after startup and every reader runs on the
///		UI thread; read them from the UI thread and add no post-startup mutation.
/// </remarks>
public static class DmTheme
{
	/// <summary>
	///		Gets the active tool and focus highlight gold.
	///		Also used as the hover stroke on object-palette thumbnails and as the WinUI
	///		scrollbar thumb fill on pointer-over. <see cref="ScrollGoldLight"/> resolves the same
	///		token; see the note there before trying to move the two apart. Shares its value with
	///		<see cref="AreaSelectedBorder"/>, <see cref="DragPlaceholderStroke"/>,
	///		<see cref="MapArea"/> and <see cref="MapObjectPlaceholder"/> today; see the
	///		shared-colour list at the top of Tokens.xaml.
	/// </summary>
	public static Color Gold =>
		Resolve<Color>("DmAccentGoldLight");

	/// <summary>
	///		Gets the inactive tool and muted label colour on tavern surfaces.
	/// </summary>
	public static Color GoldMuted =>
		Resolve<Color>("DmTextTavernMuted");

	/// <summary>
	///		Gets the subtle gold hover tint background.
	/// </summary>
	public static Color GoldHoverTint =>
		Resolve<Color>("DmGoldHoverTint");

	/// <summary>
	///		Gets the selected area border gold.
	///		Shares its value with <see cref="Gold"/>, <see cref="ScrollGoldLight"/>,
	///		<see cref="DragPlaceholderStroke"/>, <see cref="MapArea"/> and
	///		<see cref="MapObjectPlaceholder"/> today; see the shared-colour list at the top of
	///		Tokens.xaml.
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
	///		Gets the drag-and-drop placeholder stroke colour.
	///		Shares its value with <see cref="Gold"/>, <see cref="ScrollGoldLight"/>,
	///		<see cref="AreaSelectedBorder"/>, <see cref="MapArea"/> and
	///		<see cref="MapObjectPlaceholder"/> today; see the shared-colour list at the top of
	///		Tokens.xaml.
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
	///		tool and focus highlight — but the seam is nominal until that token exists. Also
	///		shares its value with <see cref="AreaSelectedBorder"/>,
	///		<see cref="DragPlaceholderStroke"/>, <see cref="MapArea"/> and
	///		<see cref="MapObjectPlaceholder"/> today; see the shared-colour list at the top of
	///		Tokens.xaml.
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
	///		brightest step of that control's three-stop ramp. Shares its value with
	///		<see cref="MapVertex"/> today; see the shared-colour list at the top of Tokens.xaml.
	/// </summary>
	public static Color TextTavern =>
		Resolve<Color>("DmTextTavern");

	/// <summary>
	///		Gets the destructive action red.
	/// </summary>
	public static Color Destructive =>
		Resolve<Color>("DmDestructive");

	/// <summary>
	///		Gets the text colour used on a destructive action surface.
	/// </summary>
	public static Color DestructiveText =>
		Resolve<Color>("DmDestructiveText");

	/// <summary>
	///		Gets the affirmative action green used by the confirmation popup's confirm button.
	/// </summary>
	public static Color Confirm =>
		Resolve<Color>("DmConfirm");

	/// <summary>
	///		Gets the text colour used on an affirmative action surface.
	/// </summary>
	public static Color ConfirmText =>
		Resolve<Color>("DmConfirmText");

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
	///		Shares its value with <see cref="Gold"/>, <see cref="ScrollGoldLight"/>,
	///		<see cref="AreaSelectedBorder"/>, <see cref="DragPlaceholderStroke"/> and
	///		<see cref="MapObjectPlaceholder"/> today, but is deliberately a separate member:
	///		the map's area colour and the UI accent gold are independent design decisions
	///		that a future restyle may want to move apart.
	/// </summary>
	public static Color MapArea =>
		Resolve<Color>("DmMapArea");

	/// <summary>
	///		Gets the map canvas outline vertex handle colour.
	///		Shares its value with <see cref="TextTavern"/> today; see the shared-colour list at
	///		the top of Tokens.xaml.
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
	///		outline. Shares its value with <see cref="Gold"/>, <see cref="ScrollGoldLight"/>,
	///		<see cref="AreaSelectedBorder"/>, <see cref="DragPlaceholderStroke"/> and
	///		<see cref="MapArea"/> today, but is deliberately separate because these are
	///		object-placeholder renderings, not area outlines or UI affordances, and a restyle
	///		may want to move them apart.
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

	// GEOMETRY. The three scales below exist for code-behind that builds layout imperatively —
	// new Grid { ColumnSpacing = … }, new RoundRectangle { CornerRadius = … } — which cannot
	// write {StaticResource}. XAML must keep using StaticResource: routing markup through here
	// would trade a compile-time-checked key for a runtime one. The geometry ratchets in
	// GeometryScaleTests scan *.cs as well as *.xaml, so a literal in either form is a failure
	// and these members are the only legal answer in C#.
	//
	// Every one is an expression-bodied member for the reason in the remarks above: geometry is
	// as unreadable before Application.Current.Resources exists as colour is.

	/// <summary>
	///		Gets the smallest gap on the spacing scale.
	/// </summary>
	public static double SpaceXXS =>
		Resolve<double>("DmSpaceXXS");

	/// <summary>
	///		Gets the tight gap used by icon and label rows.
	/// </summary>
	public static double SpaceXS =>
		Resolve<double>("DmSpaceXS");

	/// <summary>
	///		Gets the small gap that sits between the two tightest steps and the default.
	/// </summary>
	public static double SpaceSmall =>
		Resolve<double>("DmSpaceSmall");

	/// <summary>
	///		Gets the default gap on the spacing scale.
	/// </summary>
	public static double SpaceMedium =>
		Resolve<double>("DmSpaceMedium");

	/// <summary>
	///		Gets the gap used between grouped controls such as a popup's button row.
	/// </summary>
	public static double SpaceLarge =>
		Resolve<double>("DmSpaceLarge");

	/// <summary>
	///		Gets the gap used between sections of a pane.
	/// </summary>
	public static double SpaceXL =>
		Resolve<double>("DmSpaceXL");

	/// <summary>
	///		Gets the largest gap on the spacing scale.
	/// </summary>
	public static double SpaceXXL =>
		Resolve<double>("DmSpaceXXL");

	/// <summary>
	///		Gets the inset role for compact icon buttons.
	/// </summary>
	public static Thickness PaddingTight =>
		Resolve<Thickness>("DmPaddingTight");

	/// <summary>
	///		Gets the inset role for standard action buttons and tool chips.
	/// </summary>
	public static Thickness PaddingCompact =>
		Resolve<Thickness>("DmPaddingCompact");

	/// <summary>
	///		Gets the inset role for text inputs.
	/// </summary>
	public static Thickness PaddingControl =>
		Resolve<Thickness>("DmPaddingControl");

	/// <summary>
	///		Gets the inset role for card and list-row surfaces.
	/// </summary>
	public static Thickness PaddingCard =>
		Resolve<Thickness>("DmPaddingCard");

	/// <summary>
	///		Gets the inset role for popup bodies and palette panels.
	/// </summary>
	public static Thickness PaddingPanel =>
		Resolve<Thickness>("DmPaddingPanel");

	/// <summary>
	///		Gets the inset role for page roots.
	/// </summary>
	public static Thickness PaddingPage =>
		Resolve<Thickness>("DmPaddingPage");

	/// <summary>
	///		Gets the tightest corner radius, used by icon buttons and palette thumbnails.
	/// </summary>
	public static double RadiusTight =>
		Resolve<double>("DmRadiusTight");

	/// <summary>
	///		Gets the default corner radius.
	/// </summary>
	public static double RadiusDefault =>
		Resolve<double>("DmRadiusDefault");

	/// <summary>
	///		Gets the loosest corner radius, used by cards, popups and panels.
	/// </summary>
	public static double RadiusLoose =>
		Resolve<double>("DmRadiusLoose");

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
	///		Do not swap this for MAUI's own <c>ColorExtensions.ToWindowsColor</c>: its IL is
	///		<c>mul; conv.u1</c> per channel, so it truncates toward zero and does not clamp —
	///		<c>WithAlpha(0.5f)</c> yields 127 there against 128 here, and an out-of-range channel
	///		wraps rather than saturating.
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
		var clamped = Math.Clamp(
			value,
			0f,
			1f);

		return (byte)Math.Round(
			clamped * 255f,
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

		if (resources is null)
		{
			throw new InvalidOperationException(
				$"Theme resource '{key}' was read before Application.Current.Resources existed. "
				+ "See the remarks on DmTheme.");
		}

		if (!resources.TryGetValue(key, out var value))
		{
			throw new InvalidOperationException(
				$"Theme resource '{key}' is not defined under Resources/Theme.");
		}

		if (value is not T typed)
		{
			throw new InvalidOperationException(
				$"Theme resource '{key}' is a {value?.GetType().Name ?? "null"}, "
				+ $"not a {typeof(T).Name}.");
		}

		return typed;
	}
}
