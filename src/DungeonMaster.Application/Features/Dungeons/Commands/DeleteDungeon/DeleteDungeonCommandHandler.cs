using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Application.Common.Results;
using DungeonMaster.Application.Features.Dungeons;

namespace DungeonMaster.Application.Features.Dungeons.Commands.DeleteDungeon;

/// <summary>
///		Handles the <see cref="DeleteDungeonCommand"/> by permanently removing the specified dungeon.
///		Returns a not-found result if no dungeon exists with the given identifier.
/// </summary>
public sealed class DeleteDungeonCommandHandler(
	IDungeonWriteRepository writeRepository)
		: ICommandHandler<DeleteDungeonCommand>
{
	/// <inheritdoc/>
	public async Task<Result> HandleAsync(
		DeleteDungeonCommand command,
		CancellationToken ct)
	{
		var deleted = await writeRepository.DeleteAsync(
			command.Id,
			ct);

		return deleted
			? Result.Success()
			: Result.Failure(
				$"Dungeon {command.Id} not found.",
				ErrorCategory.NotFound);
	}
}
