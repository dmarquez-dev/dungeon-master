namespace DungeonMaster.Application.Features.Dungeons.Queries.GetDungeonDetail;

/// <summary>
///		Full detail projection of a dungeon, including all floors and areas.
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
/// <param name="Notes">
///		Optional free-form notes about the dungeon.
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
/// <param name="Floors">
///		The ordered list of floors belonging to this dungeon.
/// </param>
public record DungeonDetailDto(
	Guid Id,
	string Name,
	string? Description,
	string? Notes,
	int FloorCount,
	DateTimeOffset CreatedAt,
	DateTimeOffset LastModified,
	IReadOnlyList<FloorDto> Floors);
