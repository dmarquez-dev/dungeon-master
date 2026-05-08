namespace DungeonMaster.Application.Common.Dispatch;

/// <summary>
///		Defines a handler for <typeparamref name="TQuery"/>.
/// </summary>
/// <typeparam name="TQuery">
///		The query type.
/// </typeparam>
/// <typeparam name="TResult">
///		The type of value returned by the query.
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
	///		A token that may signal cancellation.
	/// </param>
	Task<TResult> HandleAsync(
		TQuery query,
		CancellationToken ct);
}
