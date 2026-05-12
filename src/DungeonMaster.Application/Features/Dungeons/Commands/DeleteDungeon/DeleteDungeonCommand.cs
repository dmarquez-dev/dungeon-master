using DungeonMaster.Application.Common.Dispatch;

namespace DungeonMaster.Application.Features.Dungeons.Commands.DeleteDungeon;

/// <summary>
///		Command that permanently removes a dungeon by its identifier.
/// </summary>
/// <param name="Id">
///		The unique identifier of the dungeon to delete.
/// </param>
public record DeleteDungeonCommand(
	Guid Id)
		: ICommand<bool>;
