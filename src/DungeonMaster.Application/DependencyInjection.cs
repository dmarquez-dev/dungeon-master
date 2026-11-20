using System.Reflection;
using DungeonMaster.Application.Common.Behaviors;
using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Application.Common.Results;
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
	///		Handlers and validators are discovered by scanning this assembly rather than listed by hand, so a new command cannot ship with its registration forgotten.
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

		foreach (var implementation in ScannedTypes)
		{
			foreach (var contract in implementation.GetInterfaces())
			{
				if (!contract.IsGenericType)
				{
					continue;
				}

				var definition = contract.GetGenericTypeDefinition();

				if (definition == typeof(ICommandHandler<>))
				{
					var commandType = contract.GetGenericArguments()[0];

					services.AddScoped(
						contract,
						implementation);

					// Closed per command, never as an open generic, so that no query can ever
					// acquire a validation behavior. See the contract, section 4.2.
					services.AddScoped(
						typeof(IPipelineBehavior<,>).MakeGenericType(
							commandType,
							typeof(Result)),
						typeof(ValidationBehavior<>).MakeGenericType(commandType));
				}
				else if (definition == typeof(ICommandHandler<,>))
				{
					var arguments = contract.GetGenericArguments();

					services.AddScoped(
						contract,
						implementation);

					services.AddScoped(
						typeof(IPipelineBehavior<,>).MakeGenericType(
							arguments[0],
							typeof(Result<>).MakeGenericType(arguments[1])),
						typeof(ValidationBehavior<,>).MakeGenericType(
							arguments[0],
							arguments[1]));
				}
				else if (definition == typeof(IQueryHandler<,>) || definition == typeof(IValidator<>))
				{
					services.AddScoped(
						contract,
						implementation);
				}
			}
		}

		return services;
	}

	/// <summary>
	///		Asserts that every command and query type in the Application assembly implements exactly one request marker and has exactly one registered handler.
	/// </summary>
	/// <param name="serviceProvider">
	///		A built provider whose registrations are to be checked.
	/// </param>
	/// <exception cref="InvalidOperationException">
	///		Thrown when any request type implements more than one of <c>ICommand</c>, <c>ICommand&lt;T&gt;</c> and <c>IQuery&lt;T&gt;</c>, or has no handler, or has more than one. The message names every offending type.
	/// </exception>
	public static void ValidateHandlerRegistrations(IServiceProvider serviceProvider)
	{
		ArgumentNullException.ThrowIfNull(serviceProvider);

		var problems = new List<string>();

		foreach (var requestType in ScannedTypes)
		{
			var markers = MarkersImplementedBy(requestType);

			if (markers.Count == 0)
			{
				continue;
			}

			// Check one: exactly one marker. This is the invariant that licences the single
			// invoker cache in Dispatcher, whose cast is otherwise unproven.
			var markerViolation = MarkerViolationFor(requestType);

			if (markerViolation is not null)
			{
				problems.Add(markerViolation);

				continue;
			}

			// Check two: exactly one registered handler, across all three handler shapes.
			var handlerType = HandlerTypeFor(requestType);
			var registered = serviceProvider.GetServices(handlerType).Count();

			if (registered != 1)
			{
				problems.Add(
					$"{requestType.Name} has {registered} registered handlers of type {handlerType.Name}; exactly one is required.");
			}
		}

		if (problems.Count > 0)
		{
			throw new InvalidOperationException(
				$"Handler registration validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
		}
	}

	private static IReadOnlyList<Type> ScannedTypes =>
		Assembly
			.GetAssembly(typeof(ApplicationServiceCollectionExtensions))!
			.GetTypes()
			.Where(type =>
				type is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false })
			.ToList();

	/// <summary>
	///		Returns a message naming the type when it implements more than one request marker, or <see langword="null"/> when it implements zero or one. Exposed for the test that proves this branch of the self-check fires.
	/// </summary>
	internal static string? MarkerViolationFor(Type requestType)
	{
		var markers = MarkersImplementedBy(requestType);

		return markers.Count > 1
			? $"{requestType.Name} implements {markers.Count} request markers ({string.Join(", ", markers.Select(marker => marker.Name))}); exactly one is required."
			: null;
	}

	private static IReadOnlyList<Type> MarkersImplementedBy(Type type)
	{
		var markers = new List<Type>();

		if (typeof(ICommand).IsAssignableFrom(type))
		{
			markers.Add(typeof(ICommand));
		}

		markers.AddRange(
			type
				.GetInterfaces()
				.Where(contract =>
					contract.IsGenericType
					&& (contract.GetGenericTypeDefinition() == typeof(ICommand<>)
						|| contract.GetGenericTypeDefinition() == typeof(IQuery<>))));

		return markers;
	}

	private static Type HandlerTypeFor(Type requestType)
	{
		var marker = MarkersImplementedBy(requestType)[0];

		if (marker == typeof(ICommand))
		{
			return typeof(ICommandHandler<>).MakeGenericType(requestType);
		}

		var payloadType = marker.GetGenericArguments()[0];

		return marker.GetGenericTypeDefinition() == typeof(ICommand<>)
			? typeof(ICommandHandler<,>).MakeGenericType(
				requestType,
				payloadType)
			: typeof(IQueryHandler<,>).MakeGenericType(
				requestType,
				payloadType);
	}
}
