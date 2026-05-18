using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Domain.Entities.Area;
using DungeonMaster.Domain.ValueObjects;

namespace DungeonMaster.Application.Features.Dungeons.Commands.AddPlacedObject;

/// <summary>
///		Represents a request to add a new placed object to the specified area within a dungeon floor.
/// </summary>
/// <param name="DungeonId">
///		The unique identifier of the dungeon that contains the floor.
/// </param>
/// <param name="FloorId">
///		The unique identifier of the floor that contains the area.
/// </param>
/// <param name="AreaId">
///		The unique identifier of the area to add the object to.
/// </param>
/// <param name="Kind">
///		The kind of object to place.
/// </param>
/// <param name="Position">
///		The canvas-space position of the object's bounding-box center,
///		expressed as a pixel offset from the area's origin vertex.
/// </param>
/// <param name="Rotation">
///		The rotation to apply to the object.
/// </param>
public record AddPlacedObjectCommand(
	Guid DungeonId,
	Guid FloorId,
	Guid AreaId,
	ObjectKind Kind,
	CanvasPoint Position,
	ObjectRotation Rotation)
		: ICommand<Guid>;
