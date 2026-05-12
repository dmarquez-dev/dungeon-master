using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Application.Features.Dungeons;

namespace DungeonMaster.Application.Features.Dungeons.Queries.GetDungeonDetail;

/// <summary>
///		Handles the <see cref="GetDungeonDetailQuery"/> by returning the full detail of a single dungeon.
/// </summary>
public sealed class GetDungeonDetailQueryHandler(
	IDungeonReadRepository readRepository)
		: IQueryHandler<GetDungeonDetailQuery, DungeonDetailDto?>
{
	/// <inheritdoc/>
	public async Task<DungeonDetailDto?> HandleAsync(
		GetDungeonDetailQuery query,
		CancellationToken ct)
		=> await readRepository.GetDetailAsync(
			query.Id,
			ct);
}
