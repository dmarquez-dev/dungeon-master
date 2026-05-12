namespace DungeonMaster.App.Graphics;

/// <summary>
///		Draws a uniform grid of faint gold lines onto a <see cref="GraphicsView"/> to represent a dungeon map background.
/// </summary>
public sealed class MapGridDrawable : IDrawable
{
	/// <summary>
	///		Renders the grid onto the provided canvas, covering the full dirty region.
	/// </summary>
	/// <param name="canvas">
	///		The canvas on which to draw the grid lines.
	/// </param>
	/// <param name="dirtyRect">
	///		The rectangular region that must be redrawn.
	/// </param>
	public void Draw(ICanvas canvas, RectF dirtyRect)
	{
		canvas.StrokeColor = Color.FromArgb("#1AC4862A");
		canvas.StrokeSize = 0.5f;

		const float tileSize = 20f;

		for (var x = 0f; x <= dirtyRect.Width; x += tileSize)
		{
			canvas.DrawLine(
				x,
				0,
				x,
				dirtyRect.Height);
		}

		for (var y = 0f; y <= dirtyRect.Height; y += tileSize)
		{
			canvas.DrawLine(
				0,
				y,
				dirtyRect.Width,
				y);
		}
	}
}
