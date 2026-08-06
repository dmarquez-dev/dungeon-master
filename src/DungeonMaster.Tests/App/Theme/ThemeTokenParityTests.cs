using System.Text.RegularExpressions;
using FluentAssertions;

namespace DungeonMaster.Tests.App.Theme;

public class ThemeTokenParityTests
{
    private static readonly Regex ReferencePattern =
        new(@"\{StaticResource\s+(D[Mm][A-Za-z0-9]*)\s*\}", RegexOptions.Compiled);

    private static readonly Regex DefinitionPattern =
        new(@"x:Key=""(D[Mm][A-Za-z0-9]*)""", RegexOptions.Compiled);

    // Code-behind resolves some theme colours through Application.Current.Resources[...], where
    // the key is a plain string. XAML-only scanning cannot see those, and a stale key there is
    // invisible until it throws at render time — NotebookView.RibbonBrush carried "DMRibbonRead"
    // inside a ternary through the whole of Task 5's migration for exactly that reason.
    private static readonly Regex CSharpKeyLiteral =
        new(@"""(D[Mm][A-Za-z0-9]{2,})""", RegexOptions.Compiled);

    // Doc comments name types with <see cref="DmCard"/>, which is not a resource lookup.
    private static readonly Regex CommentLine =
        new(@"^\s*//.*$", RegexOptions.Compiled | RegexOptions.Multiline);

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
                    missing.Add($"{Path.GetRelativePath(RepoPaths.AppRoot, path)} -> {key}");
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

                missing.Add($"{Path.GetRelativePath(RepoPaths.AppRoot, path)} -> {key}");
            }
        }

        missing.Should().BeEmpty();
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
                DefinitionPattern
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
