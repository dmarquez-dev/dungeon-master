using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Application.Common.Handlers;
using DungeonMaster.Application.Common.Interfaces;
using DungeonMaster.Application.Common.Results;

namespace DungeonMaster.Application.Features.Dungeons.Commands.UpdatePlacedObject;

/// <summary>
///		Handles the <see cref="UpdatePlacedObjectCommand"/> by loading the dungeon aggregate, applying the position, rotation, and scale update, and persisting the change.
///		Returns a not-found result if no dungeon exists with the given identifier, or if the aggregate has no matching floor or placed object.
/// </summary>
public sealed class UpdatePlacedObjectCommandHandler(
	IUnitOfWorkFactory unitOfWorkFactory)
		: ICommandHandler<UpdatePlacedObjectCommand>
{
	/// <inheritdoc/>
	public Task<Result> HandleAsync(
		UpdatePlacedObjectCommand command,
		CancellationToken ct) =>
			unitOfWorkFactory.MutateAsync(
				command.DungeonId,
				dungeon =>
					dungeon
						.UpdatePlacedObject(
							command.FloorId,
							command.ObjectId,
							command.NewPosition,
							command.NewRotation,
							command.NewScale)
						.ToResult($"Placed object {command.ObjectId} not found."),
				ct);
}
