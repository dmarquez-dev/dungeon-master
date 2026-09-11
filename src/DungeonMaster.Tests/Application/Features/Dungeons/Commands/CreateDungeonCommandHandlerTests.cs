using DungeonMaster.Application.Common.Interfaces;
using DungeonMaster.Application.Features.Dungeons.Commands.CreateDungeon;
using DungeonMaster.Domain.Entities.Dungeon;
using DungeonMaster.Tests.Application.Common.Handlers;
using FluentAssertions;
using NSubstitute;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Commands;

/// <summary>
///		Unit tests for <see cref="CreateDungeonCommandHandler"/>.
/// </summary>
public sealed class CreateDungeonCommandHandlerTests : CommandHandlerTestBase<CreateDungeonCommandHandler>
{
	/// <inheritdoc />
	protected override CreateDungeonCommandHandler CreateHandler(IUnitOfWorkFactory factory) =>
		new(factory);

	/// <summary>
	///		Verifies that a valid command produces a successful result containing a non-empty Guid,
	///		and that <see cref="IDungeonWriteRepository.Add"/> is called exactly once and then committed.
	/// </summary>
	[Fact]
	public async Task HandleAsync_WithValidCommand_CreatesAndReturnsId()
	{
		// Arrange
		var command = new CreateDungeonCommand(
			"The Sunken Vaults",
			"A flooded ruin beneath the city.");

		// Act
		var result = await Handler.HandleAsync(
			command,
			CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().NotBeEmpty();

		WriteRepository
			.Received(1)
			.Add(Arg.Any<Dungeon>());

		await UnitOfWork
			.Received(1)
			.CommitAsync(Arg.Any<CancellationToken>());
	}

	/// <summary>
	///		Verifies that the dungeon passed to the repository carries the exact Name and Description from the command.
	/// </summary>
	[Fact]
	public async Task HandleAsync_WithValidCommand_PassesCorrectDataToRepository()
	{
		// Arrange
		Dungeon? captured = null;

		WriteRepository.Add(Arg.Do<Dungeon>(dungeon =>
			captured = dungeon));

		var command = new CreateDungeonCommand(
			"Ember Crypts",
			"Smouldering catacombs beneath the volcano.");

		// Act
		await Handler.HandleAsync(
			command,
			CancellationToken.None);

		// Assert
		captured.Should().NotBeNull();
		captured!.Name.Should().Be(command.Name);
		captured.Description.Should().Be(command.Description);
	}

	/// <summary>
	///		Verifies that a command with a null Description produces a successful result
	///		and that the persisted dungeon also has a null Description.
	/// </summary>
	[Fact]
	public async Task HandleAsync_WithNullDescription_CreatesWithNullDescription()
	{
		// Arrange
		Dungeon? captured = null;

		WriteRepository.Add(Arg.Do<Dungeon>(dungeon =>
			captured = dungeon));

		var command = new CreateDungeonCommand(
			"Silent Tombs",
			null);

		// Act
		var result = await Handler.HandleAsync(
			command,
			CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();

		captured.Should().NotBeNull();
		captured!.Description.Should().BeNull();
	}
	/// <summary>
	///		Verifies that the exact token handed to the handler is the token the commit boundary is crossed with.
	///		This handler commits without loading, so the commit boundary needs a test here rather than on a shared base.
	/// </summary>
	[Fact]
	public async Task HandleAsync_WithACancellationToken_PassesThatSameTokenToUnitOfWorkCommitAsync()
	{
		// Arrange
		using var cts = new CancellationTokenSource();
		var token = cts.Token;

		// Act
		await Handler.HandleAsync(
			new CreateDungeonCommand(
				"Commit Token Probe",
				null),
			token);

		// Assert
		await UnitOfWork
			.Received(1)
			.CommitAsync(token);
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
			new CreateDungeonCommand(
				"Token Probe",
				null),
			ct);
}
