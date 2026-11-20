namespace DungeonMaster.App.Services.Platform;

/// <content>
///		The macCatalyst cursor implementation.
/// </content>
/// <remarks>
///		On macCatalyst the GraphicsView platform view is a UIView and the cursor is set through
///		<c>NSCursor</c>, which is process-wide rather than per-view — so the platform view is not
///		consulted here. UIHoverGestureRecognizer fires continuously while the pointer is over the
///		view, so the cursor is pushed once and the next hover event (or tool switch) pops it.
/// </remarks>
internal static partial class PlatformCanvasCursor
{
	// TODO(macOS): NSCursor has no SizeAll or crosshair that maps 1:1 to WinUI's SizeAll. The
	// closest available cursors are used below. Consider caching the hover recognizer on the canvas
	// view and updating its action delegate rather than re-adding it each time.
	//
	// This platform builds but cannot be run under ADR-001, so the mapping below is compiled and
	// inspected, never observed.
	/// <inheritdoc cref="SetShape(object?, CanvasCursorShape)"/>
	internal static partial void SetShape(object? platformView, CanvasCursorShape shape)
	{
		switch (shape)
		{
			case CanvasCursorShape.Crosshair:
				AppKit.NSCursor.CrosshairCursor.Set();
				break;
			case CanvasCursorShape.Move:
			case CanvasCursorShape.Grab:
				AppKit.NSCursor.OpenHandCursor.Set();
				break;
			case CanvasCursorShape.Grabbing:
				AppKit.NSCursor.ClosedHandCursor.Set();
				break;
			default:
				AppKit.NSCursor.ArrowCursor.Set();
				break;
		}
	}
}
