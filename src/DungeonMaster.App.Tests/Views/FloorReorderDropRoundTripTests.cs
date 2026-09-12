using System.Reflection;

using DungeonMaster.App.Controls;
using DungeonMaster.App.Core.Presentation;
using DungeonMaster.App.Tests.Hosting;

namespace DungeonMaster.App.Tests.Views;

/// <summary>
///		Drives the application half of the floor-reorder drop path over REAL controls and REAL MAUI
///		drag-and-drop event arguments: the payload <c>FloorSidebarItem.OnDragStarting</c> writes is
///		the payload <c>FloorSidebarItem.OnDrop</c> reads back, and a drop on a different floor
///		raises <c>ReorderRequested</c> carrying the dragged floor's id and the TARGET FLOOR'S
///		1-BASED NUMBER.
/// </summary>
/// <remarks>
///		<para>
///		What this can and cannot reach, stated so no later reader mistakes its scope. It covers
///		everything from <c>OnDragStarting</c> writing the data package to <c>OnDrop</c> deciding.
///		It does NOT cover whether Windows delivers <c>DragOver</c> and <c>Drop</c> to the
///		application at all: that is decided inside WinUI and MAUI's
///		<c>GesturePlatformManager.Windows.cs</c>, needs a real pointer-driven drag session, and is
///		verified by the human observation this session's T1 protocol describes.
///		</para>
///		<para>
///		A host is required: nothing deriving from <c>Microsoft.Maui.Controls.Element</c> constructs
///		off the host's STA thread, and <c>FloorSidebarItem</c> is a <c>ContentView</c>.
///		</para>
///		<para>
///		The two handlers are private and are reached by reflection on purpose. They are the XAML's
///		own <c>DragStarting</c> and <c>Drop</c> handler names (contract C1 section 3 freezes those
///		names), so a rename reddens this test, which is the coupling wanted here.
///		</para>
/// </remarks>
[Collection(MauiHostCollection.Name)]
public sealed class FloorReorderDropRoundTripTests(MauiHostFixture host)
{
	private static FloorItem Floor(Guid id, int number) =>
		new(id, number, $"Floor {number}", null, null, [], []);

	private static void InvokeHandler(FloorSidebarItem item, string name, object args) =>
		typeof(FloorSidebarItem)
			.GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)!
			.Invoke(item, [null, args]);

	/// <summary>
	///		The success row of contract C1 section 1. The target below is floor NUMBER 7 while the
	///		dragged floor is number 3, so a path that carried an index rather than a number would
	///		carry something other than 7 and redden this assertion.
	/// </summary>
	[Fact]
	public void DroppingAFloorOnADifferentFloorRequestsAReorderToTheTargetsNumber()
	{
		host.Invoke(() =>
		{
			var draggedId = Guid.NewGuid();
			var source = new FloorSidebarItem { Floor = Floor(draggedId, 3) };
			var target = new FloorSidebarItem { Floor = Floor(Guid.NewGuid(), 7) };

			(Guid DraggedFloorId, int TargetNumber)? captured = null;
			target.ReorderRequested += (_, args) => captured = args;

			var starting = new DragStartingEventArgs();
			InvokeHandler(source, "OnDragStarting", starting);

			InvokeHandler(target, "OnDrop", new DropEventArgs(starting.Data.View));

			Assert.NotNull(captured);
			Assert.Equal(draggedId, captured!.Value.DraggedFloorId);
			Assert.Equal(7, captured.Value.TargetNumber);
		});
	}

	/// <summary>
	///		Dropping a floor on itself is one of contract C1 section 1's four silent returns: no
	///		reorder is requested and no error is raised.
	/// </summary>
	[Fact]
	public void DroppingAFloorOnItselfRequestsNoReorder()
	{
		host.Invoke(() =>
		{
			var floorId = Guid.NewGuid();
			var item = new FloorSidebarItem { Floor = Floor(floorId, 3) };

			var raised = false;
			item.ReorderRequested += (_, _) => raised = true;

			var starting = new DragStartingEventArgs();
			InvokeHandler(item, "OnDragStarting", starting);

			InvokeHandler(item, "OnDrop", new DropEventArgs(starting.Data.View));

			Assert.False(raised);
		});
	}

	/// <summary>
	///		An EMPTY data package requests no reorder. This is the shape MAUI hands the application
	///		when its Windows gesture manager cannot recover the source package: <c>HandleDrop</c>
	///		reads <c>null</c> and <c>DropEventArgs</c>' internal constructor substitutes
	///		<c>new DataPackageView(new DataPackage())</c>, so the platform losing the payload and the
	///		user dropping something irrelevant arrive at this handler looking identical.
	/// </summary>
	[Fact]
	public void DroppingAnEmptyDataPackageRequestsNoReorder()
	{
		host.Invoke(() =>
		{
			var item = new FloorSidebarItem { Floor = Floor(Guid.NewGuid(), 7) };

			var raised = false;
			item.ReorderRequested += (_, _) => raised = true;

			InvokeHandler(item, "OnDrop", new DropEventArgs(new DataPackage().View));

			Assert.False(raised);
		});
	}
}
