using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Application.Common.Results;

namespace DungeonMaster.Application.Features.Dungeons.Commands.AddStructureElement;

public sealed class AddStructureElementCommandHandler(
	IDungeonWriteRepository writeRepository)
		: ICommandHandler<AddStructureElementCommand, Guid>
{
	public async Task<Result<Guid>> HandleAsync(AddStructureElementCommand command, CancellationToken ct)
	{
		var dungeon = await writeRepository.LoadAsync(command.DungeonId, ct);

		if (dungeon is null)
			return Result<Guid>.Failure($"Dungeon {command.DungeonId} not found.", ErrorCategory.NotFound);

		if (command.EdgeAnchor is { } edgeAnchor)
		{
			var element = dungeon.AddEdgeCenterStructureElement(
				command.FloorId, command.AreaId, edgeAnchor, command.Kind);

			if (element is null)
				return Result<Guid>.Failure("Floor or area not found.", ErrorCategory.Validation);

			await writeRepository.UpdateAsync(dungeon, ct);
			return Result<Guid>.Success(element.Id);
		}

		if (command.From is { } from && command.To is { } to)
		{
			var element = dungeon.AddStructureElement(
				command.FloorId, command.AreaId, from, to, command.Kind);

			if (element is null)
				return Result<Guid>.Failure("Floor or area not found.", ErrorCategory.Validation);

			await writeRepository.UpdateAsync(dungeon, ct);
			return Result<Guid>.Success(element.Id);
		}

		return Result<Guid>.Failure(
			"Command must specify either an EdgeAnchor or a From/To vertex pair.",
			ErrorCategory.Validation);
	}
}
