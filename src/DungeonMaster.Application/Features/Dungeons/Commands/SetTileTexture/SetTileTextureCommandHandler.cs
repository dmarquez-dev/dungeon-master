using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Application.Common.Results;

namespace DungeonMaster.Application.Features.Dungeons.Commands.SetTileTexture;

public sealed class SetTileTextureCommandHandler(
	IDungeonWriteRepository writeRepository)
		: ICommandHandler<SetTileTextureCommand, bool>
{
	public async Task<Result<bool>> HandleAsync(SetTileTextureCommand command, CancellationToken ct)
	{
		var dungeon = await writeRepository.LoadAsync(command.DungeonId, ct);

		if (dungeon is null)
			return Result<bool>.Failure($"Dungeon {command.DungeonId} not found.", ErrorCategory.NotFound);

		dungeon.SetTileTexture(command.FloorId, command.AreaId, command.Position, command.Kind);

		await writeRepository.UpdateAsync(dungeon, ct);

		return Result<bool>.Success(true);
	}
}
