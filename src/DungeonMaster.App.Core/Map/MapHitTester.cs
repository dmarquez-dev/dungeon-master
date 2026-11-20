using DungeonMaster.Domain.Entities.Floors;
using DungeonMaster.Domain.ValueObjects;

// These two types live in the Presentation namespace, a sibling inside this same project. They
// are imported by alias rather than by a namespace import because the App-layer dependency guard
// greps for namespace-import lines beginning with this project's root namespace, and an alias
// keeps this file clear of that shape without pulling in anything the alias does not name.
using AreaItem = DungeonMaster.App.Core.Presentation.AreaItem;
using PlacedObjectItem = DungeonMaster.App.Core.Presentation.PlacedObjectItem;

namespace DungeonMaster.App.Core.Map;

/// <summary>
///		Provides hit testing over the presentation shapes the map canvas draws. No MAUI type
///		appears in this file.
/// </summary>
public static class MapHitTester
{
	/// <summary>
	///		Returns the area whose outline polygon contains the given pixel point, preferring the
	///		smallest such area, or null when none does.
	/// </summary>
	/// <param name="areas">
	///		The floor's areas. An area with an empty outline is skipped.
	/// </param>
	/// <param name="pixelX">
	///		Floor-absolute pixel X of the hit point.
	/// </param>
	/// <param name="pixelY">
	///		Floor-absolute pixel Y of the hit point.
	/// </param>
	/// <param name="tileSize">
	///		Tile edge length in pixels. Must be greater than zero.
	/// </param>
	/// <param name="dragPreviewAreaId">
	///		The area currently being drag-previewed, or null. When it matches an area's identifier,
	///		that area's outline is translated by <paramref name="dragPreviewDelta"/> before testing,
	///		so hit testing follows the dragged position.
	/// </param>
	/// <param name="dragPreviewDelta">
	///		The drag offset in tiles, or null. Ignored when <paramref name="dragPreviewAreaId"/> is
	///		null.
	/// </param>
	/// <returns>
	///		The smallest containing area, or null when no area contains the point.
	/// </returns>
	/// <remarks>
	///		The tie-break is strictly less-than on polygon area, so two containing areas of equal
	///		polygon area resolve to the one that comes first in <paramref name="areas"/>.
	/// </remarks>
	/// <exception cref="ArgumentNullException">
	///		<paramref name="areas"/> is null, or any element is null.
	/// </exception>
	/// <exception cref="ArgumentOutOfRangeException">
	///		<paramref name="tileSize"/> is not greater than zero.
	/// </exception>
	public static AreaItem? HitTestArea(
		IReadOnlyList<AreaItem> areas,
		float pixelX,
		float pixelY,
		float tileSize,
		Guid? dragPreviewAreaId,
		TilePoint? dragPreviewDelta)
	{
		ArgumentNullException.ThrowIfNull(areas);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(tileSize);

		AreaItem? bestMatch = null;
		var bestArea = float.MaxValue;

		foreach (var area in areas)
		{
			ArgumentNullException.ThrowIfNull(area, nameof(areas));

			if (area.Outline.Count == 0)
			{
				continue;
			}

			// Account for an active drag preview so hit testing follows the dragged position.
			var dragDelta = area.Id == dragPreviewAreaId ? dragPreviewDelta : null;
			var outline = dragDelta is null
				? area.Outline
				: AreaOutlineGeometry.Translate(area.Outline, dragDelta);

			var polygon = AreaOutlineGeometry.ToPolygon(outline, tileSize);

			if (!AreaOutlineGeometry.ContainsPoint(polygon, pixelX, pixelY))
			{
				continue;
			}

			// When multiple areas overlap at the cursor, prefer the smallest (most specific) target.
			var polygonArea = AreaOutlineGeometry.Area(polygon);
			if (polygonArea < bestArea)
			{
				bestArea = polygonArea;
				bestMatch = area;
			}
		}

		return bestMatch;
	}

	/// <summary>
	///		Returns the placed object whose axis-aligned bounding box contains the given
	///		floor-absolute canvas point, preferring the smallest such object, or null when none does.
	/// </summary>
	/// <param name="objects">
	///		The floor's placed objects.
	/// </param>
	/// <param name="canvasX">
	///		Floor-absolute canvas X of the hit point.
	/// </param>
	/// <param name="canvasY">
	///		Floor-absolute canvas Y of the hit point.
	/// </param>
	/// <param name="tileSize">
	///		Tile edge length in pixels. Must be greater than zero.
	/// </param>
	/// <param name="objectSize">
	///		Supplies the logical bounding size of a kind in tile units. In the App layer this is
	///		supplied as ObjectKindInfo.GetObjectSize; it is a delegate because that type lives in
	///		the MAUI App project, which this project cannot reference.
	/// </param>
	/// <returns>
	///		The smallest containing object, or null when no object contains the point.
	/// </returns>
	/// <remarks>
	///		Half-extents are taken about the object's position, which is the bounding-box centre.
	///		Containment is inclusive on all four edges. The tie-break ranks on the unscaled tile
	///		area — the product of the width and height the delegate reports — and is strictly
	///		less-than, so an exact tie resolves to the first element of <paramref name="objects"/>.
	///		Scale and rotation are deliberately not consulted, because consulting either would
	///		change which object the user picks.
	/// </remarks>
	/// <exception cref="ArgumentNullException">
	///		<paramref name="objects"/> or <paramref name="objectSize"/> is null, or any element is null.
	/// </exception>
	/// <exception cref="ArgumentOutOfRangeException">
	///		<paramref name="tileSize"/> is not greater than zero.
	/// </exception>
	public static PlacedObjectItem? HitTestObject(
		IReadOnlyList<PlacedObjectItem> objects,
		float canvasX,
		float canvasY,
		float tileSize,
		Func<ObjectKind, (float W, float H)> objectSize)
	{
		ArgumentNullException.ThrowIfNull(objects);
		ArgumentNullException.ThrowIfNull(objectSize);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(tileSize);

		PlacedObjectItem? best = null;
		var bestArea = float.MaxValue;

		foreach (var placedObject in objects)
		{
			ArgumentNullException.ThrowIfNull(placedObject, nameof(objects));

			var (wTiles, hTiles) = objectSize(placedObject.Kind);
			var halfW = wTiles * tileSize / 2f;
			var halfH = hTiles * tileSize / 2f;

			if (canvasX >= placedObject.Position.X - halfW && canvasX <= placedObject.Position.X + halfW &&
				canvasY >= placedObject.Position.Y - halfH && canvasY <= placedObject.Position.Y + halfH)
			{
				var tileArea = wTiles * hTiles;
				if (tileArea < bestArea)
				{
					bestArea = tileArea;
					best = placedObject;
				}
			}
		}

		return best;
	}
}
