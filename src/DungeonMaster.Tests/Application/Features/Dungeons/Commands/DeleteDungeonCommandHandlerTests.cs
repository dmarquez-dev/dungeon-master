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
	/// <summary>
	///		Verifies that the exact token handed to the handler is the token the write repository is asked to remove with.
	///		This is the only handler that reaches <see cref="IDungeonWriteRepository.RemoveAsync"/>, so the removal boundary needs a test here rather than on a shared base.
	/// </summary>
	[Fact]
	public async Task HandleAsync_WithACancellationToken_PassesThatSameTokenToWriteRepositoryRemoveAsync()
	{
		// Arrange
		using var cts = new CancellationTokenSource();
		var token = cts.Token;

		var id = Guid.NewGuid();

		WriteRepository
			.RemoveAsync(
				Arg.Any<Guid>(),
				Arg.Any<CancellationToken>())
			.Returns(true);

		// Act
		await Handler.HandleAsync(
			new DeleteDungeonCommand(id),
			token);

		// Assert
		await WriteRepository
			.Received(1)
			.RemoveAsync(
				id,
				token);
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
			new DeleteDungeonCommand(Guid.NewGuid()),
			ct);
}
