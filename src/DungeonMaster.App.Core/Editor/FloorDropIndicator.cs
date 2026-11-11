namespace DungeonMaster.App.Core.Editor;

/// <summary>
///		Resolves where a floor being dragged would land, so the page can mark that slot with an insertion line.
/// </summary>
/// <remarks>
///		<para>
///		THE ONE-OWNER RULE. The card this names is the card that a release at the same point would hand to <see cref="FloorReorderDrop.Decide"/>.
///		A release inside a card band is that card's own drop, and a near-miss is the list's drop resolved through <see cref="FloorReorderNearest.Resolve"/>.
///		Both owners take the band index from that one resolution, so the line can never mark a slot the drop would not use.
///		</para>
///		<para>
///		WHY THE GAP IS THE LANDING. <c>Dungeon.ReorderFloor</c> gives the dragged floor the target's number and shifts every floor between the old and new numbers by one.
///		The dragged floor therefore ends at the target's 0-based position.
///		While the dragged card still occupies its own slot, that position is the gap below the target when moving down and the gap above it when moving up.
///		</para>
/// </remarks>
public static class FloorDropIndicator
{
	/// <summary>
	///		Locates the landing for a drag over the floor list.
	///		Every unusable input returns <see cref="FloorDropLanding.None"/>, and nothing is thrown.
	/// </summary>
	/// <param name="bands">
	///		The list's bands in visual order, measured in the floor list's own coordinates.
	/// </param>
	/// <param name="y">
	///		The pointer's vertical position in the same coordinates.
	/// </param>
	/// <param name="draggedIndex">
	///		The index among <paramref name="bands"/> of the card being dragged.
	/// </param>
	/// <returns>
	///		The target card, the gap it lands in and that gap's vertical position, or <see cref="FloorDropLanding.None"/> when a release here would change nothing.
	/// </returns>
	public static FloorDropLanding Locate(
		IReadOnlyList<FloorListBand>? bands,
		double y,
		int draggedIndex)
	{
		if (bands is null || bands.Count == 0 || !double.IsFinite(y))
		{
			return FloorDropLanding.None;
		}

		if (draggedIndex < 0 || draggedIndex >= bands.Count)
		{
			return FloorDropLanding.None;
		}

		var resolution = FloorReorderNearest.Resolve(
			bands,
			y);

		if (resolution.Owner == FloorDropOwner.None)
		{
			return FloorDropLanding.None;
		}

		var targetIndex = resolution.BandIndex;

		// Dropping a floor in its own slot is a no-op, so there is nothing to mark.
		if (targetIndex == draggedIndex)
		{
			return FloorDropLanding.None;
		}

		if (bands[targetIndex].Kind != FloorListBandKind.FloorCard)
		{
			return FloorDropLanding.None;
		}

		var gapIndex = draggedIndex < targetIndex
			? targetIndex + 1
			: targetIndex;

		return new FloorDropLanding(
			targetIndex,
			gapIndex,
			GapY(
				bands,
				gapIndex));
	}

	private static double GapY(
		IReadOnlyList<FloorListBand> bands,
		int gapIndex)
	{
		if (gapIndex == 0)
		{
			return bands[0].Top;
		}

		if (gapIndex == bands.Count)
		{
			return bands[^1].Bottom;
		}

		// The middle of the seam between the two neighbouring bands, whatever their heights.
		return (bands[gapIndex - 1].Bottom + bands[gapIndex].Top) / 2;
	}
}
