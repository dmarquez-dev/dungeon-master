using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Application.Common.Handlers;
using DungeonMaster.Application.Common.Results;
using DungeonMaster.Application.Features.Dungeons;

namespace DungeonMaster.Application.Features.Dungeons.Commands.UpdateFloor;

/// <summary>
///		Handles the <see cref="UpdateFloorCommand"/> by loading the aggregate and applying the floor update.
///		Returns a not-found result if no dungeon exists with the given identifier, or if the dungeon has no floor with the given identifier.
/// </summary>
public sealed class UpdateFloorCommandHandler(
	IDungeonWriteRepository writeRepository)
		: ICommandHandler<UpdateFloorCommand>
{
	/// <inheritdoc/>
	public Task<Result> HandleAsync(
		UpdateFloorCommand command,
		CancellationToken ct) =>
			writeRepository.MutateAsync(
				command.DungeonId,
				dungeon =>
					dungeon
						.UpdateFloor(
							command.FloorId,
							command.Name,
							command.Description,
							command.Notes)
						.ToResult($"Floor {command.FloorId} not found."),
				ct);
}
