using DungeonMaster.Application.Common.Interfaces;
using DungeonMaster.Application.Common.Results;
using DungeonMaster.Application.Features.Dungeons.Commands.RemoveFloor;
using DungeonMaster.Domain.Entities.Dungeons;
using DungeonMaster.Tests.Application.Common.Handlers;
using FluentAssertions;
using NSubstitute;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Commands;

public sealed class RemoveFloorCommandHandlerTests : DungeonNotFoundHandlerTestBase<RemoveFloorCommandHandler>
{
	/// <inheritdoc />
	protected override RemoveFloorCommandHandler CreateHandler(IUnitOfWorkFactory factory) =>
		new(factory);

	[Fact]
	public async Task HandleAsync_WithExistingFloor_RemovesFloorAndReturnsSuccess()
	{
		// Arrange
		var dungeon = new Dungeon("Test Dungeon");
		var floor = dungeon.AddFloor("Entry");
		WriteRepository.LoadAsync(dungeon.Id, Arg.Any<CancellationToken>()).Returns(dungeon);
		// Act
		var result = await Handler.HandleAsync(new RemoveFloorCommand(dungeon.Id, floor.Id), CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		dungeon.Floors.Should().BeEmpty();
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
			new RemoveFloorCommand(Guid.NewGuid(), Guid.NewGuid()),
			CancellationToken.None);

		return (result.IsSuccess, result.ErrorCategory);
	}

	/// <summary>
	///		Verifies that an unknown floor identifier reports not-found and does not write the unchanged aggregate back.
	/// </summary>
	[Fact]
	public async Task HandleAsync_WithUnknownFloorId_ReturnsNotFoundAndDoesNotSave()
	{
		// Arrange
		var dungeon = new Dungeon("Test Dungeon");
		dungeon.AddFloor("Entry");

		WriteRepository
			.LoadAsync(
				dungeon.Id,
				Arg.Any<CancellationToken>())
			.Returns(dungeon);

		var command = new RemoveFloorCommand(
			dungeon.Id,
			Guid.NewGuid());

		// Act
		var result = await Handler.HandleAsync(
			command,
			CancellationToken.None);

		// Assert
		result.ErrorCategory.Should().Be(ErrorCategory.NotFound);

		WriteRepository
			.DidNotReceive()
			.Update(Arg.Any<Dungeon>());

		await UnitOfWork
			.DidNotReceive()
			.CommitAsync(Arg.Any<CancellationToken>());
	}
	/// <summary>
	///		Dispatches this leaf's command with the supplied token, for the base-level tests that assert which token crossed each port.
	/// </summary>
	/// <param name="cancellationToken">
	///		The token to hand to the handler.
	/// </param>
	/// <returns>
	///		A task that completes when the handler returns.
	/// </returns>
	protected override Task InvokeWithTokenAsync(CancellationToken cancellationToken) =>
		Handler.HandleAsync(
			new RemoveFloorCommand(Guid.NewGuid(), Guid.NewGuid()),
			cancellationToken);
}
