using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Application.Common.Results;

namespace DungeonMaster.Application.Features.Dungeons.Commands.UpdatePlacedObject;

/// <summary>
///		Handles the <see cref="UpdatePlacedObjectCommand"/> by loading the dungeon aggregate,
///		applying the position, rotation, and scale update, and persisting the change.
///		Returns a not-found result if the dungeon does not exist.
///		Has no effect if the floor, area, or object is not found within the aggregate.
/// </summary>
public sealed class UpdatePlacedObjectCommandHandler(
	IDungeonWriteRepository writeRepository)
		: ICommandHandler<UpdatePlacedObjectCommand, bool>
{
	/// <inheritdoc/>
	public async Task<Result<bool>> HandleAsync(
		UpdatePlacedObjectCommand command,
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

		dungeon.UpdatePlacedObject(
			command.FloorId,
			command.AreaId,
			command.ObjectId,
			command.NewPosition,
			command.NewRotation,
			command.NewScale);

		await writeRepository.UpdateAsync(
			dungeon,
			ct);

		return Result<bool>.Success(true);
	}
}
