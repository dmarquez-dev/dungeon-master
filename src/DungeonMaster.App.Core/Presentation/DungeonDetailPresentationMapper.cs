using DungeonMaster.Application.Features.Dungeons.Queries.GetDungeonDetail;

namespace DungeonMaster.App.Core.Presentation;

/// <summary>
///		Translates the GetDungeonDetail query projection into the control kit's own presentation shapes.
///		This is the only type in DungeonMaster.App.Core permitted to name a type from DungeonMaster.Application.Features.
///		Every method here is pure: it reads no ambient state, holds none, and mapping the same input twice yields equal results.
/// </summary>
public static class DungeonDetailPresentationMapper
{
	/// <summary>
	///		Maps a single area projection to the control kit's area shape.
	/// </summary>
	/// <param name="area">
	///		The area projection to map.
	/// </param>
	/// <returns>
	///		The mapped <see cref="AreaItem"/>.
	/// </returns>
	/// <exception cref="ArgumentNullException">
	///		Thrown when <paramref name="area"/> is <see langword="null"/>.
	/// </exception>
	internal static AreaItem ToItem(AreaDto area)
	{
		ArgumentNullException.ThrowIfNull(area);

		return new AreaItem(
			area.Id,
			area.Name,
			area.Description,
			area.Notes,
			area.Outline);
	}

	/// <summary>
	///		Maps a single placed-object projection to the control kit's placed-object shape.
	/// </summary>
	/// <param name="placedObject">
	///		The placed-object projection to map.
	/// </param>
	/// <returns>
	///		The mapped <see cref="PlacedObjectItem"/>.
	/// </returns>
	/// <exception cref="ArgumentNullException">
	///		Thrown when <paramref name="placedObject"/> is <see langword="null"/>.
	/// </exception>
	internal static PlacedObjectItem ToItem(PlacedObjectDto placedObject)
	{
		ArgumentNullException.ThrowIfNull(placedObject);

		return new PlacedObjectItem(
			placedObject.Id,
			placedObject.Kind,
			placedObject.Position,
			placedObject.Rotation,
			placedObject.Scale);
	}

	/// <summary>
	///		Maps a single floor projection, and every area and object it carries, to the control kit's floor shape.
	///		Areas and objects are mapped index-for-index, so both collections keep the order the query produced.
	/// </summary>
	/// <param name="floor">
	///		The floor projection to map.
	/// </param>
	/// <returns>
	///		The mapped <see cref="FloorItem"/>.
	/// </returns>
	/// <exception cref="ArgumentNullException">
	///		Thrown when <paramref name="floor"/> is <see langword="null"/>.
	/// </exception>
	/// <exception cref="ArgumentException">
	///		Thrown when an element of the floor's Areas or Objects collection is <see langword="null"/>.
	///		The message names the collection and the zero-based index of the null element, and no element is ever silently skipped.
	/// </exception>
	internal static FloorItem ToItem(FloorDto floor)
	{
		ArgumentNullException.ThrowIfNull(floor);

		var areas = new AreaItem[floor.Areas.Count];

		for (var index = 0; index < floor.Areas.Count; index++)
		{
			var area = floor.Areas[index];

			if (area is null)
			{
				throw new ArgumentException(
					$"floor.Areas contains a null element at index {index}.",
					nameof(floor));
			}

			areas[index] = ToItem(area);
		}

		var objects = new PlacedObjectItem[floor.Objects.Count];

		for (var index = 0; index < floor.Objects.Count; index++)
		{
			var placedObject = floor.Objects[index];

			if (placedObject is null)
			{
				throw new ArgumentException(
					$"floor.Objects contains a null element at index {index}.",
					nameof(floor));
			}

			objects[index] = ToItem(placedObject);
		}

		return new FloorItem(
			floor.Id,
			floor.Number,
			floor.Name,
			floor.Description,
			floor.Notes,
			areas,
			objects);
	}

	/// <summary>
	///		Maps every floor projection carried by a GetDungeonDetail result to the control kit's floor shape.
	///		This is the single translation point an external caller uses, and floors are mapped index-for-index so the query's order survives.
	/// </summary>
	/// <param name="floors">
	///		The floor projections to map.
	/// </param>
	/// <returns>
	///		The mapped floors, in the order they were supplied.
	/// </returns>
	/// <exception cref="ArgumentNullException">
	///		Thrown when <paramref name="floors"/> is <see langword="null"/>.
	/// </exception>
	/// <exception cref="ArgumentException">
	///		Thrown when an element of <paramref name="floors"/>, or of a floor's Areas or Objects collection, is <see langword="null"/>.
	///		The message names the collection and the zero-based index of the null element, and no element is ever silently skipped.
	/// </exception>
	public static IReadOnlyList<FloorItem> ToItems(IReadOnlyList<FloorDto> floors)
	{
		ArgumentNullException.ThrowIfNull(floors);

		var items = new FloorItem[floors.Count];

		for (var index = 0; index < floors.Count; index++)
		{
			var floor = floors[index];

			if (floor is null)
			{
				throw new ArgumentException(
					$"floors contains a null element at index {index}.",
					nameof(floors));
			}

			items[index] = ToItem(floor);
		}

		return items;
	}
}
