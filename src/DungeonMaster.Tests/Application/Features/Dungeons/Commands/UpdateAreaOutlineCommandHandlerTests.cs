using DungeonMaster.Application.Common.Interfaces;
using DungeonMaster.Application.Common.Results;
using DungeonMaster.Application.Features.Dungeons.Commands.UpdateAreaOutline;
using DungeonMaster.Domain.Entities.Dungeon;
using DungeonMaster.Domain.ValueObjects;
using DungeonMaster.Tests.Application.Common.Handlers;
using FluentAssertions;
using NSubstitute;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Commands;

public sealed class UpdateAreaOutlineCommandHandlerTests : DungeonNotFoundHandlerTestBase<UpdateAreaOutlineCommandHandler>
{
	/// <inheritdoc />
	protected override UpdateAreaOutlineCommandHandler CreateHandler(IUnitOfWorkFactory factory) =>
		new(factory);

	[Fact]
	public async Task HandleAsync_WithExistingArea_UpdatesOutlineAndReturnsSuccess()
	{
		// Arrange
		var dungeon = new Dungeon("Test Dungeon");
		var floor = dungeon.AddFloor("Ground");
		dungeon.AddArea(floor.Id, "Room", null);
		WriteRepository
			.LoadAsync(dungeon.Id, Arg.Any<CancellationToken>())
			.Returns(dungeon);

		var area = floor.Areas[0];
		var outline = new List<PathSegment>
		{
			new(PathSegmentKind.Line, new TilePoint(4, 3), null, null),
			new(PathSegmentKind.Line, new TilePoint(4, 6), null, null),
			new(PathSegmentKind.Line, new TilePoint(2, 3), null, null),
		};

		var command = new UpdateAreaOutlineCommand(dungeon.Id, floor.Id, area.Id, outline);

		// Act
		var result = await Handler.HandleAsync(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		area.Outline.Should().HaveCount(3);
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
			new UpdateAreaOutlineCommand(
				Guid.NewGuid(),
				Guid.NewGuid(),
				Guid.NewGuid(),
				[]),
			CancellationToken.None);

		return (result.IsSuccess, result.ErrorCategory);
	}

	/// <summary>
	///		Verifies that an unknown area identifier reports not-found and does not write the unchanged aggregate back.
	/// </summary>
	[Fact]
	public async Task HandleAsync_WithUnknownAreaId_ReturnsNotFoundAndDoesNotSave()
	{
		// Arrange
		var dungeon = new Dungeon("Test Dungeon");
		var floor = dungeon.AddFloor("Ground");

		WriteRepository
			.LoadAsync(
				dungeon.Id,
				Arg.Any<CancellationToken>())
			.Returns(dungeon);

		var command = new UpdateAreaOutlineCommand(
			dungeon.Id,
			floor.Id,
			Guid.NewGuid(),
			[]);

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
	/// <param name="ct">
	///		The token to hand to the handler.
	/// </param>
	/// <returns>
	///		A task that completes when the handler returns.
	/// </returns>
	protected override Task InvokeWithTokenAsync(CancellationToken ct) =>
		Handler.HandleAsync(
			new UpdateAreaOutlineCommand(
				Guid.NewGuid(),
				Guid.NewGuid(),
				Guid.NewGuid(),
				[]),
			ct);
}
