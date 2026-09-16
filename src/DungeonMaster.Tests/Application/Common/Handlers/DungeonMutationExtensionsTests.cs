using DungeonMaster.Application.Common.Handlers;
using DungeonMaster.Application.Common.Interfaces;
using DungeonMaster.Application.Common.Results;
using DungeonMaster.Domain.Entities.Dungeons;
using FluentAssertions;
using NSubstitute;

namespace DungeonMaster.Tests.Application.Common.Handlers;

/// <summary>
///		Unit tests for <see cref="DungeonMutationExtensions"/>.
///		Both <c>MutateAsync</c> overloads carry their own copy of the load-guard-mutate-commit prologue, so each
///		of the three behaviours below is proven twice, once against each overload.
/// </summary>
public class DungeonMutationExtensionsTests
{
	private readonly UnitOfWorkSubstitute _unitOfWork;
	private readonly IDungeonWriteRepository _writeRepository;

	/// <summary>
	///		Initializes the test class with a substituted unit-of-work chain.
	/// </summary>
	public DungeonMutationExtensionsTests()
	{
		_unitOfWork = new UnitOfWorkSubstitute();
		_writeRepository = _unitOfWork.Dungeons;
	}

	/// <summary>
	///		Verifies that the non-generic overload reports not-found and never commits when the dungeon does not exist.
	/// </summary>
	[Fact]
	public async Task MutateAsync_WithUnknownDungeon_ReturnsNotFoundAndDoesNotSave()
	{
		// Arrange
		var dungeonId = Guid.NewGuid();
		var mutated = false;

		_writeRepository
			.LoadAsync(
				Arg.Any<Guid>(),
				Arg.Any<CancellationToken>())
			.Returns((Dungeon?)null);

		// Act
		var result = await _unitOfWork.Factory.MutateAsync(
			dungeonId,
			dungeon =>
			{
				mutated = true;
				return Result.Success();
			},
			CancellationToken.None);

		// Assert
		result.IsFailure.Should().BeTrue();
		result.ErrorCategory.Should().Be(ErrorCategory.NotFound);
		result.Error.Should().Contain(dungeonId.ToString());
		mutated.Should().BeFalse();

		_writeRepository
			.DidNotReceive()
			.Update(Arg.Any<Dungeon>());

		await _unitOfWork.UnitOfWork
			.DidNotReceive()
			.CommitAsync(Arg.Any<CancellationToken>());
	}

	/// <summary>
	///		Verifies that the value-producing overload reports not-found and never commits when the dungeon does not exist.
	/// </summary>
	[Fact]
	public async Task MutateAsync_WithValue_WithUnknownDungeon_ReturnsNotFoundAndDoesNotSave()
	{
		// Arrange
		var dungeonId = Guid.NewGuid();
		var mutated = false;

		_writeRepository
			.LoadAsync(
				Arg.Any<Guid>(),
				Arg.Any<CancellationToken>())
			.Returns((Dungeon?)null);

		// Act
		var result = await _unitOfWork.Factory.MutateAsync(
			dungeonId,
			dungeon =>
			{
				mutated = true;
				return Result<Guid>.Success(Guid.NewGuid());
			},
			CancellationToken.None);

		// Assert
		result.IsFailure.Should().BeTrue();
		result.ErrorCategory.Should().Be(ErrorCategory.NotFound);
		result.Error.Should().Contain(dungeonId.ToString());
		mutated.Should().BeFalse();

		_writeRepository
			.DidNotReceive()
			.Update(Arg.Any<Dungeon>());

		await _unitOfWork.UnitOfWork
			.DidNotReceive()
			.CommitAsync(Arg.Any<CancellationToken>());
	}

	/// <summary>
	///		Verifies that the non-generic overload short-circuits the commit when the mutation itself failed.
	/// </summary>
	[Fact]
	public async Task MutateAsync_WithFailingMutation_DoesNotSave()
	{
		// Arrange
		var dungeon = new Dungeon("Test Dungeon");

		_writeRepository
			.LoadAsync(
				dungeon.Id,
				Arg.Any<CancellationToken>())
			.Returns(dungeon);

		// Act
		var result = await _unitOfWork.Factory.MutateAsync(
			dungeon.Id,
			_ =>
				Result.Failure(
					"Floor not found.",
					ErrorCategory.NotFound),
			CancellationToken.None);

		// Assert
		result.IsFailure.Should().BeTrue();
		result.ErrorCategory.Should().Be(ErrorCategory.NotFound);

		_writeRepository
			.DidNotReceive()
			.Update(Arg.Any<Dungeon>());

		await _unitOfWork.UnitOfWork
			.DidNotReceive()
			.CommitAsync(Arg.Any<CancellationToken>());
	}

	/// <summary>
	///		Verifies that the value-producing overload short-circuits the commit when the mutation itself failed.
	/// </summary>
	[Fact]
	public async Task MutateAsync_WithValue_WithFailingMutation_DoesNotSave()
	{
		// Arrange
		var dungeon = new Dungeon("Test Dungeon");

		_writeRepository
			.LoadAsync(
				dungeon.Id,
				Arg.Any<CancellationToken>())
			.Returns(dungeon);

		// Act
		var result = await _unitOfWork.Factory.MutateAsync(
			dungeon.Id,
			_ =>
				Result<Guid>.Failure(
					"Floor not found.",
					ErrorCategory.NotFound),
			CancellationToken.None);

		// Assert
		result.IsFailure.Should().BeTrue();
		result.ErrorCategory.Should().Be(ErrorCategory.NotFound);

		_writeRepository
			.DidNotReceive()
			.Update(Arg.Any<Dungeon>());

		await _unitOfWork.UnitOfWork
			.DidNotReceive()
			.CommitAsync(Arg.Any<CancellationToken>());
	}

	/// <summary>
	///		Verifies that the non-generic overload commits exactly once when the mutation succeeded.
	/// </summary>
	[Fact]
	public async Task MutateAsync_WithSucceedingMutation_SavesOnce()
	{
		// Arrange
		var dungeon = new Dungeon("Test Dungeon");

		_writeRepository
			.LoadAsync(
				dungeon.Id,
				Arg.Any<CancellationToken>())
			.Returns(dungeon);

		// Act
		var result = await _unitOfWork.Factory.MutateAsync(
			dungeon.Id,
			_ =>
				Result.Success(),
			CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();

		_writeRepository
			.Received(1)
			.Update(dungeon);

		await _unitOfWork.UnitOfWork
			.Received(1)
			.CommitAsync(Arg.Any<CancellationToken>());
	}

	/// <summary>
	///		Verifies that the value-producing overload commits exactly once and carries the produced value.
	/// </summary>
	[Fact]
	public async Task MutateAsync_WithValue_WithSucceedingMutation_SavesOnce()
	{
		// Arrange
		var dungeon = new Dungeon("Test Dungeon");
		var expectedValue = Guid.NewGuid();

		_writeRepository
			.LoadAsync(
				dungeon.Id,
				Arg.Any<CancellationToken>())
			.Returns(dungeon);

		// Act
		var result = await _unitOfWork.Factory.MutateAsync(
			dungeon.Id,
			_ =>
				Result<Guid>.Success(expectedValue),
			CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().Be(expectedValue);

		_writeRepository
			.Received(1)
			.Update(dungeon);

		await _unitOfWork.UnitOfWork
			.Received(1)
			.CommitAsync(Arg.Any<CancellationToken>());
	}
}
