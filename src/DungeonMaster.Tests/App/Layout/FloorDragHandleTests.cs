using System.Xml.Linq;
using DungeonMaster.Tests.App.Theme;
using FluentAssertions;

namespace DungeonMaster.Tests.App.Layout;

/// <summary>
///		Ratchet for the floor reorder handle and the scrollbar gutter it used to sit under —
///		MASTER-163, this session's primary defect.
/// </summary>
/// <remarks>
///		<para>
///			WHAT WAS MEASURED, because none of it can be measured from source and every assertion
///			below is a guard on an input that produced it. In the running application at 900x640
///			with 11 floors and the sidebar scrolling, the WinUI <c>ScrollViewer</c> lays its
///			vertical scrollbar out in a <c>Grid</c> 14 DIU wide that paints OVER the content: the
///			<c>ScrollContentPresenter</c> stayed the <c>ScrollViewer</c>'s full 300 DIU, so the bar
///			took no width from the cards and every card ran underneath it. The drag handle, the
///			rightmost thing in a card, was a 7x24 glyph at x 284-291 of that 300 — so the overlay
///			covered five of its seven units. <c>Platforms/Windows/App.xaml.cs</c> overrides
///			scrollbar COLOUR only and reserves nothing.
///		</para>
///		<para>
///			THE SEVERITY IS UPWARD, NOT DOWNWARD. The reorder itself works end to end when the
///			drop lands on a target — observed with the user on 2026-09-11 in a maximized window
///			with no scrollbar rendered, UI and database agreeing. What fails is REACHING the
///			target, and a drop that misses every target is discarded silently, with no handler, no
///			error and no trace. That is why the feature reads as broken, and it is why this is not
///			an ergonomics entry.
///		</para>
///		<para>
///			THE THREE REMEDIES GUARDED HERE ARE ALL WIDTH OR HIT-AREA, and that is contract C3
///			section 5's condition: a remedy that adds a row, wraps the handle below the row, or
///			otherwise grows the card vertically is forbidden. Re-measured with the same instrument
///			after the change: header row 44 DIU and floor card 54 DIU, both unchanged; the card's
///			WIDTH went 300 to 286 and the handle went 7x24 to 23x44, which is permitted.
///		</para>
/// </remarks>
public class FloorDragHandleTests
{
	private const string ControlRelativePath = "Controls/FloorSidebarItem/FloorSidebarItem.xaml";

	private const string PageRelativePath = "Views/DungeonDetailPage/DungeonDetailPage.xaml";

	private const string HandleAutomationId = "FloorDragHandle";

	private const string NameLabelName = "FloorNameLabel";

	private const string ListContainerName = "FloorListContainer";

	private const string XamlNamespace = "http://schemas.microsoft.com/winfx/2009/xaml";

	/// <summary>
	///		The drag handle is the <c>Label</c> that hosts the reorder gesture, under the
	///		identifier the out-of-process harness addresses it by.
	/// </summary>
	/// <remarks>
	///		<para>
	///		Contract C1 amendment A1 section 1A, which SUPERSEDES section 1. This element was
	///		converted to a <c>Button</c> so it could take keyboard focus, and the conversion was
	///		REVERTED: the human observation pass O1 on 2026-09-16 found that a WinUI <c>Button</c>
	///		consumes the press, so MAUI's <c>DragGestureRecognizer</c> never started a drag from it
	///		and the pointer reorder was broken outright, and that the <c>Button</c>'s wider
	///		footprint squeezed the floor name beside it. Microsoft's drag-and-drop guidance
	///		attaches that recogniser only to non-interactive elements and exposes no property that
	///		makes a <c>Button</c> a drag source, so no fix kept it. A <c>Label</c> is therefore the
	///		asserted type, and it is the exact element 6b-iv observed starting a drag on
	///		2026-09-11. A second conversion to <c>Button</c> would break the drag the same way, and
	///		that is what this assertion is here to catch.
	///		</para>
	///		<para>
	///		THIS ELEMENT TAKES NO KEYBOARD FOCUS, and nothing here should be read as claiming it
	///		does. Only <c>Control</c>-derived classes take WinUI keyboard focus, which is exactly
	///		why amendment A1 section 3A moved the <c>Ctrl</c>+arrow gate OFF the handle and onto the
	///		floor card — <c>FloorSidebarItem.ContainsKeyboardFocus</c>, which reports focus anywhere
	///		within the card, reachable through the <c>ExpandToggle</c> <c>Button</c> that
	///		<c>MASTER-164</c> already made focusable. The route is not asserted by this file at all;
	///		it is a focus fact and this is a markup-shape test.
	///		</para>
	///		<para>
	///		The drag gesture rides on this <c>Label</c> with both handler names unchanged, and
	///		amendment A1 section 1A freezes all three of those facts: the two handler names and that
	///		this file declares exactly ONE drag source. Renaming either handler, or adding a second
	///		source, is a contract change and both are invisible in a diff that also converts an
	///		element.
	///		</para>
	/// </remarks>
	[Fact]
	public void TheDragHandleStillHostsTheReorderGestureAndNothingElseDoes()
	{
		var document = ControlDocument();
		var handle = WithAutomationId(document, HandleAutomationId);

		ThemeSource
			.QualifiedElementName(handle.Name)
			.Should()
			.Be(
				"Label",
				"contract C1 amendment A1 section 1A freezes this element as a Label: observation "
				+ "O1 found that a WinUI Button consumes the press, so the DragGestureRecognizer "
				+ "never starts a drag from one and the pointer reorder is broken outright");

		var gestures = handle
			.Elements()
			.Where(element => element.Name.LocalName.EndsWith(
				".GestureRecognizers",
				StringComparison.Ordinal))
			.Elements()
			.Where(element => element.Name.LocalName == "DragGestureRecognizer")
			.ToList();

		gestures
			.Should()
			.ContainSingle(
				"the handle is the only POINTER reorder affordance in the source — OnKeyDown "
				+ "carries the Ctrl+arrow route alongside it, gated on focus anywhere within the "
				+ "CARD rather than on this element — so a drag gesture that is not on THIS "
				+ "element is a reorder the user cannot start with the pointer");

		((string?)gestures[0].Attribute("DragStarting"))
			.Should()
			.Be(
				"OnDragStarting",
				"contract C1 amendment A1 section 1A freezes the handler name; the payload "
				+ "contract's only writer is that method");

		((string?)gestures[0].Attribute("DropCompleted"))
			.Should()
			.Be(
				"OnDropCompleted",
				"contract C1 amendment A1 section 1A freezes the handler name");

		// DESCENDANTS of the whole document, deliberately: the assertion that matters is that no
		// SECOND drag source appeared elsewhere in the card while this one was being widened.
		document
			.Descendants()
			.Count(element => element.Name.LocalName == "DragGestureRecognizer")
			.Should()
			.Be(
				1,
				"a second drag source in this file would start a reorder from somewhere the "
				+ "payload contract was never written for");
	}

	/// <summary>
	///		The handle's grab area is the widened one, and it is widened by the three properties
	///		that cost no height.
	/// </summary>
	/// <remarks>
	///		<para>
	///			<c>BackgroundColor="Transparent"</c> is the load-bearing one and reads as cosmetic:
	///			WinUI does not hit-test a null <c>Background</c>, so without it the padding is
	///			inert and only the glyph itself takes the press — the target stays 7 DIU wide and
	///			the remedy is a no-op that looks applied.
	///		</para>
	///		<para>
	///			A <c>FontSize</c> assertion is here for the opposite reason to the padding one.
	///			Growing the GLYPH is the other way to reach a bigger target, and it is the way
	///			that grows the header row — which contract C3 section 2 forbids, and on which
	///			escalation E5's reversal was conditioned.
	///		</para>
	/// </remarks>
	[Fact]
	public void TheHandleIsWidenedByPaddingRatherThanByTheGlyph()
	{
		var handle = WithAutomationId(ControlDocument(), HandleAutomationId);

		Attribute(handle, "Padding")
			.Should()
			.NotBeNullOrWhiteSpace(
				"the grab area comes from padding the glyph; at 7x24 the handle was too small to "
				+ "hit reliably even with the scrollbar moved clear of it");

		Attribute(handle, "BackgroundColor")
			.Should()
			.Be(
				"Transparent",
				"WinUI does not hit-test a null Background, so without an explicit transparent "
				+ "one the padding is inert and only the glyph takes the press");

		Attribute(handle, "FontSize")
			.Should()
			.Be(
				"{StaticResource DmIconSizeLarge}",
				"growing the glyph is the other route to a bigger target and it is the route "
				+ "that grows the header row, which contract C3 section 2 forbids");

		Attribute(handle, "HeightRequest")
			.Should()
			.BeNull(
				"the handle's height is the header row's, set by its three sibling Buttons; a "
				+ "height of its own is the one way this element can raise the row");

		Attribute(handle, "MinimumHeightRequest")
			.Should()
			.BeNull(
				"a minimum height raises the row exactly as a height does, and is easier to miss");
	}

	/// <summary>
	///		The handle announces the keyboard reorder route, verbatim, in the one string a screen
	///		reader user can learn it from.
	/// </summary>
	/// <remarks>
	///		<para>
	///		Contract C1 amendment A1 section 5A freezes this string word for word, replacing the
	///		wording section 5 froze. It is asserted whole rather than by a <c>Contain("Control")</c>,
	///		because a hint that has drifted into naming only one of the two directions, or into
	///		"Ctrl+Up", is still a passing substring match and still a defective announcement — the
	///		contract spells the keys out precisely because a screen reader reads the string as
	///		written, not as a keyboard shortcut.
	///		</para>
	///		<para>
	///		WHY THE WORDING CHANGED. Section 5's string told the user to press the keys with the
	///		handle in hand, which was true only while the handle was a <c>Button</c>. It is a
	///		<c>Label</c> again and focus never rests on it, so A1 section 5A rewords the hint to say
	///		"with a floor card focused" — the gate is now
	///		<c>FloorSidebarItem.ContainsKeyboardFocus</c>, satisfied from any focusable control on
	///		the card. The cost A1 discloses is that this handle is no longer its own tab stop, so a
	///		screen-reader user meets this string as static text rather than on a focused control.
	///		</para>
	///		<para>
	///		Nothing else in the repository guards it. The route has no visible affordance at all:
	///		the handle looks exactly like the drag handle it already was, so a sighted user learns
	///		the keys from the tooltip and a screen-reader user learns them from HERE or nowhere.
	///		Silently losing this attribute would leave the feature shipped and undiscoverable,
	///		which is the failure this test exists for. It does NOT establish that the keystroke
	///		works — observation O1 settled that, and no test in this project could.
	///		</para>
	/// </remarks>
	[Fact]
	public void TheHandleAnnouncesTheKeyboardReorderRouteInItsHint()
	{
		var handle = WithAutomationId(ControlDocument(), HandleAutomationId);

		Attribute(handle, "SemanticProperties.Hint")
			.Should()
			.Be(
				"Drag this handle onto another floor card to change the floor order. With a "
				+ "floor card focused, press Control plus Up Arrow or Control plus Down Arrow to "
				+ "move it.",
				"contract C1 amendment A1 section 5A freezes this hint verbatim, and it is the "
				+ "only place a screen reader user is told the Ctrl+Up / Ctrl+Down route exists "
				+ "at all");
	}

	/// <summary>
	///		The floor list reserves the overlay scrollbar's gutter, and reserves it on the right
	///		edge only.
	/// </summary>
	/// <remarks>
	///		The inset is on the CONTENT rather than on the card so that the drag placeholder the
	///		page's code-behind inserts into this same container is inset by it too; a margin on the
	///		card would move the cards and leave the placeholder running under the bar. A top or
	///		bottom component here would be a vertical change, which contract C3 section 5 forbids,
	///		so the shape is asserted and not merely the presence of a padding.
	/// </remarks>
	[Fact]
	public void TheFloorListReservesTheScrollbarGutterOnTheRightEdgeOnly()
	{
		var container = Named(PageDocument(), ListContainerName, PageRelativePath);

		var padding = Attribute(container, "Padding");

		padding
			.Should()
			.NotBeNullOrWhiteSpace(
				"the WinUI scrollbar is an overlay and reserves nothing itself, so an unpadded "
				+ "content panel puts every card — and the drag handle at its right edge — "
				+ "underneath the bar");

		var components = padding!
			.Split(',')
			.Select(component => component.Trim())
			.ToList();

		components
			.Should()
			.HaveCount(
				4,
				"a one- or two-component padding insets more edges than the gutter, which is a "
				+ "vertical change contract C3 section 5 forbids");

		double.Parse(components[2], System.Globalization.CultureInfo.InvariantCulture)
			.Should()
			.BeGreaterThanOrEqualTo(
				14,
				"the measured overlay band is 14 DIU wide — the Grid hosting VerticalScrollBar in "
				+ "the ScrollViewer template, inside which the ScrollBar itself measures 12");

		new[] { components[0], components[1], components[3] }
			.Should()
			.AllBe(
				"0",
				"only the right edge is reserved; any other component moves the list vertically "
				+ "or indents it, neither of which this remedy is");
	}

	/// <summary>
	///		The floor name is pinned to one line, which is what keeps contract C3 section 2's
	///		height invariant true for a name of any length.
	/// </summary>
	/// <remarks>
	///		A MAUI <c>Label</c> defaults to <c>WordWrap</c>, so this column's height is a function
	///		of the name's length. Reserving the gutter and widening the handle took the column from
	///		129 to 99 DIU, and "Main Tavern" — which measures 107 — went from one 22-DIU line to
	///		two. Two lines still measure 44 and the row is 44 because of its three sibling
	///		<c>Button</c>s, so nothing grew; a name needing three lines would have measured 66 and
	///		the row would have followed it. The invariant must hold for any name rather than for
	///		the eleven that happen to be in the database.
	/// </remarks>
	[Fact]
	public void TheFloorNameCannotWrapTheHeaderRowOntoASecondLine()
	{
		var label = Named(ControlDocument(), NameLabelName, ControlRelativePath);

		Attribute(label, "LineBreakMode")
			.Should()
			.Be(
				"TailTruncation",
				"WordWrap is the MAUI default, and under it a long enough floor name grows the "
				+ "header row — the exact change contract C3 section 2 forbids and on which "
				+ "escalation E5's reversal was conditioned");

		Attribute(label, "MaxLines")
			.Should()
			.Be(
				"1",
				"TailTruncation without a line cap still permits more than one line");
	}

	/// <summary>
	///		Anti-vacuity. Every element this class reads must actually be found, so a rename or a
	///		file move fails here rather than turning the four facts above into assertions about
	///		nothing.
	/// </summary>
	[Fact]
	public void TheScanReachesTheElementsItAssertsAbout()
	{
		ControlDocument()
			.Descendants()
			.Count(element => (string?)element.Attribute("AutomationId") == HandleAutomationId)
			.Should()
			.Be(
				1,
				"the handle is addressed by AutomationId, so a rename must fail here rather than "
				+ "silently reaching nothing");

		ControlDocument()
			.Descendants()
			.Count(element => (string?)element.Attribute(XName.Get("Name", XamlNamespace)) == NameLabelName)
			.Should()
			.Be(
				1,
				"the name label is addressed by x:Name, so a rename must fail here rather than "
				+ "silently reaching nothing");

		PageDocument()
			.Descendants()
			.Count(element => (string?)element.Attribute(XName.Get("Name", XamlNamespace)) == ListContainerName)
			.Should()
			.Be(
				1,
				"the floor list container is addressed by x:Name, so a rename must fail here "
				+ "rather than silently reaching nothing");
	}

	private static XDocument ControlDocument() =>
		Document(ControlRelativePath);

	private static XDocument PageDocument() =>
		Document(PageRelativePath);

	private static XDocument Document(string relativePath)
	{
		var document = ThemeSource
			.AppDocuments()
			.SingleOrDefault(candidate =>
				string.Equals(
					candidate.RelativePath,
					relativePath,
					StringComparison.OrdinalIgnoreCase));

		document
			.Should()
			.NotBeNull(
				"{0} must be part of the application XAML scan, or every fact in this class "
				+ "passes by reaching nothing",
				relativePath);

		return document!.Document;
	}

	private static XElement Named(XDocument document, string name, string relativePath)
	{
		var element = document
			.Descendants()
			.SingleOrDefault(candidate =>
				(string?)candidate.Attribute(XName.Get("Name", XamlNamespace)) == name);

		element
			.Should()
			.NotBeNull(
				"{0} must declare exactly one element named {1}",
				relativePath,
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
