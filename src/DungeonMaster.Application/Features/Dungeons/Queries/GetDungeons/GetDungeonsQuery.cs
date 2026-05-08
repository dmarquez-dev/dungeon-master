using DungeonMaster.Application.Common.Dispatch;

namespace DungeonMaster.Application.Features.Dungeons.Queries.GetDungeons;

/// <summary>
///		Query that retrieves all dungeons.
///		Returns a read-only list of <see cref="DungeonSummaryDto"/> projections with no filtering applied.
/// </summary>
public record GetDungeonsQuery
	: IQuery<IReadOnlyList<DungeonSummaryDto>>;
