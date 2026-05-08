namespace DungeonMaster.Domain.Entities.Dungeon;

/// <summary>
///		Represents a dungeon in a D&D campaign.
///		Serves as the root aggregate for dungeon management.
///		Floors and rooms are added in future iterations.
/// </summary>
public sealed partial class Dungeon : Entity
{
	// Required by EF Core for entity hydration.
	private Dungeon() { }

	/// <summary>
	///		Initializes a new <see cref="Dungeon"/> with the given name and optional description.
	/// </summary>
	/// <param name="name">
	///		The display name. Must not be null or whitespace.
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
}
