using DungeonMaster.Domain.Entities.Area;
using DungeonMaster.Domain.ValueObjects;
using FluentAssertions;

namespace DungeonMaster.Tests.Domain.Entities.Area;

public class PlacedObjectTests
{
    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var areaId = Guid.NewGuid();
        var pos = new CanvasPoint(120f, 200f);

        var obj = new PlacedObject(
            areaId,
            ObjectKind.DoorWood,
            pos,
            ObjectRotation.CW90);

        obj.Id.Should().NotBeEmpty();
        obj.AreaId.Should().Be(areaId);
        obj.Kind.Should().Be(ObjectKind.DoorWood);
        obj.Position.Should().Be(pos);
        obj.Rotation.Should().Be(ObjectRotation.CW90);
    }

    [Fact]
    public void Update_ChangesPositionAndRotation()
    {
        var obj = new PlacedObject(
            Guid.NewGuid(),
            ObjectKind.Chair,
            new CanvasPoint(40f, 40f),
            ObjectRotation.None);

        var newPos = new CanvasPoint(160f, 80f);

        obj.Update(newPos, ObjectRotation.CW180);

        obj.Position.Should().Be(newPos);
        obj.Rotation.Should().Be(ObjectRotation.CW180);
    }
}
