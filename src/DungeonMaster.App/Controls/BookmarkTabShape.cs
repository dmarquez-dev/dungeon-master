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
		// as nothing at all, which reads as a missing control rather than a bug.
		var notch = (float)Math.Min(NotchDepth, bounds.Height);

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
