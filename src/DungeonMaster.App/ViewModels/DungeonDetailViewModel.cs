using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DungeonMaster.Application.Features.Dungeons.Commands.AddArea;
using DungeonMaster.Application.Features.Dungeons.Commands.AddFloor;
using DungeonMaster.Application.Features.Dungeons.Commands.RemoveArea;
using DungeonMaster.Application.Features.Dungeons.Commands.RemoveFloor;
using DungeonMaster.Application.Features.Dungeons.Commands.ReorderFloor;
using DungeonMaster.Application.Features.Dungeons.Commands.UpdateArea;
using DungeonMaster.Application.Features.Dungeons.Commands.UpdateDungeon;
using DungeonMaster.Application.Features.Dungeons.Commands.UpdateFloor;
using DungeonMaster.Application.Features.Dungeons.Queries.GetDungeonDetail;
using AppDispatcher = DungeonMaster.Application.Common.Dispatch.IDispatcher;

namespace DungeonMaster.App.ViewModels;

/// <summary>
///		Provides data and commands for the dungeon detail page.
/// </summary>
public partial class DungeonDetailViewModel(AppDispatcher dispatcher) : ObservableObject, IQueryAttributable
{
	/// <summary>
	///		Gets or sets the full detail DTO for the currently loaded dungeon.
	/// </summary>
	[ObservableProperty]
	public partial DungeonDetailDto? Dungeon { get; set; }

	/// <summary>
	///		Gets or sets the editable name of the dungeon.
	/// </summary>
	[ObservableProperty]
	public partial string DungeonName { get; set; } = string.Empty;

	/// <summary>
	///		Gets or sets the editable description of the dungeon, or <see langword="null"/> when not set.
	/// </summary>
	[ObservableProperty]
	public partial string? DungeonDescription { get; set; }

	/// <summary>
	///		Gets or sets the editable notes for the dungeon, or <see langword="null"/> when not set.
	/// </summary>
	[ObservableProperty]
	public partial string? DungeonNotes { get; set; }

	/// <summary>
	///		Gets the observable collection of floors belonging to the current dungeon.
	/// </summary>
	public ObservableCollection<FloorDto> Floors { get; } = [];

	private Guid _dungeonId;

	/// <summary>
	///		Applies query-string attributes from Shell navigation to load the correct dungeon.
	/// </summary>
	/// <param name="query">
	///		The query attributes provided by Shell navigation. Must contain an <c>id</c> key with a valid <see cref="Guid"/> string.
	/// </param>
	public void ApplyQueryAttributes(IDictionary<string, object> query)
	{
		if (query.TryGetValue("id", out var val) && Guid.TryParse(val?.ToString(), out var id))
		{
			LoadDungeonCommand.ExecuteAsync(id);
		}
	}

	/// <summary>
	///		Loads the dungeon detail for the specified <paramref name="id"/> and populates all editable fields.
	/// </summary>
	/// <param name="id">
	///		The unique identifier of the dungeon to load.
	/// </param>
	[RelayCommand]
	private async Task LoadDungeonAsync(Guid id)
	{
		_dungeonId = id;
		var dto = await dispatcher.QueryAsync<DungeonDetailDto?>(
			new GetDungeonDetailQuery(id),
			CancellationToken.None);

		// Set editable fields first so the PropertyChanged handler for Dungeon reads current values
		DungeonName = dto?.Name ?? string.Empty;
		DungeonDescription = dto?.Description;
		DungeonNotes = dto?.Notes;
		Dungeon = dto;

		Floors.Clear();

		if (Dungeon is not null)
		{
			foreach (var floor in Dungeon.Floors)
			{
				Floors.Add(floor);
			}
		}
	}

	/// <summary>
	///		Persists the current values of <see cref="DungeonName"/>, <see cref="DungeonDescription"/>, and <see cref="DungeonNotes"/>.
	/// </summary>
	[RelayCommand]
	private async Task SaveDungeonAsync()
	{
		await dispatcher.SendAsync<bool>(
			new UpdateDungeonCommand(
				_dungeonId,
				DungeonName,
				DungeonDescription,
				DungeonNotes),
			CancellationToken.None);
	}

	/// <summary>
	///		Adds a new floor to the current dungeon and reloads the detail.
	/// </summary>
	[RelayCommand]
	private async Task AddFloorAsync()
	{
		await dispatcher.SendAsync<Guid>(
			new AddFloorCommand(_dungeonId, null),
			CancellationToken.None);

		await LoadDungeonAsync(_dungeonId);
	}

	/// <summary>
	///		Removes the floor with the specified <paramref name="floorId"/> and reloads the detail.
	/// </summary>
	/// <param name="floorId">
	///		The unique identifier of the floor to remove.
	/// </param>
	[RelayCommand]
	private async Task RemoveFloorAsync(Guid floorId)
	{
		await dispatcher.SendAsync<bool>(
			new RemoveFloorCommand(_dungeonId, floorId),
			CancellationToken.None);

		await LoadDungeonAsync(_dungeonId);
	}

	/// <summary>
	///		Updates the name of an existing floor without reloading the full detail.
	/// </summary>
	/// <param name="args">
	///		A tuple containing the floor identifier and its new name.
	/// </param>
	[RelayCommand]
	private async Task UpdateFloorAsync((Guid FloorId, string? Name) args)
	{
		await dispatcher.SendAsync<bool>(
			new UpdateFloorCommand(
				_dungeonId,
				args.FloorId,
				args.Name,
				null),
			CancellationToken.None);
	}

	/// <summary>
	///		Changes the display order of a floor and reloads the detail.
	/// </summary>
	/// <param name="args">
	///		A tuple containing the floor identifier and its new display number.
	/// </param>
	[RelayCommand]
	private async Task ReorderFloorAsync((Guid FloorId, int NewNumber) args)
	{
		await dispatcher.SendAsync<bool>(
			new ReorderFloorCommand(
				_dungeonId,
				args.FloorId,
				args.NewNumber),
			CancellationToken.None);

		await LoadDungeonAsync(_dungeonId);
	}

	/// <summary>
	///		Adds a new area to the specified floor and reloads the detail.
	/// </summary>
	/// <param name="floorId">
	///		The unique identifier of the floor that will receive the new area.
	/// </param>
	[RelayCommand]
	private async Task AddAreaAsync(Guid floorId)
	{
		await dispatcher.SendAsync<Guid>(
			new AddAreaCommand(
				_dungeonId,
				floorId,
				"New Area",
				null),
			CancellationToken.None);

		await LoadDungeonAsync(_dungeonId);
	}

	/// <summary>
	///		Removes an area from a floor and reloads the detail.
	/// </summary>
	/// <param name="args">
	///		A tuple containing the floor identifier and the area identifier to remove.
	/// </param>
	[RelayCommand]
	private async Task RemoveAreaAsync((Guid FloorId, Guid AreaId) args)
	{
		await dispatcher.SendAsync<bool>(
			new RemoveAreaCommand(
				_dungeonId,
				args.FloorId,
				args.AreaId),
			CancellationToken.None);

		await LoadDungeonAsync(_dungeonId);
	}

	/// <summary>
	///		Updates the name of an existing area, preserving its current description and notes.
	/// </summary>
	/// <param name="args">
	///		A tuple containing the floor identifier, area identifier, and new area name.
	/// </param>
	[RelayCommand]
	private async Task UpdateAreaNameAsync((Guid FloorId, Guid AreaId, string Name) args)
	{
		var existing = Dungeon?.Floors
			.FirstOrDefault(floor =>
				floor.Id == args.FloorId)?.Areas
			.FirstOrDefault(area =>
				area.Id == args.AreaId);

		await dispatcher.SendAsync<bool>(
			new UpdateAreaCommand(
				_dungeonId,
				args.FloorId,
				args.AreaId,
				args.Name,
				existing?.Description,
				existing?.Notes),
			CancellationToken.None);
	}
}
