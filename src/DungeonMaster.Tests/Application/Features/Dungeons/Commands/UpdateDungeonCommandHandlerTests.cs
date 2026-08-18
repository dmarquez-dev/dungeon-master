using DungeonMaster.Application.Common.Interfaces;
using DungeonMaster.Application.Common.Results;
using DungeonMaster.Application.Features.Dungeons.Commands.UpdateDungeon;
using DungeonMaster.Domain.Entities.Dungeon;
using DungeonMaster.Tests.Application.Common.Handlers;
using FluentAssertions;
using NSubstitute;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Commands;

public class UpdateDungeonCommandHandlerTests
{
	private readonly UnitOfWorkSubstitute _unitOfWork;
	private readonly IDungeonWriteRepository _writeRepository;
	private readonly UpdateDungeonCommandHandler _handler;

	public UpdateDungeonCommandHandlerTests()
	{
		_unitOfWork = new UnitOfWorkSubstitute();
		_writeRepository = _unitOfWork.Dungeons;
		_handler = new UpdateDungeonCommandHandler(_unitOfWork.Factory);
	}

	[Fact]
	public async Task HandleAsync_WithExistingDungeon_UpdatesAndReturnsSuccess()
	{
		// Arrange
		var dungeon = new Dungeon("Old Name", "Old Desc");
		_writeRepository.LoadAsync(dungeon.Id, Arg.Any<CancellationToken>()).Returns(dungeon);
		var command = new UpdateDungeonCommand(dungeon.Id, "New Name", "New Desc", "My Notes");

		// Act
		var result = await _handler.HandleAsync(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		dungeon.Name.Should().Be("New Name");
		dungeon.Notes.Should().Be("My Notes");
		_writeRepository.Received(1).Update(dungeon);
	}

	[Fact]
	public async Task HandleAsync_WithNonExistentId_ReturnsNotFoundFailure()
	{
		// Arrange
		_writeRepository.LoadAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Dungeon?)null);

		// Act
		var result = await _handler.HandleAsync(
			new UpdateDungeonCommand(Guid.NewGuid(), "Name", null, null), CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.ErrorCategory.Should().Be(ErrorCategory.NotFound);
	}
}
