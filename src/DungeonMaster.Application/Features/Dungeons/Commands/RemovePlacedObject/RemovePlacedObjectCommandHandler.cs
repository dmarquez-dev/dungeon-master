using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Application.Common.Handlers;
using DungeonMaster.Application.Common.Interfaces;
using DungeonMaster.Application.Common.Results;

namespace DungeonMaster.Application.Features.Dungeons.Commands.RemovePlacedObject;

/// <summary>
///		Handles the <see cref="RemovePlacedObjectCommand"/> by loading the dungeon aggregate, removing the placed object, and persisting the change.
///		Returns a not-found result if no dungeon exists with the given identifier, or if the aggregate has no matching floor or placed object.
/// </summary>
public sealed class RemovePlacedObjectCommandHandler(
	IUnitOfWorkFactory unitOfWorkFactory)
		: ICommandHandler<RemovePlacedObjectCommand>
{
	/// <inheritdoc/>
	public Task<Result> HandleAsync(
		RemovePlacedObjectCommand command,
		CancellationToken ct) =>
			unitOfWorkFactory.MutateAsync(
				command.DungeonId,
				dungeon =>
					dungeon
						.RemovePlacedObject(
							command.FloorId,
							command.ObjectId)
						.ToResult($"Placed object {command.ObjectId} not found."),
				ct);
}
