using DungeonMaster.Domain.Entities.Area;
using DungeonMaster.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace DungeonMaster.Infrastructure.Persistence.Configurations;

/// <summary>
///		Configures the EF Core entity mapping for <see cref="Area"/>.
/// </summary>
public sealed class AreaConfiguration
	: IEntityTypeConfiguration<Area>
{
	private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.General);

	/// <summary>
	///		Applies the column mappings, constraints, owned-entity configuration, and value conversions for the Areas table.
	/// </summary>
	/// <param name="builder">
	///		The builder used to configure the <see cref="Area"/> entity type.
	/// </param>
	public void Configure(EntityTypeBuilder<Area> builder)
	{
		builder.ToTable("Areas");

		builder.HasKey(area =>
			area.Id);

		builder.Property(area =>
			area.Id)
			.ValueGeneratedNever();

		builder.Property(area =>
			area.FloorId)
			.IsRequired();

		builder.Property(area =>
			area.Name)
			.IsRequired()
			.HasMaxLength(100);

		builder.Property(area =>
			area.Description)
			.HasMaxLength(2000);

		builder.Property(area =>
			area.Notes)
			.HasMaxLength(2000);

		builder.Property(area =>
			area.CreatedAt)
			.IsRequired();

		builder.Property(area =>
			area.LastModified)
			.IsRequired();

		builder.OwnsOne(
			area =>
				area.Origin,
			origin =>
			{
				origin.Property(point =>
					point.Col)
					.HasColumnName("Origin_Col")
					.IsRequired();

				origin.Property(point =>
					point.Row)
					.HasColumnName("Origin_Row")
					.IsRequired();
			});

		builder.Property(area =>
			area.Outline)
			.HasColumnType("TEXT")
			.HasConversion(
				outline =>
					JsonSerializer.Serialize(
						outline,
						JsonOptions),
				raw =>
					(IReadOnlyList<PathSegment>)(string.IsNullOrEmpty(raw)
						? new List<PathSegment>()
						: JsonSerializer.Deserialize<List<PathSegment>>(
							raw,
							JsonOptions) ?? new List<PathSegment>()))
			.HasField("_outline")
			.UsePropertyAccessMode(PropertyAccessMode.PreferFieldDuringConstruction);
	}
}
