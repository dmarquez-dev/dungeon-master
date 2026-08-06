using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Application.Common.Results;

namespace DungeonMaster.Application.Features.Dungeons.Commands.AddPlacedObject;

/// <summary>
///		Handles the <see cref="AddPlacedObjectCommand"/> by loading the dungeon aggregate,
///		adding the new placed object to the target floor, and persisting the change.
///		Returns a not-found result if the dungeon does not exist, or a validation failure if the floor is not found.
/// </summary>
public sealed class AddPlacedObjectCommandHandler(
	IDungeonWriteRepository writeRepository)
		: ICommandHandler<AddPlacedObjectCommand, Guid>
{
	/// <inheritdoc/>
	public async Task<Result<Guid>> HandleAsync(
		AddPlacedObjectCommand command,
		CancellationToken ct)
	{
		var dungeon = await writeRepository.LoadAsync(
			command.DungeonId,
			ct);

		if (dungeon is null)
		{
			return Result<Guid>.Failure(
				$"Dungeon {command.DungeonId} not found.",
				ErrorCategory.NotFound);
		}

		var obj = dungeon.AddPlacedObject(
			command.FloorId,
			command.Kind,
			command.Position,
			command.Rotation,
			command.Scale);

		if (obj is null)
		{
			return Result<Guid>.Failure(
				"Failed to add placed object.",
				ErrorCategory.Validation);
		}

		await writeRepository.UpdateAsync(
			dungeon,
			ct);

		return Result<Guid>.Success(obj.Id);
	}
}
