using DungeonMaster.Application.Common.Results;
using DungeonMaster.Domain.Entities.Dungeons;
using FluentAssertions;
using NSubstitute;

namespace DungeonMaster.Tests.Application.Common.Handlers;

/// <summary>
///		The one place the dungeon-missing failure path of a command handler is tested.
///		Twelve command handler test classes arranged the same load-returns-null case and asserted the same two things; this holds that test once, and each leaf supplies only the command it dispatches.
/// </summary>
/// <typeparam name="THandler">
///		The command handler under test.
/// </typeparam>
public abstract class DungeonNotFoundHandlerTestBase<THandler> : CommandHandlerTestBase<THandler>
	where THandler : class
{
	/// <summary>
	///		Dispatches this leaf's command against a dungeon identifier the repository will not resolve, and projects the outcome.
	/// </summary>
	/// <remarks>
	///		<see cref="Result"/> and <see cref="Result{T}"/> are two unrelated sealed records with no common base and no shared interface, so a generic base cannot read <c>IsSuccess</c> or <c>ErrorCategory</c> off a type parameter. The leaf projects them instead.
	/// </remarks>
	/// <returns>
	///		The success flag and error category of the result the handler returned.
	/// </returns>
	protected abstract Task<(bool IsSuccess, ErrorCategory? ErrorCategory)> InvokeWithMissingDungeonAsync();

	/// <summary>
	///		Verifies that the exact token handed to the handler is the token the write repository is asked to load with.
	///		Declared on this base, so it runs once for every command handler test class whose handler loads the aggregate.
	/// </summary>
	[Fact]
	public async Task HandleAsync_WithACancellationToken_PassesThatSameTokenToWriteRepositoryLoadAsync()
	{
		// Arrange
		using var cts = new CancellationTokenSource();
		var token = cts.Token;

		// Act
		await InvokeWithTokenAsync(token);

		// Assert
		await WriteRepository
			.Received(1)
			.LoadAsync(
				Arg.Any<Guid>(),
				token);
	}

	/// <summary>
	///		Verifies that a dungeon the repository cannot load reports not-found, does not write the aggregate back, and does not cross the commit boundary.
	/// </summary>
	[Fact]
	public async Task HandleAsync_WithMissingDungeon_ReturnsNotFoundAndDoesNotSave()
	{
		// Arrange
		WriteRepository
			.LoadAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
			.Returns((Dungeon?)null);

		// Act
		var (isSuccess, errorCategory) = await InvokeWithMissingDungeonAsync();

		// Assert
		isSuccess.Should().BeFalse();
		errorCategory.Should().Be(ErrorCategory.NotFound);

		WriteRepository
			.DidNotReceive()
			.Update(Arg.Any<Dungeon>());

		await UnitOfWork
			.DidNotReceive()
			.CommitAsync(Arg.Any<CancellationToken>());
	}
}
