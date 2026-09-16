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
		var floor = CreateFloor();
		var pos = new CanvasPoint(80f, 120f);

		var obj = floor.AddObject(
			ObjectKind.Chair,
			pos,
			ObjectRotation.None);

		obj.Kind.Should().Be(ObjectKind.Chair);
		obj.Position.Should().Be(pos);
		obj.Rotation.Should().Be(ObjectRotation.None);
		obj.FloorId.Should().Be(floor.Id);
		floor.Objects.Should().ContainSingle(placedObject => placedObject.Id == obj.Id);
	}

	[Fact]
	public void AddObject_SucceedsOnFloorWithNoAreas()
	{
		var floor = CreateFloor();

		var obj = floor.AddObject(
			ObjectKind.Barrel,
			new CanvasPoint(10f, 10f),
			ObjectRotation.None);

		floor.Areas.Should().BeEmpty();
		floor.Objects.Should().ContainSingle(placedObject => placedObject.Id == obj.Id);
	}

	[Fact]
	public void RemoveObject_RemovesById()
	{
		var floor = CreateFloor();
		var obj = floor.AddObject(
			ObjectKind.Barrel,
			new CanvasPoint(40f, 40f),
			ObjectRotation.None);

		var removed = floor.RemoveObject(obj.Id);

		removed.Should().Be(MutationOutcome.Success);
		floor.Objects.Should().BeEmpty();
	}

	[Fact]
	public void RemoveObject_UnknownId_ReturnsNotFound()
	{
		var floor = CreateFloor();

		var removed = floor.RemoveObject(Guid.NewGuid());

		removed.Should().Be(MutationOutcome.NotFound);
	}

	[Fact]
	public void UpdateObject_AppliesPositionRotationAndScale()
	{
		var floor = CreateFloor();
		var obj = floor.AddObject(
			ObjectKind.Chair,
			new CanvasPoint(0f, 0f),
			ObjectRotation.None);
		var newPos = new CanvasPoint(200f, 300f);

		var updated = floor.UpdateObject(
			obj.Id,
			newPos,
			ObjectRotation.CW90,
			2.0f);

		updated.Should().Be(MutationOutcome.Success);
		obj.Position.Should().Be(newPos);
		obj.Rotation.Should().Be(ObjectRotation.CW90);
		obj.Scale.Should().Be(2.0f);
	}

	[Fact]
	public void UpdateObject_UnknownId_ReturnsNotFound()
	{
		var floor = CreateFloor();

		var updated = floor.UpdateObject(
			Guid.NewGuid(),
			new CanvasPoint(1f, 1f),
			ObjectRotation.None,
			1.0f);

		updated.Should().Be(MutationOutcome.NotFound);
	}

	[Fact]
	public void RemoveArea_LeavesObjectsInPlace()
	{
		var dungeon = new DomainDungeon("Test");
		var floor = dungeon.AddFloor(null);
		var area = dungeon.AddArea(floor.Id, "Chapel", null)!;
		var obj = floor.AddObject(
			ObjectKind.Chair,
			new CanvasPoint(50f, 50f),
			ObjectRotation.None);

		dungeon.RemoveArea(floor.Id, area.Id);

		floor.Areas.Should().BeEmpty();
		floor.Objects.Should().ContainSingle(placedObject => placedObject.Id == obj.Id);
	}

	[Fact]
	public void UpdateObject_WithoutScaleOnExistingObject_ReturnsSuccessAndAppliesPositionAndRotation()
	{
		// Arrange
		var floor = CreateFloor();
		var obj = floor.AddObject(
			ObjectKind.Chair,
			new CanvasPoint(0f, 0f),
			ObjectRotation.None);
		var newPosition = new CanvasPoint(120f, 45f);

		// Act
		var updated = floor.UpdateObject(
			obj.Id,
			newPosition,
			ObjectRotation.CW180);

		// Assert
		updated.Should().Be(MutationOutcome.Success);
		obj.Position.Should().Be(newPosition);
		obj.Rotation.Should().Be(ObjectRotation.CW180);
	}

	[Fact]
	public void UpdateObject_WithoutScaleOnUnknownId_ReturnsNotFound()
	{
		// Arrange
		var floor = CreateFloor();

		// Act
		var updated = floor.UpdateObject(
			Guid.NewGuid(),
			new CanvasPoint(1f, 1f),
			ObjectRotation.None);

		// Assert
		updated.Should().Be(MutationOutcome.NotFound);
	}
}
