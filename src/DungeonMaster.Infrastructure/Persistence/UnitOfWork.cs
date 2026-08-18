using DungeonMaster.Application.Common.Interfaces;
using DungeonMaster.Infrastructure.Persistence.Repositories;

namespace DungeonMaster.Infrastructure.Persistence;

/// <summary>
///		One persistence session with an explicit commit boundary, owning the <see cref="AppDbContext"/> it was created with.
///		Disposing the unit of work without committing discards every staged change.
/// </summary>
public sealed class UnitOfWork(
	AppDbContext dbContext)
		: IUnitOfWork
{
	/// <inheritdoc/>
	public IDungeonWriteRepository Dungeons { get; } = new DungeonWriteRepository(dbContext);

	/// <inheritdoc/>
	public Task<int> CommitAsync(CancellationToken ct) =>
		dbContext.SaveChangesAsync(ct);

	/// <inheritdoc/>
	public ValueTask DisposeAsync() =>
		dbContext.DisposeAsync();
}
