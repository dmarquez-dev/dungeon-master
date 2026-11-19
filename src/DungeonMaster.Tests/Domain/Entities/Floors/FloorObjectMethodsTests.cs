using DungeonMaster.Domain.Entities;
using DungeonMaster.Domain.Entities.Dungeons;
using DungeonMaster.Domain.Entities.Floors;
using DungeonMaster.Domain.ValueObjects;
using FluentAssertions;

namespace DungeonMaster.Tests.Domain.Entities.Floors;

public class FloorObjectMethodsTests
{
	private static Floor CreateFloor()
	{
		var dungeon = new Dungeon("Test");
		return dungeon.AddFloor(null);
	}

	[Fact]
	public void AddObject_ReturnsPlacedObjectWithCorrectProperties()
	{
		// Arrange
		var floor = CreateFloor();
		var pos = new CanvasPoint(80f, 120f);

		// Act
		var addedObject = floor.AddObject(
			ObjectKind.Chair,
			pos,
			ObjectRotation.None,
			ObjectScale.Default);

		// Assert
		addedObject.Kind.Should().Be(ObjectKind.Chair);
		addedObject.Position.Should().Be(pos);
		addedObject.Rotation.Should().Be(ObjectRotation.None);
		addedObject.FloorId.Should().Be(floor.Id);
		floor.Objects.Should().ContainSingle(placedObject => placedObject.Id == addedObject.Id);
	}

	[Fact]
	public void AddObject_SucceedsOnFloorWithNoAreas()
	{
		// Arrange
		var floor = CreateFloor();

		// Act
		var addedObject = floor.AddObject(
			ObjectKind.Barrel,
			new CanvasPoint(10f, 10f),
			ObjectRotation.None,
			ObjectScale.Default);

		// Assert
		floor.Areas.Should().BeEmpty();
		floor.Objects.Should().ContainSingle(placedObject => placedObject.Id == addedObject.Id);
	}

	[Fact]
	public void RemoveObject_RemovesById()
	{
		// Arrange
		var floor = CreateFloor();
		var addedObject = floor.AddObject(
			ObjectKind.Barrel,
			new CanvasPoint(40f, 40f),
			ObjectRotation.None,
			ObjectScale.Default);

		// Act
		var removed = floor.RemoveObject(addedObject.Id);

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
		var addedObject = floor.AddObject(
			ObjectKind.Chair,
			new CanvasPoint(0f, 0f),
			ObjectRotation.None,
			ObjectScale.Default);
		var newPos = new CanvasPoint(200f, 300f);

		// Act
		var updated = floor.UpdateObject(
			addedObject.Id,
			newPos,
			ObjectRotation.Clockwise90,
			2.0f);

		// Assert
		updated.Should().Be(MutationOutcome.Success);
		addedObject.Position.Should().Be(newPos);
		addedObject.Rotation.Should().Be(ObjectRotation.Clockwise90);
		addedObject.Scale.Should().Be(2.0f);
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
		var dungeon = new Dungeon("Test");
		var floor = dungeon.AddFloor(null);
		var area = dungeon.AddArea(floor.Id, "Chapel", null)!;
		var addedObject = floor.AddObject(
			ObjectKind.Chair,
			new CanvasPoint(50f, 50f),
			ObjectRotation.None,
			ObjectScale.Default);

		// Act
		dungeon.RemoveArea(floor.Id, area.Id);

		// Assert
		floor.Areas.Should().BeEmpty();
		floor.Objects.Should().ContainSingle(placedObject => placedObject.Id == addedObject.Id);
	}
}
