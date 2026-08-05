using DungeonMaster.Application.Common.Results;
using DungeonMaster.Application.Features.Dungeons;
using DungeonMaster.Application.Features.Dungeons.Commands.RemoveStructureElement;
using DungeonMaster.Domain.Entities.Area;
using DungeonMaster.Domain.Entities.Dungeon;
using DungeonMaster.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Commands;

public class RemoveStructureElementCommandHandlerTests
{
	private readonly IDungeonWriteRepository _writeRepository;
	private readonly RemoveStructureElementCommandHandler _handler;

	public RemoveStructureElementCommandHandlerTests()
	{
		_writeRepository = Substitute.For<IDungeonWriteRepository>();
		_handler = new RemoveStructureElementCommandHandler(_writeRepository);
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
	public async Task HandleAsync_RemovesExistingElementAndReturnsSuccess()
	{
		var dungeon = MakeDungeon(out var floorId, out var areaId);
		var element = dungeon.AddStructureElement(floorId, areaId,
			new VertexPoint(1, 1), new VertexPoint(1, 1), StructureKind.SquareColumn)!;
		_writeRepository.LoadAsync(dungeon.Id, Arg.Any<CancellationToken>()).Returns(dungeon);

		var result = await _handler.HandleAsync(
			new RemoveStructureElementCommand(dungeon.Id, floorId, areaId, element.Id),
			CancellationToken.None);

		result.IsSuccess.Should().BeTrue();
		dungeon.Floors.First().Areas.First().StructureElements.Should().BeEmpty();
		await _writeRepository.Received(1).UpdateAsync(dungeon, Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task HandleAsync_DungeonNotFound_ReturnsNotFound()
	{
		_writeRepository.LoadAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Dungeon?)null);

		var result = await _handler.HandleAsync(
			new RemoveStructureElementCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()),
			CancellationToken.None);

		result.IsSuccess.Should().BeFalse();
		result.ErrorCategory.Should().Be(ErrorCategory.NotFound);
	}
}
