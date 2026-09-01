using DungeonMaster.App.Core.Map;
using DungeonMaster.Domain.ValueObjects;
using Microsoft.Maui.Graphics;

namespace DungeonMaster.App.Tests.Graphics;

/// <summary>
///		Pins the pixel positions the map paints an offset area at.
/// </summary>
/// <remarks>
///		MASTER-097 removed the per-segment and per-control-point <c>TilePoint</c> allocations by
///		folding the tile offset into the pixel conversion instead of building a shifted
///		<see cref="TilePoint"/> and handing it to
///		<see cref="TileCoordinateHelper.ToPixel"/>. That is an arithmetic rewrite on the rendering
///		path, so it is checked against the helper it replaced rather than against numbers typed by
///		hand.
/// </remarks>
public class FloorMapDrawableGeometryTests
{
	private const float TileSize = 40f;

	/// <summary>
	///		An area carrying a pending move offset is painted at exactly the pixels
	///		<see cref="TileCoordinateHelper.ToPixel"/> reports for each of its tile points shifted
	///		by that offset — start point first, because the outline is closed and begins at the last
	///		segment's end.
	/// </summary>
	[Fact]
	public void PendingMoveOffset_PaintsEveryPointWhereTileCoordinateHelperPutsIt()
	{
		var drawable = MapTestScene.Drawable();
		drawable.TileSize = TileSize;
		drawable.Floor = MapTestScene.FloorWith(MapTestScene.Square(5, 7));
		drawable.SetPendingMoveOffset(MapTestScene.AreaId(0), new TilePoint(3, -2));

		var canvas = new RecordingCanvas();
		drawable.Draw(canvas, new RectF(0f, 0f, 400f, 300f));

		// Closed square (5,7)-(7,9): the path starts at the final segment's end, which is (5,7).
		var expected = new[]
		{
			new TilePoint(5, 7),
			new TilePoint(7, 7),
			new TilePoint(7, 9),
			new TilePoint(5, 9),
			new TilePoint(5, 7),
		}
			.Select(tile =>
			{
				var (x, y) = TileCoordinateHelper.ToPixel(
					new TilePoint(tile.Col + 3, tile.Row - 2),
					TileSize);

				return new PointF(x, y);
			})
			.ToArray();

		Assert.Equal(expected, Assert.Single(canvas.DrawnPaths));
	}

	/// <summary>
	///		The live drag delta is added on top of the pending offset, so an area being dragged after
	///		an earlier committed move is painted at the sum of the two.
	/// </summary>
	[Fact]
	public void DragDeltaOnTopOfPendingOffset_PaintsAtTheSumOfBoth()
	{
		var drawable = MapTestScene.Drawable();
		drawable.TileSize = TileSize;
		drawable.Floor = MapTestScene.FloorWith(MapTestScene.Square(0, 0));
		drawable.SetPendingMoveOffset(MapTestScene.AreaId(0), new TilePoint(4, 6));
		drawable.DragPreviewAreaId = MapTestScene.AreaId(0);
		drawable.DragPreviewDelta = new TilePoint(-1, 2);

		var canvas = new RecordingCanvas();
		drawable.Draw(canvas, new RectF(0f, 0f, 400f, 300f));

		var (startX, startY) = TileCoordinateHelper.ToPixel(new TilePoint(0 + 3, 0 + 8), TileSize);

		Assert.Equal(new PointF(startX, startY), Assert.Single(canvas.DrawnPaths)[0]);
	}

	/// <summary>
	///		A cubic Bezier segment's two control points are offset as well as its end point, so a
	///		curved outline keeps its shape when the area is moved rather than distorting.
	/// </summary>
	[Fact]
	public void CurvedOutlineWithOffset_ShiftsItsControlPointsToo()
	{
		var drawable = MapTestScene.Drawable();
		drawable.TileSize = TileSize;
		drawable.Floor = MapTestScene.FloorWith(MapTestScene.CurvedShape(0, 0));
		drawable.SetPendingMoveOffset(MapTestScene.AreaId(0), new TilePoint(2, 5));

		var canvas = new RecordingCanvas();
		drawable.Draw(canvas, new RectF(0f, 0f, 400f, 300f));

		// CurvedShape's last segment is the curve: Ctrl1 (2,4), Ctrl2 (1,4), End (0,0).
		var expectedTail = new[]
		{
			new TilePoint(2 + 2, 4 + 5),
			new TilePoint(1 + 2, 4 + 5),
			new TilePoint(0 + 2, 0 + 5),
		}
			.Select(tile =>
			{
				var (x, y) = TileCoordinateHelper.ToPixel(tile, TileSize);

				return new PointF(x, y);
			})
			.ToArray();

		var points = Assert.Single(canvas.DrawnPaths);

		Assert.Equal(expectedTail, points[^3..]);
	}

	/// <summary>
	///		An area with no offset at all is painted exactly where its own tile points are, so the
	///		offset arithmetic cannot shift an unmoved area.
	/// </summary>
	[Fact]
	public void NoOffset_PaintsAtTheUnshiftedTilePoints()
	{
		var drawable = MapTestScene.Drawable();
		drawable.TileSize = TileSize;
		drawable.Floor = MapTestScene.FloorWith(MapTestScene.Square(5, 7));

		var canvas = new RecordingCanvas();
		drawable.Draw(canvas, new RectF(0f, 0f, 400f, 300f));

		var (x, y) = TileCoordinateHelper.ToPixel(new TilePoint(5, 7), TileSize);

		Assert.Equal(new PointF(x, y), Assert.Single(canvas.DrawnPaths)[0]);
	}
}
