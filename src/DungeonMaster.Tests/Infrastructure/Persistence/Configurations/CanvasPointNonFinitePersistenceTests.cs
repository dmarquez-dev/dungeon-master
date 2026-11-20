using System.Data.Common;
using System.Globalization;
using DungeonMaster.Domain.Entities.Dungeons;
using DungeonMaster.Domain.Entities.Floors;
using DungeonMaster.Domain.ValueObjects;
using DungeonMaster.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DungeonMaster.Tests.Infrastructure.Persistence.Configurations;

/// <summary>
///		Pins what the store actually does with a non-finite coordinate in the <c>Position</c> mapping at
///		<c>PlacedObjectConfiguration.cs:43-54</c>, where <c>OwnsOne</c> maps <c>X</c> and <c>Y</c> into
///		<c>Position_X</c> and <c>Position_Y</c>, both <c>REAL</c> and both <c>IsRequired()</c>.
///		The three non-finite values do <b>not</b> behave alike, so no single statement covers them:
///		<list type="bullet">
///			<item>
///				<description>
///					<see cref="float.NaN"/> cannot be written. <see cref="DirectParameterBind_OfANaN_IsRefusedByTheProvider"/>
///					establishes it: an <see cref="InvalidOperationException"/> whose message is
///					<c>Cannot store 'NaN' values.</c>, thrown from <c>Microsoft.Data.Sqlite.SqliteValueBinder.BindDouble</c>.
///					The refusal is the <b>ADO layer</b> (<c>Microsoft.Data.Sqlite</c>) declining to bind the parameter,
///					before any SQL is executed. It is <b>not</b> a SQLite constraint and <b>not</b> the
///					<c>IsRequired()</c> mapping — a <c>REAL NOT NULL</c> violation would instead surface as a
///					<c>SqliteException</c> carrying a constraint code. The <c>REAL</c> column type is nonetheless why a
///					floating-point bind is attempted at all, so it is not irrelevant: a <c>TEXT</c> mapping would store
///					<see cref="float.NaN"/> without complaint.
///				</description>
///			</item>
///			<item>
///				<description>
///					<see cref="float.PositiveInfinity"/> and <see cref="float.NegativeInfinity"/> round-trip intact.
///					<see cref="RawSqlWrite_ThenDirectSelect_RoundTripsAnInfiniteCoordinate"/> establishes it at the
///					storage layer: a direct <c>SELECT</c> returns the same infinity in the same coordinate and
///					<c>typeof()</c> returns <c>real</c> — not <c>NULL</c>, not clamped, not a text sentinel.
///				</description>
///			</item>
///		</list>
///		So the store stops exactly <b>one</b> of the three cases and stores the other two. The statement "SQLite rejects
///		non-finite floats" would be wrong in both directions and is not what these tests measure.
///		<para>
///			<b>Why these tests no longer route through <c>Dungeon.AddPlacedObject</c> and
///			<c>IUnitOfWork.CommitAsync</c>.</b> The original form of this class constructed
///			<c>new CanvasPoint(x, y)</c> with a non-finite <c>x</c> or <c>y</c> and committed through the unit of work.
///			The finiteness guard added to <see cref="CanvasPoint"/> (see <c>CanvasPoint.cs</c>, MASTER-081 / CORE-016)
///			now throws an <see cref="ArgumentOutOfRangeException"/> from the constructor and from the <c>init</c>
///			accessors, so such a value is rejected in Arrange and never reaches EF at all. The route was therefore
///			moved <b>beneath</b> the domain layer — raw SQL in, a direct <c>SELECT</c> out — rather than the file being
///			retired. This is <b>not</b> weakened coverage: what the original tests established is that the refusal
///			happens at the ADO layer, and a direct parameter bind measures that layer, where routing through the domain
///			and EF measured it through two layers that now reject the value first.
///		</para>
///		<para>
///			<b>NO LONGER REACHABLE.</b> One part of the original observation is lost and is recorded here rather than
///			dropped: <b>EF Core's wrapping of the ADO refusal in a <c>DbUpdateException</c></b> can no longer be
///			observed. The original test asserted a <c>DbUpdateException</c> whose <c>InnerException</c> was the
///			<see cref="InvalidOperationException"/> above, which required EF to attempt to persist a non-finite
///			coordinate held by a <see cref="CanvasPoint"/>. The domain type can no longer hold such a value, so there
///			is no longer any way to make EF attempt that persist, and the wrapping has no reachable trigger. The inner
///			refusal itself is still measured, directly, by
///			<see cref="DirectParameterBind_OfANaN_IsRefusedByTheProvider"/>; only the EF wrapper around it is gone.
///		</para>
/// </summary>
public sealed class CanvasPointNonFinitePersistenceTests : SqliteInMemoryTestBase
{
	/// <summary>
	///		Writes an infinity straight into <c>Position_X</c> and <c>Position_Y</c> through raw SQL, bypassing the
	///		domain guard, and reads it back with a direct <c>SELECT</c> over the fixture's own connection.
	///		The read deliberately does <b>not</b> go through <c>DungeonWriteRepository.LoadAsync</c>: a load would bind
	///		the owned <see cref="CanvasPoint"/> through its constructor and the guard would throw, which is what
	///		<c>CanvasPointGuardMaterializationTests.cs</c> asserts. Only a direct <c>SELECT</c> shows what the
	///		<i>store</i> holds, as distinct from what the domain will accept back.
	/// </summary>
	[Theory]
	[InlineData(float.PositiveInfinity, float.PositiveInfinity)]
	[InlineData(float.NegativeInfinity, float.NegativeInfinity)]
	[InlineData(float.PositiveInfinity, 7.25f)]
	[InlineData(12.5f, float.NegativeInfinity)]
	public async Task RawSqlWrite_ThenDirectSelect_RoundTripsAnInfiniteCoordinate(float x, float y)
	{
		// Arrange. The row is seeded through the domain with a finite position, because the guard is precisely what
		// stops a non-finite one from getting in that way.
		await SeedOneFinitePlacedObjectAsync();

		// Act
		using var writer = new AppDbContext(Options);
		var updated = await writer.Database.ExecuteSqlRawAsync(
			"UPDATE PlacedObjects SET Position_X = {0}, Position_Y = {1}",
			[x, y],
			CancellationToken.None);

		var stored = await ReadStoredPositionAsync(writer);

		// Assert. The bypass is itself asserted rather than assumed: were the write to land as a NULL, a 0 or nothing
		// at all, the read below would be reporting on an arrangement that never happened.
		updated.Should().Be(1);

		// typeof() is the storage-layer evidence a value comparison alone does not reach. A text sentinel would still
		// compare equal after conversion, so the stored affinity is asserted separately.
		stored.TypeOfX.Should().Be("real");
		stored.TypeOfY.Should().Be("real");

		stored.X.Should().Be((double)x);
		stored.Y.Should().Be((double)y);
	}

	/// <summary>
	///		Binds <see cref="float.NaN"/> as a parameter directly through <c>Microsoft.Data.Sqlite</c>, which is the
	///		layer that refuses it, and asserts the observed exception type and message. A provider upgrade that started
	///		accepting <see cref="float.NaN"/> fails here rather than passing silently.
	/// </summary>
	[Fact]
	public async Task DirectParameterBind_OfANaN_IsRefusedByTheProvider()
	{
		// Arrange
		await SeedOneFinitePlacedObjectAsync();

		using var context = new AppDbContext(Options);
		var connection = context.Database.GetDbConnection();

		await using DbCommand command = connection.CreateCommand();
		command.CommandText = "UPDATE PlacedObjects SET Position_X = $x";

		var parameter = command.CreateParameter();
		parameter.ParameterName = "$x";
		parameter.Value = float.NaN;
		command.Parameters.Add(parameter);

		// Act
		var bind = async () => await command.ExecuteNonQueryAsync(CancellationToken.None);

		// Assert. ThrowExactly, because the type is load-bearing: the refusal is an InvalidOperationException raised
		// while binding, not a SqliteException carrying a constraint code, and that distinction is the whole
		// attribution. No EF wrapper is present or expected here — nothing EF owns is on this call path.
		var thrown = await bind.Should().ThrowExactlyAsync<InvalidOperationException>();

		thrown.Which.Message.Should().Be("Cannot store 'NaN' values.");

		// And the refusal is a refusal to write, not a partial one: the seeded finite value is still in the column.
		(await ReadStoredPositionAsync(context)).X.Should().Be(12.5d);
	}

	/// <summary>
	///		Positive control. Without it a run in which every case above failed could be misread as the provider
	///		rejecting values wholesale, when it would in fact only show the fixture or the raw-SQL route broken. It
	///		differs from the cases under test in the written value alone, not in the route it takes.
	/// </summary>
	[Fact]
	public async Task RawSqlWrite_ThenDirectSelect_KeepsAFiniteCoordinateIntact()
	{
		// Arrange
		await SeedOneFinitePlacedObjectAsync();

		// Act
		using var writer = new AppDbContext(Options);
		var updated = await writer.Database.ExecuteSqlRawAsync(
			"UPDATE PlacedObjects SET Position_X = {0}, Position_Y = {1}",
			[42.5f, -8.75f],
			CancellationToken.None);

		var stored = await ReadStoredPositionAsync(writer);

		// Assert
		updated.Should().Be(1);

		stored.TypeOfX.Should().Be("real");
		stored.TypeOfY.Should().Be("real");

		stored.X.Should().Be(42.5d);
		stored.Y.Should().Be(-8.75d);
	}

	/// <summary>
	///		Seeds exactly one <c>PlacedObjects</c> row with a finite position, through the domain and the real unit of
	///		work, so that the raw-SQL writes above have a row to target and the <c>SELECT</c>s have one to return.
	/// </summary>
	private async Task SeedOneFinitePlacedObjectAsync()
	{
		var dungeon = new Dungeon("Sunken Temple", "A flooded ruin.");
		var floor = dungeon.AddFloor(null);

		dungeon.AddPlacedObject(
			floor.Id,
			ObjectKind.DoorWood,
			new CanvasPoint(12.5f, 7.25f),
			ObjectRotation.None,
			1.0f);

		await using var unitOfWork = await CreateFactory().CreateAsync(CancellationToken.None);
		unitOfWork.Dungeons.Add(dungeon);
		await unitOfWork.CommitAsync(CancellationToken.None);
	}

	/// <summary>
	///		Reads the stored coordinates and their SQLite storage classes straight off the fixture's connection, so that
	///		what the store holds is read rather than inferred from a write having reported a row.
	/// </summary>
	/// <param name="context">A context over the fixture's one open connection.</param>
	/// <returns>
	///		The stored <c>Position_X</c> and <c>Position_Y</c> of the single <c>PlacedObjects</c> row, each with the
	///		<c>typeof()</c> SQLite reports for it.
	/// </returns>
	private static async Task<(double X, double Y, string TypeOfX, string TypeOfY)> ReadStoredPositionAsync(
		AppDbContext context)
	{
		var connection = context.Database.GetDbConnection();

		await using DbCommand command = connection.CreateCommand();
		command.CommandText =
			"SELECT Position_X, Position_Y, typeof(Position_X), typeof(Position_Y) FROM PlacedObjects";

		await using var reader = await command.ExecuteReaderAsync(CancellationToken.None);

		(await reader.ReadAsync(CancellationToken.None)).Should().BeTrue();

		return (
			Convert.ToDouble(reader.GetValue(0), CultureInfo.InvariantCulture),
			Convert.ToDouble(reader.GetValue(1), CultureInfo.InvariantCulture),
			reader.GetString(2),
			reader.GetString(3));
	}
}
