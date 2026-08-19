using DungeonMaster.Domain.Entities.Dungeon;

namespace DungeonMaster.Application.Common.Interfaces;

/// <summary>
///		Provides write access to the dungeon aggregate for command handling.
///		Every method on this port only stages work; nothing reaches the store until <see cref="IUnitOfWork.CommitAsync"/> runs.
/// </summary>
public interface IDungeonWriteRepository
{
	/// <summary>
	///		Loads a dungeon aggregate by its identifier, or returns <see langword="null"/> if not found.
	/// </summary>
	/// <param name="id">
	///		The unique identifier of the dungeon to load.
	/// </param>
	/// <param name="ct">
	///		Token used to cancel the operation.
	/// </param>
	/// <returns>
	///		The <see cref="Dungeon"/> aggregate when found, or <see langword="null"/> when no dungeon matches the given identifier.
	/// </returns>
	Task<Dungeon?> LoadAsync(
		Guid id,
		CancellationToken ct);

	/// <summary>
	///		Marks a newly created dungeon aggregate for insertion.
	/// </summary>
	/// <param name="dungeon">
	///		The dungeon aggregate to add.
	/// </param>
	void Add(Dungeon dungeon);

	/// <summary>
	///		Marks changes made to an existing dungeon aggregate for update.
	/// </summary>
	/// <param name="dungeon">
	///		The dungeon aggregate to update.
	/// </param>
	void Update(Dungeon dungeon);

	/// <summary>
	///		Marks the dungeon with the specified identifier for removal.
	/// </summary>
	/// <param name="id">
	///		The unique identifier of the dungeon to remove.
	/// </param>
	/// <param name="ct">
	///		Token used to cancel the operation.
	/// </param>
	/// <returns>
	///		<see langword="true"/> when an aggregate with the given identifier was found and marked for removal; <see langword="false"/> when no aggregate with that identifier exists.
	///		Neither value says anything about whether a commit happened.
	/// </returns>
	Task<bool> RemoveAsync(
		Guid id,
		CancellationToken ct);
}
