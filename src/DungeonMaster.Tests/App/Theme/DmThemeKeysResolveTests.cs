using System.Text.RegularExpressions;
using FluentAssertions;

namespace DungeonMaster.Tests.App.Theme;

/// <summary>
///		Guards <c>DmTheme</c>, the C# accessor over the theme dictionaries. Every key it resolves
///		is a string literal, so a typo or a deleted token fails at runtime rather than at build;
///		this fact turns that into a test failure instead.
/// </summary>
public class DmThemeKeysResolveTests
{
    private static readonly Regex ResolveCallPattern =
        new(
            @"Resolve<\w+>\(""(?<key>Dm[A-Za-z0-9]*)""\)",
            RegexOptions.Compiled);

    private static readonly Regex DefinitionPattern =
        new(
            @"x:Key=""(Dm[A-Za-z0-9]*)""",
            RegexOptions.Compiled);

    /// <summary>
    ///		Asserts that every key <c>DmTheme</c> asks for is defined under Resources/Theme.
    /// </summary>
    [Fact]
    public void EveryKeyDmThemeResolves_IsDefinedInTheThemeDictionaries()
    {
        var source = File.ReadAllText(
            Path.Combine(
                RepoPaths.AppRoot,
                "Theme",
                "DmTheme.cs"));

        var requested = ResolveCallPattern
            .Matches(source)
            .Select(match =>
                match.Groups["key"].Value)
            .ToHashSet(StringComparer.Ordinal);

        requested.Should().NotBeEmpty("DmTheme must resolve at least one key or the regex is broken");

        var themeDirectory = Path.Combine(
            RepoPaths.AppRoot,
            "Resources",
            "Theme");

        var defined = Directory
            .EnumerateFiles(
                themeDirectory,
                "*.xaml",
                SearchOption.TopDirectoryOnly)
            .SelectMany(path =>
                DefinitionPattern.Matches(File.ReadAllText(path)))
            .Select(match =>
                match.Groups[1].Value)
            .ToHashSet(StringComparer.Ordinal);

        requested.Except(defined).Should().BeEmpty();
    }

    /// <summary>
    ///		Asserts that no static field initialiser in the application reads <c>DmTheme</c>.
    ///		A field initialiser runs at type-initialisation time, which can precede
    ///		<c>Application.Current.Resources</c>; <c>DmTheme</c> throws when those resources are
    ///		absent, and the message reads like a missing key when the real fault is the ordering.
    ///		This class of bug was hit twice during the theme migration, so it is guarded rather
    ///		than merely commented.
    ///		The pattern is deliberately single-line and excludes <c>=&gt;</c>, so it catches the
    ///		shape that actually occurred (<c>static readonly Color X = DmTheme.Y;</c>) while
    ///		leaving the two safe shapes alone: an expression-bodied property, and a lambda such as
    ///		<c>defaultValueCreator</c> that happens to sit inside a static field initialiser but
    ///		does not run with it.
    /// </summary>
    [Fact]
    public void NoStaticFieldInitialiserReadsDmTheme()
    {
        var staticFieldInitialiser = new Regex(
            @"static\s+(readonly\s+)?[\w<>?\[\].]+\s+\w+\s*=(?!>)[^;\r\n]*\bDmTheme\.",
            RegexOptions.Compiled);

        var offenders = RepoPaths
            .AppSourceFiles("*.cs")
            .Where(path =>
                staticFieldInitialiser.IsMatch(File.ReadAllText(path)))
            .Select(path =>
                Path.GetRelativePath(
                    RepoPaths.AppRoot,
                    path))
            .ToList();

        offenders.Should().BeEmpty();
    }
}
