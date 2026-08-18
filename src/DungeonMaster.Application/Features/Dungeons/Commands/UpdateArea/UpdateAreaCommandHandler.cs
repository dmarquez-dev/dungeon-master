using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Application.Common.Handlers;
using DungeonMaster.Application.Common.Interfaces;
using DungeonMaster.Application.Common.Results;

namespace DungeonMaster.Application.Features.Dungeons.Commands.UpdateArea;

/// <summary>
///		Handles the <see cref="UpdateAreaCommand"/> by loading the aggregate and applying the area update.
///		Returns a not-found result if no dungeon exists with the given identifier, or if the aggregate has no matching floor or area.
/// </summary>
public sealed class UpdateAreaCommandHandler(
	IUnitOfWorkFactory unitOfWorkFactory)
		: ICommandHandler<UpdateAreaCommand>
{
	/// <inheritdoc/>
	public Task<Result> HandleAsync(
		UpdateAreaCommand command,
		CancellationToken ct) =>
			unitOfWorkFactory.MutateAsync(
				command.DungeonId,
				dungeon =>
					dungeon
						.UpdateArea(
							command.FloorId,
							command.AreaId,
							command.Name,
							command.Description,
							command.Notes)
						.ToResult($"Area {command.AreaId} not found."),
				ct);
}
