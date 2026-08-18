using DungeonMaster.Application.Common.Results;
using DungeonMaster.Application.Features.Dungeons;
using DungeonMaster.Application.Features.Dungeons.Commands.UpdateArea;
using DungeonMaster.Domain.Entities.Dungeon;
using FluentAssertions;
using NSubstitute;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Commands;

public class UpdateAreaCommandHandlerTests
{
	private readonly IDungeonWriteRepository _writeRepository;
	private readonly UpdateAreaCommandHandler _handler;

	public UpdateAreaCommandHandlerTests()
	{
		_writeRepository = Substitute.For<IDungeonWriteRepository>();
		_handler = new UpdateAreaCommandHandler(_writeRepository);
	}

	[Fact]
	public async Task HandleAsync_WithExistingArea_UpdatesAreaAndReturnsSuccess()
	{
		// Arrange
		var dungeon = new Dungeon("Test Dungeon");
		var floor = dungeon.AddFloor("Ground");
		dungeon.AddArea(floor.Id, "Old Name", null);
		_writeRepository
			.LoadAsync(dungeon.Id, Arg.Any<CancellationToken>())
			.Returns(dungeon);
		_writeRepository
			.UpdateAsync(Arg.Any<Dungeon>(), Arg.Any<CancellationToken>())
			.Returns(Task.CompletedTask);

		var area = floor.Areas[0];
		var command = new UpdateAreaCommand(dungeon.Id, floor.Id, area.Id, "New Name", "New Desc", "My Notes");

		// Act
		var result = await _handler.HandleAsync(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		area.Name.Should().Be("New Name");
		area.Description.Should().Be("New Desc");
	}

	[Fact]
	public async Task HandleAsync_WithNonExistentDungeon_ReturnsNotFoundFailure()
	{
		// Arrange
		_writeRepository
			.LoadAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
			.Returns((Dungeon?)null);

		var command = new UpdateAreaCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Name", null, null);

		// Act
		var result = await _handler.HandleAsync(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.ErrorCategory.Should().Be(ErrorCategory.NotFound);
	}

	/// <summary>
	///		Verifies that an unknown area identifier reports not-found and does not write the unchanged aggregate back.
	/// </summary>
	[Fact]
	public async Task HandleAsync_WithUnknownAreaId_ReturnsNotFoundAndDoesNotSave()
	{
		// Arrange
		var dungeon = new Dungeon("Test Dungeon");
		var floor = dungeon.AddFloor("Ground");

		_writeRepository
			.LoadAsync(
				dungeon.Id,
				Arg.Any<CancellationToken>())
			.Returns(dungeon);

		var command = new UpdateAreaCommand(
			dungeon.Id,
			floor.Id,
			Guid.NewGuid(),
			"New Name",
			null,
			null);

		// Act
		var result = await _handler.HandleAsync(
			command,
			CancellationToken.None);

		// Assert
		result.ErrorCategory.Should().Be(ErrorCategory.NotFound);

		await _writeRepository
			.DidNotReceive()
			.UpdateAsync(
				Arg.Any<Dungeon>(),
				Arg.Any<CancellationToken>());
	}
}
