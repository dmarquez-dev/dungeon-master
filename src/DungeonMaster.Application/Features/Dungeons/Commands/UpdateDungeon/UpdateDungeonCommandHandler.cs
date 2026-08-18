using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Application.Common.Handlers;
using DungeonMaster.Application.Common.Interfaces;
using DungeonMaster.Application.Common.Results;

namespace DungeonMaster.Application.Features.Dungeons.Commands.UpdateDungeon;

/// <summary>
///		Handles the <see cref="UpdateDungeonCommand"/> by loading the aggregate and applying the update.
///		Returns a not-found result if no dungeon exists with the given identifier.
/// </summary>
public sealed class UpdateDungeonCommandHandler(
	IUnitOfWorkFactory unitOfWorkFactory)
		: ICommandHandler<UpdateDungeonCommand>
{
	/// <inheritdoc/>
	public Task<Result> HandleAsync(
		UpdateDungeonCommand command,
		CancellationToken ct) =>
			unitOfWorkFactory.MutateAsync(
				command.Id,
				dungeon =>
				{
					dungeon.Update(
						command.Name,
						command.Description,
						command.Notes);

					return Result.Success();
				},
				ct);
}
