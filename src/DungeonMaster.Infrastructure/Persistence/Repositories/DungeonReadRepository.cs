using DungeonMaster.Application.Common.Interfaces;
using DungeonMaster.Application.Features.Dungeons.Queries.GetDungeonDetail;
using DungeonMaster.Application.Features.Dungeons.Queries.GetDungeons;
using DungeonMaster.Domain.Entities.Floors;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DungeonMaster.Infrastructure.Persistence.Repositories;

/// <summary>
///		Provides read-side data access operations for <see cref="DungeonMaster.Domain.Entities.Dungeons.Dungeon"/> aggregates.
///		Each query is its own unit of work: it creates a context, projects, and disposes.
/// </summary>
public sealed class DungeonReadRepository(
	IDbContextFactory<AppDbContext> dbContextFactory)
		: IDungeonReadRepository
{
	/// <summary>
	///		The one projection of a floor to a <see cref="FloorDto"/>, shared by <see cref="GetDetailAsync"/> and
	///		<see cref="GetFloorDetailAsync"/> so a floor read on its own cannot drift from the same floor read as part
	///		of the full dungeon detail.
	/// </summary>
	private static readonly Expression<Func<Floor, FloorDto>> FloorProjection =
		floor =>
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
					.Select(placedObject =>
						new PlacedObjectDto(
							Id: placedObject.Id,
							Kind: placedObject.Kind,
							Position: placedObject.Position,
							Rotation: placedObject.Rotation,
							Scale: placedObject.Scale))
					.ToList());

	/// <inheritdoc/>
	public async Task<IReadOnlyList<DungeonSummaryDto>> GetSummariesAsync(CancellationToken cancellationToken)
	{
		var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
		await using var dbContextScope = dbContext.ConfigureAwait(false);

		var summaries = dbContext.Dungeons
			// The two AsNoTracking() calls in this file are not equivalent, and both
			// are kept exactly as they are.
			// This one, on the summary query, is believed to be a no-op. The
			// projection below constructs a DungeonSummaryDto, whose primary
			// constructor declares six members -- Guid Id, string Name,
			// string? Description, int FloorCount, DateTime CreatedAt and
			// DateTime LastModified -- and every one of them is a scalar. We read
			// EF Core as tracking entity instances only, so a result set of scalars
			// alone leaves the change tracker nothing to hold. Whether this call
			// should therefore be removed is a scope question this session did not
			// evaluate, so the call stays.
			.AsNoTracking()
			.Select(dungeon =>
				new DungeonSummaryDto(
					dungeon.Id,
					dungeon.Name,
					dungeon.Description,
					dungeon.Floors.Count,
					dungeon.CreatedAt,
					dungeon.LastModified));

		return await summaries.ToListAsync(cancellationToken).ConfigureAwait(false);
	}

	/// <inheritdoc/>
	public async Task<DungeonDetailDto?> GetDetailAsync(
		Guid id,
		CancellationToken cancellationToken)
	{
		var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
		await using var dbContextScope = dbContext.ConfigureAwait(false);

		var detail = dbContext.Dungeons
			// This AsNoTracking() is load-bearing, unlike the one on the summary
			// query above. The projection below reaches Position: placedObject.Position, and
			// PlacedObjectDto declares that member as CanvasPoint; CanvasPoint is
			// mapped as an owned type by PlacedObjectConfiguration's
			// OwnsOne(... Position ...) call. An owned type is an entity type in
			// EF Core's model, which is why this result set is tracked and this
			// call is not a no-op. The other members of this projection were not
			// surveyed here, so none of the above says anything about what else
			// the projection contains.
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
						.AsQueryable()
						.OrderBy(floor =>
							floor.Number)
						.Select(FloorProjection)
						.ToList()));

		return await detail.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
	}

	/// <inheritdoc/>
	public async Task<FloorDto?> GetFloorDetailAsync(
		Guid dungeonId,
		Guid floorId,
		CancellationToken cancellationToken)
	{
		var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
		await using var dbContextScope = dbContext.ConfigureAwait(false);

		var floorDetail = dbContext.Dungeons
			// Load-bearing for the same reason as the AsNoTracking() in GetDetailAsync:
			// FloorProjection reaches Position: placedObject.Position, an owned CanvasPoint.
			.AsNoTracking()
			.AsSplitQuery()
			.Where(dungeon =>
				dungeon.Id == dungeonId)
			.SelectMany(dungeon =>
				dungeon.Floors)
			.Where(floor =>
				floor.Id == floorId)
			.Select(FloorProjection);

		return await floorDetail.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
	}
}
