using DungeonMaster.Domain.Entities.Floors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DungeonMaster.Infrastructure.Persistence.Configurations;

/// <summary>
///		Configures the EF Core entity mapping for <see cref="Floor"/>.
/// </summary>
public sealed class FloorConfiguration
	: IEntityTypeConfiguration<Floor>
{
	/// <summary>
	///		Applies the column mappings, constraints, and relationships for the Floors table.
	/// </summary>
	/// <param name="builder">
	///		The builder used to configure the <see cref="Floor"/> entity type.
	/// </param>
	public void Configure(EntityTypeBuilder<Floor> builder)
	{
		builder.ToTable("Floors");

		builder.HasKey(floor =>
			floor.Id);

		builder.Property(floor =>
			floor.Id)
			.ValueGeneratedNever();

		builder.Property(floor =>
			floor.DungeonId)
			.IsRequired();

		builder.Property(floor =>
			floor.Number)
			.IsRequired();

		// INFRA-002 / MASTER-152. Each max-length declaration below is MODEL METADATA ONLY -- the store
		// enforces none of them. EF maps each of these columns as TEXT carrying only a `maxLength:`
		// annotation, and SQLite ignores a length argument on a declared type: "numeric arguments in
		// parentheses that following the type name (ex: VARCHAR(255)) are ignored by SQLite ... SQLite does
		// not impose any length restrictions (other than the large global SQLITE_MAX_LENGTH limit) on the
		// length of strings" -- SQLite datatype documentation, at sqlite.org/datatype3.html. No CHECK
		// constraint is generated either: `grep -rn 'CHECK' src/DungeonMaster.Infrastructure/Migrations`
		// returns 0. The length-limit claim rests on the cited documentation, not on a measured
		// over-length insert.
		//
		// A declaration here is therefore not evidence that a value is bounded. Each of these bounds is
		// applied in the Application layer, by a FluentValidation MaximumLength rule whose limit equals
		// the HasMaxLength argument below:
		// - Floor.Name at 100, by AddFloorCommandValidator and UpdateFloorCommandValidator;
		// - Floor.Description at 2000, by UpdateFloorCommandValidator (AddFloorCommand has no Description);
		// - Floor.Notes at 2000, by UpdateFloorCommandValidator (AddFloorCommand has no Notes).
		// A write that does not pass through those validators is not length-checked by them. Change a
		// limit here and in its validator rules together.
		builder.Property(floor =>
			floor.Name)
			.HasMaxLength(100);

		builder.Property(floor =>
			floor.Description)
			.HasMaxLength(2000);

		builder.Property(floor =>
			floor.Notes)
			.HasMaxLength(2000);

		builder.Property(floor =>
			floor.CreatedAt)
			.IsRequired();

		builder.Property(floor =>
			floor.LastModified)
			.IsRequired();

		// INFRA-005 / MASTER-156. This index on (DungeonId, Number) is DELIBERATELY NOT UNIQUE, and
		// no uniqueness may be declared on it. Floor numbers are unique per dungeon as a business
		// invariant, so the missing constraint reads as an oversight. It is not one.
		//
		// Why a unique index would break. Dungeon.ReorderFloor, in DungeonMethods.cs in the Domain, shifts
		// a contiguous block of floors and only THEN sets the moved floor. Moving floor 3 to position 1
		// shifts floors 1 and 2 up to 2 and 3, so the numbering passes through {2, 3, 3} -- a duplicate
		// Number within one DungeonId -- before the moved floor is renumbered to 1. That duplicate state
		// is not transient within a statement: the whole mutation runs on the tracked graph and is flushed
		// by a SINGLE SaveChangesAsync, in UnitOfWork's CommitAsync, reached through
		// ReorderFloorCommandHandler, whose body is one unitOfWorkFactory.MutateAsync(...) call and
		// nothing else. EF is expected to issue the shifts as separate UPDATE statements inside that one
		// transaction, which would make the duplicate observable between statements, and a unique index is
		// believed to reject it there: SQLite's CREATE TABLE grammar places DEFERRABLE only inside the
		// foreign-key clause, so a UNIQUE constraint appears to have no deferred form -- see
		// sqlite.org/lang_createtable.html. Both of those last two are BELIEF, not local measurement.
		// This session measured the duplicate-passing code path, NOT SQLite's rejection of it: no
		// duplicate insert was attempted anywhere in it.
		//
		// History, ATTRIBUTED to this session's measurement record rather than re-derived here, since a
		// task cannot run git. The index WAS unique and was made non-unique on purpose. Commit e402ed8 --
		// the same commit that implemented drag-and-drop floor reordering -- added a
		// RemoveFloorNumberUniqueIndex migration whose Up recreated this index with no unique argument
		// while its Down restored unique: true. Commit 7a4bb2d then deleted that migration in the
		// baseline squash, which is why nothing under Migrations/ mentions it today.
		//
		// DISCIPLINE DEPARTURE, recorded so the absent constraint is not later read as a defect and
		// "corrected". This project's database discipline states under Indexing: "A uniqueness rule is
		// enforced by a constraint, never by an application check alone." This code knowingly departs from
		// that rule. What carries the invariant instead is ReorderFloor's own range check,
		// `if (newNumber < 1 || newNumber > _floors.Count)`, together with the shift-then-set ordering,
		// which renumbers the shifted block so the final numbering stays contiguous -- an application
		// check, which is precisely what the rule forbids relying on alone.
		//
		// Making the index unique is therefore NOT a small change. It needs the shift renumbered through a
		// disjoint range so that no intermediate state collides, which is a redesign of the reorder
		// command and is not covered by the row's stated effort of S. Reinstating uniqueness is separate
		// work, not a one-line addition here.
		builder.HasIndex(floor =>
			new { floor.DungeonId, floor.Number });

		builder.HasMany(floor =>
			floor.Areas)
			.WithOne()
			.HasForeignKey(area =>
				area.FloorId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.Navigation(floor =>
			floor.Areas)
			.HasField("_areas")
			.UsePropertyAccessMode(PropertyAccessMode.Field);

		builder.HasMany(floor =>
			floor.Objects)
			.WithOne()
			.HasForeignKey(obj =>
				obj.FloorId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.Navigation(floor =>
			floor.Objects)
			.HasField("_objects")
			.UsePropertyAccessMode(PropertyAccessMode.Field);
	}
}
