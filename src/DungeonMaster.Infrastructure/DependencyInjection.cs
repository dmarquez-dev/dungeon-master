using DungeonMaster.Application.Features.Dungeons;
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
		services.AddDbContext<AppDbContext>(options =>
			options.UseSqlite(connectionString));

		services.AddScoped<IDungeonWriteRepository, DungeonWriteRepository>();
		services.AddScoped<IDungeonReadRepository, DungeonReadRepository>();

		return services;
	}

	/// <summary>
	///		Applies any pending EF Core migrations to the database.
	/// </summary>
	/// <param name="services">
	///		The application service provider used to resolve the database context.
	/// </param>
	public static void MigrateDatabase(this IServiceProvider services)
	{
		using var scope = services.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

		db.Database.Migrate();
	}
}
