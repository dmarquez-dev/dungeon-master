using DungeonMaster.Application.Common.Results;
using DungeonMaster.Domain.Entities;

namespace DungeonMaster.Application.Common.Handlers;

/// <summary>
///		Maps a Domain mutation outcome onto the Application layer's result type.
/// </summary>
public static class MutationOutcomeExtensions
{
	/// <summary>
	///		The message used when a mutation reports a value outside the permitted range and the caller supplied no more specific wording.
	/// </summary>
	public const string DefaultOutOfRangeMessage =
		"The requested change is outside the permitted range.";

	/// <summary>
	///		Maps the outcome, using the supplied message for a not-found result.
	/// </summary>
	/// <param name="outcome">
	///		The outcome reported by the aggregate mutation.
	/// </param>
	/// <param name="notFoundMessage">
	///		The message to carry when the mutation reported that no matching child exists.
	/// </param>
	/// <returns>
	///		A <see cref="Result"/> carrying the mapped category and message.
	/// </returns>
	/// <exception cref="ArgumentOutOfRangeException">
	///		Thrown when the outcome is not a recognised <see cref="MutationOutcome"/> member.
	/// </exception>
	public static Result ToResult(
		this MutationOutcome outcome,
		string notFoundMessage) =>
			outcome.ToResult(
				notFoundMessage,
				DefaultOutOfRangeMessage);

	/// <summary>
	///		Maps the outcome, using the supplied messages for a not-found and an out-of-range result.
	/// </summary>
	/// <param name="outcome">
	///		The outcome reported by the aggregate mutation.
	/// </param>
	/// <param name="notFoundMessage">
	///		The message to carry when the mutation reported that no matching child exists.
	/// </param>
	/// <param name="outOfRangeMessage">
	///		The message to carry when the mutation reported a value outside the permitted range.
	/// </param>
	/// <returns>
	///		A <see cref="Result"/> carrying the mapped category and message.
	/// </returns>
	/// <exception cref="ArgumentOutOfRangeException">
	///		Thrown when the outcome is not a recognised <see cref="MutationOutcome"/> member.
	/// </exception>
	public static Result ToResult(
		this MutationOutcome outcome,
		string notFoundMessage,
		string outOfRangeMessage) =>
			outcome switch
			{
				MutationOutcome.Success =>
					Result.Success(),
				MutationOutcome.NotFound =>
					Result.Failure(
						notFoundMessage,
						ErrorCategory.NotFound),
				MutationOutcome.OutOfRange =>
					Result.Failure(
						outOfRangeMessage,
						ErrorCategory.Validation),
				_ =>
					throw new ArgumentOutOfRangeException(
						nameof(outcome),
						outcome,
						"Unrecognised mutation outcome."),
			};
}
