using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Application.Common.Results;

namespace DungeonMaster.Application.Features.Dungeons.Commands.UpdateAreaOutline;

/// <summary>
///		Handles the <see cref="UpdateAreaOutlineCommand"/> by loading the aggregate, applying the outline update,
///		and persisting the change. Returns a not-found result if no dungeon exists with the given identifier.
/// </summary>
public sealed class UpdateAreaOutlineCommandHandler(
	IDungeonWriteRepository writeRepository)
		: ICommandHandler<UpdateAreaOutlineCommand, bool>
{
	/// <inheritdoc/>
	public async Task<Result<bool>> HandleAsync(
		UpdateAreaOutlineCommand command,
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

		dungeon.UpdateAreaOutline(
			command.FloorId,
			command.AreaId,
			command.Outline);

		await writeRepository.UpdateAsync(
			dungeon,
			ct);

		return Result<bool>.Success(true);
	}
}
