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
