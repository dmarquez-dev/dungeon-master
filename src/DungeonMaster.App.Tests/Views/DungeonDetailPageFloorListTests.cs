using System.Reflection;

using DungeonMaster.App.Controls;
using DungeonMaster.App.Tests.Hosting;
using DungeonMaster.App.ViewModels;
using DungeonMaster.App.Views;
using DungeonMaster.Application.Features.Dungeons.Queries.GetDungeonDetail;

namespace DungeonMaster.App.Tests.Views;

/// <summary>
///		Pins the templated floor list of <c>DungeonDetailPage</c> (MASTER-108 / UI-016) and the
///		bindings that make <c>DungeonDetailViewModel.Editor</c> the one owner of which floor is
///		expanded and which area is selected (MASTER-098 / UIKIT-011).
/// </summary>
/// <remarks>
///		<para>
///		Every test resolves a real page from the running application's container, exactly as
///		<see cref="DungeonDetailPageTeardownTests"/> does, and drives it only through its view
///		model: <c>FloorList.Populate</c>, an assignment into <c>FloorList.Items</c>, or a write to
///		<c>Editor</c>. The rows are then read back off <c>FloorListContainer</c>, so what is
///		asserted is what the templated <c>BindableLayout</c> built, not what the page meant to build.
///		</para>
///		<para>
///		No query is dispatched: <c>Populate</c> is a pure translation, and the chevron tests reach
///		only the page's selection handler, which reads <c>FloorList.Items</c> and dispatches nothing.
///		</para>
/// </remarks>
[Collection(MauiHostCollection.Name)]
public sealed class DungeonDetailPageFloorListTests(MauiHostFixture host)
{
	private static readonly DateTime FixedInstant = new(
		2026,
		1,
		1,
		0,
		0,
		0,
		DateTimeKind.Utc);

	/// <summary>
	///		After <c>Populate</c> of a three-floor detail, the container holds exactly three children,
	///		each a <see cref="FloorSidebarItem"/> whose <c>Floor</c> is the very instance
	///		<c>FloorList.Items</c> holds at the same index.
	/// </summary>
	[Fact]
	public void Populate_WithThreeFloors_BuildsOneRowPerFloorBoundToTheSameItem()
	{
		host.Invoke(() =>
		{
			var (page, viewModel) = PopulatedPage();

			var rows = Rows(page);

			Assert.Equal(3, rows.Count);

			for (var index = 0; index < rows.Count; index++)
			{
				Assert.Same(viewModel.FloorList.Items[index], rows[index].Floor);
			}
		});
	}

	/// <summary>
	///		Every templated row is a level-3 heading in the sidebar's outline.
	/// </summary>
	[Fact]
	public void Populate_EveryRowCarriesHeadingLevel3()
	{
		host.Invoke(() =>
		{
			var (page, _) = PopulatedPage();

			var rows = Rows(page);

			Assert.Equal(3, rows.Count);

			foreach (var row in rows)
			{
				Assert.Equal(SemanticHeadingLevel.Level3, SemanticProperties.GetHeadingLevel(row));
			}
		});
	}

	/// <summary>
	///		Replacing the floor at index 1 touches only that row: rows 0 and 2 are the same
	///		instances as before, and row 1 is a row bound to the replacement.
	/// </summary>
	/// <remarks>
	///		Whether row 1 is a new instance or the old one re-bound is the framework's choice and is
	///		deliberately not asserted; what UI-016 asks for is that the OTHER rows survive.
	/// </remarks>
	[Fact]
	public void ReplacingOneFloor_LeavesEveryOtherRowInstanceAndBindsThatRowToTheReplacement()
	{
		host.Invoke(() =>
		{
			var (page, viewModel) = PopulatedPage();

			var before = Rows(page);
			var replacement = viewModel.FloorList.Items[1] with { Name = "Renamed" };

			viewModel.FloorList.Items[1] = replacement;

			var after = Rows(page);

			Assert.Equal(3, after.Count);
			Assert.Same(before[0], after[0]);
			Assert.Same(before[2], after[2]);
			Assert.Same(replacement, after[1].Floor);
		});
	}

	/// <summary>
	///		A second <c>Populate</c>, the shape every full reload has, replaces every row with a new
	///		instance. <c>ReorderFocusedFloorAsync</c> restores keyboard focus for exactly this reason.
	/// </summary>
	[Fact]
	public void PopulatingAgain_ReplacesEveryRowInstance()
	{
		host.Invoke(() =>
		{
			var (page, viewModel) = PopulatedPage();

			var before = Rows(page);

			viewModel.FloorList.Populate(Detail());

			var after = Rows(page);

			Assert.Equal(3, after.Count);

			for (var index = 0; index < after.Count; index++)
			{
				Assert.NotSame(before[index], after[index]);
			}
		});
	}

	/// <summary>
	///		Selecting the second floor on the editor state expands the second row and only it.
	/// </summary>
	[Fact]
	public void SelectingAFloorOnTheEditor_ExpandsOnlyThatRow()
	{
		host.Invoke(() =>
		{
			var (page, viewModel) = PopulatedPage();

			viewModel.Editor.SelectedFloorId = viewModel.FloorList.Items[1].Id;

			var rows = Rows(page);

			Assert.False(rows[0].IsExpanded);
			Assert.True(rows[1].IsExpanded);
			Assert.False(rows[2].IsExpanded);
		});
	}

	/// <summary>
	///		Clearing the editor's selected floor collapses every row.
	/// </summary>
	[Fact]
	public void ClearingTheSelectedFloorOnTheEditor_CollapsesEveryRow()
	{
		host.Invoke(() =>
		{
			var (page, viewModel) = PopulatedPage();

			viewModel.Editor.SelectedFloorId = viewModel.FloorList.Items[1].Id;

			// Premise: the row was open, so collapsing it is an observed change, not a default.
			Assert.True(Rows(page)[1].IsExpanded);

			viewModel.Editor.SelectedFloorId = null;

			Assert.All(
				Rows(page),
				row => Assert.False(row.IsExpanded));
		});
	}

	/// <summary>
	///		With the second floor selected, replacing it leaves the second row, now bound to the
	///		replacement, expanded.
	/// </summary>
	[Fact]
	public void ReplacingTheSelectedFloor_LeavesItsRowBoundToTheReplacementExpanded()
	{
		host.Invoke(() =>
		{
			var (page, viewModel) = PopulatedPage();

			viewModel.Editor.SelectedFloorId = viewModel.FloorList.Items[1].Id;

			var replacement = viewModel.FloorList.Items[1] with { Name = "Renamed" };

			viewModel.FloorList.Items[1] = replacement;

			var row = Rows(page)[1];

			Assert.Same(replacement, row.Floor);
			Assert.True(row.IsExpanded);
		});
	}

	/// <summary>
	///		With the two-area floor selected, focusing its first area on the editor state selects
	///		that area's row and leaves the second unselected.
	/// </summary>
	[Fact]
	public void FocusingAnAreaOnTheEditor_SelectsOnlyThatAreaRow()
	{
		host.Invoke(() =>
		{
			var (page, viewModel) = PopulatedPage();

			var floor = viewModel.FloorList.Items[1];

			Assert.Equal(2, floor.Areas.Count);

			viewModel.Editor.SelectedFloorId = floor.Id;
			viewModel.Editor.FocusedAreaId = floor.Areas[0].Id;

			var areaRows = AreaRows(Rows(page)[1]);

			Assert.Equal(2, areaRows.Count);
			Assert.True(areaRows[0].IsSelected);
			Assert.False(areaRows[1].IsSelected);
		});
	}

	/// <summary>
	///		The chevron of a collapsed row selects that row's floor on the editor and expands the
	///		row: the template's <c>SelectRequested</c> wiring reaches the page.
	/// </summary>
	[Fact]
	public void ChevronOnACollapsedRow_SelectsItsFloorAndExpandsIt()
	{
		var exceptionsBefore = host.HandledExceptions.Count;

		host.Invoke(() =>
		{
			var (page, viewModel) = PopulatedPage();

			var row = Rows(page)[2];

			Assert.False(row.IsExpanded);

			InvokeHandler(row, "OnExpandToggleClicked", EventArgs.Empty);

			Assert.Equal(viewModel.FloorList.Items[2].Id, viewModel.Editor.SelectedFloorId);
			Assert.True(row.IsExpanded);
		});

		Assert.Equal(exceptionsBefore, host.HandledExceptions.Count);
	}

	/// <summary>
	///		The chevron of the row that is already selected clears the editor's selected floor and
	///		collapses the row: the toggle-off branch.
	/// </summary>
	[Fact]
	public void ChevronOnTheSelectedRow_ClearsTheSelectionAndCollapsesIt()
	{
		var exceptionsBefore = host.HandledExceptions.Count;

		host.Invoke(() =>
		{
			var (page, viewModel) = PopulatedPage();

			var row = Rows(page)[0];

			InvokeHandler(row, "OnExpandToggleClicked", EventArgs.Empty);

			Assert.True(row.IsExpanded);

			InvokeHandler(row, "OnExpandToggleClicked", EventArgs.Empty);

			Assert.Null(viewModel.Editor.SelectedFloorId);
			Assert.False(row.IsExpanded);
		});

		Assert.Equal(exceptionsBefore, host.HandledExceptions.Count);
	}

	/// <summary>
	///		Every templated row receives all five area commands from the template.
	/// </summary>
	[Fact]
	public void Populate_EveryRowReceivesTheFiveAreaCommands()
	{
		host.Invoke(() =>
		{
			var (page, _) = PopulatedPage();

			var rows = Rows(page);

			Assert.Equal(3, rows.Count);

			foreach (var row in rows)
			{
				Assert.NotNull(row.AreaSelectCommand);
				Assert.NotNull(row.AreaHoverStartedCommand);
				Assert.NotNull(row.AreaHoverEndedCommand);
				Assert.NotNull(row.AreaDeleteCommand);
				Assert.NotNull(row.AreaNotebookCommand);
			}
		});
	}

	// Must run inside host.Invoke: the page is an Element.
	private (DungeonDetailPage Page, DungeonDetailViewModel ViewModel) PopulatedPage()
	{
		var page = host.Resolve<DungeonDetailPage>();
		var viewModel = Assert.IsType<DungeonDetailViewModel>(page.BindingContext);

		viewModel.FloorList.Populate(Detail());

		Assert.Equal(3, viewModel.FloorList.Items.Count);

		return (page, viewModel);
	}

	private static List<FloorSidebarItem> Rows(DungeonDetailPage page)
	{
		var container = page.FindByName<VerticalStackLayout>("FloorListContainer");

		Assert.NotNull(container);

		return container.Children.Select(child => Assert.IsType<FloorSidebarItem>(child)).ToList();
	}

	private static List<AreaSidebarItem> AreaRows(FloorSidebarItem row)
	{
		var container = row.FindByName<VerticalStackLayout>("AreaContainer");

		Assert.NotNull(container);

		return container.Children.Select(child => Assert.IsType<AreaSidebarItem>(child)).ToList();
	}

	private static void InvokeHandler(FloorSidebarItem item, string name, object args) =>
		typeof(FloorSidebarItem)
			.GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)!
			.Invoke(item, [null, args]);

	// Three floors; the second carries two areas so the area-selection binding has a near miss.
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
				Floor(1, []),
				Floor(2, [Area("Crypt"), Area("Ossuary")]),
				Floor(3, []),
			]);

	private static FloorDto Floor(int number, IReadOnlyList<AreaDto> areas) =>
		new(
			Guid.NewGuid(),
			number,
			$"Floor {number}",
			null,
			null,
			areas,
			[]);

	private static AreaDto Area(string name) =>
		new(
			Guid.NewGuid(),
			name,
			null,
			null,
			[]);
}
