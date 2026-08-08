using System.Text.RegularExpressions;
using FluentAssertions;

namespace DungeonMaster.Tests.App.Theme;

/// <summary>
///		Guards the layered theme dictionaries under Resources/Theme.
/// </summary>
public class ThemeDictionaryTests
{
    // Matches a <Style … x:Key="Dm…"> declaration. Anchored on the element name so a keyed
    // Color, SolidColorBrush, x:Double or Thickness is not read as a style: those are tokens,
    // and a token with no consumer today is covered by RequiredKeys above, not by
    // NoOrphanedStyleKeys. [^>]*? rather than .*? so it cannot run past the element it opened.
    private static readonly Regex StyleKeyPattern =
        new(
            @"<Style[^>]*?x:Key=""(?<key>Dm[A-Za-z0-9]*)""",
            RegexOptions.Compiled);

    private static readonly Regex CSharpLineCommentPattern =
        new(
            @"//.*$",
            RegexOptions.Compiled | RegexOptions.Multiline);

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
        "DmRibbonRead", "DmRibbonReadDim", "DmRibbonText", "DmRibbonSeal",
        "DmBannerEdge", "DmBannerCenter",
        "DmDragPlaceholderStroke", "DmDragPlaceholderFill",
        "DmAreaSelectedBorder", "DmAreaSelectedTint",
        "DmGoldHoverTint", "DmEditorGlowTint",
        "DmMapGrid", "DmMapArea", "DmMapVertex", "DmMapAreaFillActive", "DmMapAreaStrokeActive",
        "DmMapObjectPlaceholder",
        // DmAccentGoldBrush was here and is gone: it had no consumer anywhere in the repo and
        // was deleted with the orphaned styles rather than kept as a spare alias. Every gold
        // stroke in the app names DmAccentGoldLight and lets MAUI convert it.
        "DmBorderTavernBrush", "DmBorderTavernHoverBrush",
        "DmBorderParchmentBrush",
        "DmRadiusTight", "DmRadiusDefault", "DmRadiusLoose",
        "DmStrokeHairline", "DmStrokeEmphasis", "DmStrokeMedium",
        // The spacing scale and the padding roles. Like the radius tokens above them these
        // have no consumers yet — the wave that defines a scale is not the wave that applies
        // it — so nothing else in this suite would notice one being dropped, renamed or never
        // added at all. This is the only fact standing between the applying wave and a
        // resource key that silently does not exist.
        "DmSpaceXXS", "DmSpaceXS", "DmSpaceSmall", "DmSpaceMedium",
        "DmSpaceLarge", "DmSpaceXL", "DmSpaceXXL",
        "DmPaddingTight", "DmPaddingCompact", "DmPaddingControl",
        "DmPaddingCard", "DmPaddingPanel", "DmPaddingPage",
        // Font families. Bound as FontFamily="..." by Tasks 4-11; a typo in one of
        // these fails silently at runtime rather than at build, so they are guarded.
        "DmFontTitle", "DmFontTitleStrong", "DmFontTitleDecorative",
        "DmFontBody", "DmFontBodyItalic", "DmFontIcon",

        "DmFontSizeBanner", "DmFontSizeTitle", "DmFontSizeHeading", "DmFontSizeSubheading",
        "DmFontSizeAction", "DmFontSizeEyebrow", "DmFontSizeMicro",
        "DmFontSizeBodyLarge", "DmFontSizeBody", "DmFontSizeBodySmall", "DmFontSizeCaption",
        "DmFontSizeMeta",
        "DmIconSizeTiny", "DmIconSizeSmall", "DmIconSizeMedium", "DmIconSizeLarge",
        "DmIconSizeXL", "DmIconSizeTool",

        // Step 0 additions: literals recovered from views, code-behind and
        // Platforms/Windows/App.xaml that carried no name before Task 2. Named from
        // their usage sites; see task-2-report.md for the full inventory.
        "DmSurfaceToolPanel", "DmSurfaceToolChip", "DmSurfaceToolChipHover",
        "DmBorderToolChip", "DmTextToolPanel",
        "DmSurfacePopup", "DmBorderPopup", "DmDivider", "DmSurfaceStepper",
        "DmTextNotebook",
        "DmScrollbarThumb", "DmScrollbarThumbDisabled", "DmTextSelection",
        "DmSurfaceComboItemHover", "DmSurfaceComboItemPressed", "DmTextComboItemDisabled",
        "DmDestructiveBright", "DmPopupButtonHover", "DmPopupButtonPressed",
        "DmPopupButtonDestructiveHover", "DmPopupButtonDestructivePressed",
        "DmHairline", "DmHairlineStrong", "DmTextPlaceholder",
        "DmDestructiveHoverSurface", "DmSavedStamp",
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
        DefinedThemeKeys()
            .Should()
            .Contain(
                key,
                "'{0}' is required by a theme consumer but no dictionary under Resources/Theme "
                + "declares x:Key=\"{0}\"",
                key);
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
        var offenders = ThemeSource
            .ThemeFiles()
            .Where(path =>
                !path.EndsWith(
                    "Tokens.xaml",
                    StringComparison.Ordinal))
            .Where(path =>
                ThemeSource.HexLiteralPattern.IsMatch(File.ReadAllText(path)))
            .Select(path =>
                Path.GetFileName(path))
            .ToList();

        offenders.Should().BeEmpty();
    }

    /// <summary>
    ///		Asserts that every file under Resources/Theme is merged into App.xaml, and that every
    ///		dictionary merged from Resources/Theme in App.xaml names a file that exists. A
    ///		dictionary that is never merged satisfies every other guard in this class and in
    ///		<c>DmThemeKeysResolveTests</c> — both treat "defined in any file under Resources/Theme"
    ///		as "resolvable" — while the app throws at runtime on the first
    ///		<c>{StaticResource}</c> reference into it. This is the only fact that would catch that.
    /// </summary>
    [Fact]
    public void EveryThemeFileIsMergedInAppXaml()
    {
        var themeSources = ThemeSource
            .ThemeFileSources()
            .ToHashSet(StringComparer.Ordinal);

        var mergedSources = MergedThemeSourcesInAppXaml()
            .ToHashSet(StringComparer.Ordinal);

        themeSources.Should().BeEquivalentTo(mergedSources);
    }

    /// <summary>
    ///		Asserts that App.xaml merges the theme dictionaries in a dependency-safe order.
    ///		A merged dictionary may reference keys from those merged before it and not from those
    ///		merged after, so the order in App.xaml is load-bearing markup — Controls.Tavern.xaml
    ///		derives from <c>DmButtonBase</c> in Controls.Base.xaml, and every Controls file
    ///		references the tokens and typography above it. <c>EveryThemeFileIsMergedInAppXaml</c>
    ///		compares sets, which is order-insensitive: the seven entries can be shuffled into any
    ///		permutation, pass every other fact in this suite, and throw at startup on the first
    ///		<c>{StaticResource}</c> that reaches backwards. This is the fact that reads the order.
    /// </summary>
    [Fact]
    public void ThemeDictionariesAreMergedInDependencyOrder()
    {
        var merged = MergedThemeSourcesInAppXaml()
            .Select(source =>
                Path.GetFileName(source))
            .ToList();

        var controlsIndices = merged
            .Select((fileName, index) =>
                (fileName, index))
            .Where(entry =>
                entry.fileName.StartsWith(
                    "Controls.",
                    StringComparison.Ordinal))
            .ToList();

        controlsIndices
            .Should()
            .NotBeEmpty("App.xaml must merge at least one Controls dictionary or this fact is vacuous");

        var firstControls = controlsIndices.Min(entry =>
            entry.index);

        foreach (var foundation in new[] { "Tokens.xaml", "Typography.xaml" })
        {
            var index = merged.IndexOf(foundation);

            index
                .Should()
                .BeGreaterThanOrEqualTo(
                    0,
                    $"App.xaml must merge {foundation}");

            index
                .Should()
                .BeLessThan(
                    firstControls,
                    $"{foundation} defines the tokens every Controls dictionary references, so it "
                    + $"must be merged before {merged[firstControls]}");
        }

        var controlsBaseIndex = merged.IndexOf("Controls.Base.xaml");

        controlsBaseIndex
            .Should()
            .BeGreaterThanOrEqualTo(
                0,
                "App.xaml must merge Controls.Base.xaml");

        var derivedControls = controlsIndices
            .Where(entry =>
                entry.fileName != "Controls.Base.xaml")
            .ToList();

        foreach (var derived in derivedControls)
        {
            derived
                .index
                .Should()
                .BeGreaterThan(
                    controlsBaseIndex,
                    $"{derived.fileName} derives styles with BasedOn from Controls.Base.xaml, so "
                    + "Controls.Base.xaml must be merged first");
        }

        merged[^1]
            .Should()
            .Be(
                "Implicit.xaml",
                "the implicit style floor references keys from every dictionary above it and must "
                + "be merged last");
    }

    /// <summary>
    ///		Asserts that every <c>Style</c> declared with an <c>x:Key</c> under Resources/Theme has
    ///		at least one consumer OUTSIDE A COMMENT.
    ///		<para>
    ///		The "outside a comment" clause is the entire point and it is not defensive coding. The
    ///		hand census this fact replaces recorded <c>DmBorderTavernStyle</c> as live on the
    ///		strength of a single grep hit — which turned out to be a mention of the key inside the
    ///		prose comment at the top of Controls.Base.xaml. It had no call site at all. Seventeen
    ///		keys were dead when that census said six were, and every one of them shipped, built
    ///		clean and rendered nothing.
    ///		</para>
    ///		<para>
    ///		A consumer is a <c>{StaticResource Key}</c> reference in any application markup —
    ///		including a <c>BasedOn</c>, which is how a base style earns its keep — or a
    ///		<c>"Key"</c> string literal in application code, which is how <c>DmTheme</c> resolves
    ///		the two button styles <c>DmConfirmPopup</c> swaps between. Comments are stripped from
    ///		both before anything is counted.
    ///		</para>
    ///		<para>
    ///		A style consumed only by another style that is itself orphaned is reported one layer at
    ///		a time: the leaf fails, and deleting it exposes the base on the next run. That is
    ///		deliberate — resolving the whole chain in one pass would name files whose only fault is
    ///		being downstream of the real one.
    ///		</para>
    /// </summary>
    [Fact]
    public void NoOrphanedStyleKeys()
    {
        var styleKeys = ThemeSource
            .ThemeFiles()
            .SelectMany(path =>
                StyleKeyPattern
                    .Matches(StripComments(File.ReadAllText(path)))
                    .Select(match =>
                        new
                        {
                            Key = match.Groups["key"].Value,
                            File = Path.GetFileName(path),
                        }))
            .ToList();

        styleKeys
            .Should()
            .NotBeEmpty("the theme dictionaries must declare keyed styles or this fact is vacuous");

        var markup = RepoPaths
            .AppSourceFiles("*.xaml")
            .Select(path =>
                StripComments(File.ReadAllText(path)))
            .ToList();

        var code = RepoPaths
            .AppSourceFiles("*.cs")
            .Select(path =>
                StripCSharpComments(File.ReadAllText(path)))
            .ToList();

        var orphans = new List<string>();

        foreach (var style in styleKeys)
        {
            var markupReference = new Regex(
                $@"\{{StaticResource\s+{Regex.Escape(style.Key)}\s*\}}",
                RegexOptions.Compiled);

            var codeReference = new Regex(
                $@"""{Regex.Escape(style.Key)}""",
                RegexOptions.Compiled);

            var consumed =
                markup.Any(text =>
                    markupReference.IsMatch(text))
                || code.Any(text =>
                    codeReference.IsMatch(text));

            if (!consumed)
            {
                orphans.Add(
                    $"{style.File} -> {style.Key} is declared as a Style but nothing outside a "
                    + "comment consumes it; adopt it at a call site or delete it");
            }
        }

        orphans
            .Should()
            .BeEmpty(
                "an unconsumed style is an abstraction that renders nowhere — it builds clean, "
                + "launches clean and is invisible until somebody counts");
    }

    private static string StripComments(string text)
    {
        return ThemeSource.XmlCommentPattern.Replace(
            text,
            string.Empty);
    }

    private static string StripCSharpComments(string text)
    {
        // Line comments only. A theme key inside a /* */ block would be missed, and the
        // application has none; a block-comment stripper that does not understand string
        // literals would corrupt more than it cleaned.
        return CSharpLineCommentPattern.Replace(
            text,
            string.Empty);
    }

    private static IReadOnlyList<string> MergedThemeSourcesInAppXaml()
    {
        var appXamlPath = Path.Combine(
            RepoPaths.AppRoot,
            "App.xaml");

        // [A-Za-z0-9./] rather than [A-Za-z0-9.]: a dictionary in a subdirectory of
        // Resources/Theme was invisible to this scan, so merging it or not made no difference.
        var sourcePattern = new Regex(
            @"Source=""(Resources/Theme/[A-Za-z0-9./]+\.xaml)""",
            RegexOptions.Compiled);

        var sources = sourcePattern
            .Matches(File.ReadAllText(appXamlPath))
            .Select(match =>
                match.Groups[1].Value)
            .ToList();

        sources
            .Should()
            .NotBeEmpty("App.xaml must merge theme dictionaries or the Source regex is broken");

        return sources;
    }

    private static HashSet<string> DefinedThemeKeys()
    {
        return ThemeSource
            .ThemeFiles()
            .SelectMany(path =>
                ThemeSource.KeyDefinitionPattern.Matches(File.ReadAllText(path)))
            .Select(match =>
                match.Groups[1].Value)
            .ToHashSet(StringComparer.Ordinal);
    }
}
