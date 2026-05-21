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
        obj.Scale.Should().BeApproximately(ObjectScale.Default, 0.001f);
    }

    [Fact]
    public void Constructor_WithCustomScale_SetsScale()
    {
        var obj = new PlacedObject(
            Guid.NewGuid(),
            ObjectKind.Chair,
            new CanvasPoint(0f, 0f),
            ObjectRotation.None,
            scale: 2.0f);

        obj.Scale.Should().BeApproximately(2.0f, 0.001f);
    }

    [Fact]
    public void Update_ChangesPositionRotationAndScale()
    {
        var obj = new PlacedObject(
            Guid.NewGuid(),
            ObjectKind.Chair,
            new CanvasPoint(40f, 40f),
            ObjectRotation.None);

        var newPos = new CanvasPoint(160f, 80f);

        obj.Update(newPos, ObjectRotation.CW180, 1.5f);

        obj.Position.Should().Be(newPos);
        obj.Rotation.Should().Be(ObjectRotation.CW180);
        obj.Scale.Should().BeApproximately(1.5f, 0.001f);
    }

    [Fact]
    public void Update_WithDefaultScale_KeepsDefaultScale()
    {
        var obj = new PlacedObject(
            Guid.NewGuid(),
            ObjectKind.Chair,
            new CanvasPoint(40f, 40f),
            ObjectRotation.None);

        obj.Update(new CanvasPoint(10f, 10f), ObjectRotation.None);

        obj.Scale.Should().BeApproximately(ObjectScale.Default, 0.001f);
    }

    [Theory]
    [InlineData(float.NaN)]
    [InlineData(float.PositiveInfinity)]
    [InlineData(float.NegativeInfinity)]
    [InlineData(0.1f)]   // below Min
    [InlineData(5.0f)]   // above Max
    public void Constructor_InvalidScale_Throws(float invalidScale)
    {
        var act = () => new PlacedObject(
            Guid.NewGuid(),
            ObjectKind.Chair,
            new CanvasPoint(0f, 0f),
            ObjectRotation.None,
            invalidScale);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(float.NaN)]
    [InlineData(float.PositiveInfinity)]
    [InlineData(0.1f)]   // below Min
    [InlineData(5.0f)]   // above Max
    public void Update_InvalidScale_Throws(float invalidScale)
    {
        var obj = new PlacedObject(
            Guid.NewGuid(),
            ObjectKind.Chair,
            new CanvasPoint(0f, 0f),
            ObjectRotation.None);

        var act = () => obj.Update(
            new CanvasPoint(10f, 10f),
            ObjectRotation.None,
            invalidScale);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Update_TwoParam_PreservesCustomScale()
    {
        var obj = new PlacedObject(
            Guid.NewGuid(),
            ObjectKind.Chair,
            new CanvasPoint(0f, 0f),
            ObjectRotation.None,
            scale: 2.0f);

        obj.Update(
            new CanvasPoint(10f, 10f),
            ObjectRotation.CW90);

        obj.Scale.Should().BeApproximately(2.0f, 0.001f);
    }
}
