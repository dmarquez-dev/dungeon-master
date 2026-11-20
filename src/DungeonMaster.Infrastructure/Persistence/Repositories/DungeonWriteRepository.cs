using DungeonMaster.Application.Common.Interfaces;
using DungeonMaster.Domain.Entities.Dungeons;
using Microsoft.EntityFrameworkCore;

namespace DungeonMaster.Infrastructure.Persistence.Repositories;

/// <summary>
///		Provides write-side data access operations for <see cref="Dungeon"/> aggregates.
///		The context is owned by the <see cref="UnitOfWork"/> that constructed this repository; nothing here saves or disposes it.
/// </summary>
public sealed class DungeonWriteRepository(
	AppDbContext dbContext)
		: IDungeonWriteRepository
{
	/// <inheritdoc/>
	public async Task<Dungeon?> LoadAsync(Guid id, CancellationToken cancellationToken)
	{
		var query = dbContext.Dungeons
			.Include(dungeon =>
				dungeon.Floors)
			.ThenInclude(floor =>
				floor.Areas)
			.Include(dungeon =>
				dungeon.Floors)
			.ThenInclude(floor =>
				floor.Objects)
			.AsSplitQuery()
			.Where(dungeon =>
				dungeon.Id == id);

		return await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
	}

	/// <inheritdoc/>
	public void Add(Dungeon dungeon) =>
		dbContext.Dungeons.Add(dungeon);

	/// <inheritdoc/>
	public void Update(Dungeon dungeon)
	{
		if (dbContext.Entry(dungeon).State is EntityState.Detached)
		{
			dbContext.Update(dungeon);
		}
	}

	/// <inheritdoc/>
	public async Task<bool> RemoveAsync(
		Guid id,
		CancellationToken cancellationToken)
	{
		var dungeon = await dbContext.Dungeons.FindAsync([id], cancellationToken).ConfigureAwait(false);

		if (dungeon is null)
		{
			return false;
		}

		dbContext.Dungeons.Remove(dungeon);

		return true;
	}
}
