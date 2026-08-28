namespace DungeonMaster.App.Services.Platform;

/// <summary>
///		Sets the mouse cursor shown over the map canvas.
/// </summary>
/// <remarks>
///		<para>
///		This is a compile-time seam, not a dependency-injected service: the implementing
///		declaration lives under <c>Platforms/Windows</c> and <c>Platforms/MacCatalyst</c>, and the
///		MAUI single-project convention compiles exactly one of them per target framework. Nothing
///		is registered in <c>MauiProgram.cs</c> and nothing is resolved at run time.
///		</para>
///		<para>
///		Both target frameworks supply an implementing declaration, which is what an extended
///		partial method requires. Adding a third target framework without adding a third
///		implementation is a compile error rather than a silent no-op, which is the point.
///		</para>
/// </remarks>
internal static partial class PlatformCanvasCursor
{
	/// <summary>
	///		Applies <paramref name="shape"/> to the cursor over <paramref name="platformView"/>.
	/// </summary>
	/// <param name="platformView">
	///		The canvas's native view, as handed over by <c>Handler?.PlatformView</c>. May be null
	///		while the handler has not been created, and may be a type the platform does not
	///		recognise; either case leaves the cursor alone.
	/// </param>
	/// <param name="shape">
	///		The shape the active tool implies.
	/// </param>
	internal static partial void SetShape(object? platformView, CanvasCursorShape shape);
}
