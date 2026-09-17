using DungeonMaster.Domain.Entities;
using FluentAssertions;
using DomainDungeon = DungeonMaster.Domain.Entities.Dungeons.Dungeon;
using DomainFloor = DungeonMaster.Domain.Entities.Floors.Floor;

namespace DungeonMaster.Tests.Domain.Entities.Floor;

public class FloorNumberGuardTests
{
	[Fact]
	public void Constructor_WithZeroNumber_Throws()
	{
		var act = () => new DomainFloor(
			Guid.NewGuid(),
			0,
			"Entry");

		act.Should().ThrowExactly<ArgumentOutOfRangeException>()
			.Which.ParamName.Should().Be("number");
	}

	[Fact]
	public void Constructor_WithNegativeNumber_Throws()
	{
		var act = () => new DomainFloor(
			Guid.NewGuid(),
			-1,
			"Entry");

		act.Should().ThrowExactly<ArgumentOutOfRangeException>()
			.Which.ParamName.Should().Be("number");
	}

	[Fact]
	public void Constructor_WithNumberOne_Succeeds()
	{
		var floor = new DomainFloor(
			Guid.NewGuid(),
			1,
			"Entry");

		floor.Number.Should().Be(1);
	}

	[Fact]
	public void UpdateNumber_WithZero_Throws()
	{
		var dungeon = new DomainDungeon("Test Dungeon");
		var floor = dungeon.AddFloor("Entry");

		var act = () => floor.UpdateNumber(0);

		act.Should().ThrowExactly<ArgumentOutOfRangeException>()
			.Which.ParamName.Should().Be("number");
	}

	[Fact]
	public void UpdateNumber_WithNegative_Throws()
	{
		var dungeon = new DomainDungeon("Test Dungeon");
		var floor = dungeon.AddFloor("Entry");

		var act = () => floor.UpdateNumber(-1);

		act.Should().ThrowExactly<ArgumentOutOfRangeException>()
			.Which.ParamName.Should().Be("number");
	}

	[Fact]
	public void UpdateNumber_WithOne_Succeeds()
	{
		var dungeon = new DomainDungeon("Test Dungeon");
		dungeon.AddFloor("Entry");
		var second = dungeon.AddFloor("Cellar");

		second.Number.Should().Be(2);

		second.UpdateNumber(1);

		second.Number.Should().Be(1);
	}

	[Fact]
	public void ReorderFloor_WithTargetZero_ReturnsOutOfRange()
	{
		var dungeon = new DomainDungeon("Test Dungeon");
		var first = dungeon.AddFloor("Entry");
		dungeon.AddFloor("Cellar");

		var outcome = dungeon.ReorderFloor(
			first.Id,
			0);

		outcome.Should().Be(MutationOutcome.OutOfRange);
		first.Number.Should().Be(1);
	}
}
