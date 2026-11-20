using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DungeonMaster.App.Core.Presentation;
using DungeonMaster.Application.Features.Dungeons.Commands.AddFloor;
using DungeonMaster.Application.Features.Dungeons.Commands.RemoveFloor;
using DungeonMaster.Application.Features.Dungeons.Commands.ReorderFloor;
using DungeonMaster.Application.Features.Dungeons.Commands.UpdateFloor;
using DungeonMaster.Application.Features.Dungeons.Queries.GetDungeonDetail;
using AppDispatcher = DungeonMaster.Application.Common.Dispatch.IDispatcher;

namespace DungeonMaster.App.ViewModels;

/// <summary>
///		Floor lifecycle and the floor collection the dungeon detail sidebar renders.
/// </summary>
/// <param name="dispatcher">
///		The application dispatcher through which the floor commands are sent.
/// </param>
/// <param name="context">
///		The dungeon-detail composition root, which owns the loaded dungeon identifier, the error
///		channel, the floor refresh, and the whole-detail reload.
/// </param>
public partial class FloorListViewModel(
	AppDispatcher dispatcher,
	IDungeonEditingContext context)
		: ObservableObject
{
	/// <summary>
	///		Gets the floors of the loaded dungeon, in the control kit's own presentation shape.
	/// </summary>
	public ObservableCollection<FloorItem> Items { get; } = [];

	/// <summary>
	///		Clears and refills <see cref="Items"/> from a loaded detail projection. Called by the
	///		composition root during load.
	/// </summary>
	/// <param name="dto">
	///		The loaded dungeon detail, or <see langword="null"/> when no dungeon is loaded, in
	///		which case the collection is left empty.
	/// </param>
	public void Populate(DungeonDetailDto? dto)
	{
		Items.Clear();

		if (dto is null)
		{
			return;
		}

		// The one translation point from the query projection to the control kit's own shapes.
		// Every consumer of Items reads FloorItem, so no control names a DTO.
		//
		// RECORDED BEHAVIOUR CHANGE, carried from sub-session 5a and deliberately NOT reverted:
		// DungeonDetailPresentationMapper.ToItems throws ArgumentNullException on a null floor
		// collection and ArgumentException on a null element. The `foreach (var floor in
		// Dungeon.Floors)` loop it replaced would instead have thrown NullReferenceException or
		// silently added a null to the collection. Failing loudly at the translation point is
		// the improvement 5a mandated; a null floor is a projection defect, not a state the page
		// should render around.
		foreach (var floor in DungeonDetailPresentationMapper.ToItems(dto.Floors))
		{
			Items.Add(floor);
		}
	}

	/// <summary>
	///		Finds the position in <see cref="Items"/> of the floor with the specified identifier.
	/// </summary>
	/// <param name="floorId">
	///		The unique identifier of the floor to find.
	/// </param>
	/// <returns>
	///		The zero-based index of the floor, or -1 when no element of <see cref="Items"/> has that
	///		identifier.
	/// </returns>
	internal int IndexOf(Guid floorId)
	{
		for (var index = 0; index < Items.Count; index++)
		{
			if (Items[index].Id == floorId)
			{
				return index;
			}
		}

		return -1;
	}

	/// <summary>
	///		Replaces the element of <see cref="Items"/> at <paramref name="index"/> with a new item
	///		mapped from <paramref name="floor"/>. Called by the composition root when it refreshes
	///		one floor in place.
	/// </summary>
	/// <remarks>
	///		The indexer assignment raises exactly one <c>CollectionChanged</c>, with
	///		<c>Action == Replace</c> at <paramref name="index"/>, and leaves every other element
	///		reference-identical, which is what lets a templated floor list rebuild one row instead
	///		of every row. The floor goes through the same translation point as
	///		<see cref="Populate"/>, so a refreshed floor and a loaded floor map identically.
	/// </remarks>
	/// <param name="index">
	///		The zero-based position of the floor being replaced.
	/// </param>
	/// <param name="floor">
	///		The re-read projection of that floor.
	/// </param>
	internal void ReplaceAt(
		int index,
		FloorDto floor)
	{
		Items[index] = DungeonDetailPresentationMapper.ToItems([floor])[0];
	}

	/// <summary>
	///		Adds a new floor to the loaded dungeon and reloads the detail.
	/// </summary>
	/// <returns>
	///		A task that completes when the floor has been added and, on success, the detail reloaded.
	/// </returns>
	/// <remarks>
	///		WHY THIS RELOADS THE WHOLE DETAIL rather than refreshing one floor: adding a floor
	///		changes the floor list's membership. There is no existing floor to replace in place,
	///		and the one-floor read cannot describe a list that has grown.
	/// </remarks>
	[RelayCommand]
	private async Task AddFloorAsync()
	{
		context.ClearError();

		if (context.NoDungeonIsLoaded())
		{
			return;
		}

		var result = await dispatcher.SendAsync<Guid>(
			new AddFloorCommand(
				context.DungeonId,
				null),
			CancellationToken.None);

		if (result.IsFailure)
		{
			context.ReportFailure(result);

			return;
		}

		await context.ReloadAsync();
	}

	/// <summary>
	///		Removes the floor with the specified <paramref name="floorId"/> and reloads the detail.
	/// </summary>
	/// <param name="floorId">
	///		The unique identifier of the floor to remove.
	/// </param>
	/// <returns>
	///		A task that completes when the floor has been removed and, on success, the detail reloaded.
	/// </returns>
	/// <remarks>
	///		WHY THIS RELOADS THE WHOLE DETAIL rather than refreshing one floor: removing a floor
	///		changes the floor list's membership, and it renumbers every later floor, so more than
	///		one floor's projection changes and the removed one has nothing to be replaced by.
	/// </remarks>
	[RelayCommand]
	private async Task RemoveFloorAsync(Guid floorId)
	{
		context.ClearError();

		if (context.NoDungeonIsLoaded())
		{
			return;
		}

		var result = await dispatcher.SendAsync(
			new RemoveFloorCommand(
				context.DungeonId,
				floorId),
			CancellationToken.None);

		if (result.IsFailure)
		{
			context.ReportFailure(result);

			return;
		}

		await context.ReloadAsync();
	}

	/// <summary>
	///		Updates the name, description, and notes of an existing floor, then refreshes that
	///		floor in place so its cached projection carries the new values.
	/// </summary>
	/// <param name="args">
	///		The floor identifier and its new name, description, and notes.
	/// </param>
	/// <returns>
	///		A task that completes when the floor has been updated and, on success, the floor refreshed.
	/// </returns>
	[RelayCommand]
	private async Task UpdateFloorAsync(UpdateFloorArgs args)
	{
		context.ClearError();

		if (context.NoDungeonIsLoaded())
		{
			return;
		}

		var result = await dispatcher.SendAsync(
			new UpdateFloorCommand(
				context.DungeonId,
				args.FloorId,
				args.Name,
				args.Description,
				args.Notes),
			CancellationToken.None);

		if (result.IsFailure)
		{
			context.ReportFailure(result);

			return;
		}

		await context.RefreshFloorAsync(args.FloorId);
	}

	/// <summary>
	///		Changes the display order of a floor and reloads the detail.
	/// </summary>
	/// <param name="args">
	///		The floor identifier and its new display number.
	/// </param>
	/// <returns>
	///		A task that completes when the floor has been moved and, on success, the detail reloaded.
	/// </returns>
	/// <remarks>
	///		WHY THIS RELOADS THE WHOLE DETAIL rather than refreshing one floor: moving a floor
	///		renumbers every floor between its old and new positions, so the change spans several
	///		floors and their order, which a one-floor read cannot describe.
	/// </remarks>
	[RelayCommand]
	private async Task ReorderFloorAsync(ReorderFloorArgs args)
	{
		context.ClearError();

		if (context.NoDungeonIsLoaded())
		{
			return;
		}

		var result = await dispatcher.SendAsync(
			new ReorderFloorCommand(
				context.DungeonId,
				args.FloorId,
				args.NewNumber),
			CancellationToken.None);

		if (result.IsFailure)
		{
			context.ReportFailure(result);

			return;
		}

		await context.ReloadAsync();
	}
}
