using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Application.Common.Handlers;
using DungeonMaster.Application.Common.Results;
using DungeonMaster.Application.Features.Dungeons;

namespace DungeonMaster.Application.Features.Dungeons.Commands.RemoveFloor;

/// <summary>
///		Handles the <see cref="RemoveFloorCommand"/> by removing the specified floor from the dungeon aggregate.
///		Returns a not-found result if no dungeon exists with the given identifier, or if the dungeon has no floor with the given identifier.
/// </summary>
public sealed class RemoveFloorCommandHandler(
	IDungeonWriteRepository writeRepository)
		: ICommandHandler<RemoveFloorCommand>
{
	/// <inheritdoc/>
	public Task<Result> HandleAsync(
		RemoveFloorCommand command,
		CancellationToken ct) =>
			writeRepository.MutateAsync(
				command.DungeonId,
				dungeon =>
					dungeon
						.RemoveFloor(command.FloorId)
						.ToResult($"Floor {command.FloorId} not found."),
				ct);
}
