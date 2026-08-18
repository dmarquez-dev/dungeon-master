using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Application.Common.Handlers;
using DungeonMaster.Application.Common.Results;
using DungeonMaster.Application.Features.Dungeons;

namespace DungeonMaster.Application.Features.Dungeons.Commands.UpdateAreaOutline;

/// <summary>
///		Handles the <see cref="UpdateAreaOutlineCommand"/> by loading the aggregate, applying the outline update, and persisting the change.
///		Returns a not-found result if no dungeon exists with the given identifier, or if the aggregate has no matching floor or area.
/// </summary>
public sealed class UpdateAreaOutlineCommandHandler(
	IDungeonWriteRepository writeRepository)
		: ICommandHandler<UpdateAreaOutlineCommand>
{
	/// <inheritdoc/>
	public Task<Result> HandleAsync(
		UpdateAreaOutlineCommand command,
		CancellationToken ct) =>
			writeRepository.MutateAsync(
				command.DungeonId,
				dungeon =>
					dungeon
						.UpdateAreaOutline(
							command.FloorId,
							command.AreaId,
							command.Outline)
						.ToResult($"Area {command.AreaId} not found."),
				ct);
}
