using CommunityToolkit.Maui.Views;
using DungeonMaster.App.ViewModels;

namespace DungeonMaster.App.Views;

/// <summary>
///		A themed popup that hosts the create-dungeon form, closing with <see langword="true"/> on success
///		or <see langword="false"/> on cancellation.
/// </summary>
public partial class CreateDungeonPopup : Popup<bool>
{
	/// <summary>
	///		Initializes a new instance of <see cref="CreateDungeonPopup"/>, binds to the provided view model,
	///		and wires the success and cancellation events to close the popup.
	/// </summary>
	/// <param name="viewModel">
	///		The view model that drives the create-dungeon form.
	/// </param>
	public CreateDungeonPopup(CreateDungeonViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;

		viewModel.CreatedSuccessfully += (_, _) =>
			_ = CloseAsync(true);

		viewModel.Cancelled += (_, _) =>
			_ = CloseAsync(false);
	}
}
