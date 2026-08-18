using System.Collections.Concurrent;
using DungeonMaster.Application.Common.Results;
using Microsoft.Extensions.DependencyInjection;

namespace DungeonMaster.Application.Common.Dispatch;

/// <summary>
///		Routes commands and queries to their registered handlers, passing every request through the behavior pipeline.
///		Reflection runs once per concrete request type: the type-erasing invoker built on the first dispatch is cached and reused for the life of the process.
/// </summary>
/// <param name="serviceProvider">
///		The provider used to resolve handlers and behaviors for each dispatch.
/// </param>
public sealed class Dispatcher(
	IServiceProvider serviceProvider)
		: IDispatcher
{
	private static readonly ConcurrentDictionary<Type, object> Invokers = new();

	/// <summary>
	///		Gets the single invoker cache, keyed on the concrete request type. Exposed for tests that assert the memoisation and single-cache invariants.
	/// </summary>
	internal static IReadOnlyDictionary<Type, object> InvokerCache =>
		Invokers;

	/// <inheritdoc/>
	public Task<Result> SendAsync(
		ICommand command,
		CancellationToken ct = default)
	{
		var invoker = (CommandInvoker)Invokers.GetOrAdd(
			command.GetType(),
			static commandType =>
				Activator.CreateInstance(
					typeof(TypedCommandInvoker<>).MakeGenericType(commandType))!);

		return invoker.InvokeAsync(
			serviceProvider,
			command,
			ct);
	}

	/// <inheritdoc/>
	public Task<Result<TResult>> SendAsync<TResult>(
		ICommand<TResult> command,
		CancellationToken ct = default)
	{
		var invoker = (CommandInvoker<TResult>)Invokers.GetOrAdd(
			command.GetType(),
			static commandType =>
				Activator.CreateInstance(
					typeof(TypedCommandInvoker<,>).MakeGenericType(
						commandType,
						typeof(TResult)))!);

		return invoker.InvokeAsync(
			serviceProvider,
			command,
			ct);
	}

	/// <inheritdoc/>
	public Task<TResult> QueryAsync<TResult>(
		IQuery<TResult> query,
		CancellationToken ct = default)
	{
		var invoker = (QueryInvoker<TResult>)Invokers.GetOrAdd(
			query.GetType(),
			static queryType =>
				Activator.CreateInstance(
					typeof(TypedQueryInvoker<,>).MakeGenericType(
						queryType,
						typeof(TResult)))!);

		return invoker.InvokeAsync(
			serviceProvider,
			query,
			ct);
	}

	/// <summary>
	///		Folds the registered behaviors around the handler, first-registered outermost. This is the only place the chain is built, so the ordering cannot differ between the three request paths.
	/// </summary>
	private static Func<Task<TResponse>> BuildPipeline<TRequest, TResponse>(
		IServiceProvider serviceProvider,
		TRequest request,
		Func<Task<TResponse>> handler,
		CancellationToken ct)
			where TRequest : IRequest<TResponse>
	{
		var behaviors = serviceProvider
			.GetServices<IPipelineBehavior<TRequest, TResponse>>()
			.Reverse()
			.ToList();

		var pipeline = handler;

		foreach (var behavior in behaviors)
		{
			var next = pipeline;
			pipeline = () =>
				behavior.HandleAsync(
					request,
					next,
					ct);
		}

		return pipeline;
	}

	private abstract class CommandInvoker
	{
		public abstract Task<Result> InvokeAsync(
			IServiceProvider serviceProvider,
			ICommand command,
			CancellationToken ct);
	}

	private sealed class TypedCommandInvoker<TCommand>
		: CommandInvoker
			where TCommand : ICommand
	{
		public override Task<Result> InvokeAsync(
			IServiceProvider serviceProvider,
			ICommand command,
			CancellationToken ct)
		{
			var typed = (TCommand)command;

			var handler = serviceProvider
				.GetRequiredService<ICommandHandler<TCommand>>();

			return BuildPipeline<TCommand, Result>(
				serviceProvider,
				typed,
				() =>
					handler.HandleAsync(
						typed,
						ct),
				ct)();
		}
	}

	private abstract class CommandInvoker<TResult>
	{
		public abstract Task<Result<TResult>> InvokeAsync(
			IServiceProvider serviceProvider,
			ICommand<TResult> command,
			CancellationToken ct);
	}

	private sealed class TypedCommandInvoker<TCommand, TResult>
		: CommandInvoker<TResult>
			where TCommand : ICommand<TResult>
	{
		public override Task<Result<TResult>> InvokeAsync(
			IServiceProvider serviceProvider,
			ICommand<TResult> command,
			CancellationToken ct)
		{
			var typed = (TCommand)command;

			var handler = serviceProvider
				.GetRequiredService<ICommandHandler<TCommand, TResult>>();

			return BuildPipeline<TCommand, Result<TResult>>(
				serviceProvider,
				typed,
				() =>
					handler.HandleAsync(
						typed,
						ct),
				ct)();
		}
	}

	private abstract class QueryInvoker<TResult>
	{
		public abstract Task<TResult> InvokeAsync(
			IServiceProvider serviceProvider,
			IQuery<TResult> query,
			CancellationToken ct);
	}

	private sealed class TypedQueryInvoker<TQuery, TResult>
		: QueryInvoker<TResult>
			where TQuery : IQuery<TResult>
	{
		public override Task<TResult> InvokeAsync(
			IServiceProvider serviceProvider,
			IQuery<TResult> query,
			CancellationToken ct)
		{
			var typed = (TQuery)query;

			var handler = serviceProvider
				.GetRequiredService<IQueryHandler<TQuery, TResult>>();

			return BuildPipeline<TQuery, TResult>(
				serviceProvider,
				typed,
				() =>
					handler.HandleAsync(
						typed,
						ct),
				ct)();
		}
	}
}
