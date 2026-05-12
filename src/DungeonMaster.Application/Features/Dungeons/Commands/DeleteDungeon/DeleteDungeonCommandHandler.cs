using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Application.Common.Results;
using DungeonMaster.Application.Features.Dungeons;

namespace DungeonMaster.Application.Features.Dungeons.Commands.DeleteDungeon;

/// <summary>
///		Handles the <see cref="DeleteDungeonCommand"/> by permanently removing the specified dungeon.
/// </summary>
public sealed class DeleteDungeonCommandHandler(
	IDungeonWriteRepository writeRepository)
		: ICommandHandler<DeleteDungeonCommand, bool>
{
	/// <inheritdoc/>
	public async Task<Result<bool>> HandleAsync(
		DeleteDungeonCommand command,
		CancellationToken ct)
	{
		await writeRepository.DeleteAsync(
			command.Id,
			ct);

		return Result<bool>.Success(true);
	}
}
