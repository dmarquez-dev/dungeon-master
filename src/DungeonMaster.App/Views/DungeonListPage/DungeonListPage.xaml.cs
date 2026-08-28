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
	private readonly Func<CreateDungeonPopup> _createPopup;
	private bool _suppressCardNavigation;

	/// <summary>
	///		Initializes a new instance of <see cref="DungeonListPage"/> and binds it to the provided view model.
	/// </summary>
	/// <param name="viewModel">
	///		The view model that drives the dungeon list.
	/// </param>
	/// <param name="createPopup">
	///		Produces a create-dungeon popup. It is a factory rather than a single instance because
	///		the popup carries per-session form state and must be new on every click; the page
	///		declares that one dependency instead of holding the container.
	/// </param>
	public DungeonListPage(
		DungeonListViewModel viewModel,
		Func<CreateDungeonPopup> createPopup)
	{
		InitializeComponent();
		_viewModel = viewModel;
		_createPopup = createPopup;
		BindingContext = viewModel;
	}

	/// <inheritdoc/>
	/// <remarks>
	///		<c>async void</c> is unavoidable on a lifecycle override — the base signature returns
	///		<see langword="void"/> — which makes an escaping exception an UNOBSERVED one, and an
	///		unobserved exception from an <c>async void</c> is rethrown on the synchronisation
	///		context and takes the process down. The <c>try</c> is therefore not defensive style;
	///		it is the only thing standing between a failed load and a crash on navigation. The
	///		failure is routed to the same <c>ErrorMessage</c> the view model uses for every other
	///		load failure, so the user sees the banner rather than losing the app.
	///		<para>
	///		<c>base.OnAppearing()</c> is called first, unconditionally. The previous expression
	///		body skipped it entirely; <see cref="Page"/> raises its <c>Appearing</c> event from
	///		that method, so anything subscribed to it — a behavior, a platform handler — was
	///		silently never notified on this page.
	///		</para>
	/// </remarks>
	protected override async void OnAppearing()
	{
		base.OnAppearing();

		try
		{
			await _viewModel.LoadDungeonsCommand.ExecuteAsync(null);
		}
		catch (Exception exception)
		{
			_viewModel.ErrorMessage = $"Could not load your dungeons: {exception.Message}";
		}
	}

	private async void OnCreateClicked(object? sender, EventArgs e)
	{
		try
		{
			var popup = _createPopup();
			var result = await Navigation.ShowPopupAsync<bool>(
				popup,
				new PopupOptions { Shape = null, Shadow = null });

			if (result.Result is true)
			{
				await _viewModel.LoadDungeonsCommand.ExecuteAsync(null);
			}
		}
		catch (Exception exception)
		{
			// async void: an escaping exception is UNOBSERVED and is rethrown on the
			// synchronisation context, which takes the process down. The failure goes to the
			// same ErrorMessage banner OnAppearing uses, per that method's remarks.
			_viewModel.ErrorMessage = $"Could not create a dungeon: {exception.Message}";
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

		try
		{
			if (sender is DmCard card && card.BindingContext is DungeonSummaryDto dto)
			{
				await _viewModel.NavigateToDetailCommand.ExecuteAsync(dto.Id);
			}
		}
		catch (Exception exception)
		{
			// async void: an escaping exception is UNOBSERVED and is rethrown on the
			// synchronisation context, which takes the process down. The failure goes to the
			// same ErrorMessage banner OnAppearing uses, per that method's remarks.
			_viewModel.ErrorMessage = $"Could not open that dungeon: {exception.Message}";
		}
	}

	private async void OnEditClicked(object? sender, EventArgs e)
	{
		_suppressCardNavigation = true;

		try
		{
			if (sender is Button btn && btn.CommandParameter is Guid id)
			{
				await _viewModel.NavigateToDetailCommand.ExecuteAsync(id);
			}
		}
		catch (Exception exception)
		{
			// async void: an escaping exception is UNOBSERVED and is rethrown on the
			// synchronisation context, which takes the process down. The failure goes to the
			// same ErrorMessage banner OnAppearing uses, per that method's remarks.
			_viewModel.ErrorMessage = $"Could not open that dungeon: {exception.Message}";
		}
	}

	private async void OnDeleteClicked(object? sender, EventArgs e)
	{
		_suppressCardNavigation = true;

		if (sender is not Button btn || btn.CommandParameter is not Guid id)
		{
			return;
		}

		try
		{
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
		catch (Exception exception)
		{
			// async void: an escaping exception is UNOBSERVED and is rethrown on the
			// synchronisation context, which takes the process down. The failure goes to the
			// same ErrorMessage banner OnAppearing uses, per that method's remarks.
			_viewModel.ErrorMessage = $"Could not delete that dungeon: {exception.Message}";
		}
	}
}
