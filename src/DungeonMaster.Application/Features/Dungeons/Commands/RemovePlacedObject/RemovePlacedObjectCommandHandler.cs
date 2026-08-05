using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Application.Common.Results;

namespace DungeonMaster.Application.Features.Dungeons.Commands.RemovePlacedObject;

/// <summary>
///		Handles the <see cref="RemovePlacedObjectCommand"/> by loading the dungeon aggregate,
///		removing the placed object, and persisting the change.
///		Returns a not-found result if the dungeon does not exist.
///		Has no effect if the floor or object is not found within the aggregate.
/// </summary>
public sealed class RemovePlacedObjectCommandHandler(
	IDungeonWriteRepository writeRepository)
		: ICommandHandler<RemovePlacedObjectCommand, bool>
{
	/// <inheritdoc/>
	public async Task<Result<bool>> HandleAsync(
		RemovePlacedObjectCommand command,
		CancellationToken ct)
	{
		var dungeon = await writeRepository.LoadAsync(
			command.DungeonId,
			ct);

		if (dungeon is null)
		{
			return Result<bool>.Failure(
				$"Dungeon {command.DungeonId} not found.",
				ErrorCategory.NotFound);
		}

		dungeon.RemovePlacedObject(
			command.FloorId,
			command.ObjectId);

		await writeRepository.UpdateAsync(
			dungeon,
			ct);

		return Result<bool>.Success(true);
	}
}
