using DungeonMaster.Domain.Entities.Dungeons;
using DungeonMaster.Domain.ValueObjects;
using DungeonMaster.Infrastructure.Persistence;
using DungeonMaster.Infrastructure.Persistence.Repositories;
using FluentAssertions;

namespace DungeonMaster.Tests.Infrastructure.Persistence.Configurations;

/// <summary>
///		Covers the <c>ValueComparer</c> that <c>AreaConfiguration</c> supplies alongside the outline JSON conversion (INFRA-004 / MASTER-015).
///		Without it EF compares the converted reference type by reference, and <c>Area.Outline</c> hands back a fresh <c>AsReadOnly</c> wrapper on every read, so the snapshot and the current value are never the same instance and every tracked area is reported as modified.
/// </summary>
public sealed class AreaOutlineChangeTrackingTests : SqliteInMemoryTestBase
{
	private static IReadOnlyList<PathSegment> Outline() =>
	[
		new PathSegment(
			PathSegmentKind.Line,
			new TilePoint(0, 0),
			null,
			null),
		new PathSegment(
			PathSegmentKind.Line,
			new TilePoint(4, 0),
			null,
			null),
		new PathSegment(
			PathSegmentKind.CubicBezier,
			new TilePoint(4, 4),
			new TilePoint(6, 1),
			new TilePoint(6, 3)),
	];

	/// <summary>
	///		A replacement outline that differs from <see cref="Outline"/> in every segment, and still
	///		satisfies the minimum of three segments that <c>Area.SetOutline</c> enforces (MASTER-168).
	/// </summary>
	private static IReadOnlyList<PathSegment> EditedOutline() =>
	[
		new PathSegment(
			PathSegmentKind.Line,
			new TilePoint(9, 9),
			null,
			null),
		new PathSegment(
			PathSegmentKind.Line,
			new TilePoint(9, 12),
			null,
			null),
		new PathSegment(
			PathSegmentKind.Line,
			new TilePoint(12, 12),
			null,
			null),
	];

	private async Task<Guid> SeedAsync()
	{
		// Arrange
		var dungeon = new Dungeon("Sunken Temple", "A flooded ruin.");
		var floor = dungeon.AddFloor(null);
		var area = dungeon.AddArea(floor.Id, "Nave", null)!;

		dungeon.UpdateAreaOutline(
			floor.Id,
			area.Id,
			Outline());

		await using var unitOfWork = await CreateFactory().CreateAsync(CancellationToken.None);
		unitOfWork.Dungeons.Add(dungeon);
		await unitOfWork.CommitAsync(CancellationToken.None);

		return dungeon.Id;
	}

	[Fact]
	public async Task CommitAsync_AfterLoadingAnUnmodifiedArea_WritesNothing()
	{
		// Arrange
		var dungeonId = await SeedAsync();

		// Act
		await using var unitOfWork = await CreateFactory().CreateAsync(CancellationToken.None);

		var loaded = await unitOfWork.Dungeons.LoadAsync(
			dungeonId,
			CancellationToken.None);

		var written = await unitOfWork.CommitAsync(CancellationToken.None);

		// Assert
		loaded!.Floors.Single().Areas.Single().Outline.Should().HaveCount(3);
		written.Should().Be(0);
	}

	[Fact]
	public async Task CommitAsync_AfterAGenuineOutlineEdit_StillWritesTheArea()
	{
		// Arrange
		var dungeonId = await SeedAsync();

		// Act
		await using var unitOfWork = await CreateFactory().CreateAsync(CancellationToken.None);

		var loaded = await unitOfWork.Dungeons.LoadAsync(
			dungeonId,
			CancellationToken.None);

		var floor = loaded!.Floors.Single();

		loaded.UpdateAreaOutline(
			floor.Id,
			floor.Areas.Single().Id,
			EditedOutline());

		unitOfWork.Dungeons.Update(loaded);

		var written = await unitOfWork.CommitAsync(CancellationToken.None);

		// Assert
		written.Should().BeGreaterThan(0);
	}

	[Fact]
	public async Task LoadAsync_AfterAnOutlineEdit_ReturnsTheEditedOutline()
	{
		// Arrange
		var dungeonId = await SeedAsync();

		await using (var unitOfWork = await CreateFactory().CreateAsync(CancellationToken.None))
		{
			var loaded = await unitOfWork.Dungeons.LoadAsync(
				dungeonId,
				CancellationToken.None);

			var floor = loaded!.Floors.Single();

			loaded.UpdateAreaOutline(
				floor.Id,
				floor.Areas.Single().Id,
				EditedOutline());

			unitOfWork.Dungeons.Update(loaded);
			await unitOfWork.CommitAsync(CancellationToken.None);
		}

		// Act
		using var verify = new AppDbContext(Options);
		var reloaded = await new DungeonWriteRepository(verify).LoadAsync(
			dungeonId,
			CancellationToken.None);

		// Assert
		reloaded!.Floors.Single().Areas.Single().Outline.Should().Equal(EditedOutline());
	}
}
