using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using DungeonMaster.App.Controls;
using DungeonMaster.App.Extensions;
using DungeonMaster.App.ViewModels;
using DungeonMaster.Application.Features.Dungeons.Queries.GetDungeons;

namespace DungeonMaster.App.Views;

/// <summary>
///		Displays the list of all dungeons with search filtering, creation, navigation, and deletion support.
/// </summary>
public partial class DungeonListPage : ContentPage
{
	private readonly DungeonListViewModel _viewModel;
	private readonly IServiceProvider _services;
	private bool _suppressCardNavigation;

	/// <summary>
	///		Initializes a new instance of <see cref="DungeonListPage"/> and binds it to the provided view model.
	/// </summary>
	/// <param name="viewModel">
	///		The view model that drives the dungeon list.
	/// </param>
	/// <param name="services">
	///		The service provider used to resolve the create-dungeon popup.
	/// </param>
	public DungeonListPage(
		DungeonListViewModel viewModel,
		IServiceProvider services)
	{
		InitializeComponent();
		_viewModel = viewModel;
		_services = services;
		BindingContext = viewModel;
	}

	/// <inheritdoc/>
	protected override async void OnAppearing() =>
		await _viewModel.LoadDungeonsCommand.ExecuteAsync(null);

	private async void OnCreateClicked(object? sender, EventArgs e)
	{
		var popup = _services.GetRequiredService<CreateDungeonPopup>();
		var result = await Navigation.ShowPopupAsync<bool>(
			popup,
			new PopupOptions { Shape = null, Shadow = null });

		if (result.Result is true)
		{
			await _viewModel.LoadDungeonsCommand.ExecuteAsync(null);
		}
	}

	private void OnFabPointerEntered(object? sender, PointerEventArgs e)
	{
		if (FabButton.Shadow is Shadow shadow)
		{
			shadow.Opacity = 0.7f;
			shadow.Radius = 18;
		}
	}

	private void OnFabPointerExited(object? sender, PointerEventArgs e)
	{
		if (FabButton.Shadow is Shadow shadow)
		{
			shadow.Opacity = 0f;
			shadow.Radius = 8;
		}
	}

	private async void OnCardTapped(object? sender, TappedEventArgs e)
	{
		if (_suppressCardNavigation)
		{
			_suppressCardNavigation = false;
			return;
		}

		if (sender is DmCard card && card.BindingContext is DungeonSummaryDto dto)
		{
			await _viewModel.NavigateToDetailCommand.ExecuteAsync(dto.Id);
		}
	}

	private async void OnEditClicked(object? sender, EventArgs e)
	{
		_suppressCardNavigation = true;

		if (sender is Button btn && btn.CommandParameter is Guid id)
		{
			await _viewModel.NavigateToDetailCommand.ExecuteAsync(id);
		}
	}

	private async void OnDeleteClicked(object? sender, EventArgs e)
	{
		_suppressCardNavigation = true;

		if (sender is not Button btn || btn.CommandParameter is not Guid id)
		{
			return;
		}

		var confirmed = await Navigation.ShowConfirmPopupAsync(
			"Delete Dungeon",
			"This will permanently remove the dungeon and all its content. Are you sure?",
			"DELETE",
			"CANCEL",
			isDestructive: true);

		if (confirmed)
		{
			await _viewModel.DeleteDungeonCommand.ExecuteAsync(id);
		}
	}
}
