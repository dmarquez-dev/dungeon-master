using DungeonMaster.Application.Common.Map;
using DungeonMaster.Domain.ValueObjects;
using FluentAssertions;

namespace DungeonMaster.Tests.Application.Common.Map;

public class SnapZoneCalculatorTests
{
	private const float TileSize = 40f;

	[Fact]
	public void ToTile_ComputesCorrectTileCoordinates()
	{
		// Canvas point (85, 45) with tile 40 → tile (2, 1)
		var tile = SnapZoneCalculator.ToTile(
			85f,
			45f,
			TileSize);

		tile.Should().Be(new TilePoint(2, 1));
	}

	[Fact]
	public void ToTile_OriginIsZero()
	{
		var tile = SnapZoneCalculator.ToTile(
			5f,
			5f,
			TileSize);

		tile.Should().Be(new TilePoint(0, 0));
	}

	[Fact]
	public void ToTile_ExactTileBoundary_MapsToNextTile()
	{
		// 80 / 40 = 2.0 exactly — should land on tile (2, 2)
		var tile = SnapZoneCalculator.ToTile(
			80f,
			80f,
			TileSize);

		tile.Should().Be(new TilePoint(2, 2));
	}

	[Fact]
	public void ToTile_NegativePartialTile_FloorsTowardNegativeInfinity()
	{
		// MathF.Floor(-0.375f) == -1 — the implementation floors, not truncates
		// so a point just inside the negative first tile maps to tile (-1, -1)
		var tile = SnapZoneCalculator.ToTile(
			-15f,
			-15f,
			TileSize);

		tile.Should().Be(new TilePoint(-1, -1));
	}

	[Fact]
	public void ToTile_NegativeFullTile_MapsToNegativeTile()
	{
		// MathF.Floor(-40f / 40f) == MathF.Floor(-1.0f) == -1
		var tile = SnapZoneCalculator.ToTile(
			-40f,
			-40f,
			TileSize);

		tile.Should().Be(new TilePoint(-1, -1));
	}
}
