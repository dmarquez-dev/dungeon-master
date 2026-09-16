namespace DungeonMaster.App.Core.Editor;

/// <summary>
///		What a keyboard reorder keystroke means. Returned by <see cref="FloorReorderKeys.Decide"/> so
///		the decision is testable in this project while the MAUI-touching work stays in the view.
/// </summary>
/// <param name="ShouldReorder">
///		True only when the keystroke moves the focused floor to a different position. False is every
///		silent "this keystroke means nothing" case of contract C1 section 4.1 and is not a failure.
/// </param>
/// <param name="TargetFloorNumber">
///		The 1-based floor <c>Number</c> the focused floor should end up at — the same display number
///		<c>FloorItem.Number</c> carries. It is NOT an index into the floor collection and NOT an
///		index into a visual-tree child collection, the same distinction
///		<see cref="FloorReorderDecision.TargetFloorNumber"/> draws for the same reason. Zero when
///		<paramref name="ShouldReorder"/> is false.
/// </param>
public readonly record struct FloorKeyboardReorderDecision(bool ShouldReorder, int TargetFloorNumber)
{
	/// <summary>
	///		The decision that means "this keystroke changes nothing".
	/// </summary>
	public static FloorKeyboardReorderDecision None => default;
}

/// <summary>
///		The single owner of what a floor-reorder keystroke means.
/// </summary>
/// <remarks>
///		<para>
///		This type takes a <c>bool moveUp</c> rather than a key: mapping a key onto that bool is the
///		page's job. It references no key type and no MAUI type on purpose, which is what keeps it in
///		a plain <c>net10.0</c> project the test project already references, so the decision is
///		testable without a MAUI host. Frozen by contract C1 section 4.1.
///		</para>
///		<para>
///		No code path here raises an exception. Every rejected keystroke — an empty or single-floor list, a floor number
///		outside the list, or an attempt to move the top floor up or the bottom floor down — is an
///		expected outcome the caller handles by doing nothing, not a defect. A keystroke pressed with
///		nothing reorderable focused is not a failure and must not surface an error to the user.
///		</para>
/// </remarks>
public static class FloorReorderKeys
{
	/// <summary>
	///		Decides whether a keyboard reorder keystroke should reorder, and to which floor number.
	/// </summary>
	/// <param name="focusedFloorNumber">
	///		The 1-based <c>Number</c> of the floor whose reorder handle holds keyboard focus.
	/// </param>
	/// <param name="floorCount">
	///		How many floors the sidebar currently lists.
	/// </param>
	/// <param name="moveUp">
	///		<see langword="true"/> to move the focused floor one position up, <see langword="false"/>
	///		to move it one position down.
	/// </param>
	/// <returns>
	///		A decision carrying the destination floor number, or
	///		<see cref="FloorKeyboardReorderDecision.None"/> for any of the five cases contract C1
	///		section 4.1 requires to return silently.
	/// </returns>
	public static FloorKeyboardReorderDecision Decide(int focusedFloorNumber, int floorCount, bool moveUp)
	{
		// Nothing to reorder against: a list of one floor, or of none.
		if (floorCount < 2)
		{
			return FloorKeyboardReorderDecision.None;
		}

		// A focused number outside the list describes no floor. Silent, per contract C1 section 4.1.
		if (focusedFloorNumber < 1 || focusedFloorNumber > floorCount)
		{
			return FloorKeyboardReorderDecision.None;
		}

		// Already at the end the keystroke asks for: the top floor cannot move up, the bottom floor
		// cannot move down. Both are no-ops, not errors.
		if (moveUp && focusedFloorNumber == 1)
		{
			return FloorKeyboardReorderDecision.None;
		}

		if (!moveUp && focusedFloorNumber == floorCount)
		{
			return FloorKeyboardReorderDecision.None;
		}

		return new FloorKeyboardReorderDecision(true, focusedFloorNumber + (moveUp ? -1 : 1));
	}
}
