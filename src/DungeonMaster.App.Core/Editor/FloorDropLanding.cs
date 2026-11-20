namespace DungeonMaster.App.Core.Editor;

/// <summary>
///		Where a floor being dragged would land if it were released at the current pointer position.
///		Returned by <see cref="FloorDropIndicator.Locate"/> so the page can draw the insertion line without deciding anything itself.
/// </summary>
/// <param name="TargetIndex">
///		The index of the floor card a drop here would hand to <see cref="FloorReorderDrop.Decide"/>.
///		It is <see cref="FloorReorderNearest.NoBand"/> when there is no landing.
/// </param>
/// <param name="GapIndex">
///		The gap between cards where the dragged floor would end up, where gap 0 lies above the first card and gap N lies below the last of N cards.
///		It is <see cref="FloorReorderNearest.NoBand"/> when there is no landing.
/// </param>
/// <param name="LineY">
///		The vertical position of that gap in the floor list's own coordinates.
///		It is <see cref="double.NaN"/> when there is no landing.
/// </param>
/// <remarks>
///		A default value reports <see cref="HasLanding"/> as true, because default sets <see cref="TargetIndex"/> to 0 and not to <see cref="FloorReorderNearest.NoBand"/>.
///		Use <see cref="None"/>, not default, to mean that there is no landing.
/// </remarks>
public readonly record struct FloorDropLanding(
	int TargetIndex,
	int GapIndex,
	double LineY)
{
	/// <summary>
	///		The landing that means "a release here changes nothing, so no line is shown".
	/// </summary>
	public static FloorDropLanding None =>
		new(
			FloorReorderNearest.NoBand,
			FloorReorderNearest.NoBand,
			double.NaN);

	/// <summary>
	///		True when a release here would reorder, which is exactly when <see cref="TargetIndex"/> names a band.
	/// </summary>
	public bool HasLanding =>
		TargetIndex != FloorReorderNearest.NoBand;
}
