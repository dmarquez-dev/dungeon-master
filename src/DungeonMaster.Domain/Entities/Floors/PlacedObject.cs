using DungeonMaster.Domain.ValueObjects;

namespace DungeonMaster.Domain.Entities.Floors;

/// <summary>
///		Represents a game object placed at a specific position within an area.
/// </summary>
public sealed partial class PlacedObject : Entity
{
	private PlacedObject() { }

	/// <summary>
	///		Initializes a new <see cref="PlacedObject"/> with an explicit scale.
	/// </summary>
	internal PlacedObject(
		Guid floorId,
		ObjectKind kind,
		CanvasPoint position,
		ObjectRotation rotation,
		float scale)
	{
		if (kind == ObjectKind.None || !Enum.IsDefined(kind))
		{
			throw new ArgumentOutOfRangeException(
				nameof(kind),
				kind,
				$"Kind must be a defined {nameof(ObjectKind)} other than {nameof(ObjectKind.None)}.");
		}

		ValidateScale(scale, nameof(scale));

		FloorId = floorId;
		Kind = kind;
		Position = position;
		Rotation = rotation;
		Scale = scale;
	}

	/// <summary>
	///		Gets the unique identifier of the floor this object belongs to.
	/// </summary>
	public Guid FloorId { get; private set; }

	/// <summary>
	///		Gets the kind of game object this represents.
	/// </summary>
	public ObjectKind Kind { get; private set; }

	/// <summary>
	///		Gets the canvas-space position of this object's bounding-box center, expressed in floor-absolute pixels.
	/// </summary>
	public CanvasPoint Position { get; private set; } = default!;

	/// <summary>
	///		Gets the rotation applied to this object.
	/// </summary>
	public ObjectRotation Rotation { get; private set; }

	/// <summary>
	///		Gets the scale multiplier applied to this object. Default is 1.0.
	/// </summary>
	public float Scale { get; private set; } = ObjectScale.Default;
}
