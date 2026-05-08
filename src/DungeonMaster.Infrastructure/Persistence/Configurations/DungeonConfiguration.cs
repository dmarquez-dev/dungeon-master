using DungeonMaster.Domain.Entities.Dungeon;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DungeonMaster.Infrastructure.Persistence.Configurations;

/// <summary>
///		EF Core entity type configuration for <see cref="Dungeon"/>.
///		Defines table mapping, key, and column constraints.
/// </summary>
public sealed class DungeonConfiguration : IEntityTypeConfiguration<Dungeon>
{
	/// <summary>
	///		Configures the <see cref="Dungeon"/> entity schema.
	/// </summary>
	/// <param name="builder">
	///		The builder used to configure the entity type.
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
	}
}
