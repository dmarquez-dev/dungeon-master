using DungeonMaster.Application.Common.Interfaces;
using DungeonMaster.Application.Features.Dungeons.Queries.GetDungeonDetail;
using DungeonMaster.Application.Features.Dungeons.Queries.GetDungeons;
using Microsoft.EntityFrameworkCore;

namespace DungeonMaster.Infrastructure.Persistence.Repositories;

/// <summary>
///		Provides read-side data access operations for <see cref="DungeonMaster.Domain.Entities.Dungeons.Dungeon"/> aggregates.
///		Each query is its own unit of work: it creates a context, projects, and disposes.
/// </summary>
public sealed class DungeonReadRepository(
	IDbContextFactory<AppDbContext> dbContextFactory)
		: IDungeonReadRepository
{
	/// <inheritdoc/>
	public async Task<IReadOnlyList<DungeonSummaryDto>> GetSummariesAsync(CancellationToken ct)
	{
		var dbContext = await dbContextFactory.CreateDbContextAsync(ct).ConfigureAwait(false);
		await using var dbContextScope = dbContext.ConfigureAwait(false);

		var summaries = dbContext.Dungeons
			.AsNoTracking()
			.Select(dungeon =>
				new DungeonSummaryDto(
					dungeon.Id,
					dungeon.Name,
					dungeon.Description,
					dungeon.Floors.Count,
					dungeon.CreatedAt,
					dungeon.LastModified));

		return await summaries.ToListAsync(ct).ConfigureAwait(false);
	}

	/// <inheritdoc/>
	public async Task<DungeonDetailDto?> GetDetailAsync(
		Guid id,
		CancellationToken ct)
	{
		var dbContext = await dbContextFactory.CreateDbContextAsync(ct).ConfigureAwait(false);
		await using var dbContextScope = dbContext.ConfigureAwait(false);

		var detail = dbContext.Dungeons
			.AsNoTracking()
			.AsSplitQuery()
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
								floor.Description,
								floor.Notes,
								floor.Areas
									.Select(area =>
										new AreaDto(
											area.Id,
											area.Name,
											area.Description,
											area.Notes,
											area.Outline))
									.ToList(),
								floor.Objects
									.Select(obj =>
										new PlacedObjectDto(
											Id: obj.Id,
											Kind: obj.Kind,
											Position: obj.Position,
											Rotation: obj.Rotation,
											Scale: obj.Scale))
									.ToList()))
						.ToList()));

		return await detail.FirstOrDefaultAsync(ct).ConfigureAwait(false);
	}
}
