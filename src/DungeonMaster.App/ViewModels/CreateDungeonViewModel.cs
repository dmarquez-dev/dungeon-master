using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DungeonMaster.Application.Common.Results;
using AppDispatcher = DungeonMaster.Application.Common.Dispatch.IDispatcher;
using DungeonMaster.Application.Features.Dungeons.Commands.CreateDungeon;

namespace DungeonMaster.App.ViewModels;

/// <summary>
///		Provides data and commands for the create-dungeon popup.
/// </summary>
public partial class CreateDungeonViewModel(
	AppDispatcher dispatcher)
		: ObservableObject
{
	/// <summary>
	///		Gets or sets the dungeon name entered by the user. Required.
	/// </summary>
	[ObservableProperty]
	[NotifyCanExecuteChangedFor(nameof(CreateCommand))]
	public partial string Name { get; set; } = string.Empty;

	/// <summary>
	///		Gets or sets the optional dungeon description entered by the user.
	/// </summary>
	[ObservableProperty]
	public partial string Description { get; set; } = string.Empty;

	/// <summary>
	///		Gets or sets the current error message, or <see langword="null"/> when no error.
	/// </summary>
	[ObservableProperty]
	public partial string? ErrorMessage { get; set; }

	/// <summary>
	///		Gets or sets the form's outcome: <see langword="null"/> while the form is still open,
	///		<see langword="true"/> once a dungeon has been created, and <see langword="false"/>
	///		once the user has cancelled.
	/// </summary>
	/// <remarks>
	///		This is the form's ONE outcome channel, replacing the pair of events the popup used to
	///		convert into a result. Two channels meant two subscriptions, two close sites and two
	///		chances to leave one of them wired; one observable value means the popup closes from a
	///		single place with the value the form states.
	/// </remarks>
	[ObservableProperty]
	public partial bool? Outcome { get; set; }

	private bool CanCreate() => !string.IsNullOrWhiteSpace(Name);

	/// <summary>
	///		Sends <see cref="CreateDungeonCommand"/> and states <see cref="Outcome"/> as
	///		<see langword="true"/> on success.
	/// </summary>
	[RelayCommand(CanExecute = nameof(CanCreate))]
	private async Task CreateAsync()
	{
		var description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim();

		var result = await dispatcher.SendAsync<Guid>(
			new CreateDungeonCommand(Name.Trim(), description),
			CancellationToken.None);

		if (result.IsSuccess)
		{
			Outcome = true;
		}
		else
		{
			ErrorMessage = ResultMessage.ForFailure(result);
		}
	}

	/// <summary>
	///		States <see cref="Outcome"/> as <see langword="false"/> to signal the popup should be
	///		closed without saving.
	/// </summary>
	[RelayCommand]
	private void Cancel() =>
		Outcome = false;
}
