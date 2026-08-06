using System.Text.RegularExpressions;
using FluentAssertions;

namespace DungeonMaster.Tests.App.Theme;

/// <summary>
///		Guards the layered theme dictionaries under Resources/Theme.
/// </summary>
public class ThemeDictionaryTests
{
    private static readonly Regex DefinitionPattern =
        new(
            @"x:Key=""(Dm[A-Za-z0-9]*)""",
            RegexOptions.Compiled);

    /// <summary>
    ///		Gets the resource keys every theme consumer is entitled to resolve.
    /// </summary>
    public static TheoryData<string> RequiredKeys =>
    [
        "DmSurfaceTavern", "DmSurfaceTavernRaised", "DmSurfaceTavernRaisedHover",
        "DmSurfaceTavernRaisedActive", "DmBorderTavern", "DmBorderTavernHover",
        "DmTextTavern", "DmTextTavernMuted",
        "DmSurfaceParchment", "DmSurfaceParchmentDeep", "DmBorderParchment",
        "DmTextParchment", "DmTextParchmentMuted",
        "DmAccentGold", "DmAccentGoldLight", "DmAccentGoldDark",
        "DmConfirm", "DmConfirmText", "DmDestructive", "DmDestructiveText",
        "DmNeutralSurface", "DmNeutralBorder",
        "DmSave", "DmSaveText", "DmDiscard", "DmDiscardText",
        "DmRibbonRead", "DmRibbonReadDim", "DmRibbonText",
        "DmBannerEdge", "DmBannerCenter",
        "DmDragPlaceholderStroke", "DmDragPlaceholderFill",
        "DmAreaSelectedBorder", "DmAreaSelectedTint",
        "DmGoldHoverTint", "DmEditorGlowTint",
        "DmMapGrid", "DmMapArea", "DmMapVertex", "DmMapAreaFillActive", "DmMapAreaStrokeActive",
        "DmAccentGoldBrush", "DmBorderTavernBrush", "DmBorderTavernHoverBrush",
        "DmBorderParchmentBrush",
        "DmRadiusTight", "DmRadiusDefault", "DmRadiusLoose",
        "DmStrokeHairline", "DmStrokeEmphasis",
        "DmFontSizeBanner", "DmFontSizeTitle", "DmFontSizeHeading", "DmFontSizeSubheading",
        "DmFontSizeAction", "DmFontSizeEyebrow", "DmFontSizeMicro",
        "DmFontSizeBodyLarge", "DmFontSizeBody", "DmFontSizeBodySmall", "DmFontSizeCaption",
        "DmIconSizeTiny", "DmIconSizeSmall", "DmIconSizeMedium", "DmIconSizeLarge",
        "DmIconSizeXL", "DmIconSizeTool",

        // Step 0 additions: unnamed literals recovered from views, code-behind and
        // Platforms/Windows/App.xaml. See task-2-report.md for the full inventory.
        "DmPaletteParchmentPanel", "DmPaletteParchmentPanelDeep", "DmPaletteParchmentPanelShade",
        "DmPaletteGoldBrown", "DmPaletteBrownDeep",
        "DmPaletteInk850", "DmPaletteBrown700", "DmPaletteBrown650", "DmPaletteBrown600",
        "DmPaletteBrown550",
        "DmPaletteBrownDeepA20", "DmPaletteBrownMidA40",
        "DmPaletteTan600", "DmPaletteTan500", "DmPaletteTan400",
        "DmPaletteRedBright", "DmPaletteRedBrightA20", "DmPaletteRedBrightA40",
        "DmPaletteGoldLightA20", "DmPaletteGoldLightA40",
        "DmPaletteBronzeA28", "DmPaletteBronzeA42Alt", "DmPaletteInkMutedA55",
        "DmPaletteRedA14", "DmPaletteOlive",
    ];

    /// <summary>
    ///		Asserts that a required key is defined somewhere in the theme dictionaries.
    /// </summary>
    /// <param name="key">
    ///		The resource key that must resolve.
    /// </param>
    [Theory]
    [MemberData(nameof(RequiredKeys))]
    public void RequiredKey_IsDefinedInThemeDictionaries(string key)
    {
        DefinedThemeKeys().Should().Contain(key);
    }

    /// <summary>
    ///		Asserts that colour literals are confined to the single token layer.
    ///		Tokens.xaml absorbed the planned Palette.xaml because a markup extension
    ///		in element-content position cannot be converted to a Color; see the
    ///		comment at the top of Tokens.xaml.
    /// </summary>
    [Fact]
    public void TokensIsTheOnlyThemeFileContainingColourLiterals()
    {
        var hexPattern = new Regex(
            @"#[0-9A-Fa-f]{6}([0-9A-Fa-f]{2})?",
            RegexOptions.Compiled);

        var offenders = ThemeFiles()
            .Where(path =>
                !path.EndsWith(
                    "Tokens.xaml",
                    StringComparison.Ordinal))
            .Where(path =>
                hexPattern.IsMatch(File.ReadAllText(path)))
            .Select(path =>
                Path.GetFileName(path))
            .ToList();

        offenders.Should().BeEmpty();
    }

    private static IReadOnlyList<string> ThemeFiles()
    {
        var themeDirectory = Path.Combine(
            RepoPaths.AppRoot,
            "Resources",
            "Theme");

        return Directory
            .EnumerateFiles(
                themeDirectory,
                "*.xaml",
                SearchOption.TopDirectoryOnly)
            .OrderBy(
                path =>
                    path,
                StringComparer.Ordinal)
            .ToList();
    }

    private static HashSet<string> DefinedThemeKeys()
    {
        return ThemeFiles()
            .SelectMany(path =>
                DefinitionPattern.Matches(File.ReadAllText(path)))
            .Select(match =>
                match.Groups[1].Value)
            .ToHashSet(StringComparer.Ordinal);
    }
}
