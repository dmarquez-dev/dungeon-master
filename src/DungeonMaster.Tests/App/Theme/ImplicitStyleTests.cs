using System.Text.RegularExpressions;
using FluentAssertions;

namespace DungeonMaster.Tests.App.Theme;

/// <summary>
///		Guards Implicit.xaml, the geometry-and-states floor.
///		Every other fact in this suite is key-based — it reads <c>x:Key="Dm…"</c> — and an implicit
///		style is <c>TargetType</c>-only, so until these facts existed the whole file was unguarded:
///		deleting the Button style's <c>MinimumHeightRequest</c> setter reproduced the measured Task
///		10 regression (every unstyled Button dropping below the 44px touch target app-wide, and the
///		search Entry shrinking from 51px to 42px) with all 256 tests green. The file exists BECAUSE
///		that regression happened, which is exactly why its contents need a guard rather than a
///		comment.
/// </summary>
public class ImplicitStyleTests
{
    private static readonly Regex StylePattern =
        new(
            @"<Style\b[^>]*TargetType=""(?<target>[A-Za-z0-9]+)""[^>]*>(?<body>.*?)</Style>",
            RegexOptions.Compiled | RegexOptions.Singleline);

    private static readonly Regex SetterPropertyPattern =
        new(
            @"<Setter\s+Property=""(?<property>[A-Za-z0-9.]+)""",
            RegexOptions.Compiled);

    /// <summary>
    ///		The control-level properties Implicit.xaml must NOT set. This is the invariant the file
    ///		states at the top of itself: colour and font belong exclusively to the keyed Dm* styles
    ///		in Controls.Base.xaml and its derivatives, and setting either here as well would give
    ///		the app two sources of truth for the same property.
    /// </summary>
    private static readonly string[] ForbiddenControlProperties =
    [
        "TextColor",
        "BackgroundColor",
        "FontSize",
    ];

    /// <summary>
    ///		The styles that carry colour by design and are therefore outside the invariant above.
    ///		Page and Shell are app chrome with exactly one instance each and no keyed alternative
    ///		to defer to, so the rule is about the control-level styles and not about the file.
    /// </summary>
    private static readonly string[] ChromeTargets =
    [
        "Page",
        "Shell",
    ];

    /// <summary>
    ///		Gets the targets that must have an implicit style, and the setters each must carry.
    /// </summary>
    public static TheoryData<string, string[]> RequiredStyles =>
        new()
        {
            {
                "Button",
                [
                    "Padding",
                    "MinimumHeightRequest",
                    "MinimumWidthRequest",
                    "CornerRadius",
                    "BorderWidth",
                ]
            },
            { "Entry", ["MinimumHeightRequest"] },
            { "Editor", ["MinimumHeightRequest"] },
            { "Picker", ["MinimumHeightRequest"] },
            { "Label", ["FontFamily"] },
            { "Page", ["BackgroundColor"] },
            { "Shell", ["Shell.BackgroundColor"] },
        };

    /// <summary>
    ///		Asserts that Implicit.xaml declares a style for the required target and that the style
    ///		carries every setter the target depends on it for.
    /// </summary>
    /// <param name="target">
    ///		The style's <c>TargetType</c>.
    /// </param>
    /// <param name="requiredSetters">
    ///		The properties the style must set.
    /// </param>
    [Theory]
    [MemberData(nameof(RequiredStyles))]
    public void ImplicitStyle_DeclaresItsRequiredSetters(
        string target,
        string[] requiredSetters)
    {
        var styles = Styles();

        styles
            .Should()
            .ContainKey(
                target,
                "Implicit.xaml is the only floor under unstyled {0} elements; without this style "
                + "they take nothing at all",
                target);

        var declared = SetterProperties(styles[target]);

        foreach (var setter in requiredSetters)
        {
            declared
                .Should()
                .Contain(
                    setter,
                    "the implicit {0} style must set {1}; MAUI supplies no default for it, so "
                    + "removing the setter silently changes the rendered geometry of every "
                    + "unstyled {0} in the app",
                    target,
                    setter);
        }
    }

    /// <summary>
    ///		Asserts that the implicit Button style carries the interaction states it was written
    ///		for. Task 4b measured only padding and minimum size and missed the state group entirely,
    ///		leaving every unstyled Button with no hover or press feedback at all — 16 of the app's
    ///		20 Button elements today; the values here are byte-identical to the stock ones this file
    ///		replaces.
    /// </summary>
    [Fact]
    public void ImplicitButtonStyle_CarriesItsCommonStates()
    {
        var button = Styles()["Button"];

        Names(
            button,
            "VisualStateGroup")
            .Should()
            .Contain(
                "CommonStates",
                "without the CommonStates group every Button that carries no Style= has no hover "
                + "or press feedback at all");

        var states = Names(
            button,
            "VisualState");

        foreach (var state in new[] { "Normal", "PointerOver", "Pressed" })
        {
            states
                .Should()
                .Contain(
                    state,
                    "the {0} state is one of the three the stock Button style carried",
                    state);
        }
    }

    /// <summary>
    ///		Asserts that no control-level implicit style sets colour or font size. Page and Shell
    ///		are excluded because they are app chrome and carry colour tokens by design; the Label
    ///		style's single FontFamily setter is the file's one deliberate exception and is not a
    ///		member of the forbidden set.
    /// </summary>
    [Fact]
    public void ImplicitControlStyles_SetNoColourOrFontSize()
    {
        var offenders = new List<string>();

        foreach (var (target, body) in Styles())
        {
            if (ChromeTargets.Contains(target))
            {
                continue;
            }

            foreach (var property in SetterProperties(body).Where(ForbiddenControlProperties.Contains))
            {
                offenders.Add($"{target} -> {property}");
            }
        }

        offenders
            .Should()
            .BeEmpty(
                "colour and font size belong exclusively to the keyed Dm* styles in "
                + "Controls.Base.xaml and its derivatives; setting either here as well gives the "
                + "app two sources of truth for the same property");
    }

    private static Dictionary<string, string> Styles()
    {
        // Comments first: this file's prose names the very properties and elements below, and a
        // scan that reads the commentary as markup both misses real setters and invents them.
        var markup = ThemeSource.XmlCommentPattern.Replace(
            ThemeSource.ThemeFileText("Implicit.xaml"),
            string.Empty);

        var styles = StylePattern
            .Matches(markup)
            .ToDictionary(
                match =>
                    match.Groups["target"].Value,
                match =>
                    match.Groups["body"].Value,
                StringComparer.Ordinal);

        styles
            .Should()
            .NotBeEmpty("Implicit.xaml must declare styles or this whole class is vacuous");

        return styles;
    }

    private static IReadOnlyList<string> Names(
        string styleBody,
        string elementName)
    {
        return Regex
            .Matches(
                styleBody,
                $@"<{elementName}\s+x:Name=""(?<name>[A-Za-z0-9]+)""")
            .Select(match =>
                match.Groups["name"].Value)
            .ToList();
    }

    private static IReadOnlyList<string> SetterProperties(string styleBody)
    {
        return SetterPropertyPattern
            .Matches(styleBody)
            .Select(match =>
                match.Groups["property"].Value)
            .ToList();
    }
}
