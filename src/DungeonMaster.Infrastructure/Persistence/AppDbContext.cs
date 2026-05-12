using DungeonMaster.Domain.Entities.Area;
using DungeonMaster.Domain.Entities.Dungeon;
using DungeonMaster.Domain.Entities.Floor;
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
	/// </summary>
	public DbSet<Dungeon> Dungeons => Set<Dungeon>();

	/// <summary>
	///		Gets the set of all persisted floors.
	/// </summary>
	public DbSet<Floor> Floors => Set<Floor>();

	/// <summary>
	///		Gets the set of all persisted areas.
	/// </summary>
	public DbSet<Area> Areas => Set<Area>();

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
