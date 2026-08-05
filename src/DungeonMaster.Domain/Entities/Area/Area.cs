using DungeonMaster.Domain.ValueObjects;

namespace DungeonMaster.Domain.Entities.Area;

/// <summary>
///		Represents a discrete area within a floor, defined by an origin point and an optional outline path.
/// </summary>
public sealed partial class Area : Entity
{
	private readonly List<PathSegment> _outline = [];
	private readonly List<PlacedObject> _objects = [];
	private readonly List<TileTexture> _tileTextures = [];
	private readonly List<StructureElement> _structureElements = [];

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
		Origin = new TilePoint(0, 0);
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
	///		Gets the tile-grid origin point of this area.
	/// </summary>
	public TilePoint Origin { get; private set; } = default!;

	/// <summary>
	///		Gets the read-only list of path segments that form the outline of this area.
	/// </summary>
	public IReadOnlyList<PathSegment> Outline =>
		_outline.AsReadOnly();

	/// <summary>
	///		Gets the read-only list of objects placed within this area.
	/// </summary>
	public IReadOnlyList<PlacedObject> Objects =>
		_objects.AsReadOnly();

	/// <summary>
	///		Gets the read-only list of tile textures applied within this area.
	/// </summary>
	public IReadOnlyList<TileTexture> TileTextures =>
		_tileTextures.AsReadOnly();

	/// <summary>
	///		Gets the read-only list of structural elements placed within this area.
	/// </summary>
	public IReadOnlyList<StructureElement> StructureElements =>
		_structureElements.AsReadOnly();
}
