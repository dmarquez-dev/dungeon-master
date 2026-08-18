using DungeonMaster.Application.Common.Results;

namespace DungeonMaster.Application.Common.Dispatch;

/// <summary>
///		Dispatches commands and queries to their registered handlers through the pipeline.
/// </summary>
public interface IDispatcher
{
	/// <summary>
	///		Sends a command that produces no value through the pipeline and returns the result.
	/// </summary>
	/// <param name="command">
	///		The command to dispatch.
	/// </param>
	/// <param name="ct">
	///		Token used to cancel the operation.
	/// </param>
	/// <returns>
	///		A <see cref="Result"/> representing the outcome of the command.
	/// </returns>
	Task<Result> SendAsync(
		ICommand command,
		CancellationToken ct = default);

	/// <summary>
	///		Sends a command through the pipeline and returns the result.
	/// </summary>
	/// <typeparam name="TResult">
	///		The type of the value returned when the command succeeds.
	/// </typeparam>
	/// <param name="command">
	///		The command to dispatch.
	/// </param>
	/// <param name="ct">
	///		Token used to cancel the operation.
	/// </param>
	/// <returns>
	///		A <see cref="Result{TResult}"/> representing the outcome of the command.
	/// </returns>
	Task<Result<TResult>> SendAsync<TResult>(
		ICommand<TResult> command,
		CancellationToken ct = default);

	/// <summary>
	///		Sends a query to its handler and returns the result.
	/// </summary>
	/// <typeparam name="TResult">
	///		The type of data returned by the query.
	/// </typeparam>
	/// <param name="query">
	///		The query to dispatch.
	/// </param>
	/// <param name="ct">
	///		Token used to cancel the operation.
	/// </param>
	/// <returns>
	///		The result of type <typeparamref name="TResult"/>.
	/// </returns>
	Task<TResult> QueryAsync<TResult>(
		IQuery<TResult> query,
		CancellationToken ct = default);
}
