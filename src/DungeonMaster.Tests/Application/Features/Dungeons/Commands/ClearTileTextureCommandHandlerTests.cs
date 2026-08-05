using DungeonMaster.Application.Common.Results;
using DungeonMaster.Application.Features.Dungeons;
using DungeonMaster.Application.Features.Dungeons.Commands.ClearTileTexture;
using DungeonMaster.Domain.Entities.Area;
using DungeonMaster.Domain.Entities.Dungeon;
using DungeonMaster.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Commands;

public class ClearTileTextureCommandHandlerTests
{
	private readonly IDungeonWriteRepository _writeRepository;
	private readonly ClearTileTextureCommandHandler _handler;

	public ClearTileTextureCommandHandlerTests()
	{
		_writeRepository = Substitute.For<IDungeonWriteRepository>();
		_handler = new ClearTileTextureCommandHandler(_writeRepository);
	}

	private static Dungeon MakeDungeon(out Guid floorId, out Guid areaId)
	{
		var dungeon = new Dungeon("D");
		var floor = dungeon.AddFloor("F");
		var area = dungeon.AddArea(floor.Id, "A", null);
		floorId = floor.Id;
		areaId = area.Id;
		return dungeon;
	}

	[Fact]
	public async Task HandleAsync_ClearsExistingTextureAndReturnsSuccess()
	{
		var dungeon = MakeDungeon(out var floorId, out var areaId);
		dungeon.SetTileTexture(floorId, areaId, new TilePoint(3, 4), TextureKind.WoodPlanks);
		_writeRepository.LoadAsync(dungeon.Id, Arg.Any<CancellationToken>()).Returns(dungeon);

		var result = await _handler.HandleAsync(
			new ClearTileTextureCommand(dungeon.Id, floorId, areaId, new TilePoint(3, 4)),
			CancellationToken.None);

		result.IsSuccess.Should().BeTrue();
		dungeon.Floors.First().Areas.First().TileTextures.Should().BeEmpty();
		await _writeRepository.Received(1).UpdateAsync(dungeon, Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task HandleAsync_DungeonNotFound_ReturnsNotFound()
	{
		_writeRepository.LoadAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Dungeon?)null);

		var result = await _handler.HandleAsync(
			new ClearTileTextureCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new TilePoint(0, 0)),
			CancellationToken.None);

		result.IsSuccess.Should().BeFalse();
		result.ErrorCategory.Should().Be(ErrorCategory.NotFound);
	}
}
