using System.Text.RegularExpressions;

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
	///		Matches the opening tag of any element together with its own attributes.
	///		<c>[^&gt;]</c> cannot cross into a nested element, so the attribute run belongs to this
	///		tag and no other.
	/// </summary>
	private static readonly Regex ElementTag =
		new(
			@"<(?<element>[A-Za-z_][\w.:-]*)\b(?<attributes>[^>]*?)/?>",
			RegexOptions.Singleline | RegexOptions.Compiled);

	private static readonly Regex IconFontAttribute =
		new(
			@"FontFamily\s*=\s*""\{\s*StaticResource\s+DmFontIcon\s*\}""",
			RegexOptions.Compiled);

	private static readonly Regex TextAttribute =
		new(
			@"(?<![A-Za-z.])Text\s*=\s*""(?<value>[^""]*)""",
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

		IconTexts = ScanIconTexts();
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

	private static IReadOnlyList<IconTextDeclaration> ScanIconTexts()
	{
		var declarations = new List<IconTextDeclaration>();

		foreach (var path in RepoPaths.AppSourceFiles("*.xaml"))
		{
			var relative = Path.GetRelativePath(
				RepoPaths.AppRoot,
				path);

			// Comments are blanked rather than deleted, so a comment containing something that
			// looks like markup cannot be matched while every reported line number still counts
			// against the file as it is on disk. ThemeSource.BlankXmlComments guarantees that
			// offset preservation; the local copy this replaced did the same thing three files
			// apart, which is MASTER-138.
			var text = ThemeSource.BlankXmlComments(File.ReadAllText(path));

			foreach (Match tag in ElementTag.Matches(text))
			{
				var attributes = tag.Groups["attributes"].Value;

				if (!IconFontAttribute.IsMatch(attributes))
				{
					continue;
				}

				var declaredText = TextAttribute.Match(attributes);

				if (!declaredText.Success)
				{
					continue;
				}

				declarations.Add(
					new IconTextDeclaration(
						$"{relative}:{ThemeSource.LineNumber(
							text,
							tag.Index)}",
						tag.Groups["element"].Value,
						ThemeSource.DecodeCharacterEntities(declaredText.Groups["value"].Value)));
			}
		}

		return declarations;
	}
}
