using DungeonMaster.Application.Features.Dungeons.Queries.GetDungeonDetail;
using DungeonMaster.Application.Features.Dungeons.Queries.GetDungeons;

namespace DungeonMaster.Application.Common.Interfaces;

/// <summary>
///		Provides read-optimized access to dungeon data for query handling.
/// </summary>
public interface IDungeonReadRepository
{
	/// <summary>
	///		Returns a summary projection of all dungeons.
	/// </summary>
	/// <param name="cancellationToken">
	///		Token used to cancel the operation.
	/// </param>
	/// <returns>
	///		A read-only list of <see cref="DungeonSummaryDto"/> instances.
	/// </returns>
	Task<IReadOnlyList<DungeonSummaryDto>> GetSummariesAsync(CancellationToken cancellationToken);

	/// <summary>
	///		Returns the full detail projection of a dungeon, or <see langword="null"/> if not found.
	/// </summary>
	/// <param name="id">
	///		The unique identifier of the dungeon to retrieve.
	/// </param>
	/// <param name="cancellationToken">
	///		Token used to cancel the operation.
	/// </param>
	/// <returns>
	///		A <see cref="DungeonDetailDto"/> when found, or <see langword="null"/> when no dungeon matches the given identifier.
	/// </returns>
	Task<DungeonDetailDto?> GetDetailAsync(
		Guid id,
		CancellationToken cancellationToken);

	/// <summary>
	///		Returns the projection of a single floor of a dungeon, or <see langword="null"/> if not found.
	/// </summary>
	/// <param name="dungeonId">
	///		The unique identifier of the dungeon that owns the floor.
	/// </param>
	/// <param name="floorId">
	///		The unique identifier of the floor to retrieve.
	/// </param>
	/// <param name="cancellationToken">
	///		Token used to cancel the operation.
	/// </param>
	/// <returns>
	///		A <see cref="FloorDto"/> when the dungeon owns the floor, or <see langword="null"/> when no dungeon has the given
	///		dungeon identifier and when that dungeon owns no floor with the given floor identifier. A floor belonging to a
	///		different dungeon is never returned.
	/// </returns>
	Task<FloorDto?> GetFloorDetailAsync(
		Guid dungeonId,
		Guid floorId,
		CancellationToken cancellationToken);
}
