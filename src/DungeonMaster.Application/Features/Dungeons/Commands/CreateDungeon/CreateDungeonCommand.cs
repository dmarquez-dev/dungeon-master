using DungeonMaster.Application.Common.Dispatch;

namespace DungeonMaster.Application.Features.Dungeons.Commands.CreateDungeon;

/// <summary>
///		Command that creates a new dungeon with the given name and optional description.
/// </summary>
/// <param name="Name">
///		The display name for the new dungeon.
/// </param>
/// <param name="Description">
///		An optional short description of the dungeon.
/// </param>
public record CreateDungeonCommand(
	string Name,
	string? Description)
		: ICommand<Guid>;
