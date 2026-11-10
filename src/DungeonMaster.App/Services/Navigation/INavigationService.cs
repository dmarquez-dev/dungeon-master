namespace DungeonMaster.App.Services.Navigation;

/// <summary>
///		Shell navigation, behind an interface so a view model can be exercised without a live
///		Shell.
/// </summary>
public interface INavigationService
{
	/// <summary>
	///		Navigates to a route.
	///		<paramref name="route"/> is a Shell route string and may carry a query.
	///		For example, <see cref="ShellRoutes.DungeonDetail"/> with its <see cref="ShellRoutes.IdQueryKey"/> query parameter set to a dungeon identifier.
	/// </summary>
	/// <param name="route">
	///		The Shell route to navigate to.
	/// </param>
	/// <returns>
	///		A task that completes when the navigation has been performed.
	/// </returns>
	/// <exception cref="ArgumentException"><paramref name="route"/> is null, empty or whitespace.</exception>
	Task GoToAsync(string route);

	/// <summary>
	///		Pops the current page. Implemented as <c>GoToAsync("..")</c>.
	/// </summary>
	/// <returns>
	///		A task that completes when the current page has been popped.
	/// </returns>
	Task GoBackAsync();
}
