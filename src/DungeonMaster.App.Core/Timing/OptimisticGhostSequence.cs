namespace DungeonMaster.App.Core.Timing;

/// <summary>
///		The tail of an optimistic-visual cycle: the placeholder is torn down only after the
///		refresh that replaces it has completed.
/// </summary>
/// <remarks>
///		<para>
///		MASTER-166. The draw and move paths both showed an optimistic visual, awaited a local
///		database write, and then cleared the placeholder BEFORE awaiting the reload that puts the
///		persisted shape on screen. That ordering opens a window in which neither the placeholder
///		nor the persisted shape is drawn, so the shape can blink out and back in. Ordering the
///		clear after the refresh closes the window: the placeholder is still on screen when the
///		real geometry arrives to replace it.
///		</para>
///		<para>
///		This exists as a seam rather than as two inline statements so that the order is asserted
///		by a test that records calls, instead of by reading the source. It lives in
///		DungeonMaster.App.Core, which references no MAUI package, so the assertion needs no
///		application host.
///		</para>
/// </remarks>
public static class OptimisticGhostSequence
{
	/// <summary>
	///		Awaits <paramref name="refreshAsync"/> to completion and only then runs
	///		<paramref name="clearPlaceholder"/>.
	/// </summary>
	/// <param name="refreshAsync">
	///		Reloads the persisted state and repaints from it.
	/// </param>
	/// <param name="clearPlaceholder">
	///		Removes the optimistic visual. Runs after <paramref name="refreshAsync"/> has
	///		completed, and does not run at all if it faults.
	/// </param>
	/// <returns>
	///		A task that completes once the refresh has completed and the placeholder has been
	///		cleared.
	/// </returns>
	/// <exception cref="ArgumentNullException">
	///		<paramref name="refreshAsync"/> or <paramref name="clearPlaceholder"/> is
	///		<see langword="null"/>.
	/// </exception>
	public static async Task RefreshThenClearAsync(
		Func<Task> refreshAsync,
		Action clearPlaceholder)
	{
		ArgumentNullException.ThrowIfNull(refreshAsync);
		ArgumentNullException.ThrowIfNull(clearPlaceholder);

		await refreshAsync();

		clearPlaceholder();
	}
}
