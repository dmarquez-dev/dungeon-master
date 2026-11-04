using System.Globalization;
using System.Text.RegularExpressions;
using FluentAssertions;

namespace DungeonMaster.Tests.App.Theme;

/// <summary>
///		Drift guard for the one place a theme token's value is unavoidably duplicated as a literal
///		in XAML markup: <c>DmBorderBase</c>'s <c>StrokeShape="RoundRectangle 3"</c> setter.
/// </summary>
/// <remarks>
///		<para>
///		THE BINDABLE-PROPERTY HALF OF THIS CLASS HAS MOVED. Two theories here —
///		<c>BindableDefault_EqualsTheTokenItMirrors</c> and
///		<c>BindableFamilyDefault_EqualsTheTokenItMirrors</c> — asserted against the LEXICAL FORM of
///		application C#: each compiled a regular expression requiring the literal sequence
///		<c>nameof(X), typeof(double), typeof(Y), &lt;number&gt;</c> to appear in a source file. Every
///		behaviour-preserving edit that rewrote that sequence — extracting the owner type to an
///		alias, hoisting the default into a <c>const double</c>, reordering the arguments onto one
///		line — reddened a change that broke nothing, and a rename of the property would have made
///		the match fail rather than report a drift. That is MASTER-109.
///		</para>
///		<para>
///		They are now
///		<c>DungeonMaster.App.UnitTests.Theme.BindablePropertyDefaultTests</c>, which reads the real
///		<c>BindableProperty</c> by reflection and compares its <c>DefaultValue</c> against the LIVE
///		merged <c>Application.Current.Resources</c>. Both halves of that comparison are runtime
///		values, so no spelling of the declaration can affect it. They cannot live in this assembly:
///		<c>DungeonMaster.Tests</c> may not reference <c>DungeonMaster.App</c> and so can never see a
///		MAUI type (contracts/app-unit-tests-project.md section 3.1).
///		</para>
///		<para>
///		What stays here is the fact below, which reads XAML rather than C# and has no MAUI type in
///		it at all.
///		</para>
/// </remarks>
public class BindablePropertyDefaultsMatchTokensTests
{
	/// <summary>
	///		Asserts that the numeric radius argument on <c>DmBorderBase</c>'s
	///		<c>StrokeShape="RoundRectangle 3"</c> setter in Controls.Base.xaml equals
	///		<c>DmRadiusDefault</c> in Tokens.xaml. <c>StrokeShape</c>'s converter parses a shape
	///		name and its arguments from one string and cannot take a markup extension for the
	///		radius, so the value is unavoidably a literal there; the comment beside it says the
	///		literal must move with the token, and this is what enforces that.
	/// </summary>
	[Fact]
	public void BorderBaseStrokeShapeRadiusEqualsDmRadiusDefault()
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
