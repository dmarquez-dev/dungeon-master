using DungeonMaster.Application.Common.Behaviors;
using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Application.Features.Dungeons.Commands.CreateDungeon;
using DungeonMaster.Application.Features.Dungeons.Commands.DeleteDungeon;
using DungeonMaster.Application.Features.Dungeons.Queries.GetDungeons;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DungeonMaster.Application;

/// <summary>
///		Provides extension methods for registering Application layer services into the dependency injection container.
/// </summary>
public static class ApplicationServiceCollectionExtensions
{
	/// <summary>
	///		Registers all Application layer services, including the dispatcher, pipeline behaviors, validators, and command/query handlers.
	/// </summary>
	/// <param name="services">
	///		The service collection to extend.
	/// </param>
	/// <returns>
	///		The service collection, enabling method chaining.
	/// </returns>
	public static IServiceCollection AddApplicationServices(this IServiceCollection services)
	{
		services.AddScoped<IDispatcher, Dispatcher>();

		services.AddScoped(
			typeof(IPipelineBehavior<,>),
			typeof(ValidationBehavior<,>));

		services.AddScoped<IValidator<CreateDungeonCommand>, CreateDungeonCommandValidator>();

		services.AddScoped<
			IQueryHandler<GetDungeonsQuery, IReadOnlyList<DungeonSummaryDto>>,
			GetDungeonsQueryHandler>();

		services.AddScoped<
			ICommandHandler<CreateDungeonCommand, Guid>,
			CreateDungeonCommandHandler>();

		services.AddScoped<
			ICommandHandler<DeleteDungeonCommand, bool>,
			DeleteDungeonCommandHandler>();

		return services;
	}
}
