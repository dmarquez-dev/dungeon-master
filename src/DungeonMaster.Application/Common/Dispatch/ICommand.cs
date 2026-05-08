namespace DungeonMaster.Application.Common.Dispatch;

/// <summary>
///		Marker interface for commands that return a <see cref="Results.Result{TResult}"/> on dispatch.
/// </summary>
/// <typeparam name="TResult">
///		The type of the value returned on success.
/// </typeparam>
public interface ICommand<TResult> { }
