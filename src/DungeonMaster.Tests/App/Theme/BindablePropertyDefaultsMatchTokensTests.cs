using System.Text.RegularExpressions;
using FluentAssertions;

namespace DungeonMaster.Tests.App.Theme;

/// <summary>
///		Drift guard for the two bindable properties whose C# defaults mirror a typography
///		token. XAML cannot supply a <c>{StaticResource}</c> as a bindable property's default
///		value, so the number is duplicated in code-behind; these facts fail if either side of
///		the pair moves without the other.
/// </summary>
public class BindablePropertyDefaultsMatchTokensTests
{
    /// <summary>
    ///		The bindable defaults under guard, as (source file, property, mirrored token key).
    /// </summary>
    public static TheoryData<string, string, string> Cases =>
        new()
        {
            { @"Controls\DmDivider\DmDivider.xaml.cs", "IconFontSize", "DmIconSizeMedium" },
            { @"Controls\DmInlineEditor\DmInlineEditor.xaml.cs", "FontSize", "DmFontSizeBody" },
        };

    /// <summary>
    ///		Asserts that the numeric default declared for a bindable property equals the value
    ///		of the typography token it mirrors.
    /// </summary>
    /// <param name="relativePath">
    ///		The declaring source file, relative to the application project root.
    /// </param>
    /// <param name="propertyName">
    ///		The name of the bindable property.
    /// </param>
    /// <param name="tokenKey">
    ///		The key of the Typography.xaml token the default mirrors.
    /// </param>
    [Theory]
    [MemberData(nameof(Cases))]
    public void BindableDefault_EqualsTheTokenItMirrors(
        string relativePath,
        string propertyName,
        string tokenKey)
    {
        var source = File.ReadAllText(
            Path.Combine(
                RepoPaths.AppRoot,
                relativePath));

        var declaration = new Regex(
            $@"nameof\({Regex.Escape(propertyName)}\),\s*typeof\(double\),\s*typeof\(\w+\),\s*(?<value>[0-9.]+)",
            RegexOptions.Compiled);

        var match = declaration.Match(source);

        match
            .Success
            .Should()
            .BeTrue($"'{propertyName}' should declare a numeric double default in {relativePath}");

        var declaredDefault = double.Parse(match.Groups["value"].Value);

        declaredDefault
            .Should()
            .Be(TokenValue(tokenKey));
    }

    private static double TokenValue(string key)
    {
        var typography = File.ReadAllText(
            Path.Combine(
                RepoPaths.AppRoot,
                "Resources",
                "Theme",
                "Typography.xaml"));

        var match = Regex.Match(
            typography,
            $@"<x:Double x:Key=""{Regex.Escape(key)}"">(?<value>[0-9.]+)</x:Double>");

        match
            .Success
            .Should()
            .BeTrue($"token '{key}' should be defined in Typography.xaml");

        return double.Parse(match.Groups["value"].Value);
    }
}
