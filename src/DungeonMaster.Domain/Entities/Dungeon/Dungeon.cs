using DungeonMaster.Domain.Entities.Floor;

namespace DungeonMaster.Domain.Entities.Dungeon;

/// <summary>
///		Represents a dungeon in the campaign world, containing floors and their areas.
/// </summary>
public sealed partial class Dungeon : Entity
{
	private readonly List<Floor.Floor> _floors = [];

	private Dungeon() { }

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
	public IReadOnlyList<Floor.Floor> Floors =>
		_floors.AsReadOnly();
}
