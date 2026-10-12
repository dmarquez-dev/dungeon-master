namespace DungeonMaster.App.Controls;

/// <summary>
///		An <see cref="IShape"/> that draws a bookmark ribbon: a rectangle with an
///		upward triangular notch cut into the centre of its bottom edge.
/// </summary>
/// <remarks>
///		The path is computed from the measured bounds rather than being a fixed
///		geometry, so a ribbon stretches correctly to whatever width its label needs.
/// </remarks>
public sealed class BookmarkTabShape : IShape
{
	/// <summary>
	///		Gets or sets how far the notch cuts up into the ribbon, in device-independent units.
	/// </summary>
	public double NotchDepth { get; set; } = 10;

	/// <summary>
	///		Builds the ribbon path for the supplied bounds.
	/// </summary>
	/// <param name="bounds">
	///		The measured bounds of the border being shaped.
	/// </param>
	/// <returns>
	///		A closed <see cref="PathF"/> describing the notched ribbon.
	/// </returns>
	public PathF PathForBounds(Rect bounds)
	{
		var left = (float)bounds.Left;
		var top = (float)bounds.Top;
		var right = (float)bounds.Right;
		var bottom = (float)bounds.Bottom;

		// Never let the notch swallow the whole ribbon — a degenerate path renders
		// as nothing at all, which reads as a missing control rather than a bug. The
		// bound is HALF the height, not the whole height: at exactly bounds.Height the
		// apex sits on the top edge and the ribbon collapses to two triangles meeting
		// at a point, so the full height is itself the degenerate case rather than the
		// safe limit. Half keeps a strictly positive band of ribbon above the apex at
		// every positive height, and it scales with the ribbon instead of reserving a
		// fixed number of device-independent units.
		var notch = (float)Math.Min(NotchDepth, bounds.Height * 0.5);

		var path = new PathF();
		path.MoveTo(left, top);
		path.LineTo(right, top);
		path.LineTo(right, bottom);
		path.LineTo((float)bounds.Center.X, bottom - notch);
		path.LineTo(left, bottom);
		path.Close();

		return path;
	}
}
