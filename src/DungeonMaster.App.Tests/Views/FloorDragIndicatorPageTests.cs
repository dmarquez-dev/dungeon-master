using System.Reflection;

using DungeonMaster.App.Controls;
using DungeonMaster.App.Tests.Hosting;
using DungeonMaster.App.ViewModels;
using DungeonMaster.App.Views;
using DungeonMaster.Application.Features.Dungeons.Queries.GetDungeonDetail;

namespace DungeonMaster.App.Tests.Views;

/// <summary>
///		Pins what a floor drag does to the rows of <c>DungeonDetailPage</c>'s floor list: the
///		dragged row is dimmed and never hidden, no drag handler adds, removes or reorders a row,
///		and every terminal path hides the insertion line and puts every row back at full opacity.
/// </summary>
/// <remarks>
///		<para>
///		Every test resolves a real page from the running application's container and populates
///		it through its view model, as <see cref="DungeonDetailPageFloorListTests"/> does, so the
///		rows asserted on are the ones the templated <c>BindableLayout</c> built and the handlers
///		reached are the ones the row template wires.
///		</para>
///		<para>
///		The handlers are private and are reached by reflection on purpose: they are the names
///		the card's and the page's XAML wire, so a rename reddens these tests. The page is never
///		laid out and no event args carry platform arguments, so every <c>GetPosition</c> here
///		returns <see langword="null"/>; these tests cover the opacity and child-list effects of
///		the handlers, not where the line is drawn, which only a real drag can show.
///		</para>
/// </remarks>
[Collection(MauiHostCollection.Name)]
public sealed class FloorDragIndicatorPageTests(MauiHostFixture host)
{
	private const double DimmedOpacity = 0.4;

	private static readonly DateTime FixedInstant = new(
		2026,
		1,
		1,
		0,
		0,
		0,
		DateTimeKind.Utc);

	/// <summary>
	///		Starting a drag on the second row dims that row to 0.4 and leaves the first and third
	///		rows at full opacity.
	/// </summary>
	[Fact]
	public void DragStartingOnTheSecondRow_DimsItAndLeavesTheOtherRowsAtFullOpacity()
	{
		host.Invoke(() =>
		{
			var page = PopulatedPage();
			var rows = Rows(page);

			InvokeRowHandler(rows[1], "OnDragStarting", new DragStartingEventArgs());

			Assert.Equal(1, rows[0].Opacity);
			Assert.Equal(DimmedOpacity, rows[1].Opacity);
			Assert.Equal(1, rows[2].Opacity);
		});
	}

	/// <summary>
	///		Dragging the second row over the third leaves the list's children the same instances
	///		in the same order as before the drag.
	/// </summary>
	[Fact]
	public void DragOverAnotherRow_LeavesTheListChildrenTheSameInstancesInTheSameOrder()
	{
		host.Invoke(() =>
		{
			var page = PopulatedPage();
			var before = Children(page);
			var rows = Rows(page);

			var starting = new DragStartingEventArgs();
			InvokeRowHandler(rows[1], "OnDragStarting", starting);
			InvokeRowHandler(rows[2], "OnDragOver", new DragEventArgs(starting.Data));

			var after = Children(page);

			Assert.Equal(before.Count, after.Count);

			for (var index = 0; index < before.Count; index++)
			{
				Assert.Same(before[index], after[index]);
			}
		});
	}

	/// <summary>
	///		A self-drop that the platform never follows with <c>DropCompleted</c> still leaves every
	///		row at full opacity and the list's children unchanged.
	/// </summary>
	[Fact]
	public void SelfDropWithNoDropCompleted_LeavesEveryRowAtFullOpacityAndTheChildrenUnchanged()
	{
		host.Invoke(() =>
		{
			var page = PopulatedPage();
			var before = Children(page);
			var rows = Rows(page);

			var starting = new DragStartingEventArgs();
			InvokeRowHandler(rows[1], "OnDragStarting", starting);
			InvokeRowHandler(rows[1], "OnDragOver", new DragEventArgs(starting.Data));
			InvokeRowHandler(rows[1], "OnDrop", new DropEventArgs(starting.Data.View));

			Assert.All(
				rows,
				row => Assert.Equal(1, row.Opacity));

			var after = Children(page);

			Assert.Equal(before.Count, after.Count);

			for (var index = 0; index < before.Count; index++)
			{
				Assert.Same(before[index], after[index]);
			}
		});
	}

	/// <summary>
	///		A list drop that carries no position hides the line and puts every row back at full
	///		opacity on its early return.
	/// </summary>
	[Fact]
	public void ListDropWithNoPosition_HidesTheLineAndRestoresEveryRowToFullOpacity()
	{
		host.Invoke(() =>
		{
			var page = PopulatedPage();
			var rows = Rows(page);
			var line = IndicatorLine(page);

			var starting = new DragStartingEventArgs();
			InvokeRowHandler(rows[1], "OnDragStarting", starting);
			line.Opacity = 1;

			InvokePageHandler(page, "OnFloorListDrop", new DropEventArgs(starting.Data.View));

			Assert.Equal(0, line.Opacity);
			Assert.All(
				rows,
				row => Assert.Equal(1, row.Opacity));
		});
	}

	/// <summary>
	///		A drag start after a lost drag hides the line it left, resets the row it left dimmed,
	///		and then dims only the row now being dragged.
	/// </summary>
	[Fact]
	public void DragStartingAfterALostDrag_HidesTheLineAndResetsTheStaleRowBeforeDimmingTheNewOne()
	{
		host.Invoke(() =>
		{
			var page = PopulatedPage();
			var rows = Rows(page);
			var line = IndicatorLine(page);

			line.Opacity = 1;
			rows[0].Opacity = DimmedOpacity;

			InvokeRowHandler(rows[1], "OnDragStarting", new DragStartingEventArgs());

			Assert.Equal(0, line.Opacity);
			Assert.Equal(1, rows[0].Opacity);
			Assert.Equal(DimmedOpacity, rows[1].Opacity);
		});
	}

	/// <summary>
	///		A card drop that the reorder decision declines still hides the line and puts every row
	///		back at full opacity.
	/// </summary>
	[Fact]
	public void CardDropTheDecisionDeclines_HidesTheLineAndRestoresEveryRowToFullOpacity()
	{
		host.Invoke(() =>
		{
			var page = PopulatedPage();
			var rows = Rows(page);
			var line = IndicatorLine(page);

			InvokeRowHandler(rows[1], "OnDragStarting", new DragStartingEventArgs());
			line.Opacity = 1;

			// An empty package carries no FloorId key, which FloorReorderDrop.Decide declines.
			InvokeRowHandler(rows[2], "OnDrop", new DropEventArgs(new DataPackage().View));

			Assert.Equal(0, line.Opacity);
			Assert.All(
				rows,
				row => Assert.Equal(1, row.Opacity));
		});
	}

	// Must run inside host.Invoke: the page is an Element.
	private DungeonDetailPage PopulatedPage()
	{
		var page = host.Resolve<DungeonDetailPage>();
		var viewModel = Assert.IsType<DungeonDetailViewModel>(page.BindingContext);

		viewModel.FloorList.Populate(Detail());

		Assert.Equal(3, viewModel.FloorList.Items.Count);

		return page;
	}

	private static VerticalStackLayout Container(DungeonDetailPage page)
	{
		var container = page.FindByName<VerticalStackLayout>("FloorListContainer");

		Assert.NotNull(container);

		return container;
	}

	private static List<IView> Children(DungeonDetailPage page) =>
		[.. Container(page).Children];

	private static List<FloorSidebarItem> Rows(DungeonDetailPage page)
	{
		var rows = Container(page).Children.Select(child => Assert.IsType<FloorSidebarItem>(child)).ToList();

		Assert.Equal(3, rows.Count);

		return rows;
	}

	private static BoxView IndicatorLine(DungeonDetailPage page)
	{
		var line = page.FindByName<BoxView>("FloorDropIndicatorLine");

		Assert.NotNull(line);

		return line;
	}

	private static void InvokeRowHandler(FloorSidebarItem item, string name, object args) =>
		typeof(FloorSidebarItem)
			.GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)!
			.Invoke(item, [null, args]);

	private static void InvokePageHandler(DungeonDetailPage page, string name, object args) =>
		typeof(DungeonDetailPage)
			.GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)!
			.Invoke(page, [null, args]);

	private static DungeonDetailDto Detail() =>
		new(
			Guid.NewGuid(),
			"Sunken Vault",
			null,
			null,
			3,
			FixedInstant,
			FixedInstant,
			[
				Floor(1),
				Floor(2),
				Floor(3),
			]);

	private static FloorDto Floor(int number) =>
		new(
			Guid.NewGuid(),
			number,
			$"Floor {number}",
			null,
			null,
			[],
			[]);
}
