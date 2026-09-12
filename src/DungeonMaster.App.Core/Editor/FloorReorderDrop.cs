namespace DungeonMaster.App.Core.Editor;

/// <summary>
///		What a drop onto a floor card means. Returned by <see cref="FloorReorderDrop.Decide"/> so the
///		decision is testable in this project while the MAUI-touching work stays in the view.
/// </summary>
/// <param name="ShouldReorder">
///		True only when the payload named a floor other than the target. False is the four silent
///		"this drop means nothing" cases of contract C1 section 1 and is not a failure.
/// </param>
/// <param name="DraggedFloorId">
///		The identifier parsed out of the drag payload. <see cref="System.Guid.Empty"/> when
///		<paramref name="ShouldReorder"/> is false.
/// </param>
/// <param name="TargetFloorNumber">
///		The target floor's 1-based <c>Number</c> — never its index in a visual-tree child collection
///		and never its index in the source collection. Zero when <paramref name="ShouldReorder"/> is
///		false.
/// </param>
public readonly record struct FloorReorderDecision(
	bool ShouldReorder,
	Guid DraggedFloorId,
	int TargetFloorNumber)
{
	/// <summary>
	///		The decision that means "this drop changes nothing".
	/// </summary>
	public static FloorReorderDecision None => default;
}

/// <summary>
///		The single owner of what a floor-reorder drop means.
/// </summary>
/// <remarks>
///		<para>
///		There are two drop targets on the floor sidebar — the floor card's own root border and the
///		transient gap placeholder the page inserts mid-drag — and before this type each one carried
///		its own hand-written copy of the same five-case decision. Two copies of one fact disagree
///		eventually; this is the one copy.
///		</para>
///		<para>
///		The payload parameter is nullable because MAUI's Windows gesture manager builds the drop
///		event's data from <c>datapackage?.View</c> — a nullable — in
///		<c>GesturePlatformManager.Windows.cs</c>'s <c>HandleDrop</c>. At Microsoft.Maui.Controls
///		10.0.41 the null never reaches here: <c>DropEventArgs</c>' internal constructor substitutes
///		<c>new DataPackageView(new DataPackage())</c>. That substitution is the fact worth carrying,
///		and it is why the first row of contract C1 section 1 is load-bearing rather than pedantic:
///		when the platform cannot recover the source package it hands the application an EMPTY one,
///		so "the platform lost the payload" and "the user dropped something irrelevant" arrive here
///		looking identical, and both are silent by contract.
///		</para>
/// </remarks>
public static class FloorReorderDrop
{
	/// <summary>
	///		The one property key the floor drag payload carries, holding the DRAGGED floor's
	///		identifier as an invariant "D"-format <see cref="System.Guid"/> string. Frozen by
	///		contract C1 section 1.
	/// </summary>
	public const string FloorIdKey = "FloorId";

	/// <summary>
	///		Decides whether a drop onto a floor card should reorder, and with what arguments.
	/// </summary>
	/// <param name="payload">
	///		The dropped data package's properties, or <see langword="null"/> when the platform could
	///		not recover a data package at all.
	/// </param>
	/// <param name="targetFloorId">
	///		The identifier of the floor whose card was dropped on, or <see langword="null"/> when
	///		that card is not currently bound to a floor.
	/// </param>
	/// <param name="targetFloorNumber">
	///		The target floor's 1-based display number.
	/// </param>
	/// <returns>
	///		A decision carrying the dragged floor's identifier and the target's number, or
	///		<see cref="FloorReorderDecision.None"/> for any of the four cases contract C1 section 1
	///		requires to return silently.
	/// </returns>
	public static FloorReorderDecision Decide(
		IReadOnlyDictionary<string, object>? payload,
		Guid? targetFloorId,
		int targetFloorNumber)
	{
		// No data package, or a card bound to no floor: nothing to reorder, and nothing has gone
		// wrong. Both are silent returns per contract C1 section 1.
		if (payload is null || targetFloorId is not Guid targetId)
		{
			return FloorReorderDecision.None;
		}

		if (!payload.TryGetValue(FloorIdKey, out var value))
		{
			return FloorReorderDecision.None;
		}

		if (value is not string text || !Guid.TryParse(text, out var draggedFloorId))
		{
			return FloorReorderDecision.None;
		}

		// Dropping a floor on itself is a no-op, not an error.
		if (draggedFloorId == targetId)
		{
			return FloorReorderDecision.None;
		}

		return new FloorReorderDecision(true, draggedFloorId, targetFloorNumber);
	}
}
