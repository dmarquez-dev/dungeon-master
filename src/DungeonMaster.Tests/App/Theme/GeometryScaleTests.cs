using System.Text.RegularExpressions;
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

	// (?<![A-Za-z]) is what keeps CharacterSpacing out: the engine can only reach the literal
	// "Spacing" at the 'S', where the preceding character is 'r'. Without it every
	// CharacterSpacing in the app would be reported as a bare spacing value.
	private static readonly IReadOnlyList<Regex> SpacingXamlPatterns =
	[
		new(
			@"(?<![A-Za-z])(?:Row|Column|Item)?Spacing=""(?<value>[0-9][^""]*)""",
			RegexOptions.Compiled),
		new(
			@"<Setter\s+Property=""(?:Row|Column|Item)?Spacing""\s+Value=""(?<value>[0-9][^""]*)""",
			RegexOptions.Compiled),
	];

	private static readonly IReadOnlyList<Regex> SpacingCSharpPatterns =
	[
		new(
			@"(?<![A-Za-z])(?:Row|Column|Item)?Spacing\s*=(?!>)\s*(?<value>[0-9][0-9.f]*)",
			RegexOptions.Compiled),
	];

	private static readonly IReadOnlyList<Regex> PaddingXamlPatterns =
	[
		new(
			@"(?<![A-Za-z])Padding=""(?<value>[0-9][^""]*)""",
			RegexOptions.Compiled),
		new(
			@"<Setter\s+Property=""Padding""\s+Value=""(?<value>[0-9][^""]*)""",
			RegexOptions.Compiled),
	];

	// The constructor form is the one C# actually writes. `Padding = 7` was covered from the
	// start and `Padding = new Thickness(7, 3)` was not, which is the wrong way round: the
	// double-to-Thickness conversion is a convenience and the constructor is the idiom. The
	// bare `new Thickness(...)` pattern deliberately has no property name in front of it, so an
	// inset built as a local or passed straight into a constructor is caught too. The lookbehind
	// hands the margin case to NoBareMarginValues rather than reporting it twice, and lets a
	// Thickness assigned to a BORDER go: BorderThickness and StrokeThickness are a stroke width
	// on the DmStroke* scale, not an inset on the DmPadding* roles, and reporting them here
	// would tell the reader to fix them with a padding token. The stroke scale now has a ratchet
	// of its own, NoBareStrokeValues, and that is where those two properties are reported.
	private static readonly IReadOnlyList<Regex> PaddingCSharpPatterns =
	[
		new(
			@"(?<![A-Za-z])Padding\s*=(?!>)\s*(?<value>[0-9][0-9.f]*)",
			RegexOptions.Compiled),
		new(
			@"(?<!(?:Margin|BorderThickness|StrokeThickness)\s*=\s*)new\s+(?:[\w]+\.)*Thickness\(\s*(?<value>[0-9][^)]*)\)",
			RegexOptions.Compiled),
	];

	private static readonly IReadOnlyList<Regex> MarginXamlPatterns =
	[
		new(
			@"(?<![A-Za-z])Margin=""(?<value>[0-9][^""]*)""",
			RegexOptions.Compiled),
		new(
			@"<Setter\s+Property=""Margin""\s+Value=""(?<value>[0-9][^""]*)""",
			RegexOptions.Compiled),
	];

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
	// shapes are the attribute, the Setter and a plain C# assignment. The bare `new Thickness(…)`
	// form is deliberately NOT here: on MAUI a stroke width is a scalar, and the Thickness
	// constructor form belongs to padding and margin, which report it already.
	private static readonly IReadOnlyList<Regex> StrokeXamlPatterns =
	[
		new(
			@"(?<![A-Za-z])(?:StrokeThickness|BorderWidth)=""(?<value>[0-9][^""]*)""",
			RegexOptions.Compiled),
		new(
			@"<Setter\s+Property=""(?:StrokeThickness|BorderWidth)""\s+Value=""(?<value>[0-9][^""]*)""",
			RegexOptions.Compiled),
	];

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
	// The lookbehind keeps nothing out that these names could collide with — no MAUI property
	// ends in "MinimumHeightRequest" — but it is written the same way as its five siblings so
	// that the whole family reads identically and a later widening starts from a known shape.
	// HeightRequest and WidthRequest WITHOUT the Minimum prefix are deliberately not scanned:
	// a fixed size is a measured shape, not a floor on a scale, and reporting one here would
	// tell the reader to fix it with a touch-target token that is not what it means.
	private static readonly IReadOnlyList<Regex> MinimumSizeXamlPatterns =
	[
		new(
			@"(?<![A-Za-z])Minimum(?:Height|Width)Request=""(?<value>[0-9][^""]*)""",
			RegexOptions.Compiled),
		new(
			@"<Setter\s+Property=""Minimum(?:Height|Width)Request""\s+Value=""(?<value>[0-9][^""]*)""",
			RegexOptions.Compiled),
	];

	private static readonly IReadOnlyList<Regex> MinimumSizeCSharpPatterns =
	[
		new(
			@"(?<![A-Za-z])Minimum(?:Height|Width)Request\s*=(?!>)\s*(?<value>[0-9][0-9.f]*)",
			RegexOptions.Compiled),
	];

	// Three shapes, because a radius can be written three ways: the CornerRadius attribute (on a
	// Button, a BoxView or an element-form RoundRectangle), a Style setter, and the STRING form
	// of StrokeShape, whose converter parses "RoundRectangle 3" out of one string.
	private static readonly IReadOnlyList<Regex> RadiusXamlPatterns =
	[
		new(
			@"(?<![A-Za-z])CornerRadius=""(?<value>[0-9][^""]*)""",
			RegexOptions.Compiled),
		new(
			@"<Setter\s+Property=""CornerRadius""\s+Value=""(?<value>[0-9][^""]*)""",
			RegexOptions.Compiled),
		new(
			@"RoundRectangle\s+(?<value>[0-9][^""]*)",
			RegexOptions.Compiled),
	];

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
	///		THREE entries, of which TWO are C#. The comment this replaces said "the four C#
	///		entries" of a list that has never had four C# entries: it had four ENTRIES, three of
	///		them C#, and the XAML one was described in the very sentence before. Counted from the
	///		initialiser below as it stands.
	///		</para>
	///		<para>
	///		The XAML entry is <c>DungeonDetailPage.xaml</c>, whose map-object glyphs are stroked at
	///		<c>1.25</c> and <c>1.45</c> — values off the 1 / 1.5 / 2 ladder entirely, and the two
	///		that most need a human decision rather than a mechanical snap. The two C# entries are
	///		hairlines built imperatively, which the ladder already expresses; they are conversions
	///		still to do rather than exemptions. <c>PaletteThumb.cs</c> was the third and was
	///		converted when <c>DmTheme.StrokeHairline</c> was added, which is why it is gone from
	///		here: <see cref="EveryGeometryAllowListEntryStillOffends"/> requires the entry and the
	///		literal to leave in the same change.
	///		</para>
	/// </summary>
	private static readonly HashSet<string> StrokePendingMigration =
		new(StringComparer.OrdinalIgnoreCase)
		{
			Relative("Controls", "AreaSidebarItem", "AreaSidebarItem.xaml.cs"),
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
	///		A regex cannot tell those two meanings apart from a number, and neither can this list:
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

	/// <summary>
	///		Asserts that no literal spacing survives outside the theme dictionaries.
	/// </summary>
	[Fact]
	public void NoBareSpacingValues()
	{
		var offenders = Scan(
				"*.xaml",
				SpacingXamlPatterns,
				SpacingPendingMigration)
			.Concat(
				Scan(
					"*.cs",
					SpacingCSharpPatterns,
					SpacingPendingMigration))
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
			.BeEmpty("every gap must come from the DmSpace* scale in Resources/Theme/Tokens.xaml");
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
		var offenders = Scan(
				"*.xaml",
				PaddingXamlPatterns,
				PaddingPendingMigration)
			.Concat(
				Scan(
					"*.cs",
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
				|| !HasDocumentingComment(
					entry.Found.FileText,
					entry.Found.Index,
					entry.Found.Value))
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
		var offenders = Scan(
				"*.xaml",
				RadiusXamlPatterns,
				RadiusPendingMigration)
			.Concat(
				Scan(
					"*.cs",
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
	///		Asserts that no literal margin survives outside the token declarations.
	/// </summary>
	/// <remarks>
	///		Margin is the same geometry as padding measured from the other side of the border, and
	///		it moves the same pixels. The spec defined scales for spacing, padding, radius and
	///		stroke and simply did not name margin, so an entire class of literal came through the
	///		migration untouched and unmeasured. This fact does not convert anything; it takes the
	///		census, pins it, and makes the next one a failure.
	/// </remarks>
	[Fact]
	public void NoBareMarginValues()
	{
		var offenders = Scan(
				"*.xaml",
				MarginXamlPatterns,
				MarginPendingMigration)
			.Concat(
				Scan(
					"*.cs",
					MarginCSharpPatterns,
					MarginPendingMigration))
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
			.BeEmpty("every offset must come from the DmSpace* scale in Resources/Theme/Tokens.xaml, "
				+ "or be recorded in MarginPendingMigration with the rest of the census");
	}

	/// <summary>
	///		Asserts that no literal stroke width survives outside the token declarations.
	/// </summary>
	/// <remarks>
	///		Spec §3 named four scales — spacing, padding, radius and stroke — and only the first
	///		three were ever ratcheted. The stroke tokens were consumed in markup by hand and the
	///		gap was recorded in a comment beside the padding patterns rather than closed, which is
	///		the same shape as every other defect this branch found: a note asserting a guard is not
	///		a guard. Like <see cref="NoBareMarginValues"/> this fact converts nothing; it takes the
	///		census, pins it, and makes the next one a failure.
	/// </remarks>
	[Fact]
	public void NoBareStrokeValues()
	{
		var offenders = Scan(
				"*.xaml",
				StrokeXamlPatterns,
				StrokePendingMigration)
			.Concat(
				Scan(
					"*.cs",
					StrokeCSharpPatterns,
					StrokePendingMigration))
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
			.BeEmpty("every stroke width must come from the DmStroke* scale in "
				+ "Resources/Theme/Tokens.xaml, or be recorded in StrokePendingMigration "
				+ "with the rest of the census");
	}

	/// <summary>
	///		Asserts that no literal minimum size survives outside the token declarations.
	/// </summary>
	/// <remarks>
	///		The touch target — 44 — was the one shared geometry value in this theme that had no
	///		token at all. It was restated as a bare literal at twelve sites: ten setters across
	///		<c>Implicit.xaml</c> and <c>Controls.Base.xaml</c>, plus the two call sites that carry
	///		a <c>Style=</c> and so opt out of the implicit floor and must restate it
	///		(<c>DmSearchBar.xaml</c> and <c>ControlsPaletteView.xaml</c>). All twelve now name
	///		<c>DmTouchTargetMinimum</c>; this fact is what makes the thirteenth a failure rather
	///		than a thirteenth.
	/// </remarks>
	[Fact]
	public void NoBareMinimumSizeValues()
	{
		var offenders = Scan(
				"*.xaml",
				MinimumSizeXamlPatterns,
				MinimumSizePendingMigration)
			.Concat(
				Scan(
					"*.cs",
					MinimumSizeCSharpPatterns,
					MinimumSizePendingMigration))
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
			.BeEmpty("every minimum size must name a token in Resources/Theme/Tokens.xaml — "
				+ "DmTouchTargetMinimum where it is the platform touch target — or be recorded "
				+ "in MinimumSizePendingMigration with the rest of the census");
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
				SpacingXamlPatterns,
				SpacingCSharpPatterns)
			.Concat(
				Stale(
					"Padding",
					PaddingPendingMigration,
					PaddingXamlPatterns,
					PaddingCSharpPatterns))
			.Concat(
				Stale(
					"Radius",
					RadiusPendingMigration,
					RadiusXamlPatterns,
					RadiusCSharpPatterns))
			.Concat(
				Stale(
					"Margin",
					MarginPendingMigration,
					MarginXamlPatterns,
					MarginCSharpPatterns))
			.Concat(
				Stale(
					"Stroke",
					StrokePendingMigration,
					StrokeXamlPatterns,
					StrokeCSharpPatterns))
			.Concat(
				Stale(
					"MinimumSize",
					MinimumSizePendingMigration,
					MinimumSizeXamlPatterns,
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

			if (!File.ReadAllText(path)
				.Contains(
					value,
					StringComparison.Ordinal))
			{
				yield return $"{listName}: {relative} ({value}) -> the value is gone; remove the exception";
			}
		}
	}

	private static IEnumerable<string> Stale(
		string listName,
		IReadOnlyCollection<string> allowList,
		IReadOnlyList<Regex> xamlPatterns,
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

			var text = File.ReadAllText(path);

			var patterns = relative.EndsWith(
				".cs",
				StringComparison.OrdinalIgnoreCase)
				? cSharpPatterns
				: xamlPatterns;

			var offends = patterns
				.SelectMany(pattern =>
					pattern.Matches(text).Cast<Match>())
				.Any(match =>
					match.Groups["value"].Value.Trim() != ZeroMeansNone);

			if (!offends)
			{
				yield return $"{listName}: {relative} -> no longer offends; remove it from the allow-list";
			}
		}
	}

	private static List<GeometryMatch> Scan(
		string searchPattern,
		IReadOnlyList<Regex> patterns,
		IReadOnlyCollection<string> allowList)
	{
		var scanned = RepoPaths
			.AppSourceFiles(searchPattern)
			.Select(path =>
				Path.GetRelativePath(
					RepoPaths.AppRoot,
					path))
			.Where(relative =>
				!IsTokenDeclaration(relative))
			.Where(relative =>
				!allowList.Contains(relative))
			.ToList();

		// Without this, a directory rename or a widened exclusion turns every geometry fact
		// green while it scans nothing at all — the failure mode behind the structurally inert
		// guards the theme migration shipped.
		scanned
			.Should()
			.NotBeEmpty($"the '{searchPattern}' scan must find application sources or the "
				+ "geometry ratchet built on it is vacuous");

		var found = new List<GeometryMatch>();

		foreach (var relative in scanned)
		{
			var text = File.ReadAllText(
				Path.Combine(
					RepoPaths.AppRoot,
					relative));

			// Matched against the masked copy, reported against the original. Masking replaces
			// the INTERIOR of every comment with spaces and leaves the length untouched, so
			// every index and every line number below still counts against the file as it is on
			// disk — and prose that merely mentions a shape, as Controls.Base.xaml does with
			// "RoundRectangle 4", stops being reported as a literal.
			var searchable = MaskCommentInteriors(text);

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
							text,
							match.Index));
				}
			}
		}

		return found;
	}

	// Reported with the line number AND the matched text: a bare relative path leaves a
	// 700-line XAML file to be searched by hand for whatever tripped the pattern.
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

	private static readonly IReadOnlySet<string> TokenDeclarationFiles =
		new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			Relative("Resources", "Theme", "Tokens.xaml"),
			Relative("Resources", "Theme", "Typography.xaml"),
		};

	private static readonly Regex XmlCommentPattern =
		new(
			@"<!--(?<body>.*?)-->",
			RegexOptions.Singleline | RegexOptions.Compiled);

	// Anchored at the start of the line on purpose. An unanchored //.*$ strips from the "//"
	// inside "ms-appx:///…" and every other URL, erasing the rest of a real line of code.
	private static readonly Regex WholeLineCommentPattern =
		new(
			@"^[ \t]*//(?<body>[^\r\n]*)",
			RegexOptions.Multiline | RegexOptions.Compiled);

	/// <summary>
	///		Replaces the body of every comment with spaces, preserving newlines and total length so
	///		that every index into the result is still an index into the original.
	/// </summary>
	private static string MaskCommentInteriors(string text)
	{
		return WholeLineCommentPattern.Replace(
			XmlCommentPattern.Replace(
				text,
				match =>
					Blank(match)),
			match =>
				Blank(match));
	}

	private static string Blank(Match match)
	{
		var body = match.Groups["body"];

		return string.Concat(
			match.Value[..(body.Index - match.Index)],
			new string(
				body
					.Value
					.Select(character =>
						character is '\n' or '\r'
							? character
							: ' ')
					.ToArray()),
			match.Value[((body.Index - match.Index) + body.Length)..]);
	}

	/// <summary>
	///		Whether a comment closing within <see cref="DocumentationWindowLines"/> lines above the
	///		match actually documents THIS padding.
	/// </summary>
	/// <remarks>
	///		The original test was only whether the string <c>--&gt;</c> appeared in the window,
	///		which in a tree this densely commented exempts almost every asymmetric padding: a
	///		comment about <c>MinimumHeightRequest</c> excused a <c>Padding="7,3,9,5"</c> four lines
	///		below it, proved by construction. A comment that documents a padding says so — it uses
	///		the word, or it quotes the value — so that is what is required, and the check now reads
	///		the comment's own text rather than the fact that one exists.
	/// </remarks>
	private static bool HasDocumentingComment(
		string text,
		int index,
		string value)
	{
		var windowStart = index;

		for (var line = 0; line <= DocumentationWindowLines; line++)
		{
			var previous = text.LastIndexOf(
				'\n',
				Math.Max(
					windowStart - 1,
					0));

			if (previous <= 0)
			{
				windowStart = 0;

				break;
			}

			windowStart = previous;
		}

		var lineEnd = text.IndexOf(
			'\n',
			index);

		if (lineEnd < 0)
		{
			lineEnd = text.Length;
		}

		// The window is bounded by the CLOSING "-->", not the opening "<!--". A one-line comment
		// carries both, so nothing is lost; but the prose blocks this project writes run to six
		// or eight lines, and keying on the opener made a fully documented padding fail because
		// its comment STARTED thirteen lines up while ending five lines up. What the window
		// should ask is "does a comment finish immediately above this attribute", and only the
		// closer answers that. Its BODY is then read, which is the part that was missing.
		var window = text[windowStart..lineEnd];
		var scanned = 0;

		while (true)
		{
			var offset = window.IndexOf(
				"-->",
				scanned,
				StringComparison.Ordinal);

			if (offset < 0)
			{
				break;
			}

			var closer = windowStart + offset;
			scanned = offset + 3;

			var opener = text.LastIndexOf(
				"<!--",
				closer,
				StringComparison.Ordinal);

			if (opener < 0)
			{
				continue;
			}

			var body = text[opener..closer];

			if (body.Contains(
					"padding",
					StringComparison.OrdinalIgnoreCase)
				|| body.Contains(
					value,
					StringComparison.Ordinal))
			{
				return true;
			}
		}

		return false;
	}

	private static string Relative(params string[] segments)
	{
		return Path.Combine(segments);
	}

	private sealed record GeometryMatch(
		string Relative,
		int Line,
		string Matched,
		string Value,
		string FileText,
		int Index);
}
