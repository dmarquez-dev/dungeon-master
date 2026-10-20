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

		// INFRA-002 / MASTER-152. The three max-length declarations below are MODEL METADATA ONLY -- the store
		// enforces none of them. EF maps all three columns as TEXT carrying only a `maxLength:` annotation,
		// and SQLite ignores a length argument on a declared type: "numeric arguments in parentheses that
		// following the type name (ex: VARCHAR(255)) are ignored by SQLite ... SQLite does not impose any
		// length restrictions (other than the large global SQLITE_MAX_LENGTH limit) on the length of
		// strings" -- SQLite datatype documentation, at sqlite.org/datatype3.html. No CHECK constraint is
		// generated either, and that half IS measured
		// here: `grep -o 'CHECK' *.cs` over Migrations/ returns 0, and `grep -rn 'HasCheckConstraint' src`
		// returns 0. What this session did NOT measure is the engine itself -- no over-length insert was
		// attempted anywhere in it -- so the length-limit claim rests on the cited documentation.
		//
		// Each bound therefore holds only where some layer applies it, and most are applied nowhere.
		// Measured over the nine max-length declarations in the Dungeon, Floor and Area configurations,
		// three in each: exactly three are additionally bounded by an Application validator, and those are
		// the Name columns at 100. The rest are bounded by NOTHING IN ANY LAYER -- not the store, not the
		// Application layer, not the Domain. In this file, Floor.Name is the bounded one, through
		// AddFloorCommandValidator and UpdateFloorCommandValidator, while Floor.Description (2000) and
		// Floor.Notes (2000) ARE NOT BOUNDED ANYWHERE: either will persist a string of any length. Six of
		// nine columns are in that state.
		//
		// Commands. `grep -rn "MaximumLength" src --include=*.cs`, excluding /obj/ and /bin/, returns six
		// rules, every one MaximumLength(100) and every one on a Name; a seventh hit, in
		// NameOnlyValidatorTestBase, is doc prose rather than a rule and is not a guard.
		// `grep -rn "Description\|Notes" src --include=*Validator.cs` over all 14 validator files returns
		// 0. `grep -rn "Length" src/DungeonMaster.Domain` returns nothing. The nine was confirmed against
		// a second population that could not share the first predicate's mistake: `grep -o maxLength` over
		// Migrations/ gives 8 + 1 + 0 across the three released migrations, and the model snapshot carries
		// nine.
		//
		// The negative case this comment exists to prevent. MASTER-152 and INFRA-002 both record that
		// these limits are "enforced today by exactly one thing -- the FluentValidation validators in the
		// Application layer". Measured above, that is FALSE in two ways at once: the validators bound
		// three of these columns and not all of them, and there are nine columns, not the eleven the row
		// counts. Do not read a max-length declaration here as evidence that a value is bounded, and do not
		// restore that sentence from the register row.
		//
		// Closing the six-column gap is deliberately not done here. The documentation branch was chosen at
		// Gate 1 and reaffirmed at Gate 2; the gap is carried to the backlog. This change adds no
		// validator rule, no CHECK constraint, no Domain guard and no migration.
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
