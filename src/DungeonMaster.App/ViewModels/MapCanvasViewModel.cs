using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DungeonMaster.App.Core.Editor;
using DungeonMaster.App.Core.Map;
using DungeonMaster.Application.Features.Dungeons.Commands.AddArea;
using DungeonMaster.Application.Features.Dungeons.Commands.AddPlacedObject;
using DungeonMaster.Application.Features.Dungeons.Commands.RemoveArea;
using DungeonMaster.Application.Features.Dungeons.Commands.RemovePlacedObject;
using DungeonMaster.Application.Features.Dungeons.Commands.UpdateArea;
using DungeonMaster.Application.Features.Dungeons.Commands.UpdateAreaOutline;
using DungeonMaster.Application.Features.Dungeons.Commands.UpdatePlacedObject;
using DungeonMaster.Domain.Entities.Floors;
using DungeonMaster.Domain.ValueObjects;
using AppDispatcher = DungeonMaster.Application.Common.Dispatch.IDispatcher;

namespace DungeonMaster.App.ViewModels;

/// <summary>
///		Canvas content and the tool state the dungeon detail page's gestures drive: the active
///		tool, the in-progress drawing session, and the area and placed-object commands.
/// </summary>
/// <param name="dispatcher">
///		The application dispatcher through which the area and placed-object commands are sent.
/// </param>
/// <param name="context">
///		The dungeon-detail composition root, which owns the loaded dungeon identifier, the error
///		channel, and the floor refresh. Every command here changes exactly one floor, so each one
///		that succeeds refreshes that floor in place rather than reloading the whole detail.
/// </param>
public partial class MapCanvasViewModel(
	AppDispatcher dispatcher,
	IDungeonEditingContext context)
		: ObservableObject
{
	/// <summary>
	///		Gets or sets the active map tool.
	/// </summary>
	/// <remarks>
	///		This is the single owner of that fact. <c>MapEditorState</c> deliberately does not
	///		store it, and the page must not shadow it in a field of its own.
	/// </remarks>
	[ObservableProperty]
	public partial MapTool ActiveTool { get; set; } = MapTool.Pan;

	/// <summary>
	///		Gets or sets the identifier of the area currently being edited in draw mode.
	/// </summary>
	[ObservableProperty]
	public partial Guid? EditingAreaId { get; set; }

	/// <summary>
	///		Gets the draw session that tracks in-progress vertices for the current area edit.
	/// </summary>
	public MapDrawingSession DrawingSession { get; } = new();

	/// <summary>
	///		Adds a new area to the specified floor and refreshes that floor.
	/// </summary>
	/// <param name="floorId">
	///		The unique identifier of the floor that will receive the new area.
	/// </param>
	/// <returns>
	///		A task that completes when the area has been added and, on success, the floor refreshed.
	/// </returns>
	[RelayCommand]
	private async Task AddAreaAsync(Guid floorId)
	{
		context.ClearError();

		if (context.NoDungeonIsLoaded())
		{
			return;
		}

		var result = await dispatcher.SendAsync<Guid>(
			new AddAreaCommand(
				context.DungeonId,
				floorId,
				"New Area",
				null),
			CancellationToken.None);

		if (result.IsFailure)
		{
			context.ReportFailure(result);

			return;
		}

		await context.RefreshFloorAsync(floorId);
	}

	/// <summary>
	///		Removes an area from a floor and refreshes that floor.
	/// </summary>
	/// <param name="args">
	///		The floor identifier and the area identifier to remove.
	/// </param>
	/// <returns>
	///		A task that completes when the area has been removed and, on success, the floor refreshed.
	/// </returns>
	[RelayCommand]
	private async Task RemoveAreaAsync(RemoveAreaArgs args)
	{
		context.ClearError();

		if (context.NoDungeonIsLoaded())
		{
			return;
		}

		var result = await dispatcher.SendAsync(
			new RemoveAreaCommand(
				context.DungeonId,
				args.FloorId,
				args.AreaId),
			CancellationToken.None);

		if (result.IsFailure)
		{
			context.ReportFailure(result);

			return;
		}

		await context.RefreshFloorAsync(args.FloorId);
	}

	/// <summary>
	///		Updates the name, description, and notes of an existing area, then refreshes the floor
	///		that holds it so the cached area projections carry the new values.
	/// </summary>
	/// <param name="args">
	///		The floor identifier, area identifier, and the area's new name, description, and notes.
	/// </param>
	/// <returns>
	///		A task that completes when the area has been updated and, on success, the floor refreshed.
	/// </returns>
	[RelayCommand]
	private async Task UpdateAreaAsync(UpdateAreaArgs args)
	{
		context.ClearError();

		if (context.NoDungeonIsLoaded())
		{
			return;
		}

		var result = await dispatcher.SendAsync(
			new UpdateAreaCommand(
				context.DungeonId,
				args.FloorId,
				args.AreaId,
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
	///		Creates a new area on the specified floor and immediately writes the outline the user
	///		has just drawn, then refreshes that floor.
	/// </summary>
	/// <remarks>
	///		Draw-to-spawn is one user operation carried by two commands: the area is created, then
	///		its outline is written. Both dispatches are captured and branched on separately, and
	///		the single floor refresh sits after the second, so the floor is repainted once from the
	///		finished state rather than twice from a half-written one.
	/// </remarks>
	/// <param name="args">
	///		The floor identifier and the outline drawn by the user, in canvas coordinates.
	/// </param>
	/// <returns>
	///		A task that completes when the area and its outline have been written and, on success,
	///		the floor refreshed.
	/// </returns>
	[RelayCommand]
	private async Task AddAreaWithOutlineAsync(AddAreaWithOutlineArgs args)
	{
		context.ClearError();

		if (context.NoDungeonIsLoaded())
		{
			return;
		}

		var addResult = await dispatcher.SendAsync<Guid>(
			new AddAreaCommand(
				context.DungeonId,
				args.FloorId,
				"New Area",
				null),
			CancellationToken.None);

		if (addResult.IsFailure)
		{
			context.ReportFailure(addResult);

			return;
		}

		var outlineResult = await dispatcher.SendAsync(
			new UpdateAreaOutlineCommand(
				context.DungeonId,
				args.FloorId,
				addResult.Value,
				args.Outline),
			CancellationToken.None);

		if (outlineResult.IsFailure)
		{
			context.ReportFailure(outlineResult);

			return;
		}

		await context.RefreshFloorAsync(args.FloorId);
	}

	/// <summary>
	///		Persists a moved area outline and refreshes the floor that holds it. Placed objects
	///		belong to the floor and are deliberately not repositioned when an area outline moves.
	/// </summary>
	/// <param name="args">
	///		The floor identifier, the area identifier, and the translated outline.
	/// </param>
	/// <returns>
	///		A task that completes when the outline has been written and, on success, the floor refreshed.
	/// </returns>
	[RelayCommand]
	private async Task MoveAreaOutlineAsync(MoveAreaOutlineArgs args)
	{
		context.ClearError();

		if (context.NoDungeonIsLoaded())
		{
			return;
		}

		var result = await dispatcher.SendAsync(
			new UpdateAreaOutlineCommand(
				context.DungeonId,
				args.FloorId,
				args.AreaId,
				args.Outline),
			CancellationToken.None);

		if (result.IsFailure)
		{
			context.ReportFailure(result);

			return;
		}

		await context.RefreshFloorAsync(args.FloorId);
	}

	/// <summary>
	///		Places a new object with the specified kind, position, rotation, and scale on the
	///		specified floor and refreshes that floor.
	/// </summary>
	/// <param name="args">
	///		The floor identifier and the new object's kind, position, rotation, and scale.
	/// </param>
	/// <returns>
	///		A task that completes when the object has been placed and, on success, the floor refreshed.
	/// </returns>
	[RelayCommand]
	private async Task AddPlacedObjectAsync(PlaceObjectArgs args)
	{
		context.ClearError();

		if (context.NoDungeonIsLoaded())
		{
			return;
		}

		var result = await dispatcher.SendAsync<Guid>(
			new AddPlacedObjectCommand(
				context.DungeonId,
				args.FloorId,
				args.Kind,
				args.Position,
				args.Rotation,
				args.Scale),
			CancellationToken.None);

		if (result.IsFailure)
		{
			context.ReportFailure(result);

			return;
		}

		await context.RefreshFloorAsync(args.FloorId);
	}

	/// <summary>
	///		Removes a placed object from the specified floor and refreshes that floor.
	/// </summary>
	/// <param name="args">
	///		The floor identifier and the identifier of the object to remove.
	/// </param>
	/// <returns>
	///		A task that completes when the object has been removed and, on success, the floor refreshed.
	/// </returns>
	[RelayCommand]
	private async Task RemovePlacedObjectAsync(RemovePlacedObjectArgs args)
	{
		context.ClearError();

		if (context.NoDungeonIsLoaded())
		{
			return;
		}

		var result = await dispatcher.SendAsync(
			new RemovePlacedObjectCommand(
				context.DungeonId,
				args.FloorId,
				args.ObjectId),
			CancellationToken.None);

		if (result.IsFailure)
		{
			context.ReportFailure(result);

			return;
		}

		await context.RefreshFloorAsync(args.FloorId);
	}

	/// <summary>
	///		Updates the position, rotation, and scale of a placed object and refreshes the floor
	///		that holds it.
	/// </summary>
	/// <param name="args">
	///		The floor identifier, the object identifier, and the object's new position, rotation,
	///		and scale. A caller that is not changing the scale omits it and the object is written
	///		at <see cref="ObjectScale.Default"/>.
	/// </param>
	/// <returns>
	///		A task that completes when the object has been updated and, on success, the floor refreshed.
	/// </returns>
	[RelayCommand]
	private async Task UpdatePlacedObjectAsync(UpdatePlacedObjectArgs args)
	{
		context.ClearError();

		if (context.NoDungeonIsLoaded())
		{
			return;
		}

		var result = await dispatcher.SendAsync(
			new UpdatePlacedObjectCommand(
				context.DungeonId,
				args.FloorId,
				args.ObjectId,
				args.Position,
				args.Rotation,
				args.Scale),
			CancellationToken.None);

		if (result.IsFailure)
		{
			context.ReportFailure(result);

			return;
		}

		await context.RefreshFloorAsync(args.FloorId);
	}
}
