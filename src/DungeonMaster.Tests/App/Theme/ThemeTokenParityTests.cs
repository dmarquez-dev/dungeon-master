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
///		legacy <c>DM</c> casing — see <see cref="ThemeSource.IsLegacyTolerantKey"/> for
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

		var references = ThemeSource
			.References()
			.Where(reference =>
				ThemeSource.IsLegacyTolerantKey(reference.Key))
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
				ThemeSource
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
						key is not null && ThemeSource.IsLegacyTolerantKey(key))
					.Select(key =>
						new KeyValuePair<string, string>(
							key!,
							document.RelativePath)))
			.ToList();
	}
}
