using DungeonMaster.Application.Common.Dispatch;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace DungeonMaster.Tests.Application.Common.Dispatch;

/// <summary>
///		Unit tests for the failure mode <see cref="Dispatcher"/> exhibits when no handler is registered for a request, on all three request paths.
///		The request types are declared here rather than in the Application assembly, because the startup self-check at <c>ApplicationServiceCollectionExtensions.ValidateHandlerRegistrations</c> scans only that assembly and would otherwise reject the gap before a dispatch could reach it.
/// </summary>
public class DispatcherMissingHandlerTests
{
	/// <summary>
	///		Verifies that dispatching a no-payload command with no registered handler throws an <see cref="InvalidOperationException"/> whose message names the handler service that was missing.
	/// </summary>
	[Fact]
	public async Task SendAsync_WithNoRegisteredCommandHandler_ThrowsInvalidOperationExceptionNamingTheHandlerService()
	{
		// Arrange
		var services = new ServiceCollection();

		services.AddScoped<IDispatcher, Dispatcher>();

		using var provider = services.BuildServiceProvider();
		var dispatcher = provider.GetRequiredService<IDispatcher>();

		// Act
		var act = async () =>
			await dispatcher.SendAsync(new UnhandledCommand());

		// Assert
		await act
			.Should()
			.ThrowAsync<InvalidOperationException>()
			.WithMessage("*ICommandHandler*");
	}

	/// <summary>
	///		Verifies that dispatching a value-producing command with no registered handler throws an <see cref="InvalidOperationException"/> whose message names the handler service that was missing.
	/// </summary>
	[Fact]
	public async Task SendAsyncOfT_WithNoRegisteredCommandHandler_ThrowsInvalidOperationExceptionNamingTheHandlerService()
	{
		// Arrange
		var services = new ServiceCollection();

		services.AddScoped<IDispatcher, Dispatcher>();

		using var provider = services.BuildServiceProvider();
		var dispatcher = provider.GetRequiredService<IDispatcher>();

		// Act
		var act = async () =>
			await dispatcher.SendAsync(new UnhandledValueCommand());

		// Assert
		await act
			.Should()
			.ThrowAsync<InvalidOperationException>()
			.WithMessage("*ICommandHandler*");
	}

	/// <summary>
	///		Verifies that dispatching a query with no registered handler throws an <see cref="InvalidOperationException"/> whose message names the handler service that was missing.
	/// </summary>
	[Fact]
	public async Task QueryAsync_WithNoRegisteredQueryHandler_ThrowsInvalidOperationExceptionNamingTheHandlerService()
	{
		// Arrange
		var services = new ServiceCollection();

		services.AddScoped<IDispatcher, Dispatcher>();

		using var provider = services.BuildServiceProvider();
		var dispatcher = provider.GetRequiredService<IDispatcher>();

		// Act
		var act = async () =>
			await dispatcher.QueryAsync(new UnhandledQuery());

		// Assert
		await act
			.Should()
			.ThrowAsync<InvalidOperationException>()
			.WithMessage("*IQueryHandler*");
	}

	/// <summary>
	///		A no-payload command deliberately left without a handler registration.
	/// </summary>
	public sealed record UnhandledCommand
		: ICommand;

	/// <summary>
	///		A value-producing command deliberately left without a handler registration.
	/// </summary>
	public sealed record UnhandledValueCommand
		: ICommand<int>;

	/// <summary>
	///		A query deliberately left without a handler registration.
	/// </summary>
	public sealed record UnhandledQuery
		: IQuery<string>;
}
