using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using FluentAssertions;

namespace DungeonMaster.Tests.App.Theme;

/// <summary>
///		Ratchets guarding the spacing scale, the padding roles, the radius tokens and the stroke
///		scale introduced by <c>docs/superpowers/specs/2026-08-07-design-system-completion-design.md</c>
///		§3, plus margin and the touch-target minimum, neither of which §3 named and both of which
///		move the same pixels.
///		<para>
///		Each fact carries its own allow-list, seeded with every file that offended when the
///		ratchet was written. A migrating wave removes its own entries and converts the sites;
///		<see cref="EveryGeometryAllowListEntryStillOffends"/> makes an entry that no longer bites
///		a failure, so nothing can be parked and forgotten and no wave can "remove its entry" and
///		see green because the entry never mattered.
///		</para>
///		<para>
///		Only <c>Tokens.xaml</c> and <c>Typography.xaml</c> are out of scope: those are where the
///		scales are DECLARED, so a literal there is the definition rather than a call site. The
///		rest of <c>Resources/Theme/</c> is IN scope. <c>Controls.*.xaml</c> and
///		<c>Implicit.xaml</c> are not definitions — they are the highest-fanout call sites in the
///		application, since adopting those styles is the mechanism by which every control reaches
///		the scale. Excluding the whole directory exempted exactly the files where one bare
///		literal reaches the most screens.
///		</para>
///		<para>
///		THE TWO HALVES OF EVERY FACT READ DIFFERENT MODELS, AND THAT SPLIT IS THE BOUNDARY.
///		The <c>*.xaml</c> half reads <c>ThemeSource.AppDocuments()</c> — the parsed
///		<c>XDocument</c> tree — so a geometry literal is found as an ATTRIBUTE with a name and a
///		value rather than as the characters <c>Padding="</c>. contracts/theme-source.md §3.5
///		measured the two readers against ten synthetic cases and they disagreed once, in the
///		object model's favour. Two consequences are worth stating because they replace machinery
///		this class used to carry: prose in an XML comment is not markup, so nothing has to mask
///		comments before matching; and an attached-property spelling such as
///		<c>Grid.Padding</c> is the same property under a prefix rather than a lookbehind case.
///		The <c>*.cs</c> half stays a raw-text scan, because <c>XDocument</c> cannot parse C#.
///		That is definitional, not a preference.
///		</para>
/// </summary>
public class GeometryScaleTests
{
	/// <summary>
	///		How many lines above a padding are searched for the comment that documents it.
	///		A padding no role token can express survives only WITH such a comment.
	///		<para>
	///		Eight, because that is the real shape of the thing being matched: XAML in this
	///		project puts one attribute per line, so a comment closing above an element whose
	///		<c>Padding</c> is its fifth attribute already sits six or seven lines up. Six was
	///		tried first and rejected a genuinely documented padding in
	///		<c>FloorSidebarItem.xaml</c> by one line. It is a window rather than "anywhere in the
	///		file" on purpose: a comment eight lines up is plausibly about this element, and one
	///		thirty lines up is not.
	///		</para>
	/// </summary>
	private const int DocumentationWindowLines = 8;

	/// <summary>
	///		The one value that is never a scale step. Zero means "none" — the absence of a
	///		spacing, padding or radius — so it stays a literal by design rather than by omission.
	/// </summary>
	private const string ZeroMeansNone = "0";

	/// <summary>
	///		The element that carries a property name in a VALUE rather than as an attribute name.
	/// </summary>
	private const string SetterElement = "Setter";

	/// <summary>
	///		<c>Setter</c>'s two attributes: the property it targets and the value it assigns.
	/// </summary>
	private const string SetterPropertyAttribute = "Property";

	private const string SetterValueAttribute = "Value";

	// The four spellings of a gap. Matched as attribute NAMES on the parsed tree, which is what
	// retires the old (?<![A-Za-z]) lookbehind: that existed only to stop the characters
	// "Spacing" matching inside "CharacterSpacing", and an attribute named CharacterSpacing is
	// simply not an attribute named Spacing.
	private static readonly XamlGeometryProbe SpacingXamlProbe =
		new(
			[
				"Spacing",
				"RowSpacing",
				"ColumnSpacing",
				"ItemSpacing",
			],
			ValuePattern: null);

	private static readonly IReadOnlyList<Regex> SpacingCSharpPatterns =
	[
		new(
			@"(?<![A-Za-z])(?:Row|Column|Item)?Spacing\s*=(?!>)\s*(?<value>[0-9][0-9.f]*)",
			RegexOptions.Compiled),
	];

	private static readonly XamlGeometryProbe PaddingXamlProbe =
		new(
			[
				"Padding",
			],
			ValuePattern: null);

	// The constructor form is the one C# actually writes. `Padding = 7` was covered from the
	// start and `Padding = new Thickness(7, 3)` was not, which is the wrong way round: the
	// double-to-Thickness conversion is a convenience and the constructor is the idiom. The
	// bare `new Thickness(...)` pattern deliberately has no property name in front of it, so an
	// inset built as a local or passed straight into a constructor is caught too. The lookbehind
	// hands the margin case to the Margin row of NoBareGeometryValues rather than reporting
	// it twice, and lets a
	// Thickness assigned to a BORDER go: BorderThickness and StrokeThickness are a stroke width
	// on the DmStroke* scale, not an inset on the DmPadding* roles, and reporting them here
	// would tell the reader to fix them with a padding token. The stroke scale has a ratchet of
	// its own, the Stroke row of NoBareGeometryValues, and that is where those two properties
	// are reported.
	private static readonly IReadOnlyList<Regex> PaddingCSharpPatterns =
	[
		new(
			@"(?<![A-Za-z])Padding\s*=(?!>)\s*(?<value>[0-9][0-9.f]*)",
			RegexOptions.Compiled),
		new(
			@"(?<!(?:Margin|BorderThickness|StrokeThickness)\s*=\s*)new\s+(?:[\w]+\.)*Thickness\(\s*(?<value>[0-9][^)]*)\)",
			RegexOptions.Compiled),
	];

	private static readonly XamlGeometryProbe MarginXamlProbe =
		new(
			[
				"Margin",
			],
			ValuePattern: null);

	private static readonly IReadOnlyList<Regex> MarginCSharpPatterns =
	[
		new(
			@"(?<![A-Za-z])Margin\s*=(?!>)\s*(?<value>[0-9][0-9.f]*)",
			RegexOptions.Compiled),
		new(
			@"(?<![A-Za-z])Margin\s*=\s*new\s+(?:[\w]+\.)*Thickness\(\s*(?<value>[0-9][^)]*)\)",
			RegexOptions.Compiled),
	];

	// The stroke scale — DmStrokeHairline, DmStrokeEmphasis, DmStrokeMedium — was the one scale
	// the spec defined and nothing ever guarded, in either language. The two properties that
	// carry it are Border.StrokeThickness and Button.BorderWidth, and both are double, so the
	// shapes are the attribute and the Setter. The bare `new Thickness(…)` form is deliberately
	// NOT in the C# list: on MAUI a stroke width is a scalar, and the Thickness constructor form
	// belongs to padding and margin, which report it already.
	private static readonly XamlGeometryProbe StrokeXamlProbe =
		new(
			[
				"StrokeThickness",
				"BorderWidth",
			],
			ValuePattern: null);

	private static readonly IReadOnlyList<Regex> StrokeCSharpPatterns =
	[
		new(
			@"(?<![A-Za-z])(?:StrokeThickness|BorderWidth)\s*=(?!>)\s*(?<value>[0-9][0-9.f]*)",
			RegexOptions.Compiled),
	];

	// The touch-target floor. MinimumHeightRequest and MinimumWidthRequest were the last
	// geometry properties in the theme with a shared value and no token at all: 44 was restated
	// as a bare literal at twelve sites, ten of them setters in the two highest-fanout
	// dictionaries. DmTouchTargetMinimum now holds it and this is what stops the thirteenth.
	//
	// HeightRequest and WidthRequest WITHOUT the Minimum prefix are deliberately not scanned:
	// a fixed size is a measured shape, not a floor on a scale, and reporting one here would
	// tell the reader to fix it with a touch-target token that is not what it means. Naming the
	// two properties exactly is what enforces that; the old pattern needed a lookbehind to say
	// the same thing less precisely.
	private static readonly XamlGeometryProbe MinimumSizeXamlProbe =
		new(
			[
				"MinimumHeightRequest",
				"MinimumWidthRequest",
			],
			ValuePattern: null);

	private static readonly IReadOnlyList<Regex> MinimumSizeCSharpPatterns =
	[
		new(
			@"(?<![A-Za-z])Minimum(?:Height|Width)Request\s*=(?!>)\s*(?<value>[0-9][0-9.f]*)",
			RegexOptions.Compiled),
	];

	/// <summary>
	///		The string form of <c>StrokeShape</c>, whose converter parses "RoundRectangle 3" out
	///		of one attribute value. It stays a pattern because it is a pattern over a VALUE rather
	///		than over markup syntax, and the object model hands it exactly that value — which is
	///		also why it no longer reads a control's own prose: the "RoundRectangle 4" written in
	///		the comment above Controls.Base.xaml's setter is a comment, not an attribute.
	/// </summary>
	private static readonly Regex RoundRectangleValuePattern =
		new(
			@"RoundRectangle\s+(?<value>[0-9][^""]*)",
			RegexOptions.Compiled);

	// Two shapes, because a radius can be written two ways in markup: the CornerRadius property
	// — as an attribute on a Button, a BoxView or an element-form RoundRectangle, or as a Style
	// setter — and the STRING form of StrokeShape.
	private static readonly XamlGeometryProbe RadiusXamlProbe =
		new(
			[
				"CornerRadius",
			],
			RoundRectangleValuePattern);

	private static readonly IReadOnlyList<Regex> RadiusCSharpPatterns =
	[
		new(
			@"(?<![A-Za-z])CornerRadius\s*=(?!>)\s*(?<value>[0-9][0-9.f]*)",
			RegexOptions.Compiled),
		new(
			@"new\s+(?:[\w]+\.)*CornerRadius\(\s*(?<value>[0-9][^)]*)\)",
			RegexOptions.Compiled),
	];

	/// <summary>
	///		Files still carrying a literal <c>Spacing</c>, <c>RowSpacing</c>, <c>ColumnSpacing</c>
	///		or <c>ItemSpacing</c>. Empty: the C# resolver this list was waiting for now exists as
	///		<c>DmTheme.Space*</c>, so code-behind that builds layout imperatively reads the same
	///		scale the markup does. Kept rather than deleted so the fact keeps the same shape as
	///		its two siblings and a future regression has an obvious place to be recorded.
	/// </summary>
	private static readonly HashSet<string> SpacingPendingMigration =
		new(StringComparer.OrdinalIgnoreCase)
		{
		};

	/// <summary>
	///		Files still carrying a literal <c>Padding</c>. Empty: every padding in the application
	///		is now a <c>DmPadding*</c> role, a documented asymmetric exception, or a zero. It is
	///		kept rather than deleted so the fact keeps the same shape as its two siblings and a
	///		future regression has an obvious place to be recorded and then removed.
	/// </summary>
	private static readonly HashSet<string> PaddingPendingMigration =
		new(StringComparer.OrdinalIgnoreCase)
		{
		};

	/// <summary>
	///		Files still carrying a literal <c>CornerRadius</c> or a string-form
	///		<c>RoundRectangle N</c>. One C# file remains: the two palette views now build their
	///		thumbnails with <c>DmTheme.RadiusTight</c>, but the map page's code-behind was not in
	///		the migrating wave's file scope. It is a mechanical conversion now that
	///		<c>DmTheme.Radius*</c> exists, not a blocked one.
	/// </summary>
	private static readonly HashSet<string> RadiusPendingMigration =
		new(StringComparer.OrdinalIgnoreCase)
		{
			Relative("Views", "DungeonDetailPage", "DungeonDetailPage.xaml.cs"),
		};

	/// <summary>
	///		The radius literals that are geometry rather than style, listed by file AND value so
	///		the exemption cannot leak to another site that happens to share the number.
	///		<para>
	///		<c>DungeonListPage.xaml</c> <c>26</c> is the floating action button's pill: it is
	///		exactly half of the 52px circle it rounds, so it is derived from the button's own
	///		height and would stop being a circle the moment a shared token moved.
	///		</para>
	/// </summary>
	private static readonly (string Relative, string Value)[] RadiusGeometryExceptions =
	[
		(Relative("Views", "DungeonListPage", "DungeonListPage.xaml"), "26"),

		// Controls.Base.xaml keeps DmBorderBase's StrokeShape as a literal on purpose. An
		// element-form <RoundRectangle> inside a Setter is ONE shared geometry instance across
		// every Border that adopts the style — ReferenceEquals is true, a mutation crosses
		// Borders and the Parent is hijacked — so the string form is the only safe way to write
		// it here. Its value is checked against DmRadiusDefault by
		// BindablePropertyDefaultsMatchTokensTests instead, which is a stronger guarantee than
		// this ratchet gives.
		(Relative("Resources", "Theme", "Controls.Base.xaml"), "3"),
	];

	/// <summary>
	///		Padding literals that no role token can express, listed by file AND value AND reason.
	///		This is the explicit form the spec asked for: an exemption that names the exact value
	///		it exempts cannot leak to the next literal that lands in the same file.
	/// </summary>
	private static readonly (string Relative, string Value, string Reason)[] PaddingScaleExceptions =
	[
		(Relative("Resources", "Theme", "Implicit.xaml"),
			"14,10",
			"the implicit Button inset, reasoned at Implicit.xaml above the setter; it is the "
			+ "measured button box rather than a step on the DmPadding* ramp"),

		(Relative("Resources", "Theme", "Controls.Base.xaml"),
			"14,10",
			"DmButtonBase carries the same measured button box as the implicit Button style, "
			+ "reasoned above the setter; the two must stay identical"),
	];

	/// <summary>
	///		Files still carrying a literal <c>Margin</c>. Margin sits on the same geometry scale as
	///		padding and spacing and was ratcheted by nothing at all, so the whole class of literal
	///		survived the migration untouched — this list is the census taken when the ratchet was
	///		added, not a set of approvals. Each entry is a conversion still to do, and
	///		<see cref="EveryGeometryAllowListEntryStillOffends"/> makes an entry that no longer
	///		bites a failure, so the list can only shrink.
	///		<para>
	///		It is a file list rather than a (file, value) list because the offenders are dozens of
	///		nudges of one, two and three rather than a handful of measured shapes; naming every
	///		value would be a transcription of the tree rather than a decision about it.
	///		</para>
	/// </summary>
	private static readonly HashSet<string> MarginPendingMigration =
		new(StringComparer.OrdinalIgnoreCase)
		{
			Relative("Controls", "AreaSidebarItem", "AreaSidebarItem.xaml"),
			Relative("Controls", "DmCard", "DmCard.xaml"),
			Relative("Controls", "DmFlagBanner", "DmFlagBanner.xaml"),
			Relative("Controls", "FloorSidebarItem", "FloorSidebarItem.xaml"),
			Relative("Views", "DungeonDetailPage", "ControlsPaletteView.xaml"),
			Relative("Views", "DungeonDetailPage", "DungeonDetailPage.xaml"),
			Relative("Views", "DungeonDetailPage", "DungeonDetailPage.xaml.cs"),
			Relative("Views", "DungeonDetailPage", "ObjectPaletteView.xaml.cs"),
			Relative("Views", "DungeonListPage", "DungeonListPage.xaml"),
		};

	/// <summary>
	///		Files still carrying a literal <c>StrokeThickness</c> or <c>BorderWidth</c>. This is
	///		the census taken when the ratchet was added, not a set of approvals: the stroke scale
	///		was defined by spec §3 and then guarded by nothing at all in either language, so every
	///		literal below came through the whole migration unexamined.
	///		<para>
	///		TWO entries, of which ONE is C#. This comment has been wrong about that number twice
	///		already. It said "the four C# entries" of a list that had four ENTRIES, three of them
	///		C#; it was corrected to "THREE entries, of which TWO are C#"; and that went stale in
	///		turn when <c>AreaSidebarItem.xaml.cs</c> left. Counted from the initialiser below as it
	///		stands rather than carried over from the last version of this sentence: two
	///		<c>Relative(</c> calls, of which one names a <c>.cs</c> file. Recount it here on the
	///		way past whenever an entry leaves.
	///		</para>
	///		<para>
	///		The XAML entry is <c>DungeonDetailPage.xaml</c>, whose map-object glyphs are stroked at
	///		<c>1.25</c> and <c>1.45</c> — values off the 1 / 1.5 / 2 ladder entirely, and the two
	///		that most need a human decision rather than a mechanical snap. The one C# entry is
	///		<c>DungeonDetailPage.xaml.cs</c>, a hairline built imperatively, which the ladder
	///		already expresses; it is a conversion still to do rather than an exemption.
	///		<c>PaletteThumb.cs</c> and <c>AreaSidebarItem.xaml.cs</c> both left this list the same
	///		way, and had to: <see cref="EveryGeometryAllowListEntryStillOffends"/> requires the
	///		entry and the literal to leave in the same change. PaletteThumb went when
	///		<c>DmTheme.StrokeHairline</c> was added; AreaSidebarItem went when its selected-border
	///		stroke moved out of <c>SetEditing</c> and into a VisualStateManager setter reading
	///		<c>DmStrokeHairline</c>, which left the file with no non-zero stroke literal at all.
	///		</para>
	/// </summary>
	private static readonly HashSet<string> StrokePendingMigration =
		new(StringComparer.OrdinalIgnoreCase)
		{
			Relative("Views", "DungeonDetailPage", "DungeonDetailPage.xaml"),
			Relative("Views", "DungeonDetailPage", "DungeonDetailPage.xaml.cs"),
		};

	/// <summary>
	///		Files still carrying a literal <c>MinimumHeightRequest</c> or
	///		<c>MinimumWidthRequest</c>. This is the census taken when the touch-target ratchet was
	///		added, not a set of approvals.
	///		<para>
	///		WHY THESE ARE A CENSUS AND NOT EXEMPTIONS, stated plainly because the distinction
	///		matters here more than on the other lists. Every one of the four is a minimum that is
	///		a LAYOUT DIMENSION rather than a touch target — a popup's minimum width, the notebook
	///		editors' minimum height, a palette column's minimum width — and none of them is 44.
	///		A scan cannot tell those two meanings apart from a number, and neither can this list:
	///		deciding whether each is a measured shape that should stay a literal, a shape that
	///		wants a token of its own, or drift, is a human decision that has not been taken. The
	///		entries record that it has not been taken; they do not pretend it has.
	///		</para>
	///		<para>
	///		Four files, six literals as counted on 2026-08-28 with
	///		<c>grep -rn 'Minimum\(Height\|Width\)Request' --include=*.xaml --include=*.cs</c> from
	///		<c>src/DungeonMaster.App</c>: <c>DmPopup.xaml</c> 300, <c>NotebookView.xaml</c> 24 and
	///		150 twice, <c>DungeonDetailPage.xaml</c> 22, <c>ObjectPaletteView.xaml</c> 204.
	///		<see cref="EveryGeometryAllowListEntryStillOffends"/> makes an entry that no longer
	///		bites a failure, so the list can only shrink.
	///		</para>
	/// </summary>
	private static readonly HashSet<string> MinimumSizePendingMigration =
		new(StringComparer.OrdinalIgnoreCase)
		{
			Relative("Controls", "DmPopup", "DmPopup.xaml"),
			Relative("Controls", "NotebookView", "NotebookView.xaml"),
			Relative("Views", "DungeonDetailPage", "DungeonDetailPage.xaml"),
			Relative("Views", "DungeonDetailPage", "ObjectPaletteView.xaml"),
		};

	private static readonly IReadOnlySet<string> TokenDeclarationFiles =
		new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			Relative("Resources", "Theme", "Tokens.xaml"),
			Relative("Resources", "Theme", "Typography.xaml"),
		};

	/// <summary>
	///		The four names <see cref="UniformGeometryRatchets"/> must carry, written out
	///		INDEPENDENTLY of that list. Deriving them from the list would make the cardinality
	///		guard agree with whatever the list happens to say, which is the vacuity it exists to
	///		prevent.
	/// </summary>
	private static readonly string[] ExpectedUniformGeometryRatchetNames =
	[
		"Spacing",
		"Margin",
		"Stroke",
		"MinimumSize",
	];

	/// <summary>
	///		The four geometry ratchets whose pipeline is uniform, as data.
	///		<para>
	///		Spacing, Margin, Stroke and MinimumSize ran four copies of one method: scan the
	///		markup, concatenate the code-behind scan, drop the zero, describe, sort, and require
	///		the result to be empty. They differ only in what they probe for, what they still
	///		excuse, and the sentence they fail with, so they are rows.
	///		</para>
	///		<para>
	///		Padding and Radius are deliberately NOT here and stay facts of their own. Padding adds
	///		a scale-exception filter and a shaping-versus-scale projection; Radius adds a geometry
	///		exception filter. Neither runs this pipeline, and folding either in would mean putting
	///		the difference back as a conditional.
	///		</para>
	///		<para>
	///		MARGIN, STROKE AND MINIMUM SIZE CONVERT NOTHING; THEY PIN A CENSUS. Margin is the same
	///		geometry as padding measured from the other side of the border, and the spec defined
	///		scales for spacing, padding, radius and stroke and simply did not name it, so an
	///		entire class of literal came through the migration untouched and unmeasured. Stroke
	///		was one of the spec's four scales and the one never ratcheted — the gap was recorded
	///		in a comment beside the padding patterns rather than closed, which is the same shape
	///		as every other defect that branch found: a note asserting a guard is not a guard. The
	///		touch target, 44, was the one shared geometry value in this theme with no token at
	///		all, restated as a bare literal at twelve sites; all twelve now name
	///		<c>DmTouchTargetMinimum</c>. Each of the three takes the census, pins it, and makes
	///		the next one a failure.
	///		</para>
	///		<para>
	///		DROPPING A ROW HERE SILENTLY RETIRES A WHOLE RATCHET and every remaining test in this
	///		class still passes, because xUnit errors only on a COMPLETELY empty theory source and
	///		never on a short one.
	///		<see cref="TheUniformGeometryRatchetSetIsExactlyTheFourUniformProperties"/> is what
	///		makes that a failure. The two scan guards above cannot: they protect the FILE SCAN,
	///		not the ROW SET.
	///		</para>
	/// </summary>
	private static readonly IReadOnlyList<UniformGeometryRatchet> UniformGeometryRatchets =
	[
		new(
			"Spacing",
			SpacingXamlProbe,
			SpacingCSharpPatterns,
			SpacingPendingMigration,
			"every gap must come from the DmSpace* scale in Resources/Theme/Tokens.xaml"),
		new(
			"Margin",
			MarginXamlProbe,
			MarginCSharpPatterns,
			MarginPendingMigration,
			"every offset must come from the DmSpace* scale in Resources/Theme/Tokens.xaml, "
			+ "or be recorded in MarginPendingMigration with the rest of the census"),
		new(
			"Stroke",
			StrokeXamlProbe,
			StrokeCSharpPatterns,
			StrokePendingMigration,
			"every stroke width must come from the DmStroke* scale in "
			+ "Resources/Theme/Tokens.xaml, or be recorded in StrokePendingMigration "
			+ "with the rest of the census"),
		new(
			"MinimumSize",
			MinimumSizeXamlProbe,
			MinimumSizeCSharpPatterns,
			MinimumSizePendingMigration,
			"every minimum size must name a token in Resources/Theme/Tokens.xaml — "
			+ "DmTouchTargetMinimum where it is the platform touch target — or be recorded "
			+ "in MinimumSizePendingMigration with the rest of the census"),
	];

	/// <summary>
	///		Gets the name of each uniform geometry ratchet, one theory row apiece.
	///		<para>
	///		The row carries the NAME rather than the ratchet, so the theory's display name says
	///		which ratchet failed and xUnit has nothing to serialise but a string. The theory looks
	///		the row up in <see cref="UniformGeometryRatchets"/>, which is the single list the
	///		cardinality guard also reads, so the guard cannot pass over a set the theory does not
	///		run.
	///		</para>
	/// </summary>
	public static TheoryData<string> UniformGeometryRatchetNames
	{
		get
		{
			var data = new TheoryData<string>();

			foreach (var ratchet in UniformGeometryRatchets)
			{
				data.Add(ratchet.Name);
			}

			return data;
		}
	}

	/// <summary>
	///		Asserts that the markup half of every geometry ratchet reaches application sources at
	///		all.
	///		<para>
	///		This assertion used to live inside <c>Scan(...)</c>, where a scan that had stopped
	///		reaching anything was reported against whichever fact called the helper first. It is a
	///		named fact of its own so the failure names its own cause; contracts/anti-vacuity.md
	///		section 2 is the rule, and it is one fact per search pattern because a guard with two
	///		reasons to fail is two guards.
	///		</para>
	/// </summary>
	[Fact]
	public void TheGeometryXamlScanReachesAtLeastOneApplicationSource()
	{
		ScannedXamlDocuments()
			.Should()
			.NotBeEmpty("the '*.xaml' scan must find application sources or the geometry ratchet "
				+ "built on it is vacuous");
	}

	/// <summary>
	///		Asserts that the code-behind half of every geometry ratchet reaches application
	///		sources at all. See <see cref="TheGeometryXamlScanReachesAtLeastOneApplicationSource"/>.
	/// </summary>
	[Fact]
	public void TheGeometryCSharpScanReachesAtLeastOneApplicationSource()
	{
		ScannedCSharpFiles()
			.Should()
			.NotBeEmpty("the '*.cs' scan must find application sources or the geometry ratchet "
				+ "built on it is vacuous");
	}

	/// <summary>
	///		Asserts that the uniform geometry theory runs over exactly the four ratchets whose
	///		pipeline is uniform, and no fewer.
	///		<para>
	///		This is a CARDINALITY guard, not a non-emptiness one, and the difference is the whole
	///		point. The two scan guards above protect the file scan; nothing protected the ROW SET.
	///		Deleting a row from <see cref="UniformGeometryRatchets"/> retires an entire ratchet
	///		while every other test in this class stays green, because xUnit reports an error only
	///		for a completely empty theory source and never for a short one. An offenders list is
	///		empty both when everything is correct and when nothing was examined, and
	///		contracts/anti-vacuity.md section 5 forbids letting those two states look alike.
	///		</para>
	/// </summary>
	[Fact]
	public void TheUniformGeometryRatchetSetIsExactlyTheFourUniformProperties()
	{
		UniformGeometryRatchets
			.Select(ratchet =>
				ratchet.Name)
			.Should()
			.BeEquivalentTo(
				ExpectedUniformGeometryRatchetNames,
				"a dropped row silently retires a whole ratchet and leaves every other test in "
				+ "this class green; xUnit errors only on a completely empty theory source, "
				+ "never on a short one");
	}

	/// <summary>
	///		Asserts that no literal geometry value survives outside the theme dictionaries, for
	///		each of the four ratchets whose pipeline is uniform.
	///		<para>
	///		The row set is guarded by
	///		<see cref="TheUniformGeometryRatchetSetIsExactlyTheFourUniformProperties"/>; the two
	///		scans this reads are guarded by the two facts above.
	///		</para>
	/// </summary>
	/// <param name="ratchetName">
	///		The name of the ratchet to run, from <see cref="UniformGeometryRatchetNames"/>.
	/// </param>
	[Theory]
	[MemberData(nameof(UniformGeometryRatchetNames))]
	public void NoBareGeometryValues(string ratchetName)
	{
		var ratchet = UniformGeometryRatchets.Single(candidate =>
			candidate.Name == ratchetName);

		var offenders = ScanXaml(
				ratchet.XamlProbe,
				ratchet.PendingMigration)
			.Concat(
				ScanCSharp(
					ratchet.CSharpPatterns,
					ratchet.PendingMigration))
			.Where(found =>
				found.Value != ZeroMeansNone)
			.Select(Describe)
			.OrderBy(
				entry =>
					entry,
				StringComparer.Ordinal)
			.ToList();

		offenders
			.Should()
			.BeEmpty(ratchet.Because);
	}

	/// <summary>
	///		Asserts that no literal padding survives outside the theme dictionaries.
	///		<para>
	///		A padding that no role token can express — one with four components, or one with a
	///		zero component, which is an inset deliberately suppressed on one edge — survives ONLY
	///		when a comment within <see cref="DocumentationWindowLines"/> lines above it says what
	///		it is shaping. An undocumented one is reported, because the spec calls it a defect.
	///		</para>
	/// </summary>
	[Fact]
	public void NoBarePaddingValues()
	{
		var offenders = ScanXaml(
				PaddingXamlProbe,
				PaddingPendingMigration)
			.Concat(
				ScanCSharp(
					PaddingCSharpPatterns,
					PaddingPendingMigration))
			.Where(found =>
				found.Value != ZeroMeansNone)
			.Where(found =>
				!PaddingScaleExceptions.Any(exception =>
					string.Equals(
						exception.Relative,
						found.Relative,
						StringComparison.OrdinalIgnoreCase)
					&& exception.Value == found.Value))
			.Select(found =>
				IsShapingPadding(found.Value)
					? (Found: found, Note: "asymmetric padding with no comment saying what it shapes")
					: (Found: found, Note: "not on the DmPadding* roles"))
			.Where(entry =>
				!IsShapingPadding(entry.Found.Value)
				|| !entry.Found.IsDocumented)
			.Select(entry =>
				$"{Describe(entry.Found)} ({entry.Note})")
			.OrderBy(
				entry =>
					entry,
				StringComparer.Ordinal)
			.ToList();

		offenders
			.Should()
			.BeEmpty("every inset must come from the DmPadding* roles in Resources/Theme/Tokens.xaml");
	}

	/// <summary>
	///		Asserts that no literal corner radius survives outside the theme dictionaries, in
	///		either the <c>CornerRadius</c> attribute form or the string form of
	///		<c>StrokeShape</c>. The element form binds a token in attribute position and is the
	///		replacement for the latter.
	/// </summary>
	[Fact]
	public void NoBareRadiusValues()
	{
		var offenders = ScanXaml(
				RadiusXamlProbe,
				RadiusPendingMigration)
			.Concat(
				ScanCSharp(
					RadiusCSharpPatterns,
					RadiusPendingMigration))
			.Where(found =>
				found.Value != ZeroMeansNone)
			.Where(found =>
				!RadiusGeometryExceptions.Any(exception =>
					string.Equals(
						exception.Relative,
						found.Relative,
						StringComparison.OrdinalIgnoreCase)
					&& exception.Value == found.Value))
			.Select(Describe)
			.OrderBy(
				entry =>
					entry,
				StringComparer.Ordinal)
			.ToList();

		offenders
			.Should()
			.BeEmpty("every corner must come from the DmRadius* tokens in Resources/Theme/Tokens.xaml");
	}

	/// <summary>
	///		Asserts that every allow-listed path names a file that exists and still carries the
	///		literal its list exempts. Without this the three ratchets are not self-checking as
	///		they shrink: a stale or mistyped entry exempts nothing, and the wave that "removed its
	///		entry" would see green from a guard that never bit it.
	/// </summary>
	[Fact]
	public void EveryGeometryAllowListEntryStillOffends()
	{
		var stale = Stale(
				"Spacing",
				SpacingPendingMigration,
				SpacingXamlProbe,
				SpacingCSharpPatterns)
			.Concat(
				Stale(
					"Padding",
					PaddingPendingMigration,
					PaddingXamlProbe,
					PaddingCSharpPatterns))
			.Concat(
				Stale(
					"Radius",
					RadiusPendingMigration,
					RadiusXamlProbe,
					RadiusCSharpPatterns))
			.Concat(
				Stale(
					"Margin",
					MarginPendingMigration,
					MarginXamlProbe,
					MarginCSharpPatterns))
			.Concat(
				Stale(
					"Stroke",
					StrokePendingMigration,
					StrokeXamlProbe,
					StrokeCSharpPatterns))
			.Concat(
				Stale(
					"MinimumSize",
					MinimumSizePendingMigration,
					MinimumSizeXamlProbe,
					MinimumSizeCSharpPatterns))
			.Concat(StaleValueExceptions())
			.OrderBy(
				entry =>
					entry,
				StringComparer.Ordinal)
			.ToList();

		stale.Should().BeEmpty();
	}

	/// <summary>
	///		The (file, value) exception lists are self-checking on the same terms as the file
	///		lists: the file must exist and the exact value must still be present in it. Without
	///		this an exception outlives the literal it excuses and silently covers the next one
	///		that happens to share the number.
	///		<para>
	///		The value is looked for in the markup's own attribute values and element text rather
	///		than anywhere in the file's bytes, so an exemption is no longer kept alive by its own
	///		explanatory comment mentioning the number it exempts.
	///		</para>
	/// </summary>
	private static IEnumerable<string> StaleValueExceptions()
	{
		var entries = RadiusGeometryExceptions
			.Select(exception =>
				("Radius", exception.Relative, exception.Value))
			.Concat(
				PaddingScaleExceptions.Select(exception =>
					("Padding", exception.Relative, exception.Value)));

		foreach (var (listName, relative, value) in entries)
		{
			var path = Path.Combine(
				RepoPaths.AppRoot,
				relative);

			if (!File.Exists(path))
			{
				yield return $"{listName}: {relative} ({value}) -> file does not exist; the entry exempts nothing";

				continue;
			}

			var document = DocumentFor(relative);

			var present = document is not null
				&& LiteralText(document.Document)
					.Any(text =>
						text.Contains(
							value,
							StringComparison.Ordinal));

			if (!present)
			{
				yield return $"{listName}: {relative} ({value}) -> the value is gone; remove the exception";
			}
		}
	}

	private static IEnumerable<string> Stale(
		string listName,
		IReadOnlyCollection<string> allowList,
		XamlGeometryProbe probe,
		IReadOnlyList<Regex> cSharpPatterns)
	{
		foreach (var relative in allowList)
		{
			var path = Path.Combine(
				RepoPaths.AppRoot,
				relative);

			if (!File.Exists(path))
			{
				yield return $"{listName}: {relative} -> file does not exist; the entry exempts nothing";

				continue;
			}

			// The staleness check reads the same model the ratchet reads, per language. Letting
			// the two diverge would produce the one failure neither fact can report: a file the
			// ratchet no longer considers an offender and the allow-list still believes is one.
			var offends = relative.EndsWith(
				".cs",
				StringComparison.OrdinalIgnoreCase)
				? StillOffendsInCSharp(path, cSharpPatterns)
				: StillOffendsInMarkup(relative, probe);

			if (!offends)
			{
				yield return $"{listName}: {relative} -> no longer offends; remove it from the allow-list";
			}
		}
	}

	private static bool StillOffendsInCSharp(
		string path,
		IReadOnlyList<Regex> patterns)
	{
		var text = File.ReadAllText(path);

		return patterns
			.SelectMany(pattern =>
				pattern.Matches(text).Cast<Match>())
			.Any(match =>
				match.Groups["value"].Value.Trim() != ZeroMeansNone);
	}

	private static bool StillOffendsInMarkup(
		string relative,
		XamlGeometryProbe probe)
	{
		var document = DocumentFor(relative);

		return document is not null
			&& MarkupMatches(
					document,
					probe,
					relative)
				.Any(found =>
					found.Value != ZeroMeansNone);
	}

	/// <summary>
	///		The parsed markup this class examines: every application document except the two
	///		dictionaries that DECLARE the scales.
	/// </summary>
	private static IReadOnlyList<ThemeDocument> ScannedXamlDocuments()
	{
		return ThemeSource
			.AppDocuments()
			.Where(document =>
				!IsTokenDeclaration(NativeRelativePath(document)))
			.ToList();
	}

	/// <summary>
	///		The code-behind this class examines, as paths relative to the application project
	///		root. There is no object model to read these with; see the class remarks.
	/// </summary>
	private static IReadOnlyList<string> ScannedCSharpFiles()
	{
		return RepoPaths
			.AppSourceFiles("*.cs")
			.Select(path =>
				Path.GetRelativePath(
					RepoPaths.AppRoot,
					path))
			.Where(relative =>
				!IsTokenDeclaration(relative))
			.ToList();
	}

	private static List<GeometryMatch> ScanXaml(
		XamlGeometryProbe probe,
		IReadOnlyCollection<string> allowList)
	{
		var found = new List<GeometryMatch>();

		foreach (var document in ScannedXamlDocuments())
		{
			var relative = NativeRelativePath(document);

			if (allowList.Contains(relative))
			{
				continue;
			}

			found.AddRange(
				MarkupMatches(
					document,
					probe,
					relative));
		}

		return found;
	}

	private static List<GeometryMatch> ScanCSharp(
		IReadOnlyList<Regex> patterns,
		IReadOnlyCollection<string> allowList)
	{
		var found = new List<GeometryMatch>();

		foreach (var relative in ScannedCSharpFiles())
		{
			if (allowList.Contains(relative))
			{
				continue;
			}

			var text = File.ReadAllText(
				Path.Combine(
					RepoPaths.AppRoot,
					relative));

			// Matched against the blanked copy, reported against the original.
			// ThemeSource.BlankCSharpComments replaces the comment with spaces and leaves the
			// length untouched, so every index and every line number below still counts against
			// the file as it is on disk. This class used to declare its own anchored pattern and
			// its own offset-preserving blanker; both are now the shared helper's, which is
			// MASTER-138 for this file. The markup half needs no equivalent at all: a comment is
			// an XComment in the object model, and the scan never looks at one.
			var searchable = ThemeSource.BlankCSharpComments(text);

			foreach (var pattern in patterns)
			{
				foreach (Match match in pattern.Matches(searchable))
				{
					found.Add(
						new GeometryMatch(
							relative,
							ThemeSource.LineNumber(
								text,
								match.Index),
							match.Value.Trim(),
							match.Groups["value"].Value.Trim(),
							// A C# file carries no XML comment, so no C# literal is ever
							// documented in the sense NoBarePaddingValues means. Measured:
							// grep -rn -e '-->' --include=*.cs src/DungeonMaster.App returns
							// nothing, so the raw-text predicate this replaces returned false
							// here every time it was called.
							IsDocumented: false));
				}
			}
		}

		return found;
	}

	/// <summary>
	///		Every geometry literal the probe describes, read off the parsed tree.
	/// </summary>
	/// <param name="document">
	///		The parsed document to read.
	/// </param>
	/// <param name="probe">
	///		The properties and the optional value pattern that define this ratchet's shape.
	/// </param>
	/// <param name="relative">
	///		The document's path relative to the application project root, in the native separator
	///		form the allow lists and the reports use.
	/// </param>
	private static IEnumerable<GeometryMatch> MarkupMatches(
		ThemeDocument document,
		XamlGeometryProbe probe,
		string relative)
	{
		foreach (var element in document.Document.Descendants())
		{
			var setterProperty = string.Equals(
				element.Name.LocalName,
				SetterElement,
				StringComparison.Ordinal)
				? PropertyName(element.Attribute(SetterPropertyAttribute)?.Value)
				: null;

			foreach (var attribute in element.Attributes())
			{
				var isSetterValue = setterProperty is not null
					&& string.Equals(
						attribute.Name.LocalName,
						SetterValueAttribute,
						StringComparison.Ordinal);

				// <Setter Property="Padding" Value="14,10" /> names the property in a VALUE, so
				// it is read from the element rather than from the attribute's own name.
				var property = isSetterValue
					? setterProperty
					: PropertyName(attribute.Name.LocalName);

				if (property is not null
					&& probe.Properties.Contains(property)
					&& IsLiteral(attribute.Value))
				{
					yield return Found(
						document,
						relative,
						attribute,
						isSetterValue
							? $"<{SetterElement} {SetterPropertyAttribute}=\"{setterProperty}\" "
								+ $"{SetterValueAttribute}=\"{attribute.Value}\""
							: $"{attribute.Name.LocalName}=\"{attribute.Value}\"",
						attribute.Value.Trim());
				}

				if (probe.ValuePattern is null)
				{
					continue;
				}

				foreach (Match match in probe.ValuePattern.Matches(attribute.Value))
				{
					yield return Found(
						document,
						relative,
						attribute,
						match.Value.Trim(),
						match.Groups["value"].Value.Trim());
				}
			}
		}
	}

	private static GeometryMatch Found(
		ThemeDocument document,
		string relative,
		XAttribute attribute,
		string matched,
		string value)
	{
		var line = LineOf(attribute);

		return new GeometryMatch(
			relative,
			line,
			matched,
			value,
			HasDocumentingComment(
				document.Document,
				line,
				value));
	}

	// Reported with the line number AND the matched text: a bare relative path leaves a
	// 700-line XAML file to be searched by hand for whatever tripped the ratchet.
	private static string Describe(GeometryMatch found)
	{
		return $"{found.Relative}:{found.Line} -> {found.Matched}";
	}

	/// <summary>
	///		Returns whether a padding expresses a shape rather than a role: four components, or
	///		any component suppressed to zero. Neither can be written with a two-value role token,
	///		so both survive as exceptions — but only documented ones.
	/// </summary>
	private static bool IsShapingPadding(string value)
	{
		var components = value.Split(',');

		return components.Length == 4
			|| components.Any(component =>
				component.Trim() == ZeroMeansNone);
	}

	/// <summary>
	///		Whether a file DECLARES the scales rather than calling them. Only the two foundation
	///		dictionaries do; <c>Controls.*.xaml</c> and <c>Implicit.xaml</c> are call sites and are
	///		scanned like any other markup.
	/// </summary>
	private static bool IsTokenDeclaration(string relative)
	{
		return TokenDeclarationFiles.Contains(relative);
	}

	/// <summary>
	///		Whether a comment closing within <see cref="DocumentationWindowLines"/> lines above the
	///		match actually documents THIS literal.
	/// </summary>
	/// <remarks>
	///		The original test was only whether the string <c>--&gt;</c> appeared in the window,
	///		which in a tree this densely commented exempts almost every asymmetric padding: a
	///		comment about <c>MinimumHeightRequest</c> excused a <c>Padding="7,3,9,5"</c> four lines
	///		below it, proved by construction. A comment that documents a padding says so — it uses
	///		the word, or it quotes the value — so that is what is required, and the check reads the
	///		comment's own text rather than the fact that one exists.
	///		<para>
	///		The window is bounded by the comment's CLOSING line, not its opening one. A one-line
	///		comment carries both, so nothing is lost; but the prose blocks this project writes run
	///		to six or eight lines, and keying on the opener made a fully documented padding fail
	///		because its comment STARTED thirteen lines up while ending five lines up. What the
	///		window should ask is "does a comment finish immediately above this attribute", and only
	///		the closer answers that.
	///		</para>
	/// </remarks>
	/// <param name="document">
	///		The document the match was found in.
	/// </param>
	/// <param name="line">
	///		The 1-based line the match falls on.
	/// </param>
	/// <param name="value">
	///		The matched value, which a documenting comment may quote instead of using the word.
	/// </param>
	private static bool HasDocumentingComment(
		XDocument document,
		int line,
		string value)
	{
		foreach (var comment in document.DescendantNodes().OfType<XComment>())
		{
			var start = LineOf(comment);

			if (start == 0)
			{
				continue;
			}

			var closing = start
				+ comment
					.Value
					.Count(character =>
						character == '\n');

			if (closing > line
				|| line - closing > DocumentationWindowLines)
			{
				continue;
			}

			if (comment.Value.Contains(
					"padding",
					StringComparison.OrdinalIgnoreCase)
				|| comment.Value.Contains(
					value,
					StringComparison.Ordinal))
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	///		The text a XAML file's own markup carries — attribute values and element text. It
	///		deliberately excludes comments: prose is not markup, and that is the whole reason this
	///		class no longer masks anything before matching.
	/// </summary>
	/// <param name="document">
	///		The parsed document to read.
	/// </param>
	private static IEnumerable<string> LiteralText(XDocument document)
	{
		foreach (var node in document.DescendantNodes())
		{
			if (node is XElement element)
			{
				foreach (var attribute in element.Attributes())
				{
					yield return attribute.Value;
				}
			}
			else if (node is XText text)
			{
				yield return text.Value;
			}
		}
	}

	private static ThemeDocument? DocumentFor(string relative)
	{
		return ThemeSource
			.AppDocuments()
			.FirstOrDefault(document =>
				string.Equals(
					NativeRelativePath(document),
					relative,
					StringComparison.OrdinalIgnoreCase));
	}

	/// <summary>
	///		The property a XAML name denotes, with any attached-property prefix removed, so that
	///		<c>Grid.Padding</c> is a Padding. Null in, null out.
	/// </summary>
	/// <param name="name">
	///		The attribute name, or a <c>Setter</c>'s <c>Property</c> value.
	/// </param>
	private static string? PropertyName(string? name)
	{
		if (name is null)
		{
			return null;
		}

		var dot = name.LastIndexOf('.');

		return dot < 0
			? name
			: name[(dot + 1)..];
	}

	/// <summary>
	///		Whether an attribute value is a literal geometry rather than a token reference. A
	///		<c>{StaticResource DmSpaceTight}</c> starts with a brace, not a digit.
	/// </summary>
	private static bool IsLiteral(string value)
	{
		return value.Length > 0
			&& char.IsAsciiDigit(value[0]);
	}

	private static string NativeRelativePath(ThemeDocument document)
	{
		return document
			.RelativePath
			.Replace(
				'/',
				Path.DirectorySeparatorChar);
	}

	private static int LineOf(IXmlLineInfo info)
	{
		return info.HasLineInfo()
			? info.LineNumber
			: 0;
	}

	private static string Relative(params string[] segments)
	{
		return Path.Combine(segments);
	}

	/// <summary>
	///		One ratchet's markup shape: the properties whose bare value it forbids, and an
	///		optional pattern applied to attribute VALUES for the one literal that is spelled
	///		inside a value rather than assigned to a property.
	/// </summary>
	/// <param name="Properties">
	///		The property names, compared after any attached-property prefix, in both the attribute
	///		and the <c>Setter</c> form.
	/// </param>
	/// <param name="ValuePattern">
	///		The value pattern, or null when the ratchet has no such shape. It must capture a
	///		<c>value</c> group.
	/// </param>
	private sealed record XamlGeometryProbe(
		IReadOnlyList<string> Properties,
		Regex? ValuePattern);

	/// <summary>
	///		One geometry ratchet whose pipeline is the uniform one, as a row.
	/// </summary>
	/// <param name="Name">
	///		The property family the ratchet guards. It is the theory's row key and what the
	///		cardinality guard counts.
	/// </param>
	/// <param name="XamlProbe">
	///		The markup shape whose bare value the ratchet forbids.
	/// </param>
	/// <param name="CSharpPatterns">
	///		The code-behind patterns, matched against the comment-blanked text.
	/// </param>
	/// <param name="PendingMigration">
	///		The files this ratchet still excuses, each self-checked by
	///		<see cref="EveryGeometryAllowListEntryStillOffends"/>.
	/// </param>
	/// <param name="Because">
	///		The sentence the ratchet fails with. The four are not interchangeable: each names a
	///		distinct failure mode in the words of the person who found it.
	/// </param>
	private sealed record UniformGeometryRatchet(
		string Name,
		XamlGeometryProbe XamlProbe,
		IReadOnlyList<Regex> CSharpPatterns,
		IReadOnlyCollection<string> PendingMigration,
		string Because);

	private sealed record GeometryMatch(
		string Relative,
		int Line,
		string Matched,
		string Value,
		bool IsDocumented);
}
