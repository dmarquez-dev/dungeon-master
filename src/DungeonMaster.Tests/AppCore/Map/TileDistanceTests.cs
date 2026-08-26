using DungeonMaster.App.Core.Map;
using DungeonMaster.Domain.ValueObjects;
using FluentAssertions;

namespace DungeonMaster.Tests.AppCore.Map;

public class TileDistanceTests
{
	private const float FeetPerTile = 5f;

	[Fact]
	public void InFeet_WhenPointsAreEqual_ReturnsZero()
	{
		var feet = TileDistance.InFeet(new TilePoint(2, 3), new TilePoint(2, 3), FeetPerTile);
		feet.Should().Be(0);
	}

	[Theory]
	[InlineData(3, 0, 15)]
	[InlineData(0, 4, 20)]
	[InlineData(-3, 0, 15)]
	public void InFeet_ForOrthogonalDistance_ReturnsTileCountTimesFeetPerTile(int col, int row, int expectedFeet)
	{
		var feet = TileDistance.InFeet(new TilePoint(0, 0), new TilePoint(col, row), FeetPerTile);
		feet.Should().Be(expectedFeet);
	}

	[Fact]
	public void InFeet_ForDiagonalDistance_ReturnsRoundedWholeFeet()
	{
		// One tile diagonally is sqrt(2) tiles, which is 7.0710678 feet at 5 feet per tile.
		var feet = TileDistance.InFeet(new TilePoint(0, 0), new TilePoint(1, 1), FeetPerTile);
		feet.Should().Be(7);
	}

	[Theory]
	[InlineData(2.5f, 2)]
	[InlineData(3.5f, 4)]
	[InlineData(4.5f, 4)]
	public void InFeet_WhenResultIsExactlyAMidpoint_RoundsToTheNearestEvenFoot(float feetPerTile, int expectedFeet)
	{
		var feet = TileDistance.InFeet(new TilePoint(0, 0), new TilePoint(1, 0), feetPerTile);
		feet.Should().Be(expectedFeet);
	}
}
