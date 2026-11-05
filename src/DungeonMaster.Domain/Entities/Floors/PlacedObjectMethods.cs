using DungeonMaster.Domain.ValueObjects;

namespace DungeonMaster.Domain.Entities.Floors;

public sealed partial class PlacedObject
{
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
