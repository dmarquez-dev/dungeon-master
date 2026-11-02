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

	// INFRA-004 / MASTER-015, revised for MASTER-114. Area.Outline no longer allocates a fresh
	// AsReadOnly() wrapper on every read: it caches the wrapper on first read and hands back that same
	// instance thereafter. The original symptom this comparer was filed for -- DetectChanges marking
	// Outline modified on every SaveChanges for every tracked Area, because reference equality on the
	// converted reference type never saw the same wrapper twice -- is therefore gone.
	//
	// The comparer is still required, and caching is precisely why: it is now load-bearing in the
	// opposite direction. Area.SetOutline mutates the _outline list IN PLACE, so after caching the
	// wrapper is reference-equal to itself across a genuine outline edit. Under reference equality EF
	// would see no change and silently drop the edit. Measured: with OutlineComparer removed from the
	// HasConversion call below, `dotnet test src/DungeonMaster.Tests/DungeonMaster.Tests.csproj
	// --filter FullyQualifiedName~AreaOutlineChangeTrackingTests` reports 1 failed of 3 --
	// LoadAsync_AfterAnOutlineEdit_ReturnsTheEditedOutline, which reloads and finds the edit missing.
	// Note that CommitAsync_AfterAGenuineOutlineEdit_StillWritesTheArea does NOT catch this: Touch()
	// dirties LastModified, so the row is written regardless and only the Outline column is stale.
	// Structural comparison is what keeps the edit detectable.
	//
	// PathSegment is a record, so its compiler-generated structural equality and GetHashCode are what
	// SequenceEqual and the aggregated hash compose here.
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

		// INFRA-002 / MASTER-152. Each max-length declaration below is MODEL METADATA ONLY -- the store
		// enforces none of them. EF maps each of these columns as TEXT carrying only a `maxLength:`
		// annotation, and SQLite ignores a length argument on a declared type: "numeric arguments in
		// parentheses that following the type name (ex: VARCHAR(255)) are ignored by SQLite ... SQLite does
		// not impose any length restrictions (other than the large global SQLITE_MAX_LENGTH limit) on the
		// length of strings" -- SQLite datatype documentation, at sqlite.org/datatype3.html. No CHECK
		// constraint is generated either: `grep -rn 'CHECK' src/DungeonMaster.Infrastructure/Migrations`
		// returns 0. The length-limit claim rests on the cited documentation, not on a measured
		// over-length insert.
		//
		// A declaration here is therefore not evidence that a value is bounded. Each of these bounds is
		// applied in the Application layer, by a FluentValidation MaximumLength rule whose limit equals
		// the HasMaxLength argument below:
		// - Area.Name at 100, by AddAreaCommandValidator and UpdateAreaCommandValidator;
		// - Area.Description at 2000, by AddAreaCommandValidator and UpdateAreaCommandValidator;
		// - Area.Notes at 2000, by UpdateAreaCommandValidator (AddAreaCommand has no Notes).
		// A write that does not pass through those validators is not length-checked by them. Change a
		// limit here and in its validator rules together. IsRequired() on Name below bounds emptiness,
		// not length.
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
