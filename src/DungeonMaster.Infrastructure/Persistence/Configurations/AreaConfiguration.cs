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

		// INFRA-002 / MASTER-152. The three max-length declarations below are MODEL METADATA ONLY -- the store
		// enforces none of them. EF maps all three columns as TEXT carrying only a `maxLength:` annotation,
		// and SQLite ignores a length argument on a declared type: "numeric arguments in parentheses that
		// following the type name (ex: VARCHAR(255)) are ignored by SQLite ... SQLite does not impose any
		// length restrictions (other than the large global SQLITE_MAX_LENGTH limit) on the length of
		// strings" -- SQLite datatype documentation, at sqlite.org/datatype3.html. No CHECK constraint is
		// generated either, and that half IS measured
		// here: `grep -o 'CHECK' *.cs` over Migrations/ returns 0, and `grep -rn 'HasCheckConstraint' src`
		// returns 0. What this session did NOT measure is the engine itself -- no over-length insert was
		// attempted anywhere in it -- so the length-limit claim rests on the cited documentation.
		//
		// Each bound therefore holds only where some layer applies it, and most are applied nowhere.
		// Measured over the nine max-length declarations in the Dungeon, Floor and Area configurations,
		// three in each: exactly three are additionally bounded by an Application validator, and those are
		// the Name columns at 100. The rest are bounded by NOTHING IN ANY LAYER -- not the store, not the
		// Application layer, not the Domain. In this file, Area.Name is the bounded one, through
		// AddAreaCommandValidator and UpdateAreaCommandValidator, while Area.Description (2000) and
		// Area.Notes (2000) ARE NOT BOUNDED ANYWHERE: either will persist a string of any length. Six of
		// nine columns are in that state. Note that IsRequired() on Name below bounds emptiness, not
		// length, and so is not a guard on any of this.
		//
		// Commands. `grep -rn "MaximumLength" src --include=*.cs`, excluding /obj/ and /bin/, returns six
		// rules, every one MaximumLength(100) and every one on a Name; a seventh hit, in
		// NameOnlyValidatorTestBase, is doc prose rather than a rule and is not a guard.
		// `grep -rn "Description\|Notes" src --include=*Validator.cs` over all 14 validator files returns
		// 0. `grep -rn "Length" src/DungeonMaster.Domain` returns nothing. The nine was confirmed against
		// a second population that could not share the first predicate's mistake: `grep -o maxLength` over
		// Migrations/ gives 8 + 1 + 0 across the three released migrations, and the model snapshot carries
		// nine.
		//
		// The negative case this comment exists to prevent. MASTER-152 and INFRA-002 both record that
		// these limits are "enforced today by exactly one thing -- the FluentValidation validators in the
		// Application layer". Measured above, that is FALSE in two ways at once: the validators bound
		// three of these columns and not all of them, and there are nine columns, not the eleven the row
		// counts. Do not read a max-length declaration here as evidence that a value is bounded, and do not
		// restore that sentence from the register row.
		//
		// Closing the six-column gap is deliberately not done here. The documentation branch was chosen at
		// Gate 1 and reaffirmed at Gate 2; the gap is carried to the backlog. This change adds no
		// validator rule, no CHECK constraint, no Domain guard and no migration.
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
