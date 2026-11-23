using FluentAssertions;

namespace DungeonMaster.Tests.App.Theme;

/// <summary>
///		Unit tests for the XAML scan <see cref="IconFontFixture"/> runs over the application tree.
/// </summary>
/// <remarks>
///		Both facts call <see cref="IconFontFixture.ScanIconTexts"/>, the method the fixture's
///		constructor calls once per <c>.xaml</c> file, on an inline document. They pin the case a
///		tag scan cannot see: a literal <c>&gt;</c> inside an attribute value written before the
///		icon-font attributes.
/// </remarks>
public class IconFontFixtureTests
{
	/// <summary>
	///		A <c>Label</c> starting on line 3 whose first attribute value holds a literal
	///		<c>&gt;</c>, followed by the icon font and a numeric character reference.
	/// </summary>
	private const string IconFontLabel = """
		<ContentView xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
		             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">
			<Label
				SemanticProperties.Hint="a > b"
				FontFamily="{StaticResource DmFontIcon}"
				Text="&#xE9FC;" />
		</ContentView>
		""";

	/// <summary>
	///		The same shape as <see cref="IconFontLabel"/>, declaring the body font instead.
	/// </summary>
	private const string BodyFontLabel = """
		<ContentView xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
		             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">
			<Label
				SemanticProperties.Hint="a > b"
				FontFamily="{StaticResource DmFontBody}"
				Text="&#xE9FC;" />
		</ContentView>
		""";

	/// <summary>
	///		A <c>&gt;</c> in an attribute value before the icon-font attributes does not hide
	///		them: the element is reported once, at its start-tag line, with its decoded text.
	/// </summary>
	[Fact]
	public void IconFontElementAfterAGreaterThanSignInAnAttributeValueIsReportedOnceAtItsStartLineWithDecodedText()
	{
		var declarations = IconFontFixture.ScanIconTexts(
			IconFontLabel,
			"Synthetic.xaml");

		declarations
			.Should()
			.ContainSingle()
			.Which
			.Should()
			.Be(
				new IconTextDeclaration(
					"Synthetic.xaml:3",
					"Label",
					""));
	}

	/// <summary>
	///		The control for the fact above: the same element declaring the body font yields no
	///		declaration.
	/// </summary>
	[Fact]
	public void ElementOfTheSameShapeDeclaringTheBodyFontYieldsNoDeclaration()
	{
		var declarations = IconFontFixture.ScanIconTexts(
			BodyFontLabel,
			"Synthetic.xaml");

		declarations
			.Should()
			.BeEmpty();
	}
}
