namespace DungeonMaster.Application.Common.Dispatch;

/// <summary>
///		Defines the contract for handling a query of type <typeparamref name="TQuery"/>.
/// </summary>
/// <typeparam name="TQuery">
///		The query type this handler processes.
/// </typeparam>
/// <typeparam name="TResult">
///		The type of data returned by the query.
/// </typeparam>
public interface IQueryHandler<in TQuery, TResult>
	where TQuery : IQuery<TResult>
{
	/// <summary>
	///		Handles the given query and returns the requested data.
	/// </summary>
	/// <param name="query">
	///		The query to handle.
	/// </param>
	/// <param name="ct">
	///		Token used to cancel the operation.
	/// </param>
	/// <returns>
	///		The result of type <typeparamref name="TResult"/>.
	/// </returns>
	Task<TResult> HandleAsync(
		TQuery query,
		CancellationToken ct);
}
