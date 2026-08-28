using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DungeonMaster.App.Services.Navigation;
using DungeonMaster.Application.Common.Results;
using AppDispatcher = DungeonMaster.Application.Common.Dispatch.IDispatcher;
using DungeonMaster.Application.Features.Dungeons.Commands.DeleteDungeon;
using DungeonMaster.Application.Features.Dungeons.Queries.GetDungeons;

namespace DungeonMaster.App.ViewModels;

/// <summary>
///		Provides data and commands for the dungeon list page.
/// </summary>
public partial class DungeonListViewModel(
	AppDispatcher dispatcher,
	INavigationService navigation) : ObservableObject
{
	private readonly ObservableCollection<DungeonSummaryDto> _allDungeons = [];

	/// <summary>
	///		Gets the filtered collection of dungeons currently displayed in the list.
	/// </summary>
	public ObservableCollection<DungeonSummaryDto> FilteredDungeons { get; } = [];

	/// <summary>
	///		Gets or sets the search text used to filter the dungeon list.
	/// </summary>
	[ObservableProperty]
	public partial string SearchText { get; set; } = string.Empty;

	/// <summary>
	///		Gets or sets the current error message, or <see langword="null"/> when no error has occurred.
	/// </summary>
	[ObservableProperty]
	public partial string? ErrorMessage { get; set; }

	partial void OnSearchTextChanged(string value) =>
		ApplyFilter(value);

	private void ApplyFilter(string search)
	{
		FilteredDungeons.Clear();
		var matches = string.IsNullOrWhiteSpace(search)
			? _allDungeons
			: _allDungeons.Where(dungeon =>
				dungeon.Name.Contains(search, StringComparison.OrdinalIgnoreCase));

		foreach (var dungeon in matches)
		{
			FilteredDungeons.Add(dungeon);
		}
	}

	/// <summary>
	///		Loads all dungeons from the data store and applies the current search filter.
	/// </summary>
	[RelayCommand]
	private async Task LoadDungeonsAsync()
	{
		var dungeons = await dispatcher.QueryAsync<IReadOnlyList<DungeonSummaryDto>>(
			new GetDungeonsQuery(),
			CancellationToken.None);

		_allDungeons.Clear();

		foreach (var dungeon in dungeons)
		{
			_allDungeons.Add(dungeon);
		}

		ApplyFilter(SearchText);
		ErrorMessage = null;
	}

	/// <summary>
	///		Permanently removes the dungeon with the specified <paramref name="id"/> and reloads the list.
	/// </summary>
	/// <param name="id">
	///		The unique identifier of the dungeon to delete.
	/// </param>
	[RelayCommand]
	private async Task DeleteDungeonAsync(Guid id)
	{
		var result = await dispatcher.SendAsync(
			new DeleteDungeonCommand(id),
			CancellationToken.None);

		if (result.IsSuccess)
		{
			await LoadDungeonsAsync();
		}
		else
		{
			ErrorMessage = ResultMessage.ForFailure(result);
		}
	}

	/// <summary>
	///		Navigates to the dungeon detail page for the specified <paramref name="id"/>.
	/// </summary>
	/// <param name="id">
	///		The unique identifier of the dungeon to open.
	/// </param>
	[RelayCommand]
	private async Task NavigateToDetailAsync(Guid id) =>
		await navigation.GoToAsync($"dungeondetail?id={id}");
}
