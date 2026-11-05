using DungeonMaster.Application.Common.Interfaces;
using DungeonMaster.Application.Common.Results;
using DungeonMaster.Application.Features.Dungeons.Commands.AddPlacedObject;
using DungeonMaster.Domain.Entities.Floors;
using DungeonMaster.Domain.Entities.Dungeons;
using DungeonMaster.Domain.ValueObjects;
using DungeonMaster.Tests.Application.Common.Handlers;
using FluentAssertions;
using NSubstitute;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Commands;

/// <summary>
///		Unit tests for <see cref="AddPlacedObjectCommandHandler"/>.
/// </summary>
public sealed class AddPlacedObjectCommandHandlerTests : DungeonNotFoundHandlerTestBase<AddPlacedObjectCommandHandler>
{
	/// <inheritdoc />
	protected override AddPlacedObjectCommandHandler CreateHandler(IUnitOfWorkFactory factory) =>
		new(factory);

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
		WriteRepository
			.LoadAsync(dungeon.Id, Arg.Any<CancellationToken>())
			.Returns(dungeon);

		var command = new AddPlacedObjectCommand(
			dungeon.Id,
			floorId,
			ObjectKind.Chair,
			new CanvasPoint(40f, 40f),
			ObjectRotation.None);

		// Act
		var result = await Handler.HandleAsync(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().NotBeEmpty();

		WriteRepository
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
		WriteRepository
			.LoadAsync(dungeon.Id, Arg.Any<CancellationToken>())
			.Returns(dungeon);

		var command = new AddPlacedObjectCommand(
			dungeon.Id,
			floorId,
			ObjectKind.Chair,
			new CanvasPoint(120f, 160f),
			ObjectRotation.Clockwise90);

		// Act
		var result = await Handler.HandleAsync(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		var addedObj = floor.Objects.Should().ContainSingle().Which;
		addedObj.Id.Should().Be(result.Value);
		addedObj.Kind.Should().Be(ObjectKind.Chair);
		addedObj.Scale.Should().BeApproximately(ObjectScale.Default, 0.001f);
	}

	/// <summary>
	///		Dispatches this leaf's command for a dungeon identifier the repository will not resolve.
	/// </summary>
	/// <returns>
	///		The success flag and error category of the result the handler returned.
	/// </returns>
	protected override async Task<(bool IsSuccess, ErrorCategory? ErrorCategory)> InvokeWithMissingDungeonAsync()
	{
		var result = await Handler.HandleAsync(
			new AddPlacedObjectCommand(
				Guid.NewGuid(),
				Guid.NewGuid(),
				ObjectKind.Chair,
				new CanvasPoint(40f, 40f),
				ObjectRotation.None),
			CancellationToken.None);

		return (result.IsSuccess, result.ErrorCategory);
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

		WriteRepository
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
		var result = await Handler.HandleAsync(
			command,
			CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.ErrorCategory.Should().Be(ErrorCategory.NotFound);
		result.Error.Should().Contain(floorId.ToString());

		WriteRepository
			.DidNotReceive()
			.Update(Arg.Any<Dungeon>());

		await UnitOfWork
			.DidNotReceive()
			.CommitAsync(Arg.Any<CancellationToken>());
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
		WriteRepository
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
		await Handler.HandleAsync(command, CancellationToken.None);

		// Assert
		floor.Objects.Should().ContainSingle().Which.Scale.Should().BeApproximately(2.0f, 0.001f);
	}
	/// <summary>
	///		Dispatches this leaf's command with the supplied token, for the base-level tests that assert which token crossed each port.
	/// </summary>
	/// <param name="ct">
	///		The token to hand to the handler.
	/// </param>
	/// <returns>
	///		A task that completes when the handler returns.
	/// </returns>
	protected override Task InvokeWithTokenAsync(CancellationToken ct) =>
		Handler.HandleAsync(
			new AddPlacedObjectCommand(
				Guid.NewGuid(),
				Guid.NewGuid(),
				ObjectKind.Chair,
				new CanvasPoint(40f, 40f),
				ObjectRotation.None),
			ct);
}
