using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Application.Common.Results;
using DungeonMaster.Application.Features.Dungeons;

namespace DungeonMaster.Application.Features.Dungeons.Commands.RemoveFloor;

/// <summary>
///		Handles the <see cref="RemoveFloorCommand"/> by removing the specified floor from the dungeon aggregate.
///		Returns a not-found result if no dungeon exists with the given identifier.
/// </summary>
public sealed class RemoveFloorCommandHandler(
	IDungeonWriteRepository writeRepository)
		: ICommandHandler<RemoveFloorCommand, bool>
{
	/// <inheritdoc/>
	public async Task<Result<bool>> HandleAsync(
		RemoveFloorCommand command,
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

		dungeon.RemoveFloor(command.FloorId);

		await writeRepository.UpdateAsync(
			dungeon,
			ct);

		return Result<bool>.Success(true);
	}
}
