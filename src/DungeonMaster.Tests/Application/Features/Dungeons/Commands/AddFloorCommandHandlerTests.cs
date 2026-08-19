using DungeonMaster.Application.Common.Interfaces;
using DungeonMaster.Application.Common.Results;
using DungeonMaster.Application.Features.Dungeons.Commands.AddFloor;
using DungeonMaster.Domain.Entities.Dungeon;
using DungeonMaster.Tests.Application.Common.Handlers;
using FluentAssertions;
using NSubstitute;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Commands;

public class AddFloorCommandHandlerTests
{
	private readonly UnitOfWorkSubstitute _unitOfWork;
	private readonly IDungeonWriteRepository _writeRepository;
	private readonly AddFloorCommandHandler _handler;

	public AddFloorCommandHandlerTests()
	{
		_unitOfWork = new UnitOfWorkSubstitute();
		_writeRepository = _unitOfWork.Dungeons;
		_handler = new AddFloorCommandHandler(_unitOfWork.Factory);
	}

	[Fact]
	public async Task HandleAsync_WithExistingDungeon_AddsFloorAndReturnsId()
	{
		// Arrange
		var dungeon = new Dungeon("Test Dungeon");
		_writeRepository.LoadAsync(dungeon.Id, Arg.Any<CancellationToken>()).Returns(dungeon);
		// Act
		var result = await _handler.HandleAsync(new AddFloorCommand(dungeon.Id, "Entry"), CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().NotBeEmpty();
		dungeon.Floors.Should().HaveCount(1);
	}

	[Fact]
	public async Task HandleAsync_WithNonExistentId_ReturnsNotFoundFailure()
	{
		// Arrange
		_writeRepository.LoadAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Dungeon?)null);

		// Act
		var result = await _handler.HandleAsync(new AddFloorCommand(Guid.NewGuid(), null), CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.ErrorCategory.Should().Be(ErrorCategory.NotFound);
	}
}
