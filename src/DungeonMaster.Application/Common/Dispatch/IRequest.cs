namespace DungeonMaster.Application.Common.Dispatch;

/// <summary>
///		Marker interface for anything the dispatcher routes to a handler.
/// </summary>
/// <typeparam name="TResponse">
///		The type the handler returns, including any result wrapper.
/// </typeparam>
public interface IRequest<TResponse> { }
