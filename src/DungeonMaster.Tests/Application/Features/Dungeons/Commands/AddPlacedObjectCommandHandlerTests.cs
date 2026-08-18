using DungeonMaster.Application.Common.Interfaces;
using DungeonMaster.Application.Common.Results;
using DungeonMaster.Application.Features.Dungeons.Commands.AddPlacedObject;
using DungeonMaster.Domain.Entities.Floor;
using DungeonMaster.Domain.Entities.Dungeon;
using DungeonMaster.Domain.ValueObjects;
using DungeonMaster.Tests.Application.Common.Handlers;
using FluentAssertions;
using NSubstitute;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Commands;

/// <summary>
///		Unit tests for <see cref="AddPlacedObjectCommandHandler"/>.
/// </summary>
public class AddPlacedObjectCommandHandlerTests
{
	private readonly UnitOfWorkSubstitute _unitOfWork;
	private readonly IDungeonWriteRepository _writeRepository;
	private readonly AddPlacedObjectCommandHandler _handler;

	/// <summary>
	///		Initializes the test class with a substituted repository and a real handler instance.
	/// </summary>
	public AddPlacedObjectCommandHandlerTests()
	{
		_unitOfWork = new UnitOfWorkSubstitute();
		_writeRepository = _unitOfWork.Dungeons;
		_handler = new AddPlacedObjectCommandHandler(_unitOfWork.Factory);
	}

	private static Dungeon MakeDungeon(out Guid floorId)
	{
		var dungeon = new Dungeon("D");
		var floor = dungeon.AddFloor("F");

		floorId = floor.Id;
		return dungeon;
	}

	/// <summary>
	///		Verifies that a valid command produces a successful result containing a non-empty Guid,
	///		and that <see cref="IDungeonWriteRepository.Update"/> is called exactly once.
	/// </summary>
	[Fact]
	public async Task HandleAsync_ValidCommand_ReturnsSuccessWithId()
	{
		// Arrange
		var dungeon = MakeDungeon(out var floorId);
		_writeRepository
			.LoadAsync(dungeon.Id, Arg.Any<CancellationToken>())
			.Returns(dungeon);

		var command = new AddPlacedObjectCommand(
			dungeon.Id,
			floorId,
			ObjectKind.Chair,
			new CanvasPoint(40f, 40f),
			ObjectRotation.None);

		// Act
		var result = await _handler.HandleAsync(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().NotBeEmpty();

		_writeRepository
			.Received(1)
			.Update(dungeon);
	}

	/// <summary>
	///		Verifies that the object is actually added to the floor after the command succeeds.
	/// </summary>
	[Fact]
	public async Task HandleAsync_ValidCommand_AddsObjectToFloor()
	{
		// Arrange
		var dungeon = MakeDungeon(out var floorId);
		var floor = dungeon.Floors.First();
		_writeRepository
			.LoadAsync(dungeon.Id, Arg.Any<CancellationToken>())
			.Returns(dungeon);

		var command = new AddPlacedObjectCommand(
			dungeon.Id,
			floorId,
			ObjectKind.Chair,
			new CanvasPoint(120f, 160f),
			ObjectRotation.CW90);

		// Act
		var result = await _handler.HandleAsync(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		var addedObj = floor.Objects.Should().ContainSingle().Which;
		addedObj.Id.Should().Be(result.Value);
		addedObj.Kind.Should().Be(ObjectKind.Chair);
		addedObj.Scale.Should().BeApproximately(ObjectScale.Default, 0.001f);
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

		var command = new AddPlacedObjectCommand(
			Guid.NewGuid(),
			Guid.NewGuid(),
			ObjectKind.Chair,
			new CanvasPoint(40f, 40f),
			ObjectRotation.None);

		// Act
		var result = await _handler.HandleAsync(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.ErrorCategory.Should().Be(ErrorCategory.NotFound);
	}

	/// <summary>
	///		Verifies that a missing floor reports not-found, names the floor in the message, and does not write the unchanged aggregate back.
	/// </summary>
	[Fact]
	public async Task HandleAsync_WithUnknownFloorId_ReturnsNotFoundNamingTheFloor()
	{
		// Arrange
		var dungeon = MakeDungeon(out _);
		var floorId = Guid.NewGuid();

		_writeRepository
			.LoadAsync(
				dungeon.Id,
				Arg.Any<CancellationToken>())
			.Returns(dungeon);

		var command = new AddPlacedObjectCommand(
			dungeon.Id,
			floorId,
			ObjectKind.Chair,
			new CanvasPoint(40f, 40f),
			ObjectRotation.None);

		// Act
		var result = await _handler.HandleAsync(
			command,
			CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.ErrorCategory.Should().Be(ErrorCategory.NotFound);
		result.Error.Should().Contain(floorId.ToString());

		_writeRepository
			.DidNotReceive()
			.Update(Arg.Any<Dungeon>());
	}

	/// <summary>
	///		Verifies that a custom scale specified in the command is stored on the placed object.
	/// </summary>
	[Fact]
	public async Task HandleAsync_WithCustomScale_StoresScaleOnObject()
	{
		// Arrange
		var dungeon = MakeDungeon(out var floorId);
		var floor = dungeon.Floors.First();
		_writeRepository
			.LoadAsync(dungeon.Id, Arg.Any<CancellationToken>())
			.Returns(dungeon);

		var command = new AddPlacedObjectCommand(
			dungeon.Id,
			floorId,
			ObjectKind.Chair,
			new CanvasPoint(40f, 40f),
			ObjectRotation.None,
			Scale: 2.0f);

		// Act
		await _handler.HandleAsync(command, CancellationToken.None);

		// Assert
		floor.Objects.Should().ContainSingle().Which.Scale.Should().BeApproximately(2.0f, 0.001f);
	}
}
