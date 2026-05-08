using DungeonMaster.Application.Features.Dungeons;
using DungeonMaster.Infrastructure.Persistence;
using DungeonMaster.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DungeonMaster.Infrastructure;

/// <summary>
///		Provides extension methods for registering Infrastructure layer services into the dependency injection container.
/// </summary>
public static class InfrastructureServiceCollectionExtensions
{
	/// <summary>
	///		Registers all Infrastructure layer services, including the EF Core <see cref="AppDbContext"/> and repository implementations.
	/// </summary>
	/// <param name="services">
	///		The service collection to extend.
	/// </param>
	/// <param name="connectionString">
	///		The SQLite connection string used to configure <see cref="AppDbContext"/>.
	/// </param>
	/// <returns>
	///		The service collection, enabling method chaining.
	/// </returns>
	public static IServiceCollection AddInfrastructureServices(
		this IServiceCollection services,
		string connectionString)
	{
		services.AddDbContext<AppDbContext>(options =>
			options.UseSqlite(connectionString));

		services.AddScoped<IDungeonRepository, DungeonRepository>();

		return services;
	}
}
