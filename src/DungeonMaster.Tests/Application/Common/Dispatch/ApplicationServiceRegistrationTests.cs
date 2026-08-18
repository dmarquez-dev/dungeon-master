using System.Reflection;
using DungeonMaster.Application;
using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Application.Common.Results;
using DungeonMaster.Application.Features.Dungeons;
using DungeonMaster.Application.Features.Dungeons.Commands.UpdateArea;
using DungeonMaster.Application.Features.Dungeons.Commands.UpdateDungeon;
using DungeonMaster.Application.Features.Dungeons.Queries.GetDungeons;
using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace DungeonMaster.Tests.Application.Common.Dispatch;

/// <summary>
///		Tests for the assembly-scan registration in <see cref="ApplicationServiceCollectionExtensions"/>, its startup self-check, and the validation behavior it attaches to both command arities.
/// </summary>
public class ApplicationServiceRegistrationTests
{
	private static readonly Assembly ApplicationAssembly =
		typeof(ApplicationServiceCollectionExtensions).Assembly;

	/// <summary>
	///		Verifies that the self-check passes against the container the application actually builds.
	/// </summary>
	[Fact]
	public void ValidateHandlerRegistrations_OnTheRealContainer_DoesNotThrow()
	{
		// Arrange
		using var provider = BuildRealContainer();

		// Act
		var act = () =>
			ApplicationServiceCollectionExtensions.ValidateHandlerRegistrations(provider);

		// Assert
		act.Should().NotThrow();
	}

	/// <summary>
	///		Verifies that the self-check detects a forgotten handler registration and names the command it belongs to.
	/// </summary>
	[Fact]
	public void ValidateHandlerRegistrations_WithAMissingHandler_ThrowsNamingTheCommand()
	{
		// Arrange
		var services = BuildRealServiceCollection();

		var descriptor = services.Single(candidate =>
			candidate.ServiceType == typeof(ICommandHandler<UpdateAreaCommand>));

		services.Remove(descriptor);

		using var provider = services.BuildServiceProvider();

		// Act
		var act = () =>
			ApplicationServiceCollectionExtensions.ValidateHandlerRegistrations(provider);

		// Assert
		act
			.Should()
			.Throw<InvalidOperationException>()
			.WithMessage($"*{nameof(UpdateAreaCommand)}*");
	}

	/// <summary>
	///		Verifies that the marker branch of the self-check fires, naming a type that implements two request markers. Together with the missing-handler test this shows both of the self-check's two checks firing.
	/// </summary>
	[Fact]
	public void MarkerViolationFor_WithATypeImplementingTwoMarkers_NamesThatType()
	{
		// Act
		var violation = ApplicationServiceCollectionExtensions.MarkerViolationFor(
			typeof(TwoMarkerRequest));

		// Assert
		violation.Should().NotBeNull();
		violation.Should().Contain(nameof(TwoMarkerRequest));
	}

	/// <summary>
	///		Verifies that the marker branch stays silent for a type carrying exactly one marker.
	/// </summary>
	[Fact]
	public void MarkerViolationFor_WithASingleMarkerType_ReturnsNull()
	{
		// Act
		var violation = ApplicationServiceCollectionExtensions.MarkerViolationFor(
			typeof(UpdateAreaCommand));

		// Assert
		violation.Should().BeNull();
	}

	/// <summary>
	///		Verifies that no request type implements more than one marker. This is the invariant that licences the single invoker cache keyed on the concrete request type.
	/// </summary>
	[Fact]
	public void EveryRequestType_ImplementsExactlyOneMarker()
	{
		// Arrange
		var offenders = ApplicationAssembly
			.GetTypes()
			.Where(type =>
				type is { IsClass: true, IsAbstract: false })
			.Select(type =>
				new
				{
					Type = type,
					MarkerCount = MarkerCountOf(type),
				})
			.Where(candidate =>
				candidate.MarkerCount > 1)
			.Select(candidate =>
				candidate.Type.Name)
			.ToList();

		// Assert
		offenders.Should().BeEmpty();
	}

	/// <summary>
	///		Verifies that every command in the assembly, across both arities, resolves a handler. The expected set is computed from the assembly, so a new command with no handler cannot pass.
	/// </summary>
	[Fact]
	public void AddApplicationServices_RegistersAHandlerForEveryCommand()
	{
		// Arrange
		using var provider = BuildRealContainer();
		using var scope = provider.CreateScope();

		var noPayloadCommands = NoPayloadCommands();
		var valueCommands = ValueProducingCommands();

		// Act
		var resolvedNoPayload = noPayloadCommands
			.Where(commandType =>
				scope.ServiceProvider.GetService(
					typeof(ICommandHandler<>).MakeGenericType(commandType)) is not null)
			.ToList();

		var resolvedValue = valueCommands
			.Where(entry =>
				scope.ServiceProvider.GetService(
					typeof(ICommandHandler<,>).MakeGenericType(
						entry.CommandType,
						entry.ResultType)) is not null)
			.ToList();

		// Assert
		noPayloadCommands.Should().NotBeEmpty();
		valueCommands.Should().NotBeEmpty();

		resolvedNoPayload.Should().BeEquivalentTo(noPayloadCommands);
		resolvedValue.Should().BeEquivalentTo(valueCommands);

		(resolvedNoPayload.Count + resolvedValue.Count)
			.Should()
			.Be(noPayloadCommands.Count + valueCommands.Count);
	}

	/// <summary>
	///		Verifies that every query in the assembly resolves a handler, with the expected set computed from the assembly.
	/// </summary>
	[Fact]
	public void AddApplicationServices_RegistersAHandlerForEveryQuery()
	{
		// Arrange
		using var provider = BuildRealContainer();
		using var scope = provider.CreateScope();

		var queries = Queries();

		// Act
		var resolved = queries
			.Where(entry =>
				scope.ServiceProvider.GetService(
					typeof(IQueryHandler<,>).MakeGenericType(
						entry.CommandType,
						entry.ResultType)) is not null)
			.ToList();

		// Assert
		queries.Should().NotBeEmpty();
		resolved.Should().BeEquivalentTo(queries);
	}

	/// <summary>
	///		Verifies that a query acquires no validation behavior, because the behavior is registered closed per command and never as an open generic.
	/// </summary>
	[Fact]
	public void AddApplicationServices_RegistersNoValidationBehaviourForAQuery()
	{
		// Arrange
		using var provider = BuildRealContainer();
		using var scope = provider.CreateScope();

		// Act
		var act = () =>
			scope.ServiceProvider
				.GetServices<IPipelineBehavior<GetDungeonsQuery, IReadOnlyList<DungeonSummaryDto>>>()
				.ToList();

		// Assert
		act.Should().NotThrow();
		act().Should().BeEmpty();
	}

	/// <summary>
	///		Verifies that exactly one validation behavior is registered for every command, on both arities, with the expected set computed from the assembly.
	/// </summary>
	[Fact]
	public void AddApplicationServices_RegistersOneValidationBehaviourForEveryCommand()
	{
		// Arrange
		using var provider = BuildRealContainer();
		using var scope = provider.CreateScope();

		var noPayloadCommands = NoPayloadCommands();
		var valueCommands = ValueProducingCommands();

		// Act
		var noPayloadCounts = noPayloadCommands.ToDictionary(
			commandType =>
				commandType.Name,
			commandType =>
				BehaviourCount(
					scope.ServiceProvider,
					commandType,
					typeof(Result)));

		var valueCounts = valueCommands.ToDictionary(
			entry =>
				entry.CommandType.Name,
			entry =>
				BehaviourCount(
					scope.ServiceProvider,
					entry.CommandType,
					typeof(Result<>).MakeGenericType(entry.ResultType)));

		// Assert
		noPayloadCounts.Should().OnlyContain(entry =>
			entry.Value == 1);

		valueCounts.Should().OnlyContain(entry =>
			entry.Value == 1);

		(noPayloadCounts.Count + valueCounts.Count)
			.Should()
			.Be(noPayloadCommands.Count + valueCommands.Count);
	}

	/// <summary>
	///		Verifies that a no-payload command carries a validation behavior on the dispatch path, so that its registered validator rejects an invalid command before the handler runs.
	/// </summary>
	[Fact]
	public async Task SendAsync_WithAnInvalidUpdateAreaCommand_IsRejectedByItsRegisteredValidator()
	{
		// Arrange
		var writeRepository = Substitute.For<IDungeonWriteRepository>();
		using var provider = BuildRealContainer(writeRepository);
		using var scope = provider.CreateScope();

		var command = new UpdateAreaCommand(
			Guid.NewGuid(),
			Guid.NewGuid(),
			Guid.NewGuid(),
			string.Empty,
			null,
			null);

		// Act
		var result = await scope.ServiceProvider
			.GetRequiredService<IDispatcher>()
			.SendAsync(command);

		// Assert
		scope.ServiceProvider
			.GetServices<IValidator<UpdateAreaCommand>>()
			.Should()
			.ContainSingle()
			.Which
			.Should()
			.BeOfType<UpdateAreaCommandValidator>();

		result.IsFailure.Should().BeTrue();
		result.ErrorCategory.Should().Be(ErrorCategory.Validation);
		result.ValidationErrors
			.Should()
			.ContainSingle()
			.Which
			.PropertyName
			.Should()
			.Be(nameof(UpdateAreaCommand.Name));

		await writeRepository
			.DidNotReceiveWithAnyArgs()
			.LoadAsync(
				default,
				default);
	}

	/// <summary>
	///		Verifies that the second of the two validators bypassed before this change also runs on the dispatch path.
	/// </summary>
	[Fact]
	public async Task SendAsync_WithAnInvalidUpdateDungeonCommand_IsRejectedByItsRegisteredValidator()
	{
		// Arrange
		var writeRepository = Substitute.For<IDungeonWriteRepository>();
		using var provider = BuildRealContainer(writeRepository);
		using var scope = provider.CreateScope();

		var command = new UpdateDungeonCommand(
			Guid.NewGuid(),
			string.Empty,
			null,
			null);

		// Act
		var result = await scope.ServiceProvider
			.GetRequiredService<IDispatcher>()
			.SendAsync(command);

		// Assert
		scope.ServiceProvider
			.GetServices<IValidator<UpdateDungeonCommand>>()
			.Should()
			.ContainSingle()
			.Which
			.Should()
			.BeOfType<UpdateDungeonCommandValidator>();

		result.IsFailure.Should().BeTrue();
		result.ErrorCategory.Should().Be(ErrorCategory.Validation);
		result.ValidationErrors
			.Should()
			.ContainSingle()
			.Which
			.PropertyName
			.Should()
			.Be(nameof(UpdateDungeonCommand.Name));

		await writeRepository
			.DidNotReceiveWithAnyArgs()
			.LoadAsync(
				default,
				default);
	}

	private static ServiceProvider BuildRealContainer(
		IDungeonWriteRepository? writeRepository = null) =>
			BuildRealServiceCollection(writeRepository).BuildServiceProvider();

	private static IServiceCollection BuildRealServiceCollection(
		IDungeonWriteRepository? writeRepository = null)
	{
		var services = new ServiceCollection();

		services.AddApplicationServices();
		services.AddScoped(_ =>
			Substitute.For<IDungeonReadRepository>());
		services.AddScoped(_ =>
			writeRepository ?? Substitute.For<IDungeonWriteRepository>());

		return services;
	}

	private static int BehaviourCount(
		IServiceProvider serviceProvider,
		Type requestType,
		Type responseType)
	{
		var behaviourType = typeof(IPipelineBehavior<,>).MakeGenericType(
			requestType,
			responseType);

		var enumerableType = typeof(IEnumerable<>).MakeGenericType(behaviourType);

		return ((System.Collections.IEnumerable)serviceProvider.GetRequiredService(enumerableType))
			.Cast<object>()
			.Count();
	}

	private static int MarkerCountOf(Type type)
	{
		var count = typeof(ICommand).IsAssignableFrom(type)
			? 1
			: 0;

		count += type
			.GetInterfaces()
			.Count(contract =>
				contract.IsGenericType
				&& (contract.GetGenericTypeDefinition() == typeof(ICommand<>)
					|| contract.GetGenericTypeDefinition() == typeof(IQuery<>)));

		return count;
	}

	private static IReadOnlyList<Type> NoPayloadCommands() =>
		ApplicationAssembly
			.GetTypes()
			.Where(type =>
				type is { IsClass: true, IsAbstract: false }
				&& typeof(ICommand).IsAssignableFrom(type))
			.ToList();

	private static IReadOnlyList<RequestEntry> ValueProducingCommands() =>
		ClosedRequestsOf(typeof(ICommand<>));

	private static IReadOnlyList<RequestEntry> Queries() =>
		ClosedRequestsOf(typeof(IQuery<>));

	private static IReadOnlyList<RequestEntry> ClosedRequestsOf(Type marker) =>
		ApplicationAssembly
			.GetTypes()
			.Where(type =>
				type is { IsClass: true, IsAbstract: false })
			.SelectMany(type =>
				type
					.GetInterfaces()
					.Where(contract =>
						contract.IsGenericType
						&& contract.GetGenericTypeDefinition() == marker)
					.Select(contract =>
						new RequestEntry(
							type,
							contract.GetGenericArguments()[0])))
			.ToList();

	private sealed record RequestEntry(
		Type CommandType,
		Type ResultType);

	/// <summary>
	///		A deliberately malformed request that implements two markers at once, used to prove the marker branch of the self-check fires. It lives in the test assembly, which the self-check does not scan, so it cannot break the real container.
	/// </summary>
	public sealed record TwoMarkerRequest
		: ICommand, IQuery<int>;
}
