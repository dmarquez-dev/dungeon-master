using DungeonMaster.App.Core.Map;

namespace DungeonMaster.App.Views.DungeonDetail;

/// <summary>
///		Rejects a <see cref="DrawSessionResult"/> that does not describe a placement attempt the
///		drawing session performed, so a leaked value is detected at the consumer rather than
///		silently falling through the page's equality comparisons (MASTER-179).
/// </summary>
internal static class DrawSessionResultGuard
{
	/// <summary>
	///		Returns the result unchanged when it describes a placement outcome.
	/// </summary>
	/// <param name="result">
	///		The result reported by <see cref="MapDrawingSession.PlaceVertex"/>.
	/// </param>
	/// <returns>
	///		The same <see cref="DrawSessionResult"/> that was supplied.
	/// </returns>
	/// <exception cref="ArgumentOutOfRangeException">
	///		Thrown when the result does not describe a placement attempt the session performed: either
	///		<see cref="DrawSessionResult.None"/>, which is a declared member that
	///		<see cref="MapDrawingSession.PlaceVertex"/> never returns,
	///		or a value outside the declared <see cref="DrawSessionResult"/> members.
	/// </exception>
	internal static DrawSessionResult EnsurePlacementOutcome(
		this DrawSessionResult result) =>
			result switch
			{
				DrawSessionResult.Added =>
					result,
				DrawSessionResult.Closed =>
					result,
				DrawSessionResult.Ignored =>
					result,
				DrawSessionResult.None =>
					throw new ArgumentOutOfRangeException(
						nameof(result),
						result,
						"No placement attempt was made."),
				_ =>
					throw new ArgumentOutOfRangeException(
						nameof(result),
						result,
						"Unrecognised draw session result."),
			};
}
