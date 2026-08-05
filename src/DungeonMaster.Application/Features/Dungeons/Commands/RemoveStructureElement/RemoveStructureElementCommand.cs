using DungeonMaster.Application.Common.Dispatch;

namespace DungeonMaster.Application.Features.Dungeons.Commands.RemoveStructureElement;

public record RemoveStructureElementCommand(
	Guid DungeonId,
	Guid FloorId,
	Guid AreaId,
	Guid ElementId)
		: ICommand<bool>;
