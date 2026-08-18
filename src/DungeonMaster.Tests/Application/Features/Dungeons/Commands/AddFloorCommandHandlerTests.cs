using DungeonMaster.Application.Common.Results;
using DungeonMaster.Application.Features.Dungeons;
using DungeonMaster.Application.Features.Dungeons.Commands.AddFloor;
using DungeonMaster.Domain.Entities.Dungeon;
using FluentAssertions;
using NSubstitute;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Commands;

public class AddFloorCommandHandlerTests
{
	private readonly IDungeonWriteRepository _writeRepository;
	private readonly AddFloorCommandHandler _handler;

	public AddFloorCommandHandlerTests()
	{
		_writeRepository = Substitute.For<IDungeonWriteRepository>();
		_handler = new AddFloorCommandHandler(_writeRepository);
	}

	[Fact]
	public async Task HandleAsync_WithExistingDungeon_AddsFloorAndReturnsId()
	{
		// Arrange
		var dungeon = new Dungeon("Test Dungeon");
		_writeRepository.LoadAsync(dungeon.Id, Arg.Any<CancellationToken>()).Returns(dungeon);
		_writeRepository.UpdateAsync(Arg.Any<Dungeon>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

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
