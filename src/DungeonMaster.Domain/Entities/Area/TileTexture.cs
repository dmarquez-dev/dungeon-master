using DungeonMaster.Domain.ValueObjects;

namespace DungeonMaster.Domain.Entities.Area;

/// <summary>
///		Represents a texture paint applied to a single tile within an area.
///		Position is area-relative tile coordinates (add area origin for absolute coordinates).
/// </summary>
public sealed class TileTexture : Entity
{
	private TileTexture() { }

	internal TileTexture(Guid areaId, TilePoint position, TextureKind kind)
	{
		AreaId = areaId;
		Position = position;
		Kind = kind;
	}

	public Guid AreaId { get; private set; }

	public TilePoint Position { get; private set; } = default!;

	public TextureKind Kind { get; private set; }

	internal void UpdateKind(TextureKind kind)
	{
		Kind = kind;
		Touch();
	}
}
