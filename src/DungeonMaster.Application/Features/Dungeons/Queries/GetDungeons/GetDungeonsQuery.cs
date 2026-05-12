using DungeonMaster.Application.Common.Dispatch;

namespace DungeonMaster.Application.Features.Dungeons.Queries.GetDungeons;

/// <summary>
///		Query that retrieves a summary list of all dungeons.
/// </summary>
public record GetDungeonsQuery
	: IQuery<IReadOnlyList<DungeonSummaryDto>>;
