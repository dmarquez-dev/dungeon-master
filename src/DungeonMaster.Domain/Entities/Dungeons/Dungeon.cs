namespace DungeonMaster.Domain.Entities.Dungeons;

/// <summary>
///		Represents a dungeon in the campaign world, containing floors and their areas.
/// </summary>
public sealed partial class Dungeon : Entity
{
	private readonly List<Floors.Floor> _floors = [];

	// MASTER-114. The read-only wrapper over _floors is built once and reused, so Floors hands back
	// the same instance on every read instead of allocating a fresh wrapper per call. A C# field
	// initializer cannot reference another instance field (CS0236), so the wrapper is constructed in
	// the parameterless constructor that every other constructor chains through.
	private readonly IReadOnlyList<Floors.Floor> _floorsView;

	private Dungeon() =>
		_floorsView = _floors.AsReadOnly();

	/// <summary>
	///		Initializes a new <see cref="Dungeon"/> with the given name and optional description.
	/// </summary>
	/// <param name="name">
	///		The display name of the dungeon. Must not be null or whitespace.
	/// </param>
	/// <param name="description">
	///		An optional description of the dungeon.
	/// </param>
	public Dungeon(
		string name,
		string? description = null)
		: this()
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);
		Name = name;
		Description = description;
	}

	/// <summary>
	///		Gets the display name of the dungeon.
	/// </summary>
	public string Name { get; private set; } = default!;

	/// <summary>
	///		Gets the optional description of the dungeon.
	/// </summary>
	public string? Description { get; private set; }

	/// <summary>
	///		Gets the optional notes associated with the dungeon.
	/// </summary>
	public string? Notes { get; private set; }

	/// <summary>
	///		Gets the read-only list of floors belonging to this dungeon.
	/// </summary>
	public IReadOnlyList<Floors.Floor> Floors =>
		_floorsView;
}
