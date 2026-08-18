using DungeonMaster.Domain.Entities.Dungeon;
using DungeonMaster.Infrastructure.Persistence;
using DungeonMaster.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DungeonMaster.Tests.Infrastructure.Persistence.Repositories;

public class DungeonReadRepositoryTests : IDisposable
{
	private readonly SqliteConnection _connection;
	private readonly DbContextOptions<AppDbContext> _options;

	public DungeonReadRepositoryTests()
	{
		_connection = new SqliteConnection("Data Source=:memory:");
		_connection.Open();

		_options = new DbContextOptionsBuilder<AppDbContext>()
			.UseSqlite(_connection)
			.Options;

		using var context = new AppDbContext(_options);
		context.Database.EnsureCreated();
	}

	public void Dispose() => _connection.Dispose();

	private AppDbContext CreateContext() => new(_options);

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

		using var readCtx = CreateContext();
		var repo = new DungeonReadRepository(readCtx);
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

		using var readCtx = CreateContext();
		var repo = new DungeonReadRepository(readCtx);
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
		using var readCtx = CreateContext();
		var repo = new DungeonReadRepository(readCtx);

		var result = await repo.GetDetailAsync(Guid.NewGuid(), CancellationToken.None);

		result.Should().BeNull();
	}
}
