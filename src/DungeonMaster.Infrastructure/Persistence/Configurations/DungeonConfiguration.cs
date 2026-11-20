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
		// generated either: `grep -rn 'CHECK' src/DungeonMaster.Infrastructure/Migrations` returns 0. The
		// length-limit claim rests on the cited documentation, not on a measured over-length insert.
		//
		// A declaration here is therefore not evidence that a value is bounded. Each of these three bounds
		// is applied in the Application layer, by a FluentValidation MaximumLength rule whose limit equals
		// the HasMaxLength argument below:
		// - Dungeon.Name at 100, by CreateDungeonCommandValidator and UpdateDungeonCommandValidator;
		// - Dungeon.Description at 2000, by CreateDungeonCommandValidator and UpdateDungeonCommandValidator;
		// - Dungeon.Notes at 4000, by UpdateDungeonCommandValidator (CreateDungeonCommand has no Notes).
		// A write that does not pass through those validators is not length-checked by them. Change a
		// limit here and in its validator rules together.
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
