using System.Xml.Linq;
using FluentAssertions;

namespace DungeonMaster.Tests.App.Theme;

/// <summary>
///		Guards Implicit.xaml, the geometry-and-states floor.
///		Every other fact in this suite is key-based — it reads <c>x:Key="Dm…"</c> — and an implicit
///		style is <c>TargetType</c>-only, so until these facts existed the whole file was unguarded:
///		deleting the Button style's <c>MinimumHeightRequest</c> setter reproduced the measured Task
///		10 regression (every unstyled Button dropping below the 44px touch target app-wide, and the
///		search Entry shrinking from 51px to 42px) with all 256 tests green. The file exists BECAUSE
///		that regression happened, which is exactly why its contents need a guard rather than a
///		comment.
///		<para>
///		The scan reads the parsed document from <see cref="ThemeSource.ThemeDocuments"/> rather than
///		matching regular expressions against XAML text. The pattern it replaces closed a style body
///		on the FIRST <c>&lt;/Style&gt;</c> with <c>.*?</c>, so a nested style mis-terminated the body
///		and attributed its setters to the wrong target;
///		<see cref="NestedStyleSettersAreAttributedToTheDeclaringStyle"/> is the fact that pins the
///		difference. XML cannot mis-terminate: an element closes where the parser says it closes.
///		</para>
/// </summary>
public class ImplicitStyleTests
{
	/// <summary>
	///		The dictionary this class scans. Named once so the whole class has ONE scan source, and
	///		so redirecting that source is a one-line edit inside this file.
	/// </summary>
	private const string ImplicitDictionaryName = "Implicit.xaml";

	/// <summary>
	///		A resource dictionary containing a style nested inside another style's setter value.
	///		Written here rather than read from disk so the fact does not depend on the application
	///		ever growing one.
	/// </summary>
	private const string NestedStyleDocument =
		"""
		<ResourceDictionary
			xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">
			<Style TargetType="Outer">
				<Setter Property="OuterBeforeTheNestedStyle" Value="1" />
				<Setter Property="NestedStyleHolder">
					<Setter.Value>
						<Style TargetType="Inner">
							<Setter Property="InnerOnly" Value="2" />
						</Style>
					</Setter.Value>
				</Setter>
				<Setter Property="OuterAfterTheNestedStyle" Value="3" />
			</Style>
		</ResourceDictionary>
		""";

	/// <summary>
	///		A resource dictionary whose one style holds a MAUI-namespace setter and a same-named
	///		setter in a <c>clr-namespace:</c> namespace. Only the first is a MAUI setter.
	/// </summary>
	private const string ForeignSetterDocument =
		"""
		<ResourceDictionary
			xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			xmlns:local="clr-namespace:DungeonMaster.Synthetic">
			<Style TargetType="Holder">
				<Setter Property="Kept" Value="1" />
				<local:Setter Property="Foreign" Value="2" />
			</Style>
		</ResourceDictionary>
		""";

	/// <summary>
	///		A resource dictionary whose one style holds a MAUI-namespace visual state group and
	///		state, and a same-named group and state in a <c>clr-namespace:</c> namespace. Only the
	///		first pair is MAUI.
	/// </summary>
	private const string ForeignVisualStateDocument =
		"""
		<ResourceDictionary
			xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			xmlns:local="clr-namespace:DungeonMaster.Synthetic">
			<Style TargetType="Holder">
				<VisualStateGroup x:Name="KeptGroup">
					<VisualState x:Name="KeptState" />
				</VisualStateGroup>
				<local:VisualStateGroup x:Name="ForeignGroup">
					<local:VisualState x:Name="ForeignState" />
				</local:VisualStateGroup>
			</Style>
		</ResourceDictionary>
		""";

	/// <summary>
	///		A resource dictionary holding a MAUI-namespace style and a same-named style in a
	///		<c>clr-namespace:</c> namespace. Only the first is a MAUI style.
	/// </summary>
	private const string ForeignStyleDocument =
		"""
		<ResourceDictionary
			xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			xmlns:local="clr-namespace:DungeonMaster.Synthetic">
			<Style TargetType="Kept" />
			<local:Style TargetType="Foreign" />
		</ResourceDictionary>
		""";

	/// <summary>
	///		The control-level properties Implicit.xaml must NOT set. This is the invariant the file
	///		states at the top of itself: colour and font belong exclusively to the keyed Dm* styles
	///		in Controls.Base.xaml and its derivatives, and setting either here as well would give
	///		the app two sources of truth for the same property.
	/// </summary>
	private static readonly string[] ForbiddenControlProperties =
	[
		"TextColor",
		"BackgroundColor",
		"FontSize",
	];

	/// <summary>
	///		The two targets <see cref="NestedStyleDocument"/> declares. The regular expression this
	///		class replaces reported only the first of them.
	/// </summary>
	private static readonly string[] NestedDocumentTargets =
	[
		"Outer",
		"Inner",
	];

	/// <summary>
	///		The setters the outer style of <see cref="NestedStyleDocument"/> owns.
	/// </summary>
	private static readonly string[] OuterStyleSetters =
	[
		"OuterBeforeTheNestedStyle",
		"NestedStyleHolder",
		"OuterAfterTheNestedStyle",
	];

	/// <summary>
	///		The setter the nested style of <see cref="NestedStyleDocument"/> owns.
	/// </summary>
	private static readonly string[] InnerStyleSetters =
	[
		"InnerOnly",
	];

	/// <summary>
	///		The styles that carry colour by design and are therefore outside the invariant above.
	///		Page and Shell are app chrome with exactly one instance each and no keyed alternative
	///		to defer to, so the rule is about the control-level styles and not about the file.
	/// </summary>
	private static readonly string[] ChromeTargets =
	[
		"Page",
		"Shell",
	];

	/// <summary>
	///		Gets the targets that must have an implicit style, and the setters each must carry.
	/// </summary>
	public static TheoryData<string, string[]> RequiredStyles =>
		new()
		{
			{
				"Button",
				[
					"Padding",
					"MinimumHeightRequest",
					"MinimumWidthRequest",
					"CornerRadius",
					"BorderWidth",
				]
			},
			{ "Entry", ["MinimumHeightRequest"] },
			{ "Editor", ["MinimumHeightRequest"] },
			{ "Picker", ["MinimumHeightRequest"] },
			{ "Label", ["FontFamily"] },
			{ "Page", ["BackgroundColor"] },
			{ "Shell", ["Shell.BackgroundColor"] },
		};

	/// <summary>
	///		Asserts that the scan this class is built on reaches at least one style.
	///		<para>
	///		This is the guard that used to live inside <c>Styles()</c>, which is an Arrange helper
	///		and therefore reported a broken scan against whichever fact called it first. It is a
	///		named fact now, per contracts/anti-vacuity.md section 2: every other fact in this class
	///		examines nothing if this one fails, and this one names the reason.
	///		</para>
	/// </summary>
	[Fact]
	public void TheImplicitStyleScanFindsAtLeastOneStyle()
	{
		Styles()
			.Should()
			.NotBeEmpty(
				"Implicit.xaml must declare styles or every fact in this class examines nothing");
	}

	/// <summary>
	///		Asserts that Implicit.xaml declares a style for the required target and that the style
	///		carries every setter the target depends on it for.
	/// </summary>
	/// <param name="target">
	///		The style's <c>TargetType</c>.
	/// </param>
	/// <param name="requiredSetters">
	///		The properties the style must set.
	/// </param>
	[Theory]
	[MemberData(nameof(RequiredStyles))]
	public void RequiredImplicitStyleExistsAndDeclaresItsRequiredSetters(
		string target,
		string[] requiredSetters)
	{
		var styles = Styles();

		styles
			.Should()
			.ContainKey(
				target,
				"Implicit.xaml is the only floor under unstyled {0} elements; without this style "
				+ "they take nothing at all",
				target);

		var declared = SetterProperties(styles[target]);

		foreach (var setter in requiredSetters)
		{
			declared
				.Should()
				.Contain(
					setter,
					"the implicit {0} style must set {1}; MAUI supplies no default for it, so "
					+ "removing the setter silently changes the rendered geometry of every "
					+ "unstyled {0} in the app",
					target,
					setter);
		}
	}

	/// <summary>
	///		Asserts that the implicit Button style carries the interaction states it was written
	///		for. Task 4b measured only padding and minimum size and missed the state group entirely,
	///		leaving every unstyled Button with no hover or press feedback at all — 16 of the app's
	///		20 Button elements today; the values here are byte-identical to the stock ones this file
	///		replaces.
	/// </summary>
	[Fact]
	public void ImplicitButtonStyleCarriesItsCommonStates()
	{
		var styles = Styles();

		styles
			.Should()
			.ContainKey(
				"Button",
				"the implicit Button style is the only thing carrying interaction states for the "
				+ "16 of 20 Button elements that name no Style");

		var button = styles["Button"];

		XamlNames(
			button,
			"VisualStateGroup")
			.Should()
			.Contain(
				"CommonStates",
				"without the CommonStates group every Button that carries no Style= has no hover "
				+ "or press feedback at all");

		var states = XamlNames(
			button,
			"VisualState");

		foreach (var state in new[] { "Normal", "PointerOver", "Pressed" })
		{
			states
				.Should()
				.Contain(
					state,
					"the {0} state is one of the three the stock Button style carried",
					state);
		}
	}

	/// <summary>
	///		Asserts that no control-level implicit style sets colour or font size. Page and Shell
	///		are excluded because they are app chrome and carry colour tokens by design; the Label
	///		style's single FontFamily setter is the file's one deliberate exception and is not a
	///		member of the forbidden set.
	/// </summary>
	[Fact]
	public void ImplicitControlStylesSetNoColourOrFontSize()
	{
		var declared = Styles()
			.Where(entry =>
				!ChromeTargets.Contains(entry.Key))
			.SelectMany(entry =>
				SetterProperties(entry.Value)
					.Select(property =>
						(Target: entry.Key, Property: property)))
			.ToList();

		// The offenders list below is empty both when every control style is clean and when the
		// scan read no setter at all, and those two states must not look the same from outside.
		// Guarding the STYLE scan alone would not catch the second: a reader that finds the styles
		// and none of their setters passes this fact while examining nothing.
		declared
			.Should()
			.NotBeEmpty(
				"an empty offenders list proves nothing unless the scan that produced it read at "
				+ "least one setter off at least one control-level style");

		var offenders = declared
			.Where(entry =>
				ForbiddenControlProperties.Contains(entry.Property))
			.Select(entry =>
				$"{entry.Target} -> {entry.Property}")
			.ToList();

		offenders
			.Should()
			.BeEmpty(
				"colour and font size belong exclusively to the keyed Dm* styles in "
				+ "Controls.Base.xaml and its derivatives; setting either here as well gives the "
				+ "app two sources of truth for the same property");
	}

	/// <summary>
	///		Asserts that a setter inside a NESTED style is attributed to the nested style and not to
	///		the style that encloses it.
	///		<para>
	///		This is the fact that pins the defect the regular expression this class used to carry
	///		could not avoid. That pattern was
	///		<c>&lt;Style\b[^&gt;]*TargetType="(?&lt;target&gt;[A-Za-z0-9]+)"[^&gt;]*&gt;(?&lt;body&gt;.*?)&lt;/Style&gt;</c>
	///		with <c>Singleline</c>: the lazy body closed on the FIRST <c>&lt;/Style&gt;</c>, which
	///		for the document below is the INNER one. The outer style therefore swallowed the inner
	///		style's setter, lost every setter written after the nested style, and the inner style
	///		never appeared as a target at all — three wrong answers from one mis-termination.
	///		</para>
	///		<para>
	///		Implicit.xaml carries no nested style today, so this fact is written against a synthetic
	///		document rather than against the file: the point is that the reader CANNOT produce the
	///		wrong answer, not that the file happens not to provoke it.
	///		</para>
	/// </summary>
	[Fact]
	public void NestedStyleSettersAreAttributedToTheDeclaringStyle()
	{
		var styles = StylesIn(XDocument.Parse(NestedStyleDocument));

		styles
			.Keys
			.Should()
			.BeEquivalentTo(
				NestedDocumentTargets,
				"a nested style is a style in its own right; the pattern this reader replaces "
				+ "consumed the inner element's closing tag as the outer element's, so the inner "
				+ "target was never reported at all");

		SetterProperties(styles["Outer"])
			.Should()
			.BeEquivalentTo(
				OuterStyleSetters,
				"the outer style owns the setters written before AND after the nested style, and "
				+ "owns none of the nested style's own");

		SetterProperties(styles["Inner"])
			.Should()
			.BeEquivalentTo(
				InnerStyleSetters,
				"the nested style owns exactly its own setter");
	}

	/// <summary>
	///		Asserts that <c>SetterProperties</c> reads a MAUI-namespace <c>Setter</c> and skips a
	///		same-named element declared in a <c>clr-namespace:</c> namespace.
	/// </summary>
	[Fact]
	public void SetterPropertiesReadsOnlyMauiNamespaceSetters()
	{
		var style = SyntheticStyle(ForeignSetterDocument);

		SetterProperties(style)
			.Should()
			.Equal(
				["Kept"],
				"only the MAUI-namespace Setter is a setter; the clr-namespace: element of the "
				+ "same local name is a different type");
	}

	/// <summary>
	///		Asserts that <c>XamlNames</c> reads the <c>x:Name</c> of MAUI-namespace visual state
	///		groups and states and skips same-named elements declared in a <c>clr-namespace:</c>
	///		namespace, for both element names its callers pass.
	/// </summary>
	[Fact]
	public void XamlNamesReadsOnlyMauiNamespaceElements()
	{
		var style = SyntheticStyle(ForeignVisualStateDocument);

		XamlNames(
			style,
			"VisualStateGroup")
			.Should()
			.Equal(
				["KeptGroup"],
				"only the MAUI-namespace VisualStateGroup is a visual state group");

		XamlNames(
			style,
			"VisualState")
			.Should()
			.Equal(
				["KeptState"],
				"only the MAUI-namespace VisualState is a visual state");
	}

	/// <summary>
	///		Asserts that <c>StylesIn</c> returns a MAUI-namespace <c>Style</c> and skips a
	///		same-named element declared in a <c>clr-namespace:</c> namespace.
	/// </summary>
	[Fact]
	public void StylesInReadsOnlyMauiNamespaceStyles()
	{
		StylesIn(XDocument.Parse(ForeignStyleDocument))
			.Keys
			.Should()
			.Equal(
				["Kept"],
				"only the MAUI-namespace Style is a style; the clr-namespace: element of the same "
				+ "local name is a different type");
	}

	/// <summary>
	///		Returns every style declared by the dictionary this class scans, keyed by
	///		<c>TargetType</c>. Asserts nothing and returns an empty dictionary when the scan reaches
	///		no such dictionary; <see cref="TheImplicitStyleScanFindsAtLeastOneStyle"/> is the guard.
	/// </summary>
	private static Dictionary<string, XElement> Styles()
	{
		var dictionary = ThemeSource
			.ThemeDocuments()
			.FirstOrDefault(document =>
				string.Equals(
					Path.GetFileName(document.Path),
					ImplicitDictionaryName,
					StringComparison.Ordinal));

		return dictionary is null
			? new Dictionary<string, XElement>(StringComparer.Ordinal)
			: StylesIn(dictionary.Document);
	}

	/// <summary>
	///		Returns every style in the supplied document, keyed by <c>TargetType</c>. Separate from
	///		<see cref="Styles"/> so the same reader can be pointed at a synthetic document.
	/// </summary>
	/// <param name="document">
	///		The parsed resource dictionary to read.
	/// </param>
	private static Dictionary<string, XElement> StylesIn(XDocument document)
	{
		return document
			.Descendants()
			.Where(IsStyle)
			.Where(element =>
				element.Attribute("TargetType") is not null)
			.ToDictionary(
				element =>
					element.Attribute("TargetType")!.Value,
				element =>
					element,
				StringComparer.Ordinal);
	}

	/// <summary>
	///		Returns the first MAUI-namespace <c>Style</c> directly under the root of the supplied
	///		synthetic document. Reads the element by its expanded name rather than through
	///		<see cref="IsStyle"/>, so a fact about one filter does not also depend on another.
	/// </summary>
	/// <param name="xaml">
	///		The synthetic resource dictionary to parse.
	/// </param>
	private static XElement SyntheticStyle(string xaml)
	{
		return XDocument
			.Parse(xaml)
			.Root!
			.Element(
				XName.Get(
					"Style",
					ThemeSource.MauiNamespace))!;
	}

	/// <summary>
	///		Returns the properties the supplied style sets, including those set inside its visual
	///		states, and EXCLUDING those belonging to a style nested inside it. "Belonging" is
	///		decided by the nearest enclosing style, which is exactly the thing the regular
	///		expression this replaces could not determine. Only a <c>Setter</c> element in the MAUI
	///		default namespace counts; a same-named element in any other namespace is not a setter.
	/// </summary>
	/// <param name="style">
	///		The style element to read.
	/// </param>
	private static IReadOnlyList<string> SetterProperties(XElement style)
	{
		return style
			.Descendants()
			.Where(element =>
				ThemeSource.IsMauiElement(
					element,
					"Setter"))
			.Where(element =>
				NearestStyle(element) == style)
			.Select(element =>
				element.Attribute("Property")?.Value)
			.Where(property =>
				property is not null)
			.Select(property =>
				property!)
			.ToList();
	}

	/// <summary>
	///		Returns the <c>x:Name</c> of every descendant of the supplied style that is the named
	///		element in the MAUI default namespace. A same-named element in any other namespace is
	///		not read.
	/// </summary>
	/// <param name="style">
	///		The style element to read.
	/// </param>
	/// <param name="localName">
	///		The bare local name of the element to match, such as <c>VisualState</c>; the element
	///		must be in the MAUI default namespace, <see cref="ThemeSource.MauiNamespace"/>.
	/// </param>
	private static IReadOnlyList<string> XamlNames(
		XElement style,
		string localName)
	{
		return style
			.Descendants()
			.Where(element =>
				ThemeSource.IsMauiElement(
					element,
					localName))
			.Select(element =>
				element
					.Attributes()
					.FirstOrDefault(attribute =>
						ThemeSource.QualifiedElementName(attribute.Name) == "x:Name")
					?.Value)
			.Where(name =>
				name is not null)
			.Select(name =>
				name!)
			.ToList();
	}

	/// <summary>
	///		Returns the innermost style enclosing the supplied element, or <see langword="null"/>
	///		when no style encloses it.
	/// </summary>
	/// <param name="element">
	///		The element whose enclosing style is wanted.
	/// </param>
	private static XElement? NearestStyle(XElement element)
	{
		return element
			.Ancestors()
			.FirstOrDefault(IsStyle);
	}

	/// <summary>
	///		Returns true when the supplied element is a <c>Style</c> in the MAUI default namespace.
	///		A same-named element in any other namespace is not a style.
	/// </summary>
	/// <param name="element">
	///		The element to test.
	/// </param>
	private static bool IsStyle(XElement element)
	{
		return ThemeSource.IsMauiElement(
			element,
			"Style");
	}
}
