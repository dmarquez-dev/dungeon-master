namespace DungeonMaster.Application.Common.Dispatch;

/// <summary>
///		Marker interface for queries that return <typeparamref name="TResult"/> directly on dispatch.
///		Queries do not use a result wrapper because an infrastructure failure is exceptional, not an expected application outcome.
/// </summary>
/// <typeparam name="TResult">
///		The type of value returned by the query.
/// </typeparam>
public interface IQuery<TResult> { }
