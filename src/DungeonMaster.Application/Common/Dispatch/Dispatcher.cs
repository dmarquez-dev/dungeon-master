using DungeonMaster.Application.Common.Results;
using Microsoft.Extensions.DependencyInjection;

namespace DungeonMaster.Application.Common.Dispatch;

/// <summary>
///		Routes commands and queries to their registered handlers, passing commands through the behavior pipeline.
/// </summary>
public sealed class Dispatcher(
	IServiceProvider serviceProvider)
		: IDispatcher
{
	/// <inheritdoc/>
	public async Task<Result> SendAsync(
		ICommand command,
		CancellationToken ct = default)
	{
		var handlerType = typeof(ICommandHandler<>)
			.MakeGenericType(command.GetType());

		dynamic handler = serviceProvider.GetRequiredService(handlerType);

		return await (Task<Result>)handler.HandleAsync(
			(dynamic)command,
			ct);
	}

	/// <inheritdoc/>
	public async Task<Result<TResult>> SendAsync<TResult>(
		ICommand<TResult> command,
		CancellationToken ct = default)
	{
		var handlerType = typeof(ICommandHandler<,>)
			.MakeGenericType(
				command.GetType(),
				typeof(TResult));

		dynamic handler = serviceProvider.GetRequiredService(handlerType);

		var behaviorType = typeof(IPipelineBehavior<,>)
			.MakeGenericType(
				command.GetType(),
				typeof(TResult));

		var behaviors = serviceProvider
			.GetServices(behaviorType)
			.Cast<dynamic>()
			.Reverse()
			.ToList();

		Func<Task<Result<TResult>>> pipeline = () =>
			(Task<Result<TResult>>)handler.HandleAsync(
				(dynamic)command,
				ct);

		foreach (var behavior in behaviors)
		{
			var next = pipeline;
			pipeline = () =>
				(Task<Result<TResult>>)behavior.HandleAsync(
					(dynamic)command,
					next,
					ct);
		}

		return await pipeline();
	}

	/// <inheritdoc/>
	public async Task<TResult> QueryAsync<TResult>(
		IQuery<TResult> query,
		CancellationToken ct = default)
	{
		var handlerType = typeof(IQueryHandler<,>)
			.MakeGenericType(
				query.GetType(),
				typeof(TResult));

		dynamic handler = serviceProvider.GetRequiredService(handlerType);

		return await (Task<TResult>)handler.HandleAsync(
			(dynamic)query,
			ct);
	}
}
