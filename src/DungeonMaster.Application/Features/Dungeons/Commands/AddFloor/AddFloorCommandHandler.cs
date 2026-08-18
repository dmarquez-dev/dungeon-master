using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Application.Common.Handlers;
using DungeonMaster.Application.Common.Results;
using DungeonMaster.Application.Features.Dungeons;

namespace DungeonMaster.Application.Features.Dungeons.Commands.AddFloor;

/// <summary>
///		Handles the <see cref="AddFloorCommand"/> by adding a new floor to the dungeon aggregate.
///		Returns a not-found result if no dungeon exists with the given identifier.
/// </summary>
public sealed class AddFloorCommandHandler(
	IDungeonWriteRepository writeRepository)
		: ICommandHandler<AddFloorCommand, Guid>
{
	/// <inheritdoc/>
	public Task<Result<Guid>> HandleAsync(
		AddFloorCommand command,
		CancellationToken ct) =>
			writeRepository.MutateAsync(
				command.DungeonId,
				dungeon =>
					Result<Guid>.Success(
						dungeon
							.AddFloor(command.Name)
							.Id),
				ct);
}
