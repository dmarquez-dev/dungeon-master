using DungeonMaster.Domain.Entities.Dungeons;
using DungeonMaster.Domain.Entities.Floors;
using DungeonMaster.Domain.ValueObjects;
using DungeonMaster.Infrastructure.Persistence.Repositories;
using FluentAssertions;

namespace DungeonMaster.Tests.Infrastructure.Persistence.Repositories;

/// <summary>
///		Pins the floor-scoped read, <c>DungeonReadRepository.GetFloorDetailAsync</c>: it returns one floor of one dungeon,
///		projected exactly as the full dungeon detail projects that floor, and <see langword="null"/> whenever the dungeon
///		does not own the floor.
/// </summary>
public sealed class DungeonReadRepositoryFloorDetailTests : SqliteInMemoryTestBase
{
	private const string TargetFloorName = "Undercroft";
	private const string TargetFloorDescription = "Damp stone and dripping water.";
	private const string TargetFloorNotes = "Secret door behind the altar.";
	private const string TargetAreaName = "Ossuary";
	private const ObjectKind TargetObjectKind = ObjectKind.DoorWood;

	/// <summary>
	///		Verifies that the floor read on its own is equivalent, member for member, to the same floor read as part of the full dungeon detail.
	/// </summary>
	[Fact]
	public async Task GetFloorDetailAsync_ForAFloorTheDungeonOwns_ReturnsTheFloorEquivalentToItsElementInGetDetailAsync()
	{
		// Arrange
		var (dungeon, targetFloor) = await SeedDungeonWithTwoFloorsAsync();
		var repo = new DungeonReadRepository(CreateDbContextFactory());

		var detail = await repo.GetDetailAsync(dungeon.Id, CancellationToken.None);
		var expected = detail!.Floors.Single(floor => floor.Id == targetFloor.Id);

		// Act
		var result = await repo.GetFloorDetailAsync(dungeon.Id, targetFloor.Id, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result.Should().BeEquivalentTo(expected);
	}

	/// <summary>
	///		Verifies that the floor read on its own carries the literals the floor, its area and its placed object were seeded with.
	///		Asserted against the seeded literals rather than against <c>GetDetailAsync</c>, so a defect in a projection both reads share is caught too.
	/// </summary>
	[Fact]
	public async Task GetFloorDetailAsync_ForAFloorTheDungeonOwns_ReturnsTheSeededFloorAreaAndObjectValues()
	{
		// Arrange
		var (dungeon, targetFloor) = await SeedDungeonWithTwoFloorsAsync();
		var repo = new DungeonReadRepository(CreateDbContextFactory());

		// Act
		var result = await repo.GetFloorDetailAsync(dungeon.Id, targetFloor.Id, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		result!.Id.Should().Be(targetFloor.Id);
		result.Number.Should().Be(2);
		result.Name.Should().Be(TargetFloorName);
		result.Description.Should().Be(TargetFloorDescription);
		result.Notes.Should().Be(TargetFloorNotes);
		result.Areas.Should().ContainSingle()
			.Which.Name.Should().Be(TargetAreaName);
		result.Objects.Should().ContainSingle()
			.Which.Kind.Should().Be(TargetObjectKind);
	}

	/// <summary>
	///		Verifies that no floor is returned when no dungeon has the given dungeon identifier.
	/// </summary>
	[Fact]
	public async Task GetFloorDetailAsync_WhenNoDungeonHasTheIdentifier_ReturnsNull()
	{
		// Arrange
		var (_, targetFloor) = await SeedDungeonWithTwoFloorsAsync();
		var repo = new DungeonReadRepository(CreateDbContextFactory());

		// Act
		var result = await repo.GetFloorDetailAsync(Guid.NewGuid(), targetFloor.Id, CancellationToken.None);

		// Assert
		result.Should().BeNull();
	}

	/// <summary>
	///		Verifies that no floor is returned when the dungeon exists but owns no floor with the given floor identifier.
	/// </summary>
	[Fact]
	public async Task GetFloorDetailAsync_WhenTheDungeonOwnsNoFloorWithTheIdentifier_ReturnsNull()
	{
		// Arrange
		var (dungeon, _) = await SeedDungeonWithTwoFloorsAsync();
		var repo = new DungeonReadRepository(CreateDbContextFactory());

		// Act
		var result = await repo.GetFloorDetailAsync(dungeon.Id, Guid.NewGuid(), CancellationToken.None);

		// Assert
		result.Should().BeNull();
	}

	/// <summary>
	///		Verifies that a floor belonging to another dungeon is never returned through this dungeon's identifier.
	///		The same floor is first read through its own dungeon, so the <see langword="null"/> is shown to come from the
	///		ownership condition and not from a floor that could not be read at all.
	/// </summary>
	[Fact]
	public async Task GetFloorDetailAsync_ForAFloorOfADifferentDungeon_ReturnsNull()
	{
		// Arrange
		var dungeonA = new Dungeon("Castle", null);
		dungeonA.AddFloor("Great Hall");
		var dungeonB = new Dungeon("Labyrinth", null);
		var floorOfB = dungeonB.AddFloor("Maze");

		using (var ctx = CreateContext())
		{
			await ctx.Dungeons.AddRangeAsync(dungeonA, dungeonB);
			await ctx.SaveChangesAsync();
		}

		var repo = new DungeonReadRepository(CreateDbContextFactory());

		var throughOwner = await repo.GetFloorDetailAsync(dungeonB.Id, floorOfB.Id, CancellationToken.None);
		throughOwner.Should().NotBeNull();

		// Act
		var result = await repo.GetFloorDetailAsync(dungeonA.Id, floorOfB.Id, CancellationToken.None);

		// Assert
		result.Should().BeNull();
	}

	/// <summary>
	///		Verifies that a token already cancelled before the call stops the floor query with <see cref="OperationCanceledException"/> rather than returning a projection.
	/// </summary>
	[Fact]
	public async Task GetFloorDetailAsync_WithAnAlreadyCancelledToken_ThrowsOperationCanceledException()
	{
		// Arrange
		var (dungeon, targetFloor) = await SeedDungeonWithTwoFloorsAsync();
		var repo = new DungeonReadRepository(CreateDbContextFactory());

		using var cts = new CancellationTokenSource();
		await cts.CancelAsync();

		// Act
		var act = async () =>
			await repo.GetFloorDetailAsync(dungeon.Id, targetFloor.Id, cts.Token);

		// Assert
		await act.Should().ThrowAsync<OperationCanceledException>();
	}

	/// <summary>
	///		Seeds one dungeon with two floors, each carrying its own area and placed object, and returns the dungeon and its second floor.
	/// </summary>
	/// <returns>
	///		The seeded dungeon and the floor the tests read, which carries the <c>Target*</c> literals.
	/// </returns>
	private async Task<(Dungeon Dungeon, Floor TargetFloor)> SeedDungeonWithTwoFloorsAsync()
	{
		var dungeon = new Dungeon("Sunken Temple", "A flooded ruin.");

		var otherFloor = dungeon.AddFloor("Antechamber");
		dungeon.UpdateFloor(otherFloor.Id, "Antechamber", "Other description.", "Other notes.");
		dungeon.AddArea(otherFloor.Id, "Vestibule", "Entry hall.");
		dungeon.AddPlacedObject(
			otherFloor.Id,
			ObjectKind.DoorWood,
			new CanvasPoint(1.5f, 2.5f),
			ObjectRotation.None,
			1.0f);

		var targetFloor = dungeon.AddFloor(TargetFloorName);
		dungeon.UpdateFloor(targetFloor.Id, TargetFloorName, TargetFloorDescription, TargetFloorNotes);
		dungeon.AddArea(targetFloor.Id, TargetAreaName, "Bones stacked to the ceiling.");
		dungeon.AddPlacedObject(
			targetFloor.Id,
			TargetObjectKind,
			new CanvasPoint(12.5f, 7.25f),
			ObjectRotation.Clockwise90,
			1.5f);

		using (var ctx = CreateContext())
		{
			await ctx.Dungeons.AddAsync(dungeon);
			await ctx.SaveChangesAsync();
		}

		return (dungeon, targetFloor);
	}
}
