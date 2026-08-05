using DungeonMaster.Domain.Entities.Area;
using DungeonMaster.Domain.ValueObjects;
using FluentAssertions;
using DomainDungeon = DungeonMaster.Domain.Entities.Dungeon.Dungeon;

namespace DungeonMaster.Tests.Domain.Entities.Area;

public class AreaOutlineTests
{
    [Fact]
    public void UpdateAreaOutline_SetsOutline()
    {
        var dungeon = new DomainDungeon("Test");
        var floor = dungeon.AddFloor(null);
        var area = dungeon.AddArea(floor.Id, "Room", null);

        var outline = new List<PathSegment>
        {
            new(PathSegmentKind.Line, new TilePoint(4, 3), null, null),
            new(PathSegmentKind.Line, new TilePoint(4, 6), null, null),
            new(PathSegmentKind.Line, new TilePoint(2, 6), null, null),
            new(PathSegmentKind.Line, new TilePoint(2, 3), null, null),
        };

        dungeon.UpdateAreaOutline(floor.Id, area.Id, outline);

        var updated = dungeon.Floors
            .First(dungeonFloor => dungeonFloor.Id == floor.Id).Areas
            .First(dungeonArea => dungeonArea.Id == area.Id);

        updated.Outline.Should().HaveCount(4);
    }

    [Fact]
    public void UpdateAreaOutline_WithUnknownFloor_IsNoOp()
    {
        var dungeon = new DomainDungeon("Test");

        var act = () => dungeon.UpdateAreaOutline(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new List<PathSegment>());

        act.Should().NotThrow();
    }

    [Fact]
    public void UpdateAreaOutline_WithUnknownArea_IsNoOp()
    {
        var dungeon = new DomainDungeon("Test");
        var floor = dungeon.AddFloor(null);

        var act = () => dungeon.UpdateAreaOutline(
            floor.Id,
            Guid.NewGuid(),
            new List<PathSegment>());

        act.Should().NotThrow();
    }

    [Fact]
    public void SetOutline_DoesNotMoveFloorObjects()
    {
        var dungeon = new DomainDungeon("Test");
        var floor = dungeon.AddFloor(null);
        var area = dungeon.AddArea(floor.Id, "Chapel", null);
        var obj = floor.AddObject(
            ObjectKind.Chair,
            new CanvasPoint(100f, 100f),
            ObjectRotation.None);

        dungeon.UpdateAreaOutline(
            floor.Id,
            area.Id,
            [
                new PathSegment(PathSegmentKind.Line, new TilePoint(5, 0), null, null),
                new PathSegment(PathSegmentKind.Line, new TilePoint(5, 5), null, null),
                new PathSegment(PathSegmentKind.Line, new TilePoint(0, 0), null, null),
            ]);

        obj.Position.Should().Be(new CanvasPoint(100f, 100f));
        area.Outline.Should().HaveCount(3);
    }
}
