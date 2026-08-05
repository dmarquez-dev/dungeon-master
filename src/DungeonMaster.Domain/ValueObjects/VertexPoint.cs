namespace DungeonMaster.Domain.ValueObjects;

/// <summary>
///		Represents a tile-grid intersection point (corner between four tiles).
///		Semantically distinct from <see cref="TilePoint"/> (tile center) to prevent accidental misuse.
/// </summary>
public record VertexPoint(int Col, int Row);
