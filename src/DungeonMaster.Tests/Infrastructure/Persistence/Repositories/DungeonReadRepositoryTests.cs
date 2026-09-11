using DungeonMaster.Domain.Entities.Dungeon;
using DungeonMaster.Infrastructure.Persistence;
using DungeonMaster.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DungeonMaster.Tests.Infrastructure.Persistence.Repositories;

public sealed class DungeonReadRepositoryTests : SqliteInMemoryTestBase
{
	[Fact]
	public async Task GetSummariesAsync_ReturnsAllDungeonsWithFloorCount()
	{
		var d1 = new Dungeon("Caverns", "Dark.");
		d1.AddFloor("L1");
		d1.AddFloor("L2");
		var d2 = new Dungeon("Labyrinth", null);

		using (var ctx = CreateContext())
		{
			await ctx.Dungeons.AddRangeAsync(d1, d2);
			await ctx.SaveChangesAsync();
		}

		var repo = new DungeonReadRepository(CreateDbContextFactory());
		var summaries = await repo.GetSummariesAsync(CancellationToken.None);

		summaries.Should().HaveCount(2);
		summaries.First(s => s.Name == "Caverns").FloorCount.Should().Be(2);
		summaries.First(s => s.Name == "Labyrinth").FloorCount.Should().Be(0);
	}

	[Fact]
	public async Task GetDetailAsync_ReturnsFullHierarchy()
	{
		var dungeon = new Dungeon("Castle", "Ancient fortress.");
		dungeon.Update("Castle", "Ancient fortress.", "DM secret notes.");
		var floor = dungeon.AddFloor("Great Hall");
		dungeon.AddArea(floor.Id, "Throne Room", "The main chamber.");

		using (var ctx = CreateContext())
		{
			await ctx.Dungeons.AddAsync(dungeon);
			await ctx.SaveChangesAsync();
		}

		var repo = new DungeonReadRepository(CreateDbContextFactory());
		var detail = await repo.GetDetailAsync(dungeon.Id, CancellationToken.None);

		detail.Should().NotBeNull();
		detail!.Name.Should().Be("Castle");
		detail.Notes.Should().Be("DM secret notes.");
		detail.FloorCount.Should().Be(1);
		detail.Floors.Should().HaveCount(1);
		detail.Floors[0].Areas.Should().HaveCount(1);
		detail.Floors[0].Areas[0].Name.Should().Be("Throne Room");
	}

	[Fact]
	public async Task GetDetailAsync_WithNonExistentId_ReturnsNull()
	{
		var repo = new DungeonReadRepository(CreateDbContextFactory());

		var result = await repo.GetDetailAsync(Guid.NewGuid(), CancellationToken.None);

		result.Should().BeNull();
	}
	/// <summary>
	///		Verifies that a token already cancelled before the call stops the summaries query with <see cref="OperationCanceledException"/> rather than returning rows.
	/// </summary>
	[Fact]
	public async Task GetSummariesAsync_WithAnAlreadyCancelledToken_ThrowsOperationCanceledException()
	{
		// Arrange
		using (var seedCtx = CreateContext())
		{
			await seedCtx.Dungeons.AddAsync(new Dungeon("Cancelled Read", null));
			await seedCtx.SaveChangesAsync();
		}

		var repo = new DungeonReadRepository(CreateDbContextFactory());

		using var cts = new CancellationTokenSource();
		await cts.CancelAsync();

		// Act
		var act = async () =>
			await repo.GetSummariesAsync(cts.Token);

		// Assert
		await act.Should().ThrowAsync<OperationCanceledException>();
	}

	/// <summary>
	///		Verifies that a token already cancelled before the call stops the detail query with <see cref="OperationCanceledException"/> rather than returning a projection.
	/// </summary>
	[Fact]
	public async Task GetDetailAsync_WithAnAlreadyCancelledToken_ThrowsOperationCanceledException()
	{
		// Arrange
		var dungeon = new Dungeon("Cancelled Detail", null);

		using (var seedCtx = CreateContext())
		{
			await seedCtx.Dungeons.AddAsync(dungeon);
			await seedCtx.SaveChangesAsync();
		}

		var repo = new DungeonReadRepository(CreateDbContextFactory());

		using var cts = new CancellationTokenSource();
		await cts.CancelAsync();

		// Act
		var act = async () =>
			await repo.GetDetailAsync(dungeon.Id, cts.Token);

		// Assert
		await act.Should().ThrowAsync<OperationCanceledException>();
	}

}
