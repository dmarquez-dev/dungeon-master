using DungeonMaster.Application.Common.Results;
using DungeonMaster.Application.Features.Dungeons;
using DungeonMaster.Application.Features.Dungeons.Commands.FloodFillTileTextures;
using DungeonMaster.Domain.Entities.Area;
using DungeonMaster.Domain.Entities.Dungeon;
using DungeonMaster.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Commands;

public class FloodFillTileTexturesCommandHandlerTests
{
	private readonly IDungeonWriteRepository _writeRepository;
	private readonly FloodFillTileTexturesCommandHandler _handler;

	public FloodFillTileTexturesCommandHandlerTests()
	{
		_writeRepository = Substitute.For<IDungeonWriteRepository>();
		_handler = new FloodFillTileTexturesCommandHandler(_writeRepository);
	}

	// Creates a dungeon with a 4x4 rectangular area (origin 0,0, outline forms a 4x4 square).
	private static Dungeon MakeDungeonWithRectArea(out Guid floorId, out Guid areaId)
	{
		var dungeon = new Dungeon("D");
		var floor = dungeon.AddFloor("F");
		var area = dungeon.AddArea(floor.Id, "A", null);
		floorId = floor.Id;
		areaId = area.Id;

		// 4x4 rectangle: origin (0,0), outline segments close back to origin
		var outline = new List<PathSegment>
		{
			new PathSegment(PathSegmentKind.Line, new TilePoint(4, 0), null, null),
			new PathSegment(PathSegmentKind.Line, new TilePoint(4, 4), null, null),
			new PathSegment(PathSegmentKind.Line, new TilePoint(0, 4), null, null),
			new PathSegment(PathSegmentKind.Line, new TilePoint(0, 0), null, null),
		};
		dungeon.UpdateAreaOutline(floorId, areaId, new TilePoint(0, 0), outline);

		return dungeon;
	}

	[Fact]
	public async Task HandleAsync_EmptyArea_FillsAllTilesInsidePolygon()
	{
		var dungeon = MakeDungeonWithRectArea(out var floorId, out var areaId);
		_writeRepository.LoadAsync(dungeon.Id, Arg.Any<CancellationToken>()).Returns(dungeon);

		var result = await _handler.HandleAsync(
			new FloodFillTileTexturesCommand(dungeon.Id, floorId, areaId, new TilePoint(0, 0), TextureKind.StoneFloor),
			CancellationToken.None);

		result.IsSuccess.Should().BeTrue();
		result.Value.Should().Be(16); // 4x4 = 16 tiles
		await _writeRepository.Received(1).UpdateAsync(dungeon, Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task HandleAsync_DungeonNotFound_ReturnsNotFound()
	{
		_writeRepository.LoadAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Dungeon?)null);

		var result = await _handler.HandleAsync(
			new FloodFillTileTexturesCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new TilePoint(0, 0), TextureKind.DirtFloor),
			CancellationToken.None);

		result.IsSuccess.Should().BeFalse();
		result.ErrorCategory.Should().Be(ErrorCategory.NotFound);
	}
}
