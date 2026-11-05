using DungeonMaster.Domain.Entities;
using DungeonMaster.Domain.Entities.Floors;
using DungeonMaster.Domain.ValueObjects;
using FluentAssertions;
using DomainDungeon = DungeonMaster.Domain.Entities.Dungeons.Dungeon;

namespace DungeonMaster.Tests.Domain.Entities.Dungeon;

public class DungeonLastModifiedOnFailedMutationTests
{
	private static readonly TimeSpan ClockAdvanceTimeout = TimeSpan.FromSeconds(1);

	[Fact]
	public void RemoveArea_WithExistingFloorAndMissingArea_ReturnsNotFoundAndLeavesLastModifiedUnchanged()
	{
		// Arrange
		var dungeon = new DomainDungeon("Test Dungeon");
		var floor = dungeon.AddFloor("Entry");
		var captured = CaptureLastModifiedAndWaitForClockToAdvance(dungeon);

		// Act
		var outcome = dungeon.RemoveArea(
			floor.Id,
			Guid.NewGuid());

		// Assert
		outcome.Should().Be(MutationOutcome.NotFound);
		dungeon.LastModified.Should().Be(
			captured,
			"a failed inner floor mutation must not advance the dungeon's LastModified");
	}

	[Fact]
	public void UpdateArea_WithExistingFloorAndMissingArea_ReturnsNotFoundAndLeavesLastModifiedUnchanged()
	{
		// Arrange
		var dungeon = new DomainDungeon("Test Dungeon");
		var floor = dungeon.AddFloor("Entry");
		var captured = CaptureLastModifiedAndWaitForClockToAdvance(dungeon);

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
			captured,
			"a failed inner floor mutation must not advance the dungeon's LastModified");
	}

	[Fact]
	public void UpdateAreaOutline_WithExistingFloorAndMissingArea_ReturnsNotFoundAndLeavesLastModifiedUnchanged()
	{
		// Arrange
		var dungeon = new DomainDungeon("Test Dungeon");
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
		var captured = CaptureLastModifiedAndWaitForClockToAdvance(dungeon);

		// Act
		var outcome = dungeon.UpdateAreaOutline(
			floor.Id,
			Guid.NewGuid(),
			outline);

		// Assert
		outcome.Should().Be(MutationOutcome.NotFound);
		dungeon.LastModified.Should().Be(
			captured,
			"a failed inner floor mutation must not advance the dungeon's LastModified");
	}

	[Fact]
	public void RemovePlacedObject_WithExistingFloorAndMissingObject_ReturnsNotFoundAndLeavesLastModifiedUnchanged()
	{
		// Arrange
		var dungeon = new DomainDungeon("Test Dungeon");
		var floor = dungeon.AddFloor("Entry");
		var captured = CaptureLastModifiedAndWaitForClockToAdvance(dungeon);

		// Act
		var outcome = dungeon.RemovePlacedObject(
			floor.Id,
			Guid.NewGuid());

		// Assert
		outcome.Should().Be(MutationOutcome.NotFound);
		dungeon.LastModified.Should().Be(
			captured,
			"a failed inner floor mutation must not advance the dungeon's LastModified");
	}

	[Fact]
	public void UpdatePlacedObject_WithExistingFloorAndMissingObject_ReturnsNotFoundAndLeavesLastModifiedUnchanged()
	{
		// Arrange
		var dungeon = new DomainDungeon("Test Dungeon");
		var floor = dungeon.AddFloor("Entry");
		var captured = CaptureLastModifiedAndWaitForClockToAdvance(dungeon);

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
			captured,
			"a failed inner floor mutation must not advance the dungeon's LastModified");
	}

	[Fact]
	public void UpdateArea_WithExistingFloorAndExistingArea_ReturnsSuccessAndAdvancesLastModified()
	{
		// Arrange
		var dungeon = new DomainDungeon("Test Dungeon");
		var floor = dungeon.AddFloor("Entry");
		var area = dungeon.AddArea(
			floor.Id,
			"Grand Foyer",
			"A large entrance hall.")!;
		var captured = CaptureLastModifiedAndWaitForClockToAdvance(dungeon);

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
			captured,
			"a successful inner floor mutation advances the dungeon's LastModified");
	}

	/// <summary>
	///		Captures the dungeon's <see cref="Entity.LastModified"/>, then waits until the UTC clock has moved past it,
	///		so that any <c>Touch</c> during the act step would produce an observably later value.
	/// </summary>
	/// <param name="dungeon">
	///		The dungeon whose timestamp to capture.
	/// </param>
	/// <returns>
	///		The captured <see cref="Entity.LastModified"/> value.
	/// </returns>
	private static DateTime CaptureLastModifiedAndWaitForClockToAdvance(DomainDungeon dungeon)
	{
		var captured = dungeon.LastModified;

		var clockAdvanced = SpinWait.SpinUntil(
			() =>
				DateTime.UtcNow > captured,
			ClockAdvanceTimeout);

		clockAdvanced.Should().BeTrue("the UTC clock must move past the captured LastModified within the timeout");

		return captured;
	}
}
