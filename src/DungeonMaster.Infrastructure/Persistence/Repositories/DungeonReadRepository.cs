using DungeonMaster.Application.Common.Interfaces;
using DungeonMaster.Application.Features.Dungeons.Queries.GetDungeonDetail;
using DungeonMaster.Application.Features.Dungeons.Queries.GetDungeons;
using Microsoft.EntityFrameworkCore;

namespace DungeonMaster.Infrastructure.Persistence.Repositories;

/// <summary>
///		Provides read-side data access operations for <see cref="DungeonMaster.Domain.Entities.Dungeon.Dungeon"/> aggregates.
///		Each query is its own unit of work: it creates a context, projects, and disposes.
/// </summary>
public sealed class DungeonReadRepository(
	IDbContextFactory<AppDbContext> dbContextFactory)
		: IDungeonReadRepository
{
	/// <inheritdoc/>
	public async Task<IReadOnlyList<DungeonSummaryDto>> GetSummariesAsync(CancellationToken ct)
	{
		await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);

		return await dbContext.Dungeons
			.AsNoTracking()
			.Select(dungeon =>
				new DungeonSummaryDto(
					dungeon.Id,
					dungeon.Name,
					dungeon.Description,
					dungeon.Floors.Count,
					dungeon.CreatedAt,
					dungeon.LastModified))
			.ToListAsync(ct);
	}

	/// <inheritdoc/>
	public async Task<DungeonDetailDto?> GetDetailAsync(
		Guid id,
		CancellationToken ct)
	{
		await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);

		return await dbContext.Dungeons
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
						.ToList()))
			.FirstOrDefaultAsync(ct);
	}
}
