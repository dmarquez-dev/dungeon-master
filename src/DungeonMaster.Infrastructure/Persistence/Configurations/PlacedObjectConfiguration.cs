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
	/// <inheritdoc/>
	public void Configure(EntityTypeBuilder<PlacedObject> builder)
	{
		builder.ToTable("PlacedObjects");

		builder.HasKey(obj => obj.Id);

		builder.Property(obj => obj.Id)
			.ValueGeneratedNever();

		builder.Property(obj => obj.FloorId)
			.IsRequired();

		builder.Property(obj => obj.Kind)
			.IsRequired()
			.HasConversion<string>();

		builder.Property(obj => obj.Rotation)
			.IsRequired()
			.HasConversion<string>();

		builder.Property(obj => obj.Scale)
			.HasColumnType("REAL");

		builder.Property(obj => obj.CreatedAt)
			.IsRequired();

		builder.Property(obj => obj.LastModified)
			.IsRequired();

		builder.OwnsOne(obj => obj.Position, position =>
		{
			position.Property(p => p.X)
				.HasColumnName("Position_X")
				.HasColumnType("REAL")
				.IsRequired();

			position.Property(p => p.Y)
				.HasColumnName("Position_Y")
				.HasColumnType("REAL")
				.IsRequired();
		});
	}
}
