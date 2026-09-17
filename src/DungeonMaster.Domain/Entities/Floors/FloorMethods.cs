using DungeonMaster.Domain.ValueObjects;

namespace DungeonMaster.Domain.Entities.Floors;

public sealed partial class Floor
{
	/// <summary>
	///		Updates the sequential number of this floor.
	/// </summary>
	/// <param name="number">
	///		The new floor number to assign.
	/// </param>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="number"/> is less than 1.</exception>
	internal void UpdateNumber(int number)
	{
		if (number < 1)
		{
			throw new ArgumentOutOfRangeException(
				nameof(number),
				number,
				"Floor number must be 1 or greater.");
		}

		Number = number;

		Touch();
	}

	/// <summary>
	///		Updates the display name, description, and notes of this floor.
	/// </summary>
	/// <param name="name">
	///		The new optional display name for the floor.
	/// </param>
	/// <param name="description">
	///		The new optional player-facing description for the floor.
	/// </param>
	/// <param name="notes">
	///		The new optional notes for the floor.
	/// </param>
	internal void UpdateDetails(
		string? name,
		string? description,
		string? notes)
	{
		Name = name;
		Description = description;
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
	///		The newly created <see cref="Areas.Area"/> instance.
	/// </returns>
	internal Areas.Area AddArea(
		string name,
		string? description)
	{
		var area = new Areas.Area(
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
	/// <returns>
	///		<see cref="MutationOutcome.Success"/> when the area was removed, or <see cref="MutationOutcome.NotFound"/> when no area with the given identifier exists.
	/// </returns>
	internal MutationOutcome RemoveArea(Guid areaId)
	{
		var area = _areas.FirstOrDefault(area =>
			area.Id == areaId);

		if (area is null)
		{
			return MutationOutcome.NotFound;
		}

		_areas.Remove(area);
		Touch();

		return MutationOutcome.Success;
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
	/// <returns>
	///		<see cref="MutationOutcome.Success"/> when the area was updated, or <see cref="MutationOutcome.NotFound"/> when no area with the given identifier exists.
	/// </returns>
	internal MutationOutcome UpdateArea(
		Guid areaId,
		string name,
		string? description,
		string? notes)
	{
		var area = _areas.FirstOrDefault(area =>
			area.Id == areaId);

		if (area is null)
		{
			return MutationOutcome.NotFound;
		}

		area.Update(
			name,
			description,
			notes);
		Touch();

		return MutationOutcome.Success;
	}

	/// <summary>
	///		Replaces the outline path of the specified area on this floor.
	///		Has no effect if no area with the given identifier exists.
	/// </summary>
	/// <param name="areaId">
	///		The unique identifier of the area to update.
	/// </param>
	/// <param name="outline">
	///		The new ordered list of path segments that form the closed outline polygon.
	/// </param>
	/// <returns>
	///		<see cref="MutationOutcome.Success"/> when the outline was replaced, or <see cref="MutationOutcome.NotFound"/> when no area with the given identifier exists.
	/// </returns>
	internal MutationOutcome UpdateAreaOutline(
		Guid areaId,
		IReadOnlyList<PathSegment> outline)
	{
		var area = _areas.FirstOrDefault(area =>
			area.Id == areaId);

		if (area is null)
		{
			return MutationOutcome.NotFound;
		}

		area.SetOutline(outline);

		Touch();

		return MutationOutcome.Success;
	}
}
