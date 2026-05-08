using DungeonMaster.Application.Common.Dispatch;

namespace DungeonMaster.Application.Features.Dungeons.Queries.GetDungeons;

/// <summary>
///		Handles <see cref="GetDungeonsQuery"/> by fetching all dungeons and projecting them to <see cref="DungeonSummaryDto"/> instances.
/// </summary>
public sealed class GetDungeonsQueryHandler(
	IDungeonRepository dungeonRepository)
		: IQueryHandler<GetDungeonsQuery, IReadOnlyList<DungeonSummaryDto>>
{
	/// <summary>
	///		Fetches all dungeons from the repository and maps each to a <see cref="DungeonSummaryDto"/>.
	/// </summary>
	/// <param name="query">
	///		The query to handle.
	/// </param>
	/// <param name="ct">
	///		A token that may signal cancellation.
	/// </param>
	public async Task<IReadOnlyList<DungeonSummaryDto>> HandleAsync(
		GetDungeonsQuery query,
		CancellationToken ct)
	{
		var dungeons = await dungeonRepository.GetAllAsync(ct);

		return dungeons
			.Select(dungeon =>
				new DungeonSummaryDto(
					dungeon.Id,
					dungeon.Name,
					dungeon.Description,
					dungeon.CreatedAt,
					dungeon.LastModified))
			.ToList();
	}
}
