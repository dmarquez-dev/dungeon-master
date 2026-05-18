using DungeonMaster.Application.Common.Behaviors;
using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Application.Features.Dungeons.Commands.AddArea;
using DungeonMaster.Application.Features.Dungeons.Commands.AddFloor;
using DungeonMaster.Application.Features.Dungeons.Commands.AddPlacedObject;
using DungeonMaster.Application.Features.Dungeons.Commands.CreateDungeon;
using DungeonMaster.Application.Features.Dungeons.Commands.DeleteDungeon;
using DungeonMaster.Application.Features.Dungeons.Commands.RemoveArea;
using DungeonMaster.Application.Features.Dungeons.Commands.RemoveFloor;
using DungeonMaster.Application.Features.Dungeons.Commands.RemovePlacedObject;
using DungeonMaster.Application.Features.Dungeons.Commands.ReorderFloor;
using DungeonMaster.Application.Features.Dungeons.Commands.UpdateArea;
using DungeonMaster.Application.Features.Dungeons.Commands.UpdateAreaOutline;
using DungeonMaster.Application.Features.Dungeons.Commands.UpdateDungeon;
using DungeonMaster.Application.Features.Dungeons.Commands.UpdateFloor;
using DungeonMaster.Application.Features.Dungeons.Commands.UpdatePlacedObject;
using DungeonMaster.Application.Features.Dungeons.Queries.GetDungeonDetail;
using DungeonMaster.Application.Features.Dungeons.Queries.GetDungeons;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DungeonMaster.Application;

/// <summary>
///		Extension methods for registering application layer services with the dependency injection container.
/// </summary>
public static class ApplicationServiceCollectionExtensions
{
	/// <summary>
	///		Registers all application services, handlers, validators, and pipeline behaviors.
	/// </summary>
	/// <param name="services">
	///		The service collection to configure.
	/// </param>
	/// <returns>
	///		The same <see cref="IServiceCollection"/> instance for chaining.
	/// </returns>
	public static IServiceCollection AddApplicationServices(this IServiceCollection services)
	{
		services.AddScoped<IDispatcher, Dispatcher>();
		services.AddScoped(
			typeof(IPipelineBehavior<,>),
			typeof(ValidationBehavior<,>));

		// Validators
		services.AddScoped<IValidator<CreateDungeonCommand>, CreateDungeonCommandValidator>();
		services.AddScoped<IValidator<UpdateDungeonCommand>, UpdateDungeonCommandValidator>();
		services.AddScoped<IValidator<AddAreaCommand>, AddAreaCommandValidator>();
		services.AddScoped<IValidator<UpdateAreaCommand>, UpdateAreaCommandValidator>();

		// Query handlers
		services.AddScoped<
			IQueryHandler<GetDungeonsQuery, IReadOnlyList<DungeonSummaryDto>>,
			GetDungeonsQueryHandler>();
		services.AddScoped<
			IQueryHandler<GetDungeonDetailQuery, DungeonDetailDto?>,
			GetDungeonDetailQueryHandler>();

		// Command handlers
		services.AddScoped<
			ICommandHandler<CreateDungeonCommand, Guid>,
			CreateDungeonCommandHandler>();
		services.AddScoped<
			ICommandHandler<DeleteDungeonCommand, bool>,
			DeleteDungeonCommandHandler>();
		services.AddScoped<
			ICommandHandler<UpdateDungeonCommand, bool>,
			UpdateDungeonCommandHandler>();
		services.AddScoped<
			ICommandHandler<AddFloorCommand, Guid>,
			AddFloorCommandHandler>();
		services.AddScoped<
			ICommandHandler<RemoveFloorCommand, bool>,
			RemoveFloorCommandHandler>();
		services.AddScoped<
			ICommandHandler<ReorderFloorCommand, bool>,
			ReorderFloorCommandHandler>();
		services.AddScoped<
			ICommandHandler<UpdateFloorCommand, bool>,
			UpdateFloorCommandHandler>();
		services.AddScoped<
			ICommandHandler<AddAreaCommand, Guid>,
			AddAreaCommandHandler>();
		services.AddScoped<
			ICommandHandler<RemoveAreaCommand, bool>,
			RemoveAreaCommandHandler>();
		services.AddScoped<
			ICommandHandler<UpdateAreaCommand, bool>,
			UpdateAreaCommandHandler>();
		services.AddScoped<
			ICommandHandler<UpdateAreaOutlineCommand, bool>,
			UpdateAreaOutlineCommandHandler>();
		services.AddScoped<
			ICommandHandler<AddPlacedObjectCommand, Guid>,
			AddPlacedObjectCommandHandler>();
		services.AddScoped<
			ICommandHandler<RemovePlacedObjectCommand, bool>,
			RemovePlacedObjectCommandHandler>();
		services.AddScoped<
			ICommandHandler<UpdatePlacedObjectCommand, bool>,
			UpdatePlacedObjectCommandHandler>();

		return services;
	}
}
