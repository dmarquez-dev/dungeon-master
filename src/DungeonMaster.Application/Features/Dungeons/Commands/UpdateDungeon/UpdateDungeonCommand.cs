using DungeonMaster.Application.Common.Dispatch;

namespace DungeonMaster.Application.Features.Dungeons.Commands.UpdateDungeon;

/// <summary>
///		Command that updates the name, description, and notes of an existing dungeon.
/// </summary>
/// <param name="Id">
///		The unique identifier of the dungeon to update.
/// </param>
/// <param name="Name">
///		The new display name for the dungeon.
/// </param>
/// <param name="Description">
///		The new optional description for the dungeon.
/// </param>
/// <param name="Notes">
///		The new optional free-form notes for the dungeon.
/// </param>
public record UpdateDungeonCommand(
	Guid Id,
	string Name,
	string? Description,
	string? Notes)
		: ICommand<bool>;
