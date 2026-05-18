using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Domain.Entities.Area;
using DungeonMaster.Domain.ValueObjects;

namespace DungeonMaster.Application.Features.Dungeons.Commands.UpdatePlacedObject;

/// <summary>
///		Represents a request to update the position and rotation of a placed object within an area.
/// </summary>
/// <param name="DungeonId">
///		The unique identifier of the dungeon that contains the floor.
/// </param>
/// <param name="FloorId">
///		The unique identifier of the floor that contains the area.
/// </param>
/// <param name="AreaId">
///		The unique identifier of the area that owns the object.
/// </param>
/// <param name="ObjectId">
///		The unique identifier of the placed object to update.
/// </param>
/// <param name="NewPosition">
///		The new canvas-space position (bounding-box center, area-relative pixels).
/// </param>
/// <param name="NewRotation">
///		The new rotation.
/// </param>
public record UpdatePlacedObjectCommand(
	Guid DungeonId,
	Guid FloorId,
	Guid AreaId,
	Guid ObjectId,
	CanvasPoint NewPosition,
	ObjectRotation NewRotation)
		: ICommand<bool>;
