using DungeonMaster.Domain.ValueObjects;

namespace DungeonMaster.Domain.Entities.Areas;

/// <summary>
///		Represents a discrete area within a floor, defined by an optional outline path in floor-absolute tile coordinates.
/// </summary>
public sealed partial class Area : Entity
{
	private readonly List<PathSegment> _outline = [];

	// MASTER-114. The read-only wrapper over _outline is built on first read and cached in _outlineView,
	// so Outline hands back the cached instance instead of allocating a fresh wrapper per call. The ??=
	// is a non-atomic check-then-assign: two threads making the first read at once can each build a
	// wrapper over the same list, and the field keeps whichever is assigned last, so the instance is
	// unique only when the first read is not concurrent.
	//
	// Unlike the wrappers on Dungeon.Floors, Floor.Areas and Floor.Objects, this one CANNOT be a
	// readonly field built in the constructor. Outline is a value-converted property mapped with
	// .HasField("_outline") and PropertyAccessMode.PreferFieldDuringConstruction in
	// AreaConfiguration, and the converter's provider-to-model lambda returns a NEW List<PathSegment>
	// that EF assigns over the field after the constructor has run. A wrapper captured in the
	// constructor therefore wraps the pre-materialization list and Outline reads back empty forever.
	// Measured: with the wrapper built in the constructor, AreaOutlineChangeTrackingTests failed 2 of
	// 3 (the assertions on the loaded aggregate and on the reloaded edited outline); deferring the
	// wrapper to first read passes 3 of 3. First read always follows materialization, so the wrapper
	// pairs with whichever list instance EF installed, and without concurrent first reads it is still
	// built only once per Area.
	private IReadOnlyList<PathSegment>? _outlineView;

	private Area() { }

	/// <summary>
	///		Initializes a new <see cref="Area"/> belonging to the specified floor with the given name and optional description.
	/// </summary>
	/// <param name="floorId">
	///		The unique identifier of the floor this area belongs to.
	/// </param>
	/// <param name="name">
	///		The display name of the area. Must not be null or whitespace.
	/// </param>
	/// <param name="description">
	///		The optional description of the area.
	/// </param>
	internal Area(
		Guid floorId,
		string name,
		string? description)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);
		FloorId = floorId;
		Name = name;
		Description = description;
	}

	/// <summary>
	///		Gets the unique identifier of the floor this area belongs to.
	/// </summary>
	public Guid FloorId { get; private set; }

	/// <summary>
	///		Gets the display name of this area.
	/// </summary>
	public string Name { get; private set; } = default!;

	/// <summary>
	///		Gets the optional description of this area.
	/// </summary>
	public string? Description { get; private set; }

	/// <summary>
	///		Gets the optional notes associated with this area.
	/// </summary>
	public string? Notes { get; private set; }

	/// <summary>
	///		Gets the read-only list of path segments that form the outline of this area.
	/// </summary>
	public IReadOnlyList<PathSegment> Outline =>
		_outlineView ??= _outline.AsReadOnly();
}
