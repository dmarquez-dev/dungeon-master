using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Application.Common.Interfaces;
using DungeonMaster.Application.Common.Results;

namespace DungeonMaster.Application.Features.Dungeons.Commands.DeleteDungeon;

/// <summary>
///		Handles the <see cref="DeleteDungeonCommand"/> by permanently removing the specified dungeon.
///		Returns a not-found result if no dungeon exists with the given identifier.
/// </summary>
public sealed class DeleteDungeonCommandHandler(
	IUnitOfWorkFactory unitOfWorkFactory)
		: ICommandHandler<DeleteDungeonCommand>
{
	/// <inheritdoc/>
	public async Task<Result> HandleAsync(
		DeleteDungeonCommand command,
		CancellationToken cancellationToken)
	{
		await using var unitOfWork = await unitOfWorkFactory.CreateAsync(cancellationToken);

		var removed = await unitOfWork.Dungeons.RemoveAsync(
			command.Id,
			cancellationToken);

		if (!removed)
		{
			return Result.Failure(
				$"Dungeon {command.Id} not found.",
				ErrorCategory.NotFound);
		}

		await unitOfWork.CommitAsync(cancellationToken);

		return Result.Success();
	}
}
