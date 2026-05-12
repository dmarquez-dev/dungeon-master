using DungeonMaster.Application.Common.Dispatch;

namespace DungeonMaster.Application.Features.Dungeons.Commands.RemoveArea;

/// <summary>
///		Command that removes an area from a floor within an existing dungeon.
/// </summary>
/// <param name="DungeonId">
///		The unique identifier of the dungeon that contains the target floor.
/// </param>
/// <param name="FloorId">
///		The unique identifier of the floor that contains the area.
/// </param>
/// <param name="AreaId">
///		The unique identifier of the area to remove.
/// </param>
public record RemoveAreaCommand(
	Guid DungeonId,
	Guid FloorId,
	Guid AreaId)
		: ICommand<bool>;
