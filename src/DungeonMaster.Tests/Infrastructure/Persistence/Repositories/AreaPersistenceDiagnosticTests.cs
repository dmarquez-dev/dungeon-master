using DungeonMaster.Domain.Entities.Dungeon;
using DungeonMaster.Infrastructure.Persistence;
using DungeonMaster.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DungeonMaster.Tests.Infrastructure.Persistence.Repositories;

/// <summary>
///     Diagnostic coverage for area removal and rename round-tripping through the real
///     write repository. Added while investigating a report that both operations appeared
///     not to persist from the designer UI.
/// </summary>
public class AreaPersistenceDiagnosticTests : IDisposable
{
	private readonly SqliteConnection _connection;
	private readonly DbContextOptions<AppDbContext> _options;

	public AreaPersistenceDiagnosticTests()
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

	private async Task<(Guid DungeonId, Guid FloorId, Guid AreaId)> SeedAsync()
	{
		var dungeon = new Dungeon("Sunken Temple", "A flooded ruin.");
		var floor = dungeon.AddFloor(null);
		var area = dungeon.AddArea(floor.Id, "New Area", null)!;

		using var ctx = CreateContext();
		var repo = new DungeonWriteRepository(ctx);
		await repo.AddAsync(dungeon, CancellationToken.None);

		return (dungeon.Id, floor.Id, area.Id);
	}

	[Fact]
	public async Task RemoveArea_PersistsAcrossReload()
	{
		var (dungeonId, floorId, areaId) = await SeedAsync();

		using (var ctx = CreateContext())
		{
			var repo = new DungeonWriteRepository(ctx);
			var loaded = await repo.LoadAsync(dungeonId, CancellationToken.None);
			loaded!.RemoveArea(floorId, areaId);
			await repo.UpdateAsync(loaded, CancellationToken.None);
		}

		using var verify = CreateContext();
		var verifyRepo = new DungeonWriteRepository(verify);
		var reloaded = await verifyRepo.LoadAsync(dungeonId, CancellationToken.None);

		reloaded!.Floors.Single().Areas.Should().BeEmpty();
	}

	[Fact]
	public async Task RemoveArea_WithEmptyOutline_PersistsAcrossReload()
	{
		var (dungeonId, floorId, areaId) = await SeedAsync();

		using (var ctx = CreateContext())
		{
			var repo = new DungeonWriteRepository(ctx);
			var loaded = await repo.LoadAsync(dungeonId, CancellationToken.None);
			loaded!.Floors.Single().Areas.Single().Outline.Should().BeEmpty();
			loaded.RemoveArea(floorId, areaId);
			await repo.UpdateAsync(loaded, CancellationToken.None);
		}

		using var verify = CreateContext();
		var verifyRepo = new DungeonWriteRepository(verify);
		var reloaded = await verifyRepo.LoadAsync(dungeonId, CancellationToken.None);

		reloaded!.Floors.Single().Areas.Should().BeEmpty();
	}

	[Fact]
	public async Task UpdateArea_RenamePersistsAcrossReload()
	{
		var (dungeonId, floorId, areaId) = await SeedAsync();

		using (var ctx = CreateContext())
		{
			var repo = new DungeonWriteRepository(ctx);
			var loaded = await repo.LoadAsync(dungeonId, CancellationToken.None);
			loaded!.UpdateArea(floorId, areaId, "Chapel", null, null);
			await repo.UpdateAsync(loaded, CancellationToken.None);
		}

		using var verify = CreateContext();
		var verifyRepo = new DungeonWriteRepository(verify);
		var reloaded = await verifyRepo.LoadAsync(dungeonId, CancellationToken.None);

		reloaded!.Floors.Single().Areas.Single().Name.Should().Be("Chapel");
	}

	[Fact]
	public async Task UpdateArea_SecondRenamePersistsAcrossReload()
	{
		var (dungeonId, floorId, areaId) = await SeedAsync();

		using (var ctx = CreateContext())
		{
			var repo = new DungeonWriteRepository(ctx);
			var loaded = await repo.LoadAsync(dungeonId, CancellationToken.None);
			loaded!.UpdateArea(floorId, areaId, "Chapel", null, null);
			await repo.UpdateAsync(loaded, CancellationToken.None);
		}

		using (var ctx = CreateContext())
		{
			var repo = new DungeonWriteRepository(ctx);
			var loaded = await repo.LoadAsync(dungeonId, CancellationToken.None);
			loaded!.UpdateArea(floorId, areaId, "Crypt", null, null);
			await repo.UpdateAsync(loaded, CancellationToken.None);
		}

		using var verify = CreateContext();
		var verifyRepo = new DungeonWriteRepository(verify);
		var reloaded = await verifyRepo.LoadAsync(dungeonId, CancellationToken.None);

		reloaded!.Floors.Single().Areas.Single().Name.Should().Be("Crypt");
	}
}
