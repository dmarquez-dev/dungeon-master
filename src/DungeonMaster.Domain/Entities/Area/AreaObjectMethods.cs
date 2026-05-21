using DungeonMaster.Domain.ValueObjects;

namespace DungeonMaster.Domain.Entities.Area;

public sealed partial class Area
{
	/// <summary>
	///		Adds a new <see cref="PlacedObject"/> of the specified kind to this area at default scale.
	/// </summary>
	internal PlacedObject AddObject(
		ObjectKind kind,
		CanvasPoint position,
		ObjectRotation rotation)
		=> AddObject(kind, position, rotation, ObjectScale.Default);

	/// <summary>
	///		Adds a new <see cref="PlacedObject"/> of the specified kind to this area.
	/// </summary>
	/// <param name="kind">
	///		The kind of object to place.
	/// </param>
	/// <param name="position">
	///		The canvas-space position of the object's bounding-box center,
	///		expressed as a pixel offset from this area's origin vertex.
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
	///		Removes the <see cref="PlacedObject"/> with the specified identifier from this area.
	///		If no matching object is found, the call is a no-op.
	/// </summary>
	/// <param name="objectId">
	///		The unique identifier of the object to remove.
	/// </param>
	/// <returns>
	///		<see langword="true"/> if the object was found and removed; otherwise <see langword="false"/>.
	/// </returns>
	internal bool RemoveObject(Guid objectId)
	{
		var obj = _objects.FirstOrDefault(placedObject =>
			placedObject.Id == objectId);

		if (obj is null)
		{
			return false;
		}

		_objects.Remove(obj);
		Touch();

		return true;
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
	///		The new canvas-space position (bounding-box center, area-relative pixels).
	/// </param>
	/// <param name="newRotation">
	///		The new rotation.
	/// </param>
	/// <returns>
	///		<see langword="true"/> if the object was found and updated; otherwise <see langword="false"/>.
	/// </returns>
	internal bool UpdateObject(
		Guid objectId,
		CanvasPoint newPosition,
		ObjectRotation newRotation)
	{
		var obj = _objects.FirstOrDefault(placedObject =>
			placedObject.Id == objectId);

		if (obj is null)
		{
			return false;
		}

		obj.Update(newPosition, newRotation);
		Touch();

		return true;
	}

	/// <summary>
	///		Updates the position, rotation, and scale of the <see cref="PlacedObject"/> with the specified identifier.
	///		If no matching object is found, the call is a no-op.
	/// </summary>
	/// <param name="objectId">
	///		The unique identifier of the object to update.
	/// </param>
	/// <param name="newPosition">
	///		The new canvas-space position (bounding-box center, area-relative pixels).
	/// </param>
	/// <param name="newRotation">
	///		The new rotation.
	/// </param>
	/// <param name="newScale">
	///		The new scale multiplier.
	/// </param>
	/// <returns>
	///		<see langword="true"/> if the object was found and updated; otherwise <see langword="false"/>.
	/// </returns>
	internal bool UpdateObject(
		Guid objectId,
		CanvasPoint newPosition,
		ObjectRotation newRotation,
		float newScale)
	{
		var obj = _objects.FirstOrDefault(placedObject =>
			placedObject.Id == objectId);

		if (obj is null)
		{
			return false;
		}

		obj.Update(
			newPosition,
			newRotation,
			newScale);

		Touch();

		return true;
	}
}
