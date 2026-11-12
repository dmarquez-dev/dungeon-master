using System.Globalization;
using System.Xml.Linq;
using FluentAssertions;

namespace DungeonMaster.Tests.App.Theme;

/// <summary>
///		Parity ratchet between the keyed <c>DmIconButton</c> style in <c>Controls.Base.xaml</c>
///		and the implicit <c>Button</c> style in <c>Implicit.xaml</c>.
///		<para>
///		<c>DmIconButton</c> exists so that icon-font Buttons can be narrower than the 44-DIU
///		width floor the implicit style sets, while keeping its 44 height. An explicit
///		<c>Style=</c> REPLACES the implicit style entirely, so every other setter of the implicit
///		style is restated by hand in <c>DmIconButton</c>: Padding, MinimumHeightRequest,
///		CornerRadius, BorderWidth and the <c>CommonStates</c> opacities. Nothing else notices when
///		the two copies drift, so a retune of the implicit hover opacity would silently leave the
///		icon Buttons behind. Each fact below therefore compares PARSED values against each other,
///		never against a literal both sides happen to carry today.
///		</para>
///		<para>
///		<c>DmIconButton</c>'s ONLY state beyond the implicit style's is <c>Disabled</c>, at Opacity 0.35.
///		A keyed style's CommonStates setters outrank a call site's local Opacity (measured: the
///		Normal state's Opacity 1 overrode the DungeonListPage StartSessionButton's local 0.35), so
///		the style itself has to dim a disabled icon button.
///		</para>
///		<para>
///		Both dictionaries are read off disk as DATA through <see cref="ThemeSource"/>;
///		<c>DungeonMaster.Tests</c> does not reference <c>DungeonMaster.App</c>.
///		</para>
/// </summary>
public class IconButtonStyleParityTests
{
	/// <summary>
	///		The dictionary declaring <c>DmIconButton</c>.
	/// </summary>
	private const string KeyedDictionary = "Controls.Base.xaml";

	/// <summary>
	///		The dictionary declaring the implicit Button style.
	/// </summary>
	private const string ImplicitDictionary = "Implicit.xaml";

	/// <summary>
	///		The keyed style's resource key.
	/// </summary>
	private const string IconButtonKey = "DmIconButton";

	/// <summary>
	///		The one property <c>DmIconButton</c> is allowed to differ on.
	/// </summary>
	private const string WidthProperty = "MinimumWidthRequest";

	/// <summary>
	///		The setter that carries the visual state groups.
	/// </summary>
	private const string StateGroupsProperty = "VisualStateManager.VisualStateGroups";

	/// <summary>
	///		The one <c>CommonStates</c> state <c>DmIconButton</c> adds beyond the implicit style's.
	/// </summary>
	private const string DisabledState = "Disabled";

	/// <summary>
	///		The Opacity <c>DmIconButton</c>'s Disabled state must set: the 0.35 the disabled icon
	///		Buttons carried locally before the keyed style began to outrank their local value.
	/// </summary>
	private const double DisabledOpacity = 0.35;

	/// <summary>
	///		The geometry setters both styles must carry for the scan to count as having reached
	///		them.
	/// </summary>
	private static readonly string[] GeometryProperties =
	{
		"Padding",
		WidthProperty,
		"MinimumHeightRequest",
		"CornerRadius",
		"BorderWidth",
	};

	/// <summary>
	///		The <c>CommonStates</c> states both styles must carry.
	/// </summary>
	private static readonly string[] CommonStateNames =
	{
		"Normal",
		"PointerOver",
		"Pressed",
	};

	/// <summary>
	///		Asserts that both styles are found and each yields direct setters and
	///		<c>CommonStates</c> opacities, so the parity facts below cannot pass by comparing two
	///		empty sets. It is a named fact so a broken scan is reported as a broken scan.
	/// </summary>
	[Fact]
	public void TheStyleScanReachesBothStylesAndTheirSettersAndStates()
	{
		var icon = IconStyle();
		var implicitStyle = ImplicitStyle();

		icon
			.Should()
			.NotBeNull(
				"DmIconButton must be declared in Controls.Base.xaml");
		implicitStyle
			.Should()
			.NotBeNull(
				"the implicit Button style must be declared in Implicit.xaml");

		foreach (var style in new[] { icon!, implicitStyle! })
		{
			DirectSetters(style)
				.Keys
				.Should()
				.Contain(
					GeometryProperties,
					"the scan has to reach the geometry setters of both styles");
			StateOpacities(style)
				.Keys
				.Should()
				.Contain(
					CommonStateNames,
					"the scan has to reach the three CommonStates opacities of both styles");
			StateOpacities(style)
				.Values
				.Should()
				.OnlyContain(
					opacity =>
						opacity.HasValue,
					"two unparsed opacities compare equal, so each has to parse");
		}
	}

	/// <summary>
	///		Asserts that <c>DmIconButton</c> drops the width floor: its <c>MinimumWidthRequest</c>
	///		parses to zero.
	/// </summary>
	[Fact]
	public void DmIconButtonMinimumWidthRequestIsZero()
	{
		var setters = DirectSetters(IconStyle()!);

		setters
			.Should()
			.ContainKey(WidthProperty);
		Parse(setters[WidthProperty])
			.Should()
			.Be(
				0d,
				"the whole point of DmIconButton is to give up the 44-DIU width floor");
	}

	/// <summary>
	///		Asserts that every setter of <c>DmIconButton</c> other than <c>MinimumWidthRequest</c>
	///		has the same property and parsed value as the implicit Button style's, including each
	///		<c>CommonStates</c> opacity, and that neither side has a setter the other lacks.
	/// </summary>
	[Fact]
	public void EveryDmIconButtonSetterOtherThanMinimumWidthEqualsTheImplicitButtonSetter()
	{
		var icon = IconStyle()!;
		var implicitStyle = ImplicitStyle()!;

		var iconSetters = DirectSetters(icon);
		var implicitSetters = DirectSetters(implicitStyle);
		iconSetters.Remove(WidthProperty);
		implicitSetters.Remove(WidthProperty);

		iconSetters
			.Keys
			.Should()
			.BeEquivalentTo(
				implicitSetters.Keys,
				"an explicit Style= replaces the implicit one, so DmIconButton must restate "
				+ "every setter it has and add none");

		foreach (var property in implicitSetters.Keys)
		{
			Parse(iconSetters[property])
				.Should()
				.Be(
					Parse(implicitSetters[property]),
					$"DmIconButton restates the implicit Button style's {property} by hand, so "
					+ "the two have to be retuned together");
		}

		var iconOpacities = StateOpacities(icon);
		iconOpacities.Remove(DisabledState);
		var implicitOpacities = StateOpacities(implicitStyle);

		iconOpacities
			.Keys
			.Should()
			.BeEquivalentTo(
				implicitOpacities.Keys,
				"DmIconButton must restate every CommonStates state the implicit style has, "
				+ "and no more than the one Disabled state, which its own fact pins");

		foreach (var state in implicitOpacities.Keys)
		{
			iconOpacities[state]
				.Should()
				.Be(
					implicitOpacities[state],
					$"DmIconButton restates the implicit style's {state} opacity by hand, so "
					+ "hover and press feedback must be retuned together");
		}
	}

	/// <summary>
	///		Asserts that <c>DmIconButton</c> declares a <c>Disabled</c> visual state whose Opacity
	///		parses to 0.35, because the keyed style outranks a call site's own local Opacity.
	/// </summary>
	[Fact]
	public void DmIconButtonDisabledVisualStateSetsOpacityThirtyFivePercent()
	{
		var opacities = StateOpacities(IconStyle()!);

		opacities
			.Should()
			.ContainKey(
				DisabledState,
				"the keyed style outranks the call site's local Opacity, so it has to dim a "
				+ "disabled icon button itself");
		opacities[DisabledState]
			.Should()
			.Be(
				DisabledOpacity,
				"0.35 is the disabled opacity the icon Buttons carried locally before the keyed "
				+ "style outranked it");
	}

	/// <summary>
	///		Returns the keyed <c>DmIconButton</c> style, or <c>null</c> when it is not declared.
	/// </summary>
	private static XElement? IconStyle()
	{
		return StyleElements(KeyedDictionary)
			.SingleOrDefault(style =>
				AttributeValue(
					style,
					"x:Key") == IconButtonKey);
	}

	/// <summary>
	///		Returns the implicit <c>Button</c> style (no <c>x:Key</c>), or <c>null</c> when it is
	///		not declared.
	/// </summary>
	private static XElement? ImplicitStyle()
	{
		return StyleElements(ImplicitDictionary)
			.SingleOrDefault(style =>
				AttributeValue(
					style,
					"TargetType") == "Button"
				&& AttributeValue(
					style,
					"x:Key") is null);
	}

	/// <summary>
	///		Returns every <c>Style</c> element of a named theme dictionary, or none when the
	///		dictionary is not found.
	/// </summary>
	private static IEnumerable<XElement> StyleElements(string fileName)
	{
		var document = ThemeSource
			.ThemeDocuments()
			.SingleOrDefault(candidate =>
				candidate
					.RelativePath
					.EndsWith(
						$"/{fileName}",
						StringComparison.Ordinal));

		if (document is null)
		{
			return Enumerable.Empty<XElement>();
		}

		return document
			.Document
			.Descendants()
			.Where(element =>
				IsNamed(
					element,
					"Style"));
	}

	/// <summary>
	///		Returns the style's own <c>Setter</c> children, keyed by <c>Property</c>, with their
	///		<c>Value</c> attribute text, excluding the visual state groups setter.
	/// </summary>
	private static Dictionary<string, string?> DirectSetters(XElement style)
	{
		return style
			.Elements()
			.Where(element =>
				IsNamed(
					element,
					"Setter")
				&& AttributeValue(
					element,
					"Property") != StateGroupsProperty)
			.ToDictionary(
				element =>
					AttributeValue(
						element,
						"Property")!,
				element =>
					AttributeValue(
						element,
						"Value"));
	}

	/// <summary>
	///		Returns the Opacity each <c>CommonStates</c> state of the style sets, keyed by state
	///		name, as a parsed <c>double</c> or <c>null</c> when it will not parse.
	/// </summary>
	private static Dictionary<string, double?> StateOpacities(XElement style)
	{
		return style
			.Descendants()
			.Where(element =>
				IsNamed(
					element,
					"VisualStateGroup")
				&& AttributeValue(
					element,
					"x:Name") == "CommonStates")
			.SelectMany(group =>
				group.Descendants())
			.Where(element =>
				IsNamed(
					element,
					"VisualState"))
			.ToDictionary(
				state =>
					AttributeValue(
						state,
						"x:Name")!,
				state =>
					Parse(
						state
							.Descendants()
							.Where(element =>
								IsNamed(
									element,
									"Setter")
								&& AttributeValue(
									element,
									"Property") == "Opacity")
							.Select(element =>
								AttributeValue(
									element,
									"Value"))
							.FirstOrDefault()) as double?);
	}

	/// <summary>
	///		Parses a setter value: a number becomes a <c>double</c>, anything else (a
	///		<c>{StaticResource ...}</c> reference, a thickness) stays the verbatim string. Returns
	///		<c>null</c> for an absent value.
	/// </summary>
	private static object? Parse(string? text)
	{
		if (text is null)
		{
			return null;
		}

		return double.TryParse(
			text,
			NumberStyles.Float,
			CultureInfo.InvariantCulture,
			out var number)
			? number
			: text;
	}

	/// <summary>
	///		Returns whether the element's name, with its XAML prefix restored, is the supplied
	///		qualified name.
	/// </summary>
	private static bool IsNamed(
		XElement element,
		string qualifiedElementName)
	{
		return ThemeSource.QualifiedElementName(element.Name) == qualifiedElementName;
	}

	/// <summary>
	///		Returns the value of the attribute whose name, with its XAML prefix restored, is the
	///		supplied qualified name, or <c>null</c> when the element carries no such attribute.
	/// </summary>
	private static string? AttributeValue(
		XElement element,
		string qualifiedAttributeName)
	{
		return element
			.Attributes()
			.FirstOrDefault(attribute =>
				ThemeSource.QualifiedElementName(attribute.Name) == qualifiedAttributeName)
			?.Value;
	}
}
