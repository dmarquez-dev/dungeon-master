using System.Text.RegularExpressions;
using FluentAssertions;

namespace DungeonMaster.Tests.App.Theme;

public class ThemeTokenParityTests
{
    private static readonly Regex ReferencePattern =
        new(@"\{StaticResource\s+(D[Mm][A-Za-z0-9]*)\s*\}", RegexOptions.Compiled);

    private static readonly Regex DefinitionPattern =
        new(@"x:Key=""(D[Mm][A-Za-z0-9]*)""", RegexOptions.Compiled);

    [Fact]
    public void EveryReferencedThemeKey_IsDefinedUnderResources()
    {
        var definitions = RepoPaths
            .AppSourceFiles("*.xaml")
            .Where(path =>
                path.Contains($"{Path.DirectorySeparatorChar}Resources{Path.DirectorySeparatorChar}"))
            .SelectMany(path =>
                DefinitionPattern.Matches(File.ReadAllText(path)))
            .Select(match =>
                match.Groups[1].Value)
            .ToHashSet(StringComparer.Ordinal);

        definitions.Should().NotBeEmpty("the scan must find resource definitions or the regex is broken");

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
}
