namespace DungeonMaster.App.Core.Editor;

/// <summary>
///		What a vertical slice of the floor list is, for the purpose of deciding who owns a drop
///		that landed on it.
/// </summary>
public enum FloorListBandKind
{
	/// <summary>
	///		A floor card. It carries a <c>DropGestureRecognizer</c> of its own AND it is a thing a
	///		reorder can name as its target.
	/// </summary>
	FloorCard,
}

/// <summary>
///		Who owns a drop that landed at a given point in the floor list.
/// </summary>
public enum FloorDropOwner
{
	/// <summary>
	///		Nobody. There is no band to resolve to, so the drop means nothing and stays silent —
	///		the same shape as contract C1 section 1's four silent returns.
	/// </summary>
	None,

	/// <summary>
	///		The point landed INSIDE an element that carries its own <c>DropGestureRecognizer</c>,
	///		so that element's handler owns this drop and the list must not act on it.
	/// </summary>
	ExistingTarget,

	/// <summary>
	///		The point missed every drop target. The list owns it, and resolves it to the floor card
	///		named by <see cref="FloorDropResolution.BandIndex"/>.
	/// </summary>
	FloorList,
}

/// <summary>
///		Which band a drop resolved to, and who owns it.
/// </summary>
/// <param name="Owner">
///		Who acts on this drop. Only <see cref="FloorDropOwner.FloorList"/> is the list's to handle.
/// </param>
/// <param name="BandIndex">
///		The index into the band list the drop resolved to, or <see cref="FloorReorderNearest.NoBand"/>
///		when <paramref name="Owner"/> is <see cref="FloorDropOwner.None"/>.
/// </param>
public readonly record struct FloorDropResolution(FloorDropOwner Owner, int BandIndex)
{
	/// <summary>
	///		The resolution that means "this drop resolves to nothing".
	/// </summary>
	public static FloorDropResolution None =>
		new(FloorDropOwner.None, FloorReorderNearest.NoBand);
}

/// <summary>
///		One element's vertical extent in the floor list, measured in the list's own coordinates.
/// </summary>
/// <param name="Kind">
///		What this band is. Only a <see cref="FloorListBandKind.FloorCard"/> can be resolved TO, and a
///		band of any kind can be landed IN.
/// </param>
/// <param name="Top">
///		The top edge of the element that carries the drop target, relative to the floor list.
/// </param>
/// <param name="Height">
///		The drop target's height. Zero or negative means "not laid out", and such a band is ignored
///		entirely rather than treated as a zero-height line the pointer can never be inside.
/// </param>
/// <remarks>
///		THE BAND IS THE DROP TARGET'S EXTENT, NOT THE CHILD'S. A floor card is a
///		<c>ContentView</c> 58 DIU tall whose root <c>Border</c> — the element that actually carries
///		the <c>DropGestureRecognizer</c> — is 54, the remaining 4 being the border's bottom margin.
///		Measured in the running application on 2026-09-12. Those 4 DIU are precisely the seam this
///		type exists to catch, so a band built from the child's 58 would swallow the gap and report
///		it as a hit.
/// </remarks>
public readonly record struct FloorListBand(FloorListBandKind Kind, double Top, double Height)
{
	/// <summary>
	///		The bottom edge, exclusive.
	/// </summary>
	public double Bottom =>
		Top + Height;

	/// <summary>
	///		The vertical centre, which is what a missed drop is resolved against.
	/// </summary>
	public double Midpoint =>
		Top + (Height / 2);
}

/// <summary>
///		Resolves a drop that landed on the floor list to the floor card it meant.
/// </summary>
/// <remarks>
///		<para>
///		WHY THIS EXISTS. At <c>b3b1641</c> <c>FloorListContainer</c> carried no
///		<c>DropGestureRecognizer</c> and <c>DungeonDetailPage.xaml</c> declared none at all, so a
///		release landing on the 4-DIU seam between two cards — or on the scrollbar strip, or on any
///		space no handler owned — was discarded with no handler, no error and no trace. The reorder
///		itself works end to end when the drop lands on a card; what failed was REACHING one, and it
///		failed SILENTLY, which is why the feature read as broken. Escalation E6.
///		</para>
///		<para>
///		WHY THE FIRST PASS IS "INSIDE AN EXISTING TARGET WINS" RATHER THAN JUST "NEAREST". The
///		floor list is a drop target in its own right, alongside each card's root <c>Border</c>.
///		Whether a drop landing on a card also reaches the list's handler depends on whether the platform bubbles the drop, which is
///		not something this code should have to know: resolving "inside a target" to
///		<see cref="FloorDropOwner.ExistingTarget"/> makes the list decline either way, so the
///		reorder runs exactly once whichever answer the platform gives.
///		</para>
///		<para>
///		THIS DECIDES WHICH CARD, AND NOTHING ELSE. What the drop then MEANS is
///		<see cref="FloorReorderDrop.Decide"/>'s, whose five rows contract C1 section 1 freezes —
///		including that a drop resolving to the dragged floor itself reorders nothing and says
///		nothing, because putting a floor back where it already is is a no-op rather than a failure.
///		</para>
/// </remarks>
public static class FloorReorderNearest
{
	/// <summary>
	///		The band index that means "no band".
	/// </summary>
	public const int NoBand = -1;

	/// <summary>
	///		Resolves a drop at <paramref name="y"/> against the list's bands.
	/// </summary>
	/// <param name="bands">
	///		The list's bands, in visual order. An empty or <see langword="null"/> list resolves to
	///		<see cref="FloorDropResolution.None"/>.
	/// </param>
	/// <param name="y">
	///		The drop's vertical position in the floor list's own coordinates. A non-finite value —
	///		which is what an unresolvable platform position degrades to — resolves to
	///		<see cref="FloorDropResolution.None"/> rather than to an arbitrary card.
	/// </param>
	/// <returns>
	///		Who owns the drop, and which band it resolved to.
	/// </returns>
	public static FloorDropResolution Resolve(IReadOnlyList<FloorListBand>? bands, double y)
	{
		if (bands is null || bands.Count == 0 || !double.IsFinite(y))
		{
			return FloorDropResolution.None;
		}

		// PASS ONE. Landing inside anything that carries its own drop target settles it: that
		// element's handler owns the drop and this one must not act, or the reorder runs twice.
		for (int index = 0; index < bands.Count; index++)
		{
			var band = bands[index];

			if (band.Height <= 0)
			{
				continue;
			}

			if (y >= band.Top && y < band.Bottom)
			{
				return new FloorDropResolution(FloorDropOwner.ExistingTarget, index);
			}
		}

		// PASS TWO. The drop missed every target, so it is a near-miss the list resolves itself.
		// Only a floor card can be resolved TO: a band of any other kind is not a floor, and
		// snapping to it would name a target that has no Number to reorder against.
		int nearest = NoBand;
		double nearestDistance = double.PositiveInfinity;

		for (int index = 0; index < bands.Count; index++)
		{
			var band = bands[index];

			if (band.Kind != FloorListBandKind.FloorCard || band.Height <= 0)
			{
				continue;
			}

			var distance = Math.Abs(y - band.Midpoint);

			// STRICTLY less than, so an exact tie keeps the UPPER card. A drop on the seam is
			// equidistant from both neighbours' midpoints whenever they are the same height, which
			// on this list is always, so the tie is the COMMON case here rather than a corner one
			// and it must not depend on iteration order changing.
			if (distance < nearestDistance)
			{
				nearestDistance = distance;
				nearest = index;
			}
		}

		return nearest == NoBand
			? FloorDropResolution.None
			: new FloorDropResolution(FloorDropOwner.FloorList, nearest);
	}
}
