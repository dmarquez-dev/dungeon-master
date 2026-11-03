using DungeonMaster.Application.Common.Dispatch;
using DungeonMaster.Application.Common.Results;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace DungeonMaster.Tests.Application.Common.Dispatch;

/// <summary>
///		Unit tests for the <see cref="CancellationToken"/> <see cref="Dispatcher"/> hands on: the token reaches the handler on all three request paths, and it reaches a registered behavior on all three as well.
///		The handler-only facts register no behavior, so a token lost on the pipeline hand-off cannot mask a token lost on the handler hand-off, nor the reverse.
///		The pipelined handler facts add the handler's own token on each path where a behavior is registered, so a handler token lost only when a behavior wraps the handler is caught as well.
/// </summary>
public class DispatcherCancellationTests
{
	/// <summary>
	///		Verifies that the exact token handed to <see cref="IDispatcher.SendAsync(ICommand, CancellationToken)"/> is the token the command handler receives.
	/// </summary>
	[Fact]
	public async Task SendAsync_WithAToken_PassesThatSameTokenToTheCommandHandler()
	{
		// Arrange
		using var cts = new CancellationTokenSource();
		var token = cts.Token;

		var handler = new RecordingCommandHandler<HandlerTokenCommand>();
		var services = new ServiceCollection();

		services.AddScoped<IDispatcher, Dispatcher>();
		services.AddScoped<ICommandHandler<HandlerTokenCommand>>(_ =>
			handler);

		using var provider = services.BuildServiceProvider();

		// Act
		await provider
			.GetRequiredService<IDispatcher>()
			.SendAsync(
				new HandlerTokenCommand(),
				token);

		// Assert
		token.Should().NotBe(CancellationToken.None);
		handler.WasCalled.Should().BeTrue();
		handler.ReceivedToken.Should().Be(token);
	}

	/// <summary>
	///		Verifies that the exact token handed to <see cref="IDispatcher.SendAsync{TResult}(ICommand{TResult}, CancellationToken)"/> is the token the value-producing command handler receives.
	/// </summary>
	[Fact]
	public async Task SendAsyncOfT_WithAToken_PassesThatSameTokenToTheCommandHandler()
	{
		// Arrange
		using var cts = new CancellationTokenSource();
		var token = cts.Token;

		var handler = new RecordingValueCommandHandler<HandlerTokenValueCommand>();
		var services = new ServiceCollection();

		services.AddScoped<IDispatcher, Dispatcher>();
		services.AddScoped<ICommandHandler<HandlerTokenValueCommand, int>>(_ =>
			handler);

		using var provider = services.BuildServiceProvider();

		// Act
		await provider
			.GetRequiredService<IDispatcher>()
			.SendAsync(
				new HandlerTokenValueCommand(),
				token);

		// Assert
		token.Should().NotBe(CancellationToken.None);
		handler.WasCalled.Should().BeTrue();
		handler.ReceivedToken.Should().Be(token);
	}

	/// <summary>
	///		Verifies that the exact token handed to <see cref="IDispatcher.QueryAsync{TResult}(IQuery{TResult}, CancellationToken)"/> is the token the query handler receives.
	/// </summary>
	[Fact]
	public async Task QueryAsync_WithAToken_PassesThatSameTokenToTheQueryHandler()
	{
		// Arrange
		using var cts = new CancellationTokenSource();
		var token = cts.Token;

		var handler = new RecordingQueryHandler<HandlerTokenQuery>();
		var services = new ServiceCollection();

		services.AddScoped<IDispatcher, Dispatcher>();
		services.AddScoped<IQueryHandler<HandlerTokenQuery, string>>(_ =>
			handler);

		using var provider = services.BuildServiceProvider();

		// Act
		await provider
			.GetRequiredService<IDispatcher>()
			.QueryAsync(
				new HandlerTokenQuery(),
				token);

		// Assert
		token.Should().NotBe(CancellationToken.None);
		handler.WasCalled.Should().BeTrue();
		handler.ReceivedToken.Should().Be(token);
	}

	/// <summary>
	///		Verifies that the exact token handed to <see cref="IDispatcher.SendAsync(ICommand, CancellationToken)"/> is the token the one registered behavior receives.
	/// </summary>
	[Fact]
	public async Task SendAsync_WithAToken_PassesThatSameTokenToTheBehaviour()
	{
		// Arrange
		using var cts = new CancellationTokenSource();
		var token = cts.Token;

		var behavior = new RecordingBehavior<BehaviourTokenCommand, Result>();
		var services = new ServiceCollection();

		services.AddScoped<IDispatcher, Dispatcher>();
		services.AddScoped<ICommandHandler<BehaviourTokenCommand>>(_ =>
			new RecordingCommandHandler<BehaviourTokenCommand>());
		services.AddScoped<IPipelineBehavior<BehaviourTokenCommand, Result>>(_ =>
			behavior);

		using var provider = services.BuildServiceProvider();

		// Act
		await provider
			.GetRequiredService<IDispatcher>()
			.SendAsync(
				new BehaviourTokenCommand(),
				token);

		// Assert
		token.Should().NotBe(CancellationToken.None);
		behavior.WasCalled.Should().BeTrue();
		behavior.ReceivedToken.Should().Be(token);
	}

	/// <summary>
	///		Verifies that the exact token handed to <see cref="IDispatcher.SendAsync{TResult}(ICommand{TResult}, CancellationToken)"/> is the token the one registered behavior receives.
	/// </summary>
	[Fact]
	public async Task SendAsyncOfT_WithAToken_PassesThatSameTokenToTheBehaviour()
	{
		// Arrange
		using var cts = new CancellationTokenSource();
		var token = cts.Token;

		var behavior = new RecordingBehavior<BehaviourTokenValueCommand, Result<int>>();
		var services = new ServiceCollection();

		services.AddScoped<IDispatcher, Dispatcher>();
		services.AddScoped<ICommandHandler<BehaviourTokenValueCommand, int>>(_ =>
			new RecordingValueCommandHandler<BehaviourTokenValueCommand>());
		services.AddScoped<IPipelineBehavior<BehaviourTokenValueCommand, Result<int>>>(_ =>
			behavior);

		using var provider = services.BuildServiceProvider();

		// Act
		await provider
			.GetRequiredService<IDispatcher>()
			.SendAsync(
				new BehaviourTokenValueCommand(),
				token);

		// Assert
		token.Should().NotBe(CancellationToken.None);
		behavior.WasCalled.Should().BeTrue();
		behavior.ReceivedToken.Should().Be(token);
	}

	/// <summary>
	///		Verifies that the exact token handed to <see cref="IDispatcher.QueryAsync{TResult}(IQuery{TResult}, CancellationToken)"/> is the token the one registered behavior receives.
	/// </summary>
	[Fact]
	public async Task QueryAsync_WithAToken_PassesThatSameTokenToTheBehaviour()
	{
		// Arrange
		using var cts = new CancellationTokenSource();
		var token = cts.Token;

		var behavior = new RecordingBehavior<BehaviourTokenQuery, string>();
		var services = new ServiceCollection();

		services.AddScoped<IDispatcher, Dispatcher>();
		services.AddScoped<IQueryHandler<BehaviourTokenQuery, string>>(_ =>
			new RecordingQueryHandler<BehaviourTokenQuery>());
		services.AddScoped<IPipelineBehavior<BehaviourTokenQuery, string>>(_ =>
			behavior);

		using var provider = services.BuildServiceProvider();

		// Act
		await provider
			.GetRequiredService<IDispatcher>()
			.QueryAsync(
				new BehaviourTokenQuery(),
				token);

		// Assert
		token.Should().NotBe(CancellationToken.None);
		behavior.WasCalled.Should().BeTrue();
		behavior.ReceivedToken.Should().Be(token);
	}

	/// <summary>
	///		Verifies that, with one behavior registered, the exact token handed to <see cref="IDispatcher.SendAsync(ICommand, CancellationToken)"/> is still the token the command handler receives.
	/// </summary>
	[Fact]
	public async Task SendAsync_WithATokenAndARegisteredBehaviour_PassesThatSameTokenToTheCommandHandler()
	{
		// Arrange
		using var cts = new CancellationTokenSource();
		var token = cts.Token;

		var behavior = new RecordingBehavior<PipelinedHandlerTokenCommand, Result>();
		var handler = new RecordingCommandHandler<PipelinedHandlerTokenCommand>();
		var services = new ServiceCollection();

		services.AddScoped<IDispatcher, Dispatcher>();
		services.AddScoped<ICommandHandler<PipelinedHandlerTokenCommand>>(_ =>
			handler);
		services.AddScoped<IPipelineBehavior<PipelinedHandlerTokenCommand, Result>>(_ =>
			behavior);

		using var provider = services.BuildServiceProvider();

		// Act
		await provider
			.GetRequiredService<IDispatcher>()
			.SendAsync(
				new PipelinedHandlerTokenCommand(),
				token);

		// Assert
		token.Should().NotBe(CancellationToken.None);
		behavior.WasCalled.Should().BeTrue();
		handler.WasCalled.Should().BeTrue();
		handler.ReceivedToken.Should().Be(token);
	}

	/// <summary>
	///		Verifies that, with one behavior registered, the exact token handed to <see cref="IDispatcher.SendAsync{TResult}(ICommand{TResult}, CancellationToken)"/> is still the token the value-producing command handler receives.
	/// </summary>
	[Fact]
	public async Task SendAsyncOfT_WithATokenAndARegisteredBehaviour_PassesThatSameTokenToTheCommandHandler()
	{
		// Arrange
		using var cts = new CancellationTokenSource();
		var token = cts.Token;

		var behavior = new RecordingBehavior<PipelinedHandlerTokenValueCommand, Result<int>>();
		var handler = new RecordingValueCommandHandler<PipelinedHandlerTokenValueCommand>();
		var services = new ServiceCollection();

		services.AddScoped<IDispatcher, Dispatcher>();
		services.AddScoped<ICommandHandler<PipelinedHandlerTokenValueCommand, int>>(_ =>
			handler);
		services.AddScoped<IPipelineBehavior<PipelinedHandlerTokenValueCommand, Result<int>>>(_ =>
			behavior);

		using var provider = services.BuildServiceProvider();

		// Act
		await provider
			.GetRequiredService<IDispatcher>()
			.SendAsync(
				new PipelinedHandlerTokenValueCommand(),
				token);

		// Assert
		token.Should().NotBe(CancellationToken.None);
		behavior.WasCalled.Should().BeTrue();
		handler.WasCalled.Should().BeTrue();
		handler.ReceivedToken.Should().Be(token);
	}

	/// <summary>
	///		Verifies that, with one behavior registered, the exact token handed to <see cref="IDispatcher.QueryAsync{TResult}(IQuery{TResult}, CancellationToken)"/> is still the token the query handler receives.
	/// </summary>
	[Fact]
	public async Task QueryAsync_WithATokenAndARegisteredBehaviour_PassesThatSameTokenToTheQueryHandler()
	{
		// Arrange
		using var cts = new CancellationTokenSource();
		var token = cts.Token;

		var behavior = new RecordingBehavior<PipelinedHandlerTokenQuery, string>();
		var handler = new RecordingQueryHandler<PipelinedHandlerTokenQuery>();
		var services = new ServiceCollection();

		services.AddScoped<IDispatcher, Dispatcher>();
		services.AddScoped<IQueryHandler<PipelinedHandlerTokenQuery, string>>(_ =>
			handler);
		services.AddScoped<IPipelineBehavior<PipelinedHandlerTokenQuery, string>>(_ =>
			behavior);

		using var provider = services.BuildServiceProvider();

		// Act
		await provider
			.GetRequiredService<IDispatcher>()
			.QueryAsync(
				new PipelinedHandlerTokenQuery(),
				token);

		// Assert
		token.Should().NotBe(CancellationToken.None);
		behavior.WasCalled.Should().BeTrue();
		handler.WasCalled.Should().BeTrue();
		handler.ReceivedToken.Should().Be(token);
	}

	/// <summary>
	///		A no-payload command defined solely for the handler hand-off token test.
	/// </summary>
	public sealed record HandlerTokenCommand
		: ICommand;

	/// <summary>
	///		A value-producing command defined solely for the handler hand-off token test.
	/// </summary>
	public sealed record HandlerTokenValueCommand
		: ICommand<int>;

	/// <summary>
	///		A query defined solely for the handler hand-off token test.
	/// </summary>
	public sealed record HandlerTokenQuery
		: IQuery<string>;

	/// <summary>
	///		A no-payload command defined solely for the behavior hand-off token test.
	/// </summary>
	public sealed record BehaviourTokenCommand
		: ICommand;

	/// <summary>
	///		A value-producing command defined solely for the behavior hand-off token test.
	/// </summary>
	public sealed record BehaviourTokenValueCommand
		: ICommand<int>;

	/// <summary>
	///		A query defined solely for the behavior hand-off token test.
	/// </summary>
	public sealed record BehaviourTokenQuery
		: IQuery<string>;

	/// <summary>
	///		A no-payload command defined solely for the handler hand-off token test with a behavior registered.
	/// </summary>
	public sealed record PipelinedHandlerTokenCommand
		: ICommand;

	/// <summary>
	///		A value-producing command defined solely for the handler hand-off token test with a behavior registered.
	/// </summary>
	public sealed record PipelinedHandlerTokenValueCommand
		: ICommand<int>;

	/// <summary>
	///		A query defined solely for the handler hand-off token test with a behavior registered.
	/// </summary>
	public sealed record PipelinedHandlerTokenQuery
		: IQuery<string>;

	/// <summary>
	///		A no-payload command handler that records the token it was handed.
	/// </summary>
	/// <typeparam name="TCommand">
	///		The command type this handler serves.
	/// </typeparam>
	private sealed class RecordingCommandHandler<TCommand>
		: ICommandHandler<TCommand>
			where TCommand : ICommand
	{
		public bool WasCalled { get; private set; }

		public CancellationToken ReceivedToken { get; private set; }

		public Task<Result> HandleAsync(
			TCommand command,
			CancellationToken ct)
		{
			WasCalled = true;
			ReceivedToken = ct;

			return Task.FromResult(Result.Success());
		}
	}

	/// <summary>
	///		A value-producing command handler that records the token it was handed.
	/// </summary>
	/// <typeparam name="TCommand">
	///		The command type this handler serves.
	/// </typeparam>
	private sealed class RecordingValueCommandHandler<TCommand>
		: ICommandHandler<TCommand, int>
			where TCommand : ICommand<int>
	{
		public bool WasCalled { get; private set; }

		public CancellationToken ReceivedToken { get; private set; }

		public Task<Result<int>> HandleAsync(
			TCommand command,
			CancellationToken ct)
		{
			WasCalled = true;
			ReceivedToken = ct;

			return Task.FromResult(Result<int>.Success(7));
		}
	}

	/// <summary>
	///		A query handler that records the token it was handed.
	/// </summary>
	/// <typeparam name="TQuery">
	///		The query type this handler serves.
	/// </typeparam>
	private sealed class RecordingQueryHandler<TQuery>
		: IQueryHandler<TQuery, string>
			where TQuery : IQuery<string>
	{
		public bool WasCalled { get; private set; }

		public CancellationToken ReceivedToken { get; private set; }

		public Task<string> HandleAsync(
			TQuery query,
			CancellationToken ct)
		{
			WasCalled = true;
			ReceivedToken = ct;

			return Task.FromResult("queried");
		}
	}

	/// <summary>
	///		A behavior that records the token it was handed before delegating to the next stage of the pipeline.
	/// </summary>
	/// <typeparam name="TRequest">
	///		The request type this behavior applies to.
	/// </typeparam>
	/// <typeparam name="TResponse">
	///		The response type the pipeline produces.
	/// </typeparam>
	private sealed class RecordingBehavior<TRequest, TResponse>
		: IPipelineBehavior<TRequest, TResponse>
			where TRequest : IRequest<TResponse>
	{
		public bool WasCalled { get; private set; }

		public CancellationToken ReceivedToken { get; private set; }

		public Task<TResponse> HandleAsync(
			TRequest request,
			Func<Task<TResponse>> next,
			CancellationToken ct)
		{
			WasCalled = true;
			ReceivedToken = ct;

			return next();
		}
	}
}
