using DungeonMaster.Domain.Entities.Dungeon;
using DungeonMaster.Infrastructure.Persistence;
using DungeonMaster.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DungeonMaster.Tests.Infrastructure.Persistence.Repositories;

/// <summary>
///		Integration tests for <see cref="DungeonRepository"/> using an in-memory SQLite database.
///		Each test class instance owns a dedicated connection so schema changes do not bleed across tests.
/// </summary>
public class DungeonRepositoryTests : IDisposable
{
	private readonly SqliteConnection _connection;
	private readonly DbContextOptions<AppDbContext> _options;

	/// <summary>
	///		Opens a shared SQLite in-memory connection, builds the schema, and prepares reusable context options.
	/// </summary>
	public DungeonRepositoryTests()
	{
		_connection = new SqliteConnection("Data Source=:memory:");
		_connection.Open();

		_options = new DbContextOptionsBuilder<AppDbContext>()
			.UseSqlite(_connection)
			.Options;

		using var context = new AppDbContext(_options);
		context.Database.EnsureCreated();
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		_connection.Dispose();
	}

	private AppDbContext CreateContext() => new(_options);

	/// <summary>
	///		Verifies that <see cref="DungeonRepository.GetAllAsync"/> returns all dungeons present in the store.
	/// </summary>
	[Fact]
	public async Task GetAllAsync_WithDungeons_ReturnsAll()
	{
		// Arrange
		var firstDungeon = new Dungeon(
			"Caverns of Doom",
			"A dark and dangerous place.");

		var secondDungeon = new Dungeon(
			"Crystal Labyrinth",
			null);

		using (var seedContext = CreateContext())
		{
			await seedContext.Dungeons
				.AddRangeAsync(
					firstDungeon,
					secondDungeon);

			await seedContext.SaveChangesAsync();
		}

		// Act
		using var readContext = CreateContext();
		var repository = new DungeonRepository(readContext);

		var result = await repository.GetAllAsync(CancellationToken.None);

		// Assert
		result.Should().HaveCount(2);
		result.Should().Contain(dungeon =>
			dungeon.Name == "Caverns of Doom");

		result.Should().Contain(dungeon =>
			dungeon.Name == "Crystal Labyrinth");
	}

	/// <summary>
	///		Verifies that <see cref="DungeonRepository.AddAsync"/> persists the dungeon so it can be retrieved via a fresh context.
	/// </summary>
	[Fact]
	public async Task AddAsync_WithValidDungeon_PersistsDungeon()
	{
		// Arrange
		var dungeon = new Dungeon(
			"Sunken Temple",
			"A flooded ruin beneath the sea.");

		// Act
		using (var writeContext = CreateContext())
		{
			var repository = new DungeonRepository(writeContext);
			await repository.AddAsync(dungeon, CancellationToken.None);
		}

		// Assert
		using var verifyContext = CreateContext();
		var persisted = await verifyContext.Dungeons
			.FindAsync(dungeon.Id);

		persisted.Should().NotBeNull();
		persisted!.Name.Should().Be("Sunken Temple");
		persisted.Description.Should().Be("A flooded ruin beneath the sea.");
	}

	/// <summary>
	///		Verifies that <see cref="DungeonRepository.DeleteAsync"/> removes an existing dungeon so it is no longer retrievable.
	/// </summary>
	[Fact]
	public async Task DeleteAsync_WithExistingId_RemovesDungeon()
	{
		// Arrange
		var dungeon = new Dungeon("Tomb of the Ancients");

		using (var seedContext = CreateContext())
		{
			await seedContext.Dungeons
				.AddAsync(dungeon);

			await seedContext.SaveChangesAsync();
		}

		// Act
		using (var deleteContext = CreateContext())
		{
			var repository = new DungeonRepository(deleteContext);
			await repository.DeleteAsync(dungeon.Id, CancellationToken.None);
		}

		// Assert
		using var verifyContext = CreateContext();
		var found = await verifyContext.Dungeons
			.FindAsync(dungeon.Id);

		found.Should().BeNull();
	}

	/// <summary>
	///		Verifies that <see cref="DungeonRepository.DeleteAsync"/> is a no-op when the given id does not match any dungeon.
	/// </summary>
	[Fact]
	public async Task DeleteAsync_WithNonExistentId_IsNoOp()
	{
		// Arrange
		var existingDungeon = new Dungeon("The Frozen Spire");

		using (var seedContext = CreateContext())
		{
			await seedContext.Dungeons
				.AddAsync(existingDungeon);

			await seedContext.SaveChangesAsync();
		}

		// Act
		using var actionContext = CreateContext();
		var repository = new DungeonRepository(actionContext);

		var act = async () =>
			await repository.DeleteAsync(
				Guid.NewGuid(),
				CancellationToken.None);

		// Assert
		await act.Should().NotThrowAsync();

		using var verifyContext = CreateContext();
		var count = await verifyContext.Dungeons
			.CountAsync();

		count.Should().Be(1);
	}
}
