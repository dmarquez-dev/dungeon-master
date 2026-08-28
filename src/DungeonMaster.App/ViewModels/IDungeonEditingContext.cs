using DungeonMaster.Application.Common.Results;

namespace DungeonMaster.App.ViewModels;

/// <summary>
///		What a child view model needs from the dungeon-detail composition root: which dungeon is
///		loaded, how to reload it, and where to put a failure.
/// </summary>
/// <remarks>
///		This is the seam that makes the reload policy explicit instead of implicit. Before this
///		session the policy was a copy of <c>await LoadDungeonAsync(_dungeonId);</c> pasted at the
///		end of some mutations and omitted from others. It is now one member every mutating
///		command awaits.
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
	///		The reload policy is uniform: every member that sends a mutating command and succeeds
	///		awaits this before returning. There are no exceptions, and a member that wants one
	///		must be given a documented reason at its own declaration.
	/// </remarks>
	/// <returns>
	///		A task that completes when the dungeon has been re-read and the children repopulated.
	/// </returns>
	Task ReloadAsync();
}
