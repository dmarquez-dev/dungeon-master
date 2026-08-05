namespace DungeonMaster.Domain.ValueObjects;

/// <summary>
///     Identifies the center of a tile edge. TileCol/TileRow name the tile that "owns" the edge
///     (the tile the door faces into). Side is the edge of that tile.
/// </summary>
public record EdgePoint(int TileCol, int TileRow, EdgeSide Side);
