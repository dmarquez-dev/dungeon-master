using DungeonMaster.Application.Common.Interfaces;
using DungeonMaster.Application.Common.Results;
using DungeonMaster.Domain.Entities.Dungeons;

namespace DungeonMaster.Application.Common.Handlers;

/// <summary>
///		Provides the shared load-guard-mutate-save prologue that every dungeon mutation command handler runs.
/// </summary>
public static class DungeonMutationExtensions
{
	/// <summary>
	///		Loads the dungeon, applies the supplied mutation, and saves it when the mutation succeeded.
	/// </summary>
	/// <param name="unitOfWorkFactory">
	///		The factory used to open the unit of work that brackets the load, the mutation and the commit.
	/// </param>
	/// <param name="dungeonId">
	///		The unique identifier of the dungeon to mutate.
	/// </param>
	/// <param name="mutate">
	///		The mutation to apply to the loaded aggregate.
	/// </param>
	/// <param name="cancellationToken">
	///		Token used to cancel the operation.
	/// </param>
	/// <returns>
	///		A not-found <see cref="Result"/> when no dungeon matches the identifier, the mutation's own failure when it failed, or the mutation's success after the aggregate has been persisted.
	/// </returns>
	public static async Task<Result> MutateAsync(
		this IUnitOfWorkFactory unitOfWorkFactory,
		Guid dungeonId,
		Func<Dungeon, Result> mutate,
		CancellationToken cancellationToken)
	{
		await using var unitOfWork = await unitOfWorkFactory.CreateAsync(cancellationToken);

		var dungeon = await unitOfWork.Dungeons.LoadAsync(
			dungeonId,
			cancellationToken);

		if (dungeon is null)
		{
			return Result.Failure(
				$"Dungeon {dungeonId} not found.",
				ErrorCategory.NotFound);
		}

		var result = mutate(dungeon);

		if (result.IsFailure)
		{
			return result;
		}

		unitOfWork.Dungeons.Update(dungeon);

		await unitOfWork.CommitAsync(cancellationToken);

		return result;
	}

	/// <summary>
	///		Loads the dungeon, applies the supplied value-producing mutation, and saves it when the mutation succeeded.
	/// </summary>
	/// <typeparam name="TResult">
	///		The type of the value produced when the mutation succeeds.
	/// </typeparam>
	/// <param name="unitOfWorkFactory">
	///		The factory used to open the unit of work that brackets the load, the mutation and the commit.
	/// </param>
	/// <param name="dungeonId">
	///		The unique identifier of the dungeon to mutate.
	/// </param>
	/// <param name="mutate">
	///		The mutation to apply to the loaded aggregate.
	/// </param>
	/// <param name="cancellationToken">
	///		Token used to cancel the operation.
	/// </param>
	/// <returns>
	///		A not-found <see cref="Result{TResult}"/> when no dungeon matches the identifier, the mutation's own failure when it failed, or the mutation's success after the aggregate has been persisted.
	/// </returns>
	public static async Task<Result<TResult>> MutateAsync<TResult>(
		this IUnitOfWorkFactory unitOfWorkFactory,
		Guid dungeonId,
		Func<Dungeon, Result<TResult>> mutate,
		CancellationToken cancellationToken)
	{
		await using var unitOfWork = await unitOfWorkFactory.CreateAsync(cancellationToken);

		var dungeon = await unitOfWork.Dungeons.LoadAsync(
			dungeonId,
			cancellationToken);

		if (dungeon is null)
		{
			return Result<TResult>.Failure(
				$"Dungeon {dungeonId} not found.",
				ErrorCategory.NotFound);
		}

		var result = mutate(dungeon);

		if (result.IsFailure)
		{
			return result;
		}

		unitOfWork.Dungeons.Update(dungeon);

		await unitOfWork.CommitAsync(cancellationToken);

		return result;
	}
}
