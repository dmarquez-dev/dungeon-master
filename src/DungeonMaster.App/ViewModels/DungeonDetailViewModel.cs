using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DungeonMaster.Application.Common.Map;
using DungeonMaster.Application.Features.Dungeons.Commands.AddArea;
using DungeonMaster.Application.Features.Dungeons.Commands.AddFloor;
using DungeonMaster.Application.Features.Dungeons.Commands.AddPlacedObject;
using DungeonMaster.Application.Features.Dungeons.Commands.RemoveArea;
using DungeonMaster.Application.Features.Dungeons.Commands.RemovePlacedObject;
using DungeonMaster.Application.Features.Dungeons.Commands.UpdatePlacedObject;
using DungeonMaster.Application.Features.Dungeons.Commands.RemoveFloor;
using DungeonMaster.Application.Features.Dungeons.Commands.ReorderFloor;
using DungeonMaster.Application.Features.Dungeons.Commands.UpdateArea;
using DungeonMaster.Application.Features.Dungeons.Commands.UpdateAreaOutline;
using DungeonMaster.Application.Features.Dungeons.Commands.UpdateDungeon;
using DungeonMaster.Application.Features.Dungeons.Commands.UpdateFloor;
using DungeonMaster.Application.Features.Dungeons.Queries.GetDungeonDetail;
using DungeonMaster.Domain.Entities.Floor;
using DungeonMaster.Domain.ValueObjects;
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
	///		Gets or sets the identifier of the area currently being edited in draw mode.
	/// </summary>
	[ObservableProperty]
	public partial Guid? EditingAreaId { get; set; }

	/// <summary>
	///		Gets or sets the active drawing tool on the map canvas.
	/// </summary>
	[ObservableProperty]
	public partial MapTool ActiveTool { get; set; } = MapTool.Pan;

	/// <summary>
	///		Gets the draw session that tracks in-progress vertices for the current area edit.
	/// </summary>
	public MapDrawingSession DrawingSession { get; } = new();

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
			_ = LoadDungeonCommand.ExecuteAsync(id);
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
	///		Updates the name, description, and notes of an existing floor, then reloads the
	///		detail so the cached floor projections carry the new values.
	/// </summary>
	/// <param name="args">
	///		A tuple containing the floor identifier and its new name, description, and notes.
	/// </param>
	[RelayCommand]
	private async Task UpdateFloorAsync((Guid FloorId, string? Name, string? Description, string? Notes) args)
	{
		await dispatcher.SendAsync<bool>(
			new UpdateFloorCommand(
				_dungeonId,
				args.FloorId,
				args.Name,
				args.Description,
				args.Notes),
			CancellationToken.None);

		await LoadDungeonAsync(_dungeonId);
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
	///		Updates the name, description, and notes of an existing area, then reloads the detail
	///		so the cached area projections carry the new values.
	/// </summary>
	/// <param name="args">
	///		A tuple containing the floor identifier, area identifier, and the area's new name,
	///		description, and notes. All three text fields are written as supplied, so a caller
	///		editing only one of them must pass the current values of the other two.
	/// </param>
	[RelayCommand]
	private async Task UpdateAreaAsync((Guid FloorId, Guid AreaId, string Name, string? Description, string? Notes) args)
	{
		await dispatcher.SendAsync<bool>(
			new UpdateAreaCommand(
				_dungeonId,
				args.FloorId,
				args.AreaId,
				args.Name,
				args.Description,
				args.Notes),
			CancellationToken.None);

		await LoadDungeonAsync(_dungeonId);
	}

	/// <summary>
	///     Persists a moved area outline and reloads the detail. Placed objects belong to the floor
	///     and are deliberately not repositioned when an area outline moves.
	/// </summary>
	public async Task MoveAreaOutlineAsync(
		Guid floorId,
		Guid areaId,
		IReadOnlyList<PathSegment> newOutline)
	{
		await dispatcher.SendAsync<bool>(
			new UpdateAreaOutlineCommand(
				_dungeonId,
				floorId,
				areaId,
				newOutline),
			CancellationToken.None);

		await LoadDungeonAsync(_dungeonId);
	}

	/// <summary>
	///     Creates a new area on the specified floor and immediately saves its outline in one operation.
	///     Used for draw-to-spawn: the area is added to the DB then its outline is written atomically.
	/// </summary>
	[RelayCommand]
	private async Task AddAreaWithOutlineAsync((Guid FloorId, IReadOnlyList<PathSegment> Outline) args)
	{
		var result = await dispatcher.SendAsync<Guid>(
			new AddAreaCommand(_dungeonId, args.FloorId, "New Area", null),
			CancellationToken.None);

		if (!result.IsSuccess || result.Value == Guid.Empty)
			return;

		await dispatcher.SendAsync<bool>(
			new UpdateAreaOutlineCommand(
				_dungeonId,
				args.FloorId,
				result.Value,
				args.Outline),
			CancellationToken.None);
	}

	/// <summary>
	///		Arguments for placing a new object on a floor.
	/// </summary>
	public record PlaceObjectArgs(Guid FloorId, ObjectKind Kind, CanvasPoint Position, ObjectRotation Rotation, float Scale);

	/// <summary>
	///		Places a new object with the specified kind, position, rotation, and scale on the specified floor and reloads the dungeon detail.
	/// </summary>
	[RelayCommand]
	private async Task AddPlacedObjectAsync(PlaceObjectArgs args)
	{
		await dispatcher.SendAsync<Guid>(
			new AddPlacedObjectCommand(
				_dungeonId,
				args.FloorId,
				args.Kind,
				args.Position,
				args.Rotation,
				args.Scale),
			CancellationToken.None);

		await LoadDungeonAsync(_dungeonId);
	}

	/// <summary>
	///     Removes a placed object from the specified floor and reloads the dungeon detail.
	/// </summary>
	public async Task RemovePlacedObjectAsync(
		Guid dungeonId,
		Guid floorId,
		Guid objectId)
	{
		await dispatcher.SendAsync<bool>(
			new RemovePlacedObjectCommand(
				dungeonId,
				floorId,
				objectId),
			CancellationToken.None);

		await LoadDungeonAsync(dungeonId);
	}

	/// <summary>
	///		Updates the position and rotation of a placed object at its current scale and reloads the dungeon detail.
	/// </summary>
	public Task UpdatePlacedObjectAsync(
		Guid dungeonId,
		Guid floorId,
		Guid objectId,
		CanvasPoint position,
		ObjectRotation rotation)
		=> UpdatePlacedObjectAsync(
			dungeonId,
			floorId,
			objectId,
			position,
			rotation,
			ObjectScale.Default);

	/// <summary>
	///		Updates the position, rotation, and scale of a placed object and reloads the dungeon detail.
	/// </summary>
	public async Task UpdatePlacedObjectAsync(
		Guid dungeonId,
		Guid floorId,
		Guid objectId,
		CanvasPoint position,
		ObjectRotation rotation,
		float scale)
	{
		await dispatcher.SendAsync<bool>(
			new UpdatePlacedObjectCommand(
				dungeonId,
				floorId,
				objectId,
				position,
				rotation,
				scale),
			CancellationToken.None);

		await LoadDungeonAsync(dungeonId);
	}

}
