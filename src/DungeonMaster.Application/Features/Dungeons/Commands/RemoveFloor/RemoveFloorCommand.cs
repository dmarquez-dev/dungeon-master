using DungeonMaster.Application.Common.Dispatch;

namespace DungeonMaster.Application.Features.Dungeons.Commands.RemoveFloor;

/// <summary>
///		Command that removes a floor from an existing dungeon.
/// </summary>
/// <param name="DungeonId">
///		The unique identifier of the dungeon that contains the floor.
/// </param>
/// <param name="FloorId">
///		The unique identifier of the floor to remove.
/// </param>
public record RemoveFloorCommand(
	Guid DungeonId,
	Guid FloorId)
		: ICommand;
