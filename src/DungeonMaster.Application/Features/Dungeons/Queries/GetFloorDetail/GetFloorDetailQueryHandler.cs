using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Application.Common.Interfaces;
using DungeonMaster.Application.Features.Dungeons.Queries.GetDungeonDetail;

namespace DungeonMaster.Application.Features.Dungeons.Queries.GetFloorDetail;

/// <summary>
///		Handles the <see cref="GetFloorDetailQuery"/> by returning the projection of a single floor of a dungeon.
/// </summary>
public sealed class GetFloorDetailQueryHandler(
	IDungeonReadRepository readRepository)
		: IQueryHandler<GetFloorDetailQuery, FloorDto?>
{
	/// <inheritdoc/>
	public async Task<FloorDto?> HandleAsync(
		GetFloorDetailQuery query,
		CancellationToken cancellationToken)
		=> await readRepository.GetFloorDetailAsync(
			query.DungeonId,
			query.FloorId,
			cancellationToken);
}
