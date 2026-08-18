using DungeonMaster.Application.Common.Results;
using DungeonMaster.Application.Features.Dungeons;
using DungeonMaster.Application.Features.Dungeons.Commands.AddArea;
using DungeonMaster.Domain.Entities.Dungeon;
using FluentAssertions;
using NSubstitute;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Commands;

public class AddAreaCommandHandlerTests
{
	private readonly IDungeonWriteRepository _writeRepository;
	private readonly AddAreaCommandHandler _handler;

	public AddAreaCommandHandlerTests()
	{
		_writeRepository = Substitute.For<IDungeonWriteRepository>();
		_handler = new AddAreaCommandHandler(_writeRepository);
	}

	[Fact]
	public async Task HandleAsync_WithExistingDungeonAndFloor_AddsAreaAndReturnsId()
	{
		// Arrange
		var dungeon = new Dungeon("Test Dungeon");
		var floor = dungeon.AddFloor("Ground");
		_writeRepository
			.LoadAsync(dungeon.Id, Arg.Any<CancellationToken>())
			.Returns(dungeon);
		_writeRepository
			.UpdateAsync(Arg.Any<Dungeon>(), Arg.Any<CancellationToken>())
			.Returns(Task.CompletedTask);

		var command = new AddAreaCommand(dungeon.Id, floor.Id, "Throne Room", null);

		// Act
		var result = await _handler.HandleAsync(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().NotBeEmpty();
		floor.Areas.Should().HaveCount(1);
		floor.Areas[0].Name.Should().Be("Throne Room");
	}

	[Fact]
	public async Task HandleAsync_WithNonExistentDungeon_ReturnsNotFoundFailure()
	{
		// Arrange
		_writeRepository
			.LoadAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
			.Returns((Dungeon?)null);

		var command = new AddAreaCommand(Guid.NewGuid(), Guid.NewGuid(), "Any Area", null);

		// Act
		var result = await _handler.HandleAsync(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.ErrorCategory.Should().Be(ErrorCategory.NotFound);
	}
}
