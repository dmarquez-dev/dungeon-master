using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Domain.Entities.Area;
using DungeonMaster.Domain.ValueObjects;

namespace DungeonMaster.Application.Features.Dungeons.Commands.FloodFillTileTextures;

public record FloodFillTileTexturesCommand(
	Guid DungeonId,
	Guid FloorId,
	Guid AreaId,
	TilePoint SeedPosition,
	TextureKind Kind)
		: ICommand<int>;
