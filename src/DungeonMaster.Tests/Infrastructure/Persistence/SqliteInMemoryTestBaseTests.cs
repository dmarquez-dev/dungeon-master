using DungeonMaster.Domain.Entities.Dungeons;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DungeonMaster.Tests.Infrastructure.Persistence;

/// <summary>
///		Guards the fixture every Infrastructure persistence test class inherits.
///		<see cref="AppliedMigrationCount"/> is the ratchet MASTER-014 exists for: because the base applies the schema with <c>Database.Migrate()</c>, a migration that is added to the Infrastructure project but never reaches the database must break a test, and a non-emptiness assertion would not break -- it stays green while some migration was applied, which is exactly the hole.
/// </summary>
public sealed class SqliteInMemoryTestBaseTests : SqliteInMemoryTestBase
{
	/// <summary>
	///		The number of migration files in <c>src/DungeonMaster.Infrastructure/Migrations</c>, excluding the designer and snapshot files.
	///		Measured with <c>ls src/DungeonMaster.Infrastructure/Migrations/*.cs | grep -v Designer | grep -v Snapshot | wc -l</c>, which returned 4: <c>InitialCreate</c>, <c>AddFloorDescription</c>, <c>UtcDateTimeTimestampsAndScaleWithoutStoreDefault</c>, <c>RenameObjectRotationClockwiseMembers</c>.
	///		A migration added without this constant moving fails <see cref="AppliedMigrations_CountMatchesTheMigrationFilesOnDisk"/> deliberately.
	/// </summary>
	private const int AppliedMigrationCount = 4;

	/// <summary>
	///		Verifies that the fixture applies every migration on disk and leaves none pending.
	///		The count is asserted, not merely non-emptiness: an assertion that some migration was applied stays green when a fourth migration is added and never applied.
	/// </summary>
	[Fact]
	public void AppliedMigrations_CountMatchesTheMigrationFilesOnDisk()
	{
		// Arrange
		using var context = CreateContext();

		// Act
		var applied = context.Database.GetAppliedMigrations();
		var pending = context.Database.GetPendingMigrations();

		// Assert
		pending.Should().BeEmpty();
		applied.Count().Should().Be(AppliedMigrationCount);
	}

	/// <summary>
	///		Verifies that <c>CreateContext()</c> hands back a context over the migrated schema, so a derived class can read and write without applying anything itself.
	/// </summary>
	[Fact]
	public async Task CreateContext_ReturnsAContextOverTheMigratedSchema()
	{
		// Arrange
		var dungeon = new Dungeon("Sunken Temple", "A flooded ruin.");

		// Act
		using (var write = CreateContext())
		{
			await write.Dungeons.AddAsync(dungeon);
			await write.SaveChangesAsync();
		}

		// Assert
		using var read = CreateContext();
		(await read.Dungeons.FindAsync(dungeon.Id)).Should().NotBeNull();
	}

	/// <summary>
	///		Verifies that <c>CreateFactory()</c> and <c>CreateDbContextFactory()</c> reach the same in-memory database as <c>CreateContext()</c>, which is what makes one shared open connection a fixture rather than three unrelated ones.
	/// </summary>
	[Fact]
	public async Task CreateFactory_AndCreateDbContextFactory_ReachTheSameDatabaseAsCreateContext()
	{
		// Arrange
		var dungeon = new Dungeon("Sunken Temple", "A flooded ruin.");

		// Act
		await using (var unitOfWork = await CreateFactory().CreateAsync(CancellationToken.None))
		{
			unitOfWork.Dungeons.Add(dungeon);
			await unitOfWork.CommitAsync(CancellationToken.None);
		}

		// Assert
		using (var viaContext = CreateContext())
		{
			(await viaContext.Dungeons.FindAsync(dungeon.Id)).Should().NotBeNull();
		}

		using var viaFactory = CreateDbContextFactory().CreateDbContext();
		(await viaFactory.Dungeons.FindAsync(dungeon.Id)).Should().NotBeNull();
	}
}
