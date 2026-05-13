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
        var pos = new TilePoint(3, 5);

        var obj = new PlacedObject(areaId, ObjectKind.DoorWood, pos, SnapAnchor.East, ObjectRotation.CW90);

        obj.Id.Should().NotBeEmpty();
        obj.AreaId.Should().Be(areaId);
        obj.Kind.Should().Be(ObjectKind.DoorWood);
        obj.Position.Should().Be(pos);
        obj.Anchor.Should().Be(SnapAnchor.East);
        obj.Rotation.Should().Be(ObjectRotation.CW90);
    }

    [Fact]
    public void Update_ChangesPositionAnchorAndRotation()
    {
        var obj = new PlacedObject(Guid.NewGuid(), ObjectKind.Chair, new TilePoint(1, 1), SnapAnchor.Center, ObjectRotation.None);
        var newPos = new TilePoint(4, 2);

        obj.Update(newPos, SnapAnchor.North, ObjectRotation.CW180);

        obj.Position.Should().Be(newPos);
        obj.Anchor.Should().Be(SnapAnchor.North);
        obj.Rotation.Should().Be(ObjectRotation.CW180);
    }
}
