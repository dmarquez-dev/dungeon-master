using DungeonMaster.Application.Common.Interfaces;
using DungeonMaster.Application.Common.Results;
using DungeonMaster.Application.Features.Dungeons.Commands.AddArea;
using DungeonMaster.Domain.Entities.Dungeon;
using DungeonMaster.Tests.Application.Common.Handlers;
using FluentAssertions;
using NSubstitute;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Commands;

public sealed class AddAreaCommandHandlerTests : DungeonNotFoundHandlerTestBase<AddAreaCommandHandler>
{
	/// <inheritdoc />
	protected override AddAreaCommandHandler CreateHandler(IUnitOfWorkFactory factory) =>
		new(factory);

	[Fact]
	public async Task HandleAsync_WithExistingDungeonAndFloor_AddsAreaAndReturnsId()
	{
		// Arrange
		var dungeon = new Dungeon("Test Dungeon");
		var floor = dungeon.AddFloor("Ground");
		WriteRepository
			.LoadAsync(dungeon.Id, Arg.Any<CancellationToken>())
			.Returns(dungeon);

		var command = new AddAreaCommand(dungeon.Id, floor.Id, "Throne Room", null);

		// Act
		var result = await Handler.HandleAsync(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().NotBeEmpty();
		floor.Areas.Should().HaveCount(1);
		floor.Areas[0].Name.Should().Be("Throne Room");
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
			new AddAreaCommand(Guid.NewGuid(), Guid.NewGuid(), "Any Area", null),
			CancellationToken.None);

		return (result.IsSuccess, result.ErrorCategory);
	}

	[Fact]
	public async Task HandleAsync_WithUnknownFloor_ReturnsNotFoundFailureNamingTheFloor()
	{
		// Arrange
		var dungeon = new Dungeon("Test Dungeon");
		dungeon.AddFloor("Ground");
		var floorId = Guid.NewGuid();
		WriteRepository
			.LoadAsync(
				dungeon.Id,
				Arg.Any<CancellationToken>())
			.Returns(dungeon);

		var command = new AddAreaCommand(
			dungeon.Id,
			floorId,
			"Orphan Area",
			null);

		// Act
		var result = await Handler.HandleAsync(
			command,
			CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.ErrorCategory.Should().Be(ErrorCategory.NotFound);
		result.Error.Should().Contain(floorId.ToString());
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
			new AddAreaCommand(Guid.NewGuid(), Guid.NewGuid(), "Any Area", null),
			ct);
}
