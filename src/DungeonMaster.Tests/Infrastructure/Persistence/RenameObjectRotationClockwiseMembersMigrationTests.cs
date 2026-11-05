using DungeonMaster.Domain.Entities.Floors;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DungeonMaster.Tests.Infrastructure.Persistence;

/// <summary>
///		Proves the <c>RenameObjectRotationClockwiseMembers</c> data migration in both directions (MASTER-130 / CORE-024).
///		<see cref="ObjectRotation"/> is persisted by member name, so renaming <c>CW90</c>, <c>CW180</c> and <c>CW270</c> to <c>Clockwise90</c>, <c>Clockwise180</c> and <c>Clockwise270</c> must rewrite the stored values.
///		Each test runs over its own in-memory database, which the base class migrates to latest, and moves the schema itself with <see cref="IMigrator"/>.
///		Rows enter the old-value state by raw SQL, because the renamed enum can no longer write <c>CW90</c>.
/// </summary>
public sealed class RenameObjectRotationClockwiseMembersMigrationTests : SqliteInMemoryTestBase
{
	/// <summary>
	///		The migration immediately before <c>RenameObjectRotationClockwiseMembers</c>, whose schema stores the old member names.
	/// </summary>
	private const string PreviousMigration = "20260818232300_UtcDateTimeTimestampsAndScaleWithoutStoreDefault";

	/// <summary>
	///		A fixed timestamp in the stored <c>DateTime</c> text form, so no test reads the wall clock.
	/// </summary>
	private const string Timestamp = "2026-11-05 12:00:00";

	/// <summary>
	///		Verifies that migrating up from the previous migration rewrites each old stored rotation name to its new member name and leaves <c>None</c> as it is.
	/// </summary>
	[Fact]
	public void MigratingUp_FromRowsHoldingTheOldNames_RewritesEachToItsClockwiseName()
	{
		// Arrange
		MigrateTo(PreviousMigration);
		var floorId = InsertFloor();
		var none = InsertPlacedObject(floorId, "None");
		var cw90 = InsertPlacedObject(floorId, "CW90");
		var cw180 = InsertPlacedObject(floorId, "CW180");
		var cw270 = InsertPlacedObject(floorId, "CW270");

		// Act
		MigrateToLatest();

		// Assert
		ReadRotation(none).Should().Be("None", "the None row is outside the rewrite");
		ReadRotation(cw90).Should().Be("Clockwise90", "the CW90 row is rewritten to Clockwise90");
		ReadRotation(cw180).Should().Be("Clockwise180", "the CW180 row is rewritten to Clockwise180");
		ReadRotation(cw270).Should().Be("Clockwise270", "the CW270 row is rewritten to Clockwise270");
	}

	/// <summary>
	///		Verifies that after migrating up, EF materialises each rewritten row as the matching <see cref="ObjectRotation"/> member.
	/// </summary>
	[Fact]
	public void MigratingUp_FromRowsHoldingTheOldNames_YieldsRowsEfMaterialisesAsEachMember()
	{
		// Arrange
		MigrateTo(PreviousMigration);
		var floorId = InsertFloor();
		var none = InsertPlacedObject(floorId, "None");
		var cw90 = InsertPlacedObject(floorId, "CW90");
		var cw180 = InsertPlacedObject(floorId, "CW180");
		var cw270 = InsertPlacedObject(floorId, "CW270");

		// Act
		MigrateToLatest();

		// Assert
		using var context = CreateContext();
		var rotations = context.Set<PlacedObject>()
			.AsNoTracking()
			.ToDictionary(
				placedObject =>
					placedObject.Id,
				placedObject =>
					placedObject.Rotation);

		rotations[none].Should().Be(ObjectRotation.None);
		rotations[cw90].Should().Be(ObjectRotation.Clockwise90);
		rotations[cw180].Should().Be(ObjectRotation.Clockwise180);
		rotations[cw270].Should().Be(ObjectRotation.Clockwise270);
	}

	/// <summary>
	///		Verifies that migrating down to the previous migration restores each new member name to its old stored name and leaves <c>None</c> as it is.
	/// </summary>
	[Fact]
	public void MigratingDown_FromRowsHoldingEachMember_RestoresEachOldStoredName()
	{
		// Arrange
		var floorId = InsertFloor();
		var none = InsertPlacedObject(floorId, nameof(ObjectRotation.None));
		var clockwise90 = InsertPlacedObject(floorId, nameof(ObjectRotation.Clockwise90));
		var clockwise180 = InsertPlacedObject(floorId, nameof(ObjectRotation.Clockwise180));
		var clockwise270 = InsertPlacedObject(floorId, nameof(ObjectRotation.Clockwise270));

		// Act
		MigrateTo(PreviousMigration);

		// Assert
		ReadRotation(none).Should().Be("None", "the None row is outside the rewrite");
		ReadRotation(clockwise90).Should().Be("CW90", "the Clockwise90 row is restored to CW90");
		ReadRotation(clockwise180).Should().Be("CW180", "the Clockwise180 row is restored to CW180");
		ReadRotation(clockwise270).Should().Be("CW270", "the Clockwise270 row is restored to CW270");
	}

	/// <summary>
	///		Verifies that migrating up leaves a row already holding a new member name untouched.
	/// </summary>
	[Fact]
	public void MigratingUp_WhenARowAlreadyHoldsClockwise90_LeavesItAtClockwise90()
	{
		// Arrange
		MigrateTo(PreviousMigration);
		var floorId = InsertFloor();
		var clockwise90 = InsertPlacedObject(floorId, "Clockwise90");

		// Act
		MigrateToLatest();

		// Assert
		ReadRotation(clockwise90).Should().Be("Clockwise90");
	}

	/// <summary>
	///		Verifies that migrating down leaves a row already holding an old stored name untouched.
	/// </summary>
	[Fact]
	public void MigratingDown_WhenARowAlreadyHoldsCW90_LeavesItAtCW90()
	{
		// Arrange
		var floorId = InsertFloor();
		var cw90 = InsertPlacedObject(floorId, "CW90");

		// Act
		MigrateTo(PreviousMigration);

		// Assert
		ReadRotation(cw90).Should().Be("CW90");
	}

	/// <summary>
	///		Moves this test's database to the named migration, applying or reverting whatever lies between.
	/// </summary>
	/// <param name="targetMigration">
	///		The migration ID to finish at.
	/// </param>
	private void MigrateTo(string targetMigration)
	{
		using var context = CreateContext();
		context.GetService<IMigrator>().Migrate(targetMigration);
	}

	/// <summary>
	///		Applies every migration this test's database does not yet have.
	/// </summary>
	private void MigrateToLatest()
	{
		using var context = CreateContext();
		context.Database.Migrate();
	}

	/// <summary>
	///		Inserts a dungeon and one floor in it by raw SQL, so placed objects have a floor to belong to.
	/// </summary>
	/// <returns>
	///		The floor's ID.
	/// </returns>
	private Guid InsertFloor()
	{
		var dungeonId = NewStoredId();
		var floorId = NewStoredId();

		using var context = CreateContext();
		context.Database.ExecuteSql(
			$"""
			INSERT INTO "Dungeons" ("Id", "Name", "CreatedAt", "LastModified")
			VALUES ({dungeonId}, {"Sunken Temple"}, {Timestamp}, {Timestamp});
			""");

		context.Database.ExecuteSql(
			$"""
			INSERT INTO "Floors" ("Id", "DungeonId", "Number", "CreatedAt", "LastModified")
			VALUES ({floorId}, {dungeonId}, {1}, {Timestamp}, {Timestamp});
			""");

		return Guid.Parse(floorId);
	}

	/// <summary>
	///		Inserts a placed object by raw SQL with the given stored rotation text.
	/// </summary>
	/// <param name="floorId">
	///		The floor the object is placed on.
	/// </param>
	/// <param name="rotation">
	///		The exact text to store in the <c>Rotation</c> column.
	/// </param>
	/// <returns>
	///		The placed object's ID.
	/// </returns>
	private Guid InsertPlacedObject(
		Guid floorId,
		string rotation)
	{
		var placedObjectId = NewStoredId();
		var storedFloorId = ToStoredId(floorId);

		using var context = CreateContext();
		context.Database.ExecuteSql(
			$"""
			INSERT INTO "PlacedObjects" ("Id", "FloorId", "Kind", "Position_X", "Position_Y", "Rotation", "Scale", "CreatedAt", "LastModified")
			VALUES ({placedObjectId}, {storedFloorId}, {"DoorWood"}, {1.0}, {2.0}, {rotation}, {1.0}, {Timestamp}, {Timestamp});
			""");

		return Guid.Parse(placedObjectId);
	}

	/// <summary>
	///		Reads a placed object's stored rotation text by raw SQL, bypassing the enum conversion.
	/// </summary>
	/// <param name="placedObjectId">
	///		The placed object to read.
	/// </param>
	/// <returns>
	///		The exact text in the <c>Rotation</c> column.
	/// </returns>
	private string ReadRotation(Guid placedObjectId)
	{
		var storedId = ToStoredId(placedObjectId);

		using var context = CreateContext();
		return context.Database
			.SqlQuery<string>(
				$"""
				SELECT "Rotation" AS "Value" FROM "PlacedObjects" WHERE "Id" = {storedId}
				""")
			.AsEnumerable()
			.Single();
	}

	/// <summary>
	///		Creates a new ID in the text form EF stores a <see cref="Guid"/> in under SQLite.
	/// </summary>
	/// <returns>
	///		The ID as stored text.
	/// </returns>
	private static string NewStoredId() =>
		ToStoredId(Guid.NewGuid());

	/// <summary>
	///		Converts an ID to the text form EF stores a <see cref="Guid"/> in under SQLite.
	/// </summary>
	/// <param name="id">
	///		The ID to convert.
	/// </param>
	/// <returns>
	///		The ID as upper-case hyphenated text.
	/// </returns>
	private static string ToStoredId(Guid id) =>
		id.ToString().ToUpperInvariant();
}
