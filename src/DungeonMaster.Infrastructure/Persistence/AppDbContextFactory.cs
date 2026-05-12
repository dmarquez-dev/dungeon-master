using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DungeonMaster.Infrastructure.Persistence;

/// <summary>
///		Creates <see cref="AppDbContext"/> instances at design time for EF Core tooling such as migrations.
/// </summary>
public sealed class AppDbContextFactory
	: IDesignTimeDbContextFactory<AppDbContext>
{
	/// <summary>
	///		Creates a new <see cref="AppDbContext"/> configured with a local SQLite database for design-time operations.
	/// </summary>
	/// <param name="args">
	///		Command-line arguments passed by the EF Core tooling. Not used.
	/// </param>
	/// <returns>
	///		A fully configured <see cref="AppDbContext"/> instance.
	/// </returns>
	public AppDbContext CreateDbContext(string[] args)
	{
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseSqlite("Data Source=dungeon-master.db")
			.Options;

		return new AppDbContext(options);
	}
}
