using System.Xml.Linq;
using FluentAssertions;

namespace DungeonMaster.Tests.App.Theme;

/// <summary>
///		Guards the layered theme dictionaries under Resources/Theme.
///		<para>
///		Every XAML scan in this class reads the parsed documents published by
///		<see cref="ThemeSource"/> — <see cref="ThemeSource.KeyedElements"/>,
///		<see cref="ThemeSource.ThemeDocuments"/> and <see cref="ThemeSource.AppDocuments"/> — rather
///		than matching regular expressions against XAML text. The class declares no regular
///		expression of its own; the C# half of the orphan scan borrows the one shared matcher in
///		<see cref="ThemeSource"/>, and <c>XDocument</c> cannot parse C#.
///		</para>
/// </summary>
public class ThemeDictionaryTests
{
	/// <summary>
	///		The one dictionary that is allowed to carry colour literals.
	/// </summary>
	private const string TokensDictionaryName = "Tokens.xaml";

	/// <summary>
	///		Gets the resource keys every theme consumer is entitled to resolve.
	/// </summary>
	public static TheoryData<string> RequiredKeys =>
	[
		"DmSurfaceTavern", "DmSurfaceTavernRaised", "DmSurfaceTavernRaisedHover",
		"DmSurfaceTavernRaisedActive", "DmBorderTavern", "DmBorderTavernHover",
		"DmTextTavern", "DmTextTavernMuted",
		"DmSurfaceParchment", "DmSurfaceParchmentDeep", "DmBorderParchment",
		"DmTextParchment", "DmTextParchmentMuted",
		"DmAccentGold", "DmAccentGoldLight", "DmAccentGoldDark",
		"DmConfirm", "DmConfirmText", "DmDestructive", "DmDestructiveText",
		"DmNeutralSurface", "DmNeutralBorder",
		// DmSave, DmSaveText, DmDiscard and DmDiscardText were here and are gone. They had no
		// consumer in markup or in code, and this list was the only thing keeping them in
		// Tokens.xaml — a required-key entry for an unconsumed token is not a guard, it is a
		// test propping up dead code. Deleted with the tokens.
		"DmRibbonRead", "DmRibbonReadDim", "DmRibbonText", "DmRibbonSeal",
		"DmBannerEdge", "DmBannerCenter",
		"DmDragPlaceholderStroke", "DmDragPlaceholderFill",
		"DmAreaSelectedBorder", "DmAreaSelectedTint",
		"DmGoldHoverTint", "DmEditorGlowTint",
		"DmMapGrid", "DmMapAreaStroke", "DmMapVertex", "DmMapAreaFillActive",
		"DmMapAreaStrokeActive",
		"DmMapObjectPlaceholder",
		// DmAccentGoldBrush was here and is gone: it had no consumer anywhere in the repo and
		// was deleted with the orphaned styles rather than kept as a spare alias. Every gold
		// stroke in the app names DmAccentGoldLight and lets MAUI convert it.
		"DmBorderTavernBrush", "DmBorderTavernHoverBrush",
		"DmBorderParchmentBrush",
		"DmRadiusTight", "DmRadiusDefault", "DmRadiusLoose",
		"DmStrokeHairline", "DmStrokeEmphasis", "DmStrokeMedium",
		// The spacing scale and the padding roles. This is the only fact standing between a
		// call site and a resource key that silently does not exist.
		//
		// DmSpaceXXL (20) was here and is gone: the applying wave never met a 20, so the token
		// had no consumer and this entry was the only reason it survived. Six steps now. Note
		// the difference from the entries around it — a key that IS consumed is guarded here
		// against a typo; a key that is not is merely pinned, and pinning is not guarding.
		"DmSpaceXXS", "DmSpaceXS", "DmSpaceSmall", "DmSpaceMedium",
		"DmSpaceLarge", "DmSpaceXL",
		"DmPaddingTight", "DmPaddingCompact", "DmPaddingControl",
		"DmPaddingCard", "DmPaddingPanel", "DmPaddingPage",
		// Font families. Bound as FontFamily="..." by Tasks 4-11; a typo in one of
		// these fails silently at runtime rather than at build, so they are guarded.
		"DmFontTitle", "DmFontTitleStrong", "DmFontTitleDecorative",
		"DmFontBody", "DmFontBodyItalic", "DmFontIcon",

		"DmFontSizeBanner", "DmFontSizeTitle", "DmFontSizeHeading", "DmFontSizeSubheading",
		"DmFontSizeAction", "DmFontSizeEyebrow", "DmFontSizeMicro",
		"DmFontSizeBodyLarge", "DmFontSizeBody", "DmFontSizeBodySmall", "DmFontSizeCaption",
		"DmFontSizeMeta",
		"DmIconSizeTiny", "DmIconSizeSmall", "DmIconSizeMedium", "DmIconSizeLarge",
		"DmIconSizeXL", "DmIconSizeTool",

		// Step 0 additions: literals recovered from views, code-behind and
		// Platforms/Windows/App.xaml that carried no name before Task 2. Named from
		// their usage sites; see task-2-report.md for the full inventory.
		"DmSurfaceToolPanel", "DmSurfaceToolChip", "DmSurfaceToolChipHover",
		"DmBorderToolChip", "DmTextToolPanel",
		"DmSurfacePopup", "DmBorderPopup", "DmSurfaceStepper",
		"DmTextNotebook",
		"DmScrollbarThumb", "DmScrollbarThumbDisabled", "DmSurfaceTextSelection",
		"DmSurfaceComboItemHover", "DmSurfaceComboItemPressed", "DmTextComboItemDisabled",
		"DmDestructiveOnPopup", "DmPopupButtonHover", "DmPopupButtonPressed",
		"DmPopupButtonDestructiveHover", "DmPopupButtonDestructivePressed",
		"DmHairline", "DmHairlineStrong", "DmHairlineSolid", "DmTextPlaceholder",
		"DmDestructiveHoverSurface", "DmSavedStampText",
	];

	/// <summary>
	///		Asserts that the App.xaml merge scan finds at least one theme dictionary.
	///		<para>
	///		This is the guard that used to live inside <c>MergedThemeSourcesInAppXaml()</c>, which
	///		is an Arrange helper and therefore reported a broken scan against whichever fact called
	///		it first. It is a named fact now, per contracts/anti-vacuity.md section 2. Without it
	///		<see cref="EveryThemeFileIsMergedInAppXaml"/> compares two sets that a broken scan makes
	///		equal by both being empty.
	///		</para>
	/// </summary>
	[Fact]
	public void TheAppXamlMergeScanFindsAtLeastOneThemeDictionary()
	{
		MergedThemeSourcesInAppXaml()
			.Should()
			.NotBeEmpty(
				"App.xaml must merge theme dictionaries or the merge facts below compare nothing "
				+ "against nothing");
	}

	/// <summary>
	///		Asserts that a required key is defined somewhere in the theme dictionaries.
	/// </summary>
	/// <param name="key">
	///		The resource key that must resolve.
	/// </param>
	[Theory]
	[MemberData(nameof(RequiredKeys))]
	public void RequiredKeyIsDefinedInThemeDictionaries(string key)
	{
		DefinedThemeKeys()
			.Should()
			.Contain(
				key,
				"'{0}' is required by a theme consumer but no dictionary under Resources/Theme "
				+ "declares x:Key=\"{0}\"",
				key);
	}

	/// <summary>
	///		Asserts that colour literals are confined to the single token layer.
	///		Tokens.xaml absorbed the planned Palette.xaml because a markup extension
	///		in element-content position cannot be converted to a Color; see the
	///		comment at the top of Tokens.xaml.
	/// </summary>
	[Fact]
	public void TokensIsTheOnlyThemeFileContainingColourLiterals()
	{
		var documents = ThemeSource.ThemeDocuments();

		// The offenders list below is empty both when only Tokens.xaml carries literals and when
		// the scan reached no dictionary at all.
		documents
			.Should()
			.NotBeEmpty(
				"an empty offenders list proves nothing unless the scan that produced it reached "
				+ "at least one dictionary");

		var tokensDictionary = documents.FirstOrDefault(document =>
			string.Equals(
				Path.GetFileName(document.Path),
				TokensDictionaryName,
				StringComparison.Ordinal));

		tokensDictionary
			.Should()
			.NotBeNull("Tokens.xaml is the token layer this fact is written about");

		// The positive control. An empty offenders list is equally consistent with a clean tree
		// and with a reader that finds no literal anywhere, so the reader is shown finding the
		// literals in the ONE file that is supposed to carry them.
		ScannedText(tokensDictionary!.Document)
			.Any(ThemeSource.HexLiteralPattern.IsMatch)
			.Should()
			.BeTrue(
				"Tokens.xaml carries every colour literal in the theme, so a reader that finds "
				+ "none there is broken and its empty offenders list proves nothing");

		var offenders = documents
			.Where(document =>
				!document.Path.EndsWith(
					TokensDictionaryName,
					StringComparison.Ordinal))
			.Where(document =>
				ScannedText(document.Document).Any(ThemeSource.HexLiteralPattern.IsMatch))
			.Select(document =>
				Path.GetFileName(document.Path))
			.ToList();

		offenders.Should().BeEmpty();
	}

	/// <summary>
	///		Asserts that every file under Resources/Theme is merged into App.xaml, and that every
	///		dictionary merged from Resources/Theme in App.xaml names a file that exists. A
	///		dictionary that is never merged satisfies every other guard in this class and in
	///		<c>DmThemeKeysResolveTests</c> — both treat "defined in any file under Resources/Theme"
	///		as "resolvable" — while the app throws at runtime on the first
	///		<c>{StaticResource}</c> reference into it. This is the only fact that would catch that.
	/// </summary>
	[Fact]
	public void EveryThemeFileIsMergedInAppXaml()
	{
		var themeSources = ThemeSource
			.ThemeFileSources()
			.ToHashSet(StringComparer.Ordinal);

		// Two empty sets are equivalent, so the comparison below says nothing at all unless the
		// file scan reached something.
		themeSources
			.Should()
			.NotBeEmpty(
				"the theme file scan must reach at least one dictionary or this comparison holds "
				+ "vacuously");

		var mergedSources = MergedThemeSourcesInAppXaml()
			.ToHashSet(StringComparer.Ordinal);

		themeSources.Should().BeEquivalentTo(mergedSources);
	}

	/// <summary>
	///		Asserts that App.xaml merges the theme dictionaries in a dependency-safe order.
	///		A merged dictionary may reference keys from those merged before it and not from those
	///		merged after, so the order in App.xaml is load-bearing markup — Controls.Tavern.xaml
	///		derives from <c>DmButtonBase</c> in Controls.Base.xaml, and every Controls file
	///		references the tokens and typography above it. <c>EveryThemeFileIsMergedInAppXaml</c>
	///		compares sets, which is order-insensitive: the seven entries can be shuffled into any
	///		permutation, pass every other fact in this suite, and throw at startup on the first
	///		<c>{StaticResource}</c> that reaches backwards. This is the fact that reads the order.
	/// </summary>
	[Fact]
	public void ThemeDictionariesAreMergedInDependencyOrder()
	{
		var merged = MergedThemeSourcesInAppXaml()
			.Select(source =>
				Path.GetFileName(source))
			.ToList();

		var controlsIndices = merged
			.Select((fileName, index) =>
				(fileName, index))
			.Where(entry =>
				entry.fileName.StartsWith(
					"Controls.",
					StringComparison.Ordinal))
			.ToList();

		controlsIndices
			.Should()
			.NotBeEmpty("App.xaml must merge at least one Controls dictionary or this fact is vacuous");

		var firstControls = controlsIndices.Min(entry =>
			entry.index);

		foreach (var foundation in new[] { "Tokens.xaml", "Typography.xaml" })
		{
			var index = merged.IndexOf(foundation);

			index
				.Should()
				.BeGreaterThanOrEqualTo(
					0,
					$"App.xaml must merge {foundation}");

			index
				.Should()
				.BeLessThan(
					firstControls,
					$"{foundation} defines the tokens every Controls dictionary references, so it "
					+ $"must be merged before {merged[firstControls]}");
		}

		var controlsBaseIndex = merged.IndexOf("Controls.Base.xaml");

		controlsBaseIndex
			.Should()
			.BeGreaterThanOrEqualTo(
				0,
				"App.xaml must merge Controls.Base.xaml");

		var derivedControls = controlsIndices
			.Where(entry =>
				entry.fileName != "Controls.Base.xaml")
			.ToList();

		foreach (var derived in derivedControls)
		{
			derived
				.index
				.Should()
				.BeGreaterThan(
					controlsBaseIndex,
					$"{derived.fileName} derives styles with BasedOn from Controls.Base.xaml, so "
					+ "Controls.Base.xaml must be merged first");
		}

		merged[^1]
			.Should()
			.Be(
				"Implicit.xaml",
				"the implicit style floor references keys from every dictionary above it and must "
				+ "be merged last");
	}

	/// <summary>
	///		Asserts that every <c>Style</c> declared with an <c>x:Key</c> under Resources/Theme has
	///		at least one consumer OUTSIDE A COMMENT.
	///		<para>
	///		The "outside a comment" clause is the entire point and it is not defensive coding. The
	///		hand census this fact replaces recorded <c>DmBorderTavernStyle</c> as live on the
	///		strength of a single grep hit — which turned out to be a mention of the key inside the
	///		prose comment at the top of Controls.Base.xaml. It had no call site at all. Seventeen
	///		keys were dead when that census said six were, and every one of them shipped, built
	///		clean and rendered nothing.
	///		</para>
	///		<para>
	///		A consumer is a <c>{StaticResource Key}</c> reference in any application markup —
	///		including a <c>BasedOn</c>, which is how a base style earns its keep — or a
	///		<c>"Key"</c> string literal in application code, which is how <c>DmTheme</c> resolves
	///		the two button styles <c>DmConfirmPopup</c> swaps between. Comments are excluded from
	///		both before anything is counted: a comment is an XML comment NODE in the parsed markup
	///		and is never read, and the C# text is blanked by
	///		<see cref="ThemeSource.BlankCSharpComments"/>.
	///		</para>
	///		<para>
	///		Every reference is collected ONCE, into a set, and each style key is then tested for
	///		membership. The two per-style compiled regular expressions this replaces were built
	///		inside the loop, which is the exact inversion of what <c>RegexOptions.Compiled</c> is
	///		for: it raises construction cost to lower match cost, so paying it per iteration loses
	///		on both.
	///		</para>
	///		<para>
	///		A style consumed only by another style that is itself orphaned is reported one layer at
	///		a time: the leaf fails, and deleting it exposes the base on the next run. That is
	///		deliberate — resolving the whole chain in one pass would name files whose only fault is
	///		being downstream of the real one.
	///		</para>
	/// </summary>
	[Fact]
	public void NoOrphanedStyleKeys()
	{
		var styleKeys = ThemeSource
			.KeyedElements()
			.Where(keyed =>
				keyed.QualifiedElementName == "Style"
				&& ThemeSource.IsCurrentKey(keyed.Key))
			.Select(keyed =>
				new
				{
					keyed.Key,
					File = Path.GetFileName(keyed.RelativePath),
				})
			.ToList();

		styleKeys
			.Should()
			.NotBeEmpty("the theme dictionaries must declare keyed styles or this fact is vacuous");

		var referencedInMarkup = ThemeSource
			.References()
			.Select(reference =>
				reference.Key)
			.ToHashSet(StringComparer.Ordinal);

		var referencedInCode = RepoPaths
			.AppSourceFiles("*.cs")
			.SelectMany(path =>
				ThemeSource.CodeKeyLiterals(
					ThemeSource.BlankCSharpComments(File.ReadAllText(path))))
			.ToHashSet(StringComparer.Ordinal);

		var orphans = styleKeys
			.Where(style =>
				!referencedInMarkup.Contains(style.Key)
				&& !referencedInCode.Contains(style.Key))
			.Select(style =>
				$"{style.File} -> {style.Key} is declared as a Style but nothing outside a "
				+ "comment consumes it; adopt it at a call site or delete it")
			.ToList();

		orphans
			.Should()
			.BeEmpty(
				"an unconsumed style is an abstraction that renders nowhere — it builds clean, "
				+ "launches clean and is invisible until somebody counts");
	}

	/// <summary>
	///		Returns the theme dictionaries merged from Resources/Theme by App.xaml, in the order
	///		App.xaml merges them. Asserts nothing and returns an empty list when App.xaml is not
	///		reached; <see cref="TheAppXamlMergeScanFindsAtLeastOneThemeDictionary"/> is the guard.
	/// </summary>
	private static IReadOnlyList<string> MergedThemeSourcesInAppXaml()
	{
		var appXaml = ThemeSource
			.AppDocuments()
			.FirstOrDefault(document =>
				string.Equals(
					document.RelativePath,
					"App.xaml",
					StringComparison.Ordinal));

		if (appXaml is null)
		{
			return [];
		}

		// Document order is the merge order, and the merge order is load-bearing markup.
		return appXaml
			.Document
			.Descendants()
			.Where(element =>
				ThemeSource.QualifiedElementName(element.Name) == "ResourceDictionary")
			.Select(element =>
				element.Attribute("Source")?.Value)
			.Where(source =>
				source is not null
				&& source.StartsWith(
					"Resources/Theme/",
					StringComparison.Ordinal)
				&& source.EndsWith(
					".xaml",
					StringComparison.Ordinal))
			.Select(source =>
				source!)
			.ToList();
	}

	/// <summary>
	///		Returns every resource key declared under Resources/Theme.
	/// </summary>
	private static HashSet<string> DefinedThemeKeys()
	{
		return ThemeSource
			.KeyedElements()
			.Select(keyed =>
				keyed.Key)
			.ToHashSet(StringComparer.Ordinal);
	}

	/// <summary>
	///		Returns every attribute value, text node and COMMENT in the supplied document.
	///		<para>
	///		Comments are included here, unlike in the reference scan: a colour literal parked in a
	///		comment in a Controls dictionary is still a colour literal outside the token layer, and
	///		the raw-text scan this replaces caught it. contracts/theme-source.md section 3.5
	///		measured the two readers against ten synthetic cases and they disagreed once, in the
	///		XML model's favour: a literal split by a character entity, which the parser resolves
	///		and the raw pattern cannot see.
	///		</para>
	/// </summary>
	/// <param name="document">
	///		The parsed document to read.
	/// </param>
	private static IEnumerable<string> ScannedText(XDocument document)
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
			else if (node is XComment comment)
			{
				yield return comment.Value;
			}
			else if (node is XProcessingInstruction instruction)
			{
				yield return instruction.Data;
			}
		}
	}
}
