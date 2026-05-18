using DungeonMaster.Domain.ValueObjects;

namespace DungeonMaster.Domain.Entities.Area;

/// <summary>
///		Represents a game object placed at a specific position within an area.
/// </summary>
public sealed class PlacedObject : Entity
{
	private PlacedObject() { }

	internal PlacedObject(
		Guid areaId,
		ObjectKind kind,
		CanvasPoint position,
		ObjectRotation rotation)
	{
		AreaId = areaId;
		Kind = kind;
		Position = position;
		Rotation = rotation;
	}

	/// <summary>
	///		Gets the unique identifier of the area this object belongs to.
	/// </summary>
	public Guid AreaId { get; private set; }

	/// <summary>
	///		Gets the kind of game object this represents.
	/// </summary>
	public ObjectKind Kind { get; private set; }

	/// <summary>
	///		Gets the canvas-space position of this object's bounding-box center,
	///		expressed as a pixel offset from the containing area's origin vertex.
	/// </summary>
	public CanvasPoint Position { get; private set; } = default!;

	/// <summary>
	///		Gets the rotation applied to this object.
	/// </summary>
	public ObjectRotation Rotation { get; private set; }

	/// <summary>
	///		Updates the position and rotation of this object.
	/// </summary>
	/// <param name="newPosition">
	///		The new canvas-space position (bounding-box center, area-relative pixels).
	/// </param>
	/// <param name="newRotation">
	///		The new rotation.
	/// </param>
	internal void Update(
		CanvasPoint newPosition,
		ObjectRotation newRotation)
	{
		Position = newPosition;
		Rotation = newRotation;
		Touch();
	}
}
