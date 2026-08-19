using DungeonMaster.Application.Common.Interfaces;
using DungeonMaster.Application.Common.Results;
using DungeonMaster.Application.Features.Dungeons.Commands.RemoveFloor;
using DungeonMaster.Domain.Entities.Dungeon;
using DungeonMaster.Tests.Application.Common.Handlers;
using FluentAssertions;
using NSubstitute;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Commands;

public class RemoveFloorCommandHandlerTests
{
	private readonly UnitOfWorkSubstitute _unitOfWork;
	private readonly IDungeonWriteRepository _writeRepository;
	private readonly RemoveFloorCommandHandler _handler;

	public RemoveFloorCommandHandlerTests()
	{
		_unitOfWork = new UnitOfWorkSubstitute();
		_writeRepository = _unitOfWork.Dungeons;
		_handler = new RemoveFloorCommandHandler(_unitOfWork.Factory);
	}

	[Fact]
	public async Task HandleAsync_WithExistingFloor_RemovesFloorAndReturnsSuccess()
	{
		// Arrange
		var dungeon = new Dungeon("Test Dungeon");
		var floor = dungeon.AddFloor("Entry");
		_writeRepository.LoadAsync(dungeon.Id, Arg.Any<CancellationToken>()).Returns(dungeon);
		// Act
		var result = await _handler.HandleAsync(new RemoveFloorCommand(dungeon.Id, floor.Id), CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		dungeon.Floors.Should().BeEmpty();
	}

	[Fact]
	public async Task HandleAsync_WithNonExistentDungeon_ReturnsNotFoundFailure()
	{
		// Arrange
		_writeRepository.LoadAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Dungeon?)null);

		// Act
		var result = await _handler.HandleAsync(
			new RemoveFloorCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.ErrorCategory.Should().Be(ErrorCategory.NotFound);
	}

	/// <summary>
	///		Verifies that an unknown floor identifier reports not-found and does not write the unchanged aggregate back.
	/// </summary>
	[Fact]
	public async Task HandleAsync_WithUnknownFloorId_ReturnsNotFoundAndDoesNotSave()
	{
		// Arrange
		var dungeon = new Dungeon("Test Dungeon");
		dungeon.AddFloor("Entry");

		_writeRepository
			.LoadAsync(
				dungeon.Id,
				Arg.Any<CancellationToken>())
			.Returns(dungeon);

		var command = new RemoveFloorCommand(
			dungeon.Id,
			Guid.NewGuid());

		// Act
		var result = await _handler.HandleAsync(
			command,
			CancellationToken.None);

		// Assert
		result.ErrorCategory.Should().Be(ErrorCategory.NotFound);

		_writeRepository
			.DidNotReceive()
			.Update(Arg.Any<Dungeon>());
	}
}
