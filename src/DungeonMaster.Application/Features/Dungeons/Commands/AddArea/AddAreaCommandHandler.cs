using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Application.Common.Results;
using DungeonMaster.Application.Features.Dungeons;

namespace DungeonMaster.Application.Features.Dungeons.Commands.AddArea;

/// <summary>
///		Handles the <see cref="AddAreaCommand"/> by adding a new area to the specified floor of the dungeon aggregate.
///		Returns a not-found result if no dungeon exists with the given identifier, or if the dungeon has no floor with the given identifier.
/// </summary>
public sealed class AddAreaCommandHandler(
	IDungeonWriteRepository writeRepository)
		: ICommandHandler<AddAreaCommand, Guid>
{
	/// <inheritdoc/>
	public async Task<Result<Guid>> HandleAsync(
		AddAreaCommand command,
		CancellationToken ct)
	{
		var dungeon = await writeRepository.LoadAsync(
			command.DungeonId,
			ct);

		if (dungeon is null)
		{
			return Result<Guid>.Failure(
				$"Dungeon {command.DungeonId} not found.",
				ErrorCategory.NotFound);
		}

		var area = dungeon.AddArea(
			command.FloorId,
			command.Name,
			command.Description);

		if (area is null)
		{
			return Result<Guid>.Failure(
				$"Floor {command.FloorId} not found.",
				ErrorCategory.NotFound);
		}

		await writeRepository.UpdateAsync(
			dungeon,
			ct);

		return Result<Guid>.Success(area.Id);
	}
}
