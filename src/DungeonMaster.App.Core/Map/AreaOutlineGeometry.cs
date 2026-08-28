using DungeonMaster.Domain.ValueObjects;

namespace DungeonMaster.App.Core.Map;

/// <summary>
///		Provides pure geometry over an area outline: translation, closed-polygon construction,
///		point containment and area. No MAUI type appears in this file.
/// </summary>
public static class AreaOutlineGeometry
{
	/// <summary>
	///		Returns a new outline in which the end point and both control points of every segment
	///		are offset by the given tile delta. Null control points stay null.
	/// </summary>
	/// <param name="outline">
	///		The ordered segments of the outline to translate. An empty outline returns an empty list.
	/// </param>
	/// <param name="delta">
	///		The offset in tile units to add to every point.
	/// </param>
	/// <returns>
	///		A new list of translated segments. A zero delta returns an equal-valued outline; it is
	///		not required to be the same instance.
	/// </returns>
	/// <exception cref="ArgumentNullException">
	///		<paramref name="outline"/> or <paramref name="delta"/> is null, or any element of
	///		<paramref name="outline"/> is null.
	/// </exception>
	public static IReadOnlyList<PathSegment> Translate(
		IReadOnlyList<PathSegment> outline,
		TilePoint delta)
	{
		ArgumentNullException.ThrowIfNull(outline);
		ArgumentNullException.ThrowIfNull(delta);

		var translated = new List<PathSegment>(outline.Count);

		foreach (var segment in outline)
		{
			ArgumentNullException.ThrowIfNull(segment, nameof(outline));

			translated.Add(segment with
			{
				End = new TilePoint(segment.End.Col + delta.Col, segment.End.Row + delta.Row),
				Ctrl1 = segment.Ctrl1 is null
					? null
					: new TilePoint(segment.Ctrl1.Col + delta.Col, segment.Ctrl1.Row + delta.Row),
				Ctrl2 = segment.Ctrl2 is null
					? null
					: new TilePoint(segment.Ctrl2.Col + delta.Col, segment.Ctrl2.Row + delta.Row),
			});
		}

		return translated;
	}

	/// <summary>
	///		Builds the closed pixel-space polygon of an outline.
	/// </summary>
	/// <param name="outline">
	///		The ordered segments of the outline. An empty outline returns an empty list.
	/// </param>
	/// <param name="tileSize">
	///		The width and height of a single tile in pixels. Must be greater than zero.
	/// </param>
	/// <returns>
	///		The polygon vertices in pixel space, without the duplicated closing vertex.
	/// </returns>
	/// <remarks>
	///		The order matters and is frozen. The polygon is closed, so the last segment's end point
	///		is also the start point: it is emitted first, then every segment's end point in order,
	///		and finally the last vertex is dropped when more than one vertex results, because it
	///		duplicates the first. A drag preview is applied by calling <see cref="Translate"/> first
	///		and passing the translated outline here; this method reads end points only.
	/// </remarks>
	/// <exception cref="ArgumentNullException">
	///		<paramref name="outline"/> is null, or any element is null.
	/// </exception>
	/// <exception cref="ArgumentOutOfRangeException">
	///		<paramref name="tileSize"/> is not greater than zero.
	/// </exception>
	public static IReadOnlyList<(float X, float Y)> ToPolygon(
		IReadOnlyList<PathSegment> outline,
		float tileSize)
	{
		ArgumentNullException.ThrowIfNull(outline);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(tileSize);

		if (outline.Count == 0)
		{
			return [];
		}

		var vertices = new List<(float X, float Y)>(outline.Count + 1);

		var closingSegment = outline[^1];
		ArgumentNullException.ThrowIfNull(closingSegment, nameof(outline));

		var (originX, originY) = TileCoordinateHelper.ToPixel(closingSegment.End, tileSize);
		vertices.Add((originX, originY));

		foreach (var segment in outline)
		{
			ArgumentNullException.ThrowIfNull(segment, nameof(outline));

			var (endX, endY) = TileCoordinateHelper.ToPixel(segment.End, tileSize);
			vertices.Add((endX, endY));
		}

		// The last segment closes back to the origin — drop the duplicate closing vertex.
		if (vertices.Count > 1)
		{
			vertices.RemoveAt(vertices.Count - 1);
		}

		return vertices;
	}

	/// <summary>
	///		Performs a ray-casting point-in-polygon test.
	/// </summary>
	/// <param name="polygon">
	///		The polygon vertices in pixel space. A polygon of fewer than three vertices returns false.
	/// </param>
	/// <param name="x">
	///		The horizontal pixel position of the point to test.
	/// </param>
	/// <param name="y">
	///		The vertical pixel position of the point to test.
	/// </param>
	/// <returns>
	///		True when the point lies inside the polygon.
	/// </returns>
	/// <remarks>
	///		Behaviour exactly on the boundary is whatever the crossing expression yields and is not
	///		specified.
	/// </remarks>
	/// <exception cref="ArgumentNullException">
	///		<paramref name="polygon"/> is null.
	/// </exception>
	public static bool ContainsPoint(
		IReadOnlyList<(float X, float Y)> polygon,
		float x,
		float y)
	{
		ArgumentNullException.ThrowIfNull(polygon);

		var n = polygon.Count;
		if (n < 3)
		{
			return false;
		}

		var inside = false;

		for (int i = 0, j = n - 1; i < n; j = i++)
		{
			var xi = polygon[i].X;
			var yi = polygon[i].Y;
			var xj = polygon[j].X;
			var yj = polygon[j].Y;

			if (((yi > y) != (yj > y)) &&
				x < (xj - xi) * (y - yi) / (yj - yi) + xi)
			{
				inside = !inside;
			}
		}

		return inside;
	}

	/// <summary>
	///		Calculates the absolute area of a polygon by the shoelace formula.
	/// </summary>
	/// <param name="polygon">
	///		The polygon vertices in pixel space.
	/// </param>
	/// <returns>
	///		The absolute area. Winding order does not affect the result.
	/// </returns>
	/// <exception cref="ArgumentNullException">
	///		<paramref name="polygon"/> is null.
	/// </exception>
	public static float Area(IReadOnlyList<(float X, float Y)> polygon)
	{
		ArgumentNullException.ThrowIfNull(polygon);

		var n = polygon.Count;
		var sum = 0f;

		for (int i = 0, j = n - 1; i < n; j = i++)
		{
			sum += (polygon[j].X + polygon[i].X) * (polygon[j].Y - polygon[i].Y);
		}

		return MathF.Abs(sum * 0.5f);
	}
}
