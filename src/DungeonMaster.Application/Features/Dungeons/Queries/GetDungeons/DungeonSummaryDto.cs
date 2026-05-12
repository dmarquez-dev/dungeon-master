namespace DungeonMaster.Application.Features.Dungeons.Queries.GetDungeons;

/// <summary>
///		A lightweight projection of a dungeon used in list views.
/// </summary>
/// <param name="Id">
///		The unique identifier of the dungeon.
/// </param>
/// <param name="Name">
///		The display name of the dungeon.
/// </param>
/// <param name="Description">
///		An optional short description of the dungeon.
/// </param>
/// <param name="FloorCount">
///		The total number of floors in the dungeon.
/// </param>
/// <param name="CreatedAt">
///		The timestamp when the dungeon was created.
/// </param>
/// <param name="LastModified">
///		The timestamp when the dungeon was last modified.
/// </param>
public record DungeonSummaryDto(
	Guid Id,
	string Name,
	string? Description,
	int FloorCount,
	DateTimeOffset CreatedAt,
	DateTimeOffset LastModified);
