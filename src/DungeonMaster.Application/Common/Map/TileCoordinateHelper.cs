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
	///		Returns the <see cref="TilePoint"/> of the tile that contains the given pixel position.
	///		Uses floor division, so the top-left corner of each tile maps to that tile.
	/// </summary>
	public static TilePoint SnapToTileCenter(
		float x,
		float y,
		float tileSize)
	{
		var col = (int)MathF.Floor(x / tileSize);
		var row = (int)MathF.Floor(y / tileSize);

		return new TilePoint(col, row);
	}

	/// <summary>
	///		Returns the <see cref="VertexPoint"/> at the nearest tile-grid intersection to the given pixel position.
	/// </summary>
	public static VertexPoint SnapToNearestVertex(
		float x,
		float y,
		float tileSize)
	{
		var col = (int)MathF.Round(x / tileSize);
		var row = (int)MathF.Round(y / tileSize);

		return new VertexPoint(col, row);
	}

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

	/// <summary>
	///     Returns the nearest tile-edge center to the given pixel position, along with its distance.
	///     Canonical convention: cursor below a horizontal edge → N side of lower tile;
	///     cursor above → S side of upper tile. Cursor right of a vertical edge → W side of right tile;
	///     cursor left → E side of left tile.
	/// </summary>
	public static (EdgePoint EdgeCenter, float Distance) SnapToNearestEdgeCenter(
		float x,
		float y,
		float tileSize)
	{
		// Nearest horizontal edge (N/S family)
		var nsRow = (int)MathF.Round(y / tileSize);
		var nsCol = (int)MathF.Floor(x / tileSize);
		var nsCenterX = (nsCol + 0.5f) * tileSize;
		var nsCenterY = nsRow * tileSize;
		var nsDist = MathF.Sqrt((x - nsCenterX) * (x - nsCenterX) + (y - nsCenterY) * (y - nsCenterY));
		var nsEdge = y >= nsCenterY
			? new EdgePoint(nsCol, nsRow, EdgeSide.N)
			: new EdgePoint(nsCol, nsRow - 1, EdgeSide.S);

		// Nearest vertical edge (E/W family)
		var ewCol = (int)MathF.Round(x / tileSize);
		var ewRow = (int)MathF.Floor(y / tileSize);
		var ewCenterX = ewCol * tileSize;
		var ewCenterY = (ewRow + 0.5f) * tileSize;
		var ewDist = MathF.Sqrt((x - ewCenterX) * (x - ewCenterX) + (y - ewCenterY) * (y - ewCenterY));
		var ewEdge = x >= ewCenterX
			? new EdgePoint(ewCol, ewRow, EdgeSide.W)
			: new EdgePoint(ewCol - 1, ewRow, EdgeSide.E);

		return nsDist <= ewDist
			? (nsEdge, nsDist)
			: (ewEdge, ewDist);
	}

	/// <summary>
	///     Returns the pixel distance from the given position to the nearest tile-grid vertex.
	/// </summary>
	public static float DistanceToNearestVertex(float x, float y, float tileSize)
	{
		var vertex = SnapToNearestVertex(x, y, tileSize);
		var vx = vertex.Col * tileSize;
		var vy = vertex.Row * tileSize;
		return MathF.Sqrt((x - vx) * (x - vx) + (y - vy) * (y - vy));
	}
}
