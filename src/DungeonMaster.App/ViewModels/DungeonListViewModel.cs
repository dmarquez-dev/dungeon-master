using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AppDispatcher = DungeonMaster.Application.Common.Dispatch.IDispatcher;
using DungeonMaster.Application.Features.Dungeons.Commands.CreateDungeon;
using DungeonMaster.Application.Features.Dungeons.Commands.DeleteDungeon;
using DungeonMaster.Application.Features.Dungeons.Queries.GetDungeons;

namespace DungeonMaster.App.ViewModels;

/// <summary>
///		Provides data and commands for the dungeon list view.
///		Loads, creates, and deletes dungeons via the application dispatcher.
/// </summary>
public partial class DungeonListViewModel(
	AppDispatcher dispatcher)
		: ObservableObject
{
	/// <summary>
	///		Gets the collection of dungeon summaries displayed in the list.
	/// </summary>
	public ObservableCollection<DungeonSummaryDto> Dungeons { get; } = [];

	/// <summary>
	///		Gets or sets the name entered by the user for a new dungeon.
	/// </summary>
	[ObservableProperty]
	public partial string NewDungeonName { get; set; } = string.Empty;

	/// <summary>
	///		Gets or sets the current error message, or <see langword="null"/> when no error is present.
	/// </summary>
	[ObservableProperty]
	public partial string? ErrorMessage { get; set; }

	/// <summary>
	///		Loads the list of dungeons from the application layer and repopulates <see cref="Dungeons"/>.
	/// </summary>
	[RelayCommand]
	public async Task LoadDungeonsAsync()
	{
		var dungeons = await dispatcher.QueryAsync<IReadOnlyList<DungeonSummaryDto>>(
			new GetDungeonsQuery(),
			CancellationToken.None);

		Dungeons.Clear();

		foreach (var dungeon in dungeons)
		{
			Dungeons.Add(dungeon);
		}

		ErrorMessage = null;
	}

	/// <summary>
	///		Creates a new dungeon using <see cref="NewDungeonName"/> and reloads the list on success.
	/// </summary>
	[RelayCommand]
	private async Task CreateDungeonAsync()
	{
		var result = await dispatcher.SendAsync<Guid>(
			new CreateDungeonCommand(
				NewDungeonName,
				null),
			CancellationToken.None);

		if (result.IsSuccess)
		{
			NewDungeonName = string.Empty;
			await LoadDungeonsAsync();
		}
		else
		{
			ErrorMessage = result.Error;
		}
	}

	/// <summary>
	///		Deletes the dungeon with the specified <paramref name="id"/> and reloads the list on success.
	/// </summary>
	/// <param name="id">
	///		The unique identifier of the dungeon to delete.
	/// </param>
	[RelayCommand]
	private async Task DeleteDungeonAsync(Guid id)
	{
		var result = await dispatcher.SendAsync<bool>(
			new DeleteDungeonCommand(id),
			CancellationToken.None);

		if (result.IsSuccess)
		{
			await LoadDungeonsAsync();
		}
		else
		{
			ErrorMessage = result.Error;
		}
	}
}
