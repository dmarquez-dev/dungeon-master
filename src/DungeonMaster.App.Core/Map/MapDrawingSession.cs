using DungeonMaster.Domain.ValueObjects;

namespace DungeonMaster.App.Core.Map;

/// <summary>
///		Represents the result of a single vertex placement attempt in a <see cref="MapDrawingSession"/>.
/// </summary>
public enum DrawSessionResult
{
	/// <summary>
	///		No placement attempt was made. Never returned by <see cref="MapDrawingSession.PlaceVertex"/>.
	/// </summary>
	None,

	/// <summary>
	///		The vertex was accepted and added to the in-progress polygon.
	/// </summary>
	Added,

	/// <summary>
	///		The vertex closed the polygon because it was near the origin with at least three prior vertices.
	/// </summary>
	Closed,

	/// <summary>
	///		The vertex was ignored because it was identical to the last placed vertex.
	/// </summary>
	Ignored,
}

/// <summary>
///		Tracks the in-progress state of a freehand polyline drawing operation on the dungeon map.
///		Callers place tile-grid vertices one at a time and the session determines when the polygon is closed.
/// </summary>
public sealed class MapDrawingSession
{
	private readonly List<TilePoint> _vertices = [];

	/// <summary>
	///		Gets the ordered list of tile-grid vertices placed so far.
	/// </summary>
	public IReadOnlyList<TilePoint> Vertices =>
		_vertices.AsReadOnly();

	/// <summary>
	///		Gets a value indicating whether at least one vertex has been placed.
	/// </summary>
	public bool IsActive =>
		_vertices.Count > 0;

	/// <summary>
	///		Attempts to place a new vertex at the given tile-grid point.
	/// </summary>
	/// <remarks>
	///		Returns <see cref="DrawSessionResult.Ignored"/> when the candidate equals the last vertex.
	///		Returns <see cref="DrawSessionResult.Closed"/> when three or more vertices are already placed
	///		and the candidate snaps to exactly the same tile as the first vertex.
	///		Otherwise returns <see cref="DrawSessionResult.Added"/>.
	/// </remarks>
	/// <param name="point">
	///		The tile-grid point at which to place the new vertex.
	/// </param>
	/// <returns>
	///		A <see cref="DrawSessionResult"/> describing the outcome of the placement attempt.
	/// </returns>
	public DrawSessionResult PlaceVertex(TilePoint point)
	{
		if (_vertices.Count > 0 && point == _vertices[^1])
		{
			return DrawSessionResult.Ignored;
		}

		if (_vertices.Count >= 3 && point == _vertices[0])
		{
			return DrawSessionResult.Closed;
		}

		_vertices.Add(point);
		return DrawSessionResult.Added;
	}

	/// <summary>
	///		Removes the last placed vertex and returns whether a vertex was removed.
	/// </summary>
	/// <returns>
	///		<see langword="true"/> if a vertex was removed; <see langword="false"/> if the session had no vertices.
	/// </returns>
	public bool UndoLastVertex()
	{
		if (_vertices.Count == 0)
		{
			return false;
		}

		_vertices.RemoveAt(_vertices.Count - 1);
		return true;
	}

	/// <summary>
	///		Clears all vertices and resets the session to its initial state.
	/// </summary>
	public void Reset()
	{
		_vertices.Clear();
	}

	/// <summary>
	///		Builds the list of line path segments that represent the closed outline polygon.
	///		The closing segment returns to the first vertex.
	///		Returns an empty list when fewer than three vertices have been placed.
	/// </summary>
	/// <returns>
	///		An ordered <see cref="IReadOnlyList{T}"/> of <see cref="PathSegment"/> values forming the polygon outline,
	///		or an empty list when the polygon is not yet closable.
	/// </returns>
	public IReadOnlyList<PathSegment> BuildOutline()
	{
		if (_vertices.Count < 3)
		{
			return [];
		}

		var segments = new List<PathSegment>(_vertices.Count);

		for (var index = 1; index < _vertices.Count; index++)
		{
			segments.Add(new PathSegment(PathSegmentKind.Line, _vertices[index], null, null));
		}

		segments.Add(new PathSegment(PathSegmentKind.Line, _vertices[0], null, null));

		return segments;
	}
}
