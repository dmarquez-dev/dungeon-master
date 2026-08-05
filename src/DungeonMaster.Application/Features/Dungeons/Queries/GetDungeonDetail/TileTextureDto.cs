using DungeonMaster.Domain.Entities.Area;
using DungeonMaster.Domain.ValueObjects;

namespace DungeonMaster.Application.Features.Dungeons.Queries.GetDungeonDetail;

public record TileTextureDto(
	Guid Id,
	TilePoint Position,
	TextureKind Kind);
