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
public class DeleteDungeonCommandHandlerTests
{
	private readonly UnitOfWorkSubstitute _unitOfWork;
	private readonly IDungeonWriteRepository _writeRepository;
	private readonly DeleteDungeonCommandHandler _handler;

	/// <summary>
	///		Initializes the test class with a substituted unit-of-work chain and a real handler instance.
	/// </summary>
	public DeleteDungeonCommandHandlerTests()
	{
		_unitOfWork = new UnitOfWorkSubstitute();
		_writeRepository = _unitOfWork.Dungeons;
		_handler = new DeleteDungeonCommandHandler(_unitOfWork.Factory);
	}

	/// <summary>
	///		Verifies that a valid identifier produces a successful result and that <see cref="IDungeonWriteRepository.RemoveAsync"/> is called exactly once.
	/// </summary>
	[Fact]
	public async Task HandleAsync_WithValidId_ReturnsSuccess()
	{
		// Arrange
		var id = Guid.NewGuid();

		_writeRepository
			.RemoveAsync(
				Arg.Any<Guid>(),
				Arg.Any<CancellationToken>())
			.Returns(true);

		var command = new DeleteDungeonCommand(id);

		// Act
		var result = await _handler.HandleAsync(
			command,
			CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();

		await _writeRepository
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

		_writeRepository
			.RemoveAsync(
				Arg.Any<Guid>(),
				Arg.Any<CancellationToken>())
			.Returns(false);

		var command = new DeleteDungeonCommand(id);

		// Act
		var result = await _handler.HandleAsync(
			command,
			CancellationToken.None);

		// Assert
		result.IsFailure.Should().BeTrue();
		result.ErrorCategory.Should().Be(ErrorCategory.NotFound);
		result.Error.Should().Contain(id.ToString());

		await _unitOfWork.UnitOfWork
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

		_writeRepository
			.RemoveAsync(
				Arg.Do<Guid>(id =>
					capturedId = id),
				Arg.Any<CancellationToken>())
			.Returns(true);

		var command = new DeleteDungeonCommand(expectedId);

		// Act
		await _handler.HandleAsync(
			command,
			CancellationToken.None);

		// Assert
		capturedId.Should().Be(expectedId);
	}
}
