using System.Text.RegularExpressions;
using FluentAssertions;

namespace DungeonMaster.Tests.App.Theme;

/// <summary>
///		Ratchets guarding the spacing scale, the padding roles and the radius tokens introduced
///		by <c>docs/superpowers/specs/2026-08-07-design-system-completion-design.md</c> §3.
///		<para>
///		Each fact carries its own allow-list, seeded with every file that offended when the
///		ratchet was written. A migrating wave removes its own entries and converts the sites;
///		<see cref="EveryGeometryAllowListEntryStillOffends"/> makes an entry that no longer bites
///		a failure, so nothing can be parked and forgotten and no wave can "remove its entry" and
///		see green because the entry never mattered.
///		</para>
///		<para>
///		<c>Resources/Theme/</c> is out of scope for all three: it is where the scales are
///		DEFINED, so a literal there is the definition rather than a call site.
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

    private static readonly IReadOnlyList<Regex> PaddingCSharpPatterns =
    [
        new(
            @"(?<![A-Za-z])Padding\s*=(?!>)\s*(?<value>[0-9][0-9.f]*)",
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
    ];

    /// <summary>
    ///		Files still carrying a literal <c>Spacing</c>, <c>RowSpacing</c>, <c>ColumnSpacing</c>
    ///		or <c>ItemSpacing</c>. Everything here is under <c>Views/</c> and belongs to the wave
    ///		that migrates the views.
    /// </summary>
    private static readonly HashSet<string> SpacingPendingMigration =
        new(StringComparer.OrdinalIgnoreCase)
        {
            Relative("Views", "CreateDungeonPopup", "CreateDungeonPopup.xaml"),
            Relative("Views", "DungeonDetailPage", "ControlsPaletteView.xaml"),
            Relative("Views", "DungeonDetailPage", "DungeonDetailPage.xaml"),
            Relative("Views", "DungeonDetailPage", "ObjectPaletteView.xaml"),
            Relative("Views", "DungeonDetailPage", "ObjectPaletteView.xaml.cs"),
            Relative("Views", "DungeonDetailPage", "SecondaryPaletteView.xaml"),
            Relative("Views", "DungeonListPage", "DungeonListPage.xaml"),
        };

    /// <summary>
    ///		Files still carrying a literal <c>Padding</c>.
    /// </summary>
    private static readonly HashSet<string> PaddingPendingMigration =
        new(StringComparer.OrdinalIgnoreCase)
        {
            Relative("Views", "CreateDungeonPopup", "CreateDungeonPopup.xaml"),
            Relative("Views", "DungeonDetailPage", "ControlsPaletteView.xaml"),
            Relative("Views", "DungeonDetailPage", "DungeonDetailPage.xaml"),
            Relative("Views", "DungeonDetailPage", "ObjectPaletteView.xaml"),
            Relative("Views", "DungeonDetailPage", "SecondaryPaletteView.xaml"),
            Relative("Views", "DungeonListPage", "DungeonListPage.xaml"),
        };

    /// <summary>
    ///		Files still carrying a literal <c>CornerRadius</c> or a string-form
    ///		<c>RoundRectangle N</c>.
    /// </summary>
    private static readonly HashSet<string> RadiusPendingMigration =
        new(StringComparer.OrdinalIgnoreCase)
        {
            Relative("Views", "DungeonDetailPage", "ControlsPaletteView.xaml"),
            Relative("Views", "DungeonDetailPage", "DungeonDetailPage.xaml"),
            Relative("Views", "DungeonDetailPage", "DungeonDetailPage.xaml.cs"),
            Relative("Views", "DungeonDetailPage", "ObjectPaletteView.xaml"),
            Relative("Views", "DungeonDetailPage", "ObjectPaletteView.xaml.cs"),
            Relative("Views", "DungeonDetailPage", "SecondaryPaletteView.xaml"),
            Relative("Views", "DungeonDetailPage", "SecondaryPaletteView.xaml.cs"),
            Relative("Views", "DungeonListPage", "DungeonListPage.xaml"),
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
    ];

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
            .Select(found =>
                IsShapingPadding(found.Value)
                    ? (Found: found, Note: "asymmetric padding with no comment saying what it shapes")
                    : (Found: found, Note: "not on the DmPadding* roles"))
            .Where(entry =>
                !IsShapingPadding(entry.Found.Value)
                || !HasDocumentingComment(
                    entry.Found.FileText,
                    entry.Found.Index))
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
            .OrderBy(
                entry =>
                    entry,
                StringComparer.Ordinal)
            .ToList();

        stale.Should().BeEmpty();
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
        var separator = Path.DirectorySeparatorChar;

        var scanned = RepoPaths
            .AppSourceFiles(searchPattern)
            .Select(path =>
                Path.GetRelativePath(
                    RepoPaths.AppRoot,
                    path))
            .Where(relative =>
                !relative.StartsWith(
                    $"Resources{separator}Theme{separator}",
                    StringComparison.OrdinalIgnoreCase))
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

            foreach (var pattern in patterns)
            {
                foreach (Match match in pattern.Matches(text))
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

    private static bool HasDocumentingComment(
        string text,
        int index)
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

        // The marker is the CLOSING "-->", not the opening "<!--". A one-line comment carries
        // both, so nothing is lost; but the prose blocks this project writes run to six or
        // eight lines, and keying on the opener made a fully documented padding fail because
        // its comment STARTED thirteen lines up while ending five lines up. What the window
        // should ask is "does a comment finish immediately above this attribute", and only the
        // closer answers that.
        return text[windowStart..lineEnd]
            .Contains(
                "-->",
                StringComparison.Ordinal);
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
