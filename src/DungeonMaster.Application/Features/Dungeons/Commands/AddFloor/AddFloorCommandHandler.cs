using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Application.Common.Results;
using DungeonMaster.Application.Features.Dungeons;

namespace DungeonMaster.Application.Features.Dungeons.Commands.AddFloor;

/// <summary>
///		Handles the <see cref="AddFloorCommand"/> by adding a new floor to the dungeon aggregate.
///		Returns a not-found result if no dungeon exists with the given identifier.
/// </summary>
public sealed class AddFloorCommandHandler(
	IDungeonWriteRepository writeRepository)
		: ICommandHandler<AddFloorCommand, Guid>
{
	/// <inheritdoc/>
	public async Task<Result<Guid>> HandleAsync(
		AddFloorCommand command,
		CancellationToken ct)
	{
		var dungeon = await writeRepository.LoadAsync(
			command.DungeonId,
			ct);

		if (dungeon is null)
		{
			return Result<Guid>.Failure(
				$"Dungeon {command.DungeonId} not found.",
				ErrorCategory.NotFound);
		}

		var floor = dungeon.AddFloor(command.Name);

		await writeRepository.UpdateAsync(
			dungeon,
			ct);

		return Result<Guid>.Success(floor.Id);
	}
}
