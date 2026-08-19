using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Application.Common.Interfaces;
using DungeonMaster.Application.Common.Results;
using DungeonMaster.Domain.Entities.Dungeon;

namespace DungeonMaster.Application.Features.Dungeons.Commands.CreateDungeon;

/// <summary>
///		Handles the <see cref="CreateDungeonCommand"/> by creating and persisting a new dungeon aggregate.
/// </summary>
public sealed class CreateDungeonCommandHandler(
	IUnitOfWorkFactory unitOfWorkFactory)
		: ICommandHandler<CreateDungeonCommand, Guid>
{
	/// <inheritdoc/>
	public async Task<Result<Guid>> HandleAsync(
		CreateDungeonCommand command,
		CancellationToken ct)
	{
		await using var unitOfWork = await unitOfWorkFactory.CreateAsync(ct);

		var dungeon = new Dungeon(
			command.Name,
			command.Description);

		unitOfWork.Dungeons.Add(dungeon);

		await unitOfWork.CommitAsync(ct);

		return Result<Guid>.Success(dungeon.Id);
	}
}
