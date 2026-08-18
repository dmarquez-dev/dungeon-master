namespace DungeonMaster.Application.Common.Dispatch;

/// <summary>
///		Defines a pipeline stage that wraps request handling with cross-cutting behavior.
/// </summary>
/// <typeparam name="TRequest">
///		The request type this behavior applies to.
/// </typeparam>
/// <typeparam name="TResponse">
///		The type the handler returns, including any result wrapper.
/// </typeparam>
public interface IPipelineBehavior<in TRequest, TResponse>
	where TRequest : IRequest<TResponse>
{
	/// <summary>
	///		Executes the behavior and invokes the next stage in the pipeline.
	/// </summary>
	/// <param name="request">
	///		The request being processed.
	/// </param>
	/// <param name="next">
	///		A delegate that invokes the next stage in the pipeline.
	/// </param>
	/// <param name="ct">
	///		Token used to cancel the operation.
	/// </param>
	/// <returns>
	///		The response produced by the pipeline.
	/// </returns>
	Task<TResponse> HandleAsync(
		TRequest request,
		Func<Task<TResponse>> next,
		CancellationToken ct);
}
