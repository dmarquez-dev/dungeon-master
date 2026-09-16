using DungeonMaster.Domain.ValueObjects;

namespace DungeonMaster.Domain.Entities.Floors;

public sealed partial class Floor
{
	/// <summary>
	///		Adds a new <see cref="PlacedObject"/> of the specified kind to this floor at default scale.
	/// </summary>
	internal PlacedObject AddObject(
		ObjectKind kind,
		CanvasPoint position,
		ObjectRotation rotation)
		=> AddObject(kind, position, rotation, ObjectScale.Default);

	/// <summary>
	///		Adds a new <see cref="PlacedObject"/> of the specified kind to this floor.
	/// </summary>
	/// <param name="kind">
	///		The kind of object to place.
	/// </param>
	/// <param name="position">
	///		The canvas-space position of the object's bounding-box center, in floor-absolute pixels.
	/// </param>
	/// <param name="rotation">
	///		The rotation to apply to the object.
	/// </param>
	/// <param name="scale">
	///		The scale multiplier to apply to the object.
	/// </param>
	/// <returns>
	///		The newly created <see cref="PlacedObject"/> instance.
	/// </returns>
	internal PlacedObject AddObject(
		ObjectKind kind,
		CanvasPoint position,
		ObjectRotation rotation,
		float scale)
	{
		var obj = new PlacedObject(
			Id,
			kind,
			position,
			rotation,
			scale);

		_objects.Add(obj);
		Touch();

		return obj;
	}

	/// <summary>
	///		Removes the <see cref="PlacedObject"/> with the specified identifier from this floor.
	///		If no matching object is found, the call is a no-op.
	/// </summary>
	/// <param name="objectId">
	///		The unique identifier of the object to remove.
	/// </param>
	/// <returns>
	///		<see cref="MutationOutcome.Success"/> when the object was removed, or <see cref="MutationOutcome.NotFound"/> when no object with the given identifier exists.
	/// </returns>
	internal MutationOutcome RemoveObject(Guid objectId)
	{
		var obj = _objects.FirstOrDefault(placedObject =>
			placedObject.Id == objectId);

		if (obj is null)
		{
			return MutationOutcome.NotFound;
		}

		_objects.Remove(obj);
		Touch();

		return MutationOutcome.Success;
	}

	/// <summary>
	///		Updates the position and rotation of the <see cref="PlacedObject"/> with the specified
	///		identifier, preserving its current scale.
	///		If no matching object is found, the call is a no-op.
	/// </summary>
	/// <param name="objectId">
	///		The unique identifier of the object to update.
	/// </param>
	/// <param name="newPosition">
	///		The new canvas-space position (bounding-box center, floor-absolute pixels).
	/// </param>
	/// <param name="newRotation">
	///		The new rotation.
	/// </param>
	/// <returns>
	///		<see cref="MutationOutcome.Success"/> when the object was updated, or <see cref="MutationOutcome.NotFound"/> when no object with the given identifier exists.
	/// </returns>
	internal MutationOutcome UpdateObject(
		Guid objectId,
		CanvasPoint newPosition,
		ObjectRotation newRotation)
	{
		var obj = _objects.FirstOrDefault(placedObject =>
			placedObject.Id == objectId);

		if (obj is null)
		{
			return MutationOutcome.NotFound;
		}

		obj.Update(newPosition, newRotation);
		Touch();

		return MutationOutcome.Success;
	}

	/// <summary>
	///		Updates the position, rotation, and scale of the <see cref="PlacedObject"/> with the specified identifier.
	///		If no matching object is found, the call is a no-op.
	/// </summary>
	/// <param name="objectId">
	///		The unique identifier of the object to update.
	/// </param>
	/// <param name="newPosition">
	///		The new canvas-space position (bounding-box center, floor-absolute pixels).
	/// </param>
	/// <param name="newRotation">
	///		The new rotation.
	/// </param>
	/// <param name="newScale">
	///		The new scale multiplier.
	/// </param>
	/// <returns>
	///		<see cref="MutationOutcome.Success"/> when the object was updated, or <see cref="MutationOutcome.NotFound"/> when no object with the given identifier exists.
	/// </returns>
	internal MutationOutcome UpdateObject(
		Guid objectId,
		CanvasPoint newPosition,
		ObjectRotation newRotation,
		float newScale)
	{
		var obj = _objects.FirstOrDefault(placedObject =>
			placedObject.Id == objectId);

		if (obj is null)
		{
			return MutationOutcome.NotFound;
		}

		obj.Update(
			newPosition,
			newRotation,
			newScale);

		Touch();

		return MutationOutcome.Success;
	}
}
