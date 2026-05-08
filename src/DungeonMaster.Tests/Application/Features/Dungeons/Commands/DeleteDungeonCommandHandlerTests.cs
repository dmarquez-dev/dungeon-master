using DungeonMaster.Application.Features.Dungeons;
using DungeonMaster.Application.Features.Dungeons.Commands.DeleteDungeon;
using FluentAssertions;
using NSubstitute;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Commands;

/// <summary>
///		Unit tests for <see cref="DeleteDungeonCommandHandler"/>.
/// </summary>
public class DeleteDungeonCommandHandlerTests
{
	private readonly IDungeonRepository _dungeonRepository;
	private readonly DeleteDungeonCommandHandler _handler;

	/// <summary>
	///		Initializes the test class with a substituted repository and a real handler instance.
	/// </summary>
	public DeleteDungeonCommandHandlerTests()
	{
		_dungeonRepository = Substitute.For<IDungeonRepository>();
		_handler = new DeleteDungeonCommandHandler(_dungeonRepository);
	}

	/// <summary>
	///		Verifies that a valid identifier produces a successful result with a value of <see langword="true"/> and that <see cref="IDungeonRepository.DeleteAsync"/> is called exactly once.
	/// </summary>
	[Fact]
	public async Task HandleAsync_WithValidId_ReturnsSuccess()
	{
		// Arrange
		var id = Guid.NewGuid();

		_dungeonRepository
			.DeleteAsync(
				Arg.Any<Guid>(),
				Arg.Any<CancellationToken>())
			.Returns(Task.CompletedTask);

		var command = new DeleteDungeonCommand(id);

		// Act
		var result = await _handler.HandleAsync(
			command,
			CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().BeTrue();

		await _dungeonRepository
			.Received(1)
			.DeleteAsync(
				Arg.Any<Guid>(),
				Arg.Any<CancellationToken>());
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

		_dungeonRepository
			.DeleteAsync(
				Arg.Do<Guid>(id =>
					capturedId = id),
				Arg.Any<CancellationToken>())
			.Returns(Task.CompletedTask);

		var command = new DeleteDungeonCommand(expectedId);

		// Act
		await _handler.HandleAsync(
			command,
			CancellationToken.None);

		// Assert
		capturedId.Should().Be(expectedId);
	}
}
