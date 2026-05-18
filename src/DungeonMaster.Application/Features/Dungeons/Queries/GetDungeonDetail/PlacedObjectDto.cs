using DungeonMaster.Domain.Entities.Area;
using DungeonMaster.Domain.ValueObjects;

namespace DungeonMaster.Application.Features.Dungeons.Queries.GetDungeonDetail;

/// <summary>
///     Projection of a game object placed within an area.
/// </summary>
/// <param name="Id">The unique identifier of the placed object.</param>
/// <param name="Kind">The kind of object this represents.</param>
/// <param name="Position">
///     The canvas-space position of this object's bounding-box center,
///     expressed as a pixel offset from the containing area's origin vertex.
/// </param>
/// <param name="Rotation">The rotation applied to this object.</param>
public record PlacedObjectDto(
    Guid Id,
    ObjectKind Kind,
    CanvasPoint Position,
    ObjectRotation Rotation);
