using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Application.Common.Results;
using FluentValidation;

namespace DungeonMaster.Application.Common.Behaviors;

/// <summary>
///		Pipeline behavior that runs all registered validators for a command before passing it to the handler.
///		Returns a validation failure result if any validator reports errors.
/// </summary>
/// <typeparam name="TCommand">
///		The command type being validated.
/// </typeparam>
/// <typeparam name="TResult">
///		The type of the value returned by the command on success.
/// </typeparam>
public sealed class ValidationBehavior<TCommand, TResult>(
	IEnumerable<IValidator<TCommand>> validators)
		: IPipelineBehavior<TCommand, TResult>
			where TCommand : ICommand<TResult>
{
	/// <inheritdoc/>
	public async Task<Result<TResult>> HandleAsync(
		TCommand command,
		Func<Task<Result<TResult>>> next,
		CancellationToken ct)
	{
		var context = new ValidationContext<TCommand>(command);

		var failures = validators
			.Select(validator =>
				validator.Validate(context))
			.SelectMany(result =>
				result.Errors)
			.Where(failure =>
				failure is not null)
			.ToList();

		if (failures.Count > 0)
		{
			return Result<TResult>.Failure(
				failures[0].ErrorMessage,
				ErrorCategory.Validation);
		}

		return await next();
	}
}
