using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using DungeonMaster.App.Controls;

namespace DungeonMaster.App.Extensions;

/// <summary>
///		Provides extension methods for <see cref="INavigation"/> that simplify common navigation patterns.
/// </summary>
internal static class NavigationExtensions
{
	/// <summary>
	///		Presents a <see cref="DmConfirmPopup"/> and returns <see langword="true"/> when the user confirms.
	/// </summary>
	/// <param name="navigation">
	///		The navigation stack on which to show the popup.
	/// </param>
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
	///		When <see langword="true"/>, the confirm button is styled with destructive colours.
	/// </param>
	/// <returns>
	///		<see langword="true"/> if the user pressed confirm; otherwise <see langword="false"/>.
	/// </returns>
	internal static async Task<bool> ShowConfirmPopupAsync(
		this INavigation navigation,
		string title,
		string message,
		string confirmText = "CONFIRM",
		string cancelText = "CANCEL",
		bool isDestructive = false)
	{
		var popup = new DmConfirmPopup(
			title,
			message,
			confirmText,
			cancelText,
			isDestructive);

		var result = await navigation.ShowPopupAsync<bool>(
			popup,
			new PopupOptions { Shape = null, Shadow = null });

		return result.Result;
	}
}
