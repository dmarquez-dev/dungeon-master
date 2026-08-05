using DungeonMaster.Domain.Entities.Dungeon;
using DungeonMaster.Infrastructure.Persistence;
using DungeonMaster.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DungeonMaster.Tests.Infrastructure.Persistence.Repositories;

/// <summary>
///     Round-trip coverage for floor metadata through the real write repository.
///     The rename-preserves-notes test is the regression guard for a defect where
///     the designer sent a hardcoded null for notes when renaming a floor.
/// </summary>
public class FloorPersistenceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;

    public FloorPersistenceTests()
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

    private async Task<(Guid DungeonId, Guid FloorId)> SeedAsync()
    {
        var dungeon = new Dungeon("Sunken Temple", "A flooded ruin.");
        var floor = dungeon.AddFloor("Crypt");

        using var ctx = CreateContext();
        var repo = new DungeonWriteRepository(ctx);
        await repo.AddAsync(dungeon, CancellationToken.None);

        return (dungeon.Id, floor.Id);
    }

    [Fact]
    public async Task UpdateFloor_DescriptionPersistsAcrossReload()
    {
        var (dungeonId, floorId) = await SeedAsync();

        using (var ctx = CreateContext())
        {
            var repo = new DungeonWriteRepository(ctx);
            var loaded = await repo.LoadAsync(dungeonId, CancellationToken.None);
            loaded!.UpdateFloor(floorId, "Crypt", "The air grows cold.", "Ghasts.");
            await repo.UpdateAsync(loaded, CancellationToken.None);
        }

        using var verify = CreateContext();
        var verifyRepo = new DungeonWriteRepository(verify);
        var reloaded = await verifyRepo.LoadAsync(dungeonId, CancellationToken.None);

        var floor = reloaded!.Floors.Single();
        floor.Description.Should().Be("The air grows cold.");
        floor.Notes.Should().Be("Ghasts.");
    }

    [Fact]
    public async Task UpdateFloor_RenamePreservesDescriptionAndNotes()
    {
        var (dungeonId, floorId) = await SeedAsync();

        using (var ctx = CreateContext())
        {
            var repo = new DungeonWriteRepository(ctx);
            var loaded = await repo.LoadAsync(dungeonId, CancellationToken.None);
            loaded!.UpdateFloor(floorId, "Crypt", "The air grows cold.", "Ghasts.");
            await repo.UpdateAsync(loaded, CancellationToken.None);
        }

        // A rename must carry the existing description and notes forward, not blank them.
        using (var ctx = CreateContext())
        {
            var repo = new DungeonWriteRepository(ctx);
            var loaded = await repo.LoadAsync(dungeonId, CancellationToken.None);
            var existing = loaded!.Floors.Single();
            loaded.UpdateFloor(floorId, "Ossuary", existing.Description, existing.Notes);
            await repo.UpdateAsync(loaded, CancellationToken.None);
        }

        using var verify = CreateContext();
        var verifyRepo = new DungeonWriteRepository(verify);
        var reloaded = await verifyRepo.LoadAsync(dungeonId, CancellationToken.None);

        var floor = reloaded!.Floors.Single();
        floor.Name.Should().Be("Ossuary");
        floor.Description.Should().Be("The air grows cold.");
        floor.Notes.Should().Be("Ghasts.");
    }
}
