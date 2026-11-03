using DungeonMaster.App.ViewModels;
using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Application.Common.Results;

using FluentAssertions;

using NSubstitute;

// Microsoft.Maui.Dispatching also publishes an IDispatcher. CreateDungeonViewModel itself
// disambiguates the same way, at CreateDungeonViewModel.cs:4.
using AppDispatcher = DungeonMaster.Application.Common.Dispatch.IDispatcher;

namespace DungeonMaster.App.Tests.ViewModels;

/// <summary>
///		Pins what <c>CreateDungeonViewModel</c> does with the error a failed create leaves behind.
///		A create that starts after a failure clears that error before its dispatch completes, and a create that then succeeds leaves no error and states the outcome.
/// </summary>
/// <remarks>
///		This class does not join <c>MauiHostCollection</c>.
///		No MAUI <c>Element</c> is constructed: the view model is a plain observable object, and the dispatcher is a substitute.
/// </remarks>
public sealed class CreateDungeonViewModelErrorResetTests
{
	private readonly AppDispatcher _dispatcher = Substitute.For<AppDispatcher>();

	private readonly CreateDungeonViewModel _viewModel;

	/// <summary>
	///		Initialises the view model with a name, so <c>CreateCommand</c> can execute.
	/// </summary>
	public CreateDungeonViewModelErrorResetTests()
	{
		_viewModel = new CreateDungeonViewModel(_dispatcher)
		{
			Name = "Sunken Vault",
		};
	}

	/// <summary>
	///		After a failed create, a second create whose dispatch is still pending shows no error.
	/// </summary>
	[Fact]
	public async Task Create_AfterAFailedCreate_ClearsErrorMessageWhileTheSecondDispatchIsPending()
	{
		// Arrange
		var pending = new TaskCompletionSource<Result<Guid>>(TaskCreationOptions.RunContinuationsAsynchronously);
		SendAsyncReturns(
			Task.FromResult(Failure()),
			pending.Task);

		await _viewModel.CreateCommand.ExecuteAsync(null);
		_viewModel.ErrorMessage.Should().NotBeNull();

		// Act
		var secondCreate = _viewModel.CreateCommand.ExecuteAsync(null);

		// Assert
		_viewModel.ErrorMessage.Should().BeNull();

		pending.SetResult(Result<Guid>.Success(Guid.NewGuid()));
		await secondCreate;
	}

	/// <summary>
	///		After a failed create, a second create that succeeds leaves <c>ErrorMessage</c> null and states <c>Outcome</c> as <see langword="true"/>.
	/// </summary>
	[Fact]
	public async Task Create_WhenASecondCreateSucceedsAfterAFailure_LeavesErrorMessageNullAndSetsOutcomeTrue()
	{
		// Arrange
		var pending = new TaskCompletionSource<Result<Guid>>(TaskCreationOptions.RunContinuationsAsynchronously);
		SendAsyncReturns(
			Task.FromResult(Failure()),
			pending.Task);

		await _viewModel.CreateCommand.ExecuteAsync(null);
		_viewModel.ErrorMessage.Should().NotBeNull();

		var secondCreate = _viewModel.CreateCommand.ExecuteAsync(null);

		// Act
		pending.SetResult(Result<Guid>.Success(Guid.NewGuid()));
		await secondCreate;

		// Assert
		_viewModel.ErrorMessage.Should().BeNull();
		_viewModel.Outcome.Should().BeTrue();
	}

	/// <summary>
	///		Control: a failed create sets <c>ErrorMessage</c> to a non-null value and leaves <c>Outcome</c> null.
	/// </summary>
	[Fact]
	public async Task Create_WhenTheDispatchFails_SetsErrorMessageAndLeavesOutcomeNull()
	{
		// Arrange
		SendAsyncReturns(Task.FromResult(Failure()));

		// Act
		await _viewModel.CreateCommand.ExecuteAsync(null);

		// Assert
		_viewModel.ErrorMessage.Should().NotBeNull();
		_viewModel.Outcome.Should().BeNull();
	}

	private static Result<Guid> Failure() =>
		Result<Guid>.Failure(
			"A dungeon with that name already exists.",
			ErrorCategory.Conflict);

	// Each successive create answers with the next task; the last repeats.
	private void SendAsyncReturns(
		Task<Result<Guid>> first,
		params Task<Result<Guid>>[] then)
	{
		_dispatcher
			.SendAsync(
				Arg.Any<ICommand<Guid>>(),
				Arg.Any<CancellationToken>())
			.Returns(
				first,
				then);
	}
}
