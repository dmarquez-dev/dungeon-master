using DungeonMaster.App.ViewModels;

namespace DungeonMaster.App.Views;

/// <summary>
///		Displays the list of dungeon campaigns available to the user.
/// </summary>
public partial class DungeonListPage : ContentPage
{
	private readonly DungeonListViewModel _viewModel;

	/// <summary>
	///		Initializes a new instance of <see cref="DungeonListPage"/> with the provided view model.
	/// </summary>
	/// <param name="viewModel">
	///		The view model that provides data and commands for this page.
	/// </param>
	public DungeonListPage(DungeonListViewModel viewModel)
	{
		_viewModel = viewModel;
		BindingContext = viewModel;

		InitializeComponent();
	}

	/// <inheritdoc />
	protected override void OnAppearing()
	{
		base.OnAppearing();
		_ = _viewModel.LoadDungeonsAsync();
	}
}
