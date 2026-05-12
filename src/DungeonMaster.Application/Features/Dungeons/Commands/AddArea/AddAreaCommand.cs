using DungeonMaster.Application.Common.Dispatch;

namespace DungeonMaster.Application.Features.Dungeons.Commands.AddArea;

/// <summary>
///		Command that adds a new area to a floor within an existing dungeon.
/// </summary>
/// <param name="DungeonId">
///		The unique identifier of the dungeon that contains the target floor.
/// </param>
/// <param name="FloorId">
///		The unique identifier of the floor to add the area to.
/// </param>
/// <param name="Name">
///		The display name for the new area.
/// </param>
/// <param name="Description">
///		An optional short description of the new area.
/// </param>
public record AddAreaCommand(
	Guid DungeonId,
	Guid FloorId,
	string Name,
	string? Description)
		: ICommand<Guid>;
