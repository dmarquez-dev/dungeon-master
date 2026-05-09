using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AppDispatcher = DungeonMaster.Application.Common.Dispatch.IDispatcher;
using DungeonMaster.Application.Features.Dungeons.Commands.DeleteDungeon;
using DungeonMaster.Application.Features.Dungeons.Queries.GetDungeons;

namespace DungeonMaster.App.ViewModels;

/// <summary>
///     Provides data and commands for the dungeon list view.
///     Loads and deletes dungeons; search-filters the displayed list client-side.
/// </summary>
public partial class DungeonListViewModel(AppDispatcher dispatcher) : ObservableObject
{
    private readonly ObservableCollection<DungeonSummaryDto> _allDungeons = [];

    /// <summary>
    ///     The filtered subset of <see cref="_allDungeons"/> shown in the list.
    ///     Updated whenever <see cref="SearchText"/> changes or the list reloads.
    /// </summary>
    public ObservableCollection<DungeonSummaryDto> FilteredDungeons { get; } = [];

    /// <summary>
    ///     Gets or sets the current search text. Setting this re-filters <see cref="FilteredDungeons"/>.
    /// </summary>
    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the current error message, or <see langword="null"/> when no error is present.
    /// </summary>
    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    partial void OnSearchTextChanged(string value) => ApplyFilter(value);

    private void ApplyFilter(string search)
    {
        FilteredDungeons.Clear();
        var matches = string.IsNullOrWhiteSpace(search)
            ? _allDungeons
            : _allDungeons.Where(d =>
                d.Name.Contains(search, StringComparison.OrdinalIgnoreCase));

        foreach (var dungeon in matches)
            FilteredDungeons.Add(dungeon);
    }

    /// <summary>
    ///     Loads all dungeons and resets the search filter.
    /// </summary>
    [RelayCommand]
    public async Task LoadDungeonsAsync()
    {
        var dungeons = await dispatcher.QueryAsync<IReadOnlyList<DungeonSummaryDto>>(
            new GetDungeonsQuery(), CancellationToken.None);

        _allDungeons.Clear();
        foreach (var dungeon in dungeons)
            _allDungeons.Add(dungeon);

        ApplyFilter(SearchText);
        ErrorMessage = null;
    }

    /// <summary>
    ///     Deletes the dungeon with the specified <paramref name="id"/> and reloads the list.
    /// </summary>
    [RelayCommand]
    private async Task DeleteDungeonAsync(Guid id)
    {
        var result = await dispatcher.SendAsync<bool>(
            new DeleteDungeonCommand(id), CancellationToken.None);

        if (result.IsSuccess)
            await LoadDungeonsAsync();
        else
            ErrorMessage = result.Error;
    }
}
