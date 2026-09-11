using DungeonMaster.Application.Common.Interfaces;
using DungeonMaster.Application.Common.Results;
using DungeonMaster.Application.Features.Dungeons.Commands.AddFloor;
using DungeonMaster.Domain.Entities.Dungeon;
using DungeonMaster.Tests.Application.Common.Handlers;
using FluentAssertions;
using NSubstitute;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Commands;

public sealed class AddFloorCommandHandlerTests : DungeonNotFoundHandlerTestBase<AddFloorCommandHandler>
{
	/// <inheritdoc />
	protected override AddFloorCommandHandler CreateHandler(IUnitOfWorkFactory factory) =>
		new(factory);

	[Fact]
	public async Task HandleAsync_WithExistingDungeon_AddsFloorAndReturnsId()
	{
		// Arrange
		var dungeon = new Dungeon("Test Dungeon");
		WriteRepository.LoadAsync(dungeon.Id, Arg.Any<CancellationToken>()).Returns(dungeon);
		// Act
		var result = await Handler.HandleAsync(new AddFloorCommand(dungeon.Id, "Entry"), CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().NotBeEmpty();
		dungeon.Floors.Should().HaveCount(1);
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
			new AddFloorCommand(Guid.NewGuid(), null),
			CancellationToken.None);

		return (result.IsSuccess, result.ErrorCategory);
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
			new AddFloorCommand(Guid.NewGuid(), null),
			ct);
}
