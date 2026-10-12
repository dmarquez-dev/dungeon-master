using DungeonMaster.Domain.Entities.Floors;
using DungeonMaster.Domain.ValueObjects;

namespace DungeonMaster.App.ViewModels;

/// <summary>
///		Arguments for placing a new object on a floor.
/// </summary>
/// <param name="FloorId">The unique identifier of the floor that will receive the object.</param>
/// <param name="Kind">The kind of object to place.</param>
/// <param name="Position">The object's position, in canvas coordinates.</param>
/// <param name="Rotation">The object's rotation.</param>
/// <param name="Scale">The object's scale.</param>
public sealed record PlaceObjectArgs(Guid FloorId, ObjectKind Kind, CanvasPoint Position, ObjectRotation Rotation, float Scale);
