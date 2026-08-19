using DungeonMaster.Application.Common.Interfaces;
using DungeonMaster.Application.Common.Results;
using DungeonMaster.Application.Features.Dungeons.Commands.UpdatePlacedObject;
using DungeonMaster.Domain.Entities.Floor;
using DungeonMaster.Domain.Entities.Dungeon;
using DungeonMaster.Domain.ValueObjects;
using DungeonMaster.Tests.Application.Common.Handlers;
using FluentAssertions;
using NSubstitute;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Commands;

/// <summary>
///		Unit tests for <see cref="UpdatePlacedObjectCommandHandler"/>.
/// </summary>
public class UpdatePlacedObjectCommandHandlerTests
{
	private readonly UnitOfWorkSubstitute _unitOfWork;
	private readonly IDungeonWriteRepository _writeRepository;
	private readonly UpdatePlacedObjectCommandHandler _handler;

	/// <summary>
	///		Initializes the test class with a substituted repository and a real handler instance.
	/// </summary>
	public UpdatePlacedObjectCommandHandlerTests()
	{
		_unitOfWork = new UnitOfWorkSubstitute();
		_writeRepository = _unitOfWork.Dungeons;
		_handler = new UpdatePlacedObjectCommandHandler(_unitOfWork.Factory);
	}

	/// <summary>
	///		Verifies that a valid command updates the object's position and rotation, then returns success.
	/// </summary>
	[Fact]
	public async Task HandleAsync_ValidCommand_UpdatesObjectAndReturnsSuccess()
	{
		// Arrange
		var dungeon = new Dungeon("D");
		var floor = dungeon.AddFloor("F");

		dungeon.AddPlacedObject(
			floor.Id,
			ObjectKind.Throne,
			new CanvasPoint(0f, 0f),
			ObjectRotation.None);

		var obj = floor.Objects[0];
		_writeRepository
			.LoadAsync(
				dungeon.Id,
				Arg.Any<CancellationToken>())
			.Returns(dungeon);

		var command = new UpdatePlacedObjectCommand(
			dungeon.Id,
			floor.Id,
			obj.Id,
			new CanvasPoint(200f, 200f),
			ObjectRotation.CW90);

		// Act
		var result = await _handler.HandleAsync(
			command,
			CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		obj.Position.Should().Be(new CanvasPoint(200f, 200f));
		obj.Rotation.Should().Be(ObjectRotation.CW90);
		obj.Scale.Should().BeApproximately(ObjectScale.Default, 0.001f);

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

		var command = new UpdatePlacedObjectCommand(
			Guid.NewGuid(),
			Guid.NewGuid(),
			Guid.NewGuid(),
			new CanvasPoint(40f, 40f),
			ObjectRotation.None);

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

		var command = new UpdatePlacedObjectCommand(
			dungeon.Id,
			Guid.NewGuid(),
			Guid.NewGuid(),
			new CanvasPoint(0f, 0f),
			ObjectRotation.None);

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
	///		Verifies that an unknown object identifier on a valid floor reports not-found, leaves existing objects untouched, and does not write the unchanged aggregate back.
	/// </summary>
	[Fact]
	public async Task HandleAsync_WithUnknownObjectId_ReturnsNotFoundAndDoesNotSave()
	{
		// Arrange
		var dungeon = new Dungeon("D");
		var floor = dungeon.AddFloor("F");

		dungeon.AddPlacedObject(
			floor.Id,
			ObjectKind.Chair,
			new CanvasPoint(10f, 10f),
			ObjectRotation.None);

		var theObj = floor.Objects.Should().ContainSingle().Which;
		var originalPos = theObj.Position;
		_writeRepository
			.LoadAsync(
				dungeon.Id,
				Arg.Any<CancellationToken>())
			.Returns(dungeon);

		var command = new UpdatePlacedObjectCommand(
			dungeon.Id,
			floor.Id,
			Guid.NewGuid(),
			new CanvasPoint(999f, 999f),
			ObjectRotation.CW180);

		// Act
		var result = await _handler.HandleAsync(
			command,
			CancellationToken.None);

		// Assert
		result.ErrorCategory.Should().Be(ErrorCategory.NotFound);
		theObj.Position.Should().Be(originalPos);

		_writeRepository
			.DidNotReceive()
			.Update(Arg.Any<Dungeon>());
	}

	/// <summary>
	///		Verifies that a custom scale specified in the command is stored on the updated placed object.
	/// </summary>
	[Fact]
	public async Task HandleAsync_WithCustomScale_StoresScaleOnObject()
	{
		// Arrange
		var dungeon = new Dungeon("D");
		var floor = dungeon.AddFloor("F");

		dungeon.AddPlacedObject(
			floor.Id,
			ObjectKind.Chair,
			new CanvasPoint(10f, 10f),
			ObjectRotation.None);

		var obj = floor.Objects.Should().ContainSingle().Which;
		_writeRepository
			.LoadAsync(
				dungeon.Id,
				Arg.Any<CancellationToken>())
			.Returns(dungeon);

		var command = new UpdatePlacedObjectCommand(
			dungeon.Id,
			floor.Id,
			obj.Id,
			obj.Position,
			obj.Rotation,
			NewScale: 1.5f);

		// Act
		await _handler.HandleAsync(
			command,
			CancellationToken.None);

		// Assert
		obj.Scale.Should().BeApproximately(1.5f, 0.001f);
	}
}
