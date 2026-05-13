using DungeonMaster.Domain.ValueObjects;

namespace DungeonMaster.Application.Features.Dungeons.Queries.GetDungeonDetail;

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
