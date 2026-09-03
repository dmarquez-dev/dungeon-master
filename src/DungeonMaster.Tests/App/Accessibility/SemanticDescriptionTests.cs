using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using DungeonMaster.Tests.App.Theme;
using FluentAssertions;

namespace DungeonMaster.Tests.App.Accessibility;

/// <summary>
///		Ratchet for the accessibility of icon-only controls.
/// </summary>
/// <remarks>
///		<para>
///			Before this fact the application declared zero <c>SemanticProperties</c> and zero
///			<c>AutomationId</c>s, across roughly twenty controls whose entire label is a private-use
///			codepoint out of an icon font. A screen reader announces nothing usable for any of them
///			— not a wrong name, no name at all — and nothing in the build, the suite or the running
///			app said so.
///		</para>
///		<para>
///			A <c>ToolTipProperties.Text</c> is not a substitute. It is a hover affordance, so it
///			reaches a mouse user and nobody else.
///		</para>
///		<para>
///			The scan reads the parsed XAML from <see cref="ThemeSource.AppDocuments"/> rather than
///			matching regular expressions against the file text. The two patterns it replaces each
///			lost controls silently: <c>ControlTag</c> matched <c>[^&gt;]*?</c> for a tag's
///			attributes, so a literal <c>&gt;</c> anywhere in an earlier attribute value truncated
///			the tag and the control was never examined, and <c>TextAttribute</c> matched only the
///			double-quoted form, so a single-quoted <c>Text='…'</c> was invisible. Neither failure
///			announced itself — the scan simply reached fewer controls and reported green.
///		</para>
/// </remarks>
public class SemanticDescriptionTests
{
	/// <summary>
	///		The element names this ratchet applies to, compared against
	///		<see cref="ThemeSource.QualifiedElementName(XName)"/> so that a hypothetical
	///		<c>x:Label</c> in the XAML namespace is not swept up with the MAUI control.
	/// </summary>
	private static readonly IReadOnlySet<string> ControlElements =
		new HashSet<string>(StringComparer.Ordinal)
		{
			"Label",
			"Button",
		};

	private const string TextAttributeName = "Text";

	private const string DescriptionAttributeName = "SemanticProperties.Description";

	private const string InAccessibleTreeAttributeName = "AutomationProperties.IsInAccessibleTree";

	private const string MauiNamespace = "http://schemas.microsoft.com/dotnet/2021/maui";

	/// <summary>
	///		Files whose icon-only controls are knowingly still undescribed. Empty, and meant to
	///		stay that way: every offender this fact found when it was written was fixed rather than
	///		listed, so an entry here is a regression with a note attached, not a normal state.
	/// </summary>
	/// <remarks>
	///		An entry is a path relative to <see cref="RepoPaths.AppRoot"/> written with FORWARD
	///		slashes, because that is the spelling <see cref="ThemeDocument.RelativePath"/> carries.
	/// </remarks>
	private static readonly IReadOnlySet<string> PendingDescription =
		new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	/// <summary>
	///		Every <c>Label</c> or <c>Button</c> whose entire <c>Text</c> is a single private-use or
	///		symbol codepoint must either carry a <c>SemanticProperties.Description</c> or be taken
	///		out of the accessibility tree outright.
	/// </summary>
	/// <remarks>
	///		The second option is not a loophole. A glyph sitting on top of an already-labelled
	///		button, or beside a wordmark that says the same thing, has no announcement of its own to
	///		make, and <c>AutomationProperties.IsInAccessibleTree="False"</c> is how MAUI says so —
	///		as opposed to leaving it in the tree with nothing to read out.
	/// </remarks>
	[Fact]
	public void EveryIconOnlyControlHasASemanticDescription()
	{
		var reached = ScannedIconOnlyControls();

		var offenders = reached
			.Where(IsOffender)
			.Select(DescribeOffence)
			.ToList();

		offenders
			.Should()
			.BeEmpty(
				"a control whose whole label is an icon codepoint announces nothing to a screen "
				+ "reader unless it carries a non-blank SemanticProperties.Description, or is "
				+ @"marked AutomationProperties.IsInAccessibleTree=""False"" because something "
				+ "beside it already says the same thing");

		// Anti-vacuity, and the only scanner in the suite that was missing one. Four plausible
		// edits make this scan reach nothing while staying green — a glyph moved into element
		// content, a decimal character entity, single-quoted attributes, or a Text written from
		// code-behind — and §4.2 of this branch converted glyph Labels to vector shapes, which is
		// exactly the kind of change that shrinks it. A guard that has stopped looking must say so.
		// Two of those four stop being reachable once the scan parses the XAML: the parser resolves
		// a decimal entity and reads a single-quoted attribute alike. The other two remain, which
		// is why the guard remains.
		reached
			.Should()
			.NotBeEmpty(
				"the scan must reach at least one icon-only control, or this fact passes because "
				+ "it found nothing rather than because everything it found was described");
	}

	/// <summary>
	///		Every file named by <see cref="PendingDescription"/> must exist, must be part of the
	///		application XAML scan, and must still contain an undescribed icon-only control.
	/// </summary>
	/// <remarks>
	///		Without this, the allow-list decays into a list of files that were once a problem, and
	///		the ratchet quietly stops applying to whatever is added to them later. It is the same
	///		shape as the geometry suite's <c>EveryGeometryAllowListEntryStillOffends</c>, and it is
	///		why the list above can safely be empty: an empty list has nothing to go stale.
	/// </remarks>
	[Fact]
	public void EverySemanticAllowListEntryStillOffends()
	{
		var stale = new List<string>();

		foreach (var relative in PendingDescription)
		{
			var path = Path.Combine(
				RepoPaths.AppRoot,
				relative);

			if (!File.Exists(path))
			{
				stale.Add($"{relative} -> file does not exist; the entry exempts nothing");
				continue;
			}

			var document = ThemeSource
				.AppDocuments()
				.FirstOrDefault(candidate =>
					string.Equals(
						candidate.RelativePath,
						relative.Replace(
							'\\',
							'/'),
						StringComparison.OrdinalIgnoreCase));

			if (document is null)
			{
				stale.Add(
					$"{relative} -> is not reached by the application XAML scan; the entry "
					+ "exempts nothing");
				continue;
			}

			if (!IconOnlyControls(
					document.Document,
					document.RelativePath)
				.Any(IsOffender))
			{
				stale.Add($"{relative} -> no longer offends; remove it from the allow-list");
			}
		}

		stale
			.Should()
			.BeEmpty("an allow-list entry that no longer exempts anything hides the next offender");
	}

	/// <summary>
	///		A <c>Label</c> whose <c>Text</c> is a supplementary-plane symbol and which carries no
	///		description is reported as an offender.
	/// </summary>
	/// <remarks>
	///		This is the end-to-end consequence of <c>MASTER-040</c>, not a helper test. The previous
	///		implementation decoded the entity through a <c>char</c> cast over a parsed hex
	///		value, truncating
	///		<c>U+1F6E1</c> to the unrelated BMP character <c>U+F6E1</c>, and then rejected anything
	///		whose <c>string.Length</c> was not 1 — a surrogate pair is two <c>char</c>s and one
	///		Rune, so the control was dropped from the scan entirely and passed the ratchet
	///		undescribed. Both halves are arithmetic, so neither can be quietly satisfied by a second
	///		mechanism.
	/// </remarks>
	[Fact]
	public void ASupplementaryPlaneIconWithNoDescriptionIsReportedAsAnOffender()
	{
		var document = SyntheticDocument(
			$@"<ContentView xmlns=""{MauiNamespace}"">",
			@"	<Label Text=""&#x1F6E1;"" />",
			@"</ContentView>");

		var reached = IconOnlyControls(
			document,
			"Synthetic.xaml");

		reached
			.Should()
			.ContainSingle(
				"a supplementary-plane symbol is one Rune and must be classified as icon-only; "
				+ "the char cast and the string.Length test both dropped it");

		reached[0]
			.Glyph
			.Should()
			.Be(
				char.ConvertFromUtf32(0x1F6E1),
				"the entity must decode to the whole codepoint, not to its low 16 bits");

		reached
			.Where(IsOffender)
			.Should()
			.ContainSingle(
				"the control carries no SemanticProperties.Description and is not excluded from "
				+ "the accessibility tree, which is exactly the state this ratchet exists to "
				+ "prevent");
	}

	/// <summary>
	///		A control is examined even when an earlier attribute value contains a literal
	///		<c>&gt;</c>.
	/// </summary>
	/// <remarks>
	///		A literal <c>&gt;</c> is legal in an XML attribute value — only <c>&lt;</c> and
	///		<c>&amp;</c> must be escaped. The <c>[^&gt;]*?</c> attribute match the previous scan
	///		used ended the tag at that character, so the <c>Text</c> that followed it was never
	///		seen and the control was never examined.
	/// </remarks>
	[Fact]
	public void AControlWithAGreaterThanSignInAnEarlierAttributeIsExamined()
	{
		var document = SyntheticDocument(
			$@"<ContentView xmlns=""{MauiNamespace}"">",
			@"	<Label AutomationId=""width > height""",
			@"		   Text=""&#xE9FC;"" />",
			@"</ContentView>");

		var reached = IconOnlyControls(
			document,
			"Synthetic.xaml");

		reached
			.Should()
			.ContainSingle(
				"the XML parser ends the tag at the real tag delimiter, so an attribute value "
				+ "holding a literal '>' no longer truncates the control out of the scan");
	}

	/// <summary>
	///		A control whose <c>Text</c> is written with single quotes is examined.
	/// </summary>
	/// <remarks>
	///		The previous scan matched only <c>Text="…"</c>, so a single-quoted attribute — legal
	///		XML and legal XAML — was invisible and the control passed the ratchet without ever
	///		being looked at.
	/// </remarks>
	[Fact]
	public void AControlWithASingleQuotedTextAttributeIsExamined()
	{
		var document = SyntheticDocument(
			$@"<ContentView xmlns=""{MauiNamespace}"">",
			@"	<Label Text='&#xE9FC;' />",
			@"</ContentView>");

		var reached = IconOnlyControls(
			document,
			"Synthetic.xaml");

		reached
			.Should()
			.ContainSingle(
				"the XML parser reads both quoting forms, so a single-quoted Text is no longer "
				+ "invisible to the scan");
	}

	/// <summary>
	///		An offender is reported against the line its element is declared on.
	/// </summary>
	/// <remarks>
	///		The line comes from the parsed document's own line info, not from counting newlines
	///		ahead of a regex match index. A reported line that is off by even one sends the next
	///		reader to the wrong control, and the counting form this replaces was one of the three
	///		identical copies <c>MASTER-138</c> names.
	/// </remarks>
	[Fact]
	public void TheScanReportsTheDeclaredLineOfAnOffendingControl()
	{
		var document = SyntheticDocument(
			$@"<ContentView xmlns=""{MauiNamespace}"">",
			@"	<VerticalStackLayout>",
			@"		<Label Text=""Words are fine"" />",
			@"		<Label Text=""&#xE9FC;"" />",
			@"	</VerticalStackLayout>",
			@"</ContentView>");

		var offenders = IconOnlyControls(
				document,
				"Synthetic.xaml")
			.Where(IsOffender)
			.Select(DescribeOffence)
			.ToList();

		offenders
			.Should()
			.ContainSingle()
			.Which
			.Should()
			.StartWith(
				"Synthetic.xaml:4 ",
				"the offending Label is declared on line 4 of the fragment, and a ratchet that "
				+ "names the wrong line sends the next reader to the wrong control");
	}

	/// <summary>
	///		An icon-only control the scan reaches, with the line it was declared on.
	/// </summary>
	/// <param name="RelativePath">
	///		The declaring file's path relative to the application project root, forward slashes.
	/// </param>
	/// <param name="LineNumber">
	///		The 1-based line the control's element starts on, from the parsed document's line info.
	/// </param>
	/// <param name="ElementName">
	///		The qualified element name, as <see cref="ThemeSource.QualifiedElementName(XName)"/>
	///		spells it.
	/// </param>
	/// <param name="Glyph">
	///		The decoded <c>Text</c> value — exactly one Rune, by construction.
	/// </param>
	/// <param name="Element">
	///		The declaring element, so a caller can read the rest of its attributes.
	/// </param>
	private sealed record IconControl(
		string RelativePath,
		int LineNumber,
		string ElementName,
		string Glyph,
		XElement Element);

	/// <summary>
	///		Returns every icon-only control in the application's XAML, skipping the allow-listed
	///		files. Returns an EMPTY list when it reaches nothing; non-emptiness is the caller's
	///		guard, per contracts/anti-vacuity.md §1.
	/// </summary>
	private static IReadOnlyList<IconControl> ScannedIconOnlyControls()
	{
		var found = new List<IconControl>();

		foreach (var document in ThemeSource.AppDocuments())
		{
			if (PendingDescription.Contains(document.RelativePath))
			{
				continue;
			}

			found.AddRange(
				IconOnlyControls(
					document.Document,
					document.RelativePath));
		}

		return found;
	}

	/// <summary>
	///		Returns every icon-only control in one parsed document. Asserts nothing.
	/// </summary>
	/// <param name="document">
	///		The parsed XAML.
	/// </param>
	/// <param name="relativePath">
	///		The path to report the control against.
	/// </param>
	private static IReadOnlyList<IconControl> IconOnlyControls(
		XDocument document,
		string relativePath)
	{
		var found = new List<IconControl>();

		foreach (var element in document.Descendants())
		{
			var elementName = ThemeSource.QualifiedElementName(element.Name);

			if (!ControlElements.Contains(elementName))
			{
				continue;
			}

			var text = element.Attribute(TextAttributeName);

			if (text is null)
			{
				continue;
			}

			var glyph = DecodedValue(text);

			if (!IsIconOnly(glyph))
			{
				continue;
			}

			found.Add(
				new IconControl(
					relativePath,
					LineNumberOf(element),
					elementName,
					glyph,
					element));
		}

		return found;
	}

	/// <summary>
	///		Whether the control is one this ratchet reports: no readable description, and not taken
	///		out of the accessibility tree.
	/// </summary>
	private static bool IsOffender(IconControl control) =>
		!HasUsableDescription(control.Element)
		&& !IsExcludedFromAccessibilityTree(control.Element);

	/// <summary>
	///		Whether the control carries a description a screen reader can actually read out.
	/// </summary>
	/// <remarks>
	///		The value matters, not the attribute's presence. Testing only for presence passed
	///		<c>SemanticProperties.Description=""</c> — proved by construction — and an empty
	///		description announces exactly nothing, which is the state this fact exists to prevent.
	/// </remarks>
	private static bool HasUsableDescription(XElement element)
	{
		var declared = element.Attribute(DescriptionAttributeName);

		return declared is not null
			&& !string.IsNullOrWhiteSpace(
				DecodedValue(declared));
	}

	/// <summary>
	///		Whether the control is declared out of the accessibility tree.
	/// </summary>
	/// <remarks>
	///		Parsed rather than string-compared against <c>"False"</c>: XAML's boolean conversion is
	///		case-insensitive, so <c>"false"</c> takes the control out of the tree just as
	///		<c>"False"</c> does, and a scan that only recognised one spelling would report a
	///		correctly excluded control as an offender.
	/// </remarks>
	private static bool IsExcludedFromAccessibilityTree(XElement element)
	{
		var declared = element.Attribute(InAccessibleTreeAttributeName);

		return declared is not null
			&& bool.TryParse(
				declared.Value,
				out var inAccessibleTree)
			&& !inAccessibleTree;
	}

	/// <summary>
	///		Returns the attribute's text with any character entity the XML parser left literal
	///		expanded, using the shared decoder.
	/// </summary>
	/// <remarks>
	///		The parser resolves <c>&amp;#xNNNN;</c> and <c>&amp;#NNN;</c> itself, so on every form
	///		present in this repository the call is an identity. It is here so that this class holds
	///		no decoder of its own: the local one it replaces truncated a supplementary-plane
	///		codepoint through a <c>char</c> cast over a parsed hex value and matched the hex
	///		form only, and the whole
	///		point of <c>MASTER-040</c> is that two decoders three files apart drifted.
	/// </remarks>
	private static string DecodedValue(XAttribute attribute) =>
		ThemeSource.DecodeCharacterEntities(attribute.Value);

	/// <summary>
	///		Returns the 1-based line the element starts on, or 0 when line info is unavailable.
	/// </summary>
	/// <remarks>
	///		This replaces an inline newline count over the text ahead of a regex match index,
	///		one of
	///		three identical copies <c>MASTER-138</c> names. The parsed document carries the real
	///		line, so no counting is needed and no offset has to survive a comment-blanking pass.
	/// </remarks>
	private static int LineNumberOf(XElement element)
	{
		var lineInfo = (IXmlLineInfo)element;

		return lineInfo.HasLineInfo()
			? lineInfo.LineNumber
			: 0;
	}

	/// <summary>
	///		Renders one offender, naming the codepoint the reader has to go and look up.
	/// </summary>
	private static string DescribeOffence(IconControl control)
	{
		var declared = control.Element.Attribute(DescriptionAttributeName);

		return $"{control.RelativePath}:{control.LineNumber} -> <{control.ElementName} "
			+ $"Text=\"U+{char.ConvertToUtf32(control.Glyph, 0):X4}\"> has "
			+ (declared is not null
				? "a blank SemanticProperties.Description"
				: "no SemanticProperties.Description")
			+ " and is not excluded from the accessibility tree";
	}

	/// <summary>
	///		A control is icon-only when its whole text is one private-use or symbol codepoint.
	///		Letters and digits are excluded, so the toolbar's shortcut letters and the zoom
	///		readout are not swept up; a run of two or more characters is excluded too, because
	///		something like "&#x2190; Back" carries a word a screen reader can read.
	/// </summary>
	/// <remarks>
	///		"One codepoint" is measured in Runes, not <c>char</c>s. A supplementary-plane glyph is
	///		two <c>char</c>s and one Rune, and the <c>string.Length</c> test this replaces
	///		rejected it outright — which let an undescribed supplementary-plane icon control pass
	///		the ratchet. <c>MASTER-040</c>.
	/// </remarks>
	private static bool IsIconOnly(string text)
	{
		if (!ThemeSource.IsSingleRune(text))
		{
			return false;
		}

		return Rune.GetUnicodeCategory(
			Rune.GetRuneAt(
				text,
				0)) switch
		{
			UnicodeCategory.PrivateUse => true,
			UnicodeCategory.OtherSymbol => true,
			UnicodeCategory.MathSymbol => true,
			UnicodeCategory.ModifierSymbol => true,
			UnicodeCategory.CurrencySymbol => true,
			_ => false,
		};
	}

	/// <summary>
	///		Parses a synthetic XAML fragment written as one string per line, so a fact can name a
	///		declaration the real tree does not contain and still get real line info.
	/// </summary>
	private static XDocument SyntheticDocument(params string[] lines) =>
		XDocument.Parse(
			string.Join(
				'\n',
				lines),
			LoadOptions.SetLineInfo);
}
