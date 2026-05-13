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

	/// <summary>Pan tool — drag to pan the map view (keyboard shortcut H).</summary>
	Pan,

	/// <summary>Place tool — click to place entities on the map (keyboard shortcut P).</summary>
	Place,
}
