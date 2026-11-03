using DungeonMaster.App.Services.Navigation;
using DungeonMaster.App.ViewModels;
using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Application.Common.Results;
using DungeonMaster.Application.Features.Dungeons.Queries.GetDungeonDetail;

using FluentAssertions;

using NSubstitute;

// Microsoft.Maui.Dispatching also publishes an IDispatcher. DungeonDetailViewModel itself
// disambiguates the same way, at DungeonDetailViewModel.cs:6.
using AppDispatcher = DungeonMaster.Application.Common.Dispatch.IDispatcher;

namespace DungeonMaster.App.Tests.ViewModels;

/// <summary>
///		Pins what <c>DungeonDetailViewModel</c> does when a well-formed dungeon identifier names no dungeon.
///		The query answers <see langword="null"/>, and the view model must clear the identifier it loaded, report the failure, and refuse to dispatch any further write.
/// </summary>
/// <remarks>
///		This class does not join <c>MauiHostCollection</c>.
///		No MAUI <c>Element</c> is constructed: the view model and its three children are plain observable objects, and the dispatcher and navigation service are substitutes.
/// </remarks>
public sealed class DungeonDetailViewModelMissingDungeonTests
{
	private static readonly DateTime FixedInstant = new(
		2026,
		1,
		1,
		0,
		0,
		0,
		DateTimeKind.Utc);

	private readonly AppDispatcher _dispatcher = Substitute.For<AppDispatcher>();

	private readonly DungeonDetailViewModel _viewModel;

	/// <summary>
	///		Initialises the substituted dispatcher, so every <c>SendAsync</c> of the non-generic overload completes with a success and is counted rather than awaited as a null task.
	/// </summary>
	public DungeonDetailViewModelMissingDungeonTests()
	{
		_dispatcher
			.SendAsync(Arg.Any<ICommand>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(Result.Success()));

		_viewModel = new DungeonDetailViewModel(
			_dispatcher,
			Substitute.For<INavigationService>());
	}

	/// <summary>
	///		A well-formed identifier whose query returns <see langword="null"/> leaves <c>DungeonId</c> at <see cref="Guid.Empty"/>.
	/// </summary>
	[Fact]
	public async Task ApplyQueryAttributes_WhenTheQueryFindsNoDungeon_LeavesDungeonIdEmpty()
	{
		// Arrange
		var id = Guid.NewGuid();
		QueryReturns(
			id,
			null);

		// Act
		await NavigateTo(id);

		// Assert
		_viewModel.DungeonId.Should().Be(Guid.Empty);
	}

	/// <summary>
	///		A well-formed identifier whose query returns <see langword="null"/> puts a message on <c>ErrorMessage</c>.
	/// </summary>
	[Fact]
	public async Task ApplyQueryAttributes_WhenTheQueryFindsNoDungeon_SetsErrorMessage()
	{
		// Arrange
		var id = Guid.NewGuid();
		QueryReturns(
			id,
			null);

		// Act
		await NavigateTo(id);

		// Assert
		_viewModel.ErrorMessage.Should().NotBeNull();
	}

	/// <summary>
	///		After a load whose query returned <see langword="null"/>, the header's save sends nothing through any <c>SendAsync</c> overload.
	/// </summary>
	[Fact]
	public async Task Save_AfterALoadThatFoundNoDungeon_SendsNoCommand()
	{
		// Arrange
		var id = Guid.NewGuid();
		QueryReturns(
			id,
			null);

		await NavigateTo(id);

		// Act
		await _viewModel.Header.SaveCommand.ExecuteAsync(null);

		// Assert
		SendAsyncCallCount().Should().Be(0);
	}

	/// <summary>
	///		A save that succeeds and whose reload finds the dungeon deleted leaves <c>DungeonId</c> at <see cref="Guid.Empty"/> and sets <c>ErrorMessage</c>.
	///		A second save then sends nothing, so exactly one <c>SendAsync</c> reaches the dispatcher across the test.
	/// </summary>
	[Fact]
	public async Task Save_WhenTheReloadFindsTheDungeonDeleted_LeavesDungeonIdEmptySetsErrorMessageAndSendsNoSecondCommand()
	{
		// Arrange
		var id = Guid.NewGuid();
		QueryReturns(
			id,
			Dungeon(id),
			(DungeonDetailDto?)null);

		await NavigateTo(id);

		// Act
		await _viewModel.Header.SaveCommand.ExecuteAsync(null);

		// Assert
		_viewModel.DungeonId.Should().Be(Guid.Empty);
		_viewModel.ErrorMessage.Should().NotBeNull();

		// Act
		await _viewModel.Header.SaveCommand.ExecuteAsync(null);

		// Assert
		SendAsyncCallCount().Should().Be(1);
	}

	/// <summary>
	///		Control: a query that returns a dungeon sets <c>DungeonId</c> to its id, leaves <c>ErrorMessage</c> null after the load, and lets one save send exactly one command.
	/// </summary>
	[Fact]
	public async Task ApplyQueryAttributes_WhenTheQueryReturnsADungeon_SetsDungeonIdLeavesErrorMessageNullAndSaveSendsOneCommand()
	{
		// Arrange
		var id = Guid.NewGuid();
		QueryReturns(
			id,
			Dungeon(id));

		// Act
		await NavigateTo(id);

		// Assert
		_viewModel.DungeonId.Should().Be(id);
		_viewModel.ErrorMessage.Should().BeNull();

		// Act
		await _viewModel.Header.SaveCommand.ExecuteAsync(null);

		// Assert
		SendAsyncCallCount().Should().Be(1);
	}

	private static DungeonDetailDto Dungeon(Guid id) =>
		new(
			id,
			"Sunken Vault",
			null,
			null,
			0,
			FixedInstant,
			FixedInstant,
			[]);

	// Each successive query for the identifier answers with the next value; the last repeats.
	private void QueryReturns(
		Guid id,
		DungeonDetailDto? first,
		params DungeonDetailDto?[] then)
	{
		_dispatcher
			.QueryAsync(
				Arg.Is<GetDungeonDetailQuery>(query =>
					query.Id == id),
				Arg.Any<CancellationToken>())
			.Returns(
				Task.FromResult(first),
				then
					.Select(dto =>
						Task.FromResult(dto))
					.ToArray());
	}

	private Task NavigateTo(Guid id)
	{
		_viewModel.ApplyQueryAttributes(new Dictionary<string, object>
		{
			[ShellRoutes.IdQueryKey] = id.ToString(),
		});

		return _viewModel.LoadTask;
	}

	// Counts calls to every SendAsync overload, generic or not, by method name.
	private int SendAsyncCallCount() =>
		_dispatcher
			.ReceivedCalls()
			.Count(call =>
				call.GetMethodInfo().Name == nameof(AppDispatcher.SendAsync));
}
