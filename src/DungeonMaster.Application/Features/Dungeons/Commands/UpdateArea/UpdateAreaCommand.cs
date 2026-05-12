using DungeonMaster.Application.Common.Dispatch;

namespace DungeonMaster.Application.Features.Dungeons.Commands.UpdateArea;

/// <summary>
///		Command that updates the name, description, and notes of an area within a dungeon floor.
/// </summary>
/// <param name="DungeonId">
///		The unique identifier of the dungeon that contains the target floor.
/// </param>
/// <param name="FloorId">
///		The unique identifier of the floor that contains the area.
/// </param>
/// <param name="AreaId">
///		The unique identifier of the area to update.
/// </param>
/// <param name="Name">
///		The new display name for the area.
/// </param>
/// <param name="Description">
///		The new optional description for the area.
/// </param>
/// <param name="Notes">
///		The new optional free-form notes for the area.
/// </param>
public record UpdateAreaCommand(
	Guid DungeonId,
	Guid FloorId,
	Guid AreaId,
	string Name,
	string? Description,
	string? Notes)
		: ICommand<bool>;
