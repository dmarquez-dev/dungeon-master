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

		// THE STYLE IS SWAPPED, NOT THE COLOURS, AND THAT IS LOAD-BEARING.
		// DmConfirmButton declares its own CommonStates group whose Normal state sets
		// BackgroundColor and TextColor. A VisualState setter outranks a locally-assigned
		// value, so the earlier form of this method — ConfirmButton.BackgroundColor =
		// DmTheme.Destructive — was overruled the instant the button entered Normal, which is
		// immediately. Every destructive dialog would have rendered confirm-green, with a
		// clean build, no warning and nothing wrong-looking in the markup.
		// DmDestructiveButton has no states of its own: it inherits DmButtonBase's group,
		// whose Normal state sets Opacity and no colour, so its BackgroundColor setter is what
		// reaches the pixel. Do not give it one.
		ConfirmButton.Style = isDestructive
			? DmTheme.DestructiveButton
			: DmTheme.ConfirmButton;
	}

	private async void OnConfirm(object? sender, EventArgs e)
	{
		try
		{
			await CloseAsync(true);
		}
		catch (Exception exception)
		{
			// A failed close leaves the popup on screen and the awaiting caller's Task<bool>
			// incomplete, so the failure is recorded rather than swallowed. This control has no
			// view model and Popup<bool> exposes no error channel, so Debug.WriteLine is the
			// App layer's last-resort channel here.
			System.Diagnostics.Debug.WriteLine(
				$"DmConfirmPopup: failed to close with the confirm result. {exception}");
		}
	}

	private async void OnCancel(object? sender, EventArgs e)
	{
		try
		{
			await CloseAsync(false);
		}
		catch (Exception exception)
		{
			// A failed close leaves the popup on screen and the awaiting caller's Task<bool>
			// incomplete, so the failure is recorded rather than swallowed. This control has no
			// view model and Popup<bool> exposes no error channel, so Debug.WriteLine is the
			// App layer's last-resort channel here.
			System.Diagnostics.Debug.WriteLine(
				$"DmConfirmPopup: failed to close with the cancel result. {exception}");
		}
	}
}
