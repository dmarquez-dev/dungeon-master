using DungeonMaster.Domain.Entities.Dungeon;
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

		builder.Property(dungeon =>
			dungeon.Name)
			.IsRequired()
			.HasMaxLength(100);

		builder.Property(dungeon =>
			dungeon.Description)
			.HasMaxLength(2000);

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
