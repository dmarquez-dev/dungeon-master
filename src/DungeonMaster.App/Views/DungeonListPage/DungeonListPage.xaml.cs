using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using DungeonMaster.App.Controls;
using DungeonMaster.App.ViewModels;
using DungeonMaster.App.Views;

namespace DungeonMaster.App.Views;

public partial class DungeonListPage : ContentPage
{
    private readonly DungeonListViewModel _viewModel;
    private readonly IServiceProvider _services;

    public DungeonListPage(DungeonListViewModel viewModel, IServiceProvider services)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _services = services;
        BindingContext = viewModel;
    }

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

    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        if (sender is not Button btn || btn.CommandParameter is not Guid id)
        {
            return;
        }

        var confirm = new DmConfirmPopup(
            title: "Delete Dungeon",
            message: "This will permanently remove the dungeon and all its content. Are you sure?",
            confirmText: "DELETE",
            cancelText: "CANCEL",
            isDestructive: true);

        var result = await Navigation.ShowPopupAsync<bool>(
            confirm,
            new PopupOptions { Shape = null, Shadow = null });

        if (result.Result is true)
        {
            await _viewModel.DeleteDungeonCommand.ExecuteAsync(id);
        }
    }
}
