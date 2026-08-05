using DungeonMaster.Domain.Entities.Floor;
using DungeonMaster.Domain.Entities.Dungeon;
using DungeonMaster.Domain.ValueObjects;
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

    [Fact]
    public async Task LoadAsync_RoundTripsFloorObjectPositionsUnchanged()
    {
        var dungeon = new Dungeon("Sunken Temple", "A flooded ruin.");
        var floor = dungeon.AddFloor(null);
        var obj = floor.AddObject(
            ObjectKind.Chair,
            new CanvasPoint(123.5f, 456.25f),
            ObjectRotation.CW90,
            1.5f);

        using (var ctx = CreateContext())
        {
            var repo = new DungeonWriteRepository(ctx);
            await repo.AddAsync(dungeon, CancellationToken.None);
        }

        using var verify = CreateContext();
        var verifyRepo = new DungeonWriteRepository(verify);
        var loaded = await verifyRepo.LoadAsync(dungeon.Id, CancellationToken.None);

        var loadedObj = loaded!.Floors.Single().Objects.Single();
        loadedObj.Id.Should().Be(obj.Id);
        loadedObj.Position.Should().Be(new CanvasPoint(123.5f, 456.25f));
        loadedObj.Rotation.Should().Be(ObjectRotation.CW90);
        loadedObj.Scale.Should().Be(1.5f);
        loadedObj.FloorId.Should().Be(floor.Id);
    }
}
