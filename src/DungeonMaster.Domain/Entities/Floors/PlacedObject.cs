using DungeonMaster.Domain.ValueObjects;

namespace DungeonMaster.Domain.Entities.Floors;

/// <summary>
///		Represents a game object placed at a specific position within an area.
/// </summary>
public sealed class PlacedObject : Entity
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

	/// <summary>
	///		Updates the position, rotation, and scale of this object.
	/// </summary>
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
		ValidateScale(newScale, nameof(newScale));

		Position = newPosition;
		Rotation = newRotation;
		Scale = newScale;
		Touch();
	}

	/// <summary>
	///		Throws if <paramref name="scale"/> is non-finite or outside the permitted range.
	/// </summary>
	/// <param name="scale">
	///		The candidate scale multiplier.
	/// </param>
	/// <param name="paramName">
	///		The caller's parameter name, reported as the thrown exception's <see cref="ArgumentException.ParamName"/>.
	///		Passed in rather than taken from <c>nameof</c> here, so each call site reports its own parameter.
	/// </param>
	/// <exception cref="ArgumentOutOfRangeException">
	///		<paramref name="scale"/> is NaN, infinite, or outside <see cref="ObjectScale.Min"/>..<see cref="ObjectScale.Max"/>.
	/// </exception>
	private static void ValidateScale(float scale, string paramName)
	{
		if (float.IsNaN(scale) || float.IsInfinity(scale) || scale < ObjectScale.Min || scale > ObjectScale.Max)
		{
			throw new ArgumentOutOfRangeException(
				paramName,
				scale,
				$"Scale must be between {ObjectScale.Min} and {ObjectScale.Max}.");
		}
	}
}
