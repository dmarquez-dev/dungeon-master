using CommunityToolkit.Maui.Views;
using DungeonMaster.App.ViewModels;

namespace DungeonMaster.App.Views;

public partial class CreateDungeonPopup : Popup<bool>
{
    private readonly CreateDungeonViewModel _viewModel;

    public CreateDungeonPopup(CreateDungeonViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;

        viewModel.CreatedSuccessfully += (_, _) => _ = CloseAsync(true);
        viewModel.Cancelled += (_, _) => _ = CloseAsync(false);
    }
}
