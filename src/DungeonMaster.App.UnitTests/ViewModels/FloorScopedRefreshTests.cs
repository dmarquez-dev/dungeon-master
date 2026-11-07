using System.Collections.Specialized;
using DungeonMaster.App.Services.Navigation;
using DungeonMaster.App.ViewModels;
using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Application.Common.Results;
using DungeonMaster.Application.Features.Dungeons.Queries.GetDungeonDetail;
using DungeonMaster.Application.Features.Dungeons.Queries.GetFloorDetail;
using DungeonMaster.Domain.Entities.Floors;
using DungeonMaster.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;
using AppDispatcher = DungeonMaster.Application.Common.Dispatch.IDispatcher;

namespace DungeonMaster.App.UnitTests.ViewModels;

/// <summary>
///		Pins what a successful mutation leaves behind on the dungeon detail page (MASTER-094): the
///		nine floor-scoped commands refresh their own floor in place, the four membership- and
///		header-shaped commands keep the full reload, a failed command refreshes nothing, and the
///		composition root owns the one <c>MapEditorState</c> of the page's lifetime (MASTER-098).
/// </summary>
/// <remarks>
///		THIS CLASS JOINS NO COLLECTION. The view models under test derive from
///		<c>ObservableObject</c> and touch no <c>Microsoft.Maui.Controls</c> type, so there is no
///		process-global state to race. Every dispatcher and child-side context is a substitute, so
///		no database and no running backend is involved.
/// </remarks>
public class FloorScopedRefreshTests
{
	private static readonly DateTime FixedTimestamp = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

	// -- Child view models against a substituted context --------------------------------------

	[Theory]
	[InlineData(nameof(MapCanvasViewModel.AddAreaCommand))]
	[InlineData(nameof(MapCanvasViewModel.RemoveAreaCommand))]
	[InlineData(nameof(MapCanvasViewModel.UpdateAreaCommand))]
	[InlineData(nameof(MapCanvasViewModel.AddAreaWithOutlineCommand))]
	[InlineData(nameof(MapCanvasViewModel.MoveAreaOutlineCommand))]
	[InlineData(nameof(MapCanvasViewModel.AddPlacedObjectCommand))]
	[InlineData(nameof(MapCanvasViewModel.RemovePlacedObjectCommand))]
	[InlineData(nameof(MapCanvasViewModel.UpdatePlacedObjectCommand))]
	[InlineData(nameof(FloorListViewModel.UpdateFloorCommand))]
	public async Task FloorScopedCommand_OnSuccess_RefreshesItsOwnFloorOnceAndDoesNotReload(string command)
	{
		// Arrange
		var floorId = Guid.NewGuid();
		var dispatcher = DispatcherReturning(succeed: true);
		var context = LoadedContext();

		// Act
		await ExecuteFloorScopedAsync(command, dispatcher, context, floorId);

		// Assert
		await context.Received(1).RefreshFloorAsync(floorId);
		await context.Received(1).RefreshFloorAsync(Arg.Any<Guid>());
		await context.DidNotReceive().ReloadAsync();
	}

	[Theory]
	[InlineData(nameof(FloorListViewModel.AddFloorCommand))]
	[InlineData(nameof(FloorListViewModel.RemoveFloorCommand))]
	[InlineData(nameof(FloorListViewModel.ReorderFloorCommand))]
	[InlineData(nameof(DungeonHeaderViewModel.SaveCommand))]
	public async Task MembershipOrHeaderCommand_OnSuccess_ReloadsOnceAndRefreshesNoFloor(string command)
	{
		// Arrange
		var dispatcher = DispatcherReturning(succeed: true);
		var context = LoadedContext();

		// Act
		await ExecuteFullReloadAsync(command, dispatcher, context, Guid.NewGuid());

		// Assert
		await context.Received(1).ReloadAsync();
		await context.DidNotReceive().RefreshFloorAsync(Arg.Any<Guid>());
	}

	[Theory]
	[InlineData(nameof(MapCanvasViewModel.AddAreaCommand))]
	[InlineData(nameof(MapCanvasViewModel.RemoveAreaCommand))]
	[InlineData(nameof(MapCanvasViewModel.UpdateAreaCommand))]
	[InlineData(nameof(MapCanvasViewModel.AddAreaWithOutlineCommand))]
	[InlineData(nameof(MapCanvasViewModel.MoveAreaOutlineCommand))]
	[InlineData(nameof(MapCanvasViewModel.AddPlacedObjectCommand))]
	[InlineData(nameof(MapCanvasViewModel.RemovePlacedObjectCommand))]
	[InlineData(nameof(MapCanvasViewModel.UpdatePlacedObjectCommand))]
	[InlineData(nameof(FloorListViewModel.UpdateFloorCommand))]
	public async Task FloorScopedCommand_OnFailure_ReportsTheFailureAndNeitherRefreshesNorReloads(string command)
	{
		// Arrange
		var dispatcher = DispatcherReturning(succeed: false);
		var context = LoadedContext();

		// Act
		await ExecuteFloorScopedAsync(command, dispatcher, context, Guid.NewGuid());

		// Assert
		context.Received(1).ReportFailure(Arg.Any<Result>());
		await context.DidNotReceive().RefreshFloorAsync(Arg.Any<Guid>());
		await context.DidNotReceive().ReloadAsync();
	}

	// -- The composition root -----------------------------------------------------------------

	[Fact]
	public async Task Editor_IsNonNullAfterConstruction_AndIsTheSameInstanceWithItsSelectionKeptAfterTwoLoads()
	{
		// Arrange
		var scenario = new Scenario();
		var editor = scenario.ViewModel.Editor;
		var selectedFloorId = scenario.Floors[1].Id;
		editor.SelectedFloorId = selectedFloorId;

		// Act
		await scenario.ViewModel.LoadDungeonCommand.ExecuteAsync(scenario.DungeonId);
		await scenario.ViewModel.LoadDungeonCommand.ExecuteAsync(scenario.DungeonId);

		// Assert
		editor.Should().NotBeNull();
		scenario.ViewModel.Editor.Should().BeSameAs(editor);
		scenario.ViewModel.Editor.SelectedFloorId.Should().Be(selectedFloorId);
	}

	[Fact]
	public async Task RefreshOfTheMiddleFloor_KeepsThreeItems_MapsTheNewFloorAtIndexOne_AndLeavesTheOthersReferenceIdentical()
	{
		// Arrange
		var scenario = await Scenario.LoadedAsync();
		var before = scenario.ViewModel.FloorList.Items.ToArray();

		// Act
		await scenario.UpdateMiddleFloorAsync();

		// Assert
		var items = scenario.ViewModel.FloorList.Items;
		items.Should().HaveCount(3);
		items[1].Should().NotBeSameAs(before[1]);
		items[1].Id.Should().Be(scenario.RefreshedFloor.Id);
		items[1].Name.Should().Be(scenario.RefreshedFloor.Name);
		items[1].Name.Should().NotBe(before[1].Name);
		items[0].Should().BeSameAs(before[0]);
		items[2].Should().BeSameAs(before[2]);
	}

	[Fact]
	public async Task RefreshOfTheMiddleFloor_RaisesExactlyOneReplaceAtIndexOne()
	{
		// Arrange
		var scenario = await Scenario.LoadedAsync();
		var raised = RecordCollectionChanged(scenario.ViewModel.FloorList.Items);

		// Act
		await scenario.UpdateMiddleFloorAsync();

		// Assert
		raised.Should().ContainSingle();
		raised[0].Action.Should().Be(NotifyCollectionChangedAction.Replace);
		raised[0].NewStartingIndex.Should().Be(1);
		raised[0].OldStartingIndex.Should().Be(1);
	}

	[Fact]
	public async Task RefreshOfTheMiddleFloor_DispatchesNoWholeDetailQuery_AndReadsOnlyThatFloor()
	{
		// Arrange
		var scenario = await Scenario.LoadedAsync();

		// Act
		await scenario.UpdateMiddleFloorAsync();

		// Assert
		scenario.WholeDetailQueryCount().Should().Be(1, "the only whole-detail query is the load's");
		await scenario.Dispatcher
			.Received(1)
			.QueryAsync(
				new GetFloorDetailQuery(scenario.DungeonId, scenario.Floors[1].Id),
				Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task RefreshOfTheMiddleFloor_ReplacesThatFloorInTheDungeonProjection_AndLeavesTheOthersReferenceIdentical()
	{
		// Arrange
		var scenario = await Scenario.LoadedAsync();
		var before = scenario.ViewModel.Dungeon!;

		// Act
		await scenario.UpdateMiddleFloorAsync();

		// Assert
		var dungeon = scenario.ViewModel.Dungeon!;
		dungeon.Floors.Should().HaveCount(3);
		dungeon.Floors[1].Should().BeSameAs(scenario.RefreshedFloor);
		dungeon.Floors[0].Should().BeSameAs(before.Floors[0]);
		dungeon.Floors[2].Should().BeSameAs(before.Floors[2]);
	}

	[Fact]
	public async Task Refresh_WhenTheFloorScopedReadReturnsNull_FallsBackToTheWholeDetailQueryAndRaisesNoReplace()
	{
		// Arrange
		var scenario = await Scenario.LoadedAsync(refreshedFloorIsMissing: true);
		var raised = RecordCollectionChanged(scenario.ViewModel.FloorList.Items);

		// Act
		await scenario.UpdateMiddleFloorAsync();

		// Assert
		scenario.WholeDetailQueryCount().Should().Be(2, "the load's query and the fallback reload's");
		raised.Should().NotContain(change => change.Action == NotifyCollectionChangedAction.Replace);
	}

	[Fact]
	public async Task Refresh_WhenNoFloorListItemHasTheFloorsId_FallsBackToTheWholeDetailQueryAndRaisesNoReplace()
	{
		// Arrange
		var scenario = await Scenario.LoadedAsync();
		var unknownFloorId = Guid.NewGuid();
		scenario.FloorReadReturns(scenario.RefreshedFloor with { Id = unknownFloorId });
		var raised = RecordCollectionChanged(scenario.ViewModel.FloorList.Items);

		// Act
		await scenario.ViewModel.FloorList.UpdateFloorCommand.ExecuteAsync(
			new UpdateFloorArgs(unknownFloorId, "Renamed", null, null));

		// Assert
		scenario.WholeDetailQueryCount().Should().Be(2, "the load's query and the fallback reload's");
		raised.Should().NotContain(change => change.Action == NotifyCollectionChangedAction.Replace);
	}

	/// <summary>
	///		Reaches the refresh with <c>Dungeon</c> null by loading three floors and then assigning
	///		<see langword="null"/> through the projection's public setter, so the floor list still
	///		holds the floor being refreshed and only the missing projection can trigger the fallback.
	/// </summary>
	[Fact]
	public async Task Refresh_WhenNoDungeonProjectionIsLoaded_FallsBackToTheWholeDetailQueryAndRaisesNoReplace()
	{
		// Arrange
		var scenario = await Scenario.LoadedAsync();
		scenario.ViewModel.Dungeon = null;
		var raised = RecordCollectionChanged(scenario.ViewModel.FloorList.Items);

		// Act
		await scenario.UpdateMiddleFloorAsync();

		// Assert
		scenario.WholeDetailQueryCount().Should().Be(2, "the load's query and the fallback reload's");
		raised.Should().NotContain(change => change.Action == NotifyCollectionChangedAction.Replace);
	}

	// -- Helpers ------------------------------------------------------------------------------

	private static AppDispatcher DispatcherReturning(bool succeed)
	{
		var dispatcher = Substitute.For<AppDispatcher>();

		dispatcher
			.SendAsync(Arg.Any<ICommand>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(succeed
				? Result.Success()
				: Result.Failure("Rejected by the test.", ErrorCategory.Validation)));

		dispatcher
			.SendAsync(Arg.Any<ICommand<Guid>>(), Arg.Any<CancellationToken>())
			.Returns(Task.FromResult(succeed
				? Result<Guid>.Success(Guid.NewGuid())
				: Result<Guid>.Failure("Rejected by the test.", ErrorCategory.Validation)));

		return dispatcher;
	}

	private static IDungeonEditingContext LoadedContext()
	{
		var context = Substitute.For<IDungeonEditingContext>();
		context.DungeonId.Returns(Guid.NewGuid());
		context.NoDungeonIsLoaded().Returns(false);
		return context;
	}

	private static Task ExecuteFloorScopedAsync(
		string command,
		AppDispatcher dispatcher,
		IDungeonEditingContext context,
		Guid floorId)
	{
		var canvas = new MapCanvasViewModel(dispatcher, context);
		var floorList = new FloorListViewModel(dispatcher, context);
		var position = new CanvasPoint(1f, 2f);

		return command switch
		{
			nameof(MapCanvasViewModel.AddAreaCommand) =>
				canvas.AddAreaCommand.ExecuteAsync(floorId),
			nameof(MapCanvasViewModel.RemoveAreaCommand) =>
				canvas.RemoveAreaCommand.ExecuteAsync(new RemoveAreaArgs(floorId, Guid.NewGuid())),
			nameof(MapCanvasViewModel.UpdateAreaCommand) =>
				canvas.UpdateAreaCommand.ExecuteAsync(new UpdateAreaArgs(floorId, Guid.NewGuid(), "Area", null, null)),
			nameof(MapCanvasViewModel.AddAreaWithOutlineCommand) =>
				canvas.AddAreaWithOutlineCommand.ExecuteAsync(new AddAreaWithOutlineArgs(floorId, [])),
			nameof(MapCanvasViewModel.MoveAreaOutlineCommand) =>
				canvas.MoveAreaOutlineCommand.ExecuteAsync(new MoveAreaOutlineArgs(floorId, Guid.NewGuid(), [])),
			nameof(MapCanvasViewModel.AddPlacedObjectCommand) =>
				canvas.AddPlacedObjectCommand.ExecuteAsync(new PlaceObjectArgs(floorId, ObjectKind.Chest, position, ObjectRotation.None, 1f)),
			nameof(MapCanvasViewModel.RemovePlacedObjectCommand) =>
				canvas.RemovePlacedObjectCommand.ExecuteAsync(new RemovePlacedObjectArgs(floorId, Guid.NewGuid())),
			nameof(MapCanvasViewModel.UpdatePlacedObjectCommand) =>
				canvas.UpdatePlacedObjectCommand.ExecuteAsync(new UpdatePlacedObjectArgs(floorId, Guid.NewGuid(), position, ObjectRotation.None)),
			nameof(FloorListViewModel.UpdateFloorCommand) =>
				floorList.UpdateFloorCommand.ExecuteAsync(new UpdateFloorArgs(floorId, "Floor", null, null)),
			_ => throw new ArgumentOutOfRangeException(nameof(command), command, "Not a floor-scoped command."),
		};
	}

	private static Task ExecuteFullReloadAsync(
		string command,
		AppDispatcher dispatcher,
		IDungeonEditingContext context,
		Guid floorId)
	{
		var floorList = new FloorListViewModel(dispatcher, context);
		var header = new DungeonHeaderViewModel(dispatcher, context);

		return command switch
		{
			nameof(FloorListViewModel.AddFloorCommand) =>
				floorList.AddFloorCommand.ExecuteAsync(null),
			nameof(FloorListViewModel.RemoveFloorCommand) =>
				floorList.RemoveFloorCommand.ExecuteAsync(floorId),
			nameof(FloorListViewModel.ReorderFloorCommand) =>
				floorList.ReorderFloorCommand.ExecuteAsync(new ReorderFloorArgs(floorId, 2)),
			nameof(DungeonHeaderViewModel.SaveCommand) =>
				header.SaveCommand.ExecuteAsync(null),
			_ => throw new ArgumentOutOfRangeException(nameof(command), command, "Not a full-reload command."),
		};
	}

	private static List<NotifyCollectionChangedEventArgs> RecordCollectionChanged(INotifyCollectionChanged collection)
	{
		var raised = new List<NotifyCollectionChangedEventArgs>();
		collection.CollectionChanged += (_, args) => raised.Add(args);
		return raised;
	}

	/// <summary>
	///		A <see cref="DungeonDetailViewModel"/> over a substituted dispatcher that serves a
	///		three-floor detail, accepts every command, and answers the floor-scoped read with
	///		<see cref="RefreshedFloor"/>: the middle floor with a changed name.
	/// </summary>
	private sealed class Scenario
	{
		public Scenario(bool refreshedFloorIsMissing = false)
		{
			Floors =
			[
				new FloorDto(Guid.NewGuid(), 1, "First", null, null, [], []),
				new FloorDto(Guid.NewGuid(), 2, "Second", null, null, [], []),
				new FloorDto(Guid.NewGuid(), 3, "Third", null, null, [], []),
			];

			RefreshedFloor = Floors[1] with { Name = "Renamed" };

			Dispatcher = DispatcherReturning(succeed: true);

			Dispatcher
				.QueryAsync(Arg.Any<IQuery<DungeonDetailDto?>>(), Arg.Any<CancellationToken>())
				.Returns(_ => Task.FromResult<DungeonDetailDto?>(new DungeonDetailDto(
					DungeonId,
					"Dungeon",
					null,
					null,
					Floors.Count,
					FixedTimestamp,
					FixedTimestamp,
					Floors)));

			FloorReadReturns(refreshedFloorIsMissing ? null : RefreshedFloor);

			ViewModel = new DungeonDetailViewModel(
				Dispatcher,
				Substitute.For<INavigationService>());
		}

		public Guid DungeonId { get; } = Guid.NewGuid();

		public IReadOnlyList<FloorDto> Floors { get; }

		public FloorDto RefreshedFloor { get; }

		public AppDispatcher Dispatcher { get; }

		public DungeonDetailViewModel ViewModel { get; }

		public static async Task<Scenario> LoadedAsync(bool refreshedFloorIsMissing = false)
		{
			var scenario = new Scenario(refreshedFloorIsMissing);
			await scenario.ViewModel.LoadDungeonCommand.ExecuteAsync(scenario.DungeonId);
			return scenario;
		}

		public void FloorReadReturns(FloorDto? floor) =>
			Dispatcher
				.QueryAsync(Arg.Any<IQuery<FloorDto?>>(), Arg.Any<CancellationToken>())
				.Returns(Task.FromResult(floor));

		public Task UpdateMiddleFloorAsync() =>
			ViewModel.FloorList.UpdateFloorCommand.ExecuteAsync(
				new UpdateFloorArgs(Floors[1].Id, RefreshedFloor.Name, null, null));

		public int WholeDetailQueryCount() =>
			Dispatcher
				.ReceivedCalls()
				.Count(call =>
					call.GetArguments().FirstOrDefault() is GetDungeonDetailQuery);
	}
}
