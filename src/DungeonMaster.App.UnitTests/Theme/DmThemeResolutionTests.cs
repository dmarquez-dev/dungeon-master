using System.Reflection;

using DungeonMaster.App.Theme;
using DungeonMaster.App.UnitTests.Fixtures;

using FluentAssertions;

using Microsoft.Maui.Controls;

namespace DungeonMaster.App.UnitTests.Theme;

/// <summary>
///		Exercises <c>DmTheme</c> against the LIVE merged <c>Application.Current.Resources</c> the
///		running application reads, rather than against the text of the source that reads them.
/// </summary>
/// <remarks>
///		<para>
///		THIS IS THE COMPENSATING CONTROL FOR
///		<c>DungeonMaster.Tests.App.Theme.DmThemeKeysResolveTests.ResolveCallPattern</c>. That scan
///		is a regular expression over <c>DmTheme.cs</c> and cannot be anything else — no test can
///		enumerate the keys the application WILL ask for at run time without running every path that
///		asks — but it matches the call as WRITTEN, so wrapping a <c>Resolve&lt;T&gt;</c> call in a
///		local helper makes that call invisible to it, and it reports fewer keys rather than
///		failing. Every accessor below is read for real here, so a call the pattern has stopped
///		seeing is still resolved, and a resolution that would throw at run time fails here
///		whatever the call site looks like.
///		</para>
///		<para>
///		WHAT "FOR REAL" MEANS, AND ITS ONE LIMIT. <c>DmTheme.Resolve&lt;T&gt;</c> is
///		<see langword="private" />, and <c>ColorByKey</c> — the only runtime-keyed entry point — is
///		<see langword="internal" /> to <c>DungeonMaster.App</c>, which grants
///		<c>InternalsVisibleTo</c> to <c>DungeonMaster.App.Tests</c> and not to this assembly. So
///		<c>Resolve&lt;T&gt;</c> is reached the way the application reaches it: through the public
///		accessors, each of which calls it with a fixed key and a fixed <c>T</c>. Reading an
///		accessor executes the real resolver, with the real key, against the real dictionaries, and
///		<c>Resolve&lt;T&gt;</c> returns only when <c>value is T</c> and does no numeric widening —
///		so an <c>x:Double</c> token read as <c>int</c> throws just as hard as a missing key, and
///		that throw is this class's finding. What is NOT reachable is calling
///		<c>Resolve&lt;T&gt;</c> on a key the accessors do not name; see the report for T9.
///		</para>
/// </remarks>
[Collection(AppTestCollection.Name)]
public class DmThemeResolutionTests
{
	private readonly AppFixture _fixture;

	/// <summary>
	///		Initializes the class with the shared application fixture.
	/// </summary>
	/// <param name="fixture">
	///		The collection fixture supplying the constructed application and its merged resources.
	/// </param>
	public DmThemeResolutionTests(AppFixture fixture)
	{
		_fixture = fixture;
	}

	/// <summary>
	///		Guards the accessor scan every fact below it depends on. Reading zero accessors passes
	///		every enumeration in this class while resolving nothing at all.
	/// </summary>
	[Fact]
	public void TheDmThemeAccessorScanReachesAtLeastOneAccessor()
	{
		Accessors()
			.Should()
			.NotBeEmpty(
				"a resolution sweep over an empty accessor set passes while calling Resolve<T> "
				+ "not once");
	}

	/// <summary>
	///		Guards the live merged dictionaries. Every comparison below is against a key the fixture
	///		found there, so a fixture that merged nothing would make them all vacuous.
	/// </summary>
	[Fact]
	public void TheLiveMergedResourcesReachAtLeastOneDmKey()
	{
		LiveDmKeys()
			.Should()
			.NotBeEmpty(
				"the seven theme dictionaries merge at App construction; an empty live resource "
				+ "set means the fixture, not the theme, is broken");
	}

	/// <summary>
	///		Asserts that every public <c>DmTheme</c> accessor resolves, and that what it returns is
	///		a value the live merged dictionaries actually hold under a <c>Dm</c> key.
	/// </summary>
	/// <remarks>
	///		This is the behavioural replacement for
	///		<c>DmThemeKeysResolveTests.EveryKeyDmThemeResolvesIsDefinedInTheThemeDictionaries</c>
	///		and
	///		<c>DmThemeKeysResolveTests.EveryKeyDmThemeResolvesIsDeclaredAsTheTypeItIsResolvedAs</c>.
	///		Both of those read the key AND the declaring element out of text; this one makes the
	///		call. A missing key, a key declared as the wrong XAML element, and a read that ran too
	///		early all throw out of
	///		<c>Resolve&lt;T&gt;</c> with a message naming the key, and that message is reported here
	///		verbatim.
	/// </remarks>
	[Fact]
	public void EveryDmThemeAccessorResolvesToAValueTheLiveMergedDictionariesHold()
	{
		var live = LiveDmKeys();
		var accessors = Accessors();
		var reached = 0;
		var failures = new List<string>();

		foreach (var accessor in accessors)
		{
			object? resolved;

			try
			{
				resolved = accessor.GetValue(null);
			}
			catch (TargetInvocationException invocation)
				when (invocation.InnerException is not null)
			{
				failures.Add(
					$"DmTheme.{accessor.Name} threw {invocation.InnerException.GetType().Name}: "
					+ invocation.InnerException.Message);

				continue;
			}

			reached++;

			if (resolved is null)
			{
				failures.Add(
					$"DmTheme.{accessor.Name} resolved to null; Resolve<T> returns only when "
					+ "'value is T', so a null return means the accessor is not going through it");

				continue;
			}

			if (!accessor.PropertyType.IsInstanceOfType(resolved))
			{
				failures.Add(
					$"DmTheme.{accessor.Name} is declared {accessor.PropertyType.Name} but "
					+ $"returned a {resolved.GetType().Name}");

				continue;
			}

			if (!live.Values.Any(value =>
				ReferenceEquals(
					value,
					resolved)
				|| Equals(
					value,
					resolved)))
			{
				failures.Add(
					$"DmTheme.{accessor.Name} returned a value no Dm-keyed entry in the live "
					+ "merged dictionaries holds, so it is not reading the theme");
			}
		}

		reached
			.Should()
			.BePositive(
				"a resolution sweep that reached no accessor passes exactly like a correct one");

		reached
			.Should()
			.Be(
				accessors.Count,
				"every accessor must be reached, not merely the ones that happen to resolve");

		failures
			.Should()
			.BeEmpty(
				"every DmTheme accessor names a key that must exist under Resources/Theme and be "
				+ "declared as the XAML element its generic argument requires; Resolve<T> matches "
				+ "on 'value is T' and does no widening, so either fault throws at first read");
	}

	/// <summary>
	///		Asserts that the two accessors whose generic arguments differ only by numeric width
	///		resolve to their declared types and not to each other's.
	/// </summary>
	/// <remarks>
	///		<c>DmRadiusTight</c> is an <c>x:Int32</c> because <c>Button.CornerRadius</c> is
	///		<c>System.Int32</c>; <c>DmStrokeHairline</c> is an <c>x:Double</c> because
	///		<c>Border.StrokeThickness</c> is <c>System.Double</c>. A resolver that widened would
	///		make both readable as either, and every case above would still pass. This is the fact
	///		that says it does not.
	/// </remarks>
	[Fact]
	public void TheIntAndDoubleTokensResolveAsTheirOwnTypesAndNotEachOthers()
	{
		_fixture
			.FindResource("DmRadiusTight")
			.Should()
			.BeOfType<int>("Button.CornerRadius is System.Int32 and rejects a double outright");

		_fixture
			.FindResource("DmStrokeHairline")
			.Should()
			.BeOfType<double>("Border.StrokeThickness is System.Double");

		DmTheme
			.RadiusTight
			.Should()
			.Be(
				(int)_fixture.FindResource("DmRadiusTight")!,
				"Resolve<int> returns the x:Int32 token unchanged");

		DmTheme
			.StrokeHairline
			.Should()
			.Be(
				(double)_fixture.FindResource("DmStrokeHairline")!,
				"Resolve<double> returns the x:Double token unchanged");
	}

	/// <summary>
	///		The negative control: an accessor whose key is not declared anywhere throws, naming the
	///		key.
	/// </summary>
	/// <remarks>
	///		Without this, a resolver that returned a default for everything would pass every
	///		positive case above. Since <c>DmTheme</c> exposes no runtime-keyed public entry point,
	///		the undeclared key is produced by detaching the merged dictionaries for the duration of
	///		the call — after which <c>DmAccentGoldLight</c> genuinely is not declared anywhere the
	///		application can see, which is exactly the condition under test. The dictionaries are
	///		restored in a <c>finally</c> and the restoration is asserted, so a failure here cannot
	///		leave the shared fixture broken for the rest of the collection.
	/// </remarks>
	[Fact]
	public void ResolvingAKeyThatIsNotDeclaredAnywhereThrowsNamingTheKey()
	{
		var merged = _fixture
			.Application
			.Resources
			.MergedDictionaries
			.ToList();

		merged
			.Should()
			.NotBeEmpty("the fixture must have merged the theme dictionaries for this to detach any");

		try
		{
			_fixture.Application.Resources.MergedDictionaries.Clear();

			_fixture
				.FindResource("DmAccentGoldLight")
				.Should()
				.BeNull("the key must genuinely be undeclared before the resolution is attempted");

			var read = () =>
				DmTheme.Gold;

			read
				.Should()
				.Throw<InvalidOperationException>(
					"a missing theme key is a defect in the theme, not an expected failure a "
					+ "caller handles, so Resolve<T> throws rather than returning a default")
				.WithMessage("*DmAccentGoldLight*");
		}
		finally
		{
			foreach (var dictionary in merged)
			{
				_fixture.Application.Resources.MergedDictionaries.Add(dictionary);
			}
		}

		_fixture
			.FindResource("DmAccentGoldLight")
			.Should()
			.NotBeNull("the fixture is shared by the whole collection and must be left intact");
	}

	/// <summary>
	///		Every public static property <c>DmTheme</c> exposes. Each one is a call site of the
	///		private <c>Resolve&lt;T&gt;</c> with a fixed key and a fixed <c>T</c>, so reading them
	///		all is how this assembly reaches the resolver.
	/// </summary>
	private static IReadOnlyList<PropertyInfo> Accessors()
	{
		return typeof(DmTheme)
			.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
			.Where(property =>
				property.CanRead)
			.OrderBy(
				property =>
					property.Name,
				StringComparer.Ordinal)
			.ToList();
	}

	/// <summary>
	///		Every <c>Dm</c>-prefixed key the live merged <c>Application.Current.Resources</c> holds,
	///		flattened across the merged dictionaries with the first declaration winning, which is
	///		the one the resolver would find.
	/// </summary>
	/// <remarks>
	///		MEASURED, NOT ASSUMED: a <c>ResourceDictionary</c> loaded from a <c>Source</c> — which
	///		all seven theme dictionaries are — reports its entries through <c>Count</c> and
	///		<c>Keys</c> but yields NOTHING from its <c>IEnumerable&lt;KeyValuePair&lt;string,
	///		object&gt;&gt;</c> enumerator. On the first merged dictionary here, <c>Count</c> and
	///		<c>Keys.Count</c> both report 89 while a <c>foreach</c> over it produces 0 items. So the
	///		sweep goes through <c>Keys</c> and the indexer, never through enumeration; a
	///		<c>foreach</c> here would silently collect nothing and make every comparison built on it
	///		vacuous, which is the exact failure this class's guard fact exists to catch — and did.
	/// </remarks>
	private IReadOnlyDictionary<string, object?> LiveDmKeys()
	{
		var keys = new Dictionary<string, object?>(StringComparer.Ordinal);

		Collect(_fixture.Application.Resources);

		return keys;

		void Collect(ResourceDictionary dictionary)
		{
			foreach (var key in dictionary.Keys)
			{
				if (!key.StartsWith(
					"Dm",
					StringComparison.Ordinal))
				{
					continue;
				}

				if (dictionary.TryGetValue(
					key,
					out var value))
				{
					keys.TryAdd(
						key,
						value);
				}
			}

			foreach (var merged in dictionary.MergedDictionaries)
			{
				Collect(merged);
			}
		}
	}
}
