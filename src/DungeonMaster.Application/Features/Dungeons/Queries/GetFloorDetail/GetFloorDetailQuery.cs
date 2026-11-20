using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Application.Features.Dungeons.Queries.GetDungeonDetail;

namespace DungeonMaster.Application.Features.Dungeons.Queries.GetFloorDetail;

/// <summary>
///		Query that retrieves the projection of a single floor of a dungeon, so a mutation that changes one floor can
///		refresh that floor without reloading the whole dungeon detail.
/// </summary>
/// <param name="DungeonId">
///		The unique identifier of the dungeon that owns the floor.
/// </param>
/// <param name="FloorId">
///		The unique identifier of the floor to retrieve.
/// </param>
public record GetFloorDetailQuery(
	Guid DungeonId,
	Guid FloorId)
		: IQuery<FloorDto?>;
