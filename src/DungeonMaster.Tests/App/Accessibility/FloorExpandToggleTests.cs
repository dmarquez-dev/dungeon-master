using System.Xml.Linq;
using DungeonMaster.Tests.App.Theme;
using FluentAssertions;

namespace DungeonMaster.Tests.App.Accessibility;

/// <summary>
///		Ratchet for the floor card's expand toggle: it must stay a keyboard-operable
///		<c>Button</c> whose footprint does not grow the header row.
/// </summary>
/// <remarks>
///		<para>
///			MASTER-164. The toggle was a <c>Label</c> and the card body it pointed at is a
///			<c>VerticalStackLayout</c> carrying a <c>TapGestureRecognizer</c>. Neither takes
///			keyboard focus on WinUI — only <c>Control</c>-derived classes do — and expanding is
///			the ONLY route to a floor's areas, so no floor's areas could be opened by keyboard at
///			all. A 40-press Tab walk reached the chevron zero times.
///		</para>
///		<para>
///			MAKING THE CARD BODY FOCUSABLE IS NOT AN ALTERNATIVE REMEDY, which is why this fact
///			asserts the element TYPE and not merely "something here is focusable":
///			<c>IsTabStop</c>, <c>TabIndex</c> and <c>ITabStopElement</c> are absent from .NET MAUI
///			10's public API, and MAUI 9 removed layout focus deliberately.
///		</para>
///		<para>
///			THE FOOTPRINT ASSERTIONS ARE NOT COSMETIC. This remedy was declined once, at
///			sub-session 5c's escalation E5, precisely because a <c>Button</c> grows the header
///			row, and it was re-taken only on the condition that the row's height does not change.
///			The conditions that hold that line — the glyph's own font size, a transparent
///			background, no border, no margin, and a hit area that comes from <c>Padding</c> — are
///			what the second fact below pins. A rendered height cannot be measured from source, so
///			this guards the inputs that produced the measured one: header row 44px and floor card
///			54px, observed in the running app at 1920x1080 with 11 floors, unchanged from the
///			<c>Label</c>.
///		</para>
/// </remarks>
public class FloorExpandToggleTests
{
	private const string ControlRelativePath = "Controls/FloorSidebarItem/FloorSidebarItem.xaml";

	private const string ToggleName = "ExpandToggle";

	private const string CardName = "FloorCard";

	private const string BorderName = "FloorBorder";

	private const string XamlNamespace = "http://schemas.microsoft.com/winfx/2009/xaml";

	/// <summary>
	///		The expand toggle is a <c>Button</c>, and keeps the <c>AutomationId</c> the Appium
	///		harness addresses it by.
	/// </summary>
	[Fact]
	public void ExpandToggleIsAKeyboardFocusableButton()
	{
		var toggle = Toggle();

		ThemeSource
			.QualifiedElementName(toggle.Name)
			.Should()
			.Be(
				"Button",
				"a Label takes no keyboard focus on WinUI, and expanding is the only route to a "
				+ "floor's areas, so a Label here makes the whole area list keyboard-unreachable "
				+ "(MASTER-164)");

		Attribute(toggle, "AutomationId")
			.Should()
			.Be(
				"FloorExpandToggle",
				"the out-of-process UI harness can address this element only by its "
				+ "AutomationId, and it is written against that id rather than against whatever "
				+ "it finds");

		Attribute(toggle, "Clicked")
			.Should()
			.NotBeNullOrWhiteSpace(
				"a focusable Button with no handler is reachable by Tab and does nothing when "
				+ "Space is pressed, which looks like the fix and is not one");
	}

	/// <summary>
	///		The toggle's footprint stays pinned: the glyph is unchanged and the hit area comes
	///		from padding rather than from a bigger glyph or a margin.
	/// </summary>
	[Fact]
	public void ExpandToggleKeepsItsPinnedFootprint()
	{
		var toggle = Toggle();

		Attribute(toggle, "FontSize")
			.Should()
			.Be(
				"{StaticResource DmIconSizeSmall}",
				"the visible chevron must be the same size it was as a Label; growing the glyph "
				+ "is the one way to reach the hit area that also grows the header row");

		Attribute(toggle, "BackgroundColor")
			.Should()
			.Be(
				"Transparent",
				"nothing of the button may be drawn — only the chevron it contains");

		Attribute(toggle, "BorderWidth")
			.Should()
			.Be(
				"0",
				"a border would draw a box around the chevron and add to its measured size");

		Attribute(toggle, "Margin")
			.Should()
			.Be(
				"0",
				"padding is the hit-area mechanism and margin is not: a margin adds space "
				+ "OUTSIDE the target, which grows the row without growing what can be clicked");

		Attribute(toggle, "Padding")
			.Should()
			.NotBeNullOrWhiteSpace(
				"the hit area is reached by padding the glyph, so removing the padding shrinks "
				+ "the target below the size that made a sub-minimum chevron defensible");
	}

	/// <summary>
	///		The card body keeps its tap gesture, and the card's root <c>Border</c> stays out of
	///		the accessibility tree as a described element.
	/// </summary>
	/// <remarks>
	///		Two prohibitions, together because both are invisible when broken. The chevron's
	///		visual is smaller than Microsoft's 23x23 minimum and is sanctioned only by their
	///		redundancy carve-out — the card-body tap IS that redundancy, so deleting it retracts
	///		the sanction. And setting a description on a control with children makes iOS and Mac
	///		Catalyst screen readers unable to reach those children, which would take all three
	///		header buttons out of VoiceOver.
	/// </remarks>
	[Fact]
	public void CardBodyKeepsItsTapGestureAndTheBorderStaysUndescribed()
	{
		var document = ControlDocument();

		var card = WithAutomationId(document, CardName);

		// DIRECT children, not Descendants(). Written as a Descendants() scan first, this read
		// green with the card's own tap deleted, because the drag handle nested inside the card
		// carries a TapGestureRecognizer of its own and satisfied the scan. The gesture that
		// matters is the one attached to the CARD, so the lookup has to be anchored to the card's
		// own GestureRecognizers collection and to the handler it names.
		card
			.Elements()
			.Where(element => element.Name.LocalName.EndsWith(
				".GestureRecognizers",
				StringComparison.Ordinal))
			.Elements()
			.Where(element => element.Name.LocalName == "TapGestureRecognizer")
			.Select(element => (string?)element.Attribute("Tapped"))
			.Should()
			.Contain(
				"OnCardTapped",
				"the chevron's sub-minimum visual is defensible only because the card tap is "
				+ "redundant with it; removing the card tap removes the justification");

		var border = Named(document, BorderName);

		Attribute(border, "SemanticProperties.Description")
			.Should()
			.BeNull(
				"a description on a control with children hides those children from iOS and Mac "
				+ "Catalyst screen readers, which would remove the notebook, delete and reorder "
				+ "controls from VoiceOver entirely");
	}

	/// <summary>
	///		Anti-vacuity. Every element this class reads must actually be found, so that a rename
	///		or a file move fails here rather than turning the three facts above into assertions
	///		about nothing.
	/// </summary>
	[Fact]
	public void TheScanReachesTheElementsItAssertsAbout()
	{
		var document = ControlDocument();

		new[] { ToggleName, BorderName }
			.Should()
			.AllSatisfy(name =>
				document
					.Descendants()
					.Count(element => (string?)element.Attribute(XName.Get("Name", XamlNamespace)) == name)
					.Should()
					.Be(
						1,
						"the scan addresses this element by x:Name, so a rename must fail here "
						+ "rather than silently reaching nothing"));

		// The card body is addressed by AutomationId rather than x:Name because it HAS no
		// x:Name — the code-behind never reaches it, only the UI harness does. Writing the
		// wrong lookup here is not hypothetical: the first draft of this class looked FloorCard
		// up by x:Name and this fact is what reported it.
		document
			.Descendants()
			.Count(element => (string?)element.Attribute("AutomationId") == CardName)
			.Should()
			.Be(
				1,
				"the card body is addressed by AutomationId, so a rename must fail here rather "
				+ "than silently reaching nothing");
	}

	private static XDocument ControlDocument()
	{
		var document = ThemeSource
			.AppDocuments()
			.SingleOrDefault(candidate =>
				string.Equals(
					candidate.RelativePath,
					ControlRelativePath,
					StringComparison.OrdinalIgnoreCase));

		document
			.Should()
			.NotBeNull(
				"{0} must be part of the application XAML scan, or every fact in this class "
				+ "passes by reaching nothing",
				ControlRelativePath);

		return document!.Document;
	}

	private static XElement Toggle() =>
		Named(
			ControlDocument(),
			ToggleName);

	private static XElement Named(XDocument document, string name)
	{
		var element = document
			.Descendants()
			.SingleOrDefault(candidate =>
				(string?)candidate.Attribute(XName.Get("Name", XamlNamespace)) == name);

		element
			.Should()
			.NotBeNull(
				"{0} must declare exactly one element named {1}",
				ControlRelativePath,
				name);

		return element!;
	}

	private static XElement WithAutomationId(XDocument document, string automationId)
	{
		var element = document
			.Descendants()
			.SingleOrDefault(candidate =>
				(string?)candidate.Attribute("AutomationId") == automationId);

		element
			.Should()
			.NotBeNull(
				"{0} must declare exactly one element with AutomationId {1}",
				ControlRelativePath,
				automationId);

		return element!;
	}

	private static string? Attribute(XElement element, string name) =>
		(string?)element.Attribute(name);
}
