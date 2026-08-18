using DungeonMaster.Application.Common.Dispatch;

namespace DungeonMaster.Application.Features.Dungeons.Commands.UpdateFloor;

/// <summary>
///		Command that updates the name, description, and notes of a floor within a dungeon.
/// </summary>
/// <param name="DungeonId">
///		The unique identifier of the dungeon that contains the floor.
/// </param>
/// <param name="FloorId">
///		The unique identifier of the floor to update.
/// </param>
/// <param name="Name">
///		The new optional display name for the floor.
/// </param>
/// <param name="Description">
///		The new optional player-facing description for the floor.
/// </param>
/// <param name="Notes">
///		The new optional free-form notes for the floor.
/// </param>
public record UpdateFloorCommand(
	Guid DungeonId,
	Guid FloorId,
	string? Name,
	string? Description,
	string? Notes)
		: ICommand;
