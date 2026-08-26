using DungeonMaster.Application.Common.Interfaces;
using DungeonMaster.Infrastructure.Persistence;
using DungeonMaster.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DungeonMaster.Infrastructure;

/// <summary>
///		Provides extension methods for registering Infrastructure layer services with the DI container.
/// </summary>
public static class InfrastructureServiceCollectionExtensions
{
	/// <summary>
	///		Registers all Infrastructure services including the database context and repository implementations.
	/// </summary>
	/// <param name="services">
	///		The service collection to add registrations to.
	/// </param>
	/// <param name="connectionString">
	///		The database connection string used to configure the SQLite context.
	/// </param>
	/// <returns>
	///		The same <see cref="IServiceCollection"/> instance for chaining.
	/// </returns>
	public static IServiceCollection AddInfrastructureServices(
		this IServiceCollection services,
		string connectionString)
	{
		services.AddDbContextFactory<AppDbContext>(options =>
			options.UseSqlite(connectionString));

		services.AddSingleton<IUnitOfWorkFactory, UnitOfWorkFactory>();
		services.AddSingleton<IDungeonReadRepository, DungeonReadRepository>();

		return services;
	}

	/// <summary>
	///		Applies any pending EF Core migrations to the database.
	/// </summary>
	/// <param name="services">
	///		The application service provider used to resolve the database context factory.
	/// </param>
	/// <param name="ct">
	///		Token used to cancel the migration.
	/// </param>
	/// <returns>
	///		A task that completes when every pending migration has been applied.
	/// </returns>
	public static async Task MigrateDatabaseAsync(
		this IServiceProvider services,
		CancellationToken ct)
	{
		var dbContextFactory = services.GetRequiredService<IDbContextFactory<AppDbContext>>();
		var dbContext = await dbContextFactory.CreateDbContextAsync(ct).ConfigureAwait(false);
		await using var dbContextScope = dbContext.ConfigureAwait(false);

		await dbContext.Database.MigrateAsync(ct).ConfigureAwait(false);
	}
}
