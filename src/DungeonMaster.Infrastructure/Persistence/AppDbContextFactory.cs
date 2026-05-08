using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DungeonMaster.Infrastructure.Persistence;

/// <summary>
///		Design-time factory for <see cref="AppDbContext"/>.
///		Used exclusively by <c>dotnet ef</c> tooling to create migrations and update the database.
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
	/// <summary>
	///		Creates a new <see cref="AppDbContext"/> pointed at <c>dungeon-master.db</c> in the current directory.
	/// </summary>
	/// <param name="args">
	///		Arguments passed by the EF Core tooling; not used.
	/// </param>
	/// <returns>
	///		A configured <see cref="AppDbContext"/> instance.
	/// </returns>
	public AppDbContext CreateDbContext(string[] args)
	{
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseSqlite("Data Source=dungeon-master.db")
			.Options;

		return new AppDbContext(options);
	}
}
