using DungeonMaster.Application.Common.Dispatch;

namespace DungeonMaster.Application.Features.Dungeons.Commands.AddFloor;

/// <summary>
///		Command that adds a new floor to an existing dungeon.
/// </summary>
/// <param name="DungeonId">
///		The unique identifier of the dungeon to add a floor to.
/// </param>
/// <param name="Name">
///		An optional display name for the new floor.
/// </param>
public record AddFloorCommand(
	Guid DungeonId,
	string? Name)
		: ICommand<Guid>;
