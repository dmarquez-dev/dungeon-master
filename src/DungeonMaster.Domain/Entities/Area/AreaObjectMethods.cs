using DungeonMaster.Domain.ValueObjects;

namespace DungeonMaster.Domain.Entities.Area;

public sealed partial class Area
{
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
	/// <returns>
	///		The newly created <see cref="PlacedObject"/> instance.
	/// </returns>
	internal PlacedObject AddObject(
		ObjectKind kind,
		CanvasPoint position,
		ObjectRotation rotation)
	{
		var obj = new PlacedObject(
			Id,
			kind,
			position,
			rotation);

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
	///		Updates the position and rotation of the <see cref="PlacedObject"/> with the specified identifier.
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

		obj.Update(
			newPosition,
			newRotation);

		Touch();

		return true;
	}
}
