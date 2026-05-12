using DungeonMaster.Application.Features.Dungeons;
using DungeonMaster.Application.Features.Dungeons.Queries.GetDungeonDetail;
using DungeonMaster.Application.Features.Dungeons.Queries.GetDungeons;
using Microsoft.EntityFrameworkCore;

namespace DungeonMaster.Infrastructure.Persistence.Repositories;

/// <summary>
///		Provides read-side data access operations for <see cref="DungeonMaster.Domain.Entities.Dungeon.Dungeon"/> aggregates.
/// </summary>
public sealed class DungeonReadRepository(
	AppDbContext dbContext)
		: IDungeonReadRepository
{
	/// <inheritdoc/>
	public async Task<IReadOnlyList<DungeonSummaryDto>> GetSummariesAsync(CancellationToken ct)
		=> await dbContext.Dungeons
			.Select(dungeon =>
				new DungeonSummaryDto(
					dungeon.Id,
					dungeon.Name,
					dungeon.Description,
					dungeon.Floors.Count,
					dungeon.CreatedAt,
					dungeon.LastModified))
			.ToListAsync(ct);

	/// <inheritdoc/>
	public async Task<DungeonDetailDto?> GetDetailAsync(Guid id, CancellationToken ct)
		=> await dbContext.Dungeons
			.Where(dungeon =>
				dungeon.Id == id)
			.Select(dungeon =>
				new DungeonDetailDto(
					dungeon.Id,
					dungeon.Name,
					dungeon.Description,
					dungeon.Notes,
					dungeon.Floors.Count,
					dungeon.CreatedAt,
					dungeon.LastModified,
					dungeon.Floors
						.OrderBy(floor =>
							floor.Number)
						.Select(floor =>
							new FloorDto(
								floor.Id,
								floor.Number,
								floor.Name,
								floor.Notes,
								floor.Areas
									.Select(area =>
										new AreaDto(
											area.Id,
											area.Name,
											area.Description,
											area.Notes))
									.ToList()))
						.ToList()))
			.FirstOrDefaultAsync(ct);
}
