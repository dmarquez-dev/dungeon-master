using CommunityToolkit.Maui.Views;
using MauiApplication = Microsoft.Maui.Controls.Application;

namespace DungeonMaster.App.Controls;

public partial class DmConfirmPopup : Popup<bool>
{
    public DmConfirmPopup(string title, string message,
        string confirmText = "Confirm", string cancelText = "Cancel",
        bool isDestructive = false)
    {
        InitializeComponent();
        Chrome.Title = title;
        MessageLabel.Text = message;
        CancelButton.Text = cancelText;
        ConfirmButton.Text = confirmText;

        if (isDestructive)
        {
            ConfirmButton.BackgroundColor = (Color)MauiApplication.Current!.Resources["DMDestructive"];
            ConfirmButton.TextColor = (Color)MauiApplication.Current!.Resources["DMDestructiveText"];
        }
        else
        {
            ConfirmButton.BackgroundColor = (Color)MauiApplication.Current!.Resources["DMConfirm"];
            ConfirmButton.TextColor = (Color)MauiApplication.Current!.Resources["DMConfirmText"];
        }
    }

    private void OnConfirm(object? sender, EventArgs e) => _ = CloseAsync(true);
    private void OnCancel(object? sender, EventArgs e) => _ = CloseAsync(false);
}
