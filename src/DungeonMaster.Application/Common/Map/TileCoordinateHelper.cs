using DungeonMaster.Domain.ValueObjects;

namespace DungeonMaster.Application.Common.Map;

/// <summary>
///		Provides static utility methods for converting between tile-grid coordinates and pixel positions,
///		and for snapping raw pixel positions to the nearest grid point.
/// </summary>
public static class TileCoordinateHelper
{
	/// <summary>
	///		Snaps a raw pixel position to the nearest tile-grid point.
	/// </summary>
	/// <param name="x">
	///		The horizontal pixel position to snap.
	/// </param>
	/// <param name="y">
	///		The vertical pixel position to snap.
	/// </param>
	/// <param name="tileSize">
	///		The width and height of a single tile in pixels.
	/// </param>
	/// <returns>
	///		The <see cref="TilePoint"/> whose top-left corner is nearest to the given pixel position.
	/// </returns>
	public static TilePoint SnapToGrid(
		float x,
		float y,
		float tileSize)
	{
		var col = (int)MathF.Floor(x / tileSize + 0.5f);
		var row = (int)MathF.Floor(y / tileSize + 0.5f);

		return new TilePoint(col, row);
	}

	/// <summary>
	///		Converts a tile-grid point to the top-left pixel position of that tile.
	/// </summary>
	/// <param name="tile">
	///		The tile-grid point to convert.
	/// </param>
	/// <param name="tileSize">
	///		The width and height of a single tile in pixels.
	/// </param>
	/// <returns>
	///		A tuple containing the pixel X and Y coordinates of the tile's top-left corner.
	/// </returns>
	public static (float x, float y) ToPixel(
		TilePoint tile,
		float tileSize) =>
		(tile.Col * tileSize, tile.Row * tileSize);

	/// <summary>
	///		Determines whether a candidate tile point is within one tile's distance of the given origin.
	/// </summary>
	/// <param name="candidate">
	///		The tile point to test.
	/// </param>
	/// <param name="origin">
	///		The reference origin tile point.
	/// </param>
	/// <param name="tileSize">
	///		The width and height of a single tile in pixels.
	/// </param>
	/// <returns>
	///		<see langword="true"/> if the candidate is within one tile of the origin in both axes; otherwise <see langword="false"/>.
	/// </returns>
	public static bool IsNearOrigin(
		TilePoint candidate,
		TilePoint origin,
		float tileSize)
	{
		var dx = MathF.Abs((candidate.Col - origin.Col) * tileSize);
		var dy = MathF.Abs((candidate.Row - origin.Row) * tileSize);

		return dx <= tileSize && dy <= tileSize;
	}
}
