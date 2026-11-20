using DungeonMaster.Domain.Entities.Dungeons;
using DungeonMaster.Domain.Entities.Floors;
using DungeonMaster.Domain.ValueObjects;
using DungeonMaster.Infrastructure.Persistence;
using DungeonMaster.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DungeonMaster.Tests.Infrastructure.Persistence.Configurations;

/// <summary>
///		Pins what the <c>CanvasPoint</c> finiteness guard (MASTER-081 / CORE-016) does to a row that already holds a
///		non-finite coordinate, which the guard cannot prevent: it closes the type, not the database. A row can hold one
///		if it predates the guard, or if anything writes the column outside the domain, so the question is what happens
///		on the way back in. EF binds the owned <c>CanvasPoint</c> through its constructor, so the guard runs during
///		materialization and the load throws rather than handing back a poisoned aggregate.
///		<para>
///			The value written here is an <b>infinity</b> and not a <see cref="float.NaN"/>, and that is forced rather
///			than chosen. <c>CanvasPointNonFinitePersistenceTests.cs</c> established that a <see cref="float.NaN"/>
///			cannot be written at all — <c>Microsoft.Data.Sqlite</c> refuses to bind a <see cref="float.NaN"/> parameter
///			— so no <see cref="float.NaN"/> row exists to load and this test could not be constructed around one.
///			Both infinities, by contrast, round-trip intact, which is exactly what makes them the reachable case.
///		</para>
/// </summary>
public sealed class CanvasPointGuardMaterializationTests : SqliteInMemoryTestBase
{
	[Theory]
	[InlineData(double.PositiveInfinity)]
	[InlineData(double.NegativeInfinity)]
	public async Task LoadAsync_ForAStoredInfiniteCoordinate_ThrowsFromTheGuardNamingX(double stored)
	{
		// Arrange. The row is created through the domain with a finite position, because the guard is precisely
		// what stops a non-finite one from getting in this way.
		var dungeon = new Dungeon("Sunken Temple", "A flooded ruin.");
		var floor = dungeon.AddFloor(null);

		dungeon.AddPlacedObject(
			floor.Id,
			ObjectKind.DoorWood,
			new CanvasPoint(12.5f, 7.25f),
			ObjectRotation.None,
			1.0f);

		await using (var unitOfWork = await CreateFactory().CreateAsync(CancellationToken.None))
		{
			unitOfWork.Dungeons.Add(dungeon);
			await unitOfWork.CommitAsync(CancellationToken.None);
		}

		// Act. Raw SQL, bypassing the domain entirely.
		using var writer = new AppDbContext(Options);
		var updated = await writer.Database.ExecuteSqlRawAsync(
			"UPDATE PlacedObjects SET Position_X = {0}",
			[stored],
			CancellationToken.None);

		// The bypass is itself asserted, not assumed. Were the write to land as a NULL, a 0 or nothing at all, the
		// load below would throw for some entirely different reason — or not throw — and the test would be reporting
		// on an arrangement that never happened.
		updated.Should().Be(1);
		(await ReadStoredPositionXAsync(writer)).Should().Be(stored);

		using var verify = new AppDbContext(Options);
		var load = async () => await new DungeonWriteRepository(verify).LoadAsync(
			dungeon.Id,
			CancellationToken.None);

		// Assert. The observed outcome is the guard's own exception, surfacing unwrapped: EF does not wrap a
		// constructor-binding failure here, so there is no DbUpdateException or InvalidOperationException around it.
		var thrown = await load.Should().ThrowExactlyAsync<ArgumentOutOfRangeException>();

		thrown.Which.ParamName.Should().Be("X");
		thrown.Which.ActualValue.Should().Be((float)stored);
		thrown.Which.InnerException.Should().BeNull();
	}

	/// <summary>
	///		Positive control. Without it a run in which the load threw for any reason at all — a broken fixture, a
	///		migration that never applied, an aggregate EF cannot bind — would read as the guard firing.
	/// </summary>
	[Fact]
	public async Task LoadAsync_ForAStoredFiniteCoordinate_Succeeds()
	{
		// Arrange
		var dungeon = new Dungeon("Sunken Temple", "A flooded ruin.");
		var floor = dungeon.AddFloor(null);

		dungeon.AddPlacedObject(
			floor.Id,
			ObjectKind.DoorWood,
			new CanvasPoint(12.5f, 7.25f),
			ObjectRotation.None,
			1.0f);

		await using (var unitOfWork = await CreateFactory().CreateAsync(CancellationToken.None))
		{
			unitOfWork.Dungeons.Add(dungeon);
			await unitOfWork.CommitAsync(CancellationToken.None);
		}

		// Act. The same raw-SQL bypass, carrying a finite value, so the control differs from the case under test in
		// the stored value alone and not in the route it took.
		using var writer = new AppDbContext(Options);
		var updated = await writer.Database.ExecuteSqlRawAsync(
			"UPDATE PlacedObjects SET Position_X = {0}",
			[42.5d],
			CancellationToken.None);

		using var verify = new AppDbContext(Options);
		var reloaded = await new DungeonWriteRepository(verify).LoadAsync(
			dungeon.Id,
			CancellationToken.None);

		// Assert
		updated.Should().Be(1);

		var position = reloaded!.Floors.Single().Objects.Single().Position;

		position.X.Should().Be(42.5f);
		position.Y.Should().Be(7.25f);
	}

	/// <summary>
	///		Reads <c>Position_X</c> straight out of the store, so that what the raw-SQL write actually landed is read
	///		rather than inferred from the update having reported a row.
	/// </summary>
	/// <param name="context">A context over the fixture's one open connection.</param>
	/// <returns>
	///		The stored <c>Position_X</c> of the single <c>PlacedObjects</c> row.
	/// </returns>
	private static async Task<double> ReadStoredPositionXAsync(AppDbContext context)
	{
		var connection = context.Database.GetDbConnection();

		await using var command = connection.CreateCommand();
		command.CommandText = "SELECT Position_X FROM PlacedObjects";

		return Convert.ToDouble(
			await command.ExecuteScalarAsync(CancellationToken.None),
			System.Globalization.CultureInfo.InvariantCulture);
	}
}
