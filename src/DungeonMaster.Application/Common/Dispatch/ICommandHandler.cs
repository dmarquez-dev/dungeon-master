using DungeonMaster.Application.Common.Results;

namespace DungeonMaster.Application.Common.Dispatch;

/// <summary>
///		Defines a handler for <typeparamref name="TCommand"/>.
/// </summary>
/// <typeparam name="TCommand">
///		The command type.
/// </typeparam>
/// <typeparam name="TResult">
///		The type of the value returned on success.
/// </typeparam>
public interface ICommandHandler<in TCommand, TResult>
	where TCommand : ICommand<TResult>
{
	/// <summary>
	///		Handles the given command and returns a result indicating success or failure.
	/// </summary>
	/// <param name="command">
	///		The command to handle.
	/// </param>
	/// <param name="ct">
	///		A token that may signal cancellation.
	/// </param>
	Task<Result<TResult>> HandleAsync(
		TCommand command,
		CancellationToken ct);
}
