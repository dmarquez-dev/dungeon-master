using DungeonMaster.Domain.Entities;
using DungeonMaster.Domain.Entities.Dungeons;
using DungeonMaster.Domain.Entities.Floors;
using FluentAssertions;

namespace DungeonMaster.Tests.Domain.Entities.Floors;

public class FloorNumberGuardTests
{
	[Fact]
	public void Constructor_WithZeroNumber_Throws()
	{
		// Act
		var act = () => new Floor(
			Guid.NewGuid(),
			0,
			"Entry");

		// Assert
		act.Should().ThrowExactly<ArgumentOutOfRangeException>()
			.Which.ParamName.Should().Be("number");
	}

	[Fact]
	public void Constructor_WithNegativeNumber_Throws()
	{
		// Act
		var act = () => new Floor(
			Guid.NewGuid(),
			-1,
			"Entry");

		// Assert
		act.Should().ThrowExactly<ArgumentOutOfRangeException>()
			.Which.ParamName.Should().Be("number");
	}

	[Fact]
	public void Constructor_WithNumberOne_Succeeds()
	{
		// Act
		var floor = new Floor(
			Guid.NewGuid(),
			1,
			"Entry");

		// Assert
		floor.Number.Should().Be(1);
	}

	[Fact]
	public void UpdateNumber_WithZero_Throws()
	{
		// Arrange
		var dungeon = new Dungeon("Test Dungeon");
		var floor = dungeon.AddFloor("Entry");

		// Act
		var act = () => floor.UpdateNumber(0);

		// Assert
		act.Should().ThrowExactly<ArgumentOutOfRangeException>()
			.Which.ParamName.Should().Be("number");
	}

	[Fact]
	public void UpdateNumber_WithNegative_Throws()
	{
		// Arrange
		var dungeon = new Dungeon("Test Dungeon");
		var floor = dungeon.AddFloor("Entry");

		// Act
		var act = () => floor.UpdateNumber(-1);

		// Assert
		act.Should().ThrowExactly<ArgumentOutOfRangeException>()
			.Which.ParamName.Should().Be("number");
	}

	[Fact]
	public void UpdateNumber_WithOne_Succeeds()
	{
		// Arrange
		var dungeon = new Dungeon("Test Dungeon");
		dungeon.AddFloor("Entry");
		var second = dungeon.AddFloor("Cellar");

		// Act
		second.UpdateNumber(1);

		// Assert
		second.Number.Should().Be(1);
	}

	[Fact]
	public void ReorderFloor_WithTargetZero_ReturnsOutOfRange()
	{
		// Arrange
		var dungeon = new Dungeon("Test Dungeon");
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
}
