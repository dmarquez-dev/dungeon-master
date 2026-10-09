using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Application.Common.Handlers;
using DungeonMaster.Application.Common.Interfaces;
using DungeonMaster.Application.Common.Results;

namespace DungeonMaster.Application.Features.Dungeons.Commands.AddPlacedObject;

/// <summary>
///		Handles the <see cref="AddPlacedObjectCommand"/> by loading the dungeon aggregate, adding the new placed object to the target floor, and persisting the change.
///		Returns a not-found result if no dungeon exists with the given identifier, or if the dungeon has no floor with the given identifier.
/// </summary>
public sealed class AddPlacedObjectCommandHandler(
	IUnitOfWorkFactory unitOfWorkFactory)
		: ICommandHandler<AddPlacedObjectCommand, Guid>
{
	/// <inheritdoc/>
	public Task<Result<Guid>> HandleAsync(
		AddPlacedObjectCommand command,
		CancellationToken cancellationToken) =>
			unitOfWorkFactory.MutateAsync(
				command.DungeonId,
				dungeon =>
				{
					var placedObject = dungeon.AddPlacedObject(
						command.FloorId,
						command.Kind,
						command.Position,
						command.Rotation,
						command.Scale);

					return placedObject is null
						? Result<Guid>.Failure(
							$"Floor {command.FloorId} not found.",
							ErrorCategory.NotFound)
						: Result<Guid>.Success(placedObject.Id);
				},
				cancellationToken);
}
