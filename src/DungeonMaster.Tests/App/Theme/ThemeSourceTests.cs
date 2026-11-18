using System.Text;
using System.Xml.Linq;
using FluentAssertions;

namespace DungeonMaster.Tests.App.Theme;

/// <summary>
///		Unit tests for the shared scanning primitives themselves.
///		<para>
///		Every other consumer of <c>ThemeSource</c> states a fact about the application; these
///		state facts about the scanner. They matter because a scanner that quietly stops
///		reaching, stops matching, or truncates what it decodes turns every fact built on it
///		green while examining nothing, and no fact about the application can tell the
///		difference.
///		</para>
/// </summary>
public class ThemeSourceTests
{
	/// <summary>
	///		A document declaring one key in the XAML language namespace and one in the default
	///		MAUI namespace, which are the two forms <c>QualifiedElementName</c> has to tell
	///		apart.
	/// </summary>
	private const string SyntheticDocument = """
		<ResourceDictionary xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
		                    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">
			<x:Double x:Key="DmSynthetic">4</x:Double>
			<Color x:Key="DmSyntheticColour">#FF0000</Color>
		</ResourceDictionary>
		""";

	/// <summary>
	///		The XAML language namespace is spelled with its prefix and the default MAUI
	///		namespace is spelled bare, because that is the vocabulary the consuming tables use.
	/// </summary>
	[Fact]
	public void QualifiedElementNamePrefixesTheXamlNamespaceAndLeavesTheDefaultNamespaceBare()
	{
		var document = XDocument.Parse(SyntheticDocument);

		var names = document
			.Root!
			.Elements()
			.Select(element =>
				ThemeSource.QualifiedElementName(element.Name))
			.ToList();

		names
			.Should()
			.Equal(
				"x:Double",
				"Color");
	}

	/// <summary>
	///		Blanking an XML comment changes no offset, so a line number taken from the blanked
	///		text is still a line number in the original file.
	/// </summary>
	[Fact]
	public void BlankXmlCommentsReplacesTheCommentWithSpacesAndMovesNothingElse()
	{
		const string original = "<A />\r\n<!-- <B x:Key=\"DmFake\" />\r\n     still comment -->\r\n<C />\r\n";

		var blanked = ThemeSource.BlankXmlComments(original);

		var inside = original.IndexOf(
			"DmFake",
			StringComparison.Ordinal);

		var after = original.IndexOf(
			"<C />",
			StringComparison.Ordinal);

		blanked
			.Length
			.Should()
			.Be(
				original.Length,
				"a shortening implementation silently breaks every line number reported against the blanked text");

		blanked
			.Should()
			.NotContain("DmFake");

		blanked[inside]
			.Should()
			.Be(' ');

		blanked
			.Substring(
				after,
				5)
			.Should()
			.Be("<C />");

		ThemeSource
			.LineNumber(
				blanked,
				inside)
			.Should()
			.Be(
				ThemeSource.LineNumber(
					original,
					inside));

		ThemeSource
			.LineNumber(
				blanked,
				after)
			.Should()
			.Be(
				ThemeSource.LineNumber(
					original,
					after));
	}

	/// <summary>
	///		Blanking a C# comment changes no offset either, and the anchored pattern leaves the
	///		slashes inside a URL alone — an unanchored pattern erases the rest of that line of
	///		real code.
	/// </summary>
	[Fact]
	public void BlankCSharpCommentsBlanksWholeLineCommentsAndLeavesAUrlIntact()
	{
		const string original = "\tvar source = \"ms-appx:///Assets/DmMarker.png\";\r\n\t// DmCommentKey\r\n\tvar keep = DmRealKey;\r\n";

		var blanked = ThemeSource.BlankCSharpComments(original);

		var inside = original.IndexOf(
			"DmCommentKey",
			StringComparison.Ordinal);

		var after = original.IndexOf(
			"DmRealKey",
			StringComparison.Ordinal);

		blanked
			.Length
			.Should()
			.Be(original.Length);

		blanked
			.Should()
			.NotContain("DmCommentKey");

		blanked
			.Should()
			.Contain(
				"ms-appx:///Assets/DmMarker.png",
				"an unanchored //.*$ strips from the slashes inside a URL and erases the rest of a real line of code");

		blanked
			.Should()
			.Contain("DmRealKey");

		ThemeSource
			.LineNumber(
				blanked,
				inside)
			.Should()
			.Be(
				ThemeSource.LineNumber(
					original,
					inside));

		ThemeSource
			.LineNumber(
				blanked,
				after)
			.Should()
			.Be(
				ThemeSource.LineNumber(
					original,
					after));
	}

	/// <summary>
	///		A supplementary-plane character reference decodes to its surrogate pair — two
	///		<c>char</c>s, one Rune — rather than being truncated into an unrelated BMP character
	///		by a <c>(char)</c> cast.
	/// </summary>
	[Fact]
	public void DecodeCharacterEntitiesExpandsASupplementaryPlaneHexEntityToOneRune()
	{
		var decoded = ThemeSource.DecodeCharacterEntities("&#x1F6E1;");

		decoded
			.Length
			.Should()
			.Be(
				2,
				"a code point above U+FFFF is a surrogate pair, and a (char) cast keeps only its low half");

		Rune
			.GetRuneAt(
				decoded,
				0)
			.Value
			.Should()
			.Be(0x1F6E1);

		ThemeSource
			.IsSingleRune(decoded)
			.Should()
			.BeTrue("a text.Length != 1 test rejects a surrogate pair outright");
	}

	/// <summary>
	///		The decimal form of a character reference decodes too; a hex-only pattern reads a
	///		decimal reference as no reference at all and leaves the markup in the text.
	/// </summary>
	[Fact]
	public void DecodeCharacterEntitiesExpandsTheDecimalFormAndTheAmpersandEntity()
	{
		ThemeSource
			.DecodeCharacterEntities("&#8592;&amp;&#x2190;")
			.Should()
			.Be("←&←");
	}

	/// <summary>
	///		A reference naming a code point outside the Unicode range names itself in the
	///		failure rather than being silently dropped.
	/// </summary>
	[Fact]
	public void DecodeCharacterEntitiesThrowsNamingTheEntityWhenItIsOutOfRange()
	{
		var decode = () =>
			ThemeSource.DecodeCharacterEntities("&#x110000;");

		decode
			.Should()
			.Throw<ArgumentOutOfRangeException>()
			.WithMessage("*&#x110000;*");
	}

	/// <summary>
	///		A single BMP character is one Rune, a surrogate pair is also one Rune, an empty string
	///		is none, and two BMP characters are two.
	/// </summary>
	[Fact]
	public void IsSingleRuneCountsRunesRatherThanChars()
	{
		ThemeSource
			.IsSingleRune("a")
			.Should()
			.BeTrue();

		ThemeSource
			.IsSingleRune("🛡")
			.Should()
			.BeTrue("a surrogate pair is two chars and one Rune, which is exactly what a Length == 1 test gets wrong");

		ThemeSource
			.IsSingleRune(string.Empty)
			.Should()
			.BeFalse();

		ThemeSource
			.IsSingleRune("ab")
			.Should()
			.BeFalse();
	}

	/// <summary>
	///		A theme dictionary is read and parsed once and is the same instance in both scans,
	///		so a consumer may call either in a loop without re-reading disk.
	/// </summary>
	[Fact]
	public void ThemeDocumentsAndAppDocumentsShareOneParsedInstanceOfTokensXaml()
	{
		var themeDocument = ThemeSource
			.ThemeDocuments()
			.First(document =>
				document.RelativePath.EndsWith(
					"Tokens.xaml",
					StringComparison.Ordinal));

		var appDocument = ThemeSource
			.AppDocuments()
			.Single(document =>
				document.Path == themeDocument.Path);

		appDocument
			.Should()
			.BeSameAs(
				themeDocument,
				"a file under Resources/Theme is parsed once and appears in both scans as the same instance");

		ThemeSource
			.ThemeDocuments()
			.First(document =>
				document.Path == themeDocument.Path)
			.Should()
			.BeSameAs(themeDocument);
	}

	/// <summary>
	///		Every keyed element carries the file it was declared in and a real line number, so a
	///		consumer can cite the declaration rather than the scan.
	/// </summary>
	[Fact]
	public void KeyedElementsCarryTheQualifiedElementFileAndLineNumberOfDmRadiusDefault()
	{
		var radius = ThemeSource
			.KeyedElements()
			.Single(element =>
				element.Key == "DmRadiusDefault");

		radius
			.QualifiedElementName
			.Should()
			.Be("x:Int32");

		radius
			.RelativePath
			.Should()
			.Be("Resources/Theme/Tokens.xaml");

		radius
			.LineNumber
			.Should()
			.BeGreaterThan(0);

		radius
			.Element
			.Value
			.Should()
			.Be("3");
	}

	/// <summary>
	///		The token readers return the declared value when the token is there.
	/// </summary>
	[Fact]
	public void TheTokenReadersReturnTheDeclaredValueWhenTheTokenIsPresent()
	{
		ThemeSource
			.DoubleToken(
				"Typography.xaml",
				"DmFontSizeBanner")
			.Should()
			.Be(24d);

		ThemeSource
			.Int32Token(
				"Tokens.xaml",
				"DmRadiusDefault")
			.Should()
			.Be(3);

		ThemeSource
			.StringToken(
				"Typography.xaml",
				"DmFontBody")
			.Should()
			.Be("EBGaramondRegular");
	}

	/// <summary>
	///		A missing <c>x:Double</c> throws a typed exception naming the file, the key and the
	///		element type expected, rather than asserting from inside a helper against whichever
	///		test called it first.
	/// </summary>
	[Fact]
	public void DoubleTokenThrowsNamingTheFileKeyAndElementWhenTheTokenIsAbsent()
	{
		var read = () =>
			ThemeSource.DoubleToken(
				"Typography.xaml",
				"DmTokenThatIsNotDeclaredAnywhere");

		read
			.Should()
			.Throw<ThemeTokenNotFoundException>()
			.WithMessage("*DmTokenThatIsNotDeclaredAnywhere*")
			.WithMessage("*Typography.xaml*")
			.WithMessage("*x:Double*");
	}

	/// <summary>
	///		A missing <c>x:Int32</c> throws the same way. All three readers are converted
	///		together: converting some and not others leaves a caller unable to say which shape
	///		of failure it is handling.
	/// </summary>
	[Fact]
	public void Int32TokenThrowsNamingTheFileKeyAndElementWhenTheTokenIsAbsent()
	{
		var read = () =>
			ThemeSource.Int32Token(
				"Tokens.xaml",
				"DmTokenThatIsNotDeclaredAnywhere");

		read
			.Should()
			.Throw<ThemeTokenNotFoundException>()
			.WithMessage("*DmTokenThatIsNotDeclaredAnywhere*")
			.WithMessage("*Tokens.xaml*")
			.WithMessage("*x:Int32*");
	}

	/// <summary>
	///		A missing <c>x:String</c> throws the same way.
	/// </summary>
	[Fact]
	public void StringTokenThrowsNamingTheFileKeyAndElementWhenTheTokenIsAbsent()
	{
		var read = () =>
			ThemeSource.StringToken(
				"Typography.xaml",
				"DmTokenThatIsNotDeclaredAnywhere");

		read
			.Should()
			.Throw<ThemeTokenNotFoundException>()
			.WithMessage("*DmTokenThatIsNotDeclaredAnywhere*")
			.WithMessage("*Typography.xaml*")
			.WithMessage("*x:String*");
	}

	/// <summary>
	///		A key declared as an <c>x:Double</c> is not read as an <c>x:Int32</c>. Both elements
	///		hold text a lenient reader would happily parse, so a reader that did not check the
	///		declaring element would pass straight through the retype that broke the radius
	///		tokens.
	/// </summary>
	[Fact]
	public void Int32TokenThrowsRatherThanParsingAKeyDeclaredAsADouble()
	{
		var read = () =>
			ThemeSource.Int32Token(
				"Typography.xaml",
				"DmFontSizeBanner");

		read
			.Should()
			.Throw<ThemeTokenNotFoundException>()
			.WithMessage("*x:Int32*");
	}
}
