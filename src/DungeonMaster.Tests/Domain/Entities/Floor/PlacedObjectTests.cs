using DungeonMaster.Domain.Entities.Floors;
using DungeonMaster.Domain.ValueObjects;
using FluentAssertions;

namespace DungeonMaster.Tests.Domain.Entities.Floor;

public class PlacedObjectTests
{
	[Fact]
	public void Constructor_SetsAllProperties()
	{
		// Arrange
		var floorId = Guid.NewGuid();
		var pos = new CanvasPoint(120f, 200f);

		// Act
		var obj = new PlacedObject(
			floorId,
			ObjectKind.DoorWood,
			pos,
			ObjectRotation.Clockwise90,
			ObjectScale.Default);

		// Assert
		obj.Id.Should().NotBeEmpty();
		obj.FloorId.Should().Be(floorId);
		obj.Kind.Should().Be(ObjectKind.DoorWood);
		obj.Position.Should().Be(pos);
		obj.Rotation.Should().Be(ObjectRotation.Clockwise90);
		obj.Scale.Should().BeApproximately(ObjectScale.Default, 0.001f);
	}

	[Fact]
	public void Constructor_WithCustomScale_SetsScale()
	{
		// Act
		var obj = new PlacedObject(
			Guid.NewGuid(),
			ObjectKind.Chair,
			new CanvasPoint(0f, 0f),
			ObjectRotation.None,
			scale: 2.0f);

		// Assert
		obj.Scale.Should().BeApproximately(2.0f, 0.001f);
	}

	[Fact]
	public void Update_ChangesPositionRotationAndScale()
	{
		// Arrange
		var obj = new PlacedObject(
			Guid.NewGuid(),
			ObjectKind.Chair,
			new CanvasPoint(40f, 40f),
			ObjectRotation.None,
			ObjectScale.Default);

		var newPos = new CanvasPoint(160f, 80f);

		// Act
		obj.Update(newPos, ObjectRotation.Clockwise180, 1.5f);

		// Assert
		obj.Position.Should().Be(newPos);
		obj.Rotation.Should().Be(ObjectRotation.Clockwise180);
		obj.Scale.Should().BeApproximately(1.5f, 0.001f);
	}

	[Theory]
	[InlineData(float.NaN)]
	[InlineData(float.PositiveInfinity)]
	[InlineData(float.NegativeInfinity)]
	[InlineData(0.1f)]   // below Min
	[InlineData(5.0f)]   // above Max
	public void Constructor_InvalidScale_Throws(float invalidScale)
	{
		// Act
		var act = () => new PlacedObject(
			Guid.NewGuid(),
			ObjectKind.Chair,
			new CanvasPoint(0f, 0f),
			ObjectRotation.None,
			invalidScale);

		// Assert
		var ex = act.Should().Throw<ArgumentOutOfRangeException>().Which;

		ex.ParamName.Should().Be("scale");
		ex.Message.Should().StartWith($"Scale must be between {ObjectScale.Min} and {ObjectScale.Max}.");
	}

	[Fact]
	public void Constructor_InvalidScale_ReportsRejectedValue()
	{
		// Act
		var act = () => new PlacedObject(
			Guid.NewGuid(),
			ObjectKind.Chair,
			new CanvasPoint(0f, 0f),
			ObjectRotation.None,
			5.0f);

		// Assert
		var ex = act.Should().Throw<ArgumentOutOfRangeException>().Which;

		ex.ParamName.Should().Be("scale");
		ex.ActualValue.Should().Be(5.0f);
		ex.Message.Should().StartWith($"Scale must be between {ObjectScale.Min} and {ObjectScale.Max}.");
	}

	[Theory]
	[InlineData(float.NaN)]
	[InlineData(float.PositiveInfinity)]
	[InlineData(float.NegativeInfinity)]
	[InlineData(0.1f)]   // below Min
	[InlineData(5.0f)]   // above Max
	public void Update_InvalidScale_Throws(float invalidScale)
	{
		// Arrange
		var obj = new PlacedObject(
			Guid.NewGuid(),
			ObjectKind.Chair,
			new CanvasPoint(0f, 0f),
			ObjectRotation.None,
			ObjectScale.Default);

		// Act
		var act = () => obj.Update(
			new CanvasPoint(10f, 10f),
			ObjectRotation.None,
			invalidScale);

		// Assert
		var ex = act.Should().Throw<ArgumentOutOfRangeException>().Which;

		ex.ParamName.Should().Be("newScale");
		ex.Message.Should().StartWith($"Scale must be between {ObjectScale.Min} and {ObjectScale.Max}.");
	}

	[Fact]
	public void Update_InvalidScale_ReportsRejectedValue()
	{
		// Arrange
		var obj = new PlacedObject(
			Guid.NewGuid(),
			ObjectKind.Chair,
			new CanvasPoint(0f, 0f),
			ObjectRotation.None,
			ObjectScale.Default);

		// Act
		var act = () => obj.Update(
			new CanvasPoint(10f, 10f),
			ObjectRotation.None,
			0.1f);

		// Assert
		var ex = act.Should().Throw<ArgumentOutOfRangeException>().Which;

		ex.ParamName.Should().Be("newScale");
		ex.ActualValue.Should().Be(0.1f);
	}
}
