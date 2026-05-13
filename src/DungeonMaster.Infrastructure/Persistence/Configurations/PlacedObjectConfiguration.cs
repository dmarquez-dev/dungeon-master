using DungeonMaster.Domain.Entities.Area;
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
    ///		Applies the column mappings, constraints, and owned-entity configuration for the PlacedObjects table.
    /// </summary>
    /// <param name="builder">
    ///		The builder used to configure the <see cref="PlacedObject"/> entity type.
    /// </param>
    public void Configure(EntityTypeBuilder<PlacedObject> builder)
    {
        builder.ToTable("PlacedObjects");

        builder.HasKey(obj =>
            obj.Id);

        builder.Property(obj =>
            obj.Id)
            .ValueGeneratedNever();

        builder.Property(obj =>
            obj.AreaId)
            .IsRequired();

        builder.Property(obj =>
            obj.Kind)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(obj =>
            obj.Anchor)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(obj =>
            obj.Rotation)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(obj =>
            obj.CreatedAt)
            .IsRequired();

        builder.Property(obj =>
            obj.LastModified)
            .IsRequired();

        builder.OwnsOne(obj =>
            obj.Position,
            position =>
            {
                position.Property(point =>
                    point.Col)
                    .HasColumnName("Position_Col")
                    .IsRequired();

                position.Property(point =>
                    point.Row)
                    .HasColumnName("Position_Row")
                    .IsRequired();
            });
    }
}
