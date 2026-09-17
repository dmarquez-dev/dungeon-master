using DungeonMaster.Domain.Entities;
using FluentAssertions;
using DomainArea = DungeonMaster.Domain.Entities.Areas.Area;
using DomainDungeon = DungeonMaster.Domain.Entities.Dungeons.Dungeon;
using DomainFloor = DungeonMaster.Domain.Entities.Floors.Floor;

namespace DungeonMaster.Tests.Domain.Entities;

public class EntityEqualityTests
{
	// DISCRIMINATING: fails against reference equality.
	[Fact]
	public void Equals_WithSameRuntimeTypeAndSameId_ReturnsTrueThroughTheTypedOverload()
	{
		// Arrange
		var dungeonId = Guid.NewGuid();
		var sharedId = Guid.NewGuid();
		var first = new DomainFloor(dungeonId, 1, "Entry");
		var second = new DomainFloor(dungeonId, 2, "Cellar");

		ForceId(first, sharedId);
		ForceId(second, sharedId);

		// Act
		var equal = first.Equals(second);

		// Assert
		ReferenceEquals(first, second).Should().BeFalse();
		equal.Should().BeTrue();
		second.Equals(first).Should().BeTrue();
	}

	// DISCRIMINATING: fails against reference equality.
	[Fact]
	public void Equals_WithSameRuntimeTypeAndSameId_ReturnsTrueThroughTheObjectOverload()
	{
		// Arrange
		var dungeonId = Guid.NewGuid();
		var sharedId = Guid.NewGuid();
		var first = new DomainFloor(dungeonId, 1, "Entry");
		var second = new DomainFloor(dungeonId, 2, "Cellar");

		ForceId(first, sharedId);
		ForceId(second, sharedId);

		// Act
		var equal = first.Equals((object?)second);

		// Assert
		ReferenceEquals(first, second).Should().BeFalse();
		equal.Should().BeTrue();
		second.Equals((object?)first).Should().BeTrue();
	}

	// DISCRIMINATING: fails against reference equality.
	[Fact]
	public void GetHashCode_WithSameRuntimeTypeAndSameId_ReturnsEqualHashCodes()
	{
		// Arrange
		var dungeonId = Guid.NewGuid();
		var sharedId = Guid.NewGuid();
		var first = new DomainFloor(dungeonId, 1, "Entry");
		var second = new DomainFloor(dungeonId, 2, "Cellar");

		ForceId(first, sharedId);
		ForceId(second, sharedId);

		// Act
		var firstHash = first.GetHashCode();
		var secondHash = second.GetHashCode();

		// Assert
		ReferenceEquals(first, second).Should().BeFalse();
		firstHash.Should().Be(secondHash);
	}

	// DISCRIMINATING: fails against reference equality. This is the observable effect at the real
	// List<T> surface that the three Domain Remove sites consult.
	[Fact]
	public void Contains_WithADistinctFloorCarryingTheTrackedFloorsId_ReturnsTrue()
	{
		// Arrange
		var dungeon = new DomainDungeon("Test Dungeon");
		var tracked = dungeon.AddFloor("Entry");
		var clone = new DomainFloor(dungeon.Id, 1, "Entry");

		ForceId(clone, tracked.Id);

		// Act
		var contains = dungeon.Floors.Contains(clone);

		// Assert
		ReferenceEquals(tracked, clone).Should().BeFalse();
		contains.Should().BeTrue();
	}

	// CONTROL: already passes against today's reference equality and proves nothing on its own.
	[Fact]
	public void Equals_WithSameRuntimeTypeAndDifferentId_ReturnsFalse()
	{
		// Arrange
		var dungeonId = Guid.NewGuid();
		var first = new DomainFloor(dungeonId, 1, "Entry");
		var second = new DomainFloor(dungeonId, 2, "Cellar");

		// Act
		var equal = first.Equals(second);

		// Assert
		first.Id.Should().NotBe(second.Id);
		equal.Should().BeFalse();
		first.Equals((object?)second).Should().BeFalse();
	}

	// CONTROL: already passes against today's reference equality and proves nothing on its own.
	[Fact]
	public void Equals_WithDifferentRuntimeTypesSharingAnId_ReturnsFalse()
	{
		// Arrange
		var sharedId = Guid.NewGuid();
		var floor = new DomainFloor(Guid.NewGuid(), 1, "Entry");
		var area = new DomainArea(floor.Id, "Guard Room", null);

		ForceId(floor, sharedId);
		ForceId(area, sharedId);

		// Act
		var equal = floor.Equals(area);

		// Assert
		floor.Id.Should().Be(area.Id);
		equal.Should().BeFalse();
		area.Equals((object?)floor).Should().BeFalse();
	}

	// CONTROL: already passes against today's reference equality and proves nothing on its own.
	[Fact]
	public void Equals_WithNullAndWithItself_ReturnsFalseThenTrue()
	{
		// Arrange
		// A second instance carries the object-overload null case: the compiler's nullable flow
		// analysis marks a receiver maybe-null after it has been compared against null, so two
		// consecutive null comparisons on one receiver would not compile.
		var floor = new DomainFloor(Guid.NewGuid(), 1, "Entry");
		var another = new DomainFloor(Guid.NewGuid(), 2, "Cellar");

		// Act
		var againstItself = floor.Equals(floor);
		var againstItselfAsObject = floor.Equals((object?)floor);
		var againstTypedNull = floor.Equals((Entity?)null);
		var againstObjectNull = another.Equals((object?)null);

		// Assert
		againstItself.Should().BeTrue();
		againstItselfAsObject.Should().BeTrue();
		againstTypedNull.Should().BeFalse();
		againstObjectNull.Should().BeFalse();
	}

	// DISCRIMINATING: the equality half fails against reference equality. The hash half exists to
	// catch a GetHashCode that reads mutable state, so it asserts that LastModified really changed.
	[Fact]
	public async Task GetHashCode_AfterAMutationThatTouchesLastModified_IsUnchanged()
	{
		// Arrange
		var dungeon = new DomainDungeon("Test Dungeon");
		var floor = dungeon.AddFloor("Entry");
		var clone = new DomainFloor(dungeon.Id, 1, "Entry");

		ForceId(clone, floor.Id);

		var hashBefore = floor.GetHashCode();
		var modifiedBefore = floor.LastModified;

		await Task.Delay(20);

		// Act
		var outcome = dungeon.UpdateFloor(
			floor.Id,
			"Renamed Entry",
			"A description",
			"Some notes");

		// Assert
		outcome.Should().Be(MutationOutcome.Success);
		floor.LastModified.Should().BeAfter(modifiedBefore);
		floor.GetHashCode().Should().Be(hashBefore);
		floor.Equals(clone).Should().BeTrue();
		floor.GetHashCode().Should().Be(clone.GetHashCode());
	}

	private static void ForceId(Entity entity, Guid id) =>
		typeof(Entity)
			.GetProperty(nameof(Entity.Id))!
			.SetValue(entity, id);
}
