namespace DungeonMaster.App.Core.Editor;

/// <summary>
///		Describes what the view must do as a result of a tool change. It is returned by
///		<see cref="MapEditorState.SetActiveTool"/> so the decision is testable in this project
///		while the MAUI-touching work stays in the page.
/// </summary>
/// <param name="AbortInProgressDraw">
///		True when a draw was in progress and the new tool ends it. The page aborts the draw.
/// </param>
/// <param name="ClearHoveredArea">
///		True when the new tool is none of Move, Select and Erase, so the canvas area-hover
///		highlight must be cleared.
/// </param>
/// <param name="LeavingPlaceTool">
///		True when the new tool is not Place. The page dismisses the secondary palette, clears the
///		ghost object and resets the controls-palette scale.
/// </param>
public readonly record struct ToolChange(
	bool AbortInProgressDraw,
	bool ClearHoveredArea,
	bool LeavingPlaceTool);
