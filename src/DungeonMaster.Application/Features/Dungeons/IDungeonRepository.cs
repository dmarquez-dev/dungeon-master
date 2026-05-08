using DungeonMaster.Domain.Entities.Dungeon;

namespace DungeonMaster.Application.Features.Dungeons;

/// <summary>
///		Defines the persistence contract for dungeon data.
///		Implemented in the Infrastructure layer; consumed by Application query handlers.
/// </summary>
public interface IDungeonRepository
{
	/// <summary>
	///		Returns all dungeons without filtering or ordering.
	/// </summary>
	/// <param name="ct">
	///		A token that may signal cancellation.
	/// </param>
	Task<IReadOnlyList<Dungeon>> GetAllAsync(CancellationToken ct);

	/// <summary>
	///		Persists a new dungeon to the data store.
	/// </summary>
	/// <param name="dungeon">
	///		The dungeon entity to add.
	/// </param>
	/// <param name="ct">
	///		A token that may signal cancellation.
	/// </param>
	Task AddAsync(
		Dungeon dungeon,
		CancellationToken ct);

	/// <summary>
	///		Removes the dungeon with the given identifier from the data store.
	///		If no matching dungeon exists, the call is a no-op.
	/// </summary>
	/// <param name="id">
	///		The unique identifier of the dungeon to delete.
	/// </param>
	/// <param name="ct">
	///		A token that may signal cancellation.
	/// </param>
	Task DeleteAsync(
		Guid id,
		CancellationToken ct);
}
