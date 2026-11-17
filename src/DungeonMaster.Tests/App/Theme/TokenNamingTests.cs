using System.Text.RegularExpressions;
using FluentAssertions;

namespace DungeonMaster.Tests.App.Theme;

/// <summary>
///		Guards the token naming rule settled on 2026-08-07 and written at the top of Tokens.xaml.
///		<para>
///		THE RULE, IN ONE SENTENCE. A colour token is either STRUCTURAL — a shared surface family,
///		named category-first as <c>Dm(Surface|Border|Text)&lt;Family&gt;&lt;Variant&gt;</c> — or
///		SEMANTIC — a named role or drawn part, named role-first as
///		<c>Dm&lt;Role&gt;&lt;Qualifier&gt;</c>. It is deliberately NOT "category-first everywhere":
///		that mechanical reading produces <c>DmTextConfirm</c> and <c>DmSurfaceScrollbarThumb</c>,
///		which are worse than the names they replace, and the fact below is what stops a later pass
///		from applying it.
///		</para>
///		<para>
///		WHAT THIS FACT CAN AND CANNOT DO, STATED PLAINLY. It checks VOCABULARY and ORDER, which
///		are mechanical. It cannot check that a name is TRUE of the colour — that
///		<c>DmMapAreaStroke</c> really is a stroke — because that lives in the drawing code, not in
///		the name. So <c>DmMapFillGrid</c> would pass: every word is declared and the order is
///		legal. That is the honest limit of a name-shaped guard, and it is not papered over here.
///		What it does catch is the two failures that actually happened: a token invented with a
///		word nobody registered, and a token written in the other family's order.
///		</para>
///		<para>
///		The vocabularies below are CLOSED. A new family, role or qualifier must be added here
///		deliberately, and adding it is the moment to ask which family the token belongs to.
///		</para>
/// </summary>
public class TokenNamingTests
{
	/// <summary>
	///		The dictionary the colour-token scan reads.
	/// </summary>
	private const string TokensDictionaryName = "Tokens.xaml";

	/// <summary>
	///		Matches a keyed colour token. <c>SolidColorBrush</c> is included because a brush alias
	///		is a colour token by another element name and obeys the same rule; the <c>Brush</c>
	///		suffix is stripped before the name is parsed.
	/// </summary>
	private static readonly Regex ColourTokenPattern =
		new(
			@"<(?:Color|SolidColorBrush)[^>]*?x:Key=""(?<key>Dm[A-Za-z0-9]*)""",
			RegexOptions.Compiled);

	private static readonly Regex StyleKeyPattern =
		new(
			@"<Style[^>]*?x:Key=""(?<key>Dm[A-Za-z0-9]*)""",
			RegexOptions.Compiled);

	private static readonly Regex PascalWordPattern =
		new(
			@"[A-Z][a-z0-9]*",
			RegexOptions.Compiled);

	/// <summary>
	///		The three words that may lead a STRUCTURAL token, and only a structural token.
	/// </summary>
	private static readonly string[] StructuralHeads =
	[
		"Surface", "Border", "Text",
	];

	/// <summary>
	///		The surface families a structural token may qualify. Longest match wins, so
	///		"TextSelection" is read as one family rather than as the head "Text" twice.
	/// </summary>
	private static readonly string[] SurfaceFamilies =
	[
		"Tavern", "Parchment", "Popup", "ToolPanel", "ToolChip",
		"ComboItem", "Stepper", "Notebook", "TextSelection",
	];

	/// <summary>
	///		The roles a SEMANTIC token may lead with. Longest match wins.
	/// </summary>
	private static readonly string[] SemanticRoles =
	[
		"AccentGold", "Gold", "Confirm", "Destructive", "Neutral", "Save", "Discard",
		"Ribbon", "Banner", "DragPlaceholder", "AreaSelected", "EditorGlow", "Map",
		"Scrollbar", "PopupButton", "Hairline", "SavedStamp",

		// The authored-content indicator, named for the scroll the notebook affordance draws.
		// Registered as a role rather than left to parse as "Scroll" + "Gold" because Gold is
		// itself a role and cannot also be a qualifier: the pair is one name. Longest match is
		// what keeps it apart from Scrollbar — neither key is a prefix of the other.
		"ScrollGold",
	];

	/// <summary>
	///		Every word that may follow a family or a role: categories, states, weights and the
	///		names of the parts a drawn element is made of.
	/// </summary>
	private static readonly string[] Qualifiers =
	[
		// Categories — the words clause (b) of the rule is about.
		"Surface", "Border", "Stroke", "Fill", "Tint", "Text",
		// States.
		"Hover", "Pressed", "Active", "Disabled", "Selected",
		// Weights and shades.
		"Light", "Dark", "Dim", "Deep", "Muted", "Strong", "Solid", "Raised",
		// Parts of a drawn element.
		"Read", "Seal", "Edge", "Center", "Grid", "Area", "Vertex", "Object",
		"Placeholder", "Thumb",
		// A role may qualify a chrome element: DmPopupButtonDestructiveHover.
		"Destructive",
	];

	/// <summary>
	///		The words that make a token's category explicit. A semantic token carrying none of
	///		them must be registered in <see cref="CategorylessByDesign"/>.
	/// </summary>
	private static readonly string[] CategoryWords =
	[
		"Surface", "Border", "Stroke", "Fill", "Tint", "Text",
	];

	/// <summary>
	///		Structural tokens whose word after the category is not a surface family, with the
	///		reason each is allowed. Exactly one today.
	/// </summary>
	private static readonly Dictionary<string, string> StructuralQualifierExceptions =
		new(StringComparer.Ordinal)
		{
			["DmTextPlaceholder"] =
				"Placeholder is a text ROLE, not a surface family: this is the ink of unentered "
				+ "text on any surface. Category-first is still right — it is a Text colour "
				+ "first and a placeholder second — so it is not moved to the semantic family.",
		};

	/// <summary>
	///		Semantic tokens that deliberately carry no category word, with the reason each is
	///		allowed. Two reasons only, and they are different reasons:
	///		a ROLE HUE may paint a fill, a label or a stroke as the site requires, and a
	///		SINGLE-PART ELEMENT has only one thing to paint.
	/// </summary>
	private static readonly Dictionary<string, string> CategorylessByDesign =
		new(StringComparer.Ordinal)
		{
			["DmAccentGold"] = "Role hue. The accent itself; painted as fill, stroke and ink.",
			["DmAccentGoldLight"] = "Role hue, lighter shade of the accent ramp.",
			["DmAccentGoldDark"] = "Role hue, darker shade of the accent ramp.",
			["DmScrollGoldLight"] =
				"Role hue. The authored-content indicator: it paints a notebook GLYPH on one "
				+ "control and a floor's NAME LABEL on another — two categories, one hue, so "
				+ "naming either would make the other a lie. Equal in value to DmAccentGoldLight "
				+ "and deliberately a separate key, so the indicator can stop tracking the "
				+ "accent by one edit in Tokens.xaml.",
			["DmConfirm"] = "Role hue. DmConfirmText is its categorised derivative.",
			["DmDestructive"] =
				"Role hue. Paints a button fill, the notebook's unsaved label and an icon "
				+ "button's hover ink — three categories, one hue.",
			["DmDestructiveOnPopup"] =
				"Role hue, qualified by the surface it is rendered on rather than by a category, "
				+ "exactly as DmDestructive is. Its two alpha derivatives are the "
				+ "DmPopupButtonDestructive* washes.",
			["DmRibbonRead"] = "Single-part element: the READ ribbon has one colour, its body.",
			["DmRibbonReadDim"] = "The same body colour in its unselected state.",
			["DmRibbonSeal"] = "Single-part element: the ribbon's wax seal.",
			["DmBannerEdge"] = "A gradient stop. Its position IS its identity; it paints nothing else.",
			["DmBannerCenter"] = "The other gradient stop.",
			["DmMapGrid"] = "Single-part element: a grid is lines and cannot be filled.",
			["DmMapVertex"] =
				"One colour serves the vertex handle's fill, its stroke and its index label. "
				+ "Splitting it would create three tokens equal by construction.",
			["DmMapObjectPlaceholder"] =
				"One colour serves the '?' glyph, its outline and the dashed ghost-placement "
				+ "outline, for the same reason as DmMapVertex.",
			["DmScrollbarThumb"] = "Single-part element: a thumb's only colour is its fill.",
			["DmScrollbarThumbDisabled"] = "The same fill in its disabled state.",
			["DmPopupButtonHover"] = "A popup button's state colour paints one thing, its background.",
			["DmPopupButtonPressed"] = "As DmPopupButtonHover.",
			["DmPopupButtonDestructiveHover"] = "As DmPopupButtonHover, on the delete button.",
			["DmPopupButtonDestructivePressed"] = "As DmPopupButtonHover, on the delete button.",
			["DmHairline"] = "Single-part element: a 1px rule is its colour and nothing else.",
			["DmHairlineStrong"] = "The same rule, heavier wash.",
			["DmHairlineSolid"] =
				"The same rule, opaque — it has to read on both the parchment tool panel and "
				+ "the near-black popup, which no alpha wash of one base can do.",
		};

	/// <summary>
	///		Asserts that the file scan this class is built on reaches at least one dictionary.
	///		<para>
	///		Until T3 landed, the only thing standing between this class and passing while examining
	///		nothing was an assertion inside <c>ThemeSource.ThemeFiles()</c> — an Arrange helper,
	///		which reports a broken scan against whichever fact called it first and names the wrong
	///		test. That assertion is gone, and contracts/anti-vacuity.md section 2 says the
	///		protection MOVES to a named fact in the consuming file rather than disappearing. This
	///		is that fact.
	///		</para>
	///		<para>
	///		It asserts on <see cref="ScannedThemeFiles"/>, this class's own scan source, rather than
	///		on <c>ThemeSource.ThemeFiles()</c> directly, so that redirecting the scan is a one-line
	///		edit inside this file and the guard can be demonstrated red without touching a file this
	///		task does not own.
	///		</para>
	/// </summary>
	[Fact]
	public void TheTokenNamingScanReachesAtLeastOneThemeDictionary()
	{
		ScannedThemeFiles()
			.Should()
			.NotBeEmpty(
				"every fact in this class reads the theme dictionaries, and each one of them "
				+ "examines nothing if this scan reaches nothing");
	}

	/// <summary>
	///		Asserts that every colour token in Tokens.xaml parses under exactly one of the two
	///		families, using only declared vocabulary.
	///		<para>
	///		This is the fact that fires when a token drifts into the other family's order. It was
	///		proved by construction on 2026-08-07 by renaming <c>DmSurfaceTavern</c> to
	///		<c>DmTavernSurface</c> in Tokens.xaml, reading the file back off disk to confirm the
	///		edit had actually landed — a proof run earlier in this project passed because a CRLF
	///		mismatch meant the violation was never written — and watching this fact report it.
	///		</para>
	/// </summary>
	[Fact]
	public void EveryColourTokenObeysTheNamingRule()
	{
		var tokens = ColourTokens();

		tokens
			.Should()
			.NotBeEmpty("the colour-token scan must find tokens or this fact is vacuous");

		var offenders = tokens
			.Select(key =>
				new
				{
					Key = key,
					Fault = Fault(key),
				})
			.Where(entry =>
				entry.Fault is not null)
			.Select(entry =>
				$"{entry.Key}: {entry.Fault}")
			.ToList();

		offenders
			.Should()
			.BeEmpty(
				"the naming rule is written at the top of Tokens.xaml and every colour token "
				+ "must obey it or be registered here with a reason");
	}

	/// <summary>
	///		Asserts that both exception registers are exactly as long as they need to be — every
	///		entry names a token that still exists AND still offends.
	///		<para>
	///		Both halves are checked, because a register that keeps entries for tokens which no
	///		longer offend is how an exemption silently becomes a licence: the next token to be
	///		given that name inherits an exemption nobody granted it.
	///		</para>
	/// </summary>
	[Fact]
	public void EveryRegisteredExceptionStillExistsAndStillOffends()
	{
		var tokens = ColourTokens().ToHashSet(StringComparer.Ordinal);

		var complaints = new List<string>();

		foreach (var entry in StructuralQualifierExceptions)
		{
			if (!tokens.Contains(entry.Key))
			{
				complaints.Add($"{entry.Key} is registered as a structural-qualifier exception but no longer exists");
				continue;
			}

			var words = Words(entry.Key);

			if (words.Count > 1
				&& LongestMatch(
					words,
					1,
					SurfaceFamilies) is not null)
			{
				complaints.Add(
					$"{entry.Key} now qualifies a declared surface family and no longer needs "
					+ "its exception; delete the entry");
			}
		}

		foreach (var entry in CategorylessByDesign)
		{
			if (!tokens.Contains(entry.Key))
			{
				complaints.Add($"{entry.Key} is registered as categoryless by design but no longer exists");
				continue;
			}

			if (Words(entry.Key).Any(word =>
					CategoryWords.Contains(
						word,
						StringComparer.Ordinal)))
			{
				complaints.Add(
					$"{entry.Key} now states its category and no longer needs its exception; "
					+ "delete the entry");
			}
		}

		complaints.Should().BeEmpty();
	}

	/// <summary>
	///		Asserts that every entry in both exception registers,
	///		<see cref="StructuralQualifierExceptions"/> and <see cref="CategorylessByDesign"/>,
	///		carries a non-blank reason.
	/// </summary>
	[Fact]
	public void EveryRegisteredExceptionStatesAReason()
	{
		StructuralQualifierExceptions
			.Values
			.Concat(CategorylessByDesign.Values)
			.Should()
			.OnlyContain(
				reason =>
					!string.IsNullOrWhiteSpace(reason),
				"an exception without a reason is an exemption nobody has to defend");
	}

	/// <summary>
	///		Asserts that the colour-token scan
	///		<see cref="EveryRegisteredExceptionStillExistsAndStillOffends"/> resolves both
	///		registers against reaches at least one token.
	/// </summary>
	[Fact]
	public void TheColourTokenScanReachesAtLeastOneToken()
	{
		// Both registers are non-empty by construction, so an empty scan would fill that fact's
		// complaints list rather than empty it — but the guard is stated anyway, as a fact of its
		// own, because "it happens to fail loudly for another reason" is a property of today's
		// registers, not of that fact.
		ColourTokens()
			.Should()
			.NotBeEmpty(
				"every entry in both registers is resolved against this scan, so an empty scan "
				+ "makes EveryRegisteredExceptionStillExistsAndStillOffends meaningless");
	}

	/// <summary>
	///		Asserts that a keyed <c>Style</c> carries the <c>Style</c> suffix if and only if a
	///		colour token of the same base name exists.
	///		<para>
	///		§5 of the design spec called <c>DmBorderTavernStyle</c> "the only style key with a
	///		Style suffix" and asked for it to be renamed. It is not the only one —
	///		<c>DmBorderParchmentStyle</c> has one too — and those two are exactly the two style
	///		keys that would collide with a colour token without it. The suffix is doing real work,
	///		so this fact holds it in BOTH directions: a colliding style must carry it, and a
	///		non-colliding style must not.
	///		</para>
	/// </summary>
	[Fact]
	public void StyleKeysCarryTheStyleSuffixExactlyWhenTheyCollideWithAColour()
	{
		var colours = ColourTokens().ToHashSet(StringComparer.Ordinal);

		var styleKeys = ScannedThemeFiles()
			.SelectMany(path =>
				StyleKeyPattern.Matches(
					ThemeSource.XmlCommentPattern.Replace(
						File.ReadAllText(path),
						string.Empty)))
			.Select(match =>
				match.Groups["key"].Value)
			.Distinct(StringComparer.Ordinal)
			.ToList();

		styleKeys
			.Should()
			.NotBeEmpty("the theme dictionaries must declare keyed styles or this fact is vacuous");

		var complaints = new List<string>();

		foreach (var key in styleKeys)
		{
			var suffixed = key.EndsWith(
				"Style",
				StringComparison.Ordinal);

			var baseName = suffixed
				? key[..^"Style".Length]
				: key;

			var collides = colours.Contains(suffixed ? baseName : key);

			if (collides && !suffixed)
			{
				complaints.Add(
					$"{key} is a Style with the same key as a colour token; append 'Style' so "
					+ "the two are distinguishable at a call site");
			}

			if (suffixed && !collides)
			{
				complaints.Add(
					$"{key} carries a 'Style' suffix but no colour token named {baseName} "
					+ "exists; the suffix is disambiguation, not decoration — drop it");
			}
		}

		complaints.Should().BeEmpty();
	}

	/// <summary>
	///		Returns the fault in a token's name, or <see langword="null"/> if it obeys the rule.
	/// </summary>
	/// <param name="key">
	///		The token's resource key, including the <c>Dm</c> prefix.
	/// </param>
	private static string? Fault(string key)
	{
		var words = Words(key);

		if (words.Count == 0)
		{
			return "has no name after the Dm prefix";
		}

		return StructuralHeads.Contains(
			words[0],
			StringComparer.Ordinal)
			? StructuralFault(
				key,
				words)
			: SemanticFault(
				key,
				words);
	}

	private static string? StructuralFault(
		string key,
		IReadOnlyList<string> words)
	{
		var index = 1;

		if (!StructuralQualifierExceptions.ContainsKey(key))
		{
			var family = LongestMatch(
				words,
				index,
				SurfaceFamilies);

			if (family is null)
			{
				return $"leads with the category '{words[0]}', so it is STRUCTURAL and the next "
					+ "word must be a declared surface family; register it in SurfaceFamilies, "
					+ "rename it into the semantic family, or add a StructuralQualifierExceptions "
					+ "entry saying why the qualifier is not a family";
			}

			index += Words(family).Count;
		}

		return UndeclaredQualifier(
			words,
			index);
	}

	private static string? SemanticFault(
		string key,
		IReadOnlyList<string> words)
	{
		var role = LongestMatch(
			words,
			0,
			SemanticRoles);

		// Longest match wins between the two vocabularies, and it has to: the role PopupButton
		// begins with the surface family Popup, so a leading-family check that ran first would
		// report all four DmPopupButton* washes as reversed structural tokens. It did, on this
		// fact's first run.
		if (LongestMatch(
				words,
				0,
				SurfaceFamilies) is { } leadingFamily
			&& (role is null
				|| Words(role).Count < Words(leadingFamily).Count))
		{
			return $"leads with the surface family '{leadingFamily}'. A surface family is a "
				+ "STRUCTURAL qualifier and must follow its category, not lead: write "
				+ $"Dm(Surface|Border|Text){leadingFamily}… instead";
		}

		if (role is null)
		{
			return $"leads with '{words[0]}', which is neither a category nor a declared role; "
				+ "add it to SemanticRoles if it really is a new role";
		}

		var index = Words(role).Count;

		// The sanctioned "role hue as rendered on a named surface" idiom: On must be followed by
		// a declared surface family and nothing else.
		if (index < words.Count
			&& string.Equals(
				words[index],
				"On",
				StringComparison.Ordinal))
		{
			var family = LongestMatch(
				words,
				index + 1,
				SurfaceFamilies);

			if (family is null)
			{
				return "uses the On<SurfaceFamily> idiom but what follows 'On' is not a declared "
					+ "surface family";
			}

			return index + 1 + Words(family).Count == words.Count
				? null
				: $"has words after On{family}; the idiom names the surface and ends there";
		}

		var fault = UndeclaredQualifier(
			words,
			index);

		if (fault is not null)
		{
			return fault;
		}

		var statesCategory = words.Any(word =>
			CategoryWords.Contains(
				word,
				StringComparer.Ordinal));

		return statesCategory || CategorylessByDesign.ContainsKey(key)
			? null
			: "states no category and is not registered in CategorylessByDesign. Either name "
				+ "what it paints (Surface, Border, Stroke, Fill, Tint or Text) or register it "
				+ "with the reason it needs none";
	}

	private static string? UndeclaredQualifier(
		IReadOnlyList<string> words,
		int index)
	{
		for (var position = index; position < words.Count; position++)
		{
			if (!Qualifiers.Contains(
					words[position],
					StringComparer.Ordinal))
			{
				return $"contains the undeclared word '{words[position]}'; add it to Qualifiers "
					+ "if it is a category, a state, a weight or the name of a part";
			}
		}

		return null;
	}

	/// <summary>
	///		Returns the longest entry of <paramref name="candidates"/> that matches the words of
	///		the key starting at <paramref name="index"/>, or <see langword="null"/> if none does.
	///		Matching is on whole words, so "Save" does not match the "Saved" of DmSavedStampText.
	/// </summary>
	private static string? LongestMatch(
		IReadOnlyList<string> words,
		int index,
		IReadOnlyList<string> candidates)
	{
		return candidates
			.Where(candidate =>
				MatchesAt(
					words,
					index,
					Words(candidate)))
			.OrderByDescending(
				candidate =>
					Words(candidate).Count)
			.FirstOrDefault();
	}

	private static bool MatchesAt(
		IReadOnlyList<string> words,
		int index,
		IReadOnlyList<string> candidateWords)
	{
		if (index + candidateWords.Count > words.Count)
		{
			return false;
		}

		for (var offset = 0; offset < candidateWords.Count; offset++)
		{
			if (!string.Equals(
					words[index + offset],
					candidateWords[offset],
					StringComparison.Ordinal))
			{
				return false;
			}
		}

		return true;
	}

	/// <summary>
	///		Splits a PascalCase name into words, dropping a leading <c>Dm</c> and a trailing
	///		<c>Brush</c>. The brush aliases are colour tokens under another element name and obey
	///		the same rule; the suffix says which element declares them, not what they are.
	/// </summary>
	private static IReadOnlyList<string> Words(string name)
	{
		var trimmed = name.StartsWith(
			"Dm",
			StringComparison.Ordinal)
			? name["Dm".Length..]
			: name;

		if (trimmed.EndsWith(
				"Brush",
				StringComparison.Ordinal)
			&& trimmed.Length > "Brush".Length)
		{
			trimmed = trimmed[..^"Brush".Length];
		}

		return PascalWordPattern
			.Matches(trimmed)
			.Select(match =>
				match.Value)
			.ToList();
	}

	private static IReadOnlyList<string> ColourTokens()
	{
		var tokensPath = ScannedThemeFiles()
			.FirstOrDefault(path =>
				string.Equals(
					Path.GetFileName(path),
					TokensDictionaryName,
					StringComparison.Ordinal));

		var text = tokensPath is null
			? string.Empty
			: File.ReadAllText(tokensPath);

		return ColourTokenPattern
			.Matches(
				ThemeSource.XmlCommentPattern.Replace(
					text,
					string.Empty))
			.Select(match =>
				match.Groups["key"].Value)
			.Distinct(StringComparer.Ordinal)
			.ToList();
	}

	/// <summary>
	///		The theme dictionaries this class scans. Every fact in the class reads the markup
	///		through this ONE member, so the class has a single scan source that
	///		<see cref="TheTokenNamingScanReachesAtLeastOneThemeDictionary"/> can guard and that a
	///		one-line edit here can redirect.
	///		<para>
	///		Asserts nothing and may return an empty list: helpers return, tests assert.
	///		</para>
	/// </summary>
	private static IReadOnlyList<string> ScannedThemeFiles()
	{
		return ThemeSource.ThemeFiles();
	}
}
