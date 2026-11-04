using DungeonMaster.Domain.Entities.Floors;
using DungeonMaster.Domain.ValueObjects;
using FluentAssertions;
using DomainDungeon = DungeonMaster.Domain.Entities.Dungeons.Dungeon;

namespace DungeonMaster.Tests.Domain.Entities.Area;

public class AreaOutlineTests
{
	[Fact]
	public void UpdateAreaOutline_SetsOutline()
	{
		// Arrange
		var dungeon = new DomainDungeon("Test");
		var floor = dungeon.AddFloor(null);
		var area = dungeon.AddArea(floor.Id, "Room", null)!;

		var outline = new List<PathSegment>
		{
			new(PathSegmentKind.Line, new TilePoint(4, 3), null, null),
			new(PathSegmentKind.Line, new TilePoint(4, 6), null, null),
			new(PathSegmentKind.Line, new TilePoint(2, 6), null, null),
			new(PathSegmentKind.Line, new TilePoint(2, 3), null, null),
		};

		// Act
		dungeon.UpdateAreaOutline(floor.Id, area.Id, outline);

		// Assert
		var updated = dungeon.Floors
			.First(dungeonFloor => dungeonFloor.Id == floor.Id).Areas
			.First(dungeonArea => dungeonArea.Id == area.Id);

		updated.Outline.Should().HaveCount(4);
	}

	[Fact]
	public void UpdateAreaOutline_WithUnknownFloor_IsNoOp()
	{
		// Arrange
		var dungeon = new DomainDungeon("Test");

		// Act
		var act = () => dungeon.UpdateAreaOutline(
			Guid.NewGuid(),
			Guid.NewGuid(),
			new List<PathSegment>());

		// Assert
		act.Should().NotThrow();
	}

	[Fact]
	public void UpdateAreaOutline_WithUnknownArea_IsNoOp()
	{
		// Arrange
		var dungeon = new DomainDungeon("Test");
		var floor = dungeon.AddFloor(null);

		// Act
		var act = () => dungeon.UpdateAreaOutline(
			floor.Id,
			Guid.NewGuid(),
			new List<PathSegment>());

		// Assert
		act.Should().NotThrow();
	}

	[Fact]
	public void SetOutline_DoesNotMoveFloorObjects()
	{
		// Arrange
		var dungeon = new DomainDungeon("Test");
		var floor = dungeon.AddFloor(null);
		var area = dungeon.AddArea(floor.Id, "Chapel", null)!;
		var obj = floor.AddObject(
			ObjectKind.Chair,
			new CanvasPoint(100f, 100f),
			ObjectRotation.None,
			ObjectScale.Default);

		// Act
		dungeon.UpdateAreaOutline(
			floor.Id,
			area.Id,
			[
				new PathSegment(PathSegmentKind.Line, new TilePoint(5, 0), null, null),
				new PathSegment(PathSegmentKind.Line, new TilePoint(5, 5), null, null),
				new PathSegment(PathSegmentKind.Line, new TilePoint(0, 0), null, null),
			]);

		// Assert
		obj.Position.Should().Be(new CanvasPoint(100f, 100f));
		area.Outline.Should().HaveCount(3);
	}

	[Fact]
	public void UpdateAreaOutline_WithNullOutline_Throws()
	{
		// Arrange
		var dungeon = new DomainDungeon("Test");
		var floor = dungeon.AddFloor(null);
		var area = dungeon.AddArea(floor.Id, "Room", null)!;

		// Act
		var act = () => dungeon.UpdateAreaOutline(
			floor.Id,
			area.Id,
			null!);

		// Assert
		act.Should().ThrowExactly<ArgumentNullException>()
			.WithParameterName("outline");
	}

	[Fact]
	public void UpdateAreaOutline_WithEmptyOutline_OnAKnownArea_Throws()
	{
		// Arrange
		var dungeon = new DomainDungeon("Test");
		var floor = dungeon.AddFloor(null);
		var area = dungeon.AddArea(floor.Id, "Room", null)!;

		// Act
		var act = () => dungeon.UpdateAreaOutline(
			floor.Id,
			area.Id,
			new List<PathSegment>());

		// Assert
		act.Should().ThrowExactly<ArgumentException>()
			.WithParameterName("outline");
	}

	[Fact]
	public void UpdateAreaOutline_WithTwoSegments_Throws_AndLeavesTheOutlineUntouched()
	{
		// Arrange
		var dungeon = new DomainDungeon("Test");
		var floor = dungeon.AddFloor(null);
		var area = dungeon.AddArea(floor.Id, "Room", null)!;

		var original = new List<PathSegment>
		{
			new(PathSegmentKind.Line, new TilePoint(0, 0), null, null),
			new(PathSegmentKind.Line, new TilePoint(3, 0), null, null),
			new(PathSegmentKind.Line, new TilePoint(3, 3), null, null),
		};

		dungeon.UpdateAreaOutline(floor.Id, area.Id, original);

		// Act
		var act = () => dungeon.UpdateAreaOutline(
			floor.Id,
			area.Id,
			[
				new PathSegment(PathSegmentKind.Line, new TilePoint(7, 7), null, null),
				new PathSegment(PathSegmentKind.Line, new TilePoint(8, 8), null, null),
			]);

		// Assert
		act.Should().ThrowExactly<ArgumentException>()
			.WithParameterName("outline");

		area.Outline.Should().Equal(original);
	}

	[Fact]
	public void UpdateAreaOutline_WithThreeSegments_Succeeds()
	{
		// Arrange
		var dungeon = new DomainDungeon("Test");
		var floor = dungeon.AddFloor(null);
		var area = dungeon.AddArea(floor.Id, "Room", null)!;

		var outline = new List<PathSegment>
		{
			new(PathSegmentKind.Line, new TilePoint(1, 1), null, null),
			new(PathSegmentKind.Line, new TilePoint(5, 1), null, null),
			new(PathSegmentKind.Line, new TilePoint(5, 5), null, null),
		};

		// Act
		var act = () => dungeon.UpdateAreaOutline(floor.Id, area.Id, outline);

		// Assert
		act.Should().NotThrow();
		area.Outline.Should().Equal(outline);
	}
}
