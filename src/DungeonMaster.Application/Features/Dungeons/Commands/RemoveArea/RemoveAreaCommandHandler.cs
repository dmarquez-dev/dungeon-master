using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Application.Common.Handlers;
using DungeonMaster.Application.Common.Interfaces;
using DungeonMaster.Application.Common.Results;

namespace DungeonMaster.Application.Features.Dungeons.Commands.RemoveArea;

/// <summary>
///		Handles the <see cref="RemoveAreaCommand"/> by removing the specified area from the dungeon aggregate.
///		Returns a not-found result if no dungeon exists with the given identifier, or if the aggregate has no matching floor or area.
/// </summary>
public sealed class RemoveAreaCommandHandler(
	IUnitOfWorkFactory unitOfWorkFactory)
		: ICommandHandler<RemoveAreaCommand>
{
	/// <inheritdoc/>
	public Task<Result> HandleAsync(
		RemoveAreaCommand command,
		CancellationToken cancellationToken) =>
			unitOfWorkFactory.MutateAsync(
				command.DungeonId,
				dungeon =>
					dungeon
						.RemoveArea(
							command.FloorId,
							command.AreaId)
						.ToResult($"Area {command.AreaId} not found."),
				cancellationToken);
}
