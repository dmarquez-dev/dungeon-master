using DungeonMaster.Application.Common.Results;
using DungeonMaster.Application.Features.Dungeons;
using DungeonMaster.Application.Features.Dungeons.Commands.AddStructureElement;
using DungeonMaster.Domain.Entities.Area;
using DungeonMaster.Domain.Entities.Dungeon;
using DungeonMaster.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Commands;

public class AddStructureElementCommandHandlerTests
{
	private readonly IDungeonWriteRepository _writeRepository;
	private readonly AddStructureElementCommandHandler _handler;

	public AddStructureElementCommandHandlerTests()
	{
		_writeRepository = Substitute.For<IDungeonWriteRepository>();
		_handler = new AddStructureElementCommandHandler(_writeRepository);
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
	public async Task HandleAsync_ValidCommand_ReturnsSuccessWithId()
	{
		var dungeon = MakeDungeon(out var floorId, out var areaId);
		_writeRepository.LoadAsync(dungeon.Id, Arg.Any<CancellationToken>()).Returns(dungeon);

		var result = await _handler.HandleAsync(
			new AddStructureElementCommand(dungeon.Id, floorId, areaId,
				new VertexPoint(2, 2), new VertexPoint(2, 2), StructureKind.RoundColumn),
			CancellationToken.None);

		result.IsSuccess.Should().BeTrue();
		result.Value.Should().NotBeEmpty();
		dungeon.Floors.First().Areas.First().StructureElements.Should().ContainSingle();
		await _writeRepository.Received(1).UpdateAsync(dungeon, Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task HandleAsync_DungeonNotFound_ReturnsNotFound()
	{
		_writeRepository.LoadAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Dungeon?)null);

		var result = await _handler.HandleAsync(
			new AddStructureElementCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
				new VertexPoint(0, 0), new VertexPoint(0, 0), StructureKind.SquareColumn),
			CancellationToken.None);

		result.IsSuccess.Should().BeFalse();
		result.ErrorCategory.Should().Be(ErrorCategory.NotFound);
	}

	[Fact]
	public async Task HandleAsync_FloorNotFound_ReturnsValidationFailure()
	{
		var dungeon = MakeDungeon(out _, out var areaId);
		_writeRepository.LoadAsync(dungeon.Id, Arg.Any<CancellationToken>()).Returns(dungeon);

		var result = await _handler.HandleAsync(
			new AddStructureElementCommand(dungeon.Id, Guid.NewGuid(), areaId,
				new VertexPoint(0, 0), new VertexPoint(1, 0), StructureKind.WallStub),
			CancellationToken.None);

		result.IsSuccess.Should().BeFalse();
		result.ErrorCategory.Should().Be(ErrorCategory.Validation);
	}

	[Fact]
	public async Task HandleAsync_EdgeCenterCommand_ReturnsSuccessWithId()
	{
		var dungeon = MakeDungeon(out var floorId, out var areaId);
		_writeRepository.LoadAsync(dungeon.Id, Arg.Any<CancellationToken>()).Returns(dungeon);

		var result = await _handler.HandleAsync(
			new AddStructureElementCommand(
				dungeon.Id, floorId, areaId,
				From: null, To: null,
				Kind: StructureKind.WoodDoor,
				EdgeAnchor: new EdgePoint(1, 1, EdgeSide.N)),
			CancellationToken.None);

		result.IsSuccess.Should().BeTrue();
		result.Value.Should().NotBeEmpty();
		dungeon.Floors.First().Areas.First().StructureElements.Should().ContainSingle(
			e => e.EdgeAnchor == new EdgePoint(1, 1, EdgeSide.N));
		await _writeRepository.Received(1).UpdateAsync(dungeon, Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task HandleAsync_NoFromToAndNoEdgeAnchor_ReturnsValidationFailure()
	{
		var dungeon = MakeDungeon(out var floorId, out var areaId);
		_writeRepository.LoadAsync(dungeon.Id, Arg.Any<CancellationToken>()).Returns(dungeon);

		var result = await _handler.HandleAsync(
			new AddStructureElementCommand(
				dungeon.Id, floorId, areaId,
				From: null, To: null,
				Kind: StructureKind.RoundColumn,
				EdgeAnchor: null),
			CancellationToken.None);

		result.IsSuccess.Should().BeFalse();
		result.ErrorCategory.Should().Be(ErrorCategory.Validation);
	}
}
