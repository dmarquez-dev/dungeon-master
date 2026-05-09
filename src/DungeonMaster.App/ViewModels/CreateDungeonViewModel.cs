using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AppDispatcher = DungeonMaster.Application.Common.Dispatch.IDispatcher;
using DungeonMaster.Application.Features.Dungeons.Commands.CreateDungeon;

namespace DungeonMaster.App.ViewModels;

/// <summary>
///     Provides data and commands for the create-dungeon popup.
/// </summary>
public partial class CreateDungeonViewModel(AppDispatcher dispatcher) : ObservableObject
{
    /// <summary>
    ///     Gets or sets the dungeon name entered by the user. Required.
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateCommand))]
    public partial string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the optional dungeon description entered by the user.
    /// </summary>
    [ObservableProperty]
    public partial string Description { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the current error message, or <see langword="null"/> when no error.
    /// </summary>
    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    /// <summary>
    ///     Raised when the dungeon was created successfully. The page closes the popup and reloads.
    /// </summary>
    public event EventHandler? CreatedSuccessfully;

    /// <summary>
    ///     Raised when the user cancels. The page closes the popup without reloading.
    /// </summary>
    public event EventHandler? Cancelled;

    private bool CanCreate() => !string.IsNullOrWhiteSpace(Name);

    /// <summary>
    ///     Sends <see cref="CreateDungeonCommand"/> and raises <see cref="CreatedSuccessfully"/> on success.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanCreate))]
    private async Task CreateAsync()
    {
        var description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim();

        var result = await dispatcher.SendAsync<Guid>(
            new CreateDungeonCommand(Name.Trim(), description),
            CancellationToken.None);

        if (result.IsSuccess)
            CreatedSuccessfully?.Invoke(this, EventArgs.Empty);
        else
            ErrorMessage = result.Error;
    }

    [RelayCommand]
    private void Cancel() => Cancelled?.Invoke(this, EventArgs.Empty);
}
