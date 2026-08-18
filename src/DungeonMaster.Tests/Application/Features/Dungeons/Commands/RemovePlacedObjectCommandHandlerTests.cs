using DungeonMaster.Application.Common.Results;
using DungeonMaster.Application.Features.Dungeons;
using DungeonMaster.Application.Features.Dungeons.Commands.RemovePlacedObject;
using DungeonMaster.Domain.Entities.Floor;
using DungeonMaster.Domain.Entities.Dungeon;
using DungeonMaster.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Commands;

/// <summary>
///		Unit tests for <see cref="RemovePlacedObjectCommandHandler"/>.
/// </summary>
public class RemovePlacedObjectCommandHandlerTests
{
	private readonly IDungeonWriteRepository _writeRepository;
	private readonly RemovePlacedObjectCommandHandler _handler;

	/// <summary>
	///		Initializes the test class with a substituted repository and a real handler instance.
	/// </summary>
	public RemovePlacedObjectCommandHandlerTests()
	{
		_writeRepository = Substitute.For<IDungeonWriteRepository>();
		_handler = new RemovePlacedObjectCommandHandler(_writeRepository);
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
			.LoadAsync(dungeon.Id, Arg.Any<CancellationToken>())
			.Returns(dungeon);

		var command = new RemovePlacedObjectCommand(
			dungeon.Id,
			floor.Id,
			objId);

		// Act
		var result = await _handler.HandleAsync(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		floor.Objects.Should().BeEmpty();

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

		var command = new RemovePlacedObjectCommand(
			Guid.NewGuid(),
			Guid.NewGuid(),
			Guid.NewGuid());

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

		var command = new RemovePlacedObjectCommand(
			dungeon.Id,
			Guid.NewGuid(),
			Guid.NewGuid());

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
	///		the handler still persists the aggregate and returns success.
	/// </summary>
	[Fact]
	public async Task HandleAsync_ObjectNotFound_ReturnsSuccessWithNoChange()
	{
		// Arrange
		var dungeon = new Dungeon("D");
		var floor = dungeon.AddFloor("F");

		_writeRepository
			.LoadAsync(dungeon.Id, Arg.Any<CancellationToken>())
			.Returns(dungeon);

		var command = new RemovePlacedObjectCommand(
			dungeon.Id,
			floor.Id,
			Guid.NewGuid());

		// Act
		var result = await _handler.HandleAsync(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		floor.Objects.Should().BeEmpty();

		await _writeRepository
			.Received(1)
			.UpdateAsync(dungeon, Arg.Any<CancellationToken>());
	}
}
