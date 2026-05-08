using DungeonMaster.Application.Common.Dispatch;

namespace DungeonMaster.Application.Features.Dungeons.Commands.DeleteDungeon;

/// <summary>
///		Command that deletes an existing dungeon by identifier.
///		Returns <see langword="true"/> when the delete completes successfully.
/// </summary>
/// <param name="Id">
///		The unique identifier of the dungeon to delete.
/// </param>
public record DeleteDungeonCommand(
	Guid Id)
		: ICommand<bool>;
