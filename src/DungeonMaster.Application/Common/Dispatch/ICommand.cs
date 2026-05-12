namespace DungeonMaster.Application.Common.Dispatch;

/// <summary>
///		Marker interface for commands that return a result of type <typeparamref name="TResult"/>.
/// </summary>
/// <typeparam name="TResult">
///		The type of the value produced when the command succeeds.
/// </typeparam>
public interface ICommand<TResult> { }
