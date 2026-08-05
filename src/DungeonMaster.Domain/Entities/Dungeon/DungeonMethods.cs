using DungeonMaster.Domain.ValueObjects;

namespace DungeonMaster.Domain.Entities.Dungeon;

public sealed partial class Dungeon
{
	/// <summary>
	///		Updates the name, description, and notes of the dungeon.
	/// </summary>
	/// <param name="name">
	///		The new display name. Must not be null or whitespace.
	/// </param>
	/// <param name="description">
	///		The new optional description.
	/// </param>
	/// <param name="notes">
	///		The new optional notes.
	/// </param>
	public void Update(
		string name,
		string? description,
		string? notes)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);

		Name = name;
		Description = description;
		Notes = notes;

		Touch();
	}

	/// <summary>
	///		Adds a new floor to the dungeon and assigns it the next sequential number.
	/// </summary>
	/// <param name="name">
	///		The optional display name for the new floor.
	/// </param>
	/// <returns>
	///		The newly created <see cref="Floor.Floor"/> instance.
	/// </returns>
	public Floor.Floor AddFloor(string? name)
	{
		var floor = new Floor.Floor(
			Id,
			_floors.Count + 1,
			name);

		_floors.Add(floor);
		Touch();

		return floor;
	}

	/// <summary>
	///		Removes the floor with the specified identifier and renumbers the remaining floors sequentially.
	/// </summary>
	/// <param name="floorId">
	///		The unique identifier of the floor to remove.
	/// </param>
	public void RemoveFloor(Guid floorId)
	{
		var floor = _floors.FirstOrDefault(floor =>
			floor.Id == floorId);

		if (floor is null)
		{
			return;
		}

		_floors.Remove(floor);

		var sorted = _floors
			.OrderBy(floor =>
				floor.Number)
			.ToList();

		for (var i = 0; i < sorted.Count; i++)
		{
			sorted[i].UpdateNumber(i + 1);
		}

		Touch();
	}

	/// <summary>
	///		Moves the specified floor to a new position and shifts affected floors to maintain sequential numbering.
	/// </summary>
	/// <param name="floorId">
	///		The unique identifier of the floor to reorder.
	/// </param>
	/// <param name="newNumber">
	///		The target floor number. Must be between 1 and the total floor count, inclusive.
	/// </param>
	public void ReorderFloor(
		Guid floorId,
		int newNumber)
	{
		var floor = _floors.FirstOrDefault(floor =>
			floor.Id == floorId);

		if (floor is null)
		{
			return;
		}

		var oldNumber = floor.Number;

		if (oldNumber == newNumber)
		{
			return;
		}

		if (newNumber < 1 || newNumber > _floors.Count)
		{
			return;
		}

		if (newNumber < oldNumber)
		{
			foreach (var shifted in _floors.Where(floor =>
				floor.Number >= newNumber && floor.Number < oldNumber))
			{
				shifted.UpdateNumber(shifted.Number + 1);
			}
		}
		else
		{
			foreach (var shifted in _floors.Where(floor =>
				floor.Number > oldNumber && floor.Number <= newNumber))
			{
				shifted.UpdateNumber(shifted.Number - 1);
			}
		}

		floor.UpdateNumber(newNumber);
		Touch();
	}

	/// <summary>
	///		Updates the name and notes of the specified floor.
	/// </summary>
	/// <param name="floorId">
	///		The unique identifier of the floor to update.
	/// </param>
	/// <param name="name">
	///		The new optional display name for the floor.
	/// </param>
	/// <param name="notes">
	///		The new optional notes for the floor.
	/// </param>
	public void UpdateFloor(
		Guid floorId,
		string? name,
		string? notes)
	{
		var floor = _floors.FirstOrDefault(floor =>
			floor.Id == floorId);

		if (floor is null)
		{
			return;
		}

		floor.UpdateDetails(name, notes);
		Touch();
	}

	/// <summary>
	///		Adds a new area to the specified floor and returns the created area.
	/// </summary>
	/// <param name="floorId">
	///		The unique identifier of the floor to add the area to.
	/// </param>
	/// <param name="name">
	///		The display name of the new area.
	/// </param>
	/// <param name="description">
	///		The optional description of the new area.
	/// </param>
	/// <returns>
	///		The newly created <see cref="Area.Area"/> instance.
	/// </returns>
	/// <exception cref="InvalidOperationException">
	///		Thrown when no floor with the given identifier exists in this dungeon.
	/// </exception>
	public Area.Area AddArea(
		Guid floorId,
		string name,
		string? description)
	{
		var floor = _floors.FirstOrDefault(floor =>
			floor.Id == floorId)
			?? throw new InvalidOperationException($"Floor {floorId} not found.");

		var area = floor.AddArea(name, description);
		Touch();

		return area;
	}

	/// <summary>
	///		Removes the specified area from the given floor.
	/// </summary>
	/// <param name="floorId">
	///		The unique identifier of the floor that owns the area.
	/// </param>
	/// <param name="areaId">
	///		The unique identifier of the area to remove.
	/// </param>
	public void RemoveArea(
		Guid floorId,
		Guid areaId)
	{
		var floor = _floors.FirstOrDefault(floor =>
			floor.Id == floorId);

		if (floor is null)
		{
			return;
		}

		floor.RemoveArea(areaId);
		Touch();
	}

	/// <summary>
	///		Updates the name, description, and notes of the specified area on the given floor.
	/// </summary>
	/// <param name="floorId">
	///		The unique identifier of the floor that owns the area.
	/// </param>
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
	public void UpdateArea(
		Guid floorId,
		Guid areaId,
		string name,
		string? description,
		string? notes)
	{
		var floor = _floors.FirstOrDefault(floor =>
			floor.Id == floorId);

		if (floor is null)
		{
			return;
		}

		floor.UpdateArea(
			areaId,
			name,
			description,
			notes);

		Touch();
	}

	/// <summary>
	///		Replaces the outline path of the specified area on the given floor.
	///		Has no effect if the floor or area identifier is not found.
	/// </summary>
	/// <param name="floorId">
	///		The unique identifier of the floor that owns the area.
	/// </param>
	/// <param name="areaId">
	///		The unique identifier of the area to update.
	/// </param>
	/// <param name="outline">
	///		The new ordered list of path segments that form the closed outline polygon.
	/// </param>
	public void UpdateAreaOutline(
		Guid floorId,
		Guid areaId,
		IReadOnlyList<PathSegment> outline)
	{
		var floor = _floors.FirstOrDefault(floor =>
			floor.Id == floorId);

		if (floor is null)
		{
			return;
		}

		floor.UpdateAreaOutline(
			areaId,
			outline);
		Touch();
	}

	/// <summary>
	///		Adds a new placed object to the given floor at default scale.
	/// </summary>
	/// <param name="floorId">
	///		The unique identifier of the floor to add the object to.
	/// </param>
	/// <param name="kind">
	///		The kind of object to place.
	/// </param>
	/// <param name="position">
	///		The canvas-space position of the object's bounding-box center, in floor-absolute pixels.
	/// </param>
	/// <param name="rotation">
	///		The rotation to apply to the object.
	/// </param>
	/// <returns>
	///		The newly created <see cref="Area.PlacedObject"/> instance, or <see langword="null"/> if the floor is not found.
	/// </returns>
	public Area.PlacedObject? AddPlacedObject(
		Guid floorId,
		Area.ObjectKind kind,
		CanvasPoint position,
		Area.ObjectRotation rotation)
		=> AddPlacedObject(
			floorId,
			kind,
			position,
			rotation,
			Area.ObjectScale.Default);

	/// <summary>
	///		Adds a new placed object to the given floor.
	/// </summary>
	/// <param name="floorId">
	///		The unique identifier of the floor to add the object to.
	/// </param>
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
	///		The newly created <see cref="Area.PlacedObject"/> instance, or <see langword="null"/> if the floor is not found.
	/// </returns>
	public Area.PlacedObject? AddPlacedObject(
		Guid floorId,
		Area.ObjectKind kind,
		CanvasPoint position,
		Area.ObjectRotation rotation,
		float scale)
	{
		var floor = _floors.FirstOrDefault(floor =>
			floor.Id == floorId);

		if (floor is null)
		{
			return null;
		}

		var result = floor.AddObject(
			kind,
			position,
			rotation,
			scale);

		Touch();

		return result;
	}

	/// <summary>
	///		Removes the placed object with the specified identifier from the given floor.
	///		Has no effect if the floor or object is not found.
	/// </summary>
	/// <param name="floorId">
	///		The unique identifier of the floor that owns the object.
	/// </param>
	/// <param name="objectId">
	///		The unique identifier of the placed object to remove.
	/// </param>
	public void RemovePlacedObject(
		Guid floorId,
		Guid objectId)
	{
		var floor = _floors.FirstOrDefault(floor =>
			floor.Id == floorId);

		if (floor is null)
		{
			return;
		}

		if (floor.RemoveObject(objectId))
		{
			Touch();
		}
	}

	/// <summary>
	///		Updates the position, rotation, and scale of the specified placed object.
	///		Has no effect if the floor or object is not found.
	/// </summary>
	/// <param name="floorId">
	///		The unique identifier of the floor that owns the object.
	/// </param>
	/// <param name="objectId">
	///		The unique identifier of the placed object to update.
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
	public void UpdatePlacedObject(
		Guid floorId,
		Guid objectId,
		CanvasPoint newPosition,
		Area.ObjectRotation newRotation,
		float newScale)
	{
		var floor = _floors.FirstOrDefault(floor =>
			floor.Id == floorId);

		if (floor is null)
		{
			return;
		}

		if (floor.UpdateObject(
			objectId,
			newPosition,
			newRotation,
			newScale))
		{
			Touch();
		}
	}
}
