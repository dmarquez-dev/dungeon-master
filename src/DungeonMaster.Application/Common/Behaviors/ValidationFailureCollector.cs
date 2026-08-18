using DungeonMaster.Application.Common.Results;
using FluentValidation;

namespace DungeonMaster.Application.Common.Behaviors;

/// <summary>
///		Runs every validator for a command and gathers the failures.
/// </summary>
internal static class ValidationFailureCollector
{
	/// <summary>
	///		Runs each validator in turn and returns every failure, in validator order then rule order. An empty list means the command is valid.
	/// </summary>
	/// <typeparam name="TCommand">
	///		The command type being validated.
	/// </typeparam>
	/// <param name="validators">
	///		The validators registered for the command type.
	/// </param>
	/// <param name="command">
	///		The command instance to validate.
	/// </param>
	/// <param name="ct">
	///		Token used to cancel the operation.
	/// </param>
	/// <returns>
	///		Every validation failure reported by the supplied validators, in validator order then rule order.
	/// </returns>
	public static async Task<IReadOnlyList<ValidationError>> CollectAsync<TCommand>(
		IEnumerable<IValidator<TCommand>> validators,
		TCommand command,
		CancellationToken ct)
	{
		var context = new ValidationContext<TCommand>(command);
		var errors = new List<ValidationError>();

		foreach (var validator in validators)
		{
			var validationResult = await validator.ValidateAsync(
				context,
				ct);

			errors.AddRange(
				validationResult.Errors.Select(failure =>
					new ValidationError(
						failure.PropertyName,
						failure.ErrorMessage)));
		}

		return errors;
	}
}
