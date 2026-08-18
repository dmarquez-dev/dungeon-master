using DungeonMaster.Application.Common.Interfaces;
using DungeonMaster.Domain.Entities.Dungeon;
using DungeonMaster.Domain.Entities.Floor;
using DungeonMaster.Domain.ValueObjects;
using DungeonMaster.Infrastructure.Persistence;
using DungeonMaster.Infrastructure.Persistence.Repositories;
using DungeonMaster.Tests.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace DungeonMaster.Tests.Infrastructure.Persistence.Configurations;

/// <summary>
///		Covers the <c>Scale</c> mapping after INFRA-008 / MASTER-075 dropped its store default.
///		With <c>HasDefaultValue(1.0f)</c> in place the property was store-generated on add, so any value equal to the CLR default would have been silently replaced by the store default; the value now travels as ordinary data.
/// </summary>
public class PlacedObjectConfigurationTests : IDisposable
{
	private readonly SqliteConnection _connection;
	private readonly DbContextOptions<AppDbContext> _options;

	public PlacedObjectConfigurationTests()
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

	private IUnitOfWorkFactory CreateFactory() =>
		new UnitOfWorkFactory(new TestDbContextFactory(_options));

	[Theory]
	[InlineData(0.25f)]
	[InlineData(2.5f)]
	[InlineData(4.0f)]
	public async Task CommitAsync_ThenReload_KeepsTheScaleTheObjectWasConstructedWith(float scale)
	{
		// Arrange
		var dungeon = new Dungeon("Sunken Temple", "A flooded ruin.");
		var floor = dungeon.AddFloor(null);

		dungeon.AddPlacedObject(
			floor.Id,
			ObjectKind.DoorWood,
			new CanvasPoint(12.5f, 7.25f),
			ObjectRotation.None,
			scale);

		// Act
		await using (var unitOfWork = await CreateFactory().CreateAsync(CancellationToken.None))
		{
			unitOfWork.Dungeons.Add(dungeon);
			await unitOfWork.CommitAsync(CancellationToken.None);
		}

		using var verify = new AppDbContext(_options);
		var reloaded = await new DungeonWriteRepository(verify).LoadAsync(
			dungeon.Id,
			CancellationToken.None);

		// Assert
		reloaded!.Floors.Single().Objects.Single().Scale.Should().Be(scale);
	}

	[Fact]
	public void Model_ForScale_DoesNotDeclareAStoreGeneratedDefault()
	{
		// Arrange
		using var context = new AppDbContext(_options);

		// Act
		var scale = context.Model
			.FindEntityType(typeof(PlacedObject))!
			.FindProperty(nameof(PlacedObject.Scale))!;

		// Assert
		scale.TryGetDefaultValue(out _).Should().BeFalse();
		scale.ValueGenerated.Should().Be(ValueGenerated.Never);
	}
}
