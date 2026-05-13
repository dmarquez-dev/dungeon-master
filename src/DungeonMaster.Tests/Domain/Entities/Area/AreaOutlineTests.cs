using DungeonMaster.Domain.ValueObjects;
using FluentAssertions;
using DomainDungeon = DungeonMaster.Domain.Entities.Dungeon.Dungeon;

namespace DungeonMaster.Tests.Domain.Entities.Area;

public class AreaOutlineTests
{
    [Fact]
    public void UpdateAreaOutline_SetsOriginAndOutline()
    {
        var dungeon = new DomainDungeon("Test");
        var floor = dungeon.AddFloor(null);
        var area = dungeon.AddArea(floor.Id, "Room", null);

        var origin = new TilePoint(2, 3);
        var outline = new List<PathSegment>
        {
            new(PathSegmentKind.Line, new TilePoint(4, 3), null, null),
            new(PathSegmentKind.Line, new TilePoint(4, 6), null, null),
            new(PathSegmentKind.Line, new TilePoint(2, 6), null, null),
            new(PathSegmentKind.Line, new TilePoint(2, 3), null, null),
        };

        dungeon.UpdateAreaOutline(floor.Id, area.Id, origin, outline);

        var updated = dungeon.Floors
            .First(dungeonFloor => dungeonFloor.Id == floor.Id).Areas
            .First(dungeonArea => dungeonArea.Id == area.Id);

        updated.Origin.Should().Be(origin);
        updated.Outline.Should().HaveCount(4);
    }

    [Fact]
    public void UpdateAreaOutline_WithUnknownFloor_IsNoOp()
    {
        var dungeon = new DomainDungeon("Test");

        var act = () => dungeon.UpdateAreaOutline(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new TilePoint(0, 0),
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
            new TilePoint(0, 0),
            new List<PathSegment>());

        act.Should().NotThrow();
    }
}
