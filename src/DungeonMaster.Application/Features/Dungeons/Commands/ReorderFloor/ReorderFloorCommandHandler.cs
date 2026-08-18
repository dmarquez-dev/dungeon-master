using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Application.Common.Handlers;
using DungeonMaster.Application.Common.Interfaces;
using DungeonMaster.Application.Common.Results;

namespace DungeonMaster.Application.Features.Dungeons.Commands.ReorderFloor;

/// <summary>
///		Handles the <see cref="ReorderFloorCommand"/> by updating the ordinal position of the specified floor.
///		Returns a not-found result if no dungeon exists with the given identifier, or if the dungeon has no floor with the given identifier.
///		Returns a validation failure if the requested floor number lies outside the permitted range.
/// </summary>
public sealed class ReorderFloorCommandHandler(
	IUnitOfWorkFactory unitOfWorkFactory)
		: ICommandHandler<ReorderFloorCommand>
{
	/// <inheritdoc/>
	public Task<Result> HandleAsync(
		ReorderFloorCommand command,
		CancellationToken ct) =>
			unitOfWorkFactory.MutateAsync(
				command.DungeonId,
				dungeon =>
					dungeon
						.ReorderFloor(
							command.FloorId,
							command.NewNumber)
						.ToResult(
							$"Floor {command.FloorId} not found.",
							$"Floor number {command.NewNumber} is out of range for this dungeon."),
				ct);
}
