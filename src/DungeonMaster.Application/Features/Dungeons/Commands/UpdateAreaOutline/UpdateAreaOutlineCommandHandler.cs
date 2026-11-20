using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Application.Common.Handlers;
using DungeonMaster.Application.Common.Interfaces;
using DungeonMaster.Application.Common.Results;

namespace DungeonMaster.Application.Features.Dungeons.Commands.UpdateAreaOutline;

/// <summary>
///		Handles the <see cref="UpdateAreaOutlineCommand"/> by loading the aggregate, applying the outline update, and persisting the change.
///		Returns a not-found result if no dungeon exists with the given identifier, or if the aggregate has no matching floor or area.
/// </summary>
public sealed class UpdateAreaOutlineCommandHandler(
	IUnitOfWorkFactory unitOfWorkFactory)
		: ICommandHandler<UpdateAreaOutlineCommand>
{
	/// <inheritdoc/>
	public Task<Result> HandleAsync(
		UpdateAreaOutlineCommand command,
		CancellationToken cancellationToken) =>
			unitOfWorkFactory.MutateAsync(
				command.DungeonId,
				dungeon =>
					dungeon
						.UpdateAreaOutline(
							command.FloorId,
							command.AreaId,
							command.Outline)
						.ToResult($"Area {command.AreaId} not found."),
				cancellationToken);
}
