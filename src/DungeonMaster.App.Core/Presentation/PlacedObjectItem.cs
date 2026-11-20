using DungeonMaster.Domain.Entities.Floors;
using DungeonMaster.Domain.ValueObjects;

namespace DungeonMaster.App.Core.Presentation;

/// <summary>
///		Represents the control kit's own shape for an object placed on a dungeon floor.
/// </summary>
/// <param name="Id">
///		The unique identifier of the placed object.
/// </param>
/// <param name="Kind">
///		The kind of object this represents.
/// </param>
/// <param name="Position">
///		The position of this object's bounding-box centre, in floor-absolute canvas pixels.
///		This is the centre of the bounding box and not its top-left corner.
/// </param>
/// <param name="Rotation">
///		The rotation applied to this object.
/// </param>
/// <param name="Scale">
///		The scale factor applied to this object.
/// </param>
public sealed record PlacedObjectItem(
	Guid Id,
	ObjectKind Kind,
	CanvasPoint Position,
	ObjectRotation Rotation,
	float Scale);
