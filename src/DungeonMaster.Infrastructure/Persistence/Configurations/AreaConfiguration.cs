using DungeonMaster.Domain.Entities.Areas;
using DungeonMaster.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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

	// INFRA-004 / MASTER-015. Without this comparer EF falls back to reference equality for the
	// converted reference type, and Area.Outline returns _outline.AsReadOnly() -- a fresh wrapper on
	// every read -- so DetectChanges marked Outline modified on every SaveChanges for every tracked
	// Area. PathSegment is a record, so its compiler-generated structural equality and GetHashCode
	// are what SequenceEqual and the aggregated hash compose here.
	private static readonly ValueComparer<IReadOnlyList<PathSegment>> OutlineComparer = new(
		// EF declares the equality lambda over the nullable form of the property type, so the two
		// null cases are handled here rather than by SequenceEqual, which rejects a null argument.
		(left, right) =>
			left == null
				? right == null
				: right != null && left.SequenceEqual(right),
		outline =>
			outline.Aggregate(
				0,
				(hash, segment) =>
					HashCode.Combine(
						hash,
						segment.GetHashCode())),
		outline =>
			(IReadOnlyList<PathSegment>)outline.ToList());

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
							JsonOptions) ?? new List<PathSegment>()),
				OutlineComparer)
			.HasField("_outline")
			.UsePropertyAccessMode(PropertyAccessMode.PreferFieldDuringConstruction);
	}
}
