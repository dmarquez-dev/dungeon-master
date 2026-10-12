namespace DungeonMaster.App.Controls;

/// <summary>
///		Identifies which kind of entity a notebook is currently authoring metadata for.
/// </summary>
public enum NotebookKind
{
	/// <summary>The dungeon itself, which has no owning floor.</summary>
	Dungeon,

	/// <summary>A single floor of a dungeon.</summary>
	Floor,

	/// <summary>A single area on a floor.</summary>
	Area
}
