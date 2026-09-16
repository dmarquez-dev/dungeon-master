using DungeonMaster.App.Core.Editor;
using FluentAssertions;

namespace DungeonMaster.Tests.AppCore;

/// <summary>
///		Covers every row of contract C1 section 4.1's table over the one owner of the floor-reorder
///		KEYSTROKE decision, plus both directions that do reorder.
/// </summary>
/// <remarks>
///		<para>
///		WHAT THESE TESTS DO NOT COVER, stated here rather than left to be inferred from a green run.
///		The keyboard route has two halves and only one of them is reachable in process. Whether
///		WinUI delivers a <c>Ctrl</c>+arrow <c>KeyDown</c> to the page's handler AT ALL while a MAUI
///		<c>Button</c> holds keyboard focus is not established by this repository, by
///		<c>Plugin.Maui.KeyListener</c>, or by any source contract C1 section 0 could cite — section 0
///		says so in as many words, and section 6 makes it a blocking human observation instead. No
///		test in this file, and no test in this project, can settle it: there is no WinUI input stack
///		in a plain <c>net10.0</c> test host to deliver a keystroke into.
///		</para>
///		<para>
///		So the coverage is: the DECISION, here — what a keystroke means once it has arrived — and the
///		MARKUP that makes arrival possible at all, in
///		<c>DungeonMaster.Tests.App.Layout.FloorDragHandleTests</c>, which pins the handle as a
///		<c>Button</c> (only <c>Control</c>-derived types take WinUI focus) and pins the hint string
///		that is a screen-reader user's only notice the route exists. A green run of both means the
///		decision is right and the affordance is in place. It does NOT mean a key ever reaches it.
///		<c>FloorReorderDropTests</c> draws the same line for the drop half of the same feature.
///		</para>
/// </remarks>
public class FloorReorderKeysTests
{
	[Fact]
	public void Decide_WithFewerThanTwoFloors_DoesNotReorder()
	{
		var decision = FloorReorderKeys.Decide(focusedFloorNumber: 1, floorCount: 1, moveUp: false);

		decision.ShouldReorder.Should().BeFalse();
		decision.TargetFloorNumber.Should().Be(0);
	}

	[Fact]
	public void Decide_WithAFocusedFloorNumberBelowOne_DoesNotReorder()
	{
		var decision = FloorReorderKeys.Decide(focusedFloorNumber: 0, floorCount: 5, moveUp: false);

		decision.ShouldReorder.Should().BeFalse();
		decision.TargetFloorNumber.Should().Be(0);
	}

	[Fact]
	public void Decide_WithAFocusedFloorNumberAboveTheFloorCount_DoesNotReorder()
	{
		var decision = FloorReorderKeys.Decide(focusedFloorNumber: 6, floorCount: 5, moveUp: true);

		decision.ShouldReorder.Should().BeFalse();
		decision.TargetFloorNumber.Should().Be(0);
	}

	/// <summary>
	///		The top floor cannot move up. This is also the boundary that must stay green under a
	///		direction inversion, so it is the positive control for this file: it returns
	///		<c>None</c> whichever way <c>Decide</c> computes its destination.
	/// </summary>
	[Fact]
	public void Decide_WithTheTopFloorFocusedAndMovingUp_DoesNotReorder()
	{
		var decision = FloorReorderKeys.Decide(focusedFloorNumber: 1, floorCount: 5, moveUp: true);

		decision.ShouldReorder.Should().BeFalse();
		decision.TargetFloorNumber.Should().Be(0);
	}

	[Fact]
	public void Decide_WithTheBottomFloorFocusedAndMovingDown_DoesNotReorder()
	{
		var decision = FloorReorderKeys.Decide(focusedFloorNumber: 5, floorCount: 5, moveUp: false);

		decision.ShouldReorder.Should().BeFalse();
		decision.TargetFloorNumber.Should().Be(0);
	}

	/// <summary>
	///		The first success row. The assertion pins the DIRECTION and not merely that something
	///		was returned: moving up means the destination is one LOWER than the focused number,
	///		because these are 1-based display numbers and floor 1 is the top. An implementation
	///		that added instead of subtracting would still reorder, still return a number inside the
	///		list, and move the floor the wrong way — which is the defect this assertion exists for.
	/// </summary>
	[Fact]
	public void Decide_WithAFloorBelowTheTopFocusedAndMovingUp_ReordersToTheNumberOneLower()
	{
		var decision = FloorReorderKeys.Decide(focusedFloorNumber: 3, floorCount: 5, moveUp: true);

		decision.ShouldReorder.Should().BeTrue();
		decision.TargetFloorNumber.Should().Be(2);
	}

	/// <summary>
	///		The second success row, and the mirror of the one above: moving down means one HIGHER.
	/// </summary>
	[Fact]
	public void Decide_WithAFloorAboveTheBottomFocusedAndMovingDown_ReordersToTheNumberOneHigher()
	{
		var decision = FloorReorderKeys.Decide(focusedFloorNumber: 3, floorCount: 5, moveUp: false);

		decision.ShouldReorder.Should().BeTrue();
		decision.TargetFloorNumber.Should().Be(4);
	}
}
