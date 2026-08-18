namespace DungeonMaster.Application.Common.Dispatch;

/// <summary>
///		Marker interface for queries that return a result of type <typeparamref name="TResult"/>.
/// </summary>
/// <typeparam name="TResult">
///		The type of the data returned by the query.
/// </typeparam>
public interface IQuery<TResult>
	: IRequest<TResult>
{
}
