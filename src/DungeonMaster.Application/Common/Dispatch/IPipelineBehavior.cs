using DungeonMaster.Application.Common.Results;

namespace DungeonMaster.Application.Common.Dispatch;

/// <summary>
///		Defines a pipeline stage that wraps command handling with cross-cutting behavior.
/// </summary>
/// <typeparam name="TCommand">
///		The command type this behavior applies to.
/// </typeparam>
/// <typeparam name="TResult">
///		The type of the value returned by the command on success.
/// </typeparam>
public interface IPipelineBehavior<in TCommand, TResult>
	where TCommand : ICommand<TResult>
{
	/// <summary>
	///		Executes the behavior and invokes the next stage in the pipeline.
	/// </summary>
	/// <param name="command">
	///		The command being processed.
	/// </param>
	/// <param name="next">
	///		A delegate that invokes the next stage in the pipeline.
	/// </param>
	/// <param name="ct">
	///		Token used to cancel the operation.
	/// </param>
	/// <returns>
	///		A <see cref="Result{TResult}"/> representing the outcome of the pipeline execution.
	/// </returns>
	Task<Result<TResult>> HandleAsync(
		TCommand command,
		Func<Task<Result<TResult>>> next,
		CancellationToken ct);
}
