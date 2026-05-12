using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Application.Common.Results;
using DungeonMaster.Application.Features.Dungeons;

namespace DungeonMaster.Application.Features.Dungeons.Commands.ReorderFloor;

/// <summary>
///		Handles the <see cref="ReorderFloorCommand"/> by updating the ordinal position of the specified floor.
///		Returns a not-found result if no dungeon exists with the given identifier.
/// </summary>
public sealed class ReorderFloorCommandHandler(
	IDungeonWriteRepository writeRepository)
		: ICommandHandler<ReorderFloorCommand, bool>
{
	/// <inheritdoc/>
	public async Task<Result<bool>> HandleAsync(
		ReorderFloorCommand command,
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

		dungeon.ReorderFloor(
			command.FloorId,
			command.NewNumber);

		await writeRepository.UpdateAsync(
			dungeon,
			ct);

		return Result<bool>.Success(true);
	}
}
