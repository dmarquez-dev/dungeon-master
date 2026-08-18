using DungeonMaster.Application.Common.Interfaces;
using DungeonMaster.Application.Common.Results;
using DungeonMaster.Application.Features.Dungeons.Commands.RemovePlacedObject;
using DungeonMaster.Domain.Entities.Floor;
using DungeonMaster.Domain.Entities.Dungeon;
using DungeonMaster.Domain.ValueObjects;
using DungeonMaster.Tests.Application.Common.Handlers;
using FluentAssertions;
using NSubstitute;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Commands;

/// <summary>
///		Unit tests for <see cref="RemovePlacedObjectCommandHandler"/>.
/// </summary>
public class RemovePlacedObjectCommandHandlerTests
{
	private readonly UnitOfWorkSubstitute _unitOfWork;
	private readonly IDungeonWriteRepository _writeRepository;
	private readonly RemovePlacedObjectCommandHandler _handler;

	/// <summary>
	///		Initializes the test class with a substituted repository and a real handler instance.
	/// </summary>
	public RemovePlacedObjectCommandHandlerTests()
	{
		_unitOfWork = new UnitOfWorkSubstitute();
		_writeRepository = _unitOfWork.Dungeons;
		_handler = new RemovePlacedObjectCommandHandler(_unitOfWork.Factory);
	}

	/// <summary>
	///		Verifies that a valid command removes the object from the floor and returns success.
	/// </summary>
	[Fact]
	public async Task HandleAsync_ValidCommand_RemovesObjectAndReturnsSuccess()
	{
		// Arrange
		var dungeon = new Dungeon("D");
		var floor = dungeon.AddFloor("F");

		dungeon.AddPlacedObject(
			floor.Id,
			ObjectKind.Barrel,
			new CanvasPoint(80f, 80f),
			ObjectRotation.None);

		var objId = floor.Objects[0].Id;
		_writeRepository
			.LoadAsync(
				dungeon.Id,
				Arg.Any<CancellationToken>())
			.Returns(dungeon);

		var command = new RemovePlacedObjectCommand(
			dungeon.Id,
			floor.Id,
			objId);

		// Act
		var result = await _handler.HandleAsync(
			command,
			CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		floor.Objects.Should().BeEmpty();

		_writeRepository
			.Received(1)
			.Update(dungeon);
	}

	/// <summary>
	///		Verifies that a missing dungeon returns a not-found failure.
	/// </summary>
	[Fact]
	public async Task HandleAsync_DungeonNotFound_ReturnsNotFound()
	{
		// Arrange
		_writeRepository
			.LoadAsync(
				Arg.Any<Guid>(),
				Arg.Any<CancellationToken>())
			.Returns((Dungeon?)null);

		var command = new RemovePlacedObjectCommand(
			Guid.NewGuid(),
			Guid.NewGuid(),
			Guid.NewGuid());

		// Act
		var result = await _handler.HandleAsync(
			command,
			CancellationToken.None);

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
		var dungeon = new Dungeon("D", null);

		_writeRepository
			.LoadAsync(
				dungeon.Id,
				Arg.Any<CancellationToken>())
			.Returns(dungeon);

		var command = new RemovePlacedObjectCommand(
			dungeon.Id,
			Guid.NewGuid(),
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

	/// <summary>
	///		Verifies that an unknown object identifier on a valid floor reports not-found and does not write the unchanged aggregate back.
	/// </summary>
	[Fact]
	public async Task HandleAsync_WithUnknownObjectId_ReturnsNotFoundAndDoesNotSave()
	{
		// Arrange
		var dungeon = new Dungeon("D");
		var floor = dungeon.AddFloor("F");

		_writeRepository
			.LoadAsync(
				dungeon.Id,
				Arg.Any<CancellationToken>())
			.Returns(dungeon);

		var command = new RemovePlacedObjectCommand(
			dungeon.Id,
			floor.Id,
			Guid.NewGuid());

		// Act
		var result = await _handler.HandleAsync(
			command,
			CancellationToken.None);

		// Assert
		result.ErrorCategory.Should().Be(ErrorCategory.NotFound);
		floor.Objects.Should().BeEmpty();

		_writeRepository
			.DidNotReceive()
			.Update(Arg.Any<Dungeon>());
	}
}
