namespace DungeonMaster.Domain.ValueObjects;

/// <summary>
///		Represents a point on the tile grid using column and row coordinates.
/// </summary>
/// <param name="Col">
///		The zero-based column index of the tile.
/// </param>
/// <param name="Row">
///		The zero-based row index of the tile.
/// </param>
public record TilePoint(int Col, int Row);
