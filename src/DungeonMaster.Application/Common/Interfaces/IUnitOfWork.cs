namespace DungeonMaster.Application.Common.Interfaces;

/// <summary>
///		Represents one persistence session with an explicit commit boundary.
///		A unit of work is created when a dispatched command begins and is disposed when that command's handler returns.
/// </summary>
public interface IUnitOfWork
	: IAsyncDisposable
{
	/// <summary>
	///		Gets the write-side dungeon repository bound to this unit of work.
	/// </summary>
	IDungeonWriteRepository Dungeons { get; }

	/// <summary>
	///		Writes every change tracked by this unit of work to the store.
	/// </summary>
	/// <param name="ct">
	///		Token used to cancel the operation.
	/// </param>
	/// <returns>
	///		The number of state entries written to the store.
	///		Returns zero when nothing was tracked as changed.
	/// </returns>
	Task<int> CommitAsync(CancellationToken ct);
}
