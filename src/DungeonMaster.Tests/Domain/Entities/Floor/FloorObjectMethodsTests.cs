using DungeonMaster.Domain.Entities;
using DungeonMaster.Domain.Entities.Floors;
using DungeonMaster.Domain.ValueObjects;
using FluentAssertions;
using DomainDungeon = DungeonMaster.Domain.Entities.Dungeons.Dungeon;
using DomainFloor = DungeonMaster.Domain.Entities.Floors.Floor;

namespace DungeonMaster.Tests.Domain.Entities.Floor;

public class FloorObjectMethodsTests
{
	private static DomainFloor CreateFloor()
	{
		var dungeon = new DomainDungeon("Test");
		return dungeon.AddFloor(null);
	}

	[Fact]
	public void AddObject_ReturnsPlacedObjectWithCorrectProperties()
	{
		// Arrange
		var floor = CreateFloor();
		var pos = new CanvasPoint(80f, 120f);

		// Act
		var obj = floor.AddObject(
			ObjectKind.Chair,
			pos,
			ObjectRotation.None,
			ObjectScale.Default);

		// Assert
		obj.Kind.Should().Be(ObjectKind.Chair);
		obj.Position.Should().Be(pos);
		obj.Rotation.Should().Be(ObjectRotation.None);
		obj.FloorId.Should().Be(floor.Id);
		floor.Objects.Should().ContainSingle(placedObject => placedObject.Id == obj.Id);
	}

	[Fact]
	public void AddObject_SucceedsOnFloorWithNoAreas()
	{
		// Arrange
		var floor = CreateFloor();

		// Act
		var obj = floor.AddObject(
			ObjectKind.Barrel,
			new CanvasPoint(10f, 10f),
			ObjectRotation.None,
			ObjectScale.Default);

		// Assert
		floor.Areas.Should().BeEmpty();
		floor.Objects.Should().ContainSingle(placedObject => placedObject.Id == obj.Id);
	}

	[Fact]
	public void RemoveObject_RemovesById()
	{
		// Arrange
		var floor = CreateFloor();
		var obj = floor.AddObject(
			ObjectKind.Barrel,
			new CanvasPoint(40f, 40f),
			ObjectRotation.None,
			ObjectScale.Default);

		// Act
		var removed = floor.RemoveObject(obj.Id);

		// Assert
		removed.Should().Be(MutationOutcome.Success);
		floor.Objects.Should().BeEmpty();
	}

	[Fact]
	public void RemoveObject_UnknownId_ReturnsNotFound()
	{
		// Arrange
		var floor = CreateFloor();

		// Act
		var removed = floor.RemoveObject(Guid.NewGuid());

		// Assert
		removed.Should().Be(MutationOutcome.NotFound);
	}

	[Fact]
	public void UpdateObject_AppliesPositionRotationAndScale()
	{
		// Arrange
		var floor = CreateFloor();
		var obj = floor.AddObject(
			ObjectKind.Chair,
			new CanvasPoint(0f, 0f),
			ObjectRotation.None,
			ObjectScale.Default);
		var newPos = new CanvasPoint(200f, 300f);

		// Act
		var updated = floor.UpdateObject(
			obj.Id,
			newPos,
			ObjectRotation.CW90,
			2.0f);

		// Assert
		updated.Should().Be(MutationOutcome.Success);
		obj.Position.Should().Be(newPos);
		obj.Rotation.Should().Be(ObjectRotation.CW90);
		obj.Scale.Should().Be(2.0f);
	}

	[Fact]
	public void UpdateObject_UnknownId_ReturnsNotFound()
	{
		// Arrange
		var floor = CreateFloor();

		// Act
		var updated = floor.UpdateObject(
			Guid.NewGuid(),
			new CanvasPoint(1f, 1f),
			ObjectRotation.None,
			1.0f);

		// Assert
		updated.Should().Be(MutationOutcome.NotFound);
	}

	[Fact]
	public void RemoveArea_LeavesObjectsInPlace()
	{
		// Arrange
		var dungeon = new DomainDungeon("Test");
		var floor = dungeon.AddFloor(null);
		var area = dungeon.AddArea(floor.Id, "Chapel", null)!;
		var obj = floor.AddObject(
			ObjectKind.Chair,
			new CanvasPoint(50f, 50f),
			ObjectRotation.None,
			ObjectScale.Default);

		// Act
		dungeon.RemoveArea(floor.Id, area.Id);

		// Assert
		floor.Areas.Should().BeEmpty();
		floor.Objects.Should().ContainSingle(placedObject => placedObject.Id == obj.Id);
	}
}
