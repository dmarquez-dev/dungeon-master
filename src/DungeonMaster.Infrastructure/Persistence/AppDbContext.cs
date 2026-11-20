using DungeonMaster.Domain.Entities.Dungeons;
using Microsoft.EntityFrameworkCore;

namespace DungeonMaster.Infrastructure.Persistence;

/// <summary>
///		The Entity Framework Core database context for the DungeonMaster application.
/// </summary>
public sealed class AppDbContext(
	DbContextOptions<AppDbContext> options)
		: DbContext(options)
{
	/// <summary>
	///		Gets the set of all persisted dungeons.
	///		<see cref="Dungeon"/> is the only aggregate root, so it is the only set exposed: <c>Floor</c>, <c>Area</c> and <c>PlacedObject</c> are reached by navigation from it.
	/// </summary>
	public DbSet<Dungeon> Dungeons => Set<Dungeon>();

	/// <summary>
	///		Applies all entity type configurations discovered in the executing assembly.
	/// </summary>
	/// <param name="modelBuilder">
	///		The builder used to construct the model for this context.
	/// </param>
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
	}
}
