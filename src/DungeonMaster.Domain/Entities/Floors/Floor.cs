using DungeonMaster.Domain.Entities.Areas;

namespace DungeonMaster.Domain.Entities.Floors;

/// <summary>
///		Represents a floor within a dungeon, containing a collection of areas.
/// </summary>
public sealed partial class Floor : Entity
{
	private readonly List<Areas.Area> _areas = [];
	private readonly List<PlacedObject> _objects = [];

	// MASTER-114. The read-only wrappers over _areas and _objects are built once and reused, so Areas
	// and Objects hand back the same instance on every read instead of allocating a fresh wrapper per
	// call. A C# field initializer cannot reference another instance field (CS0236), so the wrappers
	// are constructed in the parameterless constructor that every other constructor chains through.
	private readonly IReadOnlyList<Areas.Area> _areasView;
	private readonly IReadOnlyList<PlacedObject> _objectsView;

	private Floor()
	{
		_areasView = _areas.AsReadOnly();
		_objectsView = _objects.AsReadOnly();
	}

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
	/// <exception cref="ArgumentOutOfRangeException">
	///		<paramref name="number"/> is less than 1.
	/// </exception>
	internal Floor(
		Guid dungeonId,
		int number,
		string? name)
		: this()
	{
		if (number < 1)
		{
			throw new ArgumentOutOfRangeException(
				nameof(number),
				number,
				"Floor number must be 1 or greater.");
		}

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
	///		Gets the optional player-facing description of this floor.
	/// </summary>
	public string? Description { get; private set; }

	/// <summary>
	///		Gets the optional notes associated with this floor.
	/// </summary>
	public string? Notes { get; private set; }

	/// <summary>
	///		Gets the read-only list of areas belonging to this floor.
	/// </summary>
	public IReadOnlyList<Areas.Area> Areas =>
		_areasView;

	/// <summary>
	///		Gets the read-only list of objects placed on this floor.
	/// </summary>
	public IReadOnlyList<PlacedObject> Objects =>
		_objectsView;
}
