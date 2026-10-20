using DungeonMaster.Domain.Entities.Dungeons;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DungeonMaster.Infrastructure.Persistence.Configurations;

/// <summary>
///		Configures the EF Core entity mapping for <see cref="Dungeon"/>.
/// </summary>
public sealed class DungeonConfiguration
	: IEntityTypeConfiguration<Dungeon>
{
	/// <summary>
	///		Applies the column mappings, constraints, and relationships for the Dungeons table.
	/// </summary>
	/// <param name="builder">
	///		The builder used to configure the <see cref="Dungeon"/> entity type.
	/// </param>
	public void Configure(EntityTypeBuilder<Dungeon> builder)
	{
		builder.ToTable("Dungeons");

		builder.HasKey(dungeon =>
			dungeon.Id);

		builder.Property(dungeon =>
			dungeon.Id)
			.ValueGeneratedNever();

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
		// Application layer, not the Domain. In this file, Dungeon.Name is the bounded one, while
		// Dungeon.Description (2000) and Dungeon.Notes (4000) ARE NOT BOUNDED ANYWHERE: either will
		// persist a string of any length. Six of nine columns are in that state.
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
		builder.Property(dungeon =>
			dungeon.Name)
			.IsRequired()
			.HasMaxLength(100);

		builder.Property(dungeon =>
			dungeon.Description)
			.HasMaxLength(2000);

		builder.Property(dungeon =>
			dungeon.Notes)
			.HasMaxLength(4000);

		builder.Property(dungeon =>
			dungeon.CreatedAt)
			.IsRequired();

		builder.Property(dungeon =>
			dungeon.LastModified)
			.IsRequired();

		builder.HasMany(dungeon =>
			dungeon.Floors)
			.WithOne()
			.HasForeignKey(floor =>
				floor.DungeonId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.Navigation(dungeon =>
			dungeon.Floors)
			.HasField("_floors")
			.UsePropertyAccessMode(PropertyAccessMode.Field);
	}
}
