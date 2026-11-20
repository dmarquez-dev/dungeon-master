namespace DungeonMaster.App.Services.Navigation;

/// <summary>
///		The one place in the application that reads <c>Shell.Current</c> for navigation.
/// </summary>
/// <remarks>
///		A null <c>Shell.Current</c> is a defect, not an expected failure: it means navigation was
///		requested before the Shell existed. The route is named in the thrown
///		<see cref="InvalidOperationException"/> rather than the request being dropped — a
///		silently-ignored navigation leaves the user on a page that did not change and leaves no
///		trace of why.
/// </remarks>
public sealed class ShellNavigationService : INavigationService
{
	/// <inheritdoc/>
	public Task GoToAsync(string route)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(route);

		return CurrentShell(route)
			.GoToAsync(route);
	}

	/// <inheritdoc/>
	public Task GoBackAsync() =>
		GoToAsync("..");

	// Shell.Current is the single static ambient read this type exists to contain. It is read
	// through this helper so both entry points fail the same way, naming the route that could
	// not be taken.
	private static Shell CurrentShell(string route)
	{
		var shell = Shell.Current;

		if (shell is null)
		{
			throw new InvalidOperationException(
				$"Navigation to '{route}' was requested before a Shell existed.");
		}

		return shell;
	}
}
