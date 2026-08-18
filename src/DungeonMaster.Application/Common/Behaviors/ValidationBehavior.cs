using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Application.Common.Results;
using FluentValidation;

namespace DungeonMaster.Application.Common.Behaviors;

/// <summary>
///		Rejects a no-payload command that fails validation, before the handler runs.
/// </summary>
/// <typeparam name="TCommand">
///		The command type being validated.
/// </typeparam>
/// <param name="validators">
///		The validators registered for the command type.
/// </param>
public sealed class ValidationBehavior<TCommand>(
	IEnumerable<IValidator<TCommand>> validators)
		: IPipelineBehavior<TCommand, Result>
			where TCommand : ICommand
{
	/// <inheritdoc/>
	public async Task<Result> HandleAsync(
		TCommand command,
		Func<Task<Result>> next,
		CancellationToken ct)
	{
		var errors = await ValidationFailureCollector.CollectAsync(
			validators,
			command,
			ct);

		if (errors.Count > 0)
		{
			return Result.ValidationFailure(errors);
		}

		return await next();
	}
}

/// <summary>
///		Rejects a value-producing command that fails validation, before the handler runs.
/// </summary>
/// <typeparam name="TCommand">
///		The command type being validated.
/// </typeparam>
/// <typeparam name="TResult">
///		The type of the value returned by the command on success.
/// </typeparam>
/// <param name="validators">
///		The validators registered for the command type.
/// </param>
public sealed class ValidationBehavior<TCommand, TResult>(
	IEnumerable<IValidator<TCommand>> validators)
		: IPipelineBehavior<TCommand, Result<TResult>>
			where TCommand : ICommand<TResult>
{
	/// <inheritdoc/>
	public async Task<Result<TResult>> HandleAsync(
		TCommand command,
		Func<Task<Result<TResult>>> next,
		CancellationToken ct)
	{
		var errors = await ValidationFailureCollector.CollectAsync(
			validators,
			command,
			ct);

		if (errors.Count > 0)
		{
			return Result<TResult>.ValidationFailure(errors);
		}

		return await next();
	}
}
