using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Application.Common.Results;

namespace DungeonMaster.Application.Features.Dungeons.Commands.RemoveStructureElement;

public sealed class RemoveStructureElementCommandHandler(
	IDungeonWriteRepository writeRepository)
		: ICommandHandler<RemoveStructureElementCommand, bool>
{
	public async Task<Result<bool>> HandleAsync(RemoveStructureElementCommand command, CancellationToken ct)
	{
		var dungeon = await writeRepository.LoadAsync(command.DungeonId, ct);

		if (dungeon is null)
			return Result<bool>.Failure($"Dungeon {command.DungeonId} not found.", ErrorCategory.NotFound);

		dungeon.RemoveStructureElement(command.FloorId, command.AreaId, command.ElementId);

		await writeRepository.UpdateAsync(dungeon, ct);

		return Result<bool>.Success(true);
	}
}
