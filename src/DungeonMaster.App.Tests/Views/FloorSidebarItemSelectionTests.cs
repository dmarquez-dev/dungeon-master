using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

using DungeonMaster.App.Controls;
using DungeonMaster.App.Core.Presentation;
using DungeonMaster.App.Tests.Hosting;

namespace DungeonMaster.App.Tests.Views;

/// <summary>
///		Pins that <see cref="FloorSidebarItem"/> asks to be selected and never decides its own
///		expanded state (MASTER-098 / UIKIT-011): the card tap and the chevron each raise
///		<c>SelectRequested</c> once and write nothing to <c>IsExpanded</c>, so a binding on
///		<c>IsExpanded</c> stays the only thing that drives it.
/// </summary>
/// <remarks>
///		The two handlers are private and are reached by reflection, the route
///		<see cref="FloorReorderCommandArgsTests"/> uses for <c>OnDrop</c>.
/// </remarks>
[Collection(MauiHostCollection.Name)]
public sealed class FloorSidebarItemSelectionTests(MauiHostFixture host)
{
	/// <summary>
	///		The chevron raises <c>SelectRequested</c> exactly once and leaves <c>IsExpanded</c> false.
	/// </summary>
	[Fact]
	public void ExpandToggleClicked_RaisesSelectRequestedOnceAndLeavesIsExpandedFalse()
	{
		host.Invoke(() =>
		{
			var item = new FloorSidebarItem { Floor = Floor() };
			var raised = 0;

			item.SelectRequested += (_, _) => raised++;

			InvokeHandler(item, "OnExpandToggleClicked", EventArgs.Empty);

			Assert.Equal(1, raised);
			Assert.False(item.IsExpanded);
		});
	}

	/// <summary>
	///		A card tap with the suppress flag clear raises <c>SelectRequested</c> exactly once and
	///		leaves <c>IsExpanded</c> false.
	/// </summary>
	[Fact]
	public void CardTapped_RaisesSelectRequestedOnceAndLeavesIsExpandedFalse()
	{
		host.Invoke(() =>
		{
			var item = new FloorSidebarItem { Floor = Floor() };
			var raised = 0;

			item.SelectRequested += (_, _) => raised++;

			InvokeHandler(item, "OnCardTapped", new TappedEventArgs(null));

			Assert.Equal(1, raised);
			Assert.False(item.IsExpanded);
		});
	}

	/// <summary>
	///		A one-way binding on <c>IsExpanded</c> still follows its source after the chevron has
	///		been clicked: the click does not displace the binding with a local value.
	/// </summary>
	[Fact]
	public void ExpandToggleClicked_LeavesAOneWayIsExpandedBindingInControl()
	{
		host.Invoke(() =>
		{
			var source = new ExpandedSource();
			var item = new FloorSidebarItem { Floor = Floor() };

			item.SetBinding(
				FloorSidebarItem.IsExpandedProperty,
				new Binding(nameof(ExpandedSource.Expanded), BindingMode.OneWay, source: source));

			InvokeHandler(item, "OnExpandToggleClicked", EventArgs.Empty);

			source.Expanded = true;

			Assert.True(item.IsExpanded);

			source.Expanded = false;

			Assert.False(item.IsExpanded);
		});
	}

	private static FloorItem Floor() =>
		new(Guid.NewGuid(), 1, "Floor 1", null, null, [], []);

	private static void InvokeHandler(FloorSidebarItem item, string name, object args) =>
		typeof(FloorSidebarItem)
			.GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)!
			.Invoke(item, [null, args]);

	private sealed class ExpandedSource : INotifyPropertyChanged
	{
		private bool _expanded;

		public event PropertyChangedEventHandler? PropertyChanged;

		public bool Expanded
		{
			get => _expanded;
			set
			{
				if (_expanded == value)
				{
					return;
				}

				_expanded = value;
				OnPropertyChanged();
			}
		}

		private void OnPropertyChanged([CallerMemberName] string? name = null) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}
}
