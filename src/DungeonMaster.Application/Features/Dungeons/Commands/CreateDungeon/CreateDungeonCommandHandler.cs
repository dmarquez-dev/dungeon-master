using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Application.Common.Results;
using DungeonMaster.Domain.Entities.Dungeon;

namespace DungeonMaster.Application.Features.Dungeons.Commands.CreateDungeon;

/// <summary>
///		Handles <see cref="CreateDungeonCommand"/> by constructing and persisting a new <see cref="Dungeon"/> entity.
/// </summary>
public sealed class CreateDungeonCommandHandler(
	IDungeonRepository dungeonRepository)
		: ICommandHandler<CreateDungeonCommand, Guid>
{
	/// <summary>
	///		Creates a new dungeon from the command data, persists it, and returns its identifier.
	/// </summary>
	/// <param name="command">
	///		The command carrying the dungeon name and optional description.
	/// </param>
	/// <param name="ct">
	///		A token that may signal cancellation.
	/// </param>
	public async Task<Result<Guid>> HandleAsync(
		CreateDungeonCommand command,
		CancellationToken ct)
	{
		var dungeon = new Dungeon(
			command.Name,
			command.Description);

		await dungeonRepository.AddAsync(
			dungeon,
			ct);

		return Result<Guid>.Success(dungeon.Id);
	}
}
