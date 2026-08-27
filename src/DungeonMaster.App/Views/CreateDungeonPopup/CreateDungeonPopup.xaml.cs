using System.ComponentModel;
using CommunityToolkit.Maui.Views;
using DungeonMaster.App.ViewModels;

namespace DungeonMaster.App.Views;

/// <summary>
///		A themed popup that hosts the create-dungeon form, closing with <see langword="true"/> on success
///		or <see langword="false"/> on cancellation.
/// </summary>
/// <remarks>
///		The outcome arrives as ONE observable value —
///		<see cref="CreateDungeonViewModel.Outcome"/> — rather than as a pair of events the popup
///		converts into a result. There is therefore one subscription, one close site and one place
///		a close failure can be reported; the subscription in the constructor and the
///		unsubscription in <see cref="OnViewModelPropertyChanged"/> are the only two halves and
///		they sit in this type together.
/// </remarks>
public partial class CreateDungeonPopup : Popup<bool>
{
	private readonly CreateDungeonViewModel _viewModel;

	/// <summary>
	///		Initializes a new instance of <see cref="CreateDungeonPopup"/>, binds to the provided view model,
	///		and watches its outcome so the popup closes once the form states one.
	/// </summary>
	/// <param name="viewModel">
	///		The view model that drives the create-dungeon form.
	/// </param>
	public CreateDungeonPopup(CreateDungeonViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = viewModel;

		// Paired with the unsubscription in OnViewModelPropertyChanged.
		_viewModel.PropertyChanged += OnViewModelPropertyChanged;
	}

	private async void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName != nameof(CreateDungeonViewModel.Outcome)
			|| _viewModel.Outcome is not bool outcome)
		{
			return;
		}

		try
		{
			await CloseAsync(outcome);

			// Paired with the subscription in the constructor. It is released only after the
			// close succeeded: a failed close leaves the form on screen, and a form the user can
			// still act on must still be listened to.
			_viewModel.PropertyChanged -= OnViewModelPropertyChanged;
		}
		catch (Exception exception)
		{
			// A failed close leaves the form on screen, so the failure is surfaced on the
			// form's own error banner rather than swallowed.
			_viewModel.ErrorMessage = outcome
				? $"The dungeon was created, but the form could not be closed: {exception.Message}"
				: $"The form could not be closed: {exception.Message}";
		}
	}
}
