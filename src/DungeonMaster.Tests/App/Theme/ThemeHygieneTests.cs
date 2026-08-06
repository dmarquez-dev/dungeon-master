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

    /// <summary>
    ///		Files not yet migrated onto the theme system. Each migration task deletes its own
    ///		entries from this set BEFORE migrating, so the test fails first and passes after.
    ///		When this set is empty, the migration is complete and this comment can go.
    /// </summary>
    private static readonly HashSet<string> PendingMigration = new(StringComparer.OrdinalIgnoreCase)
    {
        // Task 5 — Controls
        @"Controls\AreaSidebarItem\AreaSidebarItem.xaml",
        @"Controls\DmCard\DmCard.xaml",
        @"Controls\DmConfirmPopup\DmConfirmPopup.xaml",
        @"Controls\DmDivider\DmDivider.xaml",
        @"Controls\DmFlagBanner\DmFlagBanner.xaml",
        @"Controls\DmInlineEditor\DmInlineEditor.xaml.cs",
        @"Controls\DmPopup\DmPopup.xaml",
        @"Controls\DmSearchBar\DmSearchBar.xaml",
        @"Controls\FloorSidebarItem\FloorSidebarItem.xaml",
        @"Controls\NotebookView\NotebookView.xaml",
        @"Controls\NotebookView\NotebookView.xaml.cs",

        // Task 6 — CreateDungeonPopup
        @"Views\CreateDungeonPopup\CreateDungeonPopup.xaml",

        // Task 7 — DungeonListPage
        @"Views\DungeonListPage\DungeonListPage.xaml",

        // Task 8 — DungeonDetailPage and palettes
        @"Views\DungeonDetailPage\ControlsPaletteView.xaml",
        @"Views\DungeonDetailPage\DungeonDetailPage.xaml",
        @"Views\DungeonDetailPage\DungeonDetailPage.xaml.cs",
        @"Views\DungeonDetailPage\ObjectPaletteView.xaml",
        @"Views\DungeonDetailPage\SecondaryPaletteView.xaml",
        @"Views\DungeonDetailPage\SecondaryPaletteView.xaml.cs",

        // Task 9 — FloorMapDrawable
        @"Graphics\FloorMapDrawable.cs",

        // Task 10 — WinUI interop and stock dictionary removal
        @"Platforms\Windows\App.xaml",
        @"Views\DungeonDetailPage\ObjectPaletteView.xaml.cs",
        @"Resources\Styles\Colors.xaml",

        // Task 11 — DmTheme port
        @"Theme\DmColors.cs",
    };

    /// <summary>
    ///		Numeric font-size defaults that cannot resolve through DmTheme because
    ///		BindableProperty.Create runs in a static initialiser, before Application.Current
    ///		and its merged dictionaries exist. BindablePropertyDefaultsMatchTokensTests
    ///		asserts these literals still equal the tokens they mirror.
    /// </summary>
    private static readonly HashSet<string> BindableDefaultExemptions = new(StringComparer.OrdinalIgnoreCase)
    {
        @"Controls\DmDivider\DmDivider.xaml.cs",
        @"Controls\DmInlineEditor\DmInlineEditor.xaml.cs",
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
                !BindableDefaultExemptions.Contains(relative))
            .Where(relative =>
                pattern.IsMatch(
                    File.ReadAllText(
                        Path.Combine(
                            RepoPaths.AppRoot,
                            relative))))
            .ToList();
    }
}
