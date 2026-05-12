using DungeonMaster.Application.Common.Results;

namespace DungeonMaster.Application.Common.Dispatch;

/// <summary>
///		Defines the contract for handling a command of type <typeparamref name="TCommand"/>.
/// </summary>
/// <typeparam name="TCommand">
///		The command type this handler processes.
/// </typeparam>
/// <typeparam name="TResult">
///		The type of the value returned on success.
/// </typeparam>
public interface ICommandHandler<in TCommand, TResult>
	where TCommand : ICommand<TResult>
{
	/// <summary>
	///		Handles the given command and returns a result.
	/// </summary>
	/// <param name="command">
	///		The command to handle.
	/// </param>
	/// <param name="ct">
	///		Token used to cancel the operation.
	/// </param>
	/// <returns>
	///		A <see cref="Result{TResult}"/> representing the outcome of the operation.
	/// </returns>
	Task<Result<TResult>> HandleAsync(
		TCommand command,
		CancellationToken ct);
}
