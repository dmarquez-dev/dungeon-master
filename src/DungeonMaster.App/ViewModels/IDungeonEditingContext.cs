using DungeonMaster.Application.Common.Results;

namespace DungeonMaster.App.ViewModels;

/// <summary>
///		What a child view model needs from the dungeon-detail composition root: which dungeon is
///		loaded, how to refresh one floor of it or reload all of it, and where to put a failure.
/// </summary>
/// <remarks>
///		<para>
///		This is the seam that makes the refresh policy explicit instead of implicit. Before it
///		existed the policy was a copy of <c>await LoadDungeonAsync(_dungeonId);</c> pasted at the
///		end of some mutations and omitted from others. Every mutating command that succeeds now
///		awaits exactly one of two members, and which one is decided by the shape of the change:
///		</para>
///		<para>
///		FLOOR-SCOPED MUTATIONS REFRESH THEIR FLOOR. A command that changes exactly one floor (an
///		area or placed-object write, an outline move, a floor's own name, description or notes)
///		awaits <see cref="RefreshFloorAsync"/> with that floor's identifier, which re-reads that
///		one floor and replaces it in place.
///		</para>
///		<para>
///		MEMBERSHIP- AND HEADER-SHAPED MUTATIONS RELOAD THE WHOLE DETAIL. Four commands await
///		<see cref="ReloadAsync"/> instead, each with its reason documented at its own declaration:
///		adding a floor and removing a floor change the floor list's membership (a removal also
///		renumbers every later floor), reordering a floor renumbers every floor between its two
///		positions, and saving the dungeon header changes the dungeon-level projection itself.
///		A one-floor read cannot describe any of those changes.
///		</para>
/// </remarks>
public interface IDungeonEditingContext
{
	/// <summary>
	///		Gets the identifier of the loaded dungeon, or <see cref="Guid.Empty"/> when none is
	///		loaded. This is the single source of the dungeon identifier: no command argument
	///		record carries one, so no caller can hand a command an identifier that disagrees with
	///		the dungeon actually on screen.
	/// </summary>
	Guid DungeonId { get; }

	/// <summary>
	///		Reports whether no dungeon is loaded. When this returns <see langword="true"/> the
	///		caller must abandon the operation; the error message has already been set.
	/// </summary>
	/// <returns>
	///		<see langword="true"/> when no dungeon is loaded and the operation must not proceed;
	///		otherwise <see langword="false"/>.
	/// </returns>
	bool NoDungeonIsLoaded();

	/// <summary>
	///		Sets the error message from a failed <paramref name="result"/> through
	///		<c>ResultMessage.ForFailure</c>, which is the single mapping that reads
	///		<c>ErrorCategory</c>.
	/// </summary>
	/// <param name="result">
	///		The failed result whose message is to be shown.
	/// </param>
	void ReportFailure(Result result);

	/// <summary>
	///		Clears the error message, so a new operation does not run under the previous
	///		operation's failure text.
	/// </summary>
	void ClearError();

	/// <summary>
	///		Re-runs the dungeon detail query for <see cref="DungeonId"/> and repopulates every
	///		child view model.
	/// </summary>
	/// <remarks>
	///		Only the membership- and header-shaped mutations await this directly, and each states
	///		why at its own declaration; every other successful mutation awaits
	///		<see cref="RefreshFloorAsync"/>. A new mutation that reloads the whole detail must be
	///		given the same documented reason at its declaration.
	/// </remarks>
	/// <returns>
	///		A task that completes when the dungeon has been re-read and the children repopulated.
	/// </returns>
	Task ReloadAsync();

	/// <summary>
	///		Re-reads the one floor identified by <paramref name="floorId"/> and replaces it in
	///		place, both in the floor list and in the loaded detail projection, leaving every other
	///		floor untouched.
	/// </summary>
	/// <remarks>
	///		Every successful floor-scoped mutation awaits this before returning. When the floor
	///		cannot be refreshed in place (the floor-scoped read finds nothing, the floor is not in
	///		the floor list, or no detail projection is loaded) it falls back to
	///		<see cref="ReloadAsync"/>, so the page is never left showing a floor that was not
	///		re-read.
	/// </remarks>
	/// <param name="floorId">
	///		The unique identifier of the floor the mutation changed.
	/// </param>
	/// <returns>
	///		A task that completes when the floor has been replaced, or when the fallback reload has
	///		completed.
	/// </returns>
	Task RefreshFloorAsync(Guid floorId);
}
