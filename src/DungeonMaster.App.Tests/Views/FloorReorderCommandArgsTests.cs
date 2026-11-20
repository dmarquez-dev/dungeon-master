using System.Reflection;

using DungeonMaster.App.Controls;
using DungeonMaster.App.Core.Presentation;
using DungeonMaster.App.Tests.Hosting;
using DungeonMaster.App.ViewModels;
using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Application.Common.Results;
using DungeonMaster.Application.Features.Dungeons.Commands.ReorderFloor;

using NSubstitute;

// Microsoft.Maui.Dispatching also publishes an IDispatcher. FloorListViewModel itself disambiguates
// the same way, at FloorListViewModel.cs:10.
using AppDispatcher = DungeonMaster.Application.Common.Dispatch.IDispatcher;

namespace DungeonMaster.App.Tests.Views;

/// <summary>
///		Closes the last hop of the floor-reorder path: what <c>ReorderFloorCommand</c> actually
///		receives. The decision is covered in <c>DungeonMaster.Tests</c> and the control's
///		<c>ReorderRequested</c> is covered in <c>FloorReorderDropRoundTripTests</c>; between the two
///		sat the line that builds <c>ReorderFloorArgs</c> from the raised tuple
///		(<c>DungeonDetailPage.xaml.cs</c>, the <c>item.ReorderRequested</c> handler, post-edit
///		<c>:860-861</c>, and the placeholder drop closure at post-edit <c>:2283-2284</c>), where an
///		index-for-number slip was caught by nothing.
/// </summary>
/// <remarks>
///		<para>
///		The fixture is built so number and index CANNOT be confused by coincidence: eleven floors,
///		and the target is floor NUMBER 7 sitting at INDEX 6. A path that carried an index would
///		carry 6 and redden the assertion. The fixture's own premise is asserted first, so a fixture
///		that stopped expressing the distinction fails loudly instead of passing vacuously.
///		</para>
///		<para>
///		The one line this test stands in for is the page's event-handler lambda. Everything on
///		either side of it is real: a real <c>FloorSidebarItem</c> raising <c>ReorderRequested</c>
///		from its real <c>OnDrop</c> against a real MAUI <c>DataPackage</c>, and a real
///		<c>FloorListViewModel</c> constructing the real <c>ReorderFloorCommand</c>
///		(<c>FloorListViewModel.cs:190</c>). Only the dispatcher and the editing context are fakes.
///		</para>
/// </remarks>
[Collection(MauiHostCollection.Name)]
public sealed class FloorReorderCommandArgsTests(MauiHostFixture host)
{
	private static FloorItem Floor(Guid id, int number) =>
		new(id, number, $"Floor {number}", null, null, [], []);

	private static void InvokeHandler(FloorSidebarItem item, string name, object args) =>
		typeof(FloorSidebarItem)
			.GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)!
			.Invoke(item, [null, args]);

	/// <summary>
	///		A drop of floor 3 onto floor 7 reaches <c>ReorderFloorCommand</c> carrying the DRAGGED
	///		floor's id and the TARGET FLOOR'S 1-BASED NUMBER, together with the dungeon id the
	///		editing context owns.
	/// </summary>
	[Fact]
	public async Task DroppingAFloorOnAnotherSendsTheReorderCommandWithTheTargetsNumberNotItsIndex()
	{
		var dungeonId = Guid.NewGuid();
		var draggedId = Guid.NewGuid();

		// Eleven floors, numbered 1..11. The target is number 7 and therefore sits at index 6.
		var floors = Enumerable.Range(1, 11).Select(n => Floor(Guid.NewGuid(), n)).ToList();
		const int TargetIndex = 6;
		var targetFloor = floors[TargetIndex];

		Assert.Equal(7, targetFloor.Number);
		Assert.NotEqual(TargetIndex, targetFloor.Number);

		// The real control decides, exactly as it does on a real drop.
		(Guid DraggedFloorId, int TargetNumber)? raised = null;

		host.Invoke(() =>
		{
			var source = new FloorSidebarItem { Floor = Floor(draggedId, 3) };
			var target = new FloorSidebarItem { Floor = targetFloor };

			target.ReorderRequested += (_, args) => raised = args;

			var starting = new DragStartingEventArgs();
			InvokeHandler(source, "OnDragStarting", starting);
			InvokeHandler(target, "OnDrop", new DropEventArgs(starting.Data.View));
		});

		Assert.NotNull(raised);

		ReorderFloorCommand? sent = null;

		var dispatcher = Substitute.For<AppDispatcher>();
		dispatcher
			.SendAsync(Arg.Any<ICommand>(), Arg.Any<CancellationToken>())
			.Returns(call =>
			{
				sent = call.Arg<ICommand>() as ReorderFloorCommand;

				return Task.FromResult(Result.Success());
			});

		var context = Substitute.For<IDungeonEditingContext>();
		context.DungeonId.Returns(dungeonId);
		context.NoDungeonIsLoaded().Returns(false);
		context.ReloadAsync().Returns(Task.CompletedTask);

		var viewModel = new FloorListViewModel(dispatcher, context);

		// The page's own hop, verbatim: the raised tuple becomes ReorderFloorArgs and goes to the
		// command. This is the line DungeonDetailPage builds at its ReorderRequested handler.
		await viewModel.ReorderFloorCommand.ExecuteAsync(
			new ReorderFloorArgs(raised!.Value.DraggedFloorId, raised.Value.TargetNumber));

		Assert.NotNull(sent);
		Assert.Equal(dungeonId, sent!.DungeonId);
		Assert.Equal(draggedId, sent.FloorId);
		Assert.Equal(7, sent.NewNumber);
	}
}
