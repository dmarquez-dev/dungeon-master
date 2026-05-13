namespace DungeonMaster.Domain.ValueObjects;

/// <summary>
///		Identifies the geometric kind of a path segment used in an area outline.
/// </summary>
public enum PathSegmentKind
{
	/// <summary>
	///		A straight line from the previous point to the end point.
	/// </summary>
	Line,

	/// <summary>
	///		A cubic Bezier curve defined by two control points and an end point.
	/// </summary>
	CubicBezier
}
