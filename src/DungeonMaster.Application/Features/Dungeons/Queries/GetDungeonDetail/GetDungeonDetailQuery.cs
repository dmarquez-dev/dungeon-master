using DungeonMaster.Application.Common.Dispatch;

namespace DungeonMaster.Application.Features.Dungeons.Queries.GetDungeonDetail;

/// <summary>
///		Query that retrieves the full detail of a single dungeon by its identifier.
/// </summary>
/// <param name="Id">
///		The unique identifier of the dungeon to retrieve.
/// </param>
public record GetDungeonDetailQuery(
	Guid Id)
		: IQuery<DungeonDetailDto?>;
