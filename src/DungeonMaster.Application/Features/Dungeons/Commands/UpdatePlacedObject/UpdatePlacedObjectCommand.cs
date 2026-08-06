using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Domain.Entities.Floor;
using DungeonMaster.Domain.ValueObjects;

namespace DungeonMaster.Application.Features.Dungeons.Commands.UpdatePlacedObject;

/// <summary>
///		Represents a request to update the position, rotation, and scale of a placed object on a floor.
/// </summary>
/// <param name="DungeonId">
///		The unique identifier of the dungeon that contains the floor.
/// </param>
/// <param name="FloorId">
///		The unique identifier of the floor that owns the object.
/// </param>
/// <param name="ObjectId">
///		The unique identifier of the placed object to update.
/// </param>
/// <param name="NewPosition">
///		The new canvas-space position (bounding-box center, floor-absolute pixels).
/// </param>
/// <param name="NewRotation">
///		The new rotation.
/// </param>
/// <param name="NewScale">
///		The new scale factor.
/// </param>
public record UpdatePlacedObjectCommand(
	Guid DungeonId,
	Guid FloorId,
	Guid ObjectId,
	CanvasPoint NewPosition,
	ObjectRotation NewRotation,
	float NewScale = ObjectScale.Default)
		: ICommand<bool>;
