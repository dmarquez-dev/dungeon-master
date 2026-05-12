using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Application.Features.Dungeons;

namespace DungeonMaster.Application.Features.Dungeons.Queries.GetDungeons;

/// <summary>
///		Handles the <see cref="GetDungeonsQuery"/> by returning a summary list of all dungeons.
/// </summary>
public sealed class GetDungeonsQueryHandler(
	IDungeonReadRepository readRepository)
		: IQueryHandler<GetDungeonsQuery, IReadOnlyList<DungeonSummaryDto>>
{
	/// <inheritdoc/>
	public async Task<IReadOnlyList<DungeonSummaryDto>> HandleAsync(
		GetDungeonsQuery query,
		CancellationToken ct)
		=> await readRepository.GetSummariesAsync(ct);
}
