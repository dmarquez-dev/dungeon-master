using System.Text.RegularExpressions;
using System.Xml.Linq;
using FluentAssertions;

namespace DungeonMaster.Tests.App.Theme;

/// <summary>
///		Parity ratchet between the theme keys the application references and the keys the
///		dictionaries under <c>Resources/</c> actually define. A <c>{StaticResource}</c> or a
///		<c>Resources["…"]</c> lookup that names a key nobody declares builds cleanly and fails at
///		render time, so both directions are scanned here rather than left to the running app.
///		<para>
///		The XAML half of both scans reads the parsed documents from
///		<see cref="ThemeSource.AppDocuments"/>; only the C# half is matched with a regular
///		expression, because <c>XDocument</c> cannot parse C#. Both scans deliberately tolerate the
///		legacy <c>DM</c> casing — see <see cref="ThemeMarkupReferences.IsLegacyTolerantKey"/> for
///		why that divergence has to exist.
///		</para>
/// </summary>
public class ThemeTokenParityTests
{
	/// <summary>
	///		Dm-prefixed string literals that are deliberately not resource keys. Add to this only
	///		for identifiers that never reach a <c>ResourceDictionary</c> lookup, and say why.
	/// </summary>
	private static readonly HashSet<string> NonResourceLiterals = new(StringComparer.Ordinal)
	{
		// MauiProgram's handler-mapper key: an arbitrary private name for an AppendToMapping
		// registration, never looked up in Application.Current.Resources.
		"DmNativeStyle",
	};

	/// <summary>
	///		Asserts that the definition scan this class is built on reaches at least one resource
	///		key.
	///		<para>
	///		This is the guard that used to live inside <c>ResourceKeyDefinitions()</c>, which is an
	///		Arrange helper and therefore reported a broken scan against whichever fact called it
	///		first. It is a named fact now, per contracts/anti-vacuity.md section 2: both parity
	///		facts below resolve every reference against an empty set if this one fails, and would
	///		then report every reference in the application as dangling rather than naming the real
	///		cause.
	///		</para>
	/// </summary>
	[Fact]
	public void TheResourceKeyDefinitionScanFindsAtLeastOneKey()
	{
		ResourceKeyDefinitions()
			.Should()
			.NotBeEmpty(
				"the definition scan must reach the dictionaries under Resources/ or both parity "
				+ "facts resolve against nothing");
	}

	/// <summary>
	///		Asserts that every <c>{StaticResource Dm…}</c> reference in application markup names a
	///		key that is actually declared under <c>Resources/</c>. A dangling reference is not a
	///		build error in XAML; it throws when the element is first inflated, which for a control
	///		behind a collapsed expander or an unvisited page can be a long time after the change
	///		that broke it.
	/// </summary>
	[Fact]
	public void EveryReferencedThemeKey_IsDefinedUnderResources()
	{
		var definitions = ResourceKeyDefinitions()
			.Select(definition =>
				definition.Key)
			.ToHashSet(StringComparer.Ordinal);

		var references = ThemeMarkupReferences
			.References()
			.Where(reference =>
				ThemeMarkupReferences.IsLegacyTolerantKey(reference.Key))
			.ToList();

		// The missing list below is empty both when every reference resolves and when the markup
		// scan reached no reference at all, and those two states must not look the same.
		references
			.Should()
			.NotBeEmpty(
				"an empty dangling-reference list proves nothing unless the markup scan that "
				+ "produced it found at least one StaticResource reference to a Dm-prefixed key");

		var missing = references
			.Where(reference =>
				!definitions.Contains(reference.Key))
			.Select(reference =>
				$"{reference.Value} -> {reference.Key}")
			.ToList();

		missing.Should().BeEmpty();
	}

	/// <summary>
	///		Asserts that every Dm-prefixed string literal in application code names a key that
	///		actually exists under <c>Resources/</c>. This is the C# counterpart of
	///		<see cref="EveryReferencedThemeKey_IsDefinedUnderResources"/>: it scans literals rather
	///		than parsing indexer expressions, so a key hidden in a ternary, a switch arm or a const
	///		is caught just as readily as a direct <c>Resources["Key"]</c> lookup.
	///		<para>
	///		It resolves against <c>Resources/Theme/</c> alone, deliberately excluding the legacy
	///		<c>Resources/Styles/Colors.xaml</c> that Task 10 deleted. Accepting a legacy key
	///		because it happened to still be defined would have deferred every such failure to
	///		the moment that file was removed, which is the opposite of what a ratchet is for.
	///		</para>
	/// </summary>
	[Fact]
	public void EveryThemeKeyStringLiteralInCode_IsDefinedUnderResources()
	{
		var definitions = ResourceKeyDefinitions()
			.Where(definition =>
				definition.Value.Contains(
					"Resources/Theme/",
					StringComparison.Ordinal))
			.Select(definition =>
				definition.Key)
			.ToHashSet(StringComparer.Ordinal);

		var literals = RepoPaths
			.AppSourceFiles("*.cs")
			.SelectMany(path =>
				// Comments name keys in prose, and a doc comment names types with
				// <see cref="DmCard"/>, which is not a resource lookup. Blanked rather than
				// deleted so offsets survive.
				ThemeMarkupReferences
					.CodeKeyLiterals(ThemeSource.BlankCSharpComments(File.ReadAllText(path)))
					.Select(key =>
						(Key: key, Path: path)))
			.ToList();

		// The missing list below is empty both when every literal resolves and when the code scan
		// found no literal at all — because it reached no file, or because the matcher is broken.
		literals
			.Should()
			.NotBeEmpty(
				"an empty dangling-literal list proves nothing unless the code scan that produced "
				+ "it found at least one Dm-prefixed string literal");

		var missing = literals
			.Where(literal =>
				!definitions.Contains(literal.Key)
				&& !NonResourceLiterals.Contains(literal.Key))
			.Select(literal =>
				$"{Path.GetRelativePath(RepoPaths.AppRoot, literal.Path)} -> {literal.Key}")
			.ToList();

		missing.Should().BeEmpty();
	}

	/// <summary>
	///		Returns every resource key defined under <c>Resources/</c>, paired with the declaring
	///		dictionary's path relative to the application root (forward slashes) so callers can
	///		scope by dictionary. Asserts nothing;
	///		<see cref="TheResourceKeyDefinitionScanFindsAtLeastOneKey"/> is the guard.
	/// </summary>
	private static IReadOnlyList<KeyValuePair<string, string>> ResourceKeyDefinitions()
	{
		return ThemeSource
			.AppDocuments()
			.Where(document =>
				document.RelativePath.Contains(
					"Resources/",
					StringComparison.Ordinal))
			.SelectMany(document =>
				document
					.Document
					.Descendants()
					.Select(element =>
						element
							.Attributes()
							.FirstOrDefault(attribute =>
								ThemeSource.QualifiedElementName(attribute.Name) == "x:Key")
							?.Value)
					.Where(key =>
						key is not null && ThemeMarkupReferences.IsLegacyTolerantKey(key))
					.Select(key =>
						new KeyValuePair<string, string>(
							key!,
							document.RelativePath)))
			.ToList();
	}
}

/// <summary>
///		The shared reader for theme-key REFERENCES — the <c>{StaticResource …}</c> markup extensions
///		in the application's XAML, and the Dm-prefixed string literals in its C#.
///		<para>
///		WHY IT LIVES IN THIS FILE. <c>ThemeDictionaryTests.NoOrphanedStyleKeys</c> and the two
///		parity facts above need the same reference scan, and two private copies of it is exactly
///		the divergence <c>MASTER-138</c> records. Its natural home is <see cref="ThemeSource"/>, and
///		<c>contracts/theme-source.md</c> declares no such member; T4 owns no file it could be added
///		to, so it is declared once here — in the file whose subject IS references — rather than
///		duplicated. Relocating it onto <c>ThemeSource</c> is a contract change and is reported as
///		one.
///		</para>
/// </summary>
internal static class ThemeMarkupReferences
{
	/// <summary>
	///		Matches a Dm- or DM-prefixed string literal in C# SOURCE TEXT. This is the one regular
	///		expression left in this file and it is never matched against XAML: <c>XDocument</c>
	///		cannot parse C#, which is the boundary <c>contracts/theme-source.md</c> section 3.5
	///		draws. Code-behind resolves some theme colours through
	///		<c>Application.Current.Resources[…]</c>, where the key is a plain string; XAML-only
	///		scanning cannot see those, and a stale key there is invisible until it throws at render
	///		time — <c>NotebookView.RibbonBrush</c> carried "DMRibbonRead" inside a ternary through
	///		the whole of Task 5's migration for exactly that reason.
	/// </summary>
	private static readonly Regex CSharpKeyLiteralPattern =
		new(
			@"""(D[Mm][A-Za-z0-9]{2,})""",
			RegexOptions.Compiled);

	/// <summary>
	///		The opening of a StaticResource markup extension.
	/// </summary>
	private const string StaticResourceOpening = "{StaticResource";

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
	public static IEnumerable<string> StaticResourceKeys(string value)
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
	///		The <c>DM</c> tolerance is deliberate and load-bearing, and it mirrors
	///		<see cref="ThemeSource.LegacyTolerantKeyDefinitionPattern"/>, whose doc comment records
	///		the reason: Task 10 deleted the legacy <c>Resources/Styles/Colors.xaml</c>, whose keys
	///		were <c>DM</c>-prefixed, and the parity facts must be able to NAME a surviving
	///		<c>DM</c> reference in order to report it as dangling. A <c>Dm</c>-only predicate would
	///		not match the reference, would not add it to the missing list, and would pass.
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
}
