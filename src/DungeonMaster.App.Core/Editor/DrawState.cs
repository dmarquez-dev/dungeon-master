namespace DungeonMaster.App.Core.Editor;

/// <summary>
///		Identifies the three states of the outline-drawing interaction.
/// </summary>
public enum DrawState
{
	/// <summary>No floor is selected, or the draw tool is not active, so drawing cannot begin.</summary>
	Idle,

	/// <summary>The draw tool is active and a floor is selected, but no outline is in progress.</summary>
	DrawingReady,

	/// <summary>An outline is currently being drawn.</summary>
	Drawing,
}
