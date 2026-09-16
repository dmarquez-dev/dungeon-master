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
