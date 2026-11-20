using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using OpenQA.Selenium.Appium;
using Xunit.Abstractions;

namespace DungeonMaster.App.UiTests;

/// <summary>
/// Drives the floor sidebar on <c>DungeonDetailPage</c> through a real Appium session.
/// </summary>
/// <remarks>
/// <para>
/// Every id addressed here is already declared in the app's XAML. This project adds none and
/// edits no <c>.xaml</c> file.
/// </para>
/// <para>
/// WHAT IS DELIBERATELY NOT HERE, and it is the point of the class. Contract C2 section 3 freezes
/// this harness's scope, and two things sit outside it:
/// </para>
/// <list type="bullet">
/// <item>
/// <description>
/// THAT A FLOOR DRAG-AND-DROP REORDER SUCCEEDS. Synthetic <c>mouse_event</c> input was measured on
/// 2026-09-11 not to initiate WinUI drag-and-drop at all — the source card stayed at full opacity,
/// so <c>OnDragStarting</c> never fired. W3C pointer actions were attempted on 2026-09-14 and
/// cannot reach it either: this server/driver pair refuses a <c>mouse</c> pointer input source
/// with <c>Currently only pen and touch pointer input source types are supported</c>, so the
/// instrument has no mouse to press. A test asserting the reorder could therefore only redden for
/// apparatus reasons and would say nothing about the feature, so the reorder's verification is a
/// human observation under T1 and no assertion here depends on it.
/// </description>
/// </item>
/// <item>
/// <description>
/// THE MOUSE CURSOR SHAPE. <c>HandCursorBehavior</c> sets WinUI's non-public
/// <c>ProtectedCursor</c>, which no other process can read, and screenshots do not capture the
/// Windows cursor. <c>AreaRow</c>'s POSITION is assertable here; its cursor is not, by anything.
/// </description>
/// </item>
/// </list>
/// <para>
/// EVERY FACT BELOW HAS A NAMED CONTROL, for the reason this project's README already states: a UI
/// experiment in which everything fails cannot tell a detected defect apart from a broken
/// apparatus. The controls are the tests whose names begin <c>Control</c>, and each fact's
/// documentation names the one that stays green when that fact reddens for a real reason.
/// </para>
/// <para>
/// UNITS. The driver reports rectangles in PHYSICAL PIXELS, not in the device-independent units
/// the XAML is written in: this display reports 120 LOGPIXELSX, a 1.25 scale, and a 44-DIU button
/// measured 55 pixels here on 2026-09-14. The thresholds below are DIU figures applied to pixel
/// measurements ON PURPOSE. At 100% scale the two coincide; at any larger scale the pixel
/// threshold is the WEAKER claim, so a display change can never redden these tests without the
/// underlying defect having genuinely returned. A ratchet's claim should be weaker than the
/// measurement it was cut from.
/// </para>
/// </remarks>
public sealed class FloorSidebarAutomationTests
{
	/// <summary>
	/// The per-dungeon edit button on the start route. Declared in DungeonListPage.xaml.
	/// </summary>
	/// <remarks>
	/// This is the only route from the app's start route to a DungeonDetailPage, and every fact in
	/// this class needs that page. It is NOT an id this project introduced — it predates this
	/// session and its Hint already reads "Opens this dungeon in the floor and map designer".
	/// </remarks>
	private const string EditDungeonButtonId = "EditDungeonButton";

	/// <summary>The card-body layout of one floor. Contract C2 section 1.</summary>
	private const string FloorCardId = "FloorCard";

	/// <summary>The expand chevron, a Button since T2. Contract C2 section 1.</summary>
	private const string FloorExpandToggleId = "FloorExpandToggle";

	/// <summary>The per-floor notebook button, unchanged this session. Contract C2 section 1.</summary>
	private const string FloorNotebookButtonId = "FloorNotebookButton";

	/// <summary>The per-floor delete button, unchanged this session. Contract C2 section 1.</summary>
	private const string FloorDeleteButtonId = "FloorDeleteButton";

	/// <summary>The reorder handle, widened by T3. Contract C2 section 1.</summary>
	private const string FloorDragHandleId = "FloorDragHandle";

	/// <summary>The "+ Add area" button, IsVisible-gated on IsExpanded. Contract C2 section 1.</summary>
	private const string AddAreaButtonId = "AddAreaButton";

	/// <summary>
	/// The sidebar's scroll viewport: the parent of the first floor card. It declares no
	/// AutomationId and is not given one — contract C2 section 1's table is frozen and this
	/// project adds nothing to it — so it is reached structurally and identified by ClassName.
	/// </summary>
	private const string ScrollViewportXPath = "//*[@AutomationId='FloorCard'][1]/..";

	/// <summary>The UIA ClassName the sidebar's scroll viewport must report.</summary>
	private const string ScrollViewportClassName = "ScrollViewer";

	/// <summary>
	/// The smallest edge, in device-independent units, Microsoft's touch minimum sets. It still
	/// holds the expand toggle's HEIGHT, which the implicit Button style's 44 minimum height
	/// keeps, and both edges of the notebook control. The toggle's WIDTH is no longer held to it:
	/// the toggle overrides the style's 44 minimum width with 0 at the user's choice, to remove
	/// the space around the chevron. Applied to a pixel measurement; see the class remarks on
	/// units.
	/// </summary>
	private const int MinimumToggleEdge = 23;

	/// <summary>
	/// The widest, in pixels, the expand toggle may measure now that its minimum width is
	/// overridden. Before the override the implicit Button style's 44-DIU minimum width sized it,
	/// and it measured 55 pixels at this display's 1.25 scale, 44 at 1.0; both exceed this
	/// bound, so losing the override reddens the fact at either scale. After it, the width is
	/// the 14-DIU chevron glyph plus 2 + 2 DIU of padding, at most 18 DIU even if the glyph's
	/// advance were the full em: 18 pixels at 1.0 and 23 at 1.25, well inside the bound.
	/// </summary>
	private const int MaximumToggleWidth = 40;

	/// <summary>
	/// How far the drag handle's right edge must sit left of the scroll viewport's right edge,
	/// in device-independent units.
	/// </summary>
	/// <remarks>
	/// 14 is T3's recorded handoff figure and it is DELIBERATELY not the measurement. T3 measured
	/// the WinUI overlay scrollbar band at 14 DIU wide and the actual clearance after its fix at
	/// 23 DIU. The durable fact is "the handle clears the gutter", and 23 carries slack a DPI
	/// change could eat, so asserting 23 would redden on a display change that had not
	/// reintroduced MASTER-163. Recorded verbatim in this session's observations/T3-sidebar.md,
	/// section "Handoff to T5".
	/// </remarks>
	private const int MinimumHandleClearance = 14;

	/// <summary>
	/// How many Tab presses the walk allows before it gives up looking for the expand toggle.
	/// </summary>
	/// <remarks>
	/// Generous on purpose: the walk's assertion is that the toggle APPEARS in the tab ring, not
	/// that it appears at a fixed index, which would break on any sidebar layout change. The
	/// toggle was reached at press 4 on 2026-09-14.
	/// </remarks>
	private const int MaximumTabPresses = 40;

	private readonly ITestOutputHelper _output;

	public FloorSidebarAutomationTests(ITestOutputHelper output) => _output = output;

	// ---------------------------------------------------------------- controls

	/// <summary>
	/// THE APPARATUS CONTROL for all four facts below. If this reddens, nothing else in this
	/// class means anything: the session did not reach a detail page carrying floors, so every
	/// other failure here is a broken apparatus rather than a detected defect.
	/// </summary>
	[Fact]
	public void ControlASessionOpensAndTheFirstDungeonDetailPageShowsAtLeastOneFloorCard()
	{
		using var session = OpenFirstDungeonDetailPage();

		var cards = session.FindAllElements(FloorCardId);
		_output.WriteLine(Line($"{FloorCardId} count = {cards.Count}"));

		cards.Should().NotBeEmpty(
			because: "every fact in this class reads a floor card's contents, so a detail page with no floors makes them all meaningless");
	}

	/// <summary>
	/// CONTROL for <see cref="TabbingFromTheDetailPageEntryReachesFloorExpandToggle"/>. It
	/// establishes only that the keyboard channel moves focus AT ALL. If the walk reddens while
	/// this stays green, the toggle is genuinely missing from the tab ring; if both redden, Tab
	/// is not being delivered and the walk has detected nothing.
	/// </summary>
	[Fact]
	public void ControlOneTabPressFromPageEntryMovesFocusOffTheEntryElement()
	{
		using var session = OpenFirstDungeonDetailPage();

		var entry = session.FocusedAutomationId();
		_output.WriteLine(Line($"focus at page entry = {entry}"));

		session.PressTab();
		var moved = AppSession.WaitUntil(() => session.FocusedAutomationId() != entry);
		var after = session.FocusedAutomationId();
		_output.WriteLine(Line($"focus after one Tab = {after}"));

		moved.Should().BeTrue(
			because: "the tab-order walk can only report a sequence if a Tab press reaches the application at all");
	}

	/// <summary>
	/// CONTROL for <see cref="FloorExpandToggleMeasuresAtLeast23HighAndAtMost40Wide"/>. The
	/// notebook button sits in the same header row and was not touched this session, so it reads
	/// the same rectangle channel against an unchanged control. If both reddens, the measurement
	/// channel is broken; if only the toggle reddens, the toggle's own footprint has regressed.
	/// </summary>
	[Fact]
	public void ControlFloorNotebookButtonMeasuresAtLeast23By23()
	{
		using var session = OpenFirstDungeonDetailPage();

		var button = session.FindAllElements(FloorNotebookButtonId)[0];
		var size = button.Size;
		_output.WriteLine(Line($"{FloorNotebookButtonId}[0] size = {size.Width}x{size.Height}"));

		size.Width.Should().BeGreaterThanOrEqualTo(MinimumToggleEdge,
			because: "an untouched header button of the same row must measure through this channel, or the channel is what failed");
		size.Height.Should().BeGreaterThanOrEqualTo(MinimumToggleEdge,
			because: "an untouched header button of the same row must measure through this channel, or the channel is what failed");
	}

	/// <summary>
	/// CONTROL for
	/// <see cref="FloorDragHandleRightEdgeClearsTheScrollViewportRightEdgeByAtLeast14"/>. It
	/// establishes that the element the clearance is measured AGAINST is the sidebar's scroll
	/// viewport and not some other ancestor a tree change redirected the lookup to. If the
	/// clearance fact reddens while this stays green, the gutter reservation has genuinely gone.
	/// </summary>
	[Fact]
	public void ControlTheSidebarScrollViewportResolvesAsAScrollViewerWiderThanAFloorCard()
	{
		using var session = OpenFirstDungeonDetailPage();

		var viewport = session.FindByXPath(ScrollViewportXPath);
		var className = viewport.GetAttribute("ClassName");
		var card = session.FindAllElements(FloorCardId)[0];
		_output.WriteLine(Line(
			$"viewport ClassName={className} width={viewport.Size.Width}; {FloorCardId}[0] width={card.Size.Width}"));

		className.Should().Be(ScrollViewportClassName,
			because: "the clearance is measured against the sidebar's scroll viewport, so the node reached must be that viewport");
		viewport.Size.Width.Should().BeGreaterThan(card.Size.Width,
			because: "a viewport no wider than the card it contains would mean the lookup reached the card itself");
	}

	/// <summary>
	/// CONTROL for
	/// <see cref="AddAreaButtonIsAbsentWhileTheFloorIsCollapsedAndPresentAfterTheExpandToggleIsActivated"/>.
	/// A sibling control of the collapsed card IS present, which is what distinguishes "the add-area
	/// button is not shown" from "this session is looking at nothing".
	/// </summary>
	[Fact]
	public void ControlFloorDeleteButtonIsPresentWhileTheFloorIsCollapsed()
	{
		using var session = OpenFirstDungeonDetailPage();

		var deletes = session.FindAllElements(FloorDeleteButtonId);
		_output.WriteLine(Line($"{FloorDeleteButtonId} count while collapsed = {deletes.Count}"));

		deletes.Should().NotBeEmpty(
			because: "the collapsed card's own header controls must be in the tree, or an absent AddAreaButton proves nothing");
	}

	// ---------------------------------------------------------------- facts

	/// <summary>
	/// Fact (a) of contract C2 section 3's assertable set. Control:
	/// <see cref="ControlOneTabPressFromPageEntryMovesFocusOffTheEntryElement"/>.
	/// </summary>
	/// <remarks>
	/// The walk starts at a freshly launched app, navigates to the detail page, and then presses
	/// Tab, reporting the focused AutomationId after each press. It asserts only that the toggle
	/// APPEARS — a fixed index would encode the current sidebar layout rather than the property
	/// that matters, which is that a keyboard user can reach the chevron at all. That is the
	/// point of T2 having made it a Button: a Label is not in the tab ring.
	/// </remarks>
	[Fact]
	public void TabbingFromTheDetailPageEntryReachesFloorExpandToggle()
	{
		using var session = OpenFirstDungeonDetailPage();

		var walk = new List<string>(MaximumTabPresses);
		_output.WriteLine(Line($"focus at page entry = {session.FocusedAutomationId()}"));

		for (var press = 1; press <= MaximumTabPresses; press++)
		{
			var previous = session.FocusedAutomationId();
			session.PressTab();
			_ = AppSession.WaitUntil(() => session.FocusedAutomationId() != previous);

			var focused = session.FocusedAutomationId();
			walk.Add(focused);
			_output.WriteLine(Line($"Tab {press} -> {focused}"));

			if (string.Equals(focused, FloorExpandToggleId, StringComparison.Ordinal))
			{
				break;
			}
		}

		walk.Should().Contain(FloorExpandToggleId,
			because: $"a keyboard user must be able to reach the floor expand chevron; the walk was [{string.Join(", ", walk)}]");
	}

	/// <summary>
	/// Fact (b) of contract C2 section 3's assertable set. Control:
	/// <see cref="ControlFloorNotebookButtonMeasuresAtLeast23By23"/>.
	/// </summary>
	/// <remarks>
	/// The implicit Button style gives every Button a 44x44 minimum, and that minimum, not the
	/// padding, is what sized this toggle. The user chose to remove the space around the chevron
	/// by overriding the style's minimum width with 0 on this button only, accepting a hit width
	/// below the touch minimum; the card-body tap remains the larger target for the same action.
	/// The width is therefore held from ABOVE, so that losing the override reddens here, and the
	/// height keeps its 23 floor, which the style's untouched 44 minimum height supplies.
	/// </remarks>
	[Fact]
	public void FloorExpandToggleMeasuresAtLeast23HighAndAtMost40Wide()
	{
		using var session = OpenFirstDungeonDetailPage();

		var toggle = session.FindAllElements(FloorExpandToggleId)[0];
		var size = toggle.Size;
		_output.WriteLine(Line($"{FloorExpandToggleId}[0] size = {size.Width}x{size.Height}"));

		size.Width.Should().BeGreaterThan(0,
			because: "the chevron must still render as a clickable element, though its width was narrowed below 23 at the user's choice");
		size.Width.Should().BeLessThanOrEqualTo(MaximumToggleWidth,
			because: "with the implicit style's minimum width overridden the toggle is the 14-DIU glyph plus 4 DIU of padding, at most 23 pixels at 1.25 scale, while the un-overridden 44-DIU minimum measured 55 pixels there and 44 at 1.0");
		size.Height.Should().BeGreaterThanOrEqualTo(MinimumToggleEdge,
			because: "the implicit Button style's 44 minimum height, which the toggle leaves in place, is what holds the chevron's hit height and the header row's");
	}

	/// <summary>
	/// Fact (c) of contract C2 section 3's assertable set — the MASTER-163 ratchet. Control:
	/// <see cref="ControlTheSidebarScrollViewportResolvesAsAScrollViewerWiderThanAFloorCard"/>.
	/// </summary>
	/// <remarks>
	/// Before T3's fix the handle occupied X 284-291 of a 300-DIU viewport while the overlay band
	/// began at 286, so the scrollbar covered five of the handle's seven units and the press was
	/// hard to land. The reservation is a right Padding on FloorListContainer, and this is the
	/// assertion that notices if it is ever removed.
	/// </remarks>
	[Fact]
	public void FloorDragHandleRightEdgeClearsTheScrollViewportRightEdgeByAtLeast14()
	{
		using var session = OpenFirstDungeonDetailPage();

		var viewport = session.FindByXPath(ScrollViewportXPath);
		var viewportRight = viewport.Location.X + viewport.Size.Width;

		var handles = session.FindAllElements(FloorDragHandleId);
		foreach (var (handle, index) in handles.Select((handle, index) => (handle, index)))
		{
			_output.WriteLine(Line(
				$"{FloorDragHandleId}[{index}] right = {handle.Location.X + handle.Size.Width}, viewport right = {viewportRight}"));
		}

		// The handles share one column, so the first card's handle carries the column's right
		// edge. Cards below the fold are reported at their scrolled positions and are logged
		// above rather than asserted on, because a clipped rectangle is not a layout fact.
		var first = handles[0];
		var clearance = viewportRight - (first.Location.X + first.Size.Width);
		_output.WriteLine(Line($"clearance = {clearance} (threshold {MinimumHandleClearance})"));

		clearance.Should().BeGreaterThanOrEqualTo(MinimumHandleClearance,
			because: "the WinUI overlay scrollbar is drawn over the content, so MASTER-163 returns the moment the reserved gutter does not");
	}

	/// <summary>
	/// Fact (d) of contract C2 section 3's assertable set. Control:
	/// <see cref="ControlFloorDeleteButtonIsPresentWhileTheFloorIsCollapsed"/>.
	/// </summary>
	/// <remarks>
	/// This is the state change that makes the chevron a control rather than an ornament, and it
	/// is read entirely through Click and element presence — the two things contract C2 section 3
	/// records as proven for this instrument.
	/// </remarks>
	[Fact]
	public void AddAreaButtonIsAbsentWhileTheFloorIsCollapsedAndPresentAfterTheExpandToggleIsActivated()
	{
		using var session = OpenFirstDungeonDetailPage();

		var before = session.CountElements(AddAreaButtonId);
		_output.WriteLine(Line($"{AddAreaButtonId} count while collapsed = {before}"));
		before.Should().Be(0,
			because: "AddAreaButton is IsVisible-gated on IsExpanded, and every floor starts collapsed");

		var toggle = session.FindAllElements(FloorExpandToggleId)[0];
		toggle.Click();
		_output.WriteLine(Line($"clicked {FloorExpandToggleId}[0]"));

		var appeared = AppSession.WaitUntil(() => session.CountElements(AddAreaButtonId) > 0);
		var after = session.CountElements(AddAreaButtonId);
		_output.WriteLine(Line($"{AddAreaButtonId} count after activation = {after}"));

		appeared.Should().BeTrue(
			because: "activating the chevron expands the floor, and the add-area row is what an expanded floor shows");
		session.FindAllElements(AddAreaButtonId)[0].Displayed.Should().BeTrue(
			because: "a control that is present but not displayed would leave the floor no more operable than before");
	}

	// ---------------------------------------------------------------- helpers

	/// <summary>
	/// Opens a session and navigates from the start route to the first dungeon's detail page,
	/// returning once at least one floor card has resolved.
	/// </summary>
	private AppSession OpenFirstDungeonDetailPage()
	{
		var session = AppSession.Open();
		try
		{
			_output.WriteLine(session.Describe());
			session.FindAllElements(EditDungeonButtonId)[0].Click();
			_ = session.WaitForElement(FloorCardId);
			return session;
		}
		catch
		{
			session.Dispose();
			throw;
		}
	}

	private static string Line(FormattableString message) =>
		message.ToString(CultureInfo.InvariantCulture);
}
