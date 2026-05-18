using DungeonMaster.Domain.ValueObjects;

namespace DungeonMaster.Application.Common.Map;

/// <summary>
///		Provides utility methods for converting canvas-space coordinates to tile-grid coordinates.
/// </summary>
public static class SnapZoneCalculator
{
	/// <summary>
	///		Converts a canvas-space position to its tile column/row.
	/// </summary>
	/// <remarks>
	///		Uses floor division so that negative coordinates map to the tile to the
	///		left/above the origin rather than truncating toward zero.
	///		For example, a canvas X of -1 with a tile size of 32 yields column -1, not 0.
	/// </remarks>
	/// <param name="canvasX">
	///		The canvas-space X coordinate.
	/// </param>
	/// <param name="canvasY">
	///		The canvas-space Y coordinate.
	/// </param>
	/// <param name="tileSize">
	///		The size of a single tile in canvas pixels.
	/// </param>
	public static TilePoint ToTile(
		float canvasX,
		float canvasY,
		float tileSize) =>
			new(
				(int)MathF.Floor(canvasX / tileSize),
				(int)MathF.Floor(canvasY / tileSize));
}
