using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Application.Common.Handlers;
using DungeonMaster.Application.Common.Results;
using DungeonMaster.Application.Features.Dungeons;

namespace DungeonMaster.Application.Features.Dungeons.Commands.UpdateDungeon;

/// <summary>
///		Handles the <see cref="UpdateDungeonCommand"/> by loading the aggregate and applying the update.
///		Returns a not-found result if no dungeon exists with the given identifier.
/// </summary>
public sealed class UpdateDungeonCommandHandler(
	IDungeonWriteRepository writeRepository)
		: ICommandHandler<UpdateDungeonCommand>
{
	/// <inheritdoc/>
	public Task<Result> HandleAsync(
		UpdateDungeonCommand command,
		CancellationToken ct) =>
			writeRepository.MutateAsync(
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
