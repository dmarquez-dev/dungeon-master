using DungeonMaster.Domain.ValueObjects;

namespace DungeonMaster.App.Core.Map;

/// <summary>
///		Provides the whole-foot distance between two tile-grid points.
///		Extracted from the map renderer so that the calculation is not mixed into a draw method.
/// </summary>
public static class TileDistance
{
	/// <summary>
	///		Returns the whole-foot distance between two tile-grid points.
	/// </summary>
	/// <remarks>
	///		Rounding is MathF.Round, which rounds a midpoint to the nearest even integer, so 2.5 rounds to 2
	///		and 3.5 rounds to 4. No argument is validated: a zero or negative <paramref name="feetPerTile"/>
	///		returns a zero or negative result, and a value that is not a number returns an unspecified result.
	/// </remarks>
	/// <param name="from">
	///		The tile-grid point to measure from.
	/// </param>
	/// <param name="to">
	///		The tile-grid point to measure to.
	/// </param>
	/// <param name="feetPerTile">
	///		The number of feet that a single tile represents.
	/// </param>
	/// <returns>
	///		The distance between the two points in whole feet.
	/// </returns>
	public static int InFeet(
		TilePoint from,
		TilePoint to,
		float feetPerTile)
	{
		var dx = to.Col - from.Col;
		var dy = to.Row - from.Row;
		var distanceInTiles = MathF.Sqrt(dx * dx + dy * dy);

		return (int)MathF.Round(distanceInTiles * feetPerTile);
	}
}
