using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Domain.Entities.Area;
using DungeonMaster.Domain.ValueObjects;

namespace DungeonMaster.Application.Features.Dungeons.Commands.AddPlacedObject;

/// <summary>
///		Represents a request to add a new placed object to the specified dungeon floor.
/// </summary>
/// <param name="DungeonId">
///		The unique identifier of the dungeon that contains the floor.
/// </param>
/// <param name="FloorId">
///		The unique identifier of the floor to add the object to.
/// </param>
/// <param name="Kind">
///		The kind of object to place.
/// </param>
/// <param name="Position">
///		The canvas-space position of the object's bounding-box center, in floor-absolute pixels.
/// </param>
/// <param name="Rotation">
///		The rotation to apply to the object.
/// </param>
/// <param name="Scale">
///		The scale factor to apply to the object.
/// </param>
public record AddPlacedObjectCommand(
	Guid DungeonId,
	Guid FloorId,
	ObjectKind Kind,
	CanvasPoint Position,
	ObjectRotation Rotation,
	float Scale = ObjectScale.Default)
		: ICommand<Guid>;
