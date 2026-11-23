using DungeonMaster.Domain.Entities;
using DungeonMaster.Domain.Entities.Dungeons;
using DungeonMaster.Domain.Entities.Floors;
using DungeonMaster.Domain.ValueObjects;
using FluentAssertions;

namespace DungeonMaster.Tests.Domain.Entities.Dungeons;

public class DungeonLastModifiedOnFailedMutationTests
{
	private static readonly DateTime BackdatedLastModified = new(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);

	[Fact]
	public void RemoveArea_WithExistingFloorAndMissingArea_ReturnsNotFoundAndLeavesLastModifiedUnchanged()
	{
		// Arrange
		var dungeon = new Dungeon("Test Dungeon");
		var floor = dungeon.AddFloor("Entry");
		BackdateLastModified(dungeon, BackdatedLastModified);

		// Act
		var outcome = dungeon.RemoveArea(
			floor.Id,
			Guid.NewGuid());

		// Assert
		outcome.Should().Be(MutationOutcome.NotFound);
		dungeon.LastModified.Should().Be(
			BackdatedLastModified,
			"a failed inner floor mutation must not advance the dungeon's LastModified");
	}

	[Fact]
	public void UpdateArea_WithExistingFloorAndMissingArea_ReturnsNotFoundAndLeavesLastModifiedUnchanged()
	{
		// Arrange
		var dungeon = new Dungeon("Test Dungeon");
		var floor = dungeon.AddFloor("Entry");
		BackdateLastModified(dungeon, BackdatedLastModified);

		// Act
		var outcome = dungeon.UpdateArea(
			floor.Id,
			Guid.NewGuid(),
			"Renamed Area",
			"A new description.",
			"New notes.");

		// Assert
		outcome.Should().Be(MutationOutcome.NotFound);
		dungeon.LastModified.Should().Be(
			BackdatedLastModified,
			"a failed inner floor mutation must not advance the dungeon's LastModified");
	}

	[Fact]
	public void UpdateAreaOutline_WithExistingFloorAndMissingArea_ReturnsNotFoundAndLeavesLastModifiedUnchanged()
	{
		// Arrange
		var dungeon = new Dungeon("Test Dungeon");
		var floor = dungeon.AddFloor("Entry");
		IReadOnlyList<PathSegment> outline =
		[
			new PathSegment(
				PathSegmentKind.Line,
				new TilePoint(4, 0),
				null,
				null),
			new PathSegment(
				PathSegmentKind.Line,
				new TilePoint(0, 4),
				null,
				null),
			new PathSegment(
				PathSegmentKind.Line,
				new TilePoint(0, 0),
				null,
				null),
		];
		BackdateLastModified(dungeon, BackdatedLastModified);

		// Act
		var outcome = dungeon.UpdateAreaOutline(
			floor.Id,
			Guid.NewGuid(),
			outline);

		// Assert
		outcome.Should().Be(MutationOutcome.NotFound);
		dungeon.LastModified.Should().Be(
			BackdatedLastModified,
			"a failed inner floor mutation must not advance the dungeon's LastModified");
	}

	[Fact]
	public void RemovePlacedObject_WithExistingFloorAndMissingObject_ReturnsNotFoundAndLeavesLastModifiedUnchanged()
	{
		// Arrange
		var dungeon = new Dungeon("Test Dungeon");
		var floor = dungeon.AddFloor("Entry");
		BackdateLastModified(dungeon, BackdatedLastModified);

		// Act
		var outcome = dungeon.RemovePlacedObject(
			floor.Id,
			Guid.NewGuid());

		// Assert
		outcome.Should().Be(MutationOutcome.NotFound);
		dungeon.LastModified.Should().Be(
			BackdatedLastModified,
			"a failed inner floor mutation must not advance the dungeon's LastModified");
	}

	[Fact]
	public void UpdatePlacedObject_WithExistingFloorAndMissingObject_ReturnsNotFoundAndLeavesLastModifiedUnchanged()
	{
		// Arrange
		var dungeon = new Dungeon("Test Dungeon");
		var floor = dungeon.AddFloor("Entry");
		BackdateLastModified(dungeon, BackdatedLastModified);

		// Act
		var outcome = dungeon.UpdatePlacedObject(
			floor.Id,
			Guid.NewGuid(),
			new CanvasPoint(10f, 10f),
			ObjectRotation.Clockwise90,
			1f);

		// Assert
		outcome.Should().Be(MutationOutcome.NotFound);
		dungeon.LastModified.Should().Be(
			BackdatedLastModified,
			"a failed inner floor mutation must not advance the dungeon's LastModified");
	}

	[Fact]
	public void UpdateArea_WithExistingFloorAndExistingArea_ReturnsSuccessAndAdvancesLastModified()
	{
		// Arrange
		var dungeon = new Dungeon("Test Dungeon");
		var floor = dungeon.AddFloor("Entry");
		var area = dungeon.AddArea(
			floor.Id,
			"Grand Foyer",
			"A large entrance hall.")!;
		BackdateLastModified(dungeon, BackdatedLastModified);

		// Act
		var outcome = dungeon.UpdateArea(
			floor.Id,
			area.Id,
			"Renamed Foyer",
			"A renamed entrance hall.",
			"New notes.");

		// Assert
		outcome.Should().Be(MutationOutcome.Success);
		dungeon.LastModified.Should().BeAfter(
			BackdatedLastModified,
			"a successful inner floor mutation advances the dungeon's LastModified");
	}

	/// <summary>
	///		Sets the entity's <see cref="Entity.LastModified"/> to a fixed past instant by reflection,
	///		so that any <c>Touch</c> during the act step produces an observably later value.
	/// </summary>
	/// <param name="entity">
	///		The entity whose timestamp to set.
	/// </param>
	/// <param name="lastModified">
	///		The instant to set.
	/// </param>
	private static void BackdateLastModified(Entity entity, DateTime lastModified) =>
		typeof(Entity)
			.GetProperty(nameof(Entity.LastModified))!
			.SetValue(entity, lastModified);
}
