using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Application.Common.Handlers;
using DungeonMaster.Application.Common.Results;
using DungeonMaster.Application.Features.Dungeons;

namespace DungeonMaster.Application.Features.Dungeons.Commands.UpdateArea;

/// <summary>
///		Handles the <see cref="UpdateAreaCommand"/> by loading the aggregate and applying the area update.
///		Returns a not-found result if no dungeon exists with the given identifier, or if the aggregate has no matching floor or area.
/// </summary>
public sealed class UpdateAreaCommandHandler(
	IDungeonWriteRepository writeRepository)
		: ICommandHandler<UpdateAreaCommand>
{
	/// <inheritdoc/>
	public Task<Result> HandleAsync(
		UpdateAreaCommand command,
		CancellationToken ct) =>
			writeRepository.MutateAsync(
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
