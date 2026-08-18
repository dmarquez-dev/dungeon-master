using System.Globalization;
using System.Text.RegularExpressions;
using FluentAssertions;

namespace DungeonMaster.Tests.App.Theme;

/// <summary>
///		Drift guard for the bindable properties whose C# defaults mirror a typography token.
///		XAML cannot supply a <c>{StaticResource}</c> as a bindable property's default value, so
///		the size or family name is duplicated in code-behind; these facts fail if either side of
///		the pair moves without the other.
/// </summary>
public class BindablePropertyDefaultsMatchTokensTests
{
	/// <summary>
	///		The numeric bindable defaults under guard, as (source file, property, mirrored
	///		token key).
	/// </summary>
	public static TheoryData<string, string, string> Cases =>
		new()
		{
			{
				Path.Combine(
					"Controls",
					"DmDivider",
					"DmDivider.xaml.cs"),
				"IconFontSize",
				"DmIconSizeMedium"
			},
			{
				Path.Combine(
					"Controls",
					"DmInlineEditor",
					"DmInlineEditor.xaml.cs"),
				"FontSize",
				"DmFontSizeBody"
			},
		};

	/// <summary>
	///		The font-family bindable defaults under guard, as (source file, property, mirrored
	///		token key). A family default drifts exactly the way a size default does — the string
	///		is a second copy of a token's value with nothing tying the two together — so it wants
	///		the same guard rather than a new mechanism.
	/// </summary>
	public static TheoryData<string, string, string> FamilyCases =>
		new()
		{
			{
				Path.Combine(
					"Controls",
					"DmInlineEditor",
					"DmInlineEditor.xaml.cs"),
				"FontFamily",
				"DmFontBody"
			},
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

		var declaredDefault = double.Parse(
			match.Groups["value"].Value,
			CultureInfo.InvariantCulture);

		declaredDefault
			.Should()
			.Be(TokenValue(tokenKey));
	}

	/// <summary>
	///		Asserts that the font-family string declared for a bindable property equals the value
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
	[MemberData(nameof(FamilyCases))]
	public void BindableFamilyDefault_EqualsTheTokenItMirrors(
		string relativePath,
		string propertyName,
		string tokenKey)
	{
		var source = File.ReadAllText(
			Path.Combine(
				RepoPaths.AppRoot,
				relativePath));

		var declaration = new Regex(
			$@"nameof\({Regex.Escape(propertyName)}\),\s*typeof\(string\),\s*typeof\(\w+\),\s*""(?<value>[^""]+)""",
			RegexOptions.Compiled);

		var match = declaration.Match(source);

		match
			.Success
			.Should()
			.BeTrue($"'{propertyName}' should declare a string default in {relativePath}");

		match
			.Groups["value"]
			.Value
			.Should()
			.Be(FamilyTokenValue(tokenKey));
	}

	private static double TokenValue(string key) =>
		ThemeSource.DoubleToken(
			"Typography.xaml",
			key);

	private static string FamilyTokenValue(string key) =>
		ThemeSource.StringToken(
			"Typography.xaml",
			key);

	/// <summary>
	///		Asserts that the numeric radius argument on <c>DmBorderBase</c>'s
	///		<c>StrokeShape="RoundRectangle 3"</c> setter in Controls.Base.xaml equals
	///		<c>DmRadiusDefault</c> in Tokens.xaml. <c>StrokeShape</c>'s converter parses a shape
	///		name and its arguments from one string and cannot take a markup extension for the
	///		radius, so the value is unavoidably a literal there; the comment beside it says the
	///		literal must move with the token, and this is what enforces that.
	/// </summary>
	[Fact]
	public void BorderBaseStrokeShapeRadius_EqualsDmRadiusDefault()
	{
		var controlsBase = File.ReadAllText(
			Path.Combine(
				RepoPaths.AppRoot,
				"Resources",
				"Theme",
				"Controls.Base.xaml"));

		var strokeShapeMatch = Regex.Match(
			controlsBase,
			@"x:Key=""DmBorderBase""[\s\S]*?Property=""StrokeShape""\s+Value=""RoundRectangle\s+(?<value>[0-9.]+)""");

		strokeShapeMatch
			.Success
			.Should()
			.BeTrue("'DmBorderBase' should declare a StrokeShape='RoundRectangle n' setter in Controls.Base.xaml");

		var declaredRadius = double.Parse(
			strokeShapeMatch.Groups["value"].Value,
			CultureInfo.InvariantCulture);

		// DmRadiusDefault is an x:Int32, not an x:Double — see the note beside the radius
		// tokens in Tokens.xaml. The literal here is still compared as a double because it is
		// parsed out of a StrokeShape string, where "3" and "3.0" would both be legal.
		declaredRadius
			.Should()
			.Be(
				ThemeSource.Int32Token(
					"Tokens.xaml",
					"DmRadiusDefault"));
	}
}
