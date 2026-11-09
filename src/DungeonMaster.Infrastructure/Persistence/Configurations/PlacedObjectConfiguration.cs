using DungeonMaster.Domain.Entities.Floors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DungeonMaster.Infrastructure.Persistence.Configurations;

/// <summary>
///		Configures the EF Core entity mapping for <see cref="PlacedObject"/>.
/// </summary>
public sealed class PlacedObjectConfiguration
	: IEntityTypeConfiguration<PlacedObject>
{
	/// <summary>
	///		Applies the column mappings, constraints, value conversions, and owned-entity configuration for the PlacedObjects table.
	/// </summary>
	/// <param name="builder">
	///		The builder used to configure the <see cref="PlacedObject"/> entity type.
	/// </param>
	public void Configure(EntityTypeBuilder<PlacedObject> builder)
	{
		builder.ToTable("PlacedObjects");

		builder.HasKey(placedObject =>
			placedObject.Id);

		builder.Property(placedObject =>
			placedObject.Id)
			.ValueGeneratedNever();

		builder.Property(placedObject =>
			placedObject.FloorId)
			.IsRequired();

		builder.Property(placedObject =>
			placedObject.Kind)
			.IsRequired()
			.HasConversion<string>();

		builder.Property(placedObject =>
			placedObject.Rotation)
			.IsRequired()
			.HasConversion<string>();

		builder.Property(placedObject =>
			placedObject.Scale)
			.HasColumnType("REAL");

		builder.Property(placedObject =>
			placedObject.CreatedAt)
			.IsRequired();

		builder.Property(placedObject =>
			placedObject.LastModified)
			.IsRequired();

		builder.OwnsOne(
			placedObject =>
				placedObject.Position,
			position =>
			{
				position.Property(point =>
					point.X)
					.HasColumnName("Position_X")
					.HasColumnType("REAL")
					.IsRequired();

				position.Property(point =>
					point.Y)
					.HasColumnName("Position_Y")
					.HasColumnType("REAL")
					.IsRequired();
			});
	}
}
