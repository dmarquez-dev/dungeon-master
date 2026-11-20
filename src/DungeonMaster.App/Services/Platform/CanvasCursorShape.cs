namespace DungeonMaster.App.Services.Platform;

/// <summary>
///		The cursor shapes the map canvas asks for.
/// </summary>
/// <remarks>
///		Deliberately platform-neutral. The page names the shape the active tool implies and each
///		platform maps that shape to its own native cursor, so the page never names a WinUI or an
///		AppKit type. The five members are exactly the distinctions the two existing platform
///		mappings make between them: WinUI separates <see cref="Move"/> from <see cref="Grab"/>
///		(SizeAll versus Hand) while AppKit does not, and AppKit separates <see cref="Grab"/> from
///		<see cref="Grabbing"/> (open versus closed hand) while WinUI does not.
/// </remarks>
public enum CanvasCursorShape
{
	/// <summary>The default pointer. Used by every tool that does not name another shape.</summary>
	Arrow,

	/// <summary>A precision crosshair. Used by the draw and erase tools.</summary>
	Crosshair,

	/// <summary>Free movement in any direction. Used by the area move tool.</summary>
	Move,

	/// <summary>The surface can be grabbed and panned. Used by the pan tool at rest.</summary>
	Grab,

	/// <summary>The surface is being panned right now. Used by the pan tool while dragging.</summary>
	Grabbing,
}
