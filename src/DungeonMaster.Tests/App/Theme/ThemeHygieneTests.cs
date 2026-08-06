using System.Text.RegularExpressions;
using FluentAssertions;

namespace DungeonMaster.Tests.App.Theme;

/// <summary>
///		Ratchet guarding the migration of application source onto the theme dictionaries.
///		Every file that still carries a raw colour, font size or font family is named in
///		<see cref="PendingMigration"/>; removing an entry makes these facts fail until the
///		file is migrated.
/// </summary>
public class ThemeHygieneTests
{
    private static readonly Regex HexPattern =
        new(
            @"#[0-9A-Fa-f]{6}([0-9A-Fa-f]{2})?",
            RegexOptions.Compiled);

    // (?<![A-Za-z]) stops this matching DmDivider's IconFontSize bindable property.
    private static readonly Regex BareFontSizePattern =
        new(
            @"(?<![A-Za-z])FontSize=""[0-9]",
            RegexOptions.Compiled);

    private static readonly Regex BareFontFamilyPattern =
        new(
            @"FontFamily=""[A-Za-z]",
            RegexOptions.Compiled);

    private static readonly Regex LegacyThemeKeyPattern =
        new(
            @"\{StaticResource\s+DM[A-Za-z0-9]*\s*\}",
            RegexOptions.Compiled);

    /// <summary>
    ///		Files not yet migrated onto the theme system. Each migration task deletes its own
    ///		entries from this set BEFORE migrating, so the test fails first and passes after.
    ///		When this set is empty, the migration is complete and this comment can go.
    /// </summary>
    private static readonly HashSet<string> PendingMigration = new(StringComparer.OrdinalIgnoreCase)
    {
        // ─────────────────────────────────────────────────────────────────────────────
        // SCOPE: the two typography facts are XAML-ONLY BY DESIGN.
        //
        // NoRawColourLiteralsOutsideThePalette scans *.xaml AND *.cs. The font-size and
        // font-family facts scan *.xaml alone. That asymmetry is a decision, not an
        // oversight, and this comment is the only place that records it.
        //
        // Colour in code-behind has a behaviour-preserving destination: DmColors (and,
        // after Task 11, a resource accessor), so the same literal moves to a name with
        // no pixel change. Typography in code-behind does not. Every C# font size in the
        // app is OFF the declared 10/12/14/16/18/20/24 scale, so migrating one is a
        // visible restyle that needs authorisation — it cannot ride a behaviour-preserving
        // migration, and the ratchet must not be able to force it.
        //
        // The known C# typography literals, deferred and visible rather than silent:
        //
        //   Views\DungeonDetailPage\ObjectPaletteView.xaml.cs:206   FontSize = 13
        //                                                    :264   FontSize = 11
        //                                                    :284   FontSize = 18
        //   Views\DungeonDetailPage\SecondaryPaletteView.xaml.cs:53 FontSize = 11
        //   Graphics\FloorMapDrawable.cs:529                        canvas.FontSize = 9f
        //
        // 13, 11, 11 and 9 are all off-scale; only the 18 maps cleanly. Extending the two
        // typography facts to *.cs would therefore fail immediately on four offenders whose
        // only exit is a design decision, forcing allow-list entries that no migration task
        // can legitimately clear. That is the ratchet running backwards, so it is not done.
        //
        // Consequence to keep in view: this set can reach empty — "migration complete" —
        // with those five literals still live. They are a follow-up, not a leftover.
        // ─────────────────────────────────────────────────────────────────────────────

        // Task 10 — WinUI interop and stock dictionary removal
        @"Platforms\Windows\App.xaml",
        @"Views\DungeonDetailPage\ObjectPaletteView.xaml.cs",
        @"Resources\Styles\Colors.xaml",

        // Task 11 — DmTheme port
        @"Theme\DmColors.cs",
    };

    /// <summary>
    ///		Asserts that colour literals appear only in the theme dictionaries.
    /// </summary>
    [Fact]
    public void NoRawColourLiteralsOutsideThePalette()
    {
        Offenders("*.xaml", HexPattern)
            .Concat(Offenders("*.cs", HexPattern))
            .Should()
            .BeEmpty();
    }

    /// <summary>
    ///		Asserts that numeric font sizes appear only in the theme dictionaries.
    /// </summary>
    [Fact]
    public void NoBareFontSizesOutsideTheThemeDictionaries()
    {
        Offenders("*.xaml", BareFontSizePattern).Should().BeEmpty();
    }

    /// <summary>
    ///		Asserts that font family names appear only in the theme dictionaries.
    /// </summary>
    [Fact]
    public void NoBareFontFamiliesOutsideTheThemeDictionaries()
    {
        Offenders("*.xaml", BareFontFamilyPattern).Should().BeEmpty();
    }

    /// <summary>
    ///		Asserts that no migrated file still references a legacy <c>DM</c>-prefixed key from
    ///		<c>Resources/Styles/Colors.xaml</c>, which Task 10 deletes. Migrating a file means
    ///		moving it onto the <c>Dm</c>-prefixed tokens in <c>Resources/Theme/Tokens.xaml</c>,
    ///		not merely off raw literals.
    /// </summary>
    [Fact]
    public void NoLegacyThemeKeyReferencesOutsideThePendingMigrationSet()
    {
        Offenders("*.xaml", LegacyThemeKeyPattern).Should().BeEmpty();
    }

    /// <summary>
    ///		Asserts that every allow-listed path names a file that exists and still carries at
    ///		least one of the literals the three facts above forbid. Without this the ratchet is
    ///		not self-checking as it shrinks: a stale or mistyped entry exempts nothing, and a
    ///		migration task can "remove its entry" and see green because the entry never bit.
    /// </summary>
    [Fact]
    public void EveryAllowListedFileExistsAndStillOffends()
    {
        var stale = PendingMigration
            .Where(relative =>
                !StillOffends(relative))
            .OrderBy(relative =>
                relative, StringComparer.Ordinal)
            .ToList();

        stale.Should().BeEmpty();
    }

    private static bool StillOffends(string relative)
    {
        var path = Path.Combine(
            RepoPaths.AppRoot,
            relative);

        if (!File.Exists(path))
        {
            return false;
        }

        var text = File.ReadAllText(path);

        return HexPattern.IsMatch(text)
            || BareFontSizePattern.IsMatch(text)
            || BareFontFamilyPattern.IsMatch(text)
            || LegacyThemeKeyPattern.IsMatch(text);
    }

    private static IEnumerable<string> Offenders(
        string searchPattern,
        Regex pattern)
    {
        var separator = Path.DirectorySeparatorChar;

        return RepoPaths
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
                !PendingMigration.Contains(relative))
            .Where(relative =>
                pattern.IsMatch(
                    File.ReadAllText(
                        Path.Combine(
                            RepoPaths.AppRoot,
                            relative))))
            .ToList();
    }
}
