namespace DungeonMaster.Application.Features.Dungeons.Queries.GetDungeons;

/// <summary>
///		A lightweight projection of a <c>Dungeon</c> entity returned by the GetDungeons query.
///		Used to populate dungeon list views without exposing entity internals.
/// </summary>
/// <param name="Id">
///		The unique identifier of the dungeon.
/// </param>
/// <param name="Name">
///		The display name of the dungeon.
/// </param>
/// <param name="Description">
///		The optional description of the dungeon.
/// </param>
/// <param name="CreatedAt">
///		The UTC timestamp when the dungeon was created.
/// </param>
/// <param name="LastModified">
///		The UTC timestamp of the most recent modification.
/// </param>
public record DungeonSummaryDto(
	Guid Id,
	string Name,
	string? Description,
	DateTimeOffset CreatedAt,
	DateTimeOffset LastModified);
