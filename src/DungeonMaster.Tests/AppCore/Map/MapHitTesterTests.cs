using DungeonMaster.App.Core.Map;
using DungeonMaster.Domain.Entities.Floors;
using DungeonMaster.Domain.ValueObjects;
using FluentAssertions;

using AreaItem = DungeonMaster.App.Core.Presentation.AreaItem;
using PlacedObjectItem = DungeonMaster.App.Core.Presentation.PlacedObjectItem;

namespace DungeonMaster.Tests.AppCore.Map;

public class MapHitTesterTests
{
	private const float TileSize = 10f;

	// A fixed identifier that never matches an area built by this fixture, so the "not the
	// previewed area" case is deterministic rather than relying on a fresh Guid.
	private static readonly Guid AnotherAreaId = new("0f8fad5b-d9cb-469f-a165-70867728950e");

	[Fact]
	public void HitTestArea_TwoContainingAreasOfDifferentSize_ReturnsTheSmallerRegardlessOfListOrder()
	{
		var big = RectangleArea("big", 0, 0, 10, 10);
		var small = RectangleArea("small", 2, 2, 6, 6);

		MapHitTester.HitTestArea([big, small], 40f, 40f, TileSize, null, null)
			.Should().Be(small);
		MapHitTester.HitTestArea([small, big], 40f, 40f, TileSize, null, null)
			.Should().Be(small);
	}

	[Fact]
	public void HitTestArea_TwoContainingAreasOfEqualPolygonArea_ReturnsTheOneEarlierInTheList()
	{
		var first = RectangleArea("first", 0, 0, 4, 4);
		var second = RectangleArea("second", 0, 0, 4, 4);

		MapHitTester.HitTestArea([first, second], 20f, 20f, TileSize, null, null)
			.Should().Be(first);
		MapHitTester.HitTestArea([second, first], 20f, 20f, TileSize, null, null)
			.Should().Be(second);
	}

	[Fact]
	public void HitTestArea_AreaWithEmptyOutline_IsSkippedInFavourOfTheContainingArea()
	{
		var empty = new AreaItem(Guid.NewGuid(), "empty", null, null, []);
		var containing = RectangleArea("containing", 0, 0, 4, 4);

		var result = MapHitTester.HitTestArea([empty, containing], 20f, 20f, TileSize, null, null);

		result.Should().Be(containing);
	}

	[Fact]
	public void HitTestArea_OnlyAreaHasAnEmptyOutline_ReturnsNull()
	{
		var empty = new AreaItem(Guid.NewGuid(), "empty", null, null, []);

		var result = MapHitTester.HitTestArea([empty], 20f, 20f, TileSize, null, null);

		result.Should().BeNull();
	}

	[Fact]
	public void HitTestArea_NoAreaContainsThePoint_ReturnsNull()
	{
		var area = RectangleArea("area", 0, 0, 4, 4);

		var result = MapHitTester.HitTestArea([area], 500f, 500f, TileSize, null, null);

		result.Should().BeNull();
	}

	[Fact]
	public void HitTestArea_DragPreviewIdMatches_HitsTheAreaAtItsTranslatedPositionOnly()
	{
		// The area spans pixels 0..40; the point at (65,15) is outside it. A delta of 3 tiles
		// moves the area to pixels 30..70, which is the only way that point can hit.
		var area = RectangleArea("dragged", 0, 0, 4, 4);

		MapHitTester.HitTestArea([area], 65f, 15f, TileSize, null, null)
			.Should().BeNull();
		MapHitTester.HitTestArea([area], 65f, 15f, TileSize, area.Id, new TilePoint(3, 0))
			.Should().Be(area);
	}

	[Fact]
	public void HitTestArea_DragPreviewIdMatchesAnotherArea_LeavesThisAreaUntranslated()
	{
		var area = RectangleArea("static", 0, 0, 4, 4);

		MapHitTester.HitTestArea([area], 65f, 15f, TileSize, AnotherAreaId, new TilePoint(3, 0))
			.Should().BeNull();
		MapHitTester.HitTestArea([area], 20f, 20f, TileSize, AnotherAreaId, new TilePoint(3, 0))
			.Should().Be(area);
	}

	[Fact]
	public void HitTestArea_NullAreas_ThrowsArgumentNullException()
	{
		var act = () => MapHitTester.HitTestArea(null!, 0f, 0f, TileSize, null, null);

		act.Should().Throw<ArgumentNullException>();
	}

	[Fact]
	public void HitTestArea_NullElementInAreas_ThrowsArgumentNullException()
	{
		IReadOnlyList<AreaItem> areas = [null!];

		var act = () => MapHitTester.HitTestArea(areas, 0f, 0f, TileSize, null, null);

		act.Should().Throw<ArgumentNullException>();
	}

	[Theory]
	[InlineData(0f)]
	[InlineData(-1f)]
	public void HitTestArea_TileSizeNotGreaterThanZero_ThrowsArgumentOutOfRangeException(float tileSize)
	{
		var areas = new[] { RectangleArea("area", 0, 0, 4, 4) };

		var act = () => MapHitTester.HitTestArea(areas, 0f, 0f, tileSize, null, null);

		act.Should().Throw<ArgumentOutOfRangeException>();
	}

	[Theory]
	[InlineData(90f, 100f)]
	[InlineData(110f, 100f)]
	[InlineData(100f, 90f)]
	[InlineData(100f, 110f)]
	[InlineData(90f, 90f)]
	[InlineData(110f, 110f)]
	public void HitTestObject_PointExactlyOnABoundingBoxEdge_ReturnsTheObject(float canvasX, float canvasY)
	{
		// A 2x2 tile object at (100,100) with a tile size of 10 spans 90..110 on both axes.
		var placed = PlacedObject(ObjectKind.Chest, 100f, 100f);

		var result = MapHitTester.HitTestObject([placed], canvasX, canvasY, TileSize, SizeOf);

		result.Should().Be(placed);
	}

	[Theory]
	[InlineData(89f, 100f)]
	[InlineData(111f, 100f)]
	[InlineData(100f, 89f)]
	[InlineData(100f, 111f)]
	public void HitTestObject_PointJustOutsideTheHalfExtents_ReturnsNull(float canvasX, float canvasY)
	{
		// Half extents are taken about Position, which is the bounding-box centre, so a point
		// one pixel beyond 90..110 misses.
		var placed = PlacedObject(ObjectKind.Chest, 100f, 100f);

		var result = MapHitTester.HitTestObject([placed], canvasX, canvasY, TileSize, SizeOf);

		result.Should().BeNull();
	}

	[Fact]
	public void HitTestObject_PositionIsTheCentreNotTheTopLeft_SoThePointAboveAndLeftOfItStillHits()
	{
		var placed = PlacedObject(ObjectKind.Chest, 100f, 100f);

		MapHitTester.HitTestObject([placed], 92f, 92f, TileSize, SizeOf).Should().Be(placed);
		MapHitTester.HitTestObject([placed], 108f, 108f, TileSize, SizeOf).Should().Be(placed);
	}

	[Fact]
	public void HitTestObject_TwoContainingObjects_ReturnsTheOneWithTheSmallerUnscaledTileArea()
	{
		var large = PlacedObject(ObjectKind.Bed, 100f, 100f);
		var small = PlacedObject(ObjectKind.Chest, 100f, 100f);

		MapHitTester.HitTestObject([large, small], 100f, 100f, TileSize, SizeOf)
			.Should().Be(small);
		MapHitTester.HitTestObject([small, large], 100f, 100f, TileSize, SizeOf)
			.Should().Be(small);
	}

	[Fact]
	public void HitTestObject_LargerTileObjectShrunkByScale_StillLosesToTheSmallerTileObject()
	{
		// Scale and Rotation are deliberately not consulted. Were Scale consulted, the 4x4 tile
		// object at Scale 0.1 would rank smaller than the 2x2 tile object at Scale 10 and win.
		var largeTilesTinyScale = PlacedObject(ObjectKind.Bed, 100f, 100f, scale: 0.1f, rotation: ObjectRotation.CW90);
		var smallTilesHugeScale = PlacedObject(ObjectKind.Chest, 100f, 100f, scale: 10f, rotation: ObjectRotation.CW270);

		var result = MapHitTester.HitTestObject(
			[largeTilesTinyScale, smallTilesHugeScale],
			100f,
			100f,
			TileSize,
			SizeOf);

		result.Should().Be(smallTilesHugeScale);
	}

	[Fact]
	public void HitTestObject_TwoContainingObjectsOfEqualTileArea_ReturnsTheOneEarlierInTheList()
	{
		var first = PlacedObject(ObjectKind.Chest, 100f, 100f);
		var second = PlacedObject(ObjectKind.Chest, 100f, 100f);

		MapHitTester.HitTestObject([first, second], 100f, 100f, TileSize, SizeOf)
			.Should().Be(first);
		MapHitTester.HitTestObject([second, first], 100f, 100f, TileSize, SizeOf)
			.Should().Be(second);
	}

	[Fact]
	public void HitTestObject_NoObjectContainsThePoint_ReturnsNull()
	{
		var placed = PlacedObject(ObjectKind.Chest, 100f, 100f);

		var result = MapHitTester.HitTestObject([placed], 500f, 500f, TileSize, SizeOf);

		result.Should().BeNull();
	}

	[Fact]
	public void HitTestObject_EmptyObjectList_ReturnsNull()
	{
		MapHitTester.HitTestObject([], 100f, 100f, TileSize, SizeOf).Should().BeNull();
	}

	[Fact]
	public void HitTestObject_NullObjects_ThrowsArgumentNullException()
	{
		var act = () => MapHitTester.HitTestObject(null!, 0f, 0f, TileSize, SizeOf);

		act.Should().Throw<ArgumentNullException>();
	}

	[Fact]
	public void HitTestObject_NullElementInObjects_ThrowsArgumentNullException()
	{
		IReadOnlyList<PlacedObjectItem> objects = [null!];

		var act = () => MapHitTester.HitTestObject(objects, 0f, 0f, TileSize, SizeOf);

		act.Should().Throw<ArgumentNullException>();
	}

	[Fact]
	public void HitTestObject_NullObjectSizeDelegate_ThrowsArgumentNullException()
	{
		var objects = new[] { PlacedObject(ObjectKind.Chest, 100f, 100f) };

		var act = () => MapHitTester.HitTestObject(objects, 0f, 0f, TileSize, null!);

		act.Should().Throw<ArgumentNullException>();
	}

	[Theory]
	[InlineData(0f)]
	[InlineData(-1f)]
	public void HitTestObject_TileSizeNotGreaterThanZero_ThrowsArgumentOutOfRangeException(float tileSize)
	{
		var objects = new[] { PlacedObject(ObjectKind.Chest, 100f, 100f) };

		var act = () => MapHitTester.HitTestObject(objects, 0f, 0f, tileSize, SizeOf);

		act.Should().Throw<ArgumentOutOfRangeException>();
	}

	// Chest stands for a 2x2 tile object and Bed for a 4x4 one, so their unscaled tile areas
	// are 4 and 16.
	private static (float W, float H) SizeOf(ObjectKind kind) =>
		kind switch
		{
			ObjectKind.Chest => (2f, 2f),
			ObjectKind.Bed => (4f, 4f),
			_ => (1f, 1f),
		};

	private static PlacedObjectItem PlacedObject(
		ObjectKind kind,
		float x,
		float y,
		float scale = 1f,
		ObjectRotation rotation = ObjectRotation.None) =>
		new(Guid.NewGuid(), kind, new CanvasPoint(x, y), rotation, scale);

	private static AreaItem RectangleArea(string name, int left, int top, int right, int bottom) =>
		new(
			Guid.NewGuid(),
			name,
			null,
			null,
			[
				Segment(left, top),
				Segment(right, top),
				Segment(right, bottom),
				Segment(left, bottom),
			]);

	private static PathSegment Segment(int col, int row) =>
		new(PathSegmentKind.Line, new TilePoint(col, row), null, null);
}
