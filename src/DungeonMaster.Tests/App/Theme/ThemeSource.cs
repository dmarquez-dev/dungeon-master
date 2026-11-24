using System.Buffers;
using System.Collections.Concurrent;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace DungeonMaster.Tests.App.Theme;

/// <summary>
///		A parsed XAML dictionary, with line info retained.
/// </summary>
/// <param name="Path">
///		The file's absolute path, exactly as <see cref="ThemeSource.ThemeFiles"/> returns it.
/// </param>
/// <param name="RelativePath">
///		The path relative to the application project root, using forward slashes so it can be
///		compared directly against an <c>App.xaml</c> <c>Source=</c> attribute.
/// </param>
/// <param name="Document">
///		The parsed document, loaded with <see cref="LoadOptions.SetLineInfo"/>.
/// </param>
public sealed record ThemeDocument(
	string Path,
	string RelativePath,
	XDocument Document);

/// <summary>
///		An <c>x:Key</c>-bearing element in a theme dictionary.
/// </summary>
/// <param name="Key">
///		The <c>x:Key</c> value verbatim, prefix casing preserved.
/// </param>
/// <param name="Element">
///		The declaring element.
/// </param>
/// <param name="QualifiedElementName">
///		The element name with its XAML prefix restored — "x:Double", "Color", "Style", and so on.
/// </param>
/// <param name="RelativePath">
///		The declaring file's path relative to the application project root, forward slashes.
/// </param>
/// <param name="LineNumber">
///		The 1-based line the element starts on, or 0 when line info is unavailable.
/// </param>
public sealed record KeyedElement(
	string Key,
	XElement Element,
	string QualifiedElementName,
	string RelativePath,
	int LineNumber);

/// <summary>
///		Shared scanning primitives for the theme facts.
///		The patterns and file scans here were duplicated across five test classes; a divergence
///		between two copies is invisible until one of them silently stops matching, which is the
///		failure mode this whole suite exists to prevent.
///		<para>
///		Nothing in this class asserts. A helper that asserts reports a broken scan against
///		whichever test happened to call it first, which names the wrong test; every scan here has
///		a named guard fact of its own in <c>ThemeSourceScanGuardTests</c> instead. The three token
///		readers cannot express "found nothing" in their return type, so they throw the typed
///		<see cref="ThemeTokenNotFoundException"/>, which names its own cause.
///		</para>
/// </summary>
internal static class ThemeSource
{
	/// <summary>
	///		The MAUI default XML namespace, the one every MAUI element in the application's XAML
	///		is declared in.
	/// </summary>
	public const string MauiNamespace = "http://schemas.microsoft.com/dotnet/2021/maui";

	/// <summary>
	///		The XAML language namespace, whose elements carry the <c>x:</c> prefix.
	/// </summary>
	private const string XamlNamespace = "http://schemas.microsoft.com/winfx/2009/xaml";

	/// <summary>
	///		The opening of a StaticResource markup extension.
	/// </summary>
	private const string StaticResourceOpening = "{StaticResource";

	/// <summary>
	///		Matches a 6- or 8-digit hex colour literal.
	/// </summary>
	public static readonly Regex HexLiteralPattern =
		new(
			@"#[0-9A-Fa-f]{6}([0-9A-Fa-f]{2})?",
			RegexOptions.Compiled);

	/// <summary>
	///		Matches an XML comment, including one spanning lines.
	///		The theme dictionaries carry long prose comments that name properties and elements;
	///		a scan that does not strip them reads the prose as markup.
	/// </summary>
	public static readonly Regex XmlCommentPattern =
		new(
			@"<!--.*?-->",
			RegexOptions.Compiled | RegexOptions.Singleline);

	/// <summary>
	///		Matches a C# single-line comment, ANCHORED to the start of its line.
	///		<para>
	///		The anchor is the whole reason this member exists in one place. Two copies of this
	///		pattern had drifted apart — <c>ThemeDictionaryTests.CSharpLineCommentPattern</c> was
	///		unanchored (<c>//.*$</c>) and <c>ThemeTokenParityTests.CommentLine</c> was anchored —
	///		and <c>GeometryScaleTests.WholeLineCommentPattern</c> records why the anchored form is
	///		the correct one: an unanchored <c>//</c> strips from the "//" inside
	///		"ms-appx:///…" and every other URL, erasing the rest of a real line of code. The
	///		shared helper therefore adopts the anchored form, and a consumer that believes the
	///		anchored form is wrong for its case escalates rather than re-declaring a local
	///		pattern.
	///		</para>
	/// </summary>
	private static readonly Regex CSharpLineCommentPattern =
		new(
			@"^[ \t]*(?<comment>//[^\r\n]*)",
			RegexOptions.Compiled | RegexOptions.Multiline);

	/// <summary>
	///		Matches an XML numeric character reference in either the decimal (<c>&amp;#8592;</c>)
	///		or the hexadecimal (<c>&amp;#x2190;</c>) form.
	/// </summary>
	private static readonly Regex CharacterEntityPattern =
		new(
			@"&#(?<hex>x?)(?<code>[0-9A-Fa-f]+);",
			RegexOptions.Compiled);

	/// <summary>
	///		Matches a Dm- or DM-prefixed string literal in C# SOURCE TEXT. This is the one
	///		regular expression in this reference scan and it is never matched against XAML:
	///		<c>XDocument</c> cannot parse C#, which is the boundary
	///		<c>contracts/theme-source.md</c> section 3.5 draws. Code-behind resolves some theme
	///		colours through <c>Application.Current.Resources[…]</c>, where the key is a plain
	///		string; XAML-only scanning cannot see those, and a stale key there is invisible until
	///		it throws at render time — <c>NotebookView.RibbonBrush</c> carried "DMRibbonRead"
	///		inside a ternary through the whole of Task 5's migration for exactly that reason.
	/// </summary>
	private static readonly Regex CSharpKeyLiteralPattern =
		new(
			@"""(D[Mm][A-Za-z0-9]{2,})""",
			RegexOptions.Compiled);

	/// <summary>
	///		The <c>x:Key</c> attribute name.
	/// </summary>
	private static readonly XName KeyAttributeName =
		XName.Get(
			"Key",
			XamlNamespace);

	/// <summary>
	///		Every document parsed so far in this process, keyed by absolute path.
	///		<c>ThemeDocuments</c> and <c>AppDocuments</c> share it, so a file under
	///		Resources/Theme is read and parsed once and is the SAME instance in both.
	/// </summary>
	private static readonly ConcurrentDictionary<string, ThemeDocument> DocumentCache =
		new(StringComparer.Ordinal);

	/// <summary>
	///		Gets the absolute path of the Resources/Theme directory.
	/// </summary>
	private static string ThemeDirectory =>
		Path.Combine(
			RepoPaths.AppRoot,
			"Resources",
			"Theme");

	/// <summary>
	///		Returns every dictionary under Resources/Theme, at any depth.
	///		<c>AllDirectories</c> is deliberate: a file in a subdirectory used to be exempt from
	///		every hygiene pattern AND invisible to every definition scan AND invisible to the
	///		merge guard, which is the widest blind spot this suite ever had.
	///		<para>
	///		Asserts nothing, and may return an empty list. The guard that used to live here is
	///		<c>ThemeSourceScanGuardTests.TheThemeFileScanReachesAtLeastOneDictionary</c>.
	///		</para>
	/// </summary>
	public static IReadOnlyList<string> ThemeFiles()
	{
		return Directory
			.EnumerateFiles(
				ThemeDirectory,
				"*.xaml",
				SearchOption.AllDirectories)
			.OrderBy(
				path =>
					path,
				StringComparer.Ordinal)
			.ToList();
	}

	/// <summary>
	///		Returns the path of each theme dictionary relative to the application project root,
	///		using forward slashes so it can be compared directly against an
	///		<c>App.xaml</c> <c>Source=</c> attribute.
	/// </summary>
	public static IReadOnlyList<string> ThemeFileSources()
	{
		return ThemeFiles()
			.Select(path =>
				Path
					.GetRelativePath(
						RepoPaths.AppRoot,
						path)
					.Replace(
						Path.DirectorySeparatorChar,
						'/'))
			.ToList();
	}

	/// <summary>
	///		Returns every dictionary under Resources/Theme, parsed, at most once per process.
	///		Ordinal-ascending by path. Empty when the directory holds no <c>.xaml</c>.
	///		Asserts nothing.
	/// </summary>
	public static IReadOnlyList<ThemeDocument> ThemeDocuments()
	{
		return ThemeFiles()
			.Select(Document)
			.ToList();
	}

	/// <summary>
	///		Returns every <c>.xaml</c> file under the application project root, parsed, at most
	///		once per process. This is the WHOLE application tree, not only Resources/Theme;
	///		<see cref="ThemeDocuments"/> is its Resources/Theme subset and shares its cache, so a
	///		theme dictionary is the same instance in both. Ordinal-ascending by path. Empty when
	///		nothing matches. Asserts nothing.
	/// </summary>
	public static IReadOnlyList<ThemeDocument> AppDocuments()
	{
		return RepoPaths
			.AppSourceFiles("*.xaml")
			.Select(Document)
			.ToList();
	}

	/// <summary>
	///		Returns every <c>x:Key</c>-bearing element in the theme dictionaries, in ordinal path
	///		order and, within a document, in document order.
	///		<para>
	///		A duplicate key is a fact for a consumer to report, so this returns EVERY declaration
	///		rather than a dictionary in which a later one silently wins.
	///		</para>
	/// </summary>
	public static IReadOnlyList<KeyedElement> KeyedElements()
	{
		return ThemeDocuments()
			.SelectMany(document =>
				document
					.Document
					.Descendants()
					.Where(element =>
						element.Attribute(KeyAttributeName) is not null)
					.Select(element =>
						new KeyedElement(
							element.Attribute(KeyAttributeName)!.Value,
							element,
							QualifiedElementName(element.Name),
							document.RelativePath,
							ElementLineNumber(element))))
			.ToList();
	}

	/// <summary>
	///		Returns the element name with its XAML prefix restored, so <c>x:Int32</c> is
	///		distinguishable from a hypothetical <c>Int32</c> in the default MAUI namespace.
	///		This is the spelling <c>DmThemeKeysResolveTests.ExpectedDeclaringElement</c> already
	///		uses as its values, so a consumer compares against its existing table without a
	///		translation layer.
	/// </summary>
	/// <param name="name">
	///		The element name to qualify.
	/// </param>
	public static string QualifiedElementName(XName name)
	{
		return name.NamespaceName == XamlNamespace
			? $"x:{name.LocalName}"
			: name.LocalName;
	}

	/// <summary>
	///		Returns true exactly when the supplied element is the named element in the MAUI
	///		default namespace, <see cref="MauiNamespace"/>, whatever prefix (or none) the document
	///		used to bind it. The same local name in any other namespace — a <c>clr-namespace:</c>
	///		namespace, the XAML language namespace, or none — is not a match. The comparison is
	///		ordinal and case-sensitive on both namespace and local name.
	///		<para>
	///		Unlike <see cref="QualifiedElementName(XName)"/>, which strips every namespace but the
	///		XAML language one, this tells a MAUI element from a same-named element declared in
	///		any other namespace.
	///		</para>
	/// </summary>
	/// <param name="element">
	///		The element to test.
	/// </param>
	/// <param name="localName">
	///		The bare local name to match, such as <c>Style</c>. It must be a valid XML name
	///		without a prefix.
	/// </param>
	/// <exception cref="XmlException">
	///		<paramref name="localName"/> carries a prefix, as the qualified spelling
	///		<c>x:Style</c> does, so a caller still passing a <see cref="QualifiedElementName(XName)"/>
	///		spelling fails loudly instead of silently matching nothing.
	/// </exception>
	/// <exception cref="ArgumentException">
	///		<paramref name="localName"/> is the empty string.
	/// </exception>
	public static bool IsMauiElement(
		XElement element,
		string localName)
	{
		return element.Name == XName.Get(
			localName,
			MauiNamespace);
	}

	/// <summary>
	///		Returns the value of an <c>x:Double</c> token declared in the named dictionary.
	/// </summary>
	/// <param name="fileName">
	///		The declaring dictionary's file name, for example "Typography.xaml".
	/// </param>
	/// <param name="key">
	///		The token's resource key.
	/// </param>
	/// <exception cref="ThemeTokenNotFoundException">
	///		The key is not declared as an <c>x:Double</c> in that dictionary.
	/// </exception>
	public static double DoubleToken(
		string fileName,
		string key)
	{
		return double.Parse(
			TokenText(
				fileName,
				key,
				"x:Double"),
			CultureInfo.InvariantCulture);
	}

	/// <summary>
	///		Returns the value of an <c>x:Int32</c> token declared in the named dictionary.
	///		<para>
	///		Separate from <see cref="DoubleToken"/> on purpose. Both would happily parse "3",
	///		so a single lenient reader that accepted either element would keep passing through
	///		exactly the retype that broke the radius tokens — and the declaring element is the
	///		thing that decides whether the token binds at all. A key declared as an
	///		<c>x:Double</c> therefore throws here rather than parsing.
	///		</para>
	/// </summary>
	/// <param name="fileName">
	///		The declaring dictionary's file name, for example "Tokens.xaml".
	/// </param>
	/// <param name="key">
	///		The token's resource key.
	/// </param>
	/// <exception cref="ThemeTokenNotFoundException">
	///		The key is not declared as an <c>x:Int32</c> in that dictionary.
	/// </exception>
	public static int Int32Token(
		string fileName,
		string key)
	{
		return int.Parse(
			TokenText(
				fileName,
				key,
				"x:Int32"),
			CultureInfo.InvariantCulture);
	}

	/// <summary>
	///		Returns the value of an <c>x:String</c> token declared in the named dictionary.
	/// </summary>
	/// <param name="fileName">
	///		The declaring dictionary's file name, for example "Typography.xaml".
	/// </param>
	/// <param name="key">
	///		The token's resource key.
	/// </param>
	/// <exception cref="ThemeTokenNotFoundException">
	///		The key is not declared as an <c>x:String</c> in that dictionary.
	/// </exception>
	public static string StringToken(
		string fileName,
		string key)
	{
		return TokenText(
			fileName,
			key,
			"x:String");
	}

	/// <summary>
	///		Replaces every character of every XML comment with a space, PRESERVING newlines and
	///		therefore every character offset in the rest of the text.
	///		<para>
	///		It does not delete. A caller may take an index into the blanked string and pass it
	///		to <see cref="LineNumber"/> to get the line number in the original file;
	///		<c>LineNumber</c> is meaningless against a shortened string, and deleting comments
	///		shifted every reported offender's line.
	///		</para>
	/// </summary>
	/// <param name="xaml">
	///		The XAML text to blank.
	/// </param>
	public static string BlankXmlComments(string xaml)
	{
		return XmlCommentPattern.Replace(
			xaml,
			match =>
				Blank(match.Value));
	}

	/// <summary>
	///		Replaces every character of every C# single-line comment with a space, PRESERVING
	///		newlines and therefore every character offset in the rest of the text. The leading
	///		indentation before the <c>//</c> is left as it is.
	///		<para>
	///		See <see cref="CSharpLineCommentPattern"/> for why the anchored form is the one this
	///		helper adopts.
	///		</para>
	/// </summary>
	/// <param name="csharp">
	///		The C# text to blank.
	/// </param>
	public static string BlankCSharpComments(string csharp)
	{
		return CSharpLineCommentPattern.Replace(
			csharp,
			match =>
			{
				var comment = match.Groups["comment"];

				return string.Concat(
					match.Value[..(comment.Index - match.Index)],
					Blank(comment.Value));
			});
	}

	/// <summary>
	///		Expands <c>&amp;#NNN;</c> and <c>&amp;#xNNNN;</c> character references and
	///		<c>&amp;amp;</c>.
	///		<para>
	///		It uses <c>char.ConvertFromUtf32</c> rather than a <c>(char)</c> cast because a code
	///		point above U+FFFF needs a surrogate pair and cannot be held in one <c>char</c>: the
	///		cast turns <c>&amp;#x1F6E1;</c> into an unrelated BMP character. That was a live
	///		defect in two places three files apart, which is why this lives here.
	///		</para>
	/// </summary>
	/// <param name="attributeValue">
	///		The attribute value or element text to decode.
	/// </param>
	/// <exception cref="ArgumentOutOfRangeException">
	///		A reference names a code point outside 0x0–0x10FFFF or inside the surrogate range.
	///		The offending entity text is appended to the message; the reference is never
	///		silently dropped.
	/// </exception>
	public static string DecodeCharacterEntities(string attributeValue)
	{
		return CharacterEntityPattern
			.Replace(
				attributeValue,
				match =>
				{
					var isHex = match.Groups["hex"].Value.Length != 0;

					if (!int.TryParse(
							match.Groups["code"].Value,
							isHex
								? NumberStyles.HexNumber
								: NumberStyles.Integer,
							CultureInfo.InvariantCulture,
							out var codePoint))
					{
						throw new ArgumentOutOfRangeException(
							nameof(attributeValue),
							match.Value,
							$"Character entity '{match.Value}' does not name a code point.");
					}

					try
					{
						return char.ConvertFromUtf32(codePoint);
					}
					catch (ArgumentOutOfRangeException exception)
					{
						throw new ArgumentOutOfRangeException(
							nameof(attributeValue),
							match.Value,
							$"{exception.Message} Offending entity: '{match.Value}'.");
					}
				})
			.Replace(
				"&amp;",
				"&",
				StringComparison.Ordinal);
	}

	/// <summary>
	///		Returns true when the text is exactly one Unicode scalar value (Rune), supplementary
	///		plane included. This is the predicate that <c>string.Length == 1</c> gets wrong: a
	///		supplementary-plane character is two <c>char</c>s and one Rune.
	/// </summary>
	/// <param name="text">
	///		The text to measure.
	/// </param>
	public static bool IsSingleRune(string text)
	{
		return !string.IsNullOrEmpty(text)
			&& Rune.DecodeFromUtf16(
				text,
				out _,
				out var consumed) == OperationStatus.Done
			&& consumed == text.Length;
	}

	/// <summary>
	///		Returns the 1-based line number of an offset within the supplied text.
	/// </summary>
	/// <param name="text">
	///		The text the offset indexes into.
	/// </param>
	/// <param name="index">
	///		The character offset.
	/// </param>
	public static int LineNumber(
		string text,
		int index)
	{
		return text
			.Take(index)
			.Count(character =>
				character == '\n')
			+ 1;
	}

	/// <summary>
	///		Returns every <c>{StaticResource …}</c> key referenced anywhere in the application's
	///		XAML, paired with the referencing file's path relative to the application root.
	///		<para>
	///		Attribute values and element text only. Comments are XML comment NODES in the parsed
	///		document and are therefore never read, which is the point the hand census this replaces
	///		got wrong: it recorded <c>DmBorderTavernStyle</c> as live on the strength of a mention
	///		inside a prose comment. Asserts nothing and may return an empty list.
	///		</para>
	/// </summary>
	public static IReadOnlyList<KeyValuePair<string, string>> References()
	{
		return ThemeSource
			.AppDocuments()
			.SelectMany(document =>
				ReferenceTexts(document.Document)
					.SelectMany(StaticResourceKeys)
					.Select(key =>
						new KeyValuePair<string, string>(
							key,
							document.RelativePath)))
			.ToList();
	}

	/// <summary>
	///		Returns every Dm- or DM-prefixed string literal in the supplied C# source text.
	/// </summary>
	/// <param name="csharpSource">
	///		The C# source to read. Blank its comments first; see
	///		<see cref="ThemeSource.BlankCSharpComments"/>.
	/// </param>
	public static IEnumerable<string> CodeKeyLiterals(string csharpSource)
	{
		return CSharpKeyLiteralPattern
			.Matches(csharpSource)
			.Select(match =>
				match.Groups[1].Value);
	}

	/// <summary>
	///		Returns every key named by a <c>{StaticResource …}</c> markup extension inside one
	///		attribute value or text node.
	///		<para>
	///		No regular expression: this reads a value the XML parser has already extracted, and a
	///		markup extension can nest inside another one — <c>Converter={StaticResource X}</c>
	///		inside a <c>{Binding …}</c> — so the key is terminated by either <c>}</c> or <c>,</c>.
	///		The pattern this replaces required a closing brace immediately after the key and so
	///		could not see the nested form at all.
	///		</para>
	/// </summary>
	/// <param name="value">
	///		The attribute value or element text to read.
	/// </param>
	private static IEnumerable<string> StaticResourceKeys(string value)
	{
		var index = value.IndexOf(
			StaticResourceOpening,
			StringComparison.Ordinal);

		while (index >= 0)
		{
			var start = index + StaticResourceOpening.Length;

			// A space is required after the extension name, so "{StaticResourceX}" is not read as
			// a reference to X.
			if (start < value.Length && char.IsWhiteSpace(value[start]))
			{
				var end = start;

				while (end < value.Length
					&& value[end] != '}'
					&& value[end] != ',')
				{
					end++;
				}

				var key = value[start..end].Trim();

				if (key.Length != 0)
				{
					yield return key;
				}
			}

			index = value.IndexOf(
				StaticResourceOpening,
				start,
				StringComparison.Ordinal);
		}
	}

	/// <summary>
	///		Returns true when the supplied text is a current, <c>Dm</c>-prefixed resource key.
	/// </summary>
	/// <param name="key">
	///		The candidate key.
	/// </param>
	public static bool IsCurrentKey(string key)
	{
		return key.StartsWith(
				"Dm",
				StringComparison.Ordinal)
			&& IsKeyBody(key);
	}

	/// <summary>
	///		Returns true when the supplied text is a resource key under EITHER casing of the prefix.
	///		<para>
	///		The <c>DM</c> tolerance is deliberate and load-bearing.
	///		Task 10 deleted the legacy <c>Resources/Styles/Colors.xaml</c>, whose keys were <c>DM</c>-prefixed, while the current tokens are <c>Dm</c>-prefixed, and the parity facts must be able to NAME a surviving <c>DM</c> reference in order to report it as dangling.
	///		A <c>Dm</c>-only predicate would not match the reference, would not add it to the missing list, and would pass.
	///		</para>
	/// </summary>
	/// <param name="key">
	///		The candidate key.
	/// </param>
	public static bool IsLegacyTolerantKey(string key)
	{
		return (key.StartsWith("Dm", StringComparison.Ordinal)
				|| key.StartsWith("DM", StringComparison.Ordinal))
			&& IsKeyBody(key);
	}

	/// <summary>
	///		Returns the text of the token with the supplied key, declared by an element of the
	///		supplied qualified name, in the named dictionary.
	/// </summary>
	private static string TokenText(
		string fileName,
		string key,
		string qualifiedElementName)
	{
		var element = Document(
				Path.Combine(
					ThemeDirectory,
					fileName))
			.Document
			.Descendants()
			.FirstOrDefault(candidate =>
				QualifiedElementName(candidate.Name) == qualifiedElementName
				&& candidate.Attribute(KeyAttributeName)?.Value == key);

		if (element is null)
		{
			throw new ThemeTokenNotFoundException(
				$"Token '{key}' is not declared as a {qualifiedElementName} in {fileName}.");
		}

		return element.Value;
	}

	/// <summary>
	///		Returns the parsed document for the supplied absolute path, reading and parsing it
	///		only if this process has not already done so.
	/// </summary>
	private static ThemeDocument Document(string path)
	{
		return DocumentCache.GetOrAdd(
			path,
			ParseDocument);
	}

	/// <summary>
	///		Reads and parses one file, retaining line info.
	/// </summary>
	private static ThemeDocument ParseDocument(string path)
	{
		var relative = Path
			.GetRelativePath(
				RepoPaths.AppRoot,
				path)
			.Replace(
				Path.DirectorySeparatorChar,
				'/');

		try
		{
			return new ThemeDocument(
				path,
				relative,
				XDocument.Parse(
					File.ReadAllText(path),
					LoadOptions.SetLineInfo));
		}
		catch (XmlException exception)
		{
			// The parser's own message names a line and a column but not a file, and this scan
			// reaches dozens of them.
			throw new XmlException(
				$"{relative}: {exception.Message}",
				exception);
		}
	}

	/// <summary>
	///		Returns the 1-based line an element starts on, or 0 when line info is unavailable.
	/// </summary>
	private static int ElementLineNumber(XElement element)
	{
		var lineInfo = (IXmlLineInfo)element;

		return lineInfo.HasLineInfo()
			? lineInfo.LineNumber
			: 0;
	}

	/// <summary>
	///		Returns true when everything after the two-character prefix is alphanumeric, which is
	///		what both key patterns on <see cref="ThemeSource"/> require.
	/// </summary>
	/// <param name="key">
	///		The candidate key.
	/// </param>
	private static bool IsKeyBody(string key)
	{
		return key
			.Skip(2)
			.All(char.IsLetterOrDigit);
	}

	/// <summary>
	///		Returns every attribute value and text node in the supplied document. Comment nodes are
	///		deliberately excluded.
	/// </summary>
	/// <param name="document">
	///		The parsed document to read.
	/// </param>
	private static IEnumerable<string> ReferenceTexts(XDocument document)
	{
		foreach (var node in document.DescendantNodes())
		{
			if (node is XElement element)
			{
				foreach (var attribute in element.Attributes())
				{
					yield return attribute.Value;
				}
			}
			else if (node is XText text)
			{
				yield return text.Value;
			}
		}
	}

	/// <summary>
	///		Returns the supplied text with every character replaced by a space, except newline
	///		characters, which are kept so that line numbers and offsets both survive.
	/// </summary>
	private static string Blank(string text)
	{
		return new string(
			text
				.Select(character =>
					character is '\n' or '\r'
						? character
						: ' ')
				.ToArray());
	}
}
