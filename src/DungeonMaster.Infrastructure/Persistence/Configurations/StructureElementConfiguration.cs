using System.Text.Json;
using System.Text.Json.Serialization;
using DungeonMaster.Domain.Entities.Area;
using DungeonMaster.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DungeonMaster.Infrastructure.Persistence.Configurations;

public sealed class StructureElementConfiguration
    : IEntityTypeConfiguration<StructureElement>
{
    // Enums serialized as strings to match the Kind column convention.
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public void Configure(EntityTypeBuilder<StructureElement> builder)
    {
        builder.ToTable("StructureElements");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedNever();

        builder.Property(e => e.AreaId)
            .IsRequired();

        builder.Property(e => e.Kind)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.LastModified)
            .IsRequired();

        // Stored as JSON value-converted TEXT columns. Using HasConversion instead of
        // OwnsOne avoids the EF Core shadow-FK naming collision that occurs when two
        // OwnsOne navigations share the same CLR type (VertexPoint) on one entity,
        // which causes NavigationFixer to confuse their 'StructureElementId' shadow FKs.
        builder.Property(e => e.From)
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, _jsonOptions),
                v => v == null ? null : JsonSerializer.Deserialize<VertexPoint>(v, _jsonOptions))
            .HasColumnType("TEXT");

        builder.Property(e => e.To)
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, _jsonOptions),
                v => v == null ? null : JsonSerializer.Deserialize<VertexPoint>(v, _jsonOptions))
            .HasColumnType("TEXT");

        builder.Property(e => e.EdgeAnchor)
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, _jsonOptions),
                v => v == null ? null : JsonSerializer.Deserialize<EdgePoint>(v, _jsonOptions))
            .HasColumnType("TEXT");
    }
}
