using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Application.Common.Handlers;
using DungeonMaster.Application.Common.Results;
using DungeonMaster.Application.Features.Dungeons;

namespace DungeonMaster.Application.Features.Dungeons.Commands.RemoveArea;

/// <summary>
///		Handles the <see cref="RemoveAreaCommand"/> by removing the specified area from the dungeon aggregate.
///		Returns a not-found result if no dungeon exists with the given identifier, or if the aggregate has no matching floor or area.
/// </summary>
public sealed class RemoveAreaCommandHandler(
	IDungeonWriteRepository writeRepository)
		: ICommandHandler<RemoveAreaCommand>
{
	/// <inheritdoc/>
	public Task<Result> HandleAsync(
		RemoveAreaCommand command,
		CancellationToken ct) =>
			writeRepository.MutateAsync(
				command.DungeonId,
				dungeon =>
					dungeon
						.RemoveArea(
							command.FloorId,
							command.AreaId)
						.ToResult($"Area {command.AreaId} not found."),
				ct);
}
