using DungeonMaster.Domain.Entities.Dungeons;
using DungeonMaster.Domain.Entities.Floors;
using DungeonMaster.Domain.ValueObjects;
using DungeonMaster.Infrastructure.Persistence;
using DungeonMaster.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DungeonMaster.Tests.Infrastructure.Persistence.Configurations;

/// <summary>
///		Pins what the store actually does with a non-finite <see cref="CanvasPoint"/> coordinate written through the <c>Position</c> mapping at <c>PlacedObjectConfiguration.cs:43-54</c>, where <c>OwnsOne</c> maps <c>X</c> and <c>Y</c> into <c>Position_X</c> and <c>Position_Y</c>, both <c>REAL</c> and both <c>IsRequired()</c>.
///		The three non-finite values do <b>not</b> behave alike, so no single statement covers them:
///		<list type="bullet">
///			<item>
///				<description><see cref="float.NaN"/> throws on save. <see cref="CommitAsync_ForANaNCoordinate_ThrowsAndNothingIsStored"/> establishes it: <see cref="DbUpdateException"/> wrapping an <see cref="InvalidOperationException"/> whose message is <c>Cannot store 'NaN' values.</c>. The rejection comes from the ADO layer (<c>Microsoft.Data.Sqlite</c>) before any SQL is executed, not from a SQLite constraint, so the column type and <c>IsRequired()</c> play no part in it.</description>
///			</item>
///			<item>
///				<description><see cref="float.PositiveInfinity"/> and <see cref="float.NegativeInfinity"/> round-trip. <see cref="CommitAsync_ThenReload_RoundTripsAnInfiniteCoordinate"/> establishes it: the save completes and a reload returns the same infinity in the same coordinate. SQLite stores them as IEEE-754 8-byte doubles and nothing rejects, clamps or nulls them.</description>
///			</item>
///		</list>
///		So the persistence layer is <b>not</b> a guard against non-finite coordinates: it stops exactly one of the three cases, and does so with an exception that surfaces at commit time rather than at the boundary where the value was accepted.
/// </summary>
public sealed class CanvasPointNonFinitePersistenceTests : SqliteInMemoryTestBase
{
	[Theory]
	[InlineData(float.NaN, float.NaN)]
	[InlineData(float.NaN, 7.25f)]
	[InlineData(12.5f, float.NaN)]
	public async Task CommitAsync_ForANaNCoordinate_ThrowsAndNothingIsStored(float x, float y)
	{
		// Arrange
		var dungeon = new Dungeon("Sunken Temple", "A flooded ruin.");
		var floor = dungeon.AddFloor(null);

		dungeon.AddPlacedObject(
			floor.Id,
			ObjectKind.DoorWood,
			new CanvasPoint(x, y),
			ObjectRotation.None,
			1.0f);

		// Act
		var commit = async () =>
		{
			await using var unitOfWork = await CreateFactory().CreateAsync(CancellationToken.None);
			unitOfWork.Dungeons.Add(dungeon);
			await unitOfWork.CommitAsync(CancellationToken.None);
		};

		// Assert
		// No reload assertion follows, and none can: the observed outcome is a thrown
		// DbUpdateException wrapping InvalidOperationException("Cannot store 'NaN' values."),
		// so the row is never written and there is nothing to load back.
		var thrown = await commit.Should().ThrowAsync<DbUpdateException>();

		thrown.Which.InnerException
			.Should().BeOfType<InvalidOperationException>()
			.Which.Message.Should().Be("Cannot store 'NaN' values.");
	}

	[Theory]
	[InlineData(float.PositiveInfinity, float.PositiveInfinity)]
	[InlineData(float.NegativeInfinity, float.NegativeInfinity)]
	[InlineData(float.PositiveInfinity, 7.25f)]
	[InlineData(12.5f, float.NegativeInfinity)]
	public async Task CommitAsync_ThenReload_RoundTripsAnInfiniteCoordinate(float x, float y)
	{
		// Arrange
		var dungeon = new Dungeon("Sunken Temple", "A flooded ruin.");
		var floor = dungeon.AddFloor(null);

		dungeon.AddPlacedObject(
			floor.Id,
			ObjectKind.DoorWood,
			new CanvasPoint(x, y),
			ObjectRotation.None,
			1.0f);

		// Act
		await using (var unitOfWork = await CreateFactory().CreateAsync(CancellationToken.None))
		{
			unitOfWork.Dungeons.Add(dungeon);
			await unitOfWork.CommitAsync(CancellationToken.None);
		}

		using var verify = new AppDbContext(Options);
		var reloaded = await new DungeonWriteRepository(verify).LoadAsync(
			dungeon.Id,
			CancellationToken.None);

		// Assert
		var position = reloaded!.Floors.Single().Objects.Single().Position;

		position.X.Should().Be(x);
		position.Y.Should().Be(y);
	}

	/// <summary>
	///		Positive control. Without it a run in which every case above threw could be misread as the store rejecting non-finite values wholesale, when it would in fact only show the fixture broken.
	/// </summary>
	[Fact]
	public async Task CommitAsync_ThenReload_KeepsAFiniteCoordinateIntact()
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

		// Act
		await using (var unitOfWork = await CreateFactory().CreateAsync(CancellationToken.None))
		{
			unitOfWork.Dungeons.Add(dungeon);
			await unitOfWork.CommitAsync(CancellationToken.None);
		}

		using var verify = new AppDbContext(Options);
		var reloaded = await new DungeonWriteRepository(verify).LoadAsync(
			dungeon.Id,
			CancellationToken.None);

		// Assert
		var position = reloaded!.Floors.Single().Objects.Single().Position;

		position.X.Should().Be(12.5f);
		position.Y.Should().Be(7.25f);
	}
}
