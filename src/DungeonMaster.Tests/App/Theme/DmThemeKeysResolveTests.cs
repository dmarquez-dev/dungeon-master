using System.Text.RegularExpressions;
using FluentAssertions;

namespace DungeonMaster.Tests.App.Theme;

/// <summary>
///		Guards <c>DmTheme</c>, the C# accessor over the theme dictionaries. Every key it resolves
///		is a string literal, so a typo or a deleted token fails at runtime rather than at build;
///		this fact turns that into a test failure instead.
/// </summary>
/// <remarks>
///		<para>
///		TWO OF THE FACTS HERE ARE SOURCE-TEXT LINTS AND SAY SO AT THEIR OWN SITE. A lint over
///		application C# is not a behavioural claim: it asserts a SPELLING, so a behaviour-preserving
///		edit can silence it. Where that limit exists it is written beside the code that carries it,
///		together with the behavioural guard that compensates. The compensating guard for
///		<c>DmTheme</c> resolution is
///		<c>DungeonMaster.App.UnitTests.Theme.DmThemeResolutionTests</c>, which reads every accessor
///		for real against the live merged resource dictionaries and therefore holds under any
///		spelling. It cannot live in this assembly: <c>DungeonMaster.Tests</c> may not reference
///		<c>DungeonMaster.App</c> (contracts/app-unit-tests-project.md section 3.1).
///		</para>
///		<para>
///		THE XAML HALF IS NO LONGER A LINT. The keyed-element scan below reads the parsed XAML object
///		model through <c>ThemeSource.KeyedElements()</c> and compares
///		<c>ThemeSource.QualifiedElementName</c> against <see cref="ExpectedDeclaringElement"/>, so a
///		reformatted attribute, a keyed element split across lines, or an <c>x:Key</c> that is not
///		the first attribute all read correctly where the previous regular expression would have
///		missed them silently.
///		</para>
/// </remarks>
public class DmThemeKeysResolveTests
{
	/// <summary>
	///		Matches an inline <c>Resolve&lt;T&gt;("DmKey")</c> call in <c>DmTheme.cs</c>.
	/// </summary>
	/// <remarks>
	///		THIS IS A SOURCE-TEXT LINT AND IT CANNOT BE ANYTHING ELSE. No test can enumerate the keys
	///		the application WILL ask for at run time without executing every code path that asks, so
	///		the set of requested keys is only legible from the source. That is why the pattern
	///		survives MASTER-109 while the bindable-default patterns did not: those had a behavioural
	///		equivalent and this has none.
	///		<para>
	///		ITS LIMIT, STATED PLAINLY: it matches the call as WRITTEN. Wrapping a
	///		<c>Resolve&lt;T&gt;</c> call in a local helper, passing the key through a <c>const</c>, or
	///		building the generic argument through an alias all leave the behaviour identical and make
	///		the call invisible to this scan. The scan then reports fewer keys rather than failing,
	///		which is a fail-open mode:
	///		<see cref="TheDmThemeSourceScanReachesAtLeastOneResolveCall"/> catches the total collapse,
	///		but not a partial one.
	///		</para>
	///		<para>
	///		THE COMPENSATING CONTROL for a call this pattern stops seeing is
	///		<c>DungeonMaster.App.UnitTests.Theme.DmThemeResolutionTests</c>. It reads every public
	///		<c>DmTheme</c> accessor for real, so a key that has become invisible here is still
	///		resolved there — and a resolution that would throw at run time fails there whatever the
	///		call site looks like.
	///		</para>
	/// </remarks>
	private static readonly Regex ResolveCallPattern =
		new(
			@"Resolve<(?<type>[\w.]+)>\(""(?<key>Dm[A-Za-z0-9]*)""\)",
			RegexOptions.Compiled);

	/// <summary>
	///		The XAML element each <c>Resolve&lt;T&gt;</c> generic argument requires the declaring
	///		token to be. <c>Resolve&lt;T&gt;</c> returns only when <c>value is T</c>, and otherwise
	///		throws with a message naming the two causes that are NOT this one — a missing key, or a
	///		read that ran before <c>Application.Current.Resources</c> existed. A token whose element
	///		is changed under a call site therefore fails at runtime with a message that points away
	///		from the fault.
	///		<para>
	///		The values are the spelling <c>ThemeSource.QualifiedElementName</c> produces, so the
	///		table is compared against the parsed document with no translation layer.
	///		</para>
	/// </summary>
	private static readonly Dictionary<string, string> ExpectedDeclaringElement =
		new(StringComparer.Ordinal)
		{
			["Color"] = "Color",
			["Brush"] = "SolidColorBrush",
			["double"] = "x:Double",
			// int is the radius ladder, and it is int rather than double because
			// Button.CornerRadius is System.Int32 and rejects a double outright. Resolve<T>
			// matches on 'value is T', which does no numeric widening at all, so an x:Double
			// token read as int throws just as hard as a missing key.
			["int"] = "x:Int32",
			["string"] = "x:String",
			["Thickness"] = "Thickness",
			["Style"] = "Style",
		};

	/// <summary>
	///		Guards the <c>DmTheme.cs</c> source scan every fact below it depends on. The scan is a
	///		regular expression over application C#, so an edit that changes how a
	///		<c>Resolve&lt;T&gt;</c> call is written stops it matching; without this fact, a scan that
	///		matched nothing would let both key facts pass while examining nothing at all.
	/// </summary>
	[Fact]
	public void TheDmThemeSourceScanReachesAtLeastOneResolveCall()
	{
		ResolveCalls()
			.Should()
			.NotBeEmpty("DmTheme must resolve at least one key or the regex is broken");
	}

	/// <summary>
	///		Guards the theme-dictionary keyed-element scan every fact below it depends on. It is
	///		strictly downstream of <c>ThemeSource.ThemeDocuments()</c>, which asserts nothing and may
	///		return an empty list, so an empty scan yields an empty declaration set and every
	///		comparison against it succeeds vacuously.
	/// </summary>
	[Fact]
	public void TheThemeDictionaryKeyedElementScanReachesAtLeastOneDeclaration()
	{
		KeyedElementsInThemeDictionaries()
			.Should()
			.NotBeEmpty("the theme dictionary scan must find keyed elements or the scan is broken");
	}

	/// <summary>
	///		Asserts that every key <c>DmTheme</c> asks for is defined under Resources/Theme.
	/// </summary>
	[Fact]
	public void EveryKeyDmThemeResolves_IsDefinedInTheThemeDictionaries()
	{
		var requested = ResolveCalls()
			.Select(call =>
				call.Key)
			.ToHashSet(StringComparer.Ordinal);

		var defined = KeyedElementsInThemeDictionaries()
			.Keys
			.ToHashSet(StringComparer.Ordinal);

		requested.Except(defined).Should().BeEmpty();
	}

	/// <summary>
	///		Asserts that every key <c>DmTheme</c> resolves is declared by the XAML element the
	///		generic argument requires. The sibling fact above proves only that a key EXISTS; its
	///		call pattern discards the type, so a token converted from <c>&lt;Color&gt;</c> to
	///		<c>&lt;SolidColorBrush&gt;</c> — or a <c>&lt;x:Double&gt;</c> retyped as an
	///		<c>&lt;x:String&gt;</c> — leaves the whole suite green and throws on first read at
	///		runtime, with a message that blames a missing key or a startup ordering fault.
	///		This is the fact that reads the type.
	/// </summary>
	[Fact]
	public void EveryKeyDmThemeResolves_IsDeclaredAsTheTypeItIsResolvedAs()
	{
		var declarations = KeyedElementsInThemeDictionaries();

		var mismatches = new List<string>();

		foreach (var call in ResolveCalls())
		{
			if (!ExpectedDeclaringElement.TryGetValue(call.Type, out var expectedElement))
			{
				mismatches.Add(
					$"DmTheme resolves '{call.Key}' as {call.Type}, a type this fact has no "
					+ "expected declaring element for; extend ExpectedDeclaringElement");

				continue;
			}

			if (!declarations.TryGetValue(call.Key, out var declaration))
			{
				// A key that is absent altogether is the sibling fact's finding, not this one's.
				continue;
			}

			if (!string.Equals(
				declaration.Element,
				expectedElement,
				StringComparison.Ordinal))
			{
				mismatches.Add(
					$"DmTheme resolves '{call.Key}' as {call.Type} but {declaration.RelativePath} "
					+ $"declares it as <{declaration.Element}>, so Resolve<{call.Type}> will throw "
					+ $"at runtime — it expects <{expectedElement}>");
			}
		}

		mismatches.Should().BeEmpty();
	}

	/// <summary>
	///		Asserts that no static field initialiser in the application reads <c>DmTheme</c>.
	///		A field initialiser runs at type-initialisation time, which can precede
	///		<c>Application.Current.Resources</c>; <c>DmTheme</c> throws when those resources are
	///		absent, and the message reads like a missing key when the real fault is the ordering.
	///		This class of bug was hit twice during the theme migration, so it is guarded rather
	///		than merely commented.
	///		Four eager shapes are caught, one per alternative plus the wrapped form of each: a
	///		static field initialiser (including one split across lines per dotnet-format.md §3),
	///		a <c>BindableProperty.Create</c> positional default, a static auto-property
	///		initialiser (<c>static Color X { get; } = …</c>, which the compiler emits as a
	///		backing-field initialiser and which therefore runs at type-init time), and a static
	///		constructor body. Two lazy shapes are deliberately left alone: an expression-bodied
	///		member, and a lambda such as <c>defaultValueCreator</c> that sits inside a static
	///		field initialiser but does not run with it.
	/// </summary>
	/// <remarks>
	///		DISPOSITION UNDER MASTER-109: KEPT, AS A DOCUMENTED SOURCE-TEXT LINT. It is a
	///		PROHIBITION, not a value check — it asserts that a shape does NOT appear — and a
	///		prohibition has no behavioural equivalent a unit test can express: proving that no static
	///		initialiser anywhere reads <c>DmTheme</c> would mean running every type initialiser in
	///		the application before <c>Application.Current</c> exists, which is the very fault being
	///		prohibited.
	///		<para>
	///		IT FAILS OPEN, AND THAT IS THE WHOLE RISK. The pattern below matches declarations by
	///		their spelling. Introducing an alias, a <c>const</c>, or an attribute between the
	///		modifiers and the type stops an alternative matching, and the fact then reports NO
	///		OFFENDERS — indistinguishable from an application that has none. So it is narrowed by a
	///		guard rather than trusted: the <c>sourceFiles</c> assertion below establishes that the
	///		C# scan reached files at all, which separates "the scan found nothing to complain about"
	///		from "the scan read nothing". It does NOT separate a partially-stopped pattern from a
	///		clean application, and no source-text lint can.
	///		</para>
	/// </remarks>
	[Fact]
	public void NoEagerDmThemeRead()
	{
		// Line-spanning comes from [^;] and the (?!=>) guard, NOT from RegexOptions.Singleline:
		// there is no unescaped '.' outside a character class here, so that option would be inert
		// and is not set. The match runs from the declaration to the first ';' or the first '=>',
		// whichever comes first — the '=>' bound is what keeps the lazy shapes out.
		var eagerRead = new Regex(
			@"(static\s+(readonly\s+)?[\w<>?\[\].]+\s+\w+\s*=(?!>)"
			+ @"|static\s+[\w<>?\[\].]+\s+\w+\s*\{[^}]*\}\s*=(?!>)"
			+ @"|static\s+\w+\s*\(\s*\)\s*\{)"
			+ @"((?!=>)[^;])*\bDmTheme\.",
			RegexOptions.Compiled);

		var sourceFiles = RepoPaths.AppSourceFiles("*.cs");

		sourceFiles
			.Should()
			.NotBeEmpty("the C# scan must find application sources or this fact is vacuous");

		var offenders = new List<string>();

		foreach (var path in sourceFiles)
		{
			var text = File.ReadAllText(path);

			var relative = Path.GetRelativePath(
				RepoPaths.AppRoot,
				path);

			foreach (Match match in eagerRead.Matches(text))
			{
				var line = ThemeSource.LineNumber(
					text,
					match.Index);

				offenders.Add($"{relative}:{line} -> {Condense(match.Value)}");
			}
		}

		offenders
			.Should()
			.BeEmpty(
				"an eager DmTheme read runs before Application.Current.Resources exists; convert "
				+ "it to an expression-bodied member, a property with a get body, or a "
				+ "BindableProperty defaultValueCreator lambda, all of which defer the read to "
				+ "first use");
	}

	/// <summary>
	///		Returns every inline <c>Resolve&lt;T&gt;("DmKey")</c> call in <c>DmTheme.cs</c>,
	///		asserting nothing — including when it finds none.
	///		<see cref="TheDmThemeSourceScanReachesAtLeastOneResolveCall"/> is the guard.
	/// </summary>
	private static IReadOnlyList<ResolveCall> ResolveCalls()
	{
		var source = File.ReadAllText(
			Path.Combine(
				RepoPaths.AppRoot,
				"Theme",
				"DmTheme.cs"));

		return ResolveCallPattern
			.Matches(source)
			.Select(match =>
				new ResolveCall(
					match.Groups["key"].Value,
					match.Groups["type"].Value))
			.ToList();
	}

	/// <summary>
	///		Returns the first declaration of each <c>x:Key</c> in the theme dictionaries, read from
	///		the parsed XAML object model rather than from a regular expression over the file text,
	///		asserting nothing — including when it finds none.
	///		<see cref="TheThemeDictionaryKeyedElementScanReachesAtLeastOneDeclaration"/> is the
	///		guard.
	///		<para>
	///		<c>ThemeSource.KeyedElements()</c> returns EVERY declaration so that a consumer can
	///		report a duplicate key; this fact's question is "what element declares this key", for
	///		which the first declaration is the answer the resolver would find, so the later ones are
	///		dropped here rather than in the shared scan.
	///		</para>
	/// </summary>
	private static Dictionary<string, KeyDeclaration> KeyedElementsInThemeDictionaries()
	{
		var declarations = new Dictionary<string, KeyDeclaration>(StringComparer.Ordinal);

		foreach (var keyed in ThemeSource.KeyedElements())
		{
			declarations.TryAdd(
				keyed.Key,
				new KeyDeclaration(
					keyed.QualifiedElementName,
					keyed.RelativePath));
		}

		return declarations;
	}

	private static string Condense(string value)
	{
		return Regex.Replace(
			value,
			@"\s+",
			" ");
	}

	private sealed record ResolveCall(
		string Key,
		string Type);

	private sealed record KeyDeclaration(
		string Element,
		string RelativePath);
}
