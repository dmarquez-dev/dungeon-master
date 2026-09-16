using DungeonMaster.Domain.Entities.Floors;
using DungeonMaster.Domain.ValueObjects;

namespace DungeonMaster.Application.Features.Dungeons.Queries.GetDungeonDetail;

/// <summary>
///     Projection of a game object placed on a floor.
/// </summary>
/// <param name="Id">The unique identifier of the placed object.</param>
/// <param name="Kind">The kind of object this represents.</param>
/// <param name="Position">
///     The canvas-space position of this object's bounding-box center, in floor-absolute pixels.
/// </param>
/// <param name="Rotation">The rotation applied to this object.</param>
/// <param name="Scale">The scale factor applied to this object.</param>
public record PlacedObjectDto(
	Guid Id,
	ObjectKind Kind,
	CanvasPoint Position,
	ObjectRotation Rotation,
	float Scale);
