using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Domain.ValueObjects;

namespace DungeonMaster.Application.Features.Dungeons.Commands.UpdateAreaOutline;

/// <summary>
///		Command that sets the tile-grid origin and outline path segments for an area within a dungeon floor.
/// </summary>
/// <param name="DungeonId">
///		The unique identifier of the dungeon that contains the target floor.
/// </param>
/// <param name="FloorId">
///		The unique identifier of the floor that contains the area.
/// </param>
/// <param name="AreaId">
///		The unique identifier of the area to update.
/// </param>
/// <param name="Origin">
///		The new tile-grid origin point for the area.
/// </param>
/// <param name="Outline">
///		The new ordered list of path segments that form the closed outline polygon.
/// </param>
public record UpdateAreaOutlineCommand(
	Guid DungeonId,
	Guid FloorId,
	Guid AreaId,
	TilePoint Origin,
	IReadOnlyList<PathSegment> Outline)
		: ICommand<bool>;
