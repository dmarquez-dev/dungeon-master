using System.Globalization;
using System.Xml.Linq;
using FluentAssertions;

namespace DungeonMaster.Tests.App.Theme;

/// <summary>
///		Opacity parity ratchet between <c>DmButtonBase</c>'s <c>CommonStates</c> group in
///		<c>Controls.Base.xaml</c> and the group <c>DmConfirmButton</c> restates in
///		<c>Controls.Semantic.xaml</c>.
///		<para>
///		THE DUPLICATION THESE FACTS GUARD IS DELIBERATE AND MUST NOT BE REFACTORED AWAY.
///		<c>DmConfirmButton</c> derives from <c>DmButtonBase</c> via <c>BasedOn</c> and then
///		declares its own complete <c>CommonStates</c> group, because a derived style REPLACES
///		the base group wholesale rather than merging into it. The comment block at the head of
///		<c>Controls.Semantic.xaml</c> records the regression that proved this and warns, in
///		capitals, that <c>DmDestructiveButton</c> must not gain a group of its own. Nothing here
///		asks for the groups to be merged; what was missing was any test that notices when the
///		two copies drift, so a retune of the base hover or press opacity silently left this copy
///		behind.
///		</para>
///		<para>
///		ONLY <c>PointerOver</c> AND <c>Pressed</c> ARE PARITY CASES. The other two states of the
///		group are excluded on purpose:
///		</para>
///		<para>
///		<c>Disabled</c> DIVERGES DELIBERATELY — <c>DmButtonBase</c> sets Opacity <c>0.45</c> and
///		<c>DmConfirmButton</c> sets <c>0.6</c>. That is not drift. <c>Controls.Semantic.xaml</c>'s
///		comment block records that <c>DmConfirmButton</c>'s states reproduce the appearance of the
///		style it replaced exactly, and that inheriting the base group would have turned the
///		disabled CREATE button in <c>CreateDungeonPopup</c> from grey-brown back to green at
///		<c>0.45</c>. DO NOT WIDEN THESE FACTS TO THE WHOLE GROUP: asserting <c>Disabled</c> parity
///		would fail today, and "fixing" the XAML to satisfy it reintroduces the regression the
///		dictionary warns about.
///		</para>
///		<para>
///		<c>Normal</c> is excluded because it is not a parity case either. Both sides do set
///		Opacity <c>1</c>, but <c>DmConfirmButton</c>'s <c>Normal</c> state deliberately overrides
///		BackgroundColor and TextColor as well, which is the very mechanism
///		<c>Controls.Semantic.xaml</c> documents as outranking an imperative assignment.
///		</para>
///		<para>
///		Both dictionaries are read off disk as DATA through <see cref="ThemeSource"/>, the
///		reader this suite's other theme facts share. <c>DungeonMaster.Tests</c> does not
///		reference <c>DungeonMaster.App</c>, so there is no styled <c>Button</c> to interrogate
///		here; the parsed XAML is the observable artifact.
///		</para>
/// </summary>
public class ConfirmButtonStateParityTests
{
	/// <summary>
	///		The dictionary declaring <c>DmButtonBase</c>.
	/// </summary>
	private const string BaseDictionary = "Controls.Base.xaml";

	/// <summary>
	///		The dictionary declaring <c>DmConfirmButton</c>.
	/// </summary>
	private const string SemanticDictionary = "Controls.Semantic.xaml";

	/// <summary>
	///		The base style's resource key.
	/// </summary>
	private const string BaseStyleKey = "DmButtonBase";

	/// <summary>
	///		The derived style's resource key.
	/// </summary>
	private const string ConfirmStyleKey = "DmConfirmButton";

	/// <summary>
	///		Asserts that the scan both parity facts below are built on actually reaches an Opacity
	///		value for each style in each of the two states it compares — four values in all.
	///		<para>
	///		This fact is not ceremony. <see cref="StateOpacity"/> returns <c>null</c> when it
	///		resolves against nothing, and two absent values compare EQUAL, so a scan broken by a
	///		renamed state, a restructured group or a moved dictionary would leave both parity
	///		facts passing while asserting nothing whatsoever. It is a named fact rather than a
	///		check inside the Arrange helper for the reason
	///		<c>ThemeTokenParityTests.TheResourceKeyDefinitionScanFindsAtLeastOneKey</c> gives,
	///		citing contracts/anti-vacuity.md section 2: a helper reports a broken scan against
	///		whichever fact happened to call it first.
	///		</para>
	/// </summary>
	[Fact]
	public void TheCommonStatesOpacityScanReachesBothStylesInBothComparedStates()
	{
		var unreached = new[]
			{
				(Dictionary: BaseDictionary, Style: BaseStyleKey, State: "PointerOver"),
				(Dictionary: SemanticDictionary, Style: ConfirmStyleKey, State: "PointerOver"),
				(Dictionary: BaseDictionary, Style: BaseStyleKey, State: "Pressed"),
				(Dictionary: SemanticDictionary, Style: ConfirmStyleKey, State: "Pressed"),
			}
			.Where(lookup =>
				StateOpacity(
					lookup.Dictionary,
					lookup.Style,
					lookup.State) is null)
			.Select(lookup =>
				$"{lookup.Dictionary}/{lookup.Style}/{lookup.State}")
			.ToList();

		unreached
			.Should()
			.BeEmpty(
				"both parity facts below compare two values that are absent — and therefore "
				+ "equal — whenever this scan fails to reach one of them");
	}

	/// <summary>
	///		Asserts that <c>DmConfirmButton</c>'s <c>PointerOver</c> Opacity is the same value as
	///		<c>DmButtonBase</c>'s, so a retune of the base hover opacity cannot leave the derived
	///		copy behind.
	///		<para>
	///		The two PARSED values are compared against each other. Neither is compared against the
	///		literal they both happen to carry today: a fact asserting <c>0.82</c> on each side
	///		still passes after the base is retuned and the copy is not, which is precisely the
	///		drift this file exists to catch.
	///		</para>
	/// </summary>
	[Fact]
	public void DmConfirmButtonPointerOverOpacityEqualsDmButtonBasePointerOverOpacity()
	{
		var baseOpacity = StateOpacity(
			BaseDictionary,
			BaseStyleKey,
			"PointerOver");

		StateOpacity(
				SemanticDictionary,
				ConfirmStyleKey,
				"PointerOver")
			.Should()
			.Be(
				baseOpacity,
				"DmConfirmButton restates DmButtonBase's CommonStates group wholesale, so its "
				+ "PointerOver opacity has to be retuned by hand whenever the base one moves; "
				+ "change both or neither");
	}

	/// <summary>
	///		Asserts that <c>DmConfirmButton</c>'s <c>Pressed</c> Opacity is the same value as
	///		<c>DmButtonBase</c>'s, comparing the two parsed values rather than either against a
	///		literal, for the reason the <c>PointerOver</c> fact above gives.
	/// </summary>
	[Fact]
	public void DmConfirmButtonPressedOpacityEqualsDmButtonBasePressedOpacity()
	{
		var baseOpacity = StateOpacity(
			BaseDictionary,
			BaseStyleKey,
			"Pressed");

		StateOpacity(
				SemanticDictionary,
				ConfirmStyleKey,
				"Pressed")
			.Should()
			.Be(
				baseOpacity,
				"DmConfirmButton restates DmButtonBase's CommonStates group wholesale, so its "
				+ "Pressed opacity has to be retuned by hand whenever the base one moves; "
				+ "change both or neither");
	}

	/// <summary>
	///		Asserts that <see cref="IsNamed"/> accepts, for each element name the scan above
	///		passes it, the element in the MAUI default namespace and not the same-named element
	///		in a <c>clr-namespace:</c> namespace.
	/// </summary>
	[Fact]
	public void IsNamedMatchesOnlyTheMauiDefaultNamespace()
	{
		var document = XDocument.Parse(
			string.Join(
				'\n',
				$@"<ResourceDictionary xmlns=""{ThemeSource.MauiNamespace}""",
				@"	xmlns:local=""clr-namespace:DungeonMaster.Synthetic"">",
				@"	<Style />",
				@"	<local:Style />",
				@"	<VisualStateGroup />",
				@"	<local:VisualStateGroup />",
				@"	<VisualState />",
				@"	<local:VisualState />",
				@"	<Setter />",
				@"	<local:Setter />",
				@"</ResourceDictionary>"));

		foreach (var localName in new[] { "Style", "VisualStateGroup", "VisualState", "Setter" })
		{
			document
				.Descendants()
				.Where(element =>
					IsNamed(
						element,
						localName))
				.Select(element =>
					element.Name)
				.Should()
				.Equal(
					new[]
					{
						XName.Get(
							localName,
							ThemeSource.MauiNamespace),
					},
					$"IsNamed must accept only the MAUI {localName}, not the clr-namespace one "
					+ "that shares its local name");
		}
	}

	/// <summary>
	///		Returns the Opacity a named <c>CommonStates</c> visual state of a named style sets, as
	///		a parsed <c>double</c>, or <c>null</c> when the scan resolves against nothing — no
	///		such dictionary, style, group, state or Opacity setter, or a value that will not
	///		parse.
	///		<para>
	///		Asserts nothing, deliberately. The guard is
	///		<see cref="TheCommonStatesOpacityScanReachesBothStylesInBothComparedStates"/>, which
	///		is a named fact so that a broken scan is reported as a broken scan rather than as a
	///		parity failure.
	///		</para>
	/// </summary>
	/// <param name="fileName">
	///		The declaring dictionary's file name, for example "Controls.Base.xaml".
	/// </param>
	/// <param name="styleKey">
	///		The declaring <c>Style</c>'s <c>x:Key</c>.
	/// </param>
	/// <param name="stateName">
	///		The <c>VisualState</c>'s <c>x:Name</c>, for example "PointerOver".
	/// </param>
	private static double? StateOpacity(
		string fileName,
		string styleKey,
		string stateName)
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
			return null;
		}

		var text = document
			.Document
			.Descendants()
			.Where(element =>
				IsNamed(
					element,
					"Style")
				&& AttributeValue(
					element,
					"x:Key") == styleKey)
			.SelectMany(style =>
				style.Descendants())
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
					"VisualState")
				&& AttributeValue(
					element,
					"x:Name") == stateName)
			.SelectMany(state =>
				state.Descendants())
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
			.FirstOrDefault(value =>
				value is not null);

		if (text is null)
		{
			return null;
		}

		return double.TryParse(
			text,
			NumberStyles.Float,
			CultureInfo.InvariantCulture,
			out var opacity)
			? opacity
			: (double?)null;
	}

	/// <summary>
	///		Returns whether the element is the named element in the MAUI default namespace,
	///		through <see cref="ThemeSource.IsMauiElement"/>. The same local name in any other
	///		namespace, such as a <c>clr-namespace:</c> one, is not a match.
	/// </summary>
	/// <param name="element">
	///		The element to test.
	/// </param>
	/// <param name="localName">
	///		The bare local name to match, for example "Style".
	/// </param>
	private static bool IsNamed(
		XElement element,
		string localName)
	{
		return ThemeSource.IsMauiElement(
			element,
			localName);
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
