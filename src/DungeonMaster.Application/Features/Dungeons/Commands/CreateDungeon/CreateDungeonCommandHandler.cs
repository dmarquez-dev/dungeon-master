using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Application.Common.Results;
using DungeonMaster.Application.Features.Dungeons;
using DungeonMaster.Domain.Entities.Dungeon;

namespace DungeonMaster.Application.Features.Dungeons.Commands.CreateDungeon;

/// <summary>
///		Handles the <see cref="CreateDungeonCommand"/> by creating and persisting a new dungeon aggregate.
/// </summary>
public sealed class CreateDungeonCommandHandler(
	IDungeonWriteRepository writeRepository)
		: ICommandHandler<CreateDungeonCommand, Guid>
{
	/// <inheritdoc/>
	public async Task<Result<Guid>> HandleAsync(
		CreateDungeonCommand command,
		CancellationToken ct)
	{
		var dungeon = new Dungeon(
			command.Name,
			command.Description);

		await writeRepository.AddAsync(
			dungeon,
			ct);

		return Result<Guid>.Success(dungeon.Id);
	}
}
