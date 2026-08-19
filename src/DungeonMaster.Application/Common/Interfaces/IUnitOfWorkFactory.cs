namespace DungeonMaster.Application.Common.Interfaces;

/// <summary>
///		Creates the unit of work that brackets a single dispatched command.
/// </summary>
public interface IUnitOfWorkFactory
{
	/// <summary>
	///		Creates a new unit of work with its own persistence session.
	/// </summary>
	/// <param name="ct">
	///		Token used to cancel the operation.
	/// </param>
	/// <returns>
	///		A new <see cref="IUnitOfWork"/> that the caller owns and must dispose.
	/// </returns>
	/// <exception cref="OperationCanceledException">
	///		Thrown when <paramref name="ct"/> is cancelled before the session is created.
	/// </exception>
	Task<IUnitOfWork> CreateAsync(CancellationToken ct);
}
