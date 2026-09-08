using System.Reflection;

using DungeonMaster.App.UnitTests.Fixtures;

using FluentAssertions;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace DungeonMaster.App.UnitTests.Theme;

/// <summary>
///		Re-measures the conversion matrix that
///		<c>DungeonMaster.Tests.App.Theme.TokenTypeMatchesTargetPropertyTests.WideningConversions</c>
///		carries as a hand-transcribed table, by doing what the original measurement did: calling
///		<c>SetValue</c> with a boxed value of the source type on a real MAUI property and reading
///		the property back.
/// </summary>
/// <remarks>
///		<para>
///		THE EXPECTED VALUES ARE DELIBERATELY DUPLICATED HERE, for the reason given on
///		<see cref="MauiPropertyTypeTests"/>: the table lives in an assembly that may not see a
///		MAUI type, so it cannot be shared and it cannot be checked over there.
///		</para>
///		<para>
///		BOTH DIRECTIONS ARE COVERED AND THAT IS THE POINT. int widens to everything and double
///		narrows to nothing; a test that established only the seven accepted conversions would
///		prove half of an asymmetry whose other half is the defect — a themed
///		<c>Button.CornerRadius</c> silently dropped on every evaluation.
///		</para>
/// </remarks>
[Collection(AppTestCollection.Name)]
public class MauiConversionMatrixTests
{
	/// <summary>
	///		The probe value, as in the original measurement.
	/// </summary>
	private const int Probe = 7;

	/// <summary>
	///		The conversions the table records as ACCEPTED, other than the identity. Restated from
	///		<c>TokenTypeMatchesTargetPropertyTests.WideningConversions</c>.
	/// </summary>
	private static readonly string[] AcceptedConversions =
	[
		"int->double",
		"int->float",
		"int->CornerRadius",
		"int->Thickness",
		"double->CornerRadius",
		"double->Thickness",
		"Color->Brush",
	];

	/// <summary>
	///		The conversions the table's own doc comment records as DROPPED — the deliberate
	///		absences from <c>WideningConversions</c>. An absence nothing re-measures is a claim,
	///		not a measurement.
	/// </summary>
	private static readonly string[] DroppedConversions =
	[
		"double->int",
		"double->float",
		"string->int",
		"string->double",
		"string->CornerRadius",
		"string->Thickness",
	];

	/// <summary>
	///		The colour the <c>Color-&gt;Brush</c> probe uses. Not a named colour, so that reading
	///		back some other brush cannot pass by coincidence.
	/// </summary>
	private static readonly Color ProbeColor = Color.FromRgb(
		17,
		34,
		51);

	private readonly AppFixture _fixture;

	/// <summary>
	///		Initializes a new instance of <see cref="MauiConversionMatrixTests"/>.
	/// </summary>
	/// <param name="fixture">The collection fixture.</param>
	public MauiConversionMatrixTests(AppFixture fixture)
	{
		_fixture = fixture;
	}

	/// <summary>
	///		The resolved MAUI assembly, quoted in failures rather than asserted against a literal.
	/// </summary>
	private static string MauiVersion =>
		$"Microsoft.Maui.Controls resolved at run time as "
		+ $"{typeof(BindableProperty).Assembly.GetName().Version?.ToString() ?? "an unknown version"}";

	/// <summary>
	///		Every conversion the table records as accepted still lands its value.
	/// </summary>
	[Fact]
	public void EveryConversionTheTableRecordsAsAcceptedStillLandsItsValue()
	{
		var reached = 0;
		var failures = new List<string>();

		foreach (var conversion in AcceptedConversions)
		{
			var source = SourceOf(conversion);
			var target = TargetOf(conversion);
			var property = PropertyFor(target);
			var owner = OwnerFor(target);
			var expected = ExpectedValueFor(target);
			var before = owner.GetValue(property);

			if (Matches(
				target,
				before))
			{
				failures.Add(
					$"{conversion}: the probe cannot discriminate — the property already reads "
					+ $"{Describe(before)} before anything is set, which is the value the "
					+ "conversion would produce");

				continue;
			}

			owner.SetValue(
				property,
				ValueOf(source));

			reached++;

			var actual = owner.GetValue(property);

			if (!Matches(
				target,
				actual))
			{
				failures.Add(
					$"{conversion}: the table says MAUI accepts it, but the property read back "
					+ $"{Describe(actual)} rather than {Describe(expected)}");
			}
		}

		reached
			.Should()
			.BeGreaterThan(
				0,
				$"a probe that reached no pair passes exactly like a correct one. {MauiVersion}");

		reached
			.Should()
			.Be(
				AcceptedConversions.Length,
				$"every accepted conversion must be probed, not merely the reachable ones. {MauiVersion}");

		failures
			.Should()
			.BeEmpty(
				"the accepted half of the matrix is what lets a themed token reach an int-typed "
				+ $"property at all. {MauiVersion}");
	}

	/// <summary>
	///		Every conversion the table deliberately omits is still dropped, leaving the property
	///		exactly as it was.
	///		<para>
	///		THE COMPARISON IS AGAINST THE VALUE READ BEFORE THE SET, NOT AGAINST
	///		<c>BindableProperty.DefaultValue</c>, AND THAT IS A MEASUREMENT RATHER THAN A
	///		PREFERENCE. Under <c>AppFixture</c> the application's merged theme dictionaries are
	///		live, so the implicit <c>Button</c> style in <c>Implicit.xaml</c> has already set
	///		<c>CornerRadius</c> to <c>DmRadiusLoose</c> (4) on a freshly constructed
	///		<c>Button</c>; the MAUI default of <c>-1</c> is never observed here. Asserting the
	///		default would fail over correct behaviour. "Unchanged" is also the stronger claim: it
	///		is what "the value is dropped" actually means.
	///		</para>
	/// </summary>
	[Fact]
	public void EveryConversionTheTableDeliberatelyOmitsIsStillDropped()
	{
		var reached = 0;
		var failures = new List<string>();

		foreach (var conversion in DroppedConversions)
		{
			if (AcceptedConversions.Contains(
				conversion,
				StringComparer.Ordinal))
			{
				failures.Add(
					$"{conversion}: named as a deliberate absence and present in the accepted "
					+ "table at the same time");

				continue;
			}

			var source = SourceOf(conversion);
			var target = TargetOf(conversion);
			var property = PropertyFor(target);
			var owner = OwnerFor(target);
			var before = owner.GetValue(property);

			owner.SetValue(
				property,
				ValueOf(source));

			reached++;

			var actual = owner.GetValue(property);

			if (!Equals(
				actual,
				before))
			{
				failures.Add(
					$"{conversion}: the table records it as dropped, but setting it changed the "
					+ $"property from {Describe(before)} to {Describe(actual)}");
			}

			if (Matches(
				target,
				actual))
			{
				failures.Add(
					$"{conversion}: the property read back {Describe(actual)}, which is exactly "
					+ "the value the conversion would have produced had MAUI accepted it");
			}
		}

		reached
			.Should()
			.BeGreaterThan(
				0,
				$"a probe that reached no pair passes exactly like a correct one. {MauiVersion}");

		reached
			.Should()
			.Be(
				DroppedConversions.Length,
				$"every deliberate absence must be probed. {MauiVersion}");

		failures
			.Should()
			.BeEmpty(
				"double narrows to nothing, and that is the half of the asymmetry the radius "
				+ $"defect was made of. {MauiVersion}");
	}

	/// <summary>
	///		The matrix this class re-measures is the size the transcribed table states, so a pair
	///		quietly dropped from either half fails here rather than shrinking the probe.
	/// </summary>
	[Fact]
	public void TheProbedMatrixIsTheSizeTheTranscribedTableStates()
	{
		AcceptedConversions
			.Should()
			.HaveCount(
				7,
				$"the table records seven accepted conversions. {MauiVersion}");

		DroppedConversions
			.Should()
			.HaveCount(
				6,
				$"the table's doc comment names six dropped conversions. {MauiVersion}");

		AcceptedConversions
			.Intersect(
				DroppedConversions,
				StringComparer.Ordinal)
			.Should()
			.BeEmpty(
				$"a conversion cannot be both accepted and deliberately absent. {MauiVersion}");
	}

	private static string SourceOf(string conversion)
	{
		return conversion.Split("->")[0];
	}

	private static string TargetOf(string conversion)
	{
		return conversion.Split("->")[1];
	}

	/// <summary>
	///		Returns a boxed value of the named source type, as the original measurement used.
	/// </summary>
	private static object ValueOf(string source)
	{
		return source switch
		{
			"int" => Probe,
			"double" => (double)Probe,
			"string" => Probe.ToString(System.Globalization.CultureInfo.InvariantCulture),
			"Color" => ProbeColor,
			_ => throw new ArgumentOutOfRangeException(
				nameof(source),
				source,
				"no probe value is defined for this source type"),
		};
	}

	/// <summary>
	///		Returns the real MAUI property standing for the named target type.
	/// </summary>
	private static BindableProperty PropertyFor(string target)
	{
		return target switch
		{
			"double" => VisualElement.HeightRequestProperty,
			"float" => FrameCornerRadiusProperty(),
			"int" => Button.CornerRadiusProperty,
			"CornerRadius" => BoxView.CornerRadiusProperty,
			"Thickness" => View.MarginProperty,
			"Brush" => Border.StrokeProperty,
			_ => throw new ArgumentOutOfRangeException(
				nameof(target),
				target,
				"no MAUI property is defined for this target type"),
		};
	}

	/// <summary>
	///		Returns the value the accepted conversion is expected to produce.
	/// </summary>
	private static object ExpectedValueFor(string target)
	{
		return target switch
		{
			"double" => (double)Probe,
			"float" => (float)Probe,
			"int" => Probe,
			"CornerRadius" => new CornerRadius(Probe),
			"Thickness" => new Thickness(Probe),
			"Brush" => ProbeColor,
			_ => throw new ArgumentOutOfRangeException(
				nameof(target),
				target,
				"no expected value is defined for this target type"),
		};
	}

	/// <summary>
	///		<c>Frame</c> is obsolete, so its bindable property is reached by reflection rather than
	///		by a symbol reference that would fail the build under TreatWarningsAsErrors. It stays
	///		in the matrix because <c>Frame.CornerRadius</c> is <c>System.Single</c> and is the rest
	///		of the trap class the transcribed table records.
	/// </summary>
	private static BindableProperty FrameCornerRadiusProperty()
	{
		var field = FrameType()
			.GetField(
				"CornerRadiusProperty",
				BindingFlags.Public | BindingFlags.Static);

		return field?.GetValue(null) as BindableProperty
			?? throw new InvalidOperationException(
				"Microsoft.Maui.Controls.Frame declares no public static CornerRadiusProperty; "
				+ "the float half of the conversion matrix has no probe.");
	}

	private static Type FrameType()
	{
		return typeof(BindableProperty)
			.Assembly
			.GetType("Microsoft.Maui.Controls.Frame")
			?? throw new InvalidOperationException(
				"Microsoft.Maui.Controls declares no type named Frame; the float half of the "
				+ "conversion matrix has no probe.");
	}

	private static string Describe(object? value)
	{
		return value is null
			? "null"
			: $"{value} ({value.GetType().Name})";
	}

	/// <summary>
	///		Returns a fresh MAUI instance carrying the property that stands for the named target
	///		type. Every instance comes through <see cref="AppFixture.Construct{T}"/>, so a
	///		construction failure is reported as the resource or element it could not resolve. A
	///		fresh instance per probe is what lets the dropped half compare against the default.
	/// </summary>
	private BindableObject OwnerFor(string target)
	{
		return target switch
		{
			"double" => _fixture.Construct(() =>
				new BoxView()),
			"float" => _fixture.Construct(() =>
				(Element)Activator.CreateInstance(FrameType())!),
			"int" => _fixture.Construct(() =>
				new Button()),
			"CornerRadius" => _fixture.Construct(() =>
				new BoxView()),
			"Thickness" => _fixture.Construct(() =>
				new BoxView()),
			"Brush" => _fixture.Construct(() =>
				new Border()),
			_ => throw new ArgumentOutOfRangeException(
				nameof(target),
				target,
				"no MAUI owner is defined for this target type"),
		};
	}

	private static bool Matches(
		string target,
		object? actual)
	{
		if (string.Equals(
			target,
			"Brush",
			StringComparison.Ordinal))
		{
			return actual is SolidColorBrush brush
				&& brush.Color == ProbeColor;
		}

		return Equals(
			actual,
			ExpectedValueFor(target));
	}
}
