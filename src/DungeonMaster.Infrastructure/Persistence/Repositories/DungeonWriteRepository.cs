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
			.Include(dungeon =>
				dungeon.Floors)
			.ThenInclude(floor =>
				floor.Objects)
			.FirstOrDefaultAsync(dungeon =>
				dungeon.Id == id, ct);

	/// <inheritdoc/>
	public async Task AddAsync(Dungeon dungeon, CancellationToken ct)
	{
		await dbContext.Dungeons.AddAsync(dungeon, ct);
		await dbContext.SaveChangesAsync(ct);
	}

	/// <inheritdoc/>
	public async Task UpdateAsync(
		Dungeon dungeon,
		CancellationToken ct)
	{
		if (dbContext.Entry(dungeon).State is EntityState.Detached)
		{
			dbContext.Update(dungeon);
		}

		await dbContext.SaveChangesAsync(ct);
	}

	/// <inheritdoc/>
	public async Task<bool> DeleteAsync(
		Guid id,
		CancellationToken ct)
	{
		var dungeon = await dbContext.Dungeons.FindAsync(
			[id],
			ct);

		if (dungeon is null)
		{
			return false;
		}

		dbContext.Dungeons.Remove(dungeon);
		await dbContext.SaveChangesAsync(ct);

		return true;
	}
}
