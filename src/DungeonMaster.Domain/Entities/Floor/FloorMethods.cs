using DungeonMaster.Domain.ValueObjects;

namespace DungeonMaster.Domain.Entities.Floor;

public sealed partial class Floor
{
	/// <summary>
	///		Updates the sequential number of this floor.
	/// </summary>
	/// <param name="number">
	///		The new floor number to assign.
	/// </param>
	internal void UpdateNumber(int number) =>
		Number = number;

	/// <summary>
	///		Updates the display name and notes of this floor.
	/// </summary>
	/// <param name="name">
	///		The new optional display name for the floor.
	/// </param>
	/// <param name="notes">
	///		The new optional notes for the floor.
	/// </param>
	internal void UpdateDetails(
		string? name,
		string? notes)
	{
		Name = name;
		Notes = notes;
		Touch();
	}

	/// <summary>
	///		Adds a new area to this floor and returns the created area.
	/// </summary>
	/// <param name="name">
	///		The display name of the new area.
	/// </param>
	/// <param name="description">
	///		The optional description of the new area.
	/// </param>
	/// <returns>
	///		The newly created <see cref="Area.Area"/> instance.
	/// </returns>
	internal Area.Area AddArea(
		string name,
		string? description)
	{
		var area = new Area.Area(
			Id,
			name,
			description);

		_areas.Add(area);
		Touch();

		return area;
	}

	/// <summary>
	///		Removes the area with the specified identifier from this floor.
	/// </summary>
	/// <param name="areaId">
	///		The unique identifier of the area to remove.
	/// </param>
	internal void RemoveArea(Guid areaId)
	{
		var area = _areas.FirstOrDefault(area =>
			area.Id == areaId);

		if (area is null)
		{
			return;
		}

		_areas.Remove(area);
		Touch();
	}

	/// <summary>
	///		Updates the name, description, and notes of the specified area on this floor.
	/// </summary>
	/// <param name="areaId">
	///		The unique identifier of the area to update.
	/// </param>
	/// <param name="name">
	///		The new display name for the area.
	/// </param>
	/// <param name="description">
	///		The new optional description for the area.
	/// </param>
	/// <param name="notes">
	///		The new optional notes for the area.
	/// </param>
	internal void UpdateArea(
		Guid areaId,
		string name,
		string? description,
		string? notes)
	{
		var area = _areas.FirstOrDefault(area =>
			area.Id == areaId);

		if (area is null)
		{
			return;
		}

		area.Update(
			name,
			description,
			notes);
		Touch();
	}

	/// <summary>
	///		Updates the tile-grid origin and outline path of the specified area on this floor.
	///		Has no effect if no area with the given identifier exists.
	/// </summary>
	/// <param name="areaId">
	///		The unique identifier of the area to update.
	/// </param>
	/// <param name="origin">
	///		The new tile-grid origin point for the area.
	/// </param>
	/// <param name="outline">
	///		The new ordered list of path segments that form the closed outline polygon.
	/// </param>
	internal void UpdateAreaOutline(
		Guid areaId,
		TilePoint origin,
		IReadOnlyList<PathSegment> outline)
	{
		var area = _areas.FirstOrDefault(area =>
			area.Id == areaId);

		if (area is null)
		{
			return;
		}

		area.SetOutline(
			origin,
			outline);

		Touch();
	}

	/// <summary>
	///		Adds a new placed object to the specified area on this floor.
	/// </summary>
	/// <param name="areaId">
	///		The unique identifier of the area to add the object to.
	/// </param>
	/// <param name="kind">
	///		The kind of object to place.
	/// </param>
	/// <param name="position">
	///		The canvas-space position of the object's bounding-box center,
	///		expressed as a pixel offset from the area's origin vertex.
	/// </param>
	/// <param name="rotation">
	///		The rotation to apply to the object.
	/// </param>
	/// <returns>
	///		The newly created <see cref="Area.PlacedObject"/> instance, or <see langword="null"/> if the area is not found.
	/// </returns>
	internal Area.PlacedObject? AddPlacedObject(
		Guid areaId,
		Area.ObjectKind kind,
		CanvasPoint position,
		Area.ObjectRotation rotation)
	{
		var area = _areas.FirstOrDefault(area =>
			area.Id == areaId);

		if (area is null)
		{
			return null;
		}

		var result = area.AddObject(
			kind,
			position,
			rotation);

		Touch();

		return result;
	}

	/// <summary>
	///		Removes the placed object with the specified identifier from the given area.
	///		Has no effect if the area or object is not found.
	/// </summary>
	/// <param name="areaId">
	///		The unique identifier of the area that owns the object.
	/// </param>
	/// <param name="objectId">
	///		The unique identifier of the placed object to remove.
	/// </param>
	/// <returns>
	///		<see langword="true"/> if the object was found and removed; otherwise <see langword="false"/>.
	/// </returns>
	internal bool RemovePlacedObject(
		Guid areaId,
		Guid objectId)
	{
		var area = _areas.FirstOrDefault(area =>
			area.Id == areaId);

		if (area is null)
		{
			return false;
		}

		var removed = area.RemoveObject(objectId);

		if (removed)
		{
			Touch();
		}

		return removed;
	}

	/// <summary>
	///		Updates the position and rotation of the specified placed object in the given area.
	///		Has no effect if the area or object is not found.
	/// </summary>
	/// <param name="areaId">
	///		The unique identifier of the area that owns the object.
	/// </param>
	/// <param name="objectId">
	///		The unique identifier of the placed object to update.
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
	internal bool UpdatePlacedObject(
		Guid areaId,
		Guid objectId,
		CanvasPoint newPosition,
		Area.ObjectRotation newRotation)
	{
		var area = _areas.FirstOrDefault(area =>
			area.Id == areaId);

		if (area is null)
		{
			return false;
		}

		var updated = area.UpdateObject(
			objectId,
			newPosition,
			newRotation);

		if (updated)
		{
			Touch();
		}

		return updated;
	}
}
