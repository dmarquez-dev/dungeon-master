using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using FluentAssertions;
using FluentAssertions.Execution;

namespace DungeonMaster.Tests.App.Theme;

/// <summary>
///		Ratchet guarding the migration of application source onto the theme dictionaries.
///		Every file that still carries a raw colour, font size or font family is named in
///		<see cref="PendingMigration"/>; removing an entry makes these facts fail until the
///		file is migrated.
///		<para>
///		THE TWO HALVES OF EVERY FACT READ DIFFERENT MODELS, AND THAT SPLIT IS THE BOUNDARY.
///		The <c>*.xaml</c> half reads <c>ThemeSource.AppDocuments()</c> — the parsed
///		<c>XDocument</c> tree — because contracts/theme-source.md section 3.5 measured the two
///		readers against ten synthetic cases and found the object model loses nothing a raw-text
///		scan catches, and catches one case the raw pattern misses (a hex literal split by a
///		character entity). The <c>*.cs</c> half stays a raw-text scan because
///		<c>XDocument</c> cannot parse C#. That is definitional, not a preference.
///		</para>
/// </summary>
public class ThemeHygieneTests
{
	/// <summary>
	///		The XAML property whose bare numeric value this ratchet forbids.
	///		<para>
	///		Matched as an ATTRIBUTE NAME on the parsed tree rather than as the text
	///		<c>FontSize="</c>. The raw-text form needed a <c>(?&lt;![A-Za-z])</c> lookbehind to
	///		stop it matching DmDivider's <c>IconFontSize</c> bindable property; an exact name
	///		comparison cannot make that mistake in the first place.
	///		</para>
	///		<para>
	///		It is matched in two forms: as an attribute name, and as the <c>Property</c> of a <c>Setter</c> whose <c>Value</c> is bare.
	///		Both forms use the same <see cref="IsProperty"/> comparison, so <c>IconFontSize</c> is outside the reach of either.
	///		</para>
	/// </summary>
	private const string FontSizeProperty = "FontSize";

	/// <summary>
	///		The XAML property whose bare font-family name this ratchet forbids. See
	///		<see cref="FontSizeProperty"/> for why this is a name rather than a pattern.
	///		It is matched in the same two forms, as an attribute name and as a <c>Setter</c>'s <c>Property</c>.
	/// </summary>
	private const string FontFamilyProperty = "FontFamily";

	/// <summary>
	///		The local name of the style element whose <c>Property</c> and <c>Value</c> attributes set a property indirectly.
	/// </summary>
	private const string SetterElement = "Setter";

	/// <summary>
	///		The <c>Setter</c> attribute that names the property being set.
	/// </summary>
	private const string SetterPropertyAttribute = "Property";

	/// <summary>
	///		The <c>Setter</c> attribute that carries the value being set.
	/// </summary>
	private const string SetterValueAttribute = "Value";

	// The C# forms of the two typography literals. (?<![A-Za-z]) keeps IconFontSize out, and
	// =(?!>) keeps expression-bodied members out. Both patterns match an assignment only —
	// "FontSize = 13", "canvas.FontSize = 9f", "FontFamily = \"EBGaramondRegular\"" — so
	// "nameof(FontSize)" and a bindable property's positional default are outside their reach.
	// That exemption is deliberate and named in the PendingMigration comment below.
	//
	// These stay regexes over raw text, and they are the half that never migrates: there is no
	// object model for C# here to migrate them onto.
	private static readonly Regex CSharpBareFontSizePattern =
		new(
			@"(?<![A-Za-z])FontSize\s*=(?!>)\s*[0-9]",
			RegexOptions.Compiled);

	private static readonly Regex CSharpBareFontFamilyPattern =
		new(
			@"(?<![A-Za-z])FontFamily\s*=(?!>)\s*""",
			RegexOptions.Compiled);

	// A markup extension lives in an attribute VALUE, so this pattern survives the migration
	// unchanged — it is simply applied to the values the parser yields rather than to the file's
	// bytes. Applied to the C# text as well, as it was before, so that a legacy key spelled into
	// a code-behind string is still reported.
	private static readonly Regex LegacyThemeKeyPattern =
		new(
			@"\{StaticResource\s+DM[A-Za-z0-9]*\s*\}",
			RegexOptions.Compiled);

	/// <summary>
	///		Files not yet migrated onto the theme system. THE MIGRATION IS COMPLETE and this set
	///		is empty: every colour, font size, font family and theme key in the application
	///		resolves from Resources/Theme. It stays here as the ratchet's escape hatch — if a new
	///		file has to carry a literal for a while, name it here and the facts below go green
	///		again, but an entry that stops offending is itself a failure, so nothing can be
	///		parked and forgotten.
	///
	///		TWO FILES SURVIVE BY DESIGN, and neither is reachable by these patterns:
	///
	///		  Controls\DmDivider\DmDivider.xaml.cs        IconFontSize default 16.0
	///		  Controls\DmInlineEditor\DmInlineEditor.xaml.cs  FontSize 18.0, FontFamily "EBGaramondRegular"
	///
	///		A BindableProperty default is evaluated in a static field initialiser, which runs at
	///		type-initialisation time and can precede Application.Current.Resources — so it cannot
	///		read DmTheme without throwing. They are declared positionally
	///		(<c>typeof(DmDivider), 16.0</c>), not as an assignment, so the two C# patterns above
	///		never see them. BindablePropertyDefaultsMatchTokensTests compares each against the
	///		token it mirrors instead, which is the guard that actually fits their shape.
	/// </summary>
	private static readonly HashSet<string> PendingMigration = new(StringComparer.OrdinalIgnoreCase);

	/// <summary>
	///		Asserts that the markup half of every hygiene fact reaches application sources at all.
	///		<para>
	///		This assertion used to live inside <c>Offenders(...)</c>, which meant a scan that had
	///		stopped reaching anything was reported against whichever fact happened to call the
	///		helper first. It is a named fact of its own so the failure names its own cause;
	///		contracts/anti-vacuity.md section 2 is the rule.
	///		</para>
	/// </summary>
	[Fact]
	public void TheHygieneXamlScanReachesAtLeastOneApplicationSource()
	{
		ScannedXamlDocuments()
			.Should()
			.NotBeEmpty("the '*.xaml' scan must find application sources or every hygiene fact "
				+ "built on it is vacuous");
	}

	/// <summary>
	///		Asserts that the code-behind half of every hygiene fact reaches application sources at
	///		all. See <see cref="TheHygieneXamlScanReachesAtLeastOneApplicationSource"/>.
	/// </summary>
	[Fact]
	public void TheHygieneCSharpScanReachesAtLeastOneApplicationSource()
	{
		ScannedCSharpFiles()
			.Should()
			.NotBeEmpty("the '*.cs' scan must find application sources or every hygiene fact "
				+ "built on it is vacuous");
	}

	/// <summary>
	///		Asserts that colour literals appear only in the theme dictionaries.
	/// </summary>
	[Fact]
	public void NoRawColourLiteralsOutsideTheThemeDictionaries()
	{
		XamlTextOffenders(ThemeSource.HexLiteralPattern)
			.Concat(CSharpOffenders(ThemeSource.HexLiteralPattern))
			.Should()
			.BeEmpty();
	}

	/// <summary>
	///		Asserts that numeric font sizes appear only in the theme dictionaries, in markup and
	///		in code-behind alike.
	///		In markup it reads both the attribute form and the <c>Setter</c> form.
	/// </summary>
	[Fact]
	public void NoBareFontSizesOutsideTheThemeDictionaries()
	{
		XamlPropertyOffenders(
				ScannedXamlDocuments(),
				FontSizeProperty,
				IsBareFontSize)
			.Concat(CSharpOffenders(CSharpBareFontSizePattern))
			.Should()
			.BeEmpty();
	}

	/// <summary>
	///		Asserts that font family names appear only in the theme dictionaries, in markup and
	///		in code-behind alike.
	///		In markup it reads both the attribute form and the <c>Setter</c> form.
	/// </summary>
	[Fact]
	public void NoBareFontFamiliesOutsideTheThemeDictionaries()
	{
		XamlPropertyOffenders(
				ScannedXamlDocuments(),
				FontFamilyProperty,
				IsBareFontFamily)
			.Concat(CSharpOffenders(CSharpBareFontFamilyPattern))
			.Should()
			.BeEmpty();
	}

	/// <summary>
	///		Asserts that the probe the two typography facts call reports a bare FontSize Setter and a bare FontFamily Setter, each with its file, line and text.
	///		The document is synthetic because no application file carries a bare Setter, so nothing else shows that the probe reaches the Setter form.
	/// </summary>
	[Fact]
	public void BareFontSizeAndFontFamilySettersAreReportedByTheTypographyProbe()
	{
		// Arrange
		var document = SyntheticDocument(
			"<Setter Property=\"FontSize\" Value=\"14\" />",
			"<Setter Property=\"FontFamily\" Value=\"EBGaramondRegular\" />");
		var relative = NativeRelativePath(document);

		// Act
		var fontSizes = XamlPropertyOffenders(
				[document],
				FontSizeProperty,
				IsBareFontSize)
			.ToList();

		var fontFamilies = XamlPropertyOffenders(
				[document],
				FontFamilyProperty,
				IsBareFontFamily)
			.ToList();

		// Assert
		using (new AssertionScope())
		{
			fontSizes.Should().Equal($"{relative}:4 -> Setter Property=\"FontSize\" Value=\"14\"");
			fontFamilies.Should().Equal($"{relative}:5 -> Setter Property=\"FontFamily\" Value=\"EBGaramondRegular\"");
		}
	}

	/// <summary>
	///		Asserts that the probe the FontSize fact calls reports neither a Setter whose Value is a token reference nor a Setter whose Property is IconFontSize.
	/// </summary>
	[Fact]
	public void TokenValuedAndIconFontSizeSettersAreNotReportedByTheTypographyProbe()
	{
		// Arrange
		var document = SyntheticDocument(
			"<Setter Property=\"FontSize\" Value=\"{StaticResource DmFontSizeBody}\" />",
			"<Setter Property=\"IconFontSize\" Value=\"16\" />");

		// Act
		var offenders = XamlPropertyOffenders(
				[document],
				FontSizeProperty,
				IsBareFontSize)
			.ToList();

		// Assert
		offenders.Should().BeEmpty();
	}

	/// <summary>
	///		Asserts that the allow-list self-check counts a bare Setter as an offence, so an entry whose only offence is a bare Setter is not reported stale.
	///		The token-valued row is the near miss that keeps the two bare rows from passing on something else in the synthetic document.
	/// </summary>
	/// <param name="setter">
	///		The one Setter the synthetic document carries.
	/// </param>
	/// <param name="expected">
	///		Whether the document should still count as offending.
	/// </param>
	[Theory]
	[InlineData("<Setter Property=\"FontSize\" Value=\"14\" />", true)]
	[InlineData("<Setter Property=\"FontFamily\" Value=\"EBGaramondRegular\" />", true)]
	[InlineData("<Setter Property=\"FontSize\" Value=\"{StaticResource DmFontSizeBody}\" />", false)]
	public void ADocumentWhoseOnlyTypographyIsASetterStillOffendsOnlyWhenTheSetterValueIsBare(
		string setter,
		bool expected)
	{
		// Arrange
		var document = SyntheticDocument(setter);

		// Act
		var offends = StillOffends(document);

		// Assert
		offends.Should().Be(expected);
	}

	/// <summary>
	///		Asserts that no migrated file still references a legacy <c>DM</c>-prefixed key from
	///		<c>Resources/Styles/Colors.xaml</c>, which Task 10 deleted. Migrating a file means
	///		moving it onto the <c>Dm</c>-prefixed tokens in <c>Resources/Theme/Tokens.xaml</c>,
	///		not merely off raw literals.
	/// </summary>
	[Fact]
	public void NoLegacyThemeKeyReferencesOutsideThePendingMigrationSet()
	{
		XamlTextOffenders(LegacyThemeKeyPattern)
			.Should()
			.BeEmpty();
	}

	/// <summary>
	///		Asserts that every allow-listed path names a file that exists and still carries at
	///		least one of the literals the three facts above forbid. Without this the ratchet is
	///		not self-checking as it shrinks: a stale or mistyped entry exempts nothing, and a
	///		migration task can "remove its entry" and see green because the entry never bit.
	///		<para>
	///		THIS FACT IS VACUOUS TODAY AND THAT IS THE POINT. <see cref="PendingMigration"/> is
	///		empty because the migration finished, so there is nothing to check and the loop below
	///		runs zero times. It is NOT dead code: it is a self-arming ratchet that starts checking
	///		the moment somebody adds the first new entry, which is exactly when it is needed and
	///		exactly when nobody would think to write it. Deleting it because "it asserts nothing"
	///		removes the guard from every future allow-list entry.
	///		</para>
	///		<para>
	///		THE DISPLAY NAME IS THE REMEDY, NOT THIS COMMENT. A reader of a test REPORT sees only
	///		a green line, and a green line from a loop that ran zero times looks exactly like a
	///		green line from a scan that has silently stopped reaching anything. The display name
	///		states the condition in the report itself, so the two can be told apart without
	///		opening this file. It must NOT acquire an Assert.NotEmpty on
	///		<see cref="PendingMigration"/>: that would turn a ratchet which is correct while empty
	///		into a test that cannot pass.
	///		</para>
	/// </summary>
	[Fact(DisplayName = "Every allow-listed file exists and still offends "
		+ "(intentionally inert while PendingMigration is empty)")]
	public void EveryAllowListedFileExistsAndStillOffends()
	{
		var stale = PendingMigration
			.Where(relative =>
				!StillOffends(relative))
			.Select(relative =>
				File.Exists(
					Path.Combine(
						RepoPaths.AppRoot,
						relative))
					? $"{relative} -> no longer offends; remove it from PendingMigration"
					: $"{relative} -> file does not exist; the entry exempts nothing")
			.OrderBy(
				entry =>
					entry,
				StringComparer.Ordinal)
			.ToList();

		stale.Should().BeEmpty();
	}

	private static bool StillOffends(string relative)
	{
		var path = Path.Combine(
			RepoPaths.AppRoot,
			relative);

		if (!File.Exists(path))
		{
			return false;
		}

		if (relative.EndsWith(
			".xaml",
			StringComparison.OrdinalIgnoreCase))
		{
			var document = ThemeSource
				.AppDocuments()
				.FirstOrDefault(candidate =>
					string.Equals(
						NativeRelativePath(candidate),
						relative,
						StringComparison.OrdinalIgnoreCase));

			return document is not null
				&& StillOffends(document);
		}

		var text = File.ReadAllText(path);

		return ThemeSource.HexLiteralPattern.IsMatch(text)
			|| CSharpBareFontSizePattern.IsMatch(text)
			|| CSharpBareFontFamilyPattern.IsMatch(text)
			|| LegacyThemeKeyPattern.IsMatch(text);
	}

	private static bool StillOffends(ThemeDocument document)
	{
		var offends = ScannedText(document.Document)
			.Any(scanned =>
				ThemeSource.HexLiteralPattern.IsMatch(scanned.Text)
				|| LegacyThemeKeyPattern.IsMatch(scanned.Text));

		return offends
			|| document
				.Document
				.Descendants()
				.Any(element =>
					PropertyOffences(
							element,
							FontSizeProperty,
							IsBareFontSize)
						.Any()
					|| PropertyOffences(
							element,
							FontFamilyProperty,
							IsBareFontFamily)
						.Any());
	}

	/// <summary>
	///		The parsed markup this class examines: every application document except the theme
	///		dictionaries themselves, which are where the literals are DECLARED, and except
	///		anything the allow list exempts.
	/// </summary>
	private static IReadOnlyList<ThemeDocument> ScannedXamlDocuments()
	{
		var separator = Path.DirectorySeparatorChar;

		return ThemeSource
			.AppDocuments()
			.Where(document =>
				!NativeRelativePath(document)
					.StartsWith(
						$"Resources{separator}Theme{separator}",
						StringComparison.OrdinalIgnoreCase))
			.Where(document =>
				!PendingMigration.Contains(NativeRelativePath(document)))
			.ToList();
	}

	/// <summary>
	///		The code-behind this class examines, as paths relative to the application project
	///		root. There is no object model to read these with; see the class remarks.
	/// </summary>
	private static IReadOnlyList<string> ScannedCSharpFiles()
	{
		var separator = Path.DirectorySeparatorChar;

		return RepoPaths
			.AppSourceFiles("*.cs")
			.Select(path =>
				Path.GetRelativePath(
					RepoPaths.AppRoot,
					path))
			.Where(relative =>
				!relative.StartsWith(
					$"Resources{separator}Theme{separator}",
					StringComparison.OrdinalIgnoreCase))
			.Where(relative =>
				!PendingMigration.Contains(relative))
			.ToList();
	}

	/// <summary>
	///		Applies a pattern to the TEXT the parser yields — attribute values, element text,
	///		comments and processing-instruction data — rather than to the file's bytes.
	/// </summary>
	/// <param name="pattern">
	///		The pattern to apply.
	/// </param>
	private static IEnumerable<string> XamlTextOffenders(Regex pattern)
	{
		foreach (var document in ScannedXamlDocuments())
		{
			var relative = NativeRelativePath(document);

			foreach (var scanned in ScannedText(document.Document))
			{
				foreach (Match match in pattern.Matches(scanned.Text))
				{
					// Reported with the line number and the matched text: the bare relative path
					// alone left a 700-line XAML file to be searched by hand for what tripped the
					// pattern.
					yield return $"{relative}:{LineOfMatch(scanned, match)} -> {match.Value}";
				}
			}
		}
	}

	/// <summary>
	///		Reports, for every document given, each place an element sets the named property to a value the predicate calls bare, with the file, the line and the offending text.
	///		This is the one probe the typography facts and their synthetic controls share, so a control cannot stay green while the facts' reach shrinks.
	///		See <see cref="PropertyOffences"/> for the two forms it reads.
	/// </summary>
	/// <param name="documents">
	///		The parsed documents to read.
	/// </param>
	/// <param name="property">
	///		The property name, compared after any attached-property prefix.
	/// </param>
	/// <param name="isBare">
	///		Whether a value is a literal rather than a token reference.
	/// </param>
	private static IEnumerable<string> XamlPropertyOffenders(
		IEnumerable<ThemeDocument> documents,
		string property,
		Func<string, bool> isBare)
	{
		foreach (var document in documents)
		{
			var relative = NativeRelativePath(document);

			foreach (var element in document.Document.Descendants())
			{
				var offences = PropertyOffences(
					element,
					property,
					isBare);

				foreach (var offence in offences)
				{
					yield return $"{relative}:{offence.Line} -> {offence.Text}";
				}
			}
		}
	}

	/// <summary>
	///		Every place one element sets the named property to a value the predicate calls bare, with the line it is on and the text to report.
	///		Two forms are read: an attribute whose name is the property, and a <c>Setter</c> whose <c>Property</c> names the property and whose <c>Value</c> is bare.
	///		Both forms use the <see cref="IsProperty"/> comparison, which is exact where a text pattern was approximate, so IconFontSize and a nameof(FontSize) in prose are outside its reach without a lookbehind.
	///		The probe and the allow-list self-check both call this, so the two cannot disagree about what offends.
	/// </summary>
	/// <param name="element">
	///		The element to examine.
	/// </param>
	/// <param name="property">
	///		The property name, compared after any attached-property prefix.
	/// </param>
	/// <param name="isBare">
	///		Whether a value is a literal rather than a token reference.
	/// </param>
	private static IEnumerable<(int Line, string Text)> PropertyOffences(
		XElement element,
		string property,
		Func<string, bool> isBare)
	{
		foreach (var attribute in element.Attributes())
		{
			if (IsProperty(
					attribute.Name.LocalName,
					property)
				&& isBare(attribute.Value))
			{
				yield return (
					LineOf(attribute),
					$"{attribute.Name.LocalName}=\"{attribute.Value}\"");
			}
		}

		// Compared on the local name: the element sits in the MAUI default namespace, so its full
		// XName is never the bare string "Setter".
		if (element.Name.LocalName != SetterElement)
		{
			yield break;
		}

		var setterProperty = element.Attribute(SetterPropertyAttribute);
		var setterValue = element.Attribute(SetterValueAttribute);

		if (setterProperty is null
			|| setterValue is null
			|| !IsProperty(
				setterProperty.Value,
				property)
			|| !isBare(setterValue.Value))
		{
			yield break;
		}

		yield return (
			LineOf(element),
			$"{SetterElement} {SetterPropertyAttribute}=\"{setterProperty.Value}\" "
				+ $"{SetterValueAttribute}=\"{setterValue.Value}\"");
	}

	/// <summary>
	///		Builds an in-memory application document whose one implicit style carries the given Setters, one per line from line 4.
	/// </summary>
	/// <param name="setters">
	///		The Setter elements, as XAML text.
	/// </param>
	private static ThemeDocument SyntheticDocument(params string[] setters)
	{
		var lines = new List<string>
		{
			"<ContentPage xmlns=\"http://schemas.microsoft.com/dotnet/2021/maui\">",
			"\t<ContentPage.Resources>",
			"\t\t<Style TargetType=\"Label\">",
		};

		lines.AddRange(setters);
		lines.Add("\t\t</Style>");
		lines.Add("\t</ContentPage.Resources>");
		lines.Add("</ContentPage>");

		return new ThemeDocument(
			"Synthetic.xaml",
			"Views/Synthetic.xaml",
			XDocument.Parse(
				string.Join(
					'\n',
					lines),
				LoadOptions.SetLineInfo));
	}

	private static IEnumerable<string> CSharpOffenders(Regex pattern)
	{
		foreach (var relative in ScannedCSharpFiles())
		{
			var text = File.ReadAllText(
				Path.Combine(
					RepoPaths.AppRoot,
					relative));

			foreach (Match match in pattern.Matches(text))
			{
				var line = ThemeSource.LineNumber(
					text,
					match.Index);

				yield return $"{relative}:{line} -> {match.Value}";
			}
		}
	}

	/// <summary>
	///		Every piece of text the parser exposes, with the line it starts on. This is the walk
	///		contracts/theme-source.md section 3.5 measured against the raw-text scan.
	/// </summary>
	/// <param name="document">
	///		The parsed document to read.
	/// </param>
	private static IEnumerable<(string Text, int Line)> ScannedText(XDocument document)
	{
		foreach (var node in document.DescendantNodes())
		{
			if (node is XElement element)
			{
				foreach (var attribute in element.Attributes())
				{
					yield return (attribute.Value, LineOf(attribute));
				}
			}
			else if (node is XText text)
			{
				yield return (text.Value, LineOf(text));
			}
			else if (node is XComment comment)
			{
				yield return (comment.Value, LineOf(comment));
			}
			else if (node is XProcessingInstruction instruction)
			{
				yield return (instruction.Data, LineOf(instruction));
			}
		}
	}

	/// <summary>
	///		Whether a property name, either an attribute's local name or a <c>Setter</c>'s <c>Property</c> value, IS the named property, ignoring any <c>Type.</c> prefix.
	///		<c>Label.FontSize</c> is a FontSize; <c>IconFontSize</c> is not.
	/// </summary>
	/// <param name="name">
	///		The property name to test.
	/// </param>
	/// <param name="property">
	///		The property name to test it against.
	/// </param>
	private static bool IsProperty(
		string name,
		string property)
	{
		var dot = name.LastIndexOf('.');

		return string.Equals(
			dot < 0
				? name
				: name[(dot + 1)..],
			property,
			StringComparison.Ordinal);
	}

	private static bool IsBareFontSize(string value)
	{
		return value.Length > 0
			&& char.IsAsciiDigit(value[0]);
	}

	private static bool IsBareFontFamily(string value)
	{
		return value.Length > 0
			&& char.IsAsciiLetter(value[0]);
	}

	private static string NativeRelativePath(ThemeDocument document)
	{
		return document
			.RelativePath
			.Replace(
				'/',
				Path.DirectorySeparatorChar);
	}

	private static int LineOf(IXmlLineInfo info)
	{
		return info.HasLineInfo()
			? info.LineNumber
			: 0;
	}

	/// <summary>
	///		The line a match falls on, counted from the line the containing node starts on. A
	///		comment or a text node can span lines, so the node's own line is a floor rather than
	///		the answer.
	/// </summary>
	/// <param name="scanned">
	///		The scanned text and the line it starts on.
	/// </param>
	/// <param name="match">
	///		The match within that text.
	/// </param>
	private static int LineOfMatch(
		(string Text, int Line) scanned,
		Match match)
	{
		return scanned.Line
			+ scanned
				.Text[..match.Index]
				.Count(character =>
					character == '\n');
	}
}
