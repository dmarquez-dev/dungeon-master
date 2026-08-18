using DungeonMaster.Domain.ValueObjects;

namespace DungeonMaster.Domain.Entities.Floor;

/// <summary>
///		Represents a game object placed at a specific position within an area.
/// </summary>
public sealed class PlacedObject : Entity
{
	private PlacedObject() { }

	/// <summary>Initializes a new <see cref="PlacedObject"/> with default scale.</summary>
	internal PlacedObject(
		Guid floorId,
		ObjectKind kind,
		CanvasPoint position,
		ObjectRotation rotation)
		: this(floorId, kind, position, rotation, ObjectScale.Default) { }

	/// <summary>Initializes a new <see cref="PlacedObject"/> with an explicit scale.</summary>
	internal PlacedObject(
		Guid floorId,
		ObjectKind kind,
		CanvasPoint position,
		ObjectRotation rotation,
		float scale)
	{
		if (float.IsNaN(scale) || float.IsInfinity(scale) || scale < ObjectScale.Min || scale > ObjectScale.Max)
		{
			throw new ArgumentOutOfRangeException(
				nameof(scale),
				scale,
				$"Scale must be between {ObjectScale.Min} and {ObjectScale.Max}.");
		}

		FloorId = floorId;
		Kind = kind;
		Position = position;
		Rotation = rotation;
		Scale = scale;
	}

	/// <summary>Gets the unique identifier of the floor this object belongs to.</summary>
	public Guid FloorId { get; private set; }

	/// <summary>Gets the kind of game object this represents.</summary>
	public ObjectKind Kind { get; private set; }

	/// <summary>
	///		Gets the canvas-space position of this object's bounding-box center,
	///		expressed in floor-absolute pixels.
	/// </summary>
	public CanvasPoint Position { get; private set; } = default!;

	/// <summary>Gets the rotation applied to this object.</summary>
	public ObjectRotation Rotation { get; private set; }

	/// <summary>Gets the scale multiplier applied to this object. Default is 1.0.</summary>
	public float Scale { get; private set; } = ObjectScale.Default;

	/// <summary>Updates the position and rotation, keeping the current scale.</summary>
	internal void Update(CanvasPoint newPosition, ObjectRotation newRotation)
		=> Update(newPosition, newRotation, Scale);

	/// <summary>Updates the position, rotation, and scale of this object.</summary>
	/// <param name="newPosition">
	///		The new canvas-space position (bounding-box center, floor-absolute pixels).
	/// </param>
	/// <param name="newRotation">
	///		The new rotation.
	/// </param>
	/// <param name="newScale">
	///		The new scale multiplier.
	/// </param>
	internal void Update(
		CanvasPoint newPosition,
		ObjectRotation newRotation,
		float newScale)
	{
		if (float.IsNaN(newScale) || float.IsInfinity(newScale) || newScale < ObjectScale.Min || newScale > ObjectScale.Max)
		{
			throw new ArgumentOutOfRangeException(
				nameof(newScale),
				newScale,
				$"Scale must be between {ObjectScale.Min} and {ObjectScale.Max}.");
		}

		Position = newPosition;
		Rotation = newRotation;
		Scale = newScale;
		Touch();
	}
}
