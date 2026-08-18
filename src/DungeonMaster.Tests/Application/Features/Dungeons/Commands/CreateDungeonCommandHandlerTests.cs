using DungeonMaster.Application.Features.Dungeons;
using DungeonMaster.Application.Features.Dungeons.Commands.CreateDungeon;
using DungeonMaster.Domain.Entities.Dungeon;
using FluentAssertions;
using NSubstitute;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Commands;

/// <summary>
///		Unit tests for <see cref="CreateDungeonCommandHandler"/>.
/// </summary>
public class CreateDungeonCommandHandlerTests
{
	private readonly IDungeonWriteRepository _writeRepository;
	private readonly CreateDungeonCommandHandler _handler;

	/// <summary>
	///		Initializes the test class with a substituted repository and a real handler instance.
	/// </summary>
	public CreateDungeonCommandHandlerTests()
	{
		_writeRepository = Substitute.For<IDungeonWriteRepository>();
		_handler = new CreateDungeonCommandHandler(_writeRepository);
	}

	/// <summary>
	///		Verifies that a valid command produces a successful result containing a non-empty Guid,
	///		and that <see cref="IDungeonWriteRepository.AddAsync"/> is called exactly once.
	/// </summary>
	[Fact]
	public async Task HandleAsync_WithValidCommand_CreatesAndReturnsId()
	{
		// Arrange
		_writeRepository
			.AddAsync(
				Arg.Any<Dungeon>(),
				Arg.Any<CancellationToken>())
			.Returns(Task.CompletedTask);

		var command = new CreateDungeonCommand(
			"The Sunken Vaults",
			"A flooded ruin beneath the city.");

		// Act
		var result = await _handler.HandleAsync(
			command,
			CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().NotBeEmpty();

		await _writeRepository
			.Received(1)
			.AddAsync(
				Arg.Any<Dungeon>(),
				Arg.Any<CancellationToken>());
	}

	/// <summary>
	///		Verifies that the dungeon passed to the repository carries the exact Name and Description from the command.
	/// </summary>
	[Fact]
	public async Task HandleAsync_WithValidCommand_PassesCorrectDataToRepository()
	{
		// Arrange
		Dungeon? captured = null;

		_writeRepository
			.AddAsync(
				Arg.Do<Dungeon>(dungeon =>
				captured = dungeon),
				Arg.Any<CancellationToken>())
			.Returns(Task.CompletedTask);

		var command = new CreateDungeonCommand(
			"Ember Crypts",
			"Smouldering catacombs beneath the volcano.");

		// Act
		await _handler.HandleAsync(
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

		_writeRepository
			.AddAsync(
				Arg.Do<Dungeon>(dungeon =>
				captured = dungeon),
				Arg.Any<CancellationToken>())
			.Returns(Task.CompletedTask);

		var command = new CreateDungeonCommand(
			"Silent Tombs",
			null);

		// Act
		var result = await _handler.HandleAsync(
			command,
			CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();

		captured.Should().NotBeNull();
		captured!.Description.Should().BeNull();
	}
}
