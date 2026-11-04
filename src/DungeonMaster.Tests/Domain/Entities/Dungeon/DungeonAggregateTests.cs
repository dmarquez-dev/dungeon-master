using DungeonMaster.Domain.Entities;
using DungeonMaster.Domain.Entities.Floors;
using DungeonMaster.Domain.ValueObjects;
using FluentAssertions;
using DomainDungeon = DungeonMaster.Domain.Entities.Dungeons.Dungeon;

namespace DungeonMaster.Tests.Domain.Entities.Dungeon;

public class DungeonAggregateTests
{
	[Fact]
	public void AddFloor_IncrementsFloorCount()
	{
		// Arrange
		var dungeon = new DomainDungeon("Test Dungeon");

		// Act
		dungeon.AddFloor("Entry Level");

		// Assert
		dungeon.Floors.Should().HaveCount(1);
		dungeon.Floors[0].Number.Should().Be(1);
		dungeon.Floors[0].Name.Should().Be("Entry Level");
	}

	[Fact]
	public void AddFloor_AssignsSequentialNumbers()
	{
		// Arrange
		var dungeon = new DomainDungeon("Test Dungeon");

		// Act
		dungeon.AddFloor(null);
		dungeon.AddFloor("Second");
		dungeon.AddFloor(null);

		// Assert
		dungeon.Floors.Select(floor => floor.Number).Should().Equal(1, 2, 3);
	}

	[Fact]
	public void RemoveFloor_RemovesAndRenumbers()
	{
		// Arrange
		var dungeon = new DomainDungeon("Test Dungeon");
		dungeon.AddFloor("First");
		var second = dungeon.AddFloor("Second");
		dungeon.AddFloor("Third");

		// Act
		dungeon.RemoveFloor(second.Id);

		// Assert
		dungeon.Floors.Should().HaveCount(2);
		dungeon.Floors.Select(floor => floor.Number).Should().Equal(1, 2);
	}

	[Fact]
	public void RemoveFloor_WithNonExistentId_IsNoOp()
	{
		// Arrange
		var dungeon = new DomainDungeon("Test Dungeon");
		dungeon.AddFloor(null);

		// Act
		var act = () => dungeon.RemoveFloor(Guid.NewGuid());

		// Assert
		act.Should().NotThrow();
		dungeon.Floors.Should().HaveCount(1);
	}

	[Fact]
	public void ReorderFloor_MovesFloorAndRenumbersOthers()
	{
		// Arrange
		var dungeon = new DomainDungeon("Test Dungeon");
		dungeon.AddFloor("First");
		dungeon.AddFloor("Second");
		var third = dungeon.AddFloor("Third");

		// Act
		dungeon.ReorderFloor(third.Id, 1);

		// Assert
		var ordered = dungeon.Floors.OrderBy(floor => floor.Number).ToList();
		ordered[0].Name.Should().Be("Third");
		ordered[1].Name.Should().Be("First");
		ordered[2].Name.Should().Be("Second");
	}

	[Fact]
	public void AddArea_AddsToCorrectFloor()
	{
		// Arrange
		var dungeon = new DomainDungeon("Test Dungeon");
		var floor = dungeon.AddFloor("Entry");

		// Act
		var area = dungeon.AddArea(floor.Id, "Grand Foyer", "A large entrance hall.")!;

		// Assert
		floor.Areas.Should().HaveCount(1);
		floor.Areas[0].Name.Should().Be("Grand Foyer");
		area.Id.Should().NotBeEmpty();
	}

	[Fact]
	public void RemoveArea_RemovesFromCorrectFloor()
	{
		// Arrange
		var dungeon = new DomainDungeon("Test Dungeon");
		var floor = dungeon.AddFloor("Entry");
		var area = dungeon.AddArea(floor.Id, "Grand Foyer", null)!;

		// Act
		dungeon.RemoveArea(floor.Id, area.Id);

		// Assert
		floor.Areas.Should().BeEmpty();
	}

	[Fact]
	public void Update_SetsAllThreeFields()
	{
		// Arrange
		var dungeon = new DomainDungeon("Old Name", "Old Desc");

		// Act
		dungeon.Update("New Name", "New Desc", "DM Notes");

		// Assert
		dungeon.Name.Should().Be("New Name");
		dungeon.Description.Should().Be("New Desc");
		dungeon.Notes.Should().Be("DM Notes");
	}

	[Fact]
	public void RemoveFloor_WithUnknownFloorId_ReturnsNotFound()
	{
		// Arrange
		var dungeon = new DomainDungeon("Test Dungeon");
		dungeon.AddFloor("Entry");

		// Act
		var outcome = dungeon.RemoveFloor(Guid.NewGuid());

		// Assert
		outcome.Should().Be(MutationOutcome.NotFound);
		dungeon.Floors.Should().HaveCount(1);
	}

	[Fact]
	public void RemoveFloor_WithKnownFloorId_ReturnsSuccess()
	{
		// Arrange
		var dungeon = new DomainDungeon("Test Dungeon");
		var floor = dungeon.AddFloor("Entry");

		// Act
		var outcome = dungeon.RemoveFloor(floor.Id);

		// Assert
		outcome.Should().Be(MutationOutcome.Success);
		dungeon.Floors.Should().BeEmpty();
	}

	[Fact]
	public void ReorderFloor_WithUnknownFloorId_ReturnsNotFound()
	{
		// Arrange
		var dungeon = new DomainDungeon("Test Dungeon");
		dungeon.AddFloor("Entry");

		// Act
		var outcome = dungeon.ReorderFloor(
			Guid.NewGuid(),
			1);

		// Assert
		outcome.Should().Be(MutationOutcome.NotFound);
	}

	[Fact]
	public void ReorderFloor_WithNumberBelowOne_ReturnsOutOfRange()
	{
		// Arrange
		var dungeon = new DomainDungeon("Test Dungeon");
		var first = dungeon.AddFloor("Entry");
		dungeon.AddFloor("Cellar");

		// Act
		var outcome = dungeon.ReorderFloor(
			first.Id,
			0);

		// Assert
		outcome.Should().Be(MutationOutcome.OutOfRange);
		first.Number.Should().Be(1);
	}

	[Fact]
	public void ReorderFloor_WithNumberAboveFloorCount_ReturnsOutOfRange()
	{
		// Arrange
		var dungeon = new DomainDungeon("Test Dungeon");
		var first = dungeon.AddFloor("Entry");
		dungeon.AddFloor("Cellar");

		// Act
		var outcome = dungeon.ReorderFloor(
			first.Id,
			3);

		// Assert
		outcome.Should().Be(MutationOutcome.OutOfRange);
		first.Number.Should().Be(1);
	}

	[Fact]
	public void ReorderFloor_WithUnchangedNumber_ReturnsSuccess()
	{
		// Arrange
		var dungeon = new DomainDungeon("Test Dungeon");
		var first = dungeon.AddFloor("Entry");
		dungeon.AddFloor("Cellar");

		// Act
		var outcome = dungeon.ReorderFloor(
			first.Id,
			1);

		// Assert
		outcome.Should().Be(MutationOutcome.Success);
		first.Number.Should().Be(1);
	}

	[Fact]
	public void UpdateFloor_WithUnknownFloorId_ReturnsNotFound()
	{
		// Arrange
		var dungeon = new DomainDungeon("Test Dungeon");
		dungeon.AddFloor("Entry");

		// Act
		var outcome = dungeon.UpdateFloor(
			Guid.NewGuid(),
			"Renamed",
			null,
			null);

		// Assert
		outcome.Should().Be(MutationOutcome.NotFound);
	}

	[Fact]
	public void AddArea_WithUnknownFloorId_ReturnsNull()
	{
		// Arrange
		var dungeon = new DomainDungeon("Test Dungeon");

		// Act
		var act = () =>
			dungeon.AddArea(
				Guid.NewGuid(),
				"Orphan Area",
				null);

		// Assert
		act.Should().NotThrow();
		act().Should().BeNull();
	}

	[Fact]
	public void RemoveArea_WithUnknownFloorId_ReturnsNotFound()
	{
		// Arrange
		var dungeon = new DomainDungeon("Test Dungeon");
		var floor = dungeon.AddFloor("Entry");
		var area = dungeon.AddArea(floor.Id, "Grand Foyer", null)!;

		// Act
		var outcome = dungeon.RemoveArea(
			Guid.NewGuid(),
			area.Id);

		// Assert
		outcome.Should().Be(MutationOutcome.NotFound);
		floor.Areas.Should().ContainSingle();
	}

	[Fact]
	public void RemoveArea_WithUnknownAreaId_ReturnsNotFound()
	{
		// Arrange
		var dungeon = new DomainDungeon("Test Dungeon");
		var floor = dungeon.AddFloor("Entry");

		// Act
		var outcome = dungeon.RemoveArea(
			floor.Id,
			Guid.NewGuid());

		// Assert
		outcome.Should().Be(MutationOutcome.NotFound);
	}

	[Fact]
	public void UpdateArea_WithUnknownFloorId_ReturnsNotFound()
	{
		// Arrange
		var dungeon = new DomainDungeon("Test Dungeon");
		var floor = dungeon.AddFloor("Entry");
		var area = dungeon.AddArea(floor.Id, "Grand Foyer", null)!;

		// Act
		var outcome = dungeon.UpdateArea(
			Guid.NewGuid(),
			area.Id,
			"Renamed",
			null,
			null);

		// Assert
		outcome.Should().Be(MutationOutcome.NotFound);
		area.Name.Should().Be("Grand Foyer");
	}

	[Fact]
	public void UpdateArea_WithUnknownAreaId_ReturnsNotFound()
	{
		// Arrange
		var dungeon = new DomainDungeon("Test Dungeon");
		var floor = dungeon.AddFloor("Entry");

		// Act
		var outcome = dungeon.UpdateArea(
			floor.Id,
			Guid.NewGuid(),
			"Renamed",
			null,
			null);

		// Assert
		outcome.Should().Be(MutationOutcome.NotFound);
	}

	[Fact]
	public void UpdateAreaOutline_WithUnknownFloorId_ReturnsNotFound()
	{
		// Arrange
		var dungeon = new DomainDungeon("Test Dungeon");
		var floor = dungeon.AddFloor("Entry");
		var area = dungeon.AddArea(floor.Id, "Grand Foyer", null)!;

		// Act
		var outcome = dungeon.UpdateAreaOutline(
			Guid.NewGuid(),
			area.Id,
			new List<PathSegment>());

		// Assert
		outcome.Should().Be(MutationOutcome.NotFound);
	}

	[Fact]
	public void UpdateAreaOutline_WithUnknownAreaId_ReturnsNotFound()
	{
		// Arrange
		var dungeon = new DomainDungeon("Test Dungeon");
		var floor = dungeon.AddFloor("Entry");

		// Act
		var outcome = dungeon.UpdateAreaOutline(
			floor.Id,
			Guid.NewGuid(),
			new List<PathSegment>());

		// Assert
		outcome.Should().Be(MutationOutcome.NotFound);
	}

	[Fact]
	public void RemovePlacedObject_WithUnknownFloorId_ReturnsNotFound()
	{
		// Arrange
		var dungeon = new DomainDungeon("Test Dungeon");
		var floor = dungeon.AddFloor("Entry");
		var placed = dungeon.AddPlacedObject(
			floor.Id,
			ObjectKind.Barrel,
			new CanvasPoint(10f, 10f),
			ObjectRotation.None,
			ObjectScale.Default)!;

		// Act
		var outcome = dungeon.RemovePlacedObject(
			Guid.NewGuid(),
			placed.Id);

		// Assert
		outcome.Should().Be(MutationOutcome.NotFound);
		floor.Objects.Should().ContainSingle();
	}

	[Fact]
	public void RemovePlacedObject_WithUnknownObjectId_ReturnsNotFound()
	{
		// Arrange
		var dungeon = new DomainDungeon("Test Dungeon");
		var floor = dungeon.AddFloor("Entry");

		// Act
		var outcome = dungeon.RemovePlacedObject(
			floor.Id,
			Guid.NewGuid());

		// Assert
		outcome.Should().Be(MutationOutcome.NotFound);
	}

	[Fact]
	public void UpdatePlacedObject_WithUnknownFloorId_ReturnsNotFound()
	{
		// Arrange
		var dungeon = new DomainDungeon("Test Dungeon");
		var floor = dungeon.AddFloor("Entry");
		var originalPosition = new CanvasPoint(10f, 10f);
		var placed = dungeon.AddPlacedObject(
			floor.Id,
			ObjectKind.Barrel,
			originalPosition,
			ObjectRotation.None,
			ObjectScale.Default)!;

		// Act
		var outcome = dungeon.UpdatePlacedObject(
			Guid.NewGuid(),
			placed.Id,
			new CanvasPoint(1f, 1f),
			ObjectRotation.CW90,
			ObjectScale.Default);

		// Assert
		outcome.Should().Be(MutationOutcome.NotFound);
		placed.Position.Should().Be(originalPosition);
	}

	[Fact]
	public void UpdatePlacedObject_WithUnknownObjectId_ReturnsNotFound()
	{
		// Arrange
		var dungeon = new DomainDungeon("Test Dungeon");
		var floor = dungeon.AddFloor("Entry");

		// Act
		var outcome = dungeon.UpdatePlacedObject(
			floor.Id,
			Guid.NewGuid(),
			new CanvasPoint(1f, 1f),
			ObjectRotation.None,
			1.0f);

		// Assert
		outcome.Should().Be(MutationOutcome.NotFound);
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public void Constructor_WithNullOrWhitespaceName_ThrowsArgumentException(string? name)
	{
		// Act
		var act = () => new DomainDungeon(name!);

		// Assert
		act.Should().Throw<ArgumentException>();
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public void Update_WithNullOrWhitespaceName_ThrowsArgumentException(string? name)
	{
		// Arrange
		var dungeon = new DomainDungeon("Test Dungeon");

		// Act
		var act = () => dungeon.Update(
			name!,
			null,
			null);

		// Assert
		act.Should().Throw<ArgumentException>();
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public void AddArea_WithNullOrWhitespaceNameOnExistingFloor_ThrowsArgumentException(string? name)
	{
		// Arrange
		var dungeon = new DomainDungeon("Test Dungeon");
		var floor = dungeon.AddFloor("Entry");

		// Act
		var act = () => dungeon.AddArea(
			floor.Id,
			name!,
			null);

		// Assert
		act.Should().Throw<ArgumentException>();
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public void UpdateArea_WithNullOrWhitespaceNameOnExistingArea_ThrowsArgumentException(string? name)
	{
		// Arrange
		var dungeon = new DomainDungeon("Test Dungeon");
		var floor = dungeon.AddFloor("Entry");
		var area = dungeon.AddArea(floor.Id, "Grand Foyer", null)!;

		// Act
		var act = () => dungeon.UpdateArea(
			floor.Id,
			area.Id,
			name!,
			null,
			null);

		// Assert
		act.Should().Throw<ArgumentException>();
	}

	[Fact]
	public void ReorderFloor_WithNegativeNumber_ReturnsOutOfRangeAndLeavesNumberUnchanged()
	{
		// Arrange
		var dungeon = new DomainDungeon("Test Dungeon");
		var first = dungeon.AddFloor("Entry");
		dungeon.AddFloor("Cellar");

		// Act
		var outcome = dungeon.ReorderFloor(
			first.Id,
			-1);

		// Assert
		outcome.Should().Be(MutationOutcome.OutOfRange);
		first.Number.Should().Be(1);
	}

	[Fact]
	public void AddPlacedObject_ReturnsObjectWithGivenKindPositionRotationAndScale()
	{
		// Arrange
		var dungeon = new DomainDungeon("Test Dungeon");
		var floor = dungeon.AddFloor("Entry");
		var position = new CanvasPoint(64f, 96f);

		// Act
		var placed = dungeon.AddPlacedObject(
			floor.Id,
			ObjectKind.Chair,
			position,
			ObjectRotation.CW90,
			2.0f);

		// Assert
		placed.Should().NotBeNull();
		placed!.Kind.Should().Be(ObjectKind.Chair);
		placed.Position.Should().Be(position);
		placed.Rotation.Should().Be(ObjectRotation.CW90);
		placed.Scale.Should().BeApproximately(2.0f, 0.001f);
		floor.Objects.Should().ContainSingle(placedObject => placedObject.Id == placed.Id);
	}

	[Fact]
	public void AddPlacedObject_OnUnknownFloorId_ReturnsNull()
	{
		// Arrange
		var dungeon = new DomainDungeon("Test Dungeon");
		dungeon.AddFloor("Entry");

		// Act
		var placed = dungeon.AddPlacedObject(
			Guid.NewGuid(),
			ObjectKind.Barrel,
			new CanvasPoint(10f, 10f),
			ObjectRotation.None,
			ObjectScale.Default);

		// Assert
		placed.Should().BeNull();
	}
}
