namespace DungeonMaster.App.ViewModels;

/// <summary>
///		Identifies the active drawing tool on the map canvas.
/// </summary>
public enum MapTool
{
	/// <summary>Draw tool — click to place polyline vertices.</summary>
	Draw,

	/// <summary>Move tool — drag area outlines to new positions.</summary>
	Move,
}
