using DungeonMaster.Application.Common.Interfaces;
using DungeonMaster.Application.Common.Results;
using DungeonMaster.Application.Features.Dungeons.Commands.DeleteDungeon;
using DungeonMaster.Tests.Application.Common.Handlers;
using FluentAssertions;
using NSubstitute;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Commands;

/// <summary>
///		Unit tests for <see cref="DeleteDungeonCommandHandler"/>.
/// </summary>
public sealed class DeleteDungeonCommandHandlerTests : CommandHandlerTestBase<DeleteDungeonCommandHandler>
{
	/// <inheritdoc />
	protected override DeleteDungeonCommandHandler CreateHandler(IUnitOfWorkFactory factory) =>
		new(factory);

	/// <summary>
	///		Verifies that a valid identifier produces a successful result and that <see cref="IDungeonWriteRepository.RemoveAsync"/> is called exactly once.
	/// </summary>
	[Fact]
	public async Task HandleAsync_WithValidId_ReturnsSuccess()
	{
		// Arrange
		var id = Guid.NewGuid();

		WriteRepository
			.RemoveAsync(
				Arg.Any<Guid>(),
				Arg.Any<CancellationToken>())
			.Returns(true);

		var command = new DeleteDungeonCommand(id);

		// Act
		var result = await Handler.HandleAsync(
			command,
			CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();

		await WriteRepository
			.Received(1)
			.RemoveAsync(
				Arg.Any<Guid>(),
				Arg.Any<CancellationToken>());
	}

	/// <summary>
	///		Verifies that an identifier the repository could not delete produces a not-found failure naming the dungeon.
	/// </summary>
	[Fact]
	public async Task HandleAsync_WithUnknownId_ReturnsNotFound()
	{
		// Arrange
		var id = Guid.NewGuid();

		WriteRepository
			.RemoveAsync(
				Arg.Any<Guid>(),
				Arg.Any<CancellationToken>())
			.Returns(false);

		var command = new DeleteDungeonCommand(id);

		// Act
		var result = await Handler.HandleAsync(
			command,
			CancellationToken.None);

		// Assert
		result.IsFailure.Should().BeTrue();
		result.ErrorCategory.Should().Be(ErrorCategory.NotFound);
		result.Error.Should().Contain(id.ToString());

		await UnitOfWork
			.DidNotReceive()
			.CommitAsync(Arg.Any<CancellationToken>());
	}

	/// <summary>
	///		Verifies that the identifier passed to the repository matches the one on the command.
	/// </summary>
	[Fact]
	public async Task HandleAsync_WhenCalled_PassesCorrectIdToRepository()
	{
		// Arrange
		var expectedId = Guid.NewGuid();
		Guid capturedId = Guid.Empty;

		WriteRepository
			.RemoveAsync(
				Arg.Do<Guid>(id =>
					capturedId = id),
				Arg.Any<CancellationToken>())
			.Returns(true);

		var command = new DeleteDungeonCommand(expectedId);

		// Act
		await Handler.HandleAsync(
			command,
			CancellationToken.None);

		// Assert
		capturedId.Should().Be(expectedId);
	}
}
