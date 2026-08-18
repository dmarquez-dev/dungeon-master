using DungeonMaster.Application.Common.Results;
using DungeonMaster.Application.Features.Dungeons;
using DungeonMaster.Application.Features.Dungeons.Commands.UpdatePlacedObject;
using DungeonMaster.Domain.Entities.Floor;
using DungeonMaster.Domain.Entities.Dungeon;
using DungeonMaster.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Commands;

/// <summary>
///		Unit tests for <see cref="UpdatePlacedObjectCommandHandler"/>.
/// </summary>
public class UpdatePlacedObjectCommandHandlerTests
{
	private readonly IDungeonWriteRepository _writeRepository;
	private readonly UpdatePlacedObjectCommandHandler _handler;

	/// <summary>
	///		Initializes the test class with a substituted repository and a real handler instance.
	/// </summary>
	public UpdatePlacedObjectCommandHandlerTests()
	{
		_writeRepository = Substitute.For<IDungeonWriteRepository>();
		_handler = new UpdatePlacedObjectCommandHandler(_writeRepository);
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
			.LoadAsync(dungeon.Id, Arg.Any<CancellationToken>())
			.Returns(dungeon);

		var command = new UpdatePlacedObjectCommand(
			dungeon.Id,
			floor.Id,
			obj.Id,
			new CanvasPoint(200f, 200f),
			ObjectRotation.CW90);

		// Act
		var result = await _handler.HandleAsync(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		obj.Position.Should().Be(new CanvasPoint(200f, 200f));
		obj.Rotation.Should().Be(ObjectRotation.CW90);
		obj.Scale.Should().BeApproximately(ObjectScale.Default, 0.001f);

		await _writeRepository
			.Received(1)
			.UpdateAsync(dungeon, Arg.Any<CancellationToken>());
	}

	/// <summary>
	///		Verifies that a missing dungeon returns a not-found failure.
	/// </summary>
	[Fact]
	public async Task HandleAsync_DungeonNotFound_ReturnsNotFound()
	{
		// Arrange
		_writeRepository
			.LoadAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
			.Returns((Dungeon?)null);

		var command = new UpdatePlacedObjectCommand(
			Guid.NewGuid(),
			Guid.NewGuid(),
			Guid.NewGuid(),
			new CanvasPoint(40f, 40f),
			ObjectRotation.None);

		// Act
		var result = await _handler.HandleAsync(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.ErrorCategory.Should().Be(ErrorCategory.NotFound);
	}

	/// <summary>
	///		Verifies that an unknown floor ID is a silent no-op at the domain level;
	///		the handler still persists the aggregate and returns success.
	/// </summary>
	[Fact]
	public async Task HandleAsync_FloorNotFound_ReturnsSuccessWithNoChange()
	{
		// Arrange
		var dungeon = new Dungeon("D", null);
		_writeRepository
			.LoadAsync(dungeon.Id, Arg.Any<CancellationToken>())
			.Returns(dungeon);

		var command = new UpdatePlacedObjectCommand(
			dungeon.Id,
			Guid.NewGuid(),
			Guid.NewGuid(),
			new CanvasPoint(0f, 0f),
			ObjectRotation.None);

		// Act
		var result = await _handler.HandleAsync(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();

		await _writeRepository
			.Received(1)
			.UpdateAsync(dungeon, Arg.Any<CancellationToken>());
	}

	/// <summary>
	///		Verifies that an unknown object ID on a valid floor is a silent no-op at the domain level;
	///		the handler still persists the aggregate and returns success, leaving existing objects untouched.
	/// </summary>
	[Fact]
	public async Task HandleAsync_ObjectNotFound_ReturnsSuccessWithNoChange()
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
			.LoadAsync(dungeon.Id, Arg.Any<CancellationToken>())
			.Returns(dungeon);

		var command = new UpdatePlacedObjectCommand(
			dungeon.Id,
			floor.Id,
			Guid.NewGuid(),
			new CanvasPoint(999f, 999f),
			ObjectRotation.CW180);

		// Act
		var result = await _handler.HandleAsync(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		theObj.Position.Should().Be(originalPos);

		await _writeRepository
			.Received(1)
			.UpdateAsync(dungeon, Arg.Any<CancellationToken>());
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
			.LoadAsync(dungeon.Id, Arg.Any<CancellationToken>())
			.Returns(dungeon);

		var command = new UpdatePlacedObjectCommand(
			dungeon.Id,
			floor.Id,
			obj.Id,
			obj.Position,
			obj.Rotation,
			NewScale: 1.5f);

		// Act
		await _handler.HandleAsync(command, CancellationToken.None);

		// Assert
		obj.Scale.Should().BeApproximately(1.5f, 0.001f);
	}
}
