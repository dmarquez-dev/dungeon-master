using System.Xml.Linq;
using FluentAssertions;

namespace DungeonMaster.Tests.App.Theme;

/// <summary>
///		Guards the one thing every other theme fact takes on trust: that a <c>Dm*</c> token can
///		actually convert to the type of the property it is bound to.
///		<para>
///		THE FAILURE THIS EXISTS FOR IS SILENT, AND IT WAS LIVE. The radius tokens were declared
///		<c>x:Double</c> while <c>Button.CornerRadius</c> is <c>System.Int32</c>.
///		<c>BindableProperty.TryConvert</c> will not narrow a double to an int, so every
///		<c>Button.CornerRadius="{StaticResource DmRadius…}"</c> in the app — the implicit style,
///		the <c>DmButtonBase</c> setter and eleven inline sites — was REJECTED on every
///		evaluation. The build was clean, the XAML was valid, every existing theme fact was green,
///		and no button in the app had a themed corner radius. The only outward signs were a
///		console warning per evaluation and the fact that the property sat on its <c>-1</c>
///		default, which WinUI renders close enough to 3-4px to survive a glance.
///		</para>
///		<para>
///		A key existing, resolving, and being spelled correctly is what the sibling facts check.
///		None of them reads the TYPE of the property on the far side of the binding, which is
///		what decides whether the value lands at all.
///		</para>
/// </summary>
/// <remarks>
///		<para>
///		THE MAUI TABLES BELOW ARE A TRANSCRIPTION AND THIS ASSEMBLY CANNOT CHECK THEM. It may not
///		reference <c>DungeonMaster.App</c> — doing so imports MAUI's global usings into a
///		640-test project — so no reflection over <c>Microsoft.Maui.Controls</c> is possible here,
///		and no result computed elsewhere can be handed in. MASTER-100 is therefore closed by TWO
///		independent alarms, one per assembly, with no data crossing between them.
///		</para>
///		<para>
///		ALARM ONE is
///		<see cref="TheMauiControlsPackageVersionStillMatchesTheVersionTheTablesWereReadOff"/> in
///		this file. A MAUI bump fails it and says the tables must be re-read, instead of leaving
///		them stating the old truth as a false pass.
///		</para>
///		<para>
///		ALARM TWO is <c>DungeonMaster.App.UnitTests.Theme.MauiPropertyTypeTests</c> and
///		<c>MauiConversionMatrixTests</c>, which restate these same values next to reflection and
///		live probes against the resolved assembly. That duplication is deliberate and its reason
///		is recorded at that site.
///		</para>
/// </remarks>
public class TokenTypeMatchesTargetPropertyTests : IClassFixture<TokenBindingFixture>
{
	/// <summary>
	///		Marks a property whose type the fact could not determine. Carried as a type name so a
	///		binding onto it flows through the same pipeline as any other and is reported by
	///		<see cref="EveryDmTokenBindingTargetsAPropertyOfAKnownType"/> rather than dropped.
	/// </summary>
	internal const string UnknownType = "?";

	/// <summary>
	///		The <c>Microsoft.Maui.Controls</c> version the tables in this file were read off.
	///		ALARM ONE compares it against the repository's own <c>Directory.Packages.props</c>.
	/// </summary>
	private const string MauiControlsVersionTheTablesWereReadOff = "10.0.41";

	/// <summary>
	///		The date the tables were read off, quoted in ALARM ONE's failure so a reader knows how
	///		old the transcription is without opening this file.
	/// </summary>
	private const string TablesReadOffOn = "2026-08-07";

	/// <summary>
	///		The CLR type each token-declaring XAML element produces.
	///		<para>
	///		A token declared as an element this fact does not know is a failure, not a skip: the
	///		whole point is that retyping a token is invisible everywhere else.
	///		</para>
	/// </summary>
	private static readonly Dictionary<string, string> TokenElementTypes =
		new(StringComparer.Ordinal)
		{
			["x:Double"] = "double",
			["x:Int32"] = "int",
			["x:String"] = "string",
			["Thickness"] = "Thickness",
			["Color"] = "Color",
			["SolidColorBrush"] = "Brush",
			["Style"] = "Style",
		};

	/// <summary>
	///		Properties whose declared type depends on the element that owns them. These are the
	///		live traps, and the reason a property-name-only table would be a lie.
	///		<para>
	///		Read off Microsoft.Maui.Controls.dll 10.0.41 by reflection on 2026-08-07. MAUI's six
	///		corner-radius-bearing properties span FOUR different types, and the two <c>int</c>
	///		ones are the only members of that set that reject a double.
	///		</para>
	///		<para>
	///		<c>ImageButton</c> and <c>Frame</c> are listed although the app uses neither. They are
	///		the rest of the trap class — <c>Frame.CornerRadius</c> is <c>System.Single</c> and
	///		rejects a double just as <c>Button</c>'s does — and seeding them costs nothing,
	///		whereas discovering them the way <c>Button</c> was discovered costs a release.
	///		</para>
	///		<para>
	///		RE-ESTABLISHED BY REFLECTION in
	///		<c>DungeonMaster.App.UnitTests.Theme.MauiPropertyTypeTests</c>, which is the only
	///		assembly in this repository that can see these types at all.
	///		</para>
	/// </summary>
	private static readonly Dictionary<string, string> PropertyTypesByElement =
		new(StringComparer.Ordinal)
		{
			["Button.CornerRadius"] = "int",
			["ImageButton.CornerRadius"] = "int",
			["Frame.CornerRadius"] = "float",
			["BoxView.CornerRadius"] = "CornerRadius",
			["RoundRectangle.CornerRadius"] = "CornerRadius",
			["Border.StrokeShape"] = "IShape",
		};

	/// <summary>
	///		Properties whose declared type is the same whatever element carries them.
	///		<para>
	///		<c>CornerRadius</c> is deliberately ABSENT. It is the property this whole fact exists
	///		for, and it has no element-independent type; leaving it out means a
	///		<c>CornerRadius</c> binding onto an element nobody has taught this table about fails
	///		here rather than being guessed at.
	///		</para>
	///		<para>
	///		RE-ESTABLISHED BY REFLECTION in
	///		<c>DungeonMaster.App.UnitTests.Theme.MauiPropertyTypeTests</c>.
	///		</para>
	/// </summary>
	private static readonly Dictionary<string, string> PropertyTypesByName =
		new(StringComparer.Ordinal)
		{
			["Padding"] = "Thickness",
			["Margin"] = "Thickness",

			["FontFamily"] = "string",

			["FontSize"] = "double",
			["CharacterSpacing"] = "double",
			["LineHeight"] = "double",
			["StrokeThickness"] = "double",
			["BorderWidth"] = "double",
			["Spacing"] = "double",
			["RowSpacing"] = "double",
			["ColumnSpacing"] = "double",
			["ItemSpacing"] = "double",
			["HeightRequest"] = "double",
			["WidthRequest"] = "double",
			["MinimumHeightRequest"] = "double",
			["MinimumWidthRequest"] = "double",

			["MaxLines"] = "int",

			["Style"] = "Style",
			["BasedOn"] = "Style",

			["Stroke"] = "Brush",
			["Fill"] = "Brush",

			["Color"] = "Color",
			["TextColor"] = "Color",
			["BackgroundColor"] = "Color",
			["BorderColor"] = "Color",
			["PlaceholderColor"] = "Color",

			["Shell.BackgroundColor"] = "Color",
			["Shell.ForegroundColor"] = "Color",
			["Shell.TitleColor"] = "Color",
			["Shell.DisabledColor"] = "Color",
			["Shell.UnselectedColor"] = "Color",
			["Shell.TabBarBackgroundColor"] = "Color",
			["Shell.TabBarForegroundColor"] = "Color",
			["Shell.TabBarTitleColor"] = "Color",
			["Shell.TabBarUnselectedColor"] = "Color",
		};

	/// <summary>
	///		The conversions <c>BindableProperty.TryConvert</c> performs, other than the identity.
	///		<para>
	///		EVERY ENTRY AND EVERY ABSENCE WAS MEASURED against Microsoft.Maui.Controls 10.0.41 on
	///		2026-08-07 by calling <c>SetValue</c> with a boxed value of the source type and
	///		reading the property back — because the failure mode is a value that is dropped, a
	///		clean build and a clean launch prove nothing here.
	///		</para>
	///		<para>
	///		The results that matter, using 7 as the probe value:
	///		</para>
	///		<para>
	///		ACCEPTED, reading back as 7 (or as a SolidColorBrush): int → double, int → float,
	///		int → CornerRadius, int → Thickness, double → CornerRadius, double → Thickness,
	///		Color → Brush.
	///		</para>
	///		<para>
	///		DROPPED, leaving the property exactly as it was: double → int, double → float,
	///		string → int, string → double, string → CornerRadius, string → Thickness.
	///		</para>
	///		<para>
	///		int widens to everything and double narrows to nothing. That asymmetry is the whole
	///		shape of the defect this fact guards.
	///		</para>
	///		<para>
	///		int reaching <c>CornerRadius</c> is a chained conversion — TryConvert widens the int
	///		and then applies CornerRadius's implicit double operator — and it is exactly the kind
	///		of thing that is easy to assume in either direction. It is in this table because it
	///		was measured, not because it was reasoned.
	///		</para>
	///		<para>
	///		RE-MEASURED, both halves, against real MAUI instances in
	///		<c>DungeonMaster.App.UnitTests.Theme.MauiConversionMatrixTests</c>.
	///		</para>
	/// </summary>
	private static readonly HashSet<string> WideningConversions =
		new(StringComparer.Ordinal)
		{
			"int->double",
			"int->float",
			"int->CornerRadius",
			"int->Thickness",
			"double->CornerRadius",
			"double->Thickness",
			"Color->Brush",
		};

	private readonly TokenBindingFixture _fixture;

	/// <summary>
	///		Initializes a new instance of <see cref="TokenTypeMatchesTargetPropertyTests"/>.
	/// </summary>
	/// <param name="fixture">
	///		The disk scan, computed once for this class rather than once per fact.
	/// </param>
	public TokenTypeMatchesTargetPropertyTests(TokenBindingFixture fixture)
	{
		_fixture = fixture;
	}

	/// <summary>
	///		ALARM ONE for MASTER-100. The repository's pinned <c>Microsoft.Maui.Controls</c>
	///		version still matches the version the tables in this file were transcribed from.
	///		<para>
	///		This assembly cannot see a MAUI type, so it cannot check the tables themselves. What
	///		it can check is whether the framework underneath them has moved. A bump fails here and
	///		says the tables must be re-read, rather than leaving them stating the old truth as a
	///		false pass.
	///		</para>
	/// </summary>
	[Fact]
	public void TheMauiControlsPackageVersionStillMatchesTheVersionTheTablesWereReadOff()
	{
		var path = Path.Combine(
			RepoPaths.Root,
			"Directory.Packages.props");

		var versions = XDocument
			.Load(path)
			.Descendants("PackageVersion")
			.Where(element =>
				string.Equals(
					element.Attribute("Include")?.Value,
					"Microsoft.Maui.Controls",
					StringComparison.Ordinal))
			.Select(element =>
				element.Attribute("Version")?.Value)
			.ToList();

		versions
			.Should()
			.ContainSingle(
				"Directory.Packages.props pins Microsoft.Maui.Controls exactly once; finding "
				+ "none would leave this alarm reading nothing and passing forever");

		versions[0]
			.Should()
			.Be(
				MauiControlsVersionTheTablesWereReadOff,
				"the MAUI property types and the conversion matrix in this file were read off "
				+ $"{MauiControlsVersionTheTablesWereReadOff} on {TablesReadOffOn} and nothing in "
				+ "this assembly can re-read them. Re-measure them against the new version, "
				+ "update them here and in DungeonMaster.App.UnitTests.Theme.MauiPropertyTypeTests "
				+ "and MauiConversionMatrixTests, then move this constant");
	}

	/// <summary>
	///		Asserts that every <c>Dm*</c> token bound in the application's XAML converts to the
	///		declared type of the property it targets. This is the fact that would have caught the
	///		radius defect on the commit that introduced it.
	/// </summary>
	[Fact]
	public void EveryDmTokenBindingConvertsToItsTargetPropertyType()
	{
		var bindings = Bindings();
		var tokenTypes = TokenTypesByKey();

		bindings
			.Should()
			.NotBeEmpty(
				"the offenders list below is empty both when every binding is correct and when "
				+ "nothing was examined, and those two states must not look the same");

		var offenders = bindings
			.Where(binding =>
				binding.PropertyType != UnknownType)
			.Where(binding =>
				tokenTypes.ContainsKey(binding.Key))
			.Where(binding =>
				!Converts(
					tokenTypes[binding.Key],
					binding.PropertyType))
			.Select(binding =>
				$"{binding.Where} -> {binding.Element}.{binding.Property} is "
				+ $"{binding.PropertyType}, but '{binding.Key}' is declared "
				+ $"{tokenTypes[binding.Key]}; MAUI drops the value and the property keeps its "
				+ "default")
			.OrderBy(
				entry =>
					entry,
				StringComparer.Ordinal)
			.ToList();

		offenders
			.Should()
			.BeEmpty(
				"a token whose type the target property cannot accept is discarded at every "
				+ "evaluation with only a console warning to show for it");
	}

	/// <summary>
	///		Asserts that this fact can name the type of every property a token is bound to.
	///		<para>
	///		Without this, the guard above degrades quietly: an unrecognised element or property
	///		would simply not be checked, and the next <c>Button.CornerRadius</c> would slip
	///		through a green suite exactly as the first one did. Extend
	///		<see cref="PropertyTypesByElement"/> or <see cref="PropertyTypesByName"/> when this
	///		fails — do not widen the scan to skip the unknown.
	///		</para>
	/// </summary>
	[Fact]
	public void EveryDmTokenBindingTargetsAPropertyOfAKnownType()
	{
		var bindings = Bindings();

		bindings
			.Should()
			.NotBeEmpty(
				"the unknown list below is empty both when every target is nameable and when "
				+ "nothing was examined, and those two states must not look the same");

		var unknown = bindings
			.Where(binding =>
				binding.PropertyType == UnknownType)
			.Select(binding =>
				$"{binding.Where} -> {binding.Element}.{binding.Property} (binds '{binding.Key}')")
			.Distinct(StringComparer.Ordinal)
			.OrderBy(
				entry =>
					entry,
				StringComparer.Ordinal)
			.ToList();

		unknown
			.Should()
			.BeEmpty(
				"this fact cannot check a binding whose target type it cannot name; add the "
				+ "element+property to PropertyTypesByElement, or the property to "
				+ "PropertyTypesByName, with the type read off Microsoft.Maui.Controls.dll");
	}

	/// <summary>
	///		Asserts that every <c>{StaticResource Dm…}</c> occurrence in the application's XAML
	///		was attributed to an element and a property.
	///		<para>
	///		This is the fact that keeps the other two honest. The scan understands four binding
	///		shapes — an attribute on an element, a <c>Setter</c> inside a keyed <c>Style</c>, a
	///		<c>Setter</c> inside a <c>VisualStateManager.VisualStateGroups</c> attached to an
	///		element, and a <c>Setter</c> carrying <c>TargetName</c> at another element in the same
	///		file — and a fifth shape appearing in the tree would otherwise be silently invisible to
	///		a guard that still reported green.
	///		</para>
	/// </summary>
	[Fact]
	public void EveryStaticResourceReferenceInTheXamlIsSeenByTheScan()
	{
		var seen = _fixture.Bindings.Count;
		var total = _fixture.StaticResourceOccurrences;

		total
			.Should()
			.BeGreaterThan(0, "the XAML scan must find token references or every fact here is vacuous");

		seen
			.Should()
			.Be(
				total,
				"every {{StaticResource Dm…}} in the application's XAML must be attributed to an "
				+ "element and a property; an unattributed one is a binding shape this scan does "
				+ "not understand and therefore does not check");
	}

	/// <summary>
	///		Asserts that the scan actually reaches at least one binding onto an <c>int</c>-typed
	///		property.
	///		<para>
	///		This is a fact about the GUARD, not about the app. A guard that is structurally
	///		incapable of firing passes forever, and the int-typed properties are the only ones in
	///		MAUI's geometry surface that can reject a plausible-looking token. If every such
	///		binding is refactored away, this fails and says so, rather than leaving a fact that
	///		looks like cover and is not.
	///		</para>
	/// </summary>
	[Fact]
	public void TheScanReachesTheIntTypedPropertiesItExistsToGuard()
	{
		var intTargets = Bindings()
			.Where(binding =>
				binding.PropertyType == "int")
			.Select(binding =>
				$"{binding.Element}.{binding.Property}")
			.Distinct(StringComparer.Ordinal)
			.ToList();

		intTargets
			.Should()
			.NotBeEmpty(
				"the int-typed properties are the live trap this fact exists for; if no token "
				+ "binds to one any more, say so deliberately rather than leaving a guard that "
				+ "cannot fire");
	}

	/// <summary>
	///		The XAML scan reaches at least one file.
	///		<para>
	///		This was an assertion inside the <c>XamlFiles()</c> Arrange helper, so a scan that
	///		reached nothing was reported against whichever fact happened to call it first.
	///		<c>contracts/anti-vacuity.md</c> section 2.
	///		</para>
	/// </summary>
	[Fact]
	public void TheXamlScanReachesAtLeastOneFile()
	{
		_fixture
			.XamlFiles
			.Should()
			.NotBeEmpty("the XAML scan must find files or every fact here is vacuous");
	}

	/// <summary>
	///		The token declaration scan reaches at least one token.
	///		<para>
	///		Retired from inside <c>TokenTypesByKey()</c>. <c>contracts/anti-vacuity.md</c>
	///		section 2.
	///		</para>
	/// </summary>
	[Fact]
	public void TheTokenDeclarationScanReachesAtLeastOneToken()
	{
		TokenTypesByKey()
			.Should()
			.NotBeEmpty("the token declaration scan must find tokens or every fact here is vacuous");
	}

	/// <summary>
	///		The application bindable property scan reaches at least one declaration.
	///		<para>
	///		Retired from inside <c>AppPropertyTypes()</c>. <c>contracts/anti-vacuity.md</c>
	///		section 2.
	///		</para>
	/// </summary>
	[Fact]
	public void TheAppBindablePropertyScanReachesAtLeastOneDeclaration()
	{
		_fixture
			.AppPropertyTypes
			.Should()
			.NotBeEmpty(
				"the application declares bindable properties; finding none means the pattern "
				+ "has stopped matching and every app-control binding is silently unchecked");
	}

	/// <summary>
	///		Every <c>Dm*</c> token is declared as an element this fact knows how to type.
	///		<para>
	///		Retired from inside <c>TokenTypesByKey()</c>, where it was reported against whichever
	///		fact called the helper first.
	///		</para>
	/// </summary>
	[Fact]
	public void EveryDmTokenIsDeclaredAsAnElementThisFactKnows()
	{
		var declarations = _fixture.TokenDeclarations;

		declarations
			.Should()
			.NotBeEmpty(
				"the unrecognised list below is empty both when every declaration is known and "
				+ "when nothing was examined");

		var unrecognised = declarations
			.Where(declaration =>
				!TokenElementTypes.ContainsKey(declaration.QualifiedElementName))
			.Select(declaration =>
				$"{declaration.Where} declares '{declaration.Key}' as "
				+ $"<{declaration.QualifiedElementName}>")
			.OrderBy(
				entry =>
					entry,
				StringComparer.Ordinal)
			.ToList();

		unrecognised
			.Should()
			.BeEmpty(
				"a token declared as an element this fact does not know cannot be type-checked; "
				+ "add the element to TokenElementTypes");
	}

	/// <summary>
	///		The structured <c>BindableProperty.Create</c> pattern still matches every plain
	///		occurrence of that call in the application's source.
	///		<para>
	///		MASTER-109. <c>TokenBindingFixture.AppBindablePropertyPattern</c> is a regular
	///		expression over source TEXT, and such a pattern fails OPEN: a reformatting that moves
	///		an argument onto another line leaves the declaration present and the pattern matching
	///		nothing, retiring the guard in silence. Comparing the structured match count against
	///		the plain substring count over the same files is what makes that loud. BOTH NUMBERS
	///		ARE DERIVED; neither is hard-coded.
	///		</para>
	///		<para>
	///		Comments are blanked before either count, because a doc comment that merely NAMES
	///		<c>BindableProperty.Create</c> in prose is not a declaration. <c>Theme/DmTheme.cs</c>
	///		carries exactly one such mention, and counting it made the two numbers differ by one.
	///		</para>
	/// </summary>
	[Fact]
	public void TheStructuredBindablePropertyPatternMatchesEveryPlainOccurrence()
	{
		_fixture
			.AppBindablePropertyOccurrences
			.Should()
			.BeGreaterThan(
				0,
				"the application declares bindable properties; finding no plain occurrence means "
				+ "this comparison is reading nothing and would agree with anything");

		_fixture
			.AppBindablePropertyDeclarations
			.Should()
			.Be(
				_fixture.AppBindablePropertyOccurrences,
				"a BindableProperty.Create the structured pattern no longer matches is a control "
				+ "property this fact has silently stopped type-checking; the substring is still "
				+ "there, so the difference is the pattern's and not the application's");
	}

	private static bool Converts(
		string from,
		string to)
	{
		return string.Equals(
				from,
				to,
				StringComparison.Ordinal)
			|| WideningConversions.Contains($"{from}->{to}");
	}

	/// <summary>
	///		Returns the CLR type of every <c>Dm*</c> token, keyed by resource key. Asserts nothing:
	///		<see cref="TheTokenDeclarationScanReachesAtLeastOneToken"/> and
	///		<see cref="EveryDmTokenIsDeclaredAsAnElementThisFactKnows"/> carry what used to be
	///		asserted from inside it.
	/// </summary>
	private Dictionary<string, string> TokenTypesByKey()
	{
		var types = new Dictionary<string, string>(StringComparer.Ordinal);

		foreach (var declaration in _fixture.TokenDeclarations)
		{
			if (TokenElementTypes.TryGetValue(
				declaration.QualifiedElementName,
				out var type))
			{
				types.TryAdd(
					declaration.Key,
					type);
			}
		}

		return types;
	}

	/// <summary>
	///		Returns every <c>Dm*</c> token binding the fixture found, with the CLR type of its
	///		target property attributed from the tables in this file.
	/// </summary>
	private IReadOnlyList<Binding> Bindings()
	{
		return _fixture
			.Bindings
			.Select(binding =>
				new Binding(
					binding.Where,
					binding.Element,
					binding.Property,
					binding.Key,
					PropertyType(
						binding.Element,
						binding.Property)))
			.ToList();
	}

	private string PropertyType(
		string owner,
		string property)
	{
		// The application's own controls first: DmDivider.IconFontSize is not a MAUI property
		// and the tables above have no business guessing at it.
		if (_fixture.AppPropertyTypes.TryGetValue(
			$"{owner}.{property}",
			out var appType))
		{
			return appType;
		}

		if (PropertyTypesByElement.TryGetValue(
			$"{owner}.{property}",
			out var elementType))
		{
			return elementType;
		}

		return PropertyTypesByName.TryGetValue(
			property,
			out var namedType)
			? namedType
			: UnknownType;
	}

	private sealed record Binding(
		string Where,
		string Element,
		string Property,
		string Key,
		string PropertyType);
}
