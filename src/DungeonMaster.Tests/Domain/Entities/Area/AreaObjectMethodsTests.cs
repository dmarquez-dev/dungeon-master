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
        return dungeon.AddArea(
            floor.Id,
            "A1",
            null);
    }

    [Fact]
    public void AddObject_ReturnsPlacedObjectWithCorrectProperties()
    {
        var area = CreateArea();
        var pos = new CanvasPoint(80f, 120f);

        var obj = area.AddObject(
            ObjectKind.Chair,
            pos,
            ObjectRotation.None);

        obj.Kind.Should().Be(ObjectKind.Chair);
        obj.Position.Should().Be(pos);
        obj.Rotation.Should().Be(ObjectRotation.None);
        area.Objects.Should().ContainSingle(placedObject => placedObject.Id == obj.Id);
    }

    [Fact]
    public void RemoveObject_RemovesById()
    {
        var area = CreateArea();
        var obj = area.AddObject(
            ObjectKind.Barrel,
            new CanvasPoint(40f, 40f),
            ObjectRotation.None);

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
    public void UpdateObject_ChangesPositionAndRotation()
    {
        var area = CreateArea();
        var obj = area.AddObject(
            ObjectKind.Throne,
            new CanvasPoint(0f, 0f),
            ObjectRotation.None);

        var newPos = new CanvasPoint(200f, 200f);

        area.UpdateObject(
            obj.Id,
            newPos,
            ObjectRotation.CW90);

        var updated = area.Objects.Single(placedObject => placedObject.Id == obj.Id);
        updated.Position.Should().Be(newPos);
        updated.Rotation.Should().Be(ObjectRotation.CW90);
    }
}
