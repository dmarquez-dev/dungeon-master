using DungeonMaster.Domain.Entities.Area;

namespace DungeonMaster.Domain.Entities.Floor;

/// <summary>
///		Represents a floor within a dungeon, containing a collection of areas.
/// </summary>
public sealed partial class Floor : Entity
{
	private readonly List<Area.Area> _areas = [];
	private readonly List<PlacedObject> _objects = [];

	private Floor() { }

	/// <summary>
	///		Initializes a new <see cref="Floor"/> belonging to the specified dungeon with its assigned number and optional name.
	/// </summary>
	/// <param name="dungeonId">
	///		The unique identifier of the dungeon this floor belongs to.
	/// </param>
	/// <param name="number">
	///		The sequential floor number within the dungeon.
	/// </param>
	/// <param name="name">
	///		The optional display name for this floor.
	/// </param>
	internal Floor(
		Guid dungeonId,
		int number,
		string? name)
	{
		DungeonId = dungeonId;
		Number = number;
		Name = name;
	}

	/// <summary>
	///		Gets the unique identifier of the dungeon this floor belongs to.
	/// </summary>
	public Guid DungeonId { get; private set; }

	/// <summary>
	///		Gets the sequential number of this floor within its dungeon.
	/// </summary>
	public int Number { get; private set; }

	/// <summary>
	///		Gets the optional display name of this floor.
	/// </summary>
	public string? Name { get; private set; }

	/// <summary>
	///		Gets the optional notes associated with this floor.
	/// </summary>
	public string? Notes { get; private set; }

	/// <summary>
	///		Gets the read-only list of areas belonging to this floor.
	/// </summary>
	public IReadOnlyList<Area.Area> Areas =>
		_areas.AsReadOnly();

	/// <summary>
	///		Gets the read-only list of objects placed on this floor.
	/// </summary>
	public IReadOnlyList<PlacedObject> Objects =>
		_objects.AsReadOnly();
}
