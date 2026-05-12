using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Application.Common.Results;
using DungeonMaster.Application.Features.Dungeons;

namespace DungeonMaster.Application.Features.Dungeons.Commands.UpdateDungeon;

/// <summary>
///		Handles the <see cref="UpdateDungeonCommand"/> by loading the aggregate and applying the update.
///		Returns a not-found result if no dungeon exists with the given identifier.
/// </summary>
public sealed class UpdateDungeonCommandHandler(
	IDungeonWriteRepository writeRepository)
		: ICommandHandler<UpdateDungeonCommand, bool>
{
	/// <inheritdoc/>
	public async Task<Result<bool>> HandleAsync(
		UpdateDungeonCommand command,
		CancellationToken ct)
	{
		var dungeon = await writeRepository.LoadAsync(
			command.Id,
			ct);

		if (dungeon is null)
		{
			return Result<bool>.Failure(
				$"Dungeon {command.Id} not found.",
				ErrorCategory.NotFound);
		}

		dungeon.Update(
			command.Name,
			command.Description,
			command.Notes);

		await writeRepository.UpdateAsync(
			dungeon,
			ct);

		return Result<bool>.Success(true);
	}
}
