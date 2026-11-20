using DungeonMaster.Application.Common.Dispatch;

namespace DungeonMaster.Application.Features.Dungeons.Commands.ReorderFloor;

/// <summary>
///		Command that changes the ordinal position of a floor within a dungeon.
/// </summary>
/// <param name="DungeonId">
///		The unique identifier of the dungeon that contains the floor.
/// </param>
/// <param name="FloorId">
///		The unique identifier of the floor to reorder.
/// </param>
/// <param name="NewNumber">
///		The new ordinal number to assign to the floor.
/// </param>
public record ReorderFloorCommand(
	Guid DungeonId,
	Guid FloorId,
	int NewNumber)
		: ICommand;
