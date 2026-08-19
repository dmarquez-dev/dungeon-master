using DungeonMaster.Application.Common.Interfaces;
using DungeonMaster.Application.Common.Results;
using DungeonMaster.Application.Features.Dungeons.Commands.UpdateAreaOutline;
using DungeonMaster.Domain.Entities.Dungeon;
using DungeonMaster.Domain.ValueObjects;
using DungeonMaster.Tests.Application.Common.Handlers;
using FluentAssertions;
using NSubstitute;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Commands;

public class UpdateAreaOutlineCommandHandlerTests
{
	private readonly UnitOfWorkSubstitute _unitOfWork;
	private readonly IDungeonWriteRepository _writeRepository;
	private readonly UpdateAreaOutlineCommandHandler _handler;

	public UpdateAreaOutlineCommandHandlerTests()
	{
		_unitOfWork = new UnitOfWorkSubstitute();
		_writeRepository = _unitOfWork.Dungeons;
		_handler = new UpdateAreaOutlineCommandHandler(_unitOfWork.Factory);
	}

	[Fact]
	public async Task HandleAsync_WithExistingArea_UpdatesOutlineAndReturnsSuccess()
	{
		// Arrange
		var dungeon = new Dungeon("Test Dungeon");
		var floor = dungeon.AddFloor("Ground");
		dungeon.AddArea(floor.Id, "Room", null);
		_writeRepository
			.LoadAsync(dungeon.Id, Arg.Any<CancellationToken>())
			.Returns(dungeon);

		var area = floor.Areas[0];
		var outline = new List<PathSegment>
		{
			new(PathSegmentKind.Line, new TilePoint(4, 3), null, null),
			new(PathSegmentKind.Line, new TilePoint(4, 6), null, null),
			new(PathSegmentKind.Line, new TilePoint(2, 3), null, null),
		};

		var command = new UpdateAreaOutlineCommand(dungeon.Id, floor.Id, area.Id, outline);

		// Act
		var result = await _handler.HandleAsync(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		area.Outline.Should().HaveCount(3);
	}

	[Fact]
	public async Task HandleAsync_WithNonExistentDungeon_ReturnsNotFoundFailure()
	{
		// Arrange
		_writeRepository
			.LoadAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
			.Returns((Dungeon?)null);

		var command = new UpdateAreaOutlineCommand(
			Guid.NewGuid(),
			Guid.NewGuid(),
			Guid.NewGuid(),
			[]);

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

		var command = new UpdateAreaOutlineCommand(
			dungeon.Id,
			floor.Id,
			Guid.NewGuid(),
			[]);

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
