using DungeonMaster.Application.Features.Dungeons;
using DungeonMaster.Domain.Entities.Dungeon;
using Microsoft.EntityFrameworkCore;

namespace DungeonMaster.Infrastructure.Persistence.Repositories;

/// <summary>
///		EF Core implementation of <see cref="IDungeonRepository"/>.
///		All reads and writes are performed against the <see cref="AppDbContext"/>.
/// </summary>
public sealed class DungeonRepository(
	AppDbContext dbContext)
		: IDungeonRepository
{
	/// <inheritdoc/>
	public async Task<IReadOnlyList<Dungeon>> GetAllAsync(CancellationToken ct)
	{
		return await dbContext.Dungeons
			.ToListAsync(ct);
	}

	/// <inheritdoc/>
	public async Task AddAsync(
		Dungeon dungeon,
		CancellationToken ct)
	{
		await dbContext.Dungeons
			.AddAsync(
				dungeon,
				ct);

		await dbContext.SaveChangesAsync(ct);
	}

	/// <inheritdoc/>
	public async Task DeleteAsync(
		Guid id,
		CancellationToken ct)
	{
		var dungeon = await dbContext.Dungeons
			.FindAsync(
				[id],
				ct);

		if (dungeon is not null)
		{
			dbContext.Dungeons.Remove(dungeon);
			await dbContext.SaveChangesAsync(ct);
		}
	}
}
