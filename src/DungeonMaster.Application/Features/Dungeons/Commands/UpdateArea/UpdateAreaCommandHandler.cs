using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Application.Common.Results;
using DungeonMaster.Application.Features.Dungeons;

namespace DungeonMaster.Application.Features.Dungeons.Commands.UpdateArea;

/// <summary>
///		Handles the <see cref="UpdateAreaCommand"/> by loading the aggregate and applying the area update.
///		Returns a not-found result if no dungeon exists with the given identifier.
/// </summary>
public sealed class UpdateAreaCommandHandler(
	IDungeonWriteRepository writeRepository)
		: ICommandHandler<UpdateAreaCommand, bool>
{
	/// <inheritdoc/>
	public async Task<Result<bool>> HandleAsync(
		UpdateAreaCommand command,
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

		dungeon.UpdateArea(
			command.FloorId,
			command.AreaId,
			command.Name,
			command.Description,
			command.Notes);

		await writeRepository.UpdateAsync(
			dungeon,
			ct);

		return Result<bool>.Success(true);
	}
}
