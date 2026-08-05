using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Application.Common.Results;

namespace DungeonMaster.Application.Features.Dungeons.Commands.ClearTileTexture;

public sealed class ClearTileTextureCommandHandler(
	IDungeonWriteRepository writeRepository)
		: ICommandHandler<ClearTileTextureCommand, bool>
{
	public async Task<Result<bool>> HandleAsync(ClearTileTextureCommand command, CancellationToken ct)
	{
		var dungeon = await writeRepository.LoadAsync(command.DungeonId, ct);

		if (dungeon is null)
			return Result<bool>.Failure($"Dungeon {command.DungeonId} not found.", ErrorCategory.NotFound);

		dungeon.ClearTileTexture(command.FloorId, command.AreaId, command.Position);

		await writeRepository.UpdateAsync(dungeon, ct);

		return Result<bool>.Success(true);
	}
}
