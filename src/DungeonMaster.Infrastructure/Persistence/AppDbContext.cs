using DungeonMaster.Domain.Entities.Dungeon;
using Microsoft.EntityFrameworkCore;

namespace DungeonMaster.Infrastructure.Persistence;

/// <summary>
///		EF Core database context for the DungeonMaster application.
///		All entity configurations are applied from the Infrastructure assembly.
/// </summary>
public sealed class AppDbContext(
	DbContextOptions<AppDbContext> options)
		: DbContext(options)
{
	/// <summary>
	///		Gets the set of persisted dungeons.
	/// </summary>
	public DbSet<Dungeon> Dungeons =>
		Set<Dungeon>();

	/// <inheritdoc/>
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
	}
}
