using DungeonMaster.Domain.Entities.Area;
using DungeonMaster.Domain.ValueObjects;
using FluentAssertions;
using DomainArea = DungeonMaster.Domain.Entities.Area.Area;
using DomainDungeon = DungeonMaster.Domain.Entities.Dungeon.Dungeon;

namespace DungeonMaster.Tests.Domain.Entities.Area;

public class AreaObjectMethodsTests
{
    private static DomainArea CreateArea()
    {
        var dungeon = new DomainDungeon("Test");
        var floor = dungeon.AddFloor(null);
        return dungeon.AddArea(floor.Id, "A1", null);
    }

    [Fact]
    public void AddObject_ReturnsPlacedObjectWithCorrectProperties()
    {
        var area = CreateArea();
        var pos = new TilePoint(2, 3);

        var obj = area.AddObject(ObjectKind.Chair, pos, SnapAnchor.Center, ObjectRotation.None);

        obj.Kind.Should().Be(ObjectKind.Chair);
        obj.Position.Should().Be(pos);
        obj.Anchor.Should().Be(SnapAnchor.Center);
        obj.Rotation.Should().Be(ObjectRotation.None);
        area.Objects.Should().ContainSingle(o => o.Id == obj.Id);
    }

    [Fact]
    public void RemoveObject_RemovesById()
    {
        var area = CreateArea();
        var obj = area.AddObject(ObjectKind.Barrel, new TilePoint(1, 1), SnapAnchor.Center, ObjectRotation.None);

        area.RemoveObject(obj.Id);

        area.Objects.Should().BeEmpty();
    }

    [Fact]
    public void RemoveObject_UnknownId_DoesNotThrow()
    {
        var area = CreateArea();

        var act = () => area.RemoveObject(Guid.NewGuid());

        act.Should().NotThrow();
    }

    [Fact]
    public void UpdateObject_ChangesPositionAnchorAndRotation()
    {
        var area = CreateArea();
        var obj = area.AddObject(ObjectKind.Throne, new TilePoint(0, 0), SnapAnchor.Center, ObjectRotation.None);
        var newPos = new TilePoint(5, 5);

        area.UpdateObject(obj.Id, newPos, SnapAnchor.North, ObjectRotation.CW90);

        var updated = area.Objects.Single(o => o.Id == obj.Id);
        updated.Position.Should().Be(newPos);
        updated.Anchor.Should().Be(SnapAnchor.North);
        updated.Rotation.Should().Be(ObjectRotation.CW90);
    }
}
