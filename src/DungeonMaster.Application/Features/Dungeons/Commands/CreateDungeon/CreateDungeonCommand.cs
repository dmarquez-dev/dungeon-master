using DungeonMaster.Application.Common.Dispatch;

namespace DungeonMaster.Application.Features.Dungeons.Commands.CreateDungeon;

/// <summary>
///		Command that creates a new dungeon and returns the newly assigned identifier.
/// </summary>
/// <param name="Name">
///		The display name of the dungeon.
/// </param>
/// <param name="Description">
///		An optional description of the dungeon.
/// </param>
public record CreateDungeonCommand(
	string Name,
	string? Description)
		: ICommand<Guid>;
