namespace DungeonMaster.Domain.Entities.Dungeon;

public sealed partial class Dungeon
{
	/// <summary>
	///		Updates the dungeon's name and description and records the modification time.
	/// </summary>
	/// <param name="name">
	///		The new display name. Must not be null or whitespace.
	/// </param>
	/// <param name="description">
	///		The new optional description.
	/// </param>
	public void Update(
		string name,
		string? description)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);
		Name = name;
		Description = description;

		Touch();
	}
}
