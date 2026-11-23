using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace DungeonMaster.Tests.App.Theme;

/// <summary>
///		One literal <c>Text</c> declared on a control that also declares the icon font.
/// </summary>
/// <param name="Where">
///		The file and line, as "<c>relative/path.xaml:12</c>", counted against the file as it is
///		on disk.
/// </param>
/// <param name="Element">
///		The element name as written in the XAML.
/// </param>
/// <param name="Text">
///		The attribute value with its character entities already decoded.
/// </param>
public sealed record IconTextDeclaration(
	string Where,
	string Element,
	string Text);

/// <summary>
///		The two disk scans the facts in <see cref="IconFontCoverageTests"/> stand on, computed
///		ONCE per test class instead of once per fact.
/// </summary>
/// <remarks>
///		<para>
///		MASTER-068 / TEST-017. Both facts in that class called <c>IconFontCodepoints()</c>, which
///		re-read the 148,988-byte TTF off disk and re-parsed its <c>cmap</c> every time; the
///		coverage fact additionally re-read and re-scanned every <c>.xaml</c> file under the
///		application project. As an <c>IClassFixture</c> the font is read and parsed once and the
///		XAML tree is walked once, for one identical answer.
///		</para>
///		<para>
///		NOTHING HERE ASSERTS. The font may be missing and the scan may come back empty, and each
///		says so by a property rather than by a <c>.Should()</c> in somebody else's Arrange.
///		Non-emptiness is a named <c>[Fact]</c> in the consuming class, per
///		<c>contracts/anti-vacuity.md</c> section 1.
///		</para>
/// </remarks>
public sealed class IconFontFixture
{
	/// <summary>
	///		The file backing the <c>RpgAwesome</c> alias that <c>DmFontIcon</c> names. Read as a
	///		build asset out of the application project rather than from the build output, so the
	///		facts run against the font the source tree ships.
	/// </summary>
	private const string IconFontFile = "rpgawesome-webfont.ttf";

	/// <summary>
	///		Matches a whole <c>FontFamily</c> value naming the icon font, with optional whitespace
	///		inside the braces.
	/// </summary>
	private static readonly Regex IconFontValue =
		new(
			@"^\{\s*StaticResource\s+DmFontIcon\s*\}$",
			RegexOptions.Compiled);

	/// <summary>
	///		Initializes a new instance of <see cref="IconFontFixture"/>, running both scans.
	/// </summary>
	public IconFontFixture()
	{
		IconFontPath = Path.Combine(
			RepoPaths.AppRoot,
			"Resources",
			"Fonts",
			IconFontFile);

		IconFontExists = File.Exists(IconFontPath);

		Codepoints = IconFontExists
			? TrueTypeCmap.Codepoints(File.ReadAllBytes(IconFontPath))
			: new HashSet<int>();

		IconTexts = RepoPaths
			.AppSourceFiles("*.xaml")
			.SelectMany(path =>
				ScanIconTexts(
					File.ReadAllText(path),
					Path.GetRelativePath(
						RepoPaths.AppRoot,
						path)))
			.ToList();
	}

	/// <summary>
	///		The absolute path the font scan looked at, reported whether or not it was there.
	/// </summary>
	public string IconFontPath { get; }

	/// <summary>
	///		Whether the icon font was present at <see cref="IconFontPath"/>.
	/// </summary>
	public bool IconFontExists { get; }

	/// <summary>
	///		Every codepoint the icon font's <c>cmap</c> maps. May be empty.
	/// </summary>
	public IReadOnlySet<int> Codepoints { get; }

	/// <summary>
	///		Every literal <c>Text</c> on a control declaring <c>DmFontIcon</c>. May be empty.
	/// </summary>
	public IReadOnlyList<IconTextDeclaration> IconTexts { get; }

	/// <summary>
	///		Returns every element in one XAML document that declares <c>DmFontIcon</c> as its
	///		<c>FontFamily</c> together with a literal, unqualified <c>Text</c>, in document order.
	///		This is the scan the constructor runs once per application <c>.xaml</c> file.
	/// </summary>
	/// <remarks>
	///		The document is parsed rather than pattern-matched, so a comment is never mistaken
	///		for markup and a <c>&gt;</c> inside an attribute value cannot end an element early.
	///		Each line comes from the element's own line info, so it is the line its start tag
	///		begins on in the text as supplied; each value comes from the parser, so its
	///		character entities are already decoded.
	/// </remarks>
	/// <param name="xaml">
	///		The XAML text to scan.
	/// </param>
	/// <param name="label">
	///		The name each declaration's <see cref="IconTextDeclaration.Where"/> reports the text
	///		under, ahead of the line number.
	/// </param>
	/// <exception cref="XmlException">
	///		The text is not well-formed XML; the message is prefixed with <paramref name="label"/>.
	/// </exception>
	internal static IReadOnlyList<IconTextDeclaration> ScanIconTexts(
		string xaml,
		string label)
	{
		XDocument document;

		try
		{
			document = XDocument.Parse(
				xaml,
				LoadOptions.SetLineInfo);
		}
		catch (XmlException exception)
		{
			// The parser's own message names a line and a column but not a file.
			throw new XmlException(
				$"{label}: {exception.Message}",
				exception);
		}

		return document
			.Descendants()
			.Where(element =>
				element.Attribute("FontFamily") is { } fontFamily
				&& IconFontValue.IsMatch(fontFamily.Value)
				&& element.Attribute("Text") is not null)
			.Select(element =>
				new IconTextDeclaration(
					$"{label}:{((IXmlLineInfo)element).LineNumber}",
					ElementNameAsWritten(element),
					element.Attribute("Text")!.Value))
			.ToList();
	}

	/// <summary>
	///		Returns the element name with the prefix its document declares for its namespace,
	///		so <c>controls:DmIconButton</c> is reported as written rather than as a bare local
	///		name.
	/// </summary>
	private static string ElementNameAsWritten(XElement element)
	{
		var prefix = element.GetPrefixOfNamespace(element.Name.Namespace);

		return string.IsNullOrEmpty(prefix)
			? element.Name.LocalName
			: $"{prefix}:{element.Name.LocalName}";
	}
}
