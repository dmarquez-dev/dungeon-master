using DungeonMaster.Domain.ValueObjects;

namespace DungeonMaster.Application.Features.Dungeons.Queries.GetDungeonDetail;

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
/// <param name="Origin">
///		The tile-grid origin point of the area.
/// </param>
/// <param name="Outline">
///		The ordered list of path segments that form the closed outline polygon of the area.
/// </param>
/// <param name="Objects">
///		The list of objects placed within this area.
/// </param>
/// <param name="TileTextures">
///		The list of tile textures painted within this area.
/// </param>
/// <param name="StructureElements">
///		The list of structural elements placed within this area.
/// </param>
public record AreaDto(
	Guid Id,
	string Name,
	string? Description,
	string? Notes,
	TilePoint Origin,
	IReadOnlyList<PathSegment> Outline,
	IReadOnlyList<PlacedObjectDto> Objects,
	IReadOnlyList<TileTextureDto> TileTextures,
	IReadOnlyList<StructureElementDto> StructureElements);
