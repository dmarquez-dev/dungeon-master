using System.Text;
using FluentAssertions;

namespace DungeonMaster.Tests.App.Theme;

/// <summary>
///		Ratchet binding every control that declares the icon font to the glyphs that font can
///		actually draw.
/// </summary>
/// <remarks>
///		<para>
///			<c>RpgAwesome</c> maps private-use codepoints and the space, and nothing else. A control
///			that declares <c>FontFamily="{StaticResource DmFontIcon}"</c> and then sets its
///			<c>Text</c> to an ordinary symbol — a close cross, a triangular disclosure arrow, a
///			vertical ellipsis — is asking for a glyph the font does not have. Nothing goes missing
///			on screen, because the platform silently substitutes a system font; only the metrics
///			change, so the glyph lands off-centre and differently on every machine.
///		</para>
///		<para>
///			That is invisible to a build, invisible to a launch and invisible to a screenshot taken
///			by the person who wrote it. It has now been shipped twice: once in
///			<c>DungeonDetailPage.xaml</c>, and once across six further controls that a hand audit
///			scoped to the first file did not reach. A file-scoped audit cannot catch a class of
///			defect; reading the font's own <c>cmap</c> can.
///		</para>
///		<para>
///			Only the literal attribute form is checked. A <c>Text</c> assigned from code-behind or
///			bound is outside a static scan, which is why the sibling facts in
///			<c>DungeonMaster.Tests.App.Accessibility</c> exist — but a code-behind assignment still
///			inherits the <c>FontFamily</c> its XAML declares, so fixing the declaration here fixes
///			the imperative writer with it.
///		</para>
///		<para>
///			Both scans live in <see cref="IconFontFixture"/> and run once per class rather than
///			once per fact (MASTER-068), and the <c>cmap</c> reader they use is the named
///			<see cref="TrueTypeCmap"/> rather than a private class buried in this file
///			(MASTER-101).
///		</para>
/// </remarks>
public class IconFontCoverageTests : IClassFixture<IconFontFixture>
{
	private readonly IconFontFixture fixture;

	/// <summary>
	///		Initializes a new instance of the <see cref="IconFontCoverageTests"/> class.
	/// </summary>
	/// <param name="fixture">
	///		The once-per-class font and XAML scan.
	/// </param>
	public IconFontCoverageTests(IconFontFixture fixture)
	{
		this.fixture = fixture;
	}

	/// <summary>
	///		Asserts that the XAML scan <see cref="EveryDmFontIconTextIsInTheIconFontsCmap"/> reads
	///		reaches at least one control declaring <c>DmFontIcon</c> with a literal <c>Text</c>.
	/// </summary>
	/// <remarks>
	///		That fact's offenders list is empty both when every icon is right and when the scan
	///		reached nothing, so on its own it passes on an empty scan. This fact is the one that
	///		fails then. The reason below was written when the guard sat inside that fact, so its
	///		"this fact" names that one.
	/// </remarks>
	[Fact]
	public void TheIconTextScanReachesAtLeastOneDmFontIconDeclaration()
	{
		// Four plausible edits make the scan reach nothing while staying green: the alias
		// renamed, the fonts folder moved, DmFontIcon declared through a Setter instead of an
		// attribute, or every icon Text moved to code-behind.
		this.fixture
			.IconTexts
			.Should()
			.NotBeEmpty(
				"the scan must reach at least one control declaring DmFontIcon with a literal "
				+ "Text, or this fact guards nothing");
	}

	/// <summary>
	///		Every literal <c>Text</c> on a control declaring <c>DmFontIcon</c> must be drawable by
	///		the icon font.
	/// </summary>
	[Fact]
	public void EveryDmFontIconTextIsInTheIconFontsCmap()
	{
		// The offenders list below is empty both when every icon is right and when the scan
		// reached nothing; the second case is reported by
		// TheIconTextScanReachesAtLeastOneDmFontIconDeclaration, not here.
		var offenders = new List<string>();

		foreach (var declaration in this.fixture.IconTexts)
		{
			foreach (var codepoint in Codepoints(declaration.Text))
			{
				if (this.fixture.Codepoints.Contains(codepoint))
				{
					continue;
				}

				offenders.Add(
					$"{declaration.Where} -> <{declaration.Element}> declares DmFontIcon "
					+ $"but its Text contains U+{codepoint:X4}, which RpgAwesome does not map");
			}
		}

		offenders
			.Should()
			.BeEmpty(
				"a glyph outside the icon font's cmap is drawn by whatever the platform "
				+ "substitutes, so it renders with foreign metrics and differs by machine; use "
				+ "DmFontBody for a glyph the icon font does not carry");
	}

	/// <summary>
	///		Asserts the font parse itself is honest: a <c>cmap</c> read that silently produced an
	///		empty set would make the fact above pass everything, forever.
	/// </summary>
	[Fact]
	public void TheIconFontCmapParseFindsAtLeastOneCodepoint()
	{
		this.fixture
			.Codepoints
			.Should()
			.NotBeEmpty("the cmap parse must find glyphs or the coverage fact is vacuous");
	}

	/// <summary>
	///		Asserts the font parse finds at least one codepoint in the private-use area, where
	///		<c>RpgAwesome</c> maps its icons.
	/// </summary>
	[Fact]
	public void TheIconFontCmapParsesToPrivateUseGlyphs()
	{
		this.fixture
			.Codepoints
			.Where(codepoint =>
				codepoint is >= 0xE000 and <= 0xF8FF)
			.Should()
			.NotBeEmpty(
				"RpgAwesome maps its icons into the private-use area, so a parse that finds none "
				+ "of them has read the wrong table");
	}

	/// <summary>
	///		Asserts the font parse contains <c>U+EA8F</c>, the lower of the two codepoints the
	///		application actually asks the icon font for at its two extremes.
	/// </summary>
	/// <remarks>
	///		Named literally so a parse that returns a plausible-looking but wrong range fails.
	/// </remarks>
	[Fact]
	public void TheIconFontCmapContainsTheNotebookGlyph()
	{
		this.fixture
			.Codepoints
			.Should()
			.Contain(
				0xEA8F,
				"the notebook glyph is in use at FloorSidebarItem.xaml");
	}

	/// <summary>
	///		Asserts the font parse contains <c>U+EAD4</c>, the upper of the two codepoints the
	///		application actually asks the icon font for at its two extremes.
	/// </summary>
	/// <remarks>
	///		Named literally so a parse that returns a plausible-looking but wrong range fails.
	/// </remarks>
	[Fact]
	public void TheIconFontCmapContainsTheDividerOrnament()
	{
		this.fixture
			.Codepoints
			.Should()
			.Contain(
				0xEAD4,
				"the divider ornament is in use at DmDivider.xaml");
	}

	/// <summary>
	///		Guard fact for the font scan, per <c>contracts/anti-vacuity.md</c> section 2. This
	///		carries the protection that used to sit as a <c>.Should()</c> inside the non-test
	///		helper <c>IconFontCodepoints()</c>, where a missing font was reported against whichever
	///		fact happened to call it first.
	/// </summary>
	[Fact]
	public void TheIconFontScanReachesTheFontFileOnDisk()
	{
		this.fixture
			.IconFontExists
			.Should()
			.BeTrue(
				$"the icon font must be readable at '{this.fixture.IconFontPath}' for this fact "
				+ "to mean anything");
	}

	/// <summary>
	///		Yields the codepoints a font would have to draw. Whitespace is dropped because it is
	///		layout rather than a glyph, and a markup binding is dropped because its value is not
	///		knowable from the source.
	/// </summary>
	private static IEnumerable<int> Codepoints(string text)
	{
		if (text.Contains(
				'{',
				StringComparison.Ordinal))
		{
			yield break;
		}

		foreach (var rune in text.EnumerateRunes())
		{
			if (Rune.IsWhiteSpace(rune))
			{
				continue;
			}

			yield return rune.Value;
		}
	}
}
