using System.Collections;
using System.Reflection;
using DungeonMaster.Application;
using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Application.Common.Interfaces;
using DungeonMaster.Application.Common.Results;
using DungeonMaster.Application.Features.Dungeons.Commands.AddPlacedObject;
using DungeonMaster.Application.Features.Dungeons.Commands.ReorderFloor;
using DungeonMaster.Application.Features.Dungeons.Commands.UpdateAreaOutline;
using DungeonMaster.Application.Features.Dungeons.Commands.UpdatePlacedObject;
using DungeonMaster.Domain.Entities.Floor;
using DungeonMaster.Domain.ValueObjects;
using DungeonMaster.Tests.Application.Common.Handlers;
using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Validators;

/// <summary>
///		Cross-cutting coverage test proving that every command in the Application assembly has at least one validator registered in the real container. The command set is discovered by reflection rather than listed, so a command added later fails this test until it gets a validator.
/// </summary>
public class CommandValidatorCoverageTests
{
	private static readonly Assembly ApplicationAssembly =
		typeof(ApplicationServiceCollectionExtensions).Assembly;

	[Fact]
	public void EveryCommand_HasARegisteredValidator()
	{
		// Arrange
		var commandTypes = CommandTypes();

		using var provider = BuildRealContainer();

		// Act
		var withoutValidator = commandTypes
			.Where(commandType =>
				RegisteredValidatorCount(
					provider,
					commandType) == 0)
			.Select(commandType =>
				commandType.Name)
			.ToList();

		// Assert
		commandTypes.Should().NotBeEmpty();
		withoutValidator.Should().BeEmpty();
	}

	[Fact]
	public void CommandDiscovery_CoversBothCommandArities()
	{
		// Arrange
		var commandTypes = CommandTypes();

		// Act
		var noPayload = commandTypes
			.Where(IsNoPayloadCommand)
			.ToList();

		var valueProducing = commandTypes
			.Where(IsValueProducingCommand)
			.ToList();

		// Assert
		noPayload.Should().NotBeEmpty();
		valueProducing.Should().NotBeEmpty();
		(noPayload.Count + valueProducing.Count).Should().Be(commandTypes.Count);
	}

	/// <summary>
	///		Proves that the non-generic <see cref="ICommand"/> dispatch path actually runs the registered validator: a floor number below 1 comes back as a validation failure from the pipeline, and the handler never loads the aggregate.
	/// </summary>
	[Fact]
	public async Task SendAsync_WithAnInvalidReorderFloorCommand_IsRejectedByThePipeline()
	{
		// Arrange
		var unitOfWork = new UnitOfWorkSubstitute();
		using var provider = BuildRealContainer(unitOfWork);
		using var scope = provider.CreateScope();

		var command = new ReorderFloorCommand(
			Guid.NewGuid(),
			Guid.NewGuid(),
			0);

		// Act
		var result = await scope.ServiceProvider
			.GetRequiredService<IDispatcher>()
			.SendAsync(command);

		// Assert
		result.IsFailure.Should().BeTrue();
		result.ErrorCategory.Should().Be(ErrorCategory.Validation);
		result.ValidationErrors
			.Should()
			.Contain(error =>
				error.PropertyName == nameof(ReorderFloorCommand.NewNumber));

		await unitOfWork.Dungeons
			.DidNotReceiveWithAnyArgs()
			.LoadAsync(
				default,
				default);
	}

	/// <summary>
	///		Proves the same for the second non-generic command guarded in this session: an empty outline is rejected on the dispatch path rather than by the aggregate.
	/// </summary>
	[Fact]
	public async Task SendAsync_WithAnEmptyUpdateAreaOutlineCommand_IsRejectedByThePipeline()
	{
		// Arrange
		using var provider = BuildRealContainer();
		using var scope = provider.CreateScope();

		var command = new UpdateAreaOutlineCommand(
			Guid.NewGuid(),
			Guid.NewGuid(),
			Guid.NewGuid(),
			Array.Empty<PathSegment>());

		// Act
		var result = await scope.ServiceProvider
			.GetRequiredService<IDispatcher>()
			.SendAsync(command);

		// Assert
		result.IsFailure.Should().BeTrue();
		result.ErrorCategory.Should().Be(ErrorCategory.Validation);
		result.ValidationErrors
			.Should()
			.Contain(error =>
				error.PropertyName == nameof(UpdateAreaOutlineCommand.Outline));
	}

	/// <summary>
	///		Proves the value-producing <see cref="ICommand{TValue}"/> dispatch path runs its validator too: a scale above <see cref="ObjectScale.Max"/> is rejected before the handler.
	/// </summary>
	[Fact]
	public async Task SendAsync_WithAnOutOfRangeAddPlacedObjectScale_IsRejectedByThePipeline()
	{
		// Arrange
		using var provider = BuildRealContainer();
		using var scope = provider.CreateScope();

		var command = new AddPlacedObjectCommand(
			Guid.NewGuid(),
			Guid.NewGuid(),
			ObjectKind.DoorWood,
			new CanvasPoint(0f, 0f),
			ObjectRotation.None,
			ObjectScale.Max + 1f);

		// Act
		var result = await scope.ServiceProvider
			.GetRequiredService<IDispatcher>()
			.SendAsync(command);

		// Assert
		result.IsFailure.Should().BeTrue();
		result.ErrorCategory.Should().Be(ErrorCategory.Validation);
		result.ValidationErrors
			.Should()
			.Contain(error =>
				error.PropertyName == nameof(AddPlacedObjectCommand.Scale));
	}

	/// <summary>
	///		Proves the update counterpart is bounded on the dispatch path: a scale below <see cref="ObjectScale.Min"/> is rejected before the handler.
	/// </summary>
	[Fact]
	public async Task SendAsync_WithAnOutOfRangeUpdatePlacedObjectScale_IsRejectedByThePipeline()
	{
		// Arrange
		using var provider = BuildRealContainer();
		using var scope = provider.CreateScope();

		var command = new UpdatePlacedObjectCommand(
			Guid.NewGuid(),
			Guid.NewGuid(),
			Guid.NewGuid(),
			new CanvasPoint(0f, 0f),
			ObjectRotation.None,
			ObjectScale.Min - 1f);

		// Act
		var result = await scope.ServiceProvider
			.GetRequiredService<IDispatcher>()
			.SendAsync(command);

		// Assert
		result.IsFailure.Should().BeTrue();
		result.ErrorCategory.Should().Be(ErrorCategory.Validation);
		result.ValidationErrors
			.Should()
			.Contain(error =>
				error.PropertyName == nameof(UpdatePlacedObjectCommand.NewScale));
	}

	private static IReadOnlyList<Type> CommandTypes() =>
		ApplicationAssembly
			.GetTypes()
			.Where(type =>
				type is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false })
			.Where(type =>
				IsNoPayloadCommand(type) || IsValueProducingCommand(type))
			.ToList();

	private static bool IsNoPayloadCommand(Type type) =>
		typeof(ICommand).IsAssignableFrom(type);

	private static bool IsValueProducingCommand(Type type) =>
		type
			.GetInterfaces()
			.Any(contract =>
				contract.IsGenericType
				&& contract.GetGenericTypeDefinition() == typeof(ICommand<>));

	private static int RegisteredValidatorCount(
		IServiceProvider serviceProvider,
		Type commandType)
	{
		var validatorType = typeof(IValidator<>).MakeGenericType(commandType);
		var enumerableType = typeof(IEnumerable<>).MakeGenericType(validatorType);

		return ((IEnumerable)serviceProvider.GetRequiredService(enumerableType))
			.Cast<object>()
			.Count();
	}

	private static ServiceProvider BuildRealContainer(
		UnitOfWorkSubstitute? unitOfWork = null)
	{
		var services = new ServiceCollection();
		var unitOfWorkSubstitute = unitOfWork ?? new UnitOfWorkSubstitute();

		services.AddApplicationServices();
		services.AddScoped(_ =>
			Substitute.For<IDungeonReadRepository>());
		services.AddScoped(_ =>
			unitOfWorkSubstitute.Factory);

		return services.BuildServiceProvider();
	}
}
