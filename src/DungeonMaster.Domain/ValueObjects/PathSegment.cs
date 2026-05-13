namespace DungeonMaster.Domain.ValueObjects;

/// <summary>
///		Represents a single segment in an area outline path, supporting both straight lines and cubic Bezier curves.
/// </summary>
/// <param name="Kind">
///		The geometric kind of this segment.
/// </param>
/// <param name="End">
///		The end point of this segment on the tile grid.
/// </param>
/// <param name="Ctrl1">
///		The first control point, used only when <see cref="PathSegmentKind.CubicBezier"/> is specified.
/// </param>
/// <param name="Ctrl2">
///		The second control point, used only when <see cref="PathSegmentKind.CubicBezier"/> is specified.
/// </param>
public record PathSegment(
	PathSegmentKind Kind,
	TilePoint End,
	TilePoint? Ctrl1,
	TilePoint? Ctrl2);
