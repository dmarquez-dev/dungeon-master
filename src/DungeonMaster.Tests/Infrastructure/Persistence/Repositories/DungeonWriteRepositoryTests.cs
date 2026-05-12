using DungeonMaster.Domain.Entities.Dungeon;
using DungeonMaster.Infrastructure.Persistence;
using DungeonMaster.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DungeonMaster.Tests.Infrastructure.Persistence.Repositories;

public class DungeonWriteRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;

    public DungeonWriteRepositoryTests()
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
    public async Task AddAsync_PersistsDungeon()
    {
        var dungeon = new Dungeon("Sunken Temple", "A flooded ruin.");

        using (var ctx = CreateContext())
        {
            var repo = new DungeonWriteRepository(ctx);
            await repo.AddAsync(dungeon, CancellationToken.None);
        }

        using var verify = CreateContext();
        var persisted = await verify.Dungeons.FindAsync(dungeon.Id);
        persisted.Should().NotBeNull();
        persisted!.Name.Should().Be("Sunken Temple");
    }

    [Fact]
    public async Task DeleteAsync_RemovesDungeon()
    {
        var dungeon = new Dungeon("Tomb of the Ancients");
        using (var ctx = CreateContext())
        {
            await ctx.Dungeons.AddAsync(dungeon);
            await ctx.SaveChangesAsync();
        }

        using (var ctx = CreateContext())
        {
            var repo = new DungeonWriteRepository(ctx);
            await repo.DeleteAsync(dungeon.Id, CancellationToken.None);
        }

        using var verify = CreateContext();
        var found = await verify.Dungeons.FindAsync(dungeon.Id);
        found.Should().BeNull();
    }

    [Fact]
    public async Task LoadAsync_HydratesFloorAndAreas()
    {
        var dungeon = new Dungeon("Castle Ravenloft");
        var floor = dungeon.AddFloor("Main Hall");
        dungeon.AddArea(floor.Id, "Throne Room", "The seat of power.");

        using (var ctx = CreateContext())
        {
            await ctx.Dungeons.AddAsync(dungeon);
            await ctx.SaveChangesAsync();
        }

        using var readCtx = CreateContext();
        var repo = new DungeonWriteRepository(readCtx);
        var loaded = await repo.LoadAsync(dungeon.Id, CancellationToken.None);

        loaded.Should().NotBeNull();
        loaded!.Floors.Should().HaveCount(1);
        loaded.Floors[0].Areas.Should().HaveCount(1);
        loaded.Floors[0].Areas[0].Name.Should().Be("Throne Room");
    }

    [Fact]
    public async Task UpdateAsync_PersistsDomainMutations()
    {
        var dungeon = new Dungeon("Old Name");
        using (var ctx = CreateContext())
        {
            await ctx.Dungeons.AddAsync(dungeon);
            await ctx.SaveChangesAsync();
        }

        using (var ctx = CreateContext())
        {
            var repo = new DungeonWriteRepository(ctx);
            var loaded = await repo.LoadAsync(dungeon.Id, CancellationToken.None);
            loaded!.Update("New Name", null, "Some notes");
            await repo.UpdateAsync(loaded, CancellationToken.None);
        }

        using var verify = CreateContext();
        var updated = await verify.Dungeons.FindAsync(dungeon.Id);
        updated!.Name.Should().Be("New Name");
        updated.Notes.Should().Be("Some notes");
    }
}
