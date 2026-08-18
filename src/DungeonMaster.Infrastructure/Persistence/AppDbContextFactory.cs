using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DungeonMaster.Infrastructure.Persistence;

/// <summary>
///		Creates <see cref="AppDbContext"/> instances at design time for EF Core tooling such as migrations.
/// </summary>
/// <remarks>
///		The database path is resolved so that design time and run time reach the same file.
///		The run-time path is built by <c>MauiProgram.CreateMauiApp</c> and is the default here.
/// </remarks>
public sealed class AppDbContextFactory
	: IDesignTimeDbContextFactory<AppDbContext>
{
	/// <summary>
	///		The name of the environment variable that overrides the design-time database path.
	/// </summary>
	public const string DatabasePathVariable = "DUNGEONMASTER_DB_PATH";

	/// <summary>
	///		Creates a new <see cref="AppDbContext"/> configured with the same local SQLite database the application uses at run time.
	/// </summary>
	/// <param name="args">
	///		Command-line arguments passed by the EF Core tooling after <c>--</c>.
	///		The first argument, when present and non-blank, is taken as the database file path and wins over every other source.
	/// </param>
	/// <returns>
	///		A fully configured <see cref="AppDbContext"/> instance.
	/// </returns>
	public AppDbContext CreateDbContext(string[] args)
	{
		var databasePath = ResolveDatabasePath(args);

		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseSqlite($"Data Source={databasePath}")
			.Options;

		return new AppDbContext(options);
	}

	/// <summary>
	///		Resolves the design-time database path from, in order, the first command-line argument, the <see cref="DatabasePathVariable"/> environment variable, and the run-time default under <see cref="Environment.SpecialFolder.LocalApplicationData"/>.
	/// </summary>
	/// <param name="args">
	///		Command-line arguments passed by the EF Core tooling.
	/// </param>
	/// <returns>
	///		The absolute or tooling-relative path to the SQLite database file.
	/// </returns>
	private static string ResolveDatabasePath(string[] args)
	{
		if (args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]))
		{
			return args[0];
		}

		var fromEnvironment = Environment.GetEnvironmentVariable(DatabasePathVariable);

		if (!string.IsNullOrWhiteSpace(fromEnvironment))
		{
			return fromEnvironment;
		}

		return Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
			"DungeonMaster",
			"dungeon-master.db");
	}
}
