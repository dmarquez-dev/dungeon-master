using DungeonMaster.App.Core.Map;
using DungeonMaster.Domain.ValueObjects;
using FluentAssertions;

namespace DungeonMaster.Tests.AppCore.Map;

public class TileCoordinateHelperTests
{
	private const float TileSize = 20f;

	[Theory]
	[InlineData(0f, 0f, 0, 0)]
	[InlineData(10f, 10f, 1, 1)]
	[InlineData(19f, 19f, 1, 1)]
	[InlineData(40f, 60f, 2, 3)]
	[InlineData(41f, 59f, 2, 3)]
	public void SnapToGrid_ReturnsNearestTilePoint(float x, float y, int expectedCol, int expectedRow)
	{
		// Act
		var result = TileCoordinateHelper.SnapToGrid(x, y, TileSize);

		// Assert
		result.Col.Should().Be(expectedCol);
		result.Row.Should().Be(expectedRow);
	}

	// The four facts below pin what SnapToGrid CURRENTLY returns for a non-finite coordinate.
	// They are not a desired result: a NaN snaps to a real tile, so callers must guard against it.
	[Fact]
	public void SnapToGrid_NaNX_PinsCurrentBehaviourCallersMustGuardAgainst()
	{
		// Act
		var result = TileCoordinateHelper.SnapToGrid(float.NaN, 40f, TileSize);

		// Assert
		result.Col.Should().Be(0, "this pins current behaviour for a NaN x, which callers must guard against");
		result.Row.Should().Be(2, "this pins current behaviour for a NaN x, which callers must guard against");
	}

	[Fact]
	public void SnapToGrid_NaNY_PinsCurrentBehaviourCallersMustGuardAgainst()
	{
		// Act
		var result = TileCoordinateHelper.SnapToGrid(40f, float.NaN, TileSize);

		// Assert
		result.Col.Should().Be(2, "this pins current behaviour for a NaN y, which callers must guard against");
		result.Row.Should().Be(0, "this pins current behaviour for a NaN y, which callers must guard against");
	}

	[Fact]
	public void SnapToGrid_PositiveInfinity_PinsCurrentBehaviourCallersMustGuardAgainst()
	{
		// Act
		var result = TileCoordinateHelper.SnapToGrid(float.PositiveInfinity, float.PositiveInfinity, TileSize);

		// Assert
		result.Col.Should().Be(int.MaxValue, "this pins current behaviour for positive infinity, which callers must guard against");
		result.Row.Should().Be(int.MaxValue, "this pins current behaviour for positive infinity, which callers must guard against");
	}

	[Fact]
	public void SnapToGrid_NegativeInfinity_PinsCurrentBehaviourCallersMustGuardAgainst()
	{
		// Act
		var result = TileCoordinateHelper.SnapToGrid(float.NegativeInfinity, float.NegativeInfinity, TileSize);

		// Assert
		result.Col.Should().Be(int.MinValue, "this pins current behaviour for negative infinity, which callers must guard against");
		result.Row.Should().Be(int.MinValue, "this pins current behaviour for negative infinity, which callers must guard against");
	}

	[Theory]
	[InlineData(0, 0, 0f, 0f)]
	[InlineData(1, 2, 20f, 40f)]
	[InlineData(3, 5, 60f, 100f)]
	public void ToPixel_ReturnsTopLeftOfTile(int col, int row, float expectedX, float expectedY)
	{
		// Act
		var (x, y) = TileCoordinateHelper.ToPixel(new TilePoint(col, row), TileSize);

		// Assert
		x.Should().Be(expectedX);
		y.Should().Be(expectedY);
	}
}
