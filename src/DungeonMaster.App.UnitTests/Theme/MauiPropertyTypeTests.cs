using System.Reflection;

using DungeonMaster.App.UnitTests.Fixtures;

using FluentAssertions;

using Microsoft.Maui.Controls;

namespace DungeonMaster.App.UnitTests.Theme;

/// <summary>
///		Re-establishes, by reflection over the resolved <c>Microsoft.Maui.Controls</c> assembly,
///		the property-type tables that
///		<c>DungeonMaster.Tests.App.Theme.TokenTypeMatchesTargetPropertyTests</c> carries as
///		hand-transcribed literals.
/// </summary>
/// <remarks>
///		<para>
///		THE EXPECTED VALUES ARE DELIBERATELY DUPLICATED HERE. They cannot be shared: the table
///		they duplicate lives in <c>DungeonMaster.Tests</c>, which by invariant may not reference
///		<c>DungeonMaster.App</c> and therefore can never see a MAUI type, so no cross-assembly
///		handoff is possible in either direction. The duplication is the point rather than a
///		compromise — here the values sit next to the reflection that checks them, so a MAUI
///		change makes them fail; over there nothing can check them at all, and the sibling alarm
///		<c>TheMauiControlsPackageVersionStillMatchesTheVersionTheTablesWereReadOff</c> is what
///		tells that assembly its copy may have gone stale.
///		</para>
///		<para>
///		Read off <c>Microsoft.Maui.Controls</c> 10.0.41 on 2026-08-07. This class does not assert
///		that literal: it reports the version it actually resolved in every failure message, so a
///		package bump produces a legible diff rather than a stale claim.
///		</para>
///		<para>
///		It joins the collection without injecting <c>AppFixture</c>: it touches
///		<c>Microsoft.Maui.Controls</c> types and runs the application assembly's type
///		initialisers, which is what the collection rule in
///		<c>contracts/app-unit-tests-project.md</c> section 4.2 is about, but it constructs no
///		element and so has nothing to ask the fixture for. The conversion probes next door do,
///		and they go through <c>AppFixture.Construct</c>.
///		</para>
/// </remarks>
[Collection(AppTestCollection.Name)]
public class MauiPropertyTypeTests
{
	/// <summary>
	///		Properties whose declared type depends on the element that owns them — the trap class
	///		the whole guard exists for. Restated from
	///		<c>TokenTypeMatchesTargetPropertyTests.PropertyTypesByElement</c>.
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
	///		Properties whose declared type is the same whatever element carries them. Restated from
	///		<c>TokenTypeMatchesTargetPropertyTests.PropertyTypesByName</c>.
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
	///		The six corner-radius-bearing pairs the table calls the trap class, named here so that
	///		dropping one from <see cref="PropertyTypesByElement"/> fails rather than shrinking the
	///		guard in silence.
	/// </summary>
	private static readonly string[] TrapClassPairs =
	[
		"Button.CornerRadius",
		"ImageButton.CornerRadius",
		"Frame.CornerRadius",
		"BoxView.CornerRadius",
		"RoundRectangle.CornerRadius",
		"Border.StrokeShape",
	];

	/// <summary>
	///		The <c>Microsoft.Maui.Controls</c> assembly as actually resolved at run time, which is
	///		the thing being read. Nothing here trusts a version literal.
	/// </summary>
	private static Assembly MauiControls =>
		typeof(BindableProperty).Assembly;

	/// <summary>
	///		The resolved assembly's version, quoted in every failure message so a bump is legible.
	/// </summary>
	private static string MauiVersion =>
		$"Microsoft.Maui.Controls resolved at run time as "
		+ $"{MauiControls.GetName().Version?.ToString() ?? "an unknown version"} / "
		+ $"{MauiControls.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "no informational version"}";

	/// <summary>
	///		Every element-qualified entry states the type the resolved assembly actually declares.
	/// </summary>
	[Fact]
	public void EveryElementQualifiedPropertyTypeMatchesTheResolvedMauiAssembly()
	{
		var reached = 0;
		var disagreements = new List<string>();

		foreach (var (pair, expected) in PropertyTypesByElement)
		{
			var owner = pair.Split('.')[0];
			var property = pair.Split('.')[1];
			var type = ResolveType(owner);

			if (type is null)
			{
				disagreements.Add($"{pair}: no public type named '{owner}' in the assembly");

				continue;
			}

			var declared = DeclaredPropertyType(
				type,
				property);

			if (declared is null)
			{
				disagreements.Add(
					$"{pair}: '{type.FullName}' declares no property or bindable property "
					+ $"'{property}' (expected {expected})");

				continue;
			}

			reached++;

			if (!string.Equals(
				Normalise(declared),
				expected,
				StringComparison.Ordinal))
			{
				disagreements.Add(
					$"{pair}: the table says {expected}, the assembly declares "
					+ $"{Normalise(declared)} ({declared.FullName})");
			}
		}

		reached
			.Should()
			.BeGreaterThan(
				0,
				$"a reflection check that reached nothing passes exactly like a correct one. {MauiVersion}");

		reached
			.Should()
			.Be(
				PropertyTypesByElement.Count,
				$"every entry must be reached, not merely the ones whose type still resolves. {MauiVersion}");

		disagreements
			.Should()
			.BeEmpty(
				"a hand-transcribed property type that no longer matches the framework makes the "
				+ $"guard in DungeonMaster.Tests state the old truth as a false pass. {MauiVersion}");
	}

	/// <summary>
	///		Every element-independent entry states the type the resolved assembly declares, on
	///		every type that declares it — which is the table's actual claim.
	/// </summary>
	[Fact]
	public void EveryElementIndependentPropertyTypeMatchesTheResolvedMauiAssembly()
	{
		var reached = 0;
		var disagreements = new List<string>();

		foreach (var (name, expected) in PropertyTypesByName)
		{
			var declarations = DeclarationsOf(name);

			if (declarations.Count == 0)
			{
				disagreements.Add(
					$"{name}: no type in the assembly declares it (expected {expected}); a "
					+ "property that stopped resolving is never silently skipped");

				continue;
			}

			reached++;

			foreach (var (owner, declared) in declarations)
			{
				if (!string.Equals(
					Normalise(declared),
					expected,
					StringComparison.Ordinal))
				{
					disagreements.Add(
						$"{name}: the table says {expected}, but {owner}.{name} is declared "
						+ $"{Normalise(declared)} ({declared.FullName})");
				}
			}
		}

		reached
			.Should()
			.BeGreaterThan(
				0,
				$"a reflection check that reached nothing passes exactly like a correct one. {MauiVersion}");

		reached
			.Should()
			.Be(
				PropertyTypesByName.Count,
				$"every entry must be reached, not merely the ones that still resolve. {MauiVersion}");

		disagreements
			.Should()
			.BeEmpty(
				"an element-independent entry that is not element-independent any more is the "
				+ "exact shape of the defect the table was written after — a property whose type "
				+ $"depends on its owner, read as though it did not. {MauiVersion}");
	}

	/// <summary>
	///		All six trap-class pairs are still covered by name.
	/// </summary>
	[Fact]
	public void AllSixCornerRadiusTrapPairsAreStillCoveredByName()
	{
		PropertyTypesByElement
			.Keys
			.Should()
			.BeEquivalentTo(
				TrapClassPairs,
				"the six corner-radius-bearing properties span four types and the two int ones "
				+ "reject a double; dropping one shrinks the guard without failing anything else. "
				+ $"{MauiVersion}");
	}

	/// <summary>
	///		Every <c>BindableProperty</c> the application declares reports a name, a return type
	///		and a declaring type.
	///		<para>
	///		This is MASTER-109's behavioural half, satisfied inside this assembly. It reads the
	///		real declarations rather than their spelling, so it holds under any behaviour-
	///		preserving edit to the source text — which is precisely what a regular expression over
	///		that source text cannot claim.
	///		</para>
	/// </summary>
	[Fact]
	public void EveryBindablePropertyTheApplicationDeclaresReportsItsNameTypeAndOwner()
	{
		var declared = ApplicationBindableProperties();

		declared
			.Should()
			.NotBeEmpty(
				"the application declares bindable properties; reflecting over none of them "
				+ "would let this fact pass while checking nothing");

		var incomplete = declared
			.Where(entry =>
				string.IsNullOrWhiteSpace(entry.Property.PropertyName)
				|| entry.Property.ReturnType is null
				|| entry.Property.DeclaringType is null)
			.Select(entry =>
				$"{entry.Field} reports PropertyName='{entry.Property.PropertyName}', "
				+ $"ReturnType={entry.Property.ReturnType?.FullName ?? "null"}, "
				+ $"DeclaringType={entry.Property.DeclaringType?.FullName ?? "null"}")
			.OrderBy(
				entry =>
					entry,
				StringComparer.Ordinal)
			.ToList();

		incomplete
			.Should()
			.BeEmpty(
				"a bindable property whose metadata is incomplete cannot be type-checked against "
				+ "a token, and the source-text scan that stands in for this in DungeonMaster.Tests "
				+ "would report it as present");
	}

	/// <summary>
	///		Returns every <c>BindableProperty</c> the application assembly declares as a public or
	///		non-public static field, with the field's qualified name for reporting.
	/// </summary>
	private static IReadOnlyList<(string Field, BindableProperty Property)> ApplicationBindableProperties()
	{
		return typeof(DungeonMaster.App.App)
			.Assembly
			.GetTypes()
			.SelectMany(type =>
				type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly))
			.Where(field =>
				field.FieldType == typeof(BindableProperty))
			.Select(field =>
				(Field: $"{field.DeclaringType?.FullName}.{field.Name}",
					Property: (BindableProperty)field.GetValue(null)!))
			.Where(entry =>
				entry.Property is not null)
			.ToList();
	}

	/// <summary>
	///		The types a themed XAML binding can actually target: the MAUI object model proper and
	///		its shapes.
	///		<para>
	///		MEASURED, not assumed. Sweeping every exported type instead reports
	///		<c>Microsoft.Maui.Controls.Internals.GIFHeader.BackgroundColor</c>, an
	///		<c>System.Int32</c> field of a GIF decoder, as a counterexample to
	///		"<c>BackgroundColor</c> is a <c>Color</c>". Nothing in <c>Internals</c> is an element a
	///		<c>{StaticResource Dm…}</c> can bind onto, so it is not part of the claim the table
	///		makes.
	///		</para>
	/// </summary>
	private static IEnumerable<Type> XamlObjectModelTypes()
	{
		return MauiControls
			.GetExportedTypes()
			.Where(type =>
				type.Namespace is "Microsoft.Maui.Controls" or "Microsoft.Maui.Controls.Shapes");
	}

	/// <summary>
	///		Resolves a public type by its simple name in the resolved MAUI assembly.
	/// </summary>
	private static Type? ResolveType(string name)
	{
		return XamlObjectModelTypes()
			.FirstOrDefault(type =>
				string.Equals(
					type.Name,
					name,
					StringComparison.Ordinal));
	}

	/// <summary>
	///		Returns the declared type of a property on a specific owner, preferring the CLR
	///		property and falling back to the attached <c>BindableProperty</c>'s return type.
	/// </summary>
	private static Type? DeclaredPropertyType(
		Type owner,
		string property)
	{
		var clr = owner.GetProperty(
			property,
			BindingFlags.Public | BindingFlags.Instance);

		if (clr is not null)
		{
			return clr.PropertyType;
		}

		var field = owner.GetField(
			$"{property}Property",
			BindingFlags.Public | BindingFlags.Static);

		return field?.GetValue(null) is BindableProperty bindable
			? bindable.ReturnType
			: null;
	}

	/// <summary>
	///		Returns every declaration of a property name in the resolved MAUI assembly. A dotted
	///		name is an attached property and is resolved against its named owner only; a bare name
	///		is swept across every exported type that declares it, which is what makes the "same
	///		type whatever element carries it" claim checkable rather than assumed.
	/// </summary>
	private static IReadOnlyList<(string Owner, Type Declared)> DeclarationsOf(string name)
	{
		if (name.Contains('.', StringComparison.Ordinal))
		{
			var owner = name.Split('.')[0];
			var property = name.Split('.')[1];
			var type = ResolveType(owner);

			if (type is null)
			{
				return [];
			}

			var attached = type.GetField(
				$"{property}Property",
				BindingFlags.Public | BindingFlags.Static);

			return attached?.GetValue(null) is BindableProperty bindable
				? [(owner, bindable.ReturnType)]
				: [];
		}

		return XamlObjectModelTypes()
			.SelectMany(type =>
				type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
					.Where(property =>
						string.Equals(
							property.Name,
							name,
							StringComparison.Ordinal))
					.Select(property =>
						(Owner: type.FullName ?? type.Name, Declared: property.PropertyType)))
			.ToList();
	}

	/// <summary>
	///		Maps a CLR type onto the type vocabulary the transcribed tables are written in.
	/// </summary>
	private static string Normalise(Type type)
	{
		var bare = Nullable.GetUnderlyingType(type) ?? type;

		if (bare == typeof(double))
		{
			return "double";
		}

		if (bare == typeof(int))
		{
			return "int";
		}

		if (bare == typeof(float))
		{
			return "float";
		}

		return bare == typeof(string)
			? "string"
			: bare.Name;
	}
}
