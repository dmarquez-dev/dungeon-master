namespace DungeonMaster.App.Services.Platform;

/// <content>
///		The WinUI cursor implementation. WinUI exposes no public way to set a cursor on an
///		arbitrary element, so <c>Microsoft.UI.Xaml.UIElement.ProtectedCursor</c> is reached by
///		reflection — the same approach <c>Behaviors/HandCursorBehavior.cs</c> takes.
/// </content>
internal static partial class PlatformCanvasCursor
{
	/// <summary>
	///		The non-public <c>Microsoft.UI.Xaml.UIElement.ProtectedCursor</c> property, resolved
	///		ONCE for the process rather than on every pointer event.
	/// </summary>
	/// <remarks>
	///		Null when the running WinUI version no longer exposes the member. That is reported by
	///		the type initializer below rather than swallowed: the previous code hung a
	///		<c>?.SetValue</c> off the lookup, so a WinUI change would have left the cursor silently
	///		stuck on the arrow with nothing anywhere saying why.
	/// </remarks>
	private static readonly System.Reflection.PropertyInfo? ProtectedCursorProperty =
		typeof(Microsoft.UI.Xaml.UIElement)
			.GetProperty(
				"ProtectedCursor",
				System.Reflection.BindingFlags.Instance |
				System.Reflection.BindingFlags.NonPublic |
				System.Reflection.BindingFlags.Public);

	// Runs once, at the first use of this type, which is also the only time the lookup above can
	// fail. Emitting here rather than per call means the diagnostic names the failure once instead
	// of once per pointer move.
	static PlatformCanvasCursor()
	{
		if (ProtectedCursorProperty is null)
		{
			System.Diagnostics.Debug.WriteLine(
				"PlatformCanvasCursor: Microsoft.UI.Xaml.UIElement.ProtectedCursor could not be " +
				"resolved by reflection. The map canvas will keep the system default cursor for " +
				"every tool. The running WinUI version no longer exposes the member this depends on.");
		}
	}

	/// <inheritdoc cref="SetShape(object?, CanvasCursorShape)"/>
	internal static partial void SetShape(object? platformView, CanvasCursorShape shape)
	{
		// The lookup failed once, at type initialization, and was reported there. There is nothing
		// to set and nothing to retry.
		if (ProtectedCursorProperty is null)
		{
			return;
		}

		if (platformView is not Microsoft.UI.Xaml.UIElement element)
		{
			return;
		}

		ProtectedCursorProperty.SetValue(
			element,
			Microsoft.UI.Input.InputSystemCursor.Create(ToSystemShape(shape)));
	}

	// Grabbing maps to SizeAll rather than to a closed hand because WinUI has no closed-hand
	// system cursor; that is the mapping the page has used since the pan tool was added.
	private static Microsoft.UI.Input.InputSystemCursorShape ToSystemShape(CanvasCursorShape shape) =>
		shape switch
		{
			CanvasCursorShape.Crosshair => Microsoft.UI.Input.InputSystemCursorShape.Cross,
			CanvasCursorShape.Move => Microsoft.UI.Input.InputSystemCursorShape.SizeAll,
			CanvasCursorShape.Grab => Microsoft.UI.Input.InputSystemCursorShape.Hand,
			CanvasCursorShape.Grabbing => Microsoft.UI.Input.InputSystemCursorShape.SizeAll,
			_ => Microsoft.UI.Input.InputSystemCursorShape.Arrow,
		};
}
