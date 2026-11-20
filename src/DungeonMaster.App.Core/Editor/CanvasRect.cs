namespace DungeonMaster.App.Core.Editor;

/// <summary>
///		Represents an axis-aligned rectangle in canvas pixels. It exists because the MAUI graphics
///		rectangle type is unavailable to this project, which takes no dependency on MAUI.
/// </summary>
/// <param name="X">
///		The horizontal pixel position of the rectangle's left edge.
/// </param>
/// <param name="Y">
///		The vertical pixel position of the rectangle's top edge.
/// </param>
/// <param name="Width">
///		The width of the rectangle in pixels.
/// </param>
/// <param name="Height">
///		The height of the rectangle in pixels.
/// </param>
public sealed record CanvasRect(float X, float Y, float Width, float Height)
{
	/// <summary>
	///		Determines whether the given point lies within this rectangle, inclusive on all four edges.
	/// </summary>
	/// <param name="x">
	///		The horizontal pixel position of the point to test.
	/// </param>
	/// <param name="y">
	///		The vertical pixel position of the point to test.
	/// </param>
	/// <returns>
	///		True when the point lies within the rectangle or on any of its edges.
	/// </returns>
	public bool Contains(float x, float y) =>
		x >= X && x <= X + Width &&
		y >= Y && y <= Y + Height;
}
