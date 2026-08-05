using DungeonMaster.Application.Common.Map;
using DungeonMaster.Domain.ValueObjects;
using FluentAssertions;

namespace DungeonMaster.Tests.Application.Common.Map;

public class TileCoordinateHelperTests
{
	private const float TileSize = 20f;

	[Theory]
	[InlineData(0f, 0f, 0, 0)]
	[InlineData(10f, 10f, 1, 1)]
	[InlineData(19f, 19f, 1, 1)]
	[InlineData(40f, 60f, 2, 3)]
	[InlineData(41f, 59f, 2, 3)]
	public void SnapToGrid_ReturnsNearestTilePoint(float x, float y, int expectedCol, int expectedRow)
	{
		var result = TileCoordinateHelper.SnapToGrid(x, y, TileSize);
		result.Col.Should().Be(expectedCol);
		result.Row.Should().Be(expectedRow);
	}

	[Theory]
	[InlineData(0, 0, 0f, 0f)]
	[InlineData(1, 2, 20f, 40f)]
	[InlineData(3, 5, 60f, 100f)]
	public void ToPixel_ReturnsTopLeftOfTile(int col, int row, float expectedX, float expectedY)
	{
		var (x, y) = TileCoordinateHelper.ToPixel(new TilePoint(col, row), TileSize);
		x.Should().Be(expectedX);
		y.Should().Be(expectedY);
	}

	[Theory]
	[InlineData(0, 0, 0, 0, true)]
	[InlineData(0, 0, 1, 0, true)]
	[InlineData(0, 0, 1, 1, true)]
	[InlineData(0, 0, 2, 0, false)]
	[InlineData(0, 0, 0, 2, false)]
	public void IsNearOrigin_ReturnsTrueWithinOneTile(
		int originCol, int originRow,
		int candidateCol, int candidateRow,
		bool expected)
	{
		var origin = new TilePoint(originCol, originRow);
		var candidate = new TilePoint(candidateCol, candidateRow);
		var result = TileCoordinateHelper.IsNearOrigin(candidate, origin, TileSize);
		result.Should().Be(expected);
	}

	// SnapToTileCenter
	[Theory]
	[InlineData(0f,  0f,  40f, 0, 0)]   // top-left corner → tile (0,0)
	[InlineData(20f, 20f, 40f, 0, 0)]   // center of tile (0,0) → tile (0,0)
	[InlineData(39f, 39f, 40f, 0, 0)]   // near bottom-right of tile (0,0) → still tile (0,0)
	[InlineData(40f, 0f,  40f, 1, 0)]   // exactly on tile boundary → tile (1,0)
	[InlineData(55f, 85f, 40f, 1, 2)]   // middle of tile (1,2) → tile (1,2)
	public void SnapToTileCenter_ReturnsContainingTile(
		float x, float y, float tileSize, int expectedCol, int expectedRow)
	{
		var result = TileCoordinateHelper.SnapToTileCenter(x, y, tileSize);

		result.Should().Be(new TilePoint(expectedCol, expectedRow));
	}

	// SnapToNearestVertex
	[Theory]
	[InlineData(0f,   0f,  40f, 0, 0)]   // at origin vertex
	[InlineData(19f,  19f, 40f, 0, 0)]   // closer to (0,0) vertex
	[InlineData(21f,  19f, 40f, 1, 0)]   // closer to (1,0) vertex
	[InlineData(80f,  80f, 40f, 2, 2)]   // exactly at (2,2) vertex
	[InlineData(60f,  60f, 40f, 2, 2)]   // midpoint between 4 vertices → nearest is (2,2) (rounding up)
	public void SnapToNearestVertex_ReturnsNearestGridIntersection(
		float x, float y, float tileSize, int expectedCol, int expectedRow)
	{
		var result = TileCoordinateHelper.SnapToNearestVertex(x, y, tileSize);

		result.Should().Be(new VertexPoint(expectedCol, expectedRow));
	}

	// ── SnapToNearestEdgeCenter ───────────────────────────────────────────────

	private const float TS = 60f;

	[Fact]
	public void SnapToNearestEdgeCenter_NearNorthEdgeFromBelow_ReturnsNorthEdge()
	{
		// cursor is just below the N edge of tile (1,1) at pixel (90, 60)
		// N edge of (1,1) center = (1.5*TS, 1*TS) = (90, 60)
		// cursor at (90, 65) — below the edge
		var (edge, dist) = TileCoordinateHelper.SnapToNearestEdgeCenter(90f, 65f, TS);

		edge.Should().Be(new EdgePoint(1, 1, EdgeSide.N));
		dist.Should().BeApproximately(5f, 0.01f);
	}

	[Fact]
	public void SnapToNearestEdgeCenter_NearNorthEdgeFromAbove_ReturnsSouthEdgeOfUpperTile()
	{
		// cursor just above N edge of tile (1,1) = S edge of tile (1,0)
		// edge at pixel (90, 60), cursor at (90, 55)
		var (edge, _) = TileCoordinateHelper.SnapToNearestEdgeCenter(90f, 55f, TS);

		edge.Should().Be(new EdgePoint(1, 0, EdgeSide.S));
	}

	[Fact]
	public void SnapToNearestEdgeCenter_NearEastEdgeFromRight_ReturnsWestEdgeOfRightTile()
	{
		// E edge of tile (0,0) = W edge of tile (1,0) at pixel (60, 30)
		// cursor at (65, 30) — to the right of the edge
		var (edge, _) = TileCoordinateHelper.SnapToNearestEdgeCenter(65f, 30f, TS);

		edge.Should().Be(new EdgePoint(1, 0, EdgeSide.W));
	}

	[Fact]
	public void SnapToNearestEdgeCenter_NearEastEdgeFromLeft_ReturnsEastEdgeOfLeftTile()
	{
		// cursor at (55, 30) — to the left of the same edge
		var (edge, _) = TileCoordinateHelper.SnapToNearestEdgeCenter(55f, 30f, TS);

		edge.Should().Be(new EdgePoint(0, 0, EdgeSide.E));
	}

	// ── DistanceToNearestVertex ───────────────────────────────────────────────

	[Fact]
	public void DistanceToNearestVertex_AtExactVertex_ReturnsZero()
	{
		var dist = TileCoordinateHelper.DistanceToNearestVertex(120f, 60f, TS);

		dist.Should().BeApproximately(0f, 0.01f);
	}

	[Fact]
	public void DistanceToNearestVertex_AtEdgeCenter_ReturnsTileSizeOverTwo()
	{
		// N edge center of tile (1,1) = pixel (90, 60); nearest vertex is at (60,60) or (120,60) = 30px away
		var dist = TileCoordinateHelper.DistanceToNearestVertex(90f, 60f, TS);

		dist.Should().BeApproximately(TS / 2f, 0.01f);
	}
}
