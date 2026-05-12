using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Application.Common.Results;
using DungeonMaster.Application.Features.Dungeons;

namespace DungeonMaster.Application.Features.Dungeons.Commands.RemoveArea;

/// <summary>
///		Handles the <see cref="RemoveAreaCommand"/> by removing the specified area from the dungeon aggregate.
///		Returns a not-found result if no dungeon exists with the given identifier.
/// </summary>
public sealed class RemoveAreaCommandHandler(
	IDungeonWriteRepository writeRepository)
		: ICommandHandler<RemoveAreaCommand, bool>
{
	/// <inheritdoc/>
	public async Task<Result<bool>> HandleAsync(
		RemoveAreaCommand command,
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

		dungeon.RemoveArea(
			command.FloorId,
			command.AreaId);

		await writeRepository.UpdateAsync(
			dungeon,
			ct);

		return Result<bool>.Success(true);
	}
}
