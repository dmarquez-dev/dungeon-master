using DungeonMaster.Application.Common.Results;

namespace DungeonMaster.Application.Common.Dispatch;

/// <summary>
///		Dispatches commands through the behavior pipeline to their handlers,
///		and routes queries directly to their handlers.
/// </summary>
public interface IDispatcher
{
	/// <summary>
	///		Dispatches <paramref name="command"/> through the pipeline and returns the result.
	/// </summary>
	/// <typeparam name="TResult">
	///		The type of the value returned on success.
	/// </typeparam>
	/// <param name="command">
	///		The command to dispatch.
	/// </param>
	/// <param name="ct">
	///		A token that may signal cancellation.
	/// </param>
	Task<Result<TResult>> SendAsync<TResult>(
		ICommand<TResult> command,
		CancellationToken ct = default);

	/// <summary>
	///		Dispatches <paramref name="query"/> to its handler and returns the result directly.
	/// </summary>
	/// <typeparam name="TResult">
	///		The type of value returned by the query.
	/// </typeparam>
	/// <param name="query">
	///		The query to dispatch.
	/// </param>
	/// <param name="ct">
	///		A token that may signal cancellation.
	/// </param>
	Task<TResult> QueryAsync<TResult>(
		IQuery<TResult> query,
		CancellationToken ct = default);
}
