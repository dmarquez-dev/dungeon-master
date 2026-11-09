using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Application.Common.Results;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace DungeonMaster.Tests.Application.Common.Dispatch;

/// <summary>
///		Unit tests for <see cref="Dispatcher"/>: behavior ordering across all three request paths, and the single memoised invoker cache.
/// </summary>
public class DispatcherTests
{
	/// <summary>
	///		Verifies that two behaviors registered against a no-payload command run first-registered outermost.
	/// </summary>
	[Fact]
	public async Task SendAsync_WithTwoBehaviours_RunsThemFirstRegisteredOutermost()
	{
		// Arrange
		var log = new List<string>();
		var services = new ServiceCollection();

		services.AddScoped<IDispatcher, Dispatcher>();
		services.AddScoped<ICommandHandler<OrderingCommand>>(_ =>
			new OrderingCommandHandler(log));
		services.AddScoped<IPipelineBehavior<OrderingCommand, Result>>(_ =>
			new RecordingBehavior<OrderingCommand, Result>(
				"first",
				log));
		services.AddScoped<IPipelineBehavior<OrderingCommand, Result>>(_ =>
			new RecordingBehavior<OrderingCommand, Result>(
				"second",
				log));

		using var provider = services.BuildServiceProvider();

		// Act
		var result = await provider
			.GetRequiredService<IDispatcher>()
			.SendAsync(new OrderingCommand());

		// Assert
		result.IsSuccess.Should().BeTrue();
		log.Should().Equal(
			"first:enter",
			"second:enter",
			"handler",
			"second:exit",
			"first:exit");
	}

	/// <summary>
	///		Verifies that the same two behaviors registered against a value-producing command run in the same order as they do on the no-payload path.
	/// </summary>
	[Fact]
	public async Task SendAsyncOfT_WithTwoBehaviours_RunsThemInTheSameOrder()
	{
		// Arrange
		var log = new List<string>();
		var services = new ServiceCollection();

		services.AddScoped<IDispatcher, Dispatcher>();
		services.AddScoped<ICommandHandler<OrderingValueCommand, int>>(_ =>
			new OrderingValueCommandHandler(log));
		services.AddScoped<IPipelineBehavior<OrderingValueCommand, Result<int>>>(_ =>
			new RecordingBehavior<OrderingValueCommand, Result<int>>(
				"first",
				log));
		services.AddScoped<IPipelineBehavior<OrderingValueCommand, Result<int>>>(_ =>
			new RecordingBehavior<OrderingValueCommand, Result<int>>(
				"second",
				log));

		using var provider = services.BuildServiceProvider();

		// Act
		var result = await provider
			.GetRequiredService<IDispatcher>()
			.SendAsync(new OrderingValueCommand());

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().Be(7);
		log.Should().Equal(
			"first:enter",
			"second:enter",
			"handler",
			"second:exit",
			"first:exit");
	}

	/// <summary>
	///		Verifies that a behavior registered against a query runs around the query handler. This test cannot be written before queries gained a pipeline.
	/// </summary>
	[Fact]
	public async Task QueryAsync_WithABehaviour_RunsItAroundTheHandler()
	{
		// Arrange
		var log = new List<string>();
		var services = new ServiceCollection();

		services.AddScoped<IDispatcher, Dispatcher>();
		services.AddScoped<IQueryHandler<OrderingQuery, string>>(_ =>
			new OrderingQueryHandler(log));
		services.AddScoped<IPipelineBehavior<OrderingQuery, string>>(_ =>
			new RecordingBehavior<OrderingQuery, string>(
				"only",
				log));

		using var provider = services.BuildServiceProvider();

		// Act
		var result = await provider
			.GetRequiredService<IDispatcher>()
			.QueryAsync(new OrderingQuery());

		// Assert
		result.Should().Be("queried");
		log.Should().Equal(
			"only:enter",
			"handler",
			"only:exit");
	}

	/// <summary>
	///		Verifies that a query with no behavior registered still returns the handler's result unchanged.
	/// </summary>
	[Fact]
	public async Task QueryAsync_WithNoBehaviour_ReturnsTheHandlerResult()
	{
		// Arrange
		var log = new List<string>();
		var services = new ServiceCollection();

		services.AddScoped<IDispatcher, Dispatcher>();
		services.AddScoped<IQueryHandler<PlainQuery, string>>(_ =>
			new PlainQueryHandler(log));

		using var provider = services.BuildServiceProvider();

		// Act
		var result = await provider
			.GetRequiredService<IDispatcher>()
			.QueryAsync(new PlainQuery());

		// Assert
		result.Should().Be("queried");
		log.Should().Equal("handler");
	}

	/// <summary>
	///		Verifies that reflection runs once per request type: the invoker cached on the first dispatch is the very same instance served to a later dispatch through a different provider.
	/// </summary>
	[Fact]
	public async Task SendAsync_DispatchedTwice_ReflectsTypeOnlyOnce()
	{
		// Arrange
		using var firstProvider = BuildMemoisationProvider();
		using var secondProvider = BuildMemoisationProvider();

		// Act
		await firstProvider
			.GetRequiredService<IDispatcher>()
			.SendAsync(new MemoisedCommand());

		var afterFirstDispatch = Dispatcher.InvokerCache[typeof(MemoisedCommand)];

		await secondProvider
			.GetRequiredService<IDispatcher>()
			.SendAsync(new MemoisedCommand());

		var afterSecondDispatch = Dispatcher.InvokerCache[typeof(MemoisedCommand)];

		// Assert
		afterSecondDispatch.Should().BeSameAs(
			afterFirstDispatch,
			"a second reflection pass would have constructed a new invoker instance");
	}

	/// <summary>
	///		Verifies that both command arities share the one invoker dictionary, each keyed on its own concrete command type and each holding an invoker of its own family.
	/// </summary>
	[Fact]
	public async Task SendAsync_WithBothCommandArities_SharesOneInvokerCache()
	{
		// Arrange
		var log = new List<string>();
		var services = new ServiceCollection();

		services.AddScoped<IDispatcher, Dispatcher>();
		services.AddScoped<ICommandHandler<SharedCacheCommand>>(_ =>
			new SharedCacheCommandHandler(log));
		services.AddScoped<ICommandHandler<SharedCacheValueCommand, int>>(_ =>
			new SharedCacheValueCommandHandler(log));

		using var provider = services.BuildServiceProvider();
		var dispatcher = provider.GetRequiredService<IDispatcher>();

		// Act
		await dispatcher.SendAsync(new SharedCacheCommand());
		await dispatcher.SendAsync(new SharedCacheValueCommand());

		// Assert
		Dispatcher.InvokerCache.Should().ContainKey(typeof(SharedCacheCommand));
		Dispatcher.InvokerCache.Should().ContainKey(typeof(SharedCacheValueCommand));

		Dispatcher.InvokerCache[typeof(SharedCacheCommand)]
			.GetType()
			.BaseType!
			.Name
			.Should()
			.Be("CommandInvoker");

		Dispatcher.InvokerCache[typeof(SharedCacheValueCommand)]
			.GetType()
			.BaseType!
			.Name
			.Should()
			.Be("CommandInvoker`1");
	}

	private static ServiceProvider BuildMemoisationProvider()
	{
		var services = new ServiceCollection();

		services.AddScoped<IDispatcher, Dispatcher>();
		services.AddScoped<ICommandHandler<MemoisedCommand>, MemoisedCommandHandler>();

		return services.BuildServiceProvider();
	}

	/// <summary>
	///		A behavior that records its entry and exit around the next stage of the pipeline.
	/// </summary>
	/// <typeparam name="TRequest">
	///		The request type this behavior applies to.
	/// </typeparam>
	/// <typeparam name="TResponse">
	///		The response type the pipeline produces.
	/// </typeparam>
	/// <param name="name">
	///		The name recorded alongside each entry and exit.
	/// </param>
	/// <param name="log">
	///		The shared log the sequence is recorded into.
	/// </param>
	private sealed class RecordingBehavior<TRequest, TResponse>(
		string name,
		List<string> log)
			: IPipelineBehavior<TRequest, TResponse>
				where TRequest : IRequest<TResponse>
	{
		public async Task<TResponse> HandleAsync(
			TRequest request,
			Func<Task<TResponse>> next,
			CancellationToken cancellationToken)
		{
			log.Add($"{name}:enter");

			var response = await next();

			log.Add($"{name}:exit");

			return response;
		}
	}

	/// <summary>
	///		A no-payload command defined solely for the ordering test.
	/// </summary>
	public sealed record OrderingCommand
		: ICommand;

	/// <summary>
	///		A value-producing command defined solely for the ordering test.
	/// </summary>
	public sealed record OrderingValueCommand
		: ICommand<int>;

	/// <summary>
	///		A query defined solely for the query-pipeline test.
	/// </summary>
	public sealed record OrderingQuery
		: IQuery<string>;

	/// <summary>
	///		A query defined solely for the no-behavior query test.
	/// </summary>
	public sealed record PlainQuery
		: IQuery<string>;

	/// <summary>
	///		A no-payload command defined solely for the memoisation test.
	/// </summary>
	public sealed record MemoisedCommand
		: ICommand;

	/// <summary>
	///		A no-payload command defined solely for the shared-cache test.
	/// </summary>
	public sealed record SharedCacheCommand
		: ICommand;

	/// <summary>
	///		A value-producing command defined solely for the shared-cache test.
	/// </summary>
	public sealed record SharedCacheValueCommand
		: ICommand<int>;

	private sealed class OrderingCommandHandler(
		List<string> log)
			: ICommandHandler<OrderingCommand>
	{
		public Task<Result> HandleAsync(
			OrderingCommand command,
			CancellationToken cancellationToken)
		{
			log.Add("handler");

			return Task.FromResult(Result.Success());
		}
	}

	private sealed class OrderingValueCommandHandler(
		List<string> log)
			: ICommandHandler<OrderingValueCommand, int>
	{
		public Task<Result<int>> HandleAsync(
			OrderingValueCommand command,
			CancellationToken cancellationToken)
		{
			log.Add("handler");

			return Task.FromResult(Result<int>.Success(7));
		}
	}

	private sealed class OrderingQueryHandler(
		List<string> log)
			: IQueryHandler<OrderingQuery, string>
	{
		public Task<string> HandleAsync(
			OrderingQuery query,
			CancellationToken cancellationToken)
		{
			log.Add("handler");

			return Task.FromResult("queried");
		}
	}

	private sealed class PlainQueryHandler(
		List<string> log)
			: IQueryHandler<PlainQuery, string>
	{
		public Task<string> HandleAsync(
			PlainQuery query,
			CancellationToken cancellationToken)
		{
			log.Add("handler");

			return Task.FromResult("queried");
		}
	}

	private sealed class MemoisedCommandHandler
		: ICommandHandler<MemoisedCommand>
	{
		public Task<Result> HandleAsync(
			MemoisedCommand command,
			CancellationToken cancellationToken) =>
				Task.FromResult(Result.Success());
	}

	private sealed class SharedCacheCommandHandler(
		List<string> log)
			: ICommandHandler<SharedCacheCommand>
	{
		public Task<Result> HandleAsync(
			SharedCacheCommand command,
			CancellationToken cancellationToken)
		{
			log.Add("handler");

			return Task.FromResult(Result.Success());
		}
	}

	private sealed class SharedCacheValueCommandHandler(
		List<string> log)
			: ICommandHandler<SharedCacheValueCommand, int>
	{
		public Task<Result<int>> HandleAsync(
			SharedCacheValueCommand command,
			CancellationToken cancellationToken)
		{
			log.Add("handler");

			return Task.FromResult(Result<int>.Success(7));
		}
	}
}
