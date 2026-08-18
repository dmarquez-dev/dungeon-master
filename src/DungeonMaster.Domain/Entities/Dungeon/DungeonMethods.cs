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
	/// <returns>
	///		<see cref="MutationOutcome.Success"/> when the floor was removed, or <see cref="MutationOutcome.NotFound"/> when no floor with the given identifier exists.
	/// </returns>
	public MutationOutcome RemoveFloor(Guid floorId)
	{
		var floor = _floors.FirstOrDefault(floor =>
			floor.Id == floorId);

		if (floor is null)
		{
			return MutationOutcome.NotFound;
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

		return MutationOutcome.Success;
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
	/// <returns>
	///		<see cref="MutationOutcome.Success"/> when the floor was moved or already carried the requested number, <see cref="MutationOutcome.NotFound"/> when no floor with the given identifier exists, or <see cref="MutationOutcome.OutOfRange"/> when the target number lies outside the permitted range.
	/// </returns>
	public MutationOutcome ReorderFloor(
		Guid floorId,
		int newNumber)
	{
		var floor = _floors.FirstOrDefault(floor =>
			floor.Id == floorId);

		if (floor is null)
		{
			return MutationOutcome.NotFound;
		}

		var oldNumber = floor.Number;

		if (oldNumber == newNumber)
		{
			return MutationOutcome.Success;
		}

		if (newNumber < 1 || newNumber > _floors.Count)
		{
			return MutationOutcome.OutOfRange;
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

		return MutationOutcome.Success;
	}

	/// <summary>
	///		Updates the name, description, and notes of the specified floor.
	/// </summary>
	/// <param name="floorId">
	///		The unique identifier of the floor to update.
	/// </param>
	/// <param name="name">
	///		The new optional display name for the floor.
	/// </param>
	/// <param name="description">
	///		The new optional player-facing description for the floor.
	/// </param>
	/// <param name="notes">
	///		The new optional notes for the floor.
	/// </param>
	/// <returns>
	///		<see cref="MutationOutcome.Success"/> when the floor was updated, or <see cref="MutationOutcome.NotFound"/> when no floor with the given identifier exists.
	/// </returns>
	public MutationOutcome UpdateFloor(
		Guid floorId,
		string? name,
		string? description,
		string? notes)
	{
		var floor = _floors.FirstOrDefault(floor =>
			floor.Id == floorId);

		if (floor is null)
		{
			return MutationOutcome.NotFound;
		}

		floor.UpdateDetails(name, description, notes);
		Touch();

		return MutationOutcome.Success;
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
	///		The newly created <see cref="Area.Area"/> instance, or <see langword="null"/> when no floor with the given identifier exists.
	/// </returns>
	public Area.Area? AddArea(
		Guid floorId,
		string name,
		string? description)
	{
		var floor = _floors.FirstOrDefault(floor =>
			floor.Id == floorId);

		if (floor is null)
		{
			return null;
		}

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
	/// <returns>
	///		<see cref="MutationOutcome.Success"/> when the area was removed, or <see cref="MutationOutcome.NotFound"/> when the floor or the area was not found.
	/// </returns>
	public MutationOutcome RemoveArea(
		Guid floorId,
		Guid areaId)
	{
		var floor = _floors.FirstOrDefault(floor =>
			floor.Id == floorId);

		if (floor is null)
		{
			return MutationOutcome.NotFound;
		}

		var outcome = floor.RemoveArea(areaId);

		if (outcome is not MutationOutcome.Success)
		{
			return outcome;
		}

		Touch();

		return MutationOutcome.Success;
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
	/// <returns>
	///		<see cref="MutationOutcome.Success"/> when the area was updated, or <see cref="MutationOutcome.NotFound"/> when the floor or the area was not found.
	/// </returns>
	public MutationOutcome UpdateArea(
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
			return MutationOutcome.NotFound;
		}

		var outcome = floor.UpdateArea(
			areaId,
			name,
			description,
			notes);

		if (outcome is not MutationOutcome.Success)
		{
			return outcome;
		}

		Touch();

		return MutationOutcome.Success;
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
	/// <returns>
	///		<see cref="MutationOutcome.Success"/> when the outline was replaced, or <see cref="MutationOutcome.NotFound"/> when the floor or the area was not found.
	/// </returns>
	public MutationOutcome UpdateAreaOutline(
		Guid floorId,
		Guid areaId,
		IReadOnlyList<PathSegment> outline)
	{
		var floor = _floors.FirstOrDefault(floor =>
			floor.Id == floorId);

		if (floor is null)
		{
			return MutationOutcome.NotFound;
		}

		var outcome = floor.UpdateAreaOutline(
			areaId,
			outline);

		if (outcome is not MutationOutcome.Success)
		{
			return outcome;
		}

		Touch();

		return MutationOutcome.Success;
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
	///		The newly created <see cref="Floor.PlacedObject"/> instance, or <see langword="null"/> if the floor is not found.
	/// </returns>
	public Floor.PlacedObject? AddPlacedObject(
		Guid floorId,
		Floor.ObjectKind kind,
		CanvasPoint position,
		Floor.ObjectRotation rotation)
		=> AddPlacedObject(
			floorId,
			kind,
			position,
			rotation,
			Floor.ObjectScale.Default);

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
	///		The newly created <see cref="Floor.PlacedObject"/> instance, or <see langword="null"/> if the floor is not found.
	/// </returns>
	public Floor.PlacedObject? AddPlacedObject(
		Guid floorId,
		Floor.ObjectKind kind,
		CanvasPoint position,
		Floor.ObjectRotation rotation,
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
	/// <returns>
	///		<see cref="MutationOutcome.Success"/> when the object was removed, or <see cref="MutationOutcome.NotFound"/> when the floor or the object was not found.
	/// </returns>
	public MutationOutcome RemovePlacedObject(
		Guid floorId,
		Guid objectId)
	{
		var floor = _floors.FirstOrDefault(floor =>
			floor.Id == floorId);

		if (floor is null)
		{
			return MutationOutcome.NotFound;
		}

		var outcome = floor.RemoveObject(objectId);

		if (outcome is not MutationOutcome.Success)
		{
			return outcome;
		}

		Touch();

		return MutationOutcome.Success;
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
	/// <returns>
	///		<see cref="MutationOutcome.Success"/> when the object was updated, or <see cref="MutationOutcome.NotFound"/> when the floor or the object was not found.
	/// </returns>
	public MutationOutcome UpdatePlacedObject(
		Guid floorId,
		Guid objectId,
		CanvasPoint newPosition,
		Floor.ObjectRotation newRotation,
		float newScale)
	{
		var floor = _floors.FirstOrDefault(floor =>
			floor.Id == floorId);

		if (floor is null)
		{
			return MutationOutcome.NotFound;
		}

		var outcome = floor.UpdateObject(
			objectId,
			newPosition,
			newRotation,
			newScale);

		if (outcome is not MutationOutcome.Success)
		{
			return outcome;
		}

		Touch();

		return MutationOutcome.Success;
	}
}
