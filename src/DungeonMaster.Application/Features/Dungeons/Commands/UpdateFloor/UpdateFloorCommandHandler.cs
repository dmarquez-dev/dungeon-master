using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Application.Common.Results;
using DungeonMaster.Application.Features.Dungeons;

namespace DungeonMaster.Application.Features.Dungeons.Commands.UpdateFloor;

/// <summary>
///		Handles the <see cref="UpdateFloorCommand"/> by loading the aggregate and applying the floor update.
///		Returns a not-found result if no dungeon exists with the given identifier.
/// </summary>
public sealed class UpdateFloorCommandHandler(
	IDungeonWriteRepository writeRepository)
		: ICommandHandler<UpdateFloorCommand, bool>
{
	/// <inheritdoc/>
	public async Task<Result<bool>> HandleAsync(
		UpdateFloorCommand command,
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

		dungeon.UpdateFloor(
			command.FloorId,
			command.Name,
			command.Description,
			command.Notes);

		await writeRepository.UpdateAsync(
			dungeon,
			ct);

		return Result<bool>.Success(true);
	}
}
