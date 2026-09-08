using System.Reflection;

using DungeonMaster.App.Controls;
using DungeonMaster.App.UnitTests.Fixtures;

using FluentAssertions;

using Microsoft.Maui.Controls;

namespace DungeonMaster.App.UnitTests.Theme;

/// <summary>
///		Drift guard for the bindable properties whose C# defaults mirror a typography token. XAML
///		cannot supply a <c>{StaticResource}</c> as a bindable property's default value, so the size
///		or family name is duplicated in code-behind; these facts fail if either side of the pair
///		moves without the other.
/// </summary>
/// <remarks>
///		<para>
///		THIS REPLACES A PAIR OF REGULAR EXPRESSIONS OVER APPLICATION C#. Its predecessor —
///		<c>DungeonMaster.Tests.App.Theme.BindablePropertyDefaultsMatchTokensTests</c>'s
///		<c>BindableDefault_EqualsTheTokenItMirrors</c> and its family sibling — required the literal
///		sequence <c>nameof(X), typeof(double), typeof(Y), &lt;number&gt;</c> to appear in a named
///		source file, and compared the captured text against a token value parsed out of XAML text.
///		Neither half was a runtime value. Extracting the owner type to an alias, hoisting the
///		default into a <c>const double</c>, or putting the arguments on one line each changed
///		nothing observable and each broke the match; a renamed property broke it silently in the
///		other direction. That is MASTER-109, and MASTER-063's last two <c>new Regex</c> sites went
///		with it.
///		</para>
///		<para>
///		BOTH HALVES ARE NOW REAL RUNTIME VALUES. The left-hand side is the <c>BindableProperty</c>
///		object itself, read by reflection off the static field the compiler emitted, reporting its
///		own <c>PropertyName</c>, <c>ReturnType</c>, <c>DeclaringType</c> and <c>DefaultValue</c>.
///		The right-hand side is <c>AppFixture.FindResource</c>, a lookup in the LIVE merged
///		<c>Application.Current.Resources</c> the running application would read. No spelling of the
///		declaration and no formatting of the XAML can affect either.
///		</para>
///		<para>
///		<c>ThemeSource.DoubleToken</c> and <c>ThemeSource.StringToken</c>, which the predecessor
///		used for the right-hand side, are deliberately NOT used here and could not be: they live in
///		<c>DungeonMaster.Tests</c>, which may not reference <c>DungeonMaster.App</c> and is
///		unreachable from this assembly (contracts/app-unit-tests-project.md section 3.1).
///		</para>
/// </remarks>
[Collection(AppTestCollection.Name)]
public class BindablePropertyDefaultTests
{
	private readonly AppFixture _fixture;

	/// <summary>
	///		Initializes the class with the shared application fixture.
	/// </summary>
	/// <param name="fixture">
	///		The collection fixture supplying the constructed application and its merged resources.
	/// </param>
	public BindablePropertyDefaultTests(AppFixture fixture)
	{
		_fixture = fixture;
	}

	/// <summary>
	///		The numeric bindable defaults under guard. The declaring type is a <see cref="Type"/>
	///		rather than a source path: the fact reads the compiled declaration, so there is nothing
	///		for a file name to point at.
	/// </summary>
	private static readonly MirroredDefault[] NumericMirroredDefaults =
	[
		new(typeof(DmDivider), "IconFontSize", "DmIconSizeMedium"),
		new(typeof(DmInlineEditor), "FontSize", "DmFontSizeBody"),
	];

	/// <summary>
	///		The font-family bindable defaults under guard. A family default drifts exactly the way a
	///		size default does — the string is a second copy of a token's value with nothing tying
	///		the two together — so it wants the same guard rather than a new mechanism.
	/// </summary>
	private static readonly MirroredDefault[] FamilyMirroredDefaults =
	[
		new(typeof(DmInlineEditor), "FontFamily", "DmFontBody"),
	];

	/// <summary>
	///		<see cref="NumericMirroredDefaults"/> as theory data.
	/// </summary>
	public static TheoryData<Type, string, string> Cases =>
		AsTheoryData(NumericMirroredDefaults);

	/// <summary>
	///		<see cref="FamilyMirroredDefaults"/> as theory data.
	/// </summary>
	public static TheoryData<Type, string, string> FamilyCases =>
		AsTheoryData(FamilyMirroredDefaults);

	/// <summary>
	///		Guards the numeric case set. A <c>[Theory]</c> whose <c>MemberData</c> returned nothing
	///		runs zero cases and reports green, and a case whose <c>BindableProperty</c> field no
	///		longer resolves would be a single red case rather than a shrunken guard — so the reached
	///		count is DERIVED here by resolving every case, and compared against the case count.
	/// </summary>
	[Fact]
	public void TheNumericBindableDefaultCaseSetReachesEveryPropertyItNames()
	{
		var reached = ReachedProperties(NumericMirroredDefaults);

		reached
			.Should()
			.BePositive("a theory over a case set that reaches nothing passes while checking nothing");

		reached
			.Should()
			.Be(
				NumericMirroredDefaults.Length,
				"a case set that reaches only some of what it names has silently shrunk");
	}

	/// <summary>
	///		Guards the font-family case set, for the reason given on its numeric sibling.
	/// </summary>
	[Fact]
	public void TheFamilyBindableDefaultCaseSetReachesEveryPropertyItNames()
	{
		var reached = ReachedProperties(FamilyMirroredDefaults);

		reached
			.Should()
			.BePositive("a theory over a case set that reaches nothing passes while checking nothing");

		reached
			.Should()
			.Be(
				FamilyMirroredDefaults.Length,
				"a case set that reaches only some of what it names has silently shrunk");
	}

	/// <summary>
	///		Asserts that the numeric default the real <c>BindableProperty</c> carries equals the
	///		value of the typography token it mirrors, as the live merged dictionaries hold it.
	/// </summary>
	/// <param name="declaringType">
	///		The type that declares the bindable property.
	/// </param>
	/// <param name="propertyName">
	///		The name of the bindable property.
	/// </param>
	/// <param name="tokenKey">
	///		The key of the Typography.xaml token the default mirrors.
	/// </param>
	[Theory]
	[MemberData(nameof(Cases))]
	public void BindableDefault_EqualsTheTokenItMirrors(
		Type declaringType,
		string propertyName,
		string tokenKey)
	{
		var property = BindablePropertyOf(
			declaringType,
			propertyName);

		property
			.PropertyName
			.Should()
			.Be(
				propertyName,
				"the field's own metadata names the property it identifies");

		property
			.ReturnType
			.Should()
			.Be<double>(
				$"'{propertyName}' mirrors the x:Double token '{tokenKey}', and Resolve<T> does no "
				+ "numeric widening");

		property
			.DeclaringType
			.Should()
			.Be(
				declaringType,
				"a bindable property registered against another owner does not apply to this one");

		var token = _fixture.FindResource(tokenKey);

		token
			.Should()
			.BeOfType<double>(
				$"'{tokenKey}' must be a live x:Double in the merged dictionaries for the default "
				+ "to mirror it");

		property
			.DefaultValue
			.Should()
			.Be(
				token,
				$"'{declaringType.Name}.{propertyName}' duplicates '{tokenKey}' because XAML cannot "
				+ "supply a StaticResource as a bindable default; the two must move together");
	}

	/// <summary>
	///		Asserts that the font-family string the real <c>BindableProperty</c> carries equals the
	///		value of the typography token it mirrors, as the live merged dictionaries hold it.
	/// </summary>
	/// <param name="declaringType">
	///		The type that declares the bindable property.
	/// </param>
	/// <param name="propertyName">
	///		The name of the bindable property.
	/// </param>
	/// <param name="tokenKey">
	///		The key of the Typography.xaml token the default mirrors.
	/// </param>
	[Theory]
	[MemberData(nameof(FamilyCases))]
	public void BindableFamilyDefault_EqualsTheTokenItMirrors(
		Type declaringType,
		string propertyName,
		string tokenKey)
	{
		var property = BindablePropertyOf(
			declaringType,
			propertyName);

		property
			.PropertyName
			.Should()
			.Be(
				propertyName,
				"the field's own metadata names the property it identifies");

		property
			.ReturnType
			.Should()
			.Be<string>(
				$"'{propertyName}' mirrors the x:String token '{tokenKey}'");

		property
			.DeclaringType
			.Should()
			.Be(
				declaringType,
				"a bindable property registered against another owner does not apply to this one");

		var token = _fixture.FindResource(tokenKey);

		token
			.Should()
			.BeOfType<string>(
				$"'{tokenKey}' must be a live x:String in the merged dictionaries for the default "
				+ "to mirror it");

		property
			.DefaultValue
			.Should()
			.Be(
				token,
				$"'{declaringType.Name}.{propertyName}' duplicates '{tokenKey}' because XAML cannot "
				+ "supply a StaticResource as a bindable default; the two must move together");
	}

	/// <summary>
	///		Asserts that <c>DefaultValue</c> reports what was registered rather than a value that
	///		merely happens to agree.
	/// </summary>
	/// <remarks>
	///		The whole comparison above rests on <c>BindableProperty.DefaultValue</c> being genuinely
	///		read back. Two registrations in the same file pin it from opposite ends:
	///		<c>DmInlineEditor.Text</c> registers <c>string.Empty</c> explicitly, and
	///		<c>DmInlineEditor.TextColor</c> registers no default at all and must therefore report
	///		<see langword="null"/>. A <c>DefaultValue</c> that returned the type's own default for
	///		everything would fail the first; one that returned some ambient non-null would fail the
	///		second. This is the spike's measurement, kept as a fact.
	/// </remarks>
	[Fact]
	public void DefaultValueReportsTheRegisteredDefaultAndNotACoincidence()
	{
		BindablePropertyOf(
				typeof(DmInlineEditor),
				"Text")
			.DefaultValue
			.Should()
			.Be(
				string.Empty,
				"DmInlineEditor.Text registers string.Empty as its default, and an empty string is "
				+ "not the same reading as no default at all");

		BindablePropertyOf(
				typeof(DmInlineEditor),
				"TextColor")
			.DefaultValue
			.Should()
			.BeNull(
				"DmInlineEditor.TextColor registers no default, so DefaultValue must report null "
				+ "rather than an ambient value that would make every comparison above pass");
	}

	/// <summary>
	///		Returns the <c>BindableProperty</c> a type declares for a property name, failing with a
	///		message that names the field it looked for when the declaration is not there.
	/// </summary>
	private static BindableProperty BindablePropertyOf(
		Type declaringType,
		string propertyName)
	{
		var field = BindablePropertyField(
			declaringType,
			propertyName);

		field
			.Should()
			.NotBeNull(
				$"'{declaringType.FullName}' must declare a public static BindableProperty field "
				+ $"'{propertyName}Property'");

		return (BindableProperty)field!.GetValue(null)!;
	}

	/// <summary>
	///		Returns the static field that identifies a bindable property, or <see langword="null"/>
	///		when the type declares none.
	/// </summary>
	private static FieldInfo? BindablePropertyField(
		Type declaringType,
		string propertyName)
	{
		var field = declaringType.GetField(
			$"{propertyName}Property",
			BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

		return field?.FieldType == typeof(BindableProperty)
			? field
			: null;
	}

	/// <summary>
	///		Returns how many of a case set's named properties actually resolve to a
	///		<c>BindableProperty</c> field. Derived by resolving them, never counted from the table.
	/// </summary>
	private static int ReachedProperties(IReadOnlyList<MirroredDefault> cases)
	{
		return cases.Count(mirrored =>
			BindablePropertyField(
				mirrored.DeclaringType,
				mirrored.PropertyName) is not null);
	}

	/// <summary>
	///		Projects a case set onto the theory data shape xunit needs, so the set is declared once.
	/// </summary>
	private static TheoryData<Type, string, string> AsTheoryData(IReadOnlyList<MirroredDefault> cases)
	{
		var data = new TheoryData<Type, string, string>();

		foreach (var mirrored in cases)
		{
			data.Add(
				mirrored.DeclaringType,
				mirrored.PropertyName,
				mirrored.TokenKey);
		}

		return data;
	}

	/// <summary>
	///		A bindable property default that duplicates a theme token's value.
	/// </summary>
	/// <param name="DeclaringType">
	///		The type that declares the bindable property.
	/// </param>
	/// <param name="PropertyName">
	///		The name of the bindable property.
	/// </param>
	/// <param name="TokenKey">
	///		The key of the token the default mirrors.
	/// </param>
	private sealed record MirroredDefault(
		Type DeclaringType,
		string PropertyName,
		string TokenKey);
}
