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
///		channel, and the reload.
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
	///		Adds a new floor to the loaded dungeon and reloads the detail.
	/// </summary>
	/// <returns>
	///		A task that completes when the floor has been added and, on success, the detail reloaded.
	/// </returns>
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
	///		Updates the name, description, and notes of an existing floor, then reloads the detail
	///		so the cached floor projections carry the new values.
	/// </summary>
	/// <param name="args">
	///		The floor identifier and its new name, description, and notes.
	/// </param>
	/// <returns>
	///		A task that completes when the floor has been updated and, on success, the detail reloaded.
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

		await context.ReloadAsync();
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
