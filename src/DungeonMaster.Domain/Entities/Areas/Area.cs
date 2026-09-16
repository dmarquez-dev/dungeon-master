using DungeonMaster.Domain.ValueObjects;

namespace DungeonMaster.Domain.Entities.Areas;

/// <summary>
///		Represents a discrete area within a floor, defined by an optional outline path in floor-absolute tile coordinates.
/// </summary>
public sealed partial class Area : Entity
{
	private readonly List<PathSegment> _outline = [];

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
		_outline.AsReadOnly();
}
