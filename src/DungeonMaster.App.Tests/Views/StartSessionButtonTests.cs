using DungeonMaster.App.Tests.Hosting;
using DungeonMaster.App.ViewModels;
using DungeonMaster.App.Views;
using DungeonMaster.Application.Features.Dungeons.Queries.GetDungeonDetail;
using DungeonMaster.Application.Features.Dungeons.Queries.GetDungeons;

namespace DungeonMaster.App.Tests.Views;

/// <summary>
///		Pins A3: the unwired <c>StartSessionButton</c> on <c>DungeonDetailPage</c> and on each
///		<c>DungeonListPage</c> card is disabled and dimmed for every floor count, not only for a
///		dungeon with no floors. Sessions do not exist yet, so a button enabled by a first floor was an
///		affordance with nothing behind it.
/// </summary>
/// <remarks>
///		<para>
///		The evidence is the real button's <c>IsEnabled</c> and <c>Opacity</c> after its binding
///		context resolves, on a page resolved from the running application's container - not an
///		assertion over source text. A host is required because both pages, and the list page's item
///		template content, derive from <c>Microsoft.Maui.Controls.Element</c>.
///		</para>
///		<para>
///		The zero-floor facts are controls: the button was already disabled there before A3, so they
///		pass with or without the fix. They exist to show a floored failure is about the floor count,
///		not about a binding context that never resolved.
///		</para>
/// </remarks>
[Collection(MauiHostCollection.Name)]
public sealed class StartSessionButtonTests(MauiHostFixture host)
{
	private const string StartSessionAutomationId = "StartSessionButton";

	private const double DisabledOpacity = 0.35;

	private const int OpacityPrecision = 3;

	private static readonly DateTime FixedInstant = new(
		2026,
		1,
		1,
		0,
		0,
		0,
		DateTimeKind.Utc);

	/// <summary>
	///		A dungeon with a floor no longer enables the detail page's button.
	/// </summary>
	[Fact]
	public void DetailPageStartSessionButton_WithOneFloor_IsDisabled()
	{
		host.Invoke(() =>
		{
			var button = DetailPageButton(floorCount: 1);

			Assert.False(button.IsEnabled);
		});
	}

	/// <summary>
	///		A dungeon with a floor leaves the detail page's button dimmed to the disabled opacity.
	/// </summary>
	[Fact]
	public void DetailPageStartSessionButton_WithOneFloor_HasDisabledOpacity()
	{
		host.Invoke(() =>
		{
			var button = DetailPageButton(floorCount: 1);

			Assert.Equal(DisabledOpacity, button.Opacity, OpacityPrecision);
		});
	}

	/// <summary>
	///		Control: a dungeon with no floors leaves the detail page's button disabled.
	/// </summary>
	[Fact]
	public void DetailPageStartSessionButton_WithNoFloors_IsDisabled()
	{
		host.Invoke(() =>
		{
			var button = DetailPageButton(floorCount: 0);

			Assert.False(button.IsEnabled);
		});
	}

	/// <summary>
	///		A dungeon card for a dungeon with a floor no longer enables its button.
	/// </summary>
	[Fact]
	public void ListPageStartSessionButton_WithOneFloor_IsDisabled()
	{
		host.Invoke(() =>
		{
			var button = ListPageButton(floorCount: 1);

			Assert.False(button.IsEnabled);
		});
	}

	/// <summary>
	///		A dungeon card for a dungeon with a floor leaves its button dimmed to the disabled opacity.
	/// </summary>
	[Fact]
	public void ListPageStartSessionButton_WithOneFloor_HasDisabledOpacity()
	{
		host.Invoke(() =>
		{
			var button = ListPageButton(floorCount: 1);

			Assert.Equal(DisabledOpacity, button.Opacity, OpacityPrecision);
		});
	}

	/// <summary>
	///		Control: a dungeon card for a dungeon with no floors leaves its button disabled.
	/// </summary>
	[Fact]
	public void ListPageStartSessionButton_WithNoFloors_IsDisabled()
	{
		host.Invoke(() =>
		{
			var button = ListPageButton(floorCount: 0);

			Assert.False(button.IsEnabled);
		});
	}

	/// <summary>
	///		Resolves a real <c>DungeonDetailPage</c>, gives its view model a dungeon with
	///		<paramref name="floorCount"/> floors, and returns the page's start-session button.
	/// </summary>
	private Button DetailPageButton(int floorCount)
	{
		var page = host.Resolve<DungeonDetailPage>();

		var viewModel = Assert.IsType<DungeonDetailViewModel>(page.BindingContext);

		viewModel.Dungeon = new DungeonDetailDto(
			Guid.NewGuid(),
			"Test dungeon",
			null,
			null,
			floorCount,
			FixedInstant,
			FixedInstant,
			[]);

		return FindStartSessionButton(page);
	}

	/// <summary>
	///		Resolves a real <c>DungeonListPage</c>, creates one item of its dungeon list's template
	///		bound to a dungeon with <paramref name="floorCount"/> floors, and returns that item's
	///		start-session button.
	/// </summary>
	private Button ListPageButton(int floorCount)
	{
		var page = host.Resolve<DungeonListPage>();

		var list = Assert.Single(Descendants(page).OfType<CollectionView>());

		var item = Assert.IsAssignableFrom<View>(list.ItemTemplate.CreateContent());

		item.BindingContext = new DungeonSummaryDto(
			Guid.NewGuid(),
			"Test dungeon",
			null,
			floorCount,
			FixedInstant,
			FixedInstant);

		return FindStartSessionButton(item);
	}

	private static Button FindStartSessionButton(Element root)
	{
		var match = Assert.Single(
			Descendants(root).OfType<Button>(),
			candidate => candidate.AutomationId == StartSessionAutomationId);

		return match;
	}

	private static IEnumerable<Element> Descendants(Element root)
	{
		foreach (var child in ((IVisualTreeElement)root).GetVisualChildren())
		{
			if (child is not Element element)
			{
				continue;
			}

			yield return element;

			foreach (var descendant in Descendants(element))
			{
				yield return descendant;
			}
		}
	}
}
