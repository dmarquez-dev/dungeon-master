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
///		The first control point. Required when <see cref="PathSegmentKind.CubicBezier"/> is specified, and must be
///		<see langword="null"/> when <see cref="PathSegmentKind.Line"/> is specified.
/// </param>
/// <param name="Ctrl2">
///		The second control point. Required when <see cref="PathSegmentKind.CubicBezier"/> is specified, and must be
///		<see langword="null"/> when <see cref="PathSegmentKind.Line"/> is specified.
/// </param>
/// <exception cref="ArgumentException">
///		<paramref name="Kind"/> is <see cref="PathSegmentKind.CubicBezier"/> and either control point is
///		<see langword="null"/>, or <paramref name="Kind"/> is <see cref="PathSegmentKind.Line"/> and either control
///		point is non-<see langword="null"/> (MASTER-170).
/// </exception>
/// <exception cref="ArgumentOutOfRangeException">
///		Thrown when <paramref name="Kind"/> is not a defined <see cref="PathSegmentKind"/>.
/// </exception>
public record PathSegment(
	PathSegmentKind Kind,
	TilePoint End,
	TilePoint? Ctrl1,
	TilePoint? Ctrl2)
{
	/// <summary>
	///		The geometric kind of this segment. Redeclared so that the primary constructor validates the control
	///		points against it: a positional record has no constructor body, so the initializer is the only hook.
	/// </summary>
	public PathSegmentKind Kind { get; init; } = ValidateControlPoints(Kind, Ctrl1, Ctrl2);

	// The parameters deliberately carry the record's positional names rather than camel case: the reported
	// ParamName must be Kind, Ctrl1 or Ctrl2, the names a caller wrote, and CA2208 requires that the paramName
	// argument match a parameter of the throwing method exactly.
	private static PathSegmentKind ValidateControlPoints(
		PathSegmentKind Kind,
		TilePoint? Ctrl1,
		TilePoint? Ctrl2)
	{
		if (Kind == PathSegmentKind.CubicBezier)
		{
			if (Ctrl1 is null)
			{
				throw new ArgumentException(
					"A CubicBezier segment requires both control points.",
					nameof(Ctrl1));
			}

			if (Ctrl2 is null)
			{
				throw new ArgumentException(
					"A CubicBezier segment requires both control points.",
					nameof(Ctrl2));
			}
		}
		else if (Kind == PathSegmentKind.Line)
		{
			if (Ctrl1 is not null)
			{
				throw new ArgumentException(
					"A Line segment must not carry control points.",
					nameof(Ctrl1));
			}

			if (Ctrl2 is not null)
			{
				throw new ArgumentException(
					"A Line segment must not carry control points.",
					nameof(Ctrl2));
			}
		}
		else
		{
			throw new ArgumentOutOfRangeException(
				nameof(Kind),
				Kind,
				$"Kind must be a defined {nameof(PathSegmentKind)}.");
		}

		return Kind;
	}
}
