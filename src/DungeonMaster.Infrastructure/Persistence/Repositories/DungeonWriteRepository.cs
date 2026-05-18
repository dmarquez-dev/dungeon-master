using DungeonMaster.Application.Features.Dungeons;
using DungeonMaster.Domain.Entities.Dungeon;
using Microsoft.EntityFrameworkCore;

namespace DungeonMaster.Infrastructure.Persistence.Repositories;

/// <summary>
///		Provides write-side data access operations for <see cref="Dungeon"/> aggregates.
/// </summary>
public sealed class DungeonWriteRepository(
	AppDbContext dbContext)
		: IDungeonWriteRepository
{
	/// <inheritdoc/>
	public async Task<Dungeon?> LoadAsync(Guid id, CancellationToken ct)
		=> await dbContext.Dungeons
			.Include(dungeon =>
				dungeon.Floors)
			.ThenInclude(floor =>
				floor.Areas)
			.ThenInclude(area =>
				area.Objects)
			.FirstOrDefaultAsync(dungeon =>
				dungeon.Id == id, ct);

	/// <inheritdoc/>
	public async Task AddAsync(Dungeon dungeon, CancellationToken ct)
	{
		await dbContext.Dungeons.AddAsync(dungeon, ct);
		await dbContext.SaveChangesAsync(ct);
	}

	/// <inheritdoc/>
	public async Task UpdateAsync(Dungeon dungeon, CancellationToken ct)
	{
		if (dbContext.Entry(dungeon).State is EntityState.Detached)
		{
			dbContext.Attach(dungeon);
		}

		await dbContext.SaveChangesAsync(ct);
	}

	/// <inheritdoc/>
	public async Task DeleteAsync(Guid id, CancellationToken ct)
	{
		var dungeon = await dbContext.Dungeons.FindAsync([id], ct);

		if (dungeon is not null)
		{
			dbContext.Dungeons.Remove(dungeon);
			await dbContext.SaveChangesAsync(ct);
		}
	}
}
