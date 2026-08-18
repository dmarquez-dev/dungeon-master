using DungeonMaster.Domain.Entities.Dungeon;

namespace DungeonMaster.Application.Features.Dungeons;

/// <summary>
///		Provides write access to the dungeon aggregate for command handling.
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
	///		Persists a newly created dungeon aggregate.
	/// </summary>
	/// <param name="dungeon">
	///		The dungeon aggregate to add.
	/// </param>
	/// <param name="ct">
	///		Token used to cancel the operation.
	/// </param>
	Task AddAsync(
		Dungeon dungeon,
		CancellationToken ct);

	/// <summary>
	///		Persists changes made to an existing dungeon aggregate.
	/// </summary>
	/// <param name="dungeon">
	///		The dungeon aggregate to update.
	/// </param>
	/// <param name="ct">
	///		Token used to cancel the operation.
	/// </param>
	Task UpdateAsync(
		Dungeon dungeon,
		CancellationToken ct);

	/// <summary>
	///		Removes the dungeon with the specified identifier.
	/// </summary>
	/// <param name="id">
	///		The unique identifier of the dungeon to delete.
	/// </param>
	/// <param name="ct">
	///		Token used to cancel the operation.
	/// </param>
	/// <returns>
	///		<see langword="true"/> when a dungeon with the given identifier was found and deleted; otherwise <see langword="false"/>.
	/// </returns>
	Task<bool> DeleteAsync(
		Guid id,
		CancellationToken ct);
}
