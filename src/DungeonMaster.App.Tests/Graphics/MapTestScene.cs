using DungeonMaster.App.Core.Presentation;
using DungeonMaster.App.Graphics;
using DungeonMaster.Domain.Entities.Floor;
using DungeonMaster.Domain.ValueObjects;
using Microsoft.Maui.Graphics;

namespace DungeonMaster.App.Tests.Graphics;

/// <summary>
///		Builds the floors and the palette the <see cref="FloorMapDrawable"/> facts draw.
/// </summary>
/// <remarks>
///		The palette is a FIXED set of colours rather than the live theme, because
///		<c>DmTheme</c> reads <c>Application.Current.Resources</c> and this project runs outside a
///		MAUI host — constructing a <c>Microsoft.Maui.Controls.Application</c> here throws a
///		<c>COMException</c> out of the WinUI <c>FocusManager</c>. Supplying the palette through the
///		drawable's constructor is what makes <c>Draw</c> reachable at all from a test.
/// </remarks>
internal static class MapTestScene
{
	/// <summary>The colour a highlighted area's outline is painted in.</summary>
	public static Color FullOpacityStroke { get; } = Color.FromRgb(255, 0, 0);

	/// <summary>The colour a dimmed area's outline is painted in.</summary>
	public static Color DimmedStroke { get; } = Color.FromRgb(0, 0, 255);

	/// <summary>
	///		A palette whose members are all distinguishable, so a fact can name which one was used.
	/// </summary>
	public static FloorMapPalette Palette { get; } = new(
		Grid: Color.FromRgb(1, 1, 1),
		AreaOutline: FullOpacityStroke,
		AreaOutlineDimmed: DimmedStroke,
		ObjectHighlightFill: Color.FromRgb(2, 2, 2),
		ObjectHighlightStroke: Color.FromRgb(3, 3, 3),
		ObjectPlaceholder: Color.FromRgb(4, 4, 4),
		Vertex: Color.FromRgb(5, 5, 5),
		VertexPath: Color.FromRgb(6, 6, 6),
		PhantomSegment: Color.FromRgb(7, 7, 7),
		DistanceLabel: Color.FromRgb(8, 8, 8),
		DistanceLabelFontSize: 9f);

	/// <summary>
	///		A drawable wired to <see cref="Palette"/>.
	/// </summary>
	public static FloorMapDrawable Drawable() =>
		new(() =>
			Palette);

	/// <summary>
	///		A square outline of four line segments whose top-left corner is at
	///		(<paramref name="col"/>, <paramref name="row"/>).
	/// </summary>
	public static IReadOnlyList<PathSegment> Square(int col, int row, int size = 2) =>
	[
		new PathSegment(PathSegmentKind.Line, new TilePoint(col + size, row), null, null),
		new PathSegment(PathSegmentKind.Line, new TilePoint(col + size, row + size), null, null),
		new PathSegment(PathSegmentKind.Line, new TilePoint(col, row + size), null, null),
		new PathSegment(PathSegmentKind.Line, new TilePoint(col, row), null, null),
	];

	/// <summary>
	///		An outline whose last segment is a cubic Bezier, so the control-point path is exercised.
	/// </summary>
	public static IReadOnlyList<PathSegment> CurvedShape(int col, int row) =>
	[
		new PathSegment(PathSegmentKind.Line, new TilePoint(col + 3, row), null, null),
		new PathSegment(PathSegmentKind.Line, new TilePoint(col + 3, row + 3), null, null),
		new PathSegment(
			PathSegmentKind.CubicBezier,
			new TilePoint(col, row),
			new TilePoint(col + 2, row + 4),
			new TilePoint(col + 1, row + 4)),
	];

	/// <summary>
	///		A floor carrying the given area outlines, each with its own identifier.
	/// </summary>
	public static FloorItem FloorWith(params IReadOnlyList<PathSegment>[] outlines)
	{
		var areas = outlines
			.Select((outline, index) =>
				new AreaItem(
					AreaId(index),
					$"Area {index}",
					null,
					null,
					outline))
			.ToList();

		return new FloorItem(
			Guid.Parse("11111111-1111-1111-1111-111111111111"),
			1,
			"Test floor",
			null,
			null,
			areas,
			[]);
	}

	/// <summary>
	///		The identifier <see cref="FloorWith"/> gives the area at <paramref name="index"/>.
	///		Stable across calls so a fact can name the area it hovers or focuses.
	/// </summary>
	public static Guid AreaId(int index) =>
		new(index + 1, 0, 0, [0, 0, 0, 0, 0, 0, 0, 0]);

	/// <summary>
	///		A floor big enough to stand in for a real one in an allocation measurement: twenty
	///		areas of twelve segments each, two of them curved, plus six placed objects.
	/// </summary>
	public static FloorItem RepresentativeFloor()
	{
		var areas = new List<AreaItem>();

		for (var i = 0; i < 20; i++)
		{
			var segments = new List<PathSegment>();

			for (var s = 0; s < 11; s++)
			{
				segments.Add(
					new PathSegment(PathSegmentKind.Line, new TilePoint(i * 4 + s, i * 3), null, null));
			}

			segments.Add(
				i % 10 == 0
					? new PathSegment(
						PathSegmentKind.CubicBezier,
						new TilePoint(i * 4, i * 3),
						new TilePoint(i * 4 + 1, i * 3 + 1),
						new TilePoint(i * 4 + 2, i * 3 + 2))
					: new PathSegment(PathSegmentKind.Line, new TilePoint(i * 4, i * 3), null, null));

			areas.Add(new AreaItem(AreaId(i), $"Area {i}", null, null, segments));
		}

		var objects = Enumerable
			.Range(0, 6)
			.Select(i =>
				new PlacedObjectItem(
					AreaId(100 + i),
					ObjectKind.DoorWood,
					new CanvasPoint(i * 80f, i * 60f),
					ObjectRotation.None,
					ObjectScale.Default))
			.ToList();

		return new FloorItem(
			Guid.Parse("22222222-2222-2222-2222-222222222222"),
			1,
			"Representative floor",
			null,
			null,
			areas,
			objects);
	}
}
