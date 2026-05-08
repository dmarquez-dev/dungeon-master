using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Application.Common.Results;

namespace DungeonMaster.Application.Features.Dungeons.Commands.DeleteDungeon;

/// <summary>
///		Handles <see cref="DeleteDungeonCommand"/> by delegating removal to the dungeon repository.
/// </summary>
public sealed class DeleteDungeonCommandHandler(
	IDungeonRepository dungeonRepository)
		: ICommandHandler<DeleteDungeonCommand, bool>
{
	/// <summary>
	///		Deletes the dungeon identified by the command and returns a successful result.
	/// </summary>
	/// <param name="command">
	///		The command carrying the identifier of the dungeon to delete.
	/// </param>
	/// <param name="ct">
	///		A token that may signal cancellation.
	/// </param>
	public async Task<Result<bool>> HandleAsync(
		DeleteDungeonCommand command,
		CancellationToken ct)
	{
		await dungeonRepository.DeleteAsync(
			command.Id,
			ct);

		return Result<bool>.Success(true);
	}
}
