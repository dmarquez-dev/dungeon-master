using System.Text.RegularExpressions;
using FluentAssertions;

namespace DungeonMaster.Tests.App.Theme;

/// <summary>
///		Parity ratchet between the theme keys the application references and the keys the
///		dictionaries under <c>Resources/</c> actually define. A <c>{StaticResource}</c> or a
///		<c>Resources["…"]</c> lookup that names a key nobody declares builds cleanly and fails at
///		render time, so both directions are scanned here rather than left to the running app.
///		Both scans deliberately tolerate the legacy <c>DM</c> casing; see
///		<see cref="ThemeSource.LegacyTolerantKeyDefinitionPattern"/> for why that divergence has
///		to exist.
/// </summary>
public class ThemeTokenParityTests
{
    private static readonly Regex ReferencePattern =
        new(
            @"\{StaticResource\s+(D[Mm][A-Za-z0-9]*)\s*\}",
            RegexOptions.Compiled);

    // Code-behind resolves some theme colours through Application.Current.Resources[...], where
    // the key is a plain string. XAML-only scanning cannot see those, and a stale key there is
    // invisible until it throws at render time — NotebookView.RibbonBrush carried "DMRibbonRead"
    // inside a ternary through the whole of Task 5's migration for exactly that reason.
    private static readonly Regex CSharpKeyLiteral =
        new(
            @"""(D[Mm][A-Za-z0-9]{2,})""",
            RegexOptions.Compiled);

    // Doc comments name types with <see cref="DmCard"/>, which is not a resource lookup.
    private static readonly Regex CommentLine =
        new(
            @"^\s*//.*$",
            RegexOptions.Compiled | RegexOptions.Multiline);

    /// <summary>
    ///		Dm-prefixed string literals that are deliberately not resource keys. Add to this only
    ///		for identifiers that never reach a <c>ResourceDictionary</c> lookup, and say why.
    /// </summary>
    private static readonly HashSet<string> NonResourceLiterals = new(StringComparer.Ordinal)
    {
        // MauiProgram's handler-mapper key: an arbitrary private name for an AppendToMapping
        // registration, never looked up in Application.Current.Resources.
        "DmNativeStyle",
    };

    /// <summary>
    ///		Asserts that every <c>{StaticResource Dm…}</c> reference in application markup names a
    ///		key that is actually declared under <c>Resources/</c>. A dangling reference is not a
    ///		build error in XAML; it throws when the element is first inflated, which for a control
    ///		behind a collapsed expander or an unvisited page can be a long time after the change
    ///		that broke it.
    /// </summary>
    [Fact]
    public void EveryReferencedThemeKey_IsDefinedUnderResources()
    {
        var definitions = ResourceKeyDefinitions()
            .Select(definition =>
                definition.Key)
            .ToHashSet(StringComparer.Ordinal);

        var missing = new List<string>();

        foreach (var path in RepoPaths.AppSourceFiles("*.xaml"))
        {
            foreach (Match match in ReferencePattern.Matches(File.ReadAllText(path)))
            {
                var key = match.Groups[1].Value;

                if (!definitions.Contains(key))
                {
                    var relative = Path.GetRelativePath(
                        RepoPaths.AppRoot,
                        path);

                    missing.Add($"{relative} -> {key}");
                }
            }
        }

        missing.Should().BeEmpty();
    }

    /// <summary>
    ///		Asserts that every Dm-prefixed string literal in application code names a key that
    ///		actually exists under <c>Resources/</c>. This is the C# counterpart of
    ///		<see cref="EveryReferencedThemeKey_IsDefinedUnderResources"/>: it scans literals rather
    ///		than parsing indexer expressions, so a key hidden in a ternary, a switch arm or a const
    ///		is caught just as readily as a direct <c>Resources["Key"]</c> lookup.
    ///		<para>
    ///		It resolves against <c>Resources/Theme/</c> alone, deliberately excluding the legacy
    ///		<c>Resources/Styles/Colors.xaml</c> that Task 10 deleted. Accepting a legacy key
    ///		because it happened to still be defined would have deferred every such failure to
    ///		the moment that file was removed, which is the opposite of what a ratchet is for.
    ///		</para>
    /// </summary>
    [Fact]
    public void EveryThemeKeyStringLiteralInCode_IsDefinedUnderResources()
    {
        var definitions = ResourceKeyDefinitions()
            .Where(definition =>
                definition.Value.Contains(
                    $"{Path.DirectorySeparatorChar}Theme{Path.DirectorySeparatorChar}",
                    StringComparison.Ordinal))
            .Select(definition =>
                definition.Key)
            .ToHashSet(StringComparer.Ordinal);

        var missing = new List<string>();

        foreach (var path in RepoPaths.AppSourceFiles("*.cs"))
        {
            var source = CommentLine.Replace(
                File.ReadAllText(path),
                string.Empty);

            foreach (Match match in CSharpKeyLiteral.Matches(source))
            {
                var key = match.Groups[1].Value;

                if (definitions.Contains(key) || NonResourceLiterals.Contains(key))
                {
                    continue;
                }

                var relative = Path.GetRelativePath(
                    RepoPaths.AppRoot,
                    path);

                missing.Add($"{relative} -> {key}");
            }
        }

        missing.Should().BeEmpty();
    }

    /// <summary>
    ///		Pins <c>DmToolbarHeight</c> to the four inputs it was measured against.
    ///		54.4 is not arithmetic: it is the map toolbar's rendered height, read back from the live
    ///		Windows layout, and Tokens.xaml says in as many words that it must be RE-MEASURED from a
    ///		rendered frame if any font size, padding or stroke inside the toolbar changes. Only its
    ///		existence was tested, so changing any of those inputs silently clipped the tool tiles or
    ///		left a gap under them, in both the toolbar and the sidebar dungeon card that binds the
    ///		same token. This fact converts that silent clip into a loud instruction.
    /// </summary>
    [Fact]
    public void ToolbarHeightInputs_HaveNotMovedSinceItWasMeasured()
    {
        const string Remedy =
            "re-measure DmToolbarHeight from a rendered frame — it is a measured height, not a "
            + "computed one, and an earlier attempt to recalculate it from padding, stroke and an "
            + "assumed font-size multiplier was visibly wrong on screen";

        var detailPage = File.ReadAllText(
            Path.Combine(
                RepoPaths.AppRoot,
                "Views",
                "DungeonDetailPage",
                "DungeonDetailPage.xaml"));

        var toolbarMatch = Regex.Match(
            detailPage,
            @"<Border[^>]*x:Name=""MapToolbar""[^>]*>");

        toolbarMatch
            .Success
            .Should()
            .BeTrue("DungeonDetailPage.xaml should declare a Border named MapToolbar");

        var paddingMatch = Regex.Match(
            toolbarMatch.Value,
            @"Padding=""(?<value>[^""]+)""");

        paddingMatch
            .Success
            .Should()
            .BeTrue("MapToolbar should declare a Padding");

        paddingMatch
            .Groups["value"]
            .Value
            .Should()
            .Be(
                "6,4",
                Remedy);

        ThemeSource
            .DoubleToken(
                "Typography.xaml",
                "DmIconSizeTool")
            .Should()
            .Be(
                22,
                Remedy);

        ThemeSource
            .DoubleToken(
                "Typography.xaml",
                "DmFontSizeMicro")
            .Should()
            .Be(
                10,
                Remedy);

        ThemeSource
            .DoubleToken(
                "Tokens.xaml",
                "DmStrokeHairline")
            .Should()
            .Be(
                1,
                Remedy);
    }

    /// <summary>
    ///		Returns every resource key defined under <c>Resources/</c>, paired with the absolute
    ///		path of the dictionary that declares it so callers can scope by dictionary.
    /// </summary>
    private static IReadOnlyList<KeyValuePair<string, string>> ResourceKeyDefinitions()
    {
        var definitions = RepoPaths
            .AppSourceFiles("*.xaml")
            .Where(path =>
                path.Contains($"{Path.DirectorySeparatorChar}Resources{Path.DirectorySeparatorChar}"))
            .SelectMany(path =>
                ThemeSource
                    .LegacyTolerantKeyDefinitionPattern
                    .Matches(File.ReadAllText(path))
                    .Select(match =>
                        new KeyValuePair<string, string>(
                            match.Groups[1].Value,
                            path)))
            .ToList();

        definitions.Should().NotBeEmpty("the scan must find resource definitions or the regex is broken");

        return definitions;
    }
}
