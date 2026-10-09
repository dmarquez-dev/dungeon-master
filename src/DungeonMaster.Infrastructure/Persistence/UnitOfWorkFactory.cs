using DungeonMaster.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DungeonMaster.Infrastructure.Persistence;

/// <summary>
///		Creates a <see cref="UnitOfWork"/> over a freshly constructed <see cref="AppDbContext"/>, one per dispatched command.
/// </summary>
public sealed class UnitOfWorkFactory(
	IDbContextFactory<AppDbContext> dbContextFactory)
		: IUnitOfWorkFactory
{
	/// <inheritdoc/>
	public async Task<IUnitOfWork> CreateAsync(CancellationToken cancellationToken)
	{
		var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

		return new UnitOfWork(dbContext);
	}
}
