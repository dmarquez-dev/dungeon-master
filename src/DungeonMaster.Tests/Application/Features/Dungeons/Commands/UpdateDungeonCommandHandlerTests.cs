using DungeonMaster.Application.Common.Interfaces;
using DungeonMaster.Application.Common.Results;
using DungeonMaster.Application.Features.Dungeons.Commands.UpdateDungeon;
using DungeonMaster.Domain.Entities.Dungeon;
using DungeonMaster.Tests.Application.Common.Handlers;
using FluentAssertions;
using NSubstitute;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Commands;

public sealed class UpdateDungeonCommandHandlerTests : DungeonNotFoundHandlerTestBase<UpdateDungeonCommandHandler>
{
	/// <inheritdoc />
	protected override UpdateDungeonCommandHandler CreateHandler(IUnitOfWorkFactory factory) =>
		new(factory);

	[Fact]
	public async Task HandleAsync_WithExistingDungeon_UpdatesAndReturnsSuccess()
	{
		// Arrange
		var dungeon = new Dungeon("Old Name", "Old Desc");
		WriteRepository.LoadAsync(dungeon.Id, Arg.Any<CancellationToken>()).Returns(dungeon);
		var command = new UpdateDungeonCommand(dungeon.Id, "New Name", "New Desc", "My Notes");

		// Act
		var result = await Handler.HandleAsync(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		dungeon.Name.Should().Be("New Name");
		dungeon.Notes.Should().Be("My Notes");
		WriteRepository.Received(1).Update(dungeon);
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
			new UpdateDungeonCommand(Guid.NewGuid(), "Name", null, null),
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
			new UpdateDungeonCommand(Guid.NewGuid(), "Name", null, null),
			ct);
}
