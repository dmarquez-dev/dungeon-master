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

/// <summary>
///		Projection of a dungeon floor, including all areas on that floor.
/// </summary>
/// <param name="Id">
///		The unique identifier of the floor.
/// </param>
/// <param name="Number">
///		The ordinal position of the floor within the dungeon.
/// </param>
/// <param name="Name">
///		An optional display name for the floor.
/// </param>
/// <param name="Notes">
///		Optional free-form notes about the floor.
/// </param>
/// <param name="Areas">
///		The list of areas located on this floor.
/// </param>
public record FloorDto(
	Guid Id,
	int Number,
	string? Name,
	string? Notes,
	IReadOnlyList<AreaDto> Areas);

/// <summary>
///		Projection of an area within a dungeon floor.
/// </summary>
/// <param name="Id">
///		The unique identifier of the area.
/// </param>
/// <param name="Name">
///		The display name of the area.
/// </param>
/// <param name="Description">
///		An optional short description of the area.
/// </param>
/// <param name="Notes">
///		Optional free-form notes about the area.
/// </param>
public record AreaDto(
	Guid Id,
	string Name,
	string? Description,
	string? Notes);
