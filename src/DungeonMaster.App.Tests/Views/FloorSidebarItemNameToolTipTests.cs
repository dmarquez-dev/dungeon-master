using DungeonMaster.App.Controls;
using DungeonMaster.App.Core.Presentation;
using DungeonMaster.App.Tests.Hosting;
using FluentAssertions;

namespace DungeonMaster.App.Tests.Views;

/// <summary>
///		Pins that the floor name in <see cref="FloorSidebarItem"/> carries a tooltip holding exactly
///		the text the label shows, so a name the label tail-truncates can still be read in full.
/// </summary>
/// <remarks>
///		The label is pinned to one line, and a name longer than its column is cut off. The tooltip
///		is the recovery route, so it must hold the full name for a named floor and the same
///		<c>Floor N</c> placeholder the label shows for an unnamed one, and it must follow a rename.
/// </remarks>
[Collection(MauiHostCollection.Name)]
public sealed class FloorSidebarItemNameToolTipTests(MauiHostFixture host)
{
	/// <summary>
	///		A named floor's tooltip is its full name, the same text as the label.
	/// </summary>
	[Fact]
	public void NamedFloor_ToolTipEqualsTheLabelText()
	{
		host.Invoke(() =>
		{
			var item = new FloorSidebarItem { Floor = Floor("The Long Hall of the Forgotten Kings") };
			var label = NameLabel(item);

			label.Text.Should().Be("The Long Hall of the Forgotten Kings");
			ToolTipProperties.GetText(label).Should().Be(label.Text);
		});
	}

	/// <summary>
	///		An unnamed floor's tooltip is the <c>Floor N</c> placeholder the label shows.
	/// </summary>
	[Fact]
	public void UnnamedFloor_ToolTipEqualsThePlaceholderLabelText()
	{
		host.Invoke(() =>
		{
			var item = new FloorSidebarItem { Floor = Floor(null) };
			var label = NameLabel(item);

			label.Text.Should().Be("Floor 3");
			ToolTipProperties.GetText(label).Should().Be(label.Text);
		});
	}

	/// <summary>
	///		Repopulating after a rename moves the tooltip to the new name.
	/// </summary>
	[Fact]
	public void RenamedFloor_ToolTipFollowsTheNewName()
	{
		host.Invoke(() =>
		{
			var item = new FloorSidebarItem { Floor = Floor("Cellar") };

			item.Floor = Floor("Wine Cellar");

			var label = NameLabel(item);

			label.Text.Should().Be("Wine Cellar");
			ToolTipProperties.GetText(label).Should().Be(label.Text);
		});
	}

	private static FloorItem Floor(string? name) =>
		new(
			Guid.NewGuid(),
			3,
			name,
			null,
			null,
			[],
			[]);

	private static Label NameLabel(FloorSidebarItem item)
	{
		var label = item.FindByName<Label>("FloorNameLabel");

		label.Should().NotBeNull("the tests read the floor name label by its x:Name");

		return label!;
	}
}
