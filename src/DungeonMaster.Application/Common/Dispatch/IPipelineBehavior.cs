using DungeonMaster.Application.Common.Results;

namespace DungeonMaster.Application.Common.Dispatch;

/// <summary>
///		Defines a behavior that wraps command dispatch with cross-cutting logic such as validation or logging.
///		Behaviors are composed in registration order.
/// </summary>
/// <typeparam name="TCommand">
///		The command type.
/// </typeparam>
/// <typeparam name="TResult">
///		The result type.
/// </typeparam>
public interface IPipelineBehavior<in TCommand, TResult>
	where TCommand : ICommand<TResult>
{
	/// <summary>
	///		Executes cross-cutting logic and delegates to <paramref name="next"/> to continue the pipeline.
	/// </summary>
	/// <param name="command">
	///		The command being dispatched.
	/// </param>
	/// <param name="next">
	///		A delegate that invokes the next step in the pipeline.
	/// </param>
	/// <param name="ct">
	///		A token that may signal cancellation.
	/// </param>
	Task<Result<TResult>> HandleAsync(
		TCommand command,
		Func<Task<Result<TResult>>> next,
		CancellationToken ct);
}
