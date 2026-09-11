using DungeonMaster.Application.Common.Interfaces;
using DungeonMaster.Infrastructure.Persistence;
using DungeonMaster.Infrastructure.Persistence.Repositories;
using DungeonMaster.Tests.Infrastructure.Persistence.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DungeonMaster.Tests.Infrastructure.Persistence;

/// <summary>
///		The one in-memory SQLite fixture for every Infrastructure persistence test class (MASTER-065).
///		Holds a single open <see cref="SqliteConnection"/> against <c>Data Source=:memory:</c> for the lifetime of one test instance, so the schema and the data survive between the contexts a test opens and are discarded with the instance.
///		The schema is applied with <c>Database.Migrate()</c> rather than <c>EnsureCreated()</c> (MASTER-014), so the tests exercise the migration chain that ships and a migration that was never applied cannot pass unnoticed.
///		This is the one non-sealed <see cref="IDisposable"/> in the set and therefore the one that carries the full disposal pattern (MASTER-127); every derived class is <c>sealed</c> and inherits it.
/// </summary>
public abstract class SqliteInMemoryTestBase : IDisposable
{
	private readonly SqliteConnection _connection;

	/// <summary>
	///		Opens the connection and applies the migrations to the fresh in-memory database.
	/// </summary>
	protected SqliteInMemoryTestBase()
	{
		_connection = new SqliteConnection("Data Source=:memory:");
		_connection.Open();

		Options = new DbContextOptionsBuilder<AppDbContext>()
			.UseSqlite(_connection)
			.Options;

		using var context = new AppDbContext(Options);
		context.Database.Migrate();
	}

	/// <summary>
	///		The options every context in this fixture is built over, and therefore the handle onto the one open connection.
	///		Derived classes need it for anything the three helpers below do not cover, such as constructing a <c>TestDbContextFactory</c> directly.
	/// </summary>
	protected DbContextOptions<AppDbContext> Options { get; }

	/// <summary>
	///		Creates a context over the shared options.
	/// </summary>
	/// <returns>
	///		A new <see cref="AppDbContext"/> the caller owns and disposes.
	/// </returns>
	protected AppDbContext CreateContext() =>
		new(Options);

	/// <summary>
	///		Creates a context factory over the shared options, for production code that takes an <see cref="IDbContextFactory{TContext}"/> rather than a unit of work.
	/// </summary>
	/// <returns>
	///		A factory producing contexts over the one open connection.
	/// </returns>
	protected IDbContextFactory<AppDbContext> CreateDbContextFactory() =>
		new TestDbContextFactory(Options);

	/// <summary>
	///		Creates the real unit-of-work factory over the shared options.
	/// </summary>
	/// <returns>
	///		A factory producing units of work against the one open connection.
	/// </returns>
	protected IUnitOfWorkFactory CreateFactory() =>
		new UnitOfWorkFactory(new TestDbContextFactory(Options));

	/// <inheritdoc/>
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	///		Releases the connection that backs the in-memory database, discarding it.
	/// </summary>
	/// <param name="disposing">
	///		<see langword="true"/> when called from <see cref="Dispose()"/>.
	/// </param>
	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			_connection.Dispose();
		}
	}
}
