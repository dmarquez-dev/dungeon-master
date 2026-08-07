using CommunityToolkit.Maui.Views;
using DungeonMaster.App.Theme;

namespace DungeonMaster.App.Controls;

/// <summary>
///		A themed confirmation popup that presents a title, message, and confirm/cancel buttons.
///		Supports a destructive styling mode for irreversible actions.
/// </summary>
public partial class DmConfirmPopup : Popup<bool>
{
	/// <summary>
	///		Initializes a new instance of <see cref="DmConfirmPopup"/> with the specified content and styling.
	/// </summary>
	/// <param name="title">
	///		The heading text displayed in the popup chrome.
	/// </param>
	/// <param name="message">
	///		The body message asking the user to confirm or cancel.
	/// </param>
	/// <param name="confirmText">
	///		The label for the confirm button.
	/// </param>
	/// <param name="cancelText">
	///		The label for the cancel button.
	/// </param>
	/// <param name="isDestructive">
	///		When <see langword="true"/>, the confirm button is styled with destructive colours to signal an irreversible action.
	/// </param>
	public DmConfirmPopup(
		string title,
		string message,
		string confirmText = "Confirm",
		string cancelText = "Cancel",
		bool isDestructive = false)
	{
		InitializeComponent();
		Chrome.Title = title;
		MessageLabel.Text = message;
		CancelButton.Text = cancelText;
		ConfirmButton.Text = confirmText;

		if (isDestructive)
		{
			ConfirmButton.BackgroundColor = DmTheme.Destructive;
			ConfirmButton.TextColor = DmTheme.DestructiveText;
		}
		else
		{
			ConfirmButton.BackgroundColor = DmTheme.Confirm;
			ConfirmButton.TextColor = DmTheme.ConfirmText;
		}
	}

	private void OnConfirm(object? sender, EventArgs e) =>
		_ = CloseAsync(true);

	private void OnCancel(object? sender, EventArgs e) =>
		_ = CloseAsync(false);
}
