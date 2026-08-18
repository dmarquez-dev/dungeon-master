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
	public async Task DeleteAsync_WithExistingDungeon_ReturnsTrue()
	{
		// Arrange
		var dungeon = new Dungeon("Doomed Keep");

		using (var seedCtx = CreateContext())
		{
			await seedCtx.Dungeons.AddAsync(dungeon);
			await seedCtx.SaveChangesAsync();
		}

		// Act
		bool deleted;

		using (var ctx = CreateContext())
		{
			var repo = new DungeonWriteRepository(ctx);
			deleted = await repo.DeleteAsync(dungeon.Id, CancellationToken.None);
		}

		// Assert
		deleted.Should().BeTrue();

		using var verify = CreateContext();
		var found = await verify.Dungeons.FindAsync(dungeon.Id);
		found.Should().BeNull();
	}

	[Fact]
	public async Task DeleteAsync_WithUnknownId_ReturnsFalseAndSavesNothing()
	{
		// Arrange
		var survivor = new Dungeon("Untouched Vault");

		using (var seedCtx = CreateContext())
		{
			await seedCtx.Dungeons.AddAsync(survivor);
			await seedCtx.SaveChangesAsync();
		}

		// Act
		bool deleted;

		using (var ctx = CreateContext())
		{
			var repo = new DungeonWriteRepository(ctx);
			deleted = await repo.DeleteAsync(Guid.NewGuid(), CancellationToken.None);
		}

		// Assert
		deleted.Should().BeFalse();

		using var verify = CreateContext();
		var found = await verify.Dungeons.FindAsync(survivor.Id);
		found.Should().NotBeNull();
		found!.Name.Should().Be("Untouched Vault");
	}

	[Fact]
	public async Task UpdateAsync_WithDetachedAggregate_PersistsTheChange()
	{
		// Arrange
		var dungeon = new Dungeon("Detached Original");

		using (var seedCtx = CreateContext())
		{
			await seedCtx.Dungeons.AddAsync(dungeon);
			await seedCtx.SaveChangesAsync();
		}

		Dungeon detached;

		using (var loadCtx = CreateContext())
		{
			var loadRepo = new DungeonWriteRepository(loadCtx);
			detached = (await loadRepo.LoadAsync(dungeon.Id, CancellationToken.None))!;
			detached.Update("Detached Updated", null, "Detached notes");
		}

		// Act
		EntityState observedState;

		using (var updateCtx = CreateContext())
		{
			observedState = updateCtx.Entry(detached).State;

			var updateRepo = new DungeonWriteRepository(updateCtx);
			await updateRepo.UpdateAsync(detached, CancellationToken.None);
		}

		// Assert
		observedState.Should().Be(EntityState.Detached);

		using var verify = CreateContext();
		var persisted = await verify.Dungeons.FindAsync(dungeon.Id);
		persisted!.Name.Should().Be("Detached Updated");
		persisted.Notes.Should().Be("Detached notes");
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
