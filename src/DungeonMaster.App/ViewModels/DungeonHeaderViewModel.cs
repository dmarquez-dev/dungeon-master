using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DungeonMaster.Application.Features.Dungeons.Commands.UpdateDungeon;
using DungeonMaster.Application.Features.Dungeons.Queries.GetDungeonDetail;
using AppDispatcher = DungeonMaster.Application.Common.Dispatch.IDispatcher;

namespace DungeonMaster.App.ViewModels;

/// <summary>
///		Dungeon metadata editing: name, description, notes, and the save that persists them.
/// </summary>
/// <param name="dispatcher">
///		The application dispatcher through which the update command is sent.
/// </param>
/// <param name="context">
///		The dungeon-detail composition root, which owns the loaded dungeon identifier, the error
///		channel, and the reload.
/// </param>
public partial class DungeonHeaderViewModel(
	AppDispatcher dispatcher,
	IDungeonEditingContext context)
		: ObservableObject
{
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
	///		Sets the three editable fields from a loaded detail projection. Called by the
	///		composition root during load, before it publishes the projection itself, so a handler
	///		reacting to the projection reads fields that already carry the new values.
	/// </summary>
	/// <param name="dto">
	///		The loaded dungeon detail, or <see langword="null"/> when no dungeon is loaded.
	/// </param>
	public void Populate(DungeonDetailDto? dto)
	{
		DungeonName = dto?.Name ?? string.Empty;
		DungeonDescription = dto?.Description;
		DungeonNotes = dto?.Notes;
	}

	/// <summary>
	///		Persists the current values of <see cref="DungeonName"/>, <see cref="DungeonDescription"/>,
	///		and <see cref="DungeonNotes"/>, then reloads the dungeon detail.
	/// </summary>
	/// <remarks>
	///		DELIBERATE BEHAVIOUR CHANGE. Before this session the save did NOT reload: it was one
	///		of three mutations that dispatched and returned, leaving the page showing the values
	///		it had before the write. It now reloads like every other mutation, which is what makes
	///		the reload policy uniform. A save that succeeds therefore repaints the header from the
	///		persisted dungeon rather than from the text still sitting in the editor.
	/// </remarks>
	/// <returns>
	///		A task that completes when the update has been sent and, on success, the detail reloaded.
	/// </returns>
	[RelayCommand]
	private async Task SaveAsync()
	{
		context.ClearError();

		if (context.NoDungeonIsLoaded())
		{
			return;
		}

		var result = await dispatcher.SendAsync(
			new UpdateDungeonCommand(
				context.DungeonId,
				DungeonName,
				DungeonDescription,
				DungeonNotes),
			CancellationToken.None);

		if (result.IsFailure)
		{
			context.ReportFailure(result);

			return;
		}

		await context.ReloadAsync();
	}
}
