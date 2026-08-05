using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Domain.Entities.Area;
using DungeonMaster.Domain.ValueObjects;

namespace DungeonMaster.Application.Features.Dungeons.Commands.SetTileTexture;

public record SetTileTextureCommand(
	Guid DungeonId,
	Guid FloorId,
	Guid AreaId,
	TilePoint Position,
	TextureKind Kind)
		: ICommand<bool>;
