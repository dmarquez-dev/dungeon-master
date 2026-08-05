using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Domain.ValueObjects;

namespace DungeonMaster.Application.Features.Dungeons.Commands.ClearTileTexture;

public record ClearTileTextureCommand(
	Guid DungeonId,
	Guid FloorId,
	Guid AreaId,
	TilePoint Position)
		: ICommand<bool>;
