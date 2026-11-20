namespace DungeonMaster.App.Views.DungeonDetail;

/// <summary>
///		Decides the zoom level the map canvas moves to when a zoom input requests a new raw level.
///		The decision lives here rather than in the page so it executes on the test host with no page.
/// </summary>
internal static class ZoomLevelGuard
{
	/// <summary>
	///		The distance from 100% within which a requested level snaps to exactly 1.0.
	/// </summary>
	internal const float SnapThreshold = 0.05f;

	/// <summary>
	///		The smallest zoom level the canvas accepts.
	/// </summary>
	internal const float MinimumZoom = 0.25f;

	/// <summary>
	///		The largest zoom level the canvas accepts.
	/// </summary>
	internal const float MaximumZoom = 4.0f;

	/// <summary>
	///		Returns the zoom level that follows <paramref name="current"/> when <paramref name="raw"/> is requested.
	///		A non-finite request is rejected and leaves the level where it is, as <c>CanvasPoint</c> and
	///		<c>FloorReorderNearest.Resolve</c> reject non-finite coordinates: a NaN would otherwise pass
	///		through the clamp unchanged and an infinity would land on a bound the user never asked for.
	/// </summary>
	/// <param name="current">
	///		The zoom level the canvas is at now.
	/// </param>
	/// <param name="raw">
	///		The level the zoom input requested, before snapping and clamping.
	/// </param>
	/// <returns>
	///		<paramref name="current"/> when <paramref name="raw"/> is NaN or infinite; otherwise
	///		<paramref name="raw"/> snapped to 1.0 when it lies within <see cref="SnapThreshold"/> of it,
	///		then clamped to [<see cref="MinimumZoom"/>, <see cref="MaximumZoom"/>].
	/// </returns>
	internal static float Next(float current, float raw)
	{
		if (!float.IsFinite(raw))
		{
			return current;
		}

		if (Math.Abs(raw - 1.0f) < SnapThreshold)
		{
			raw = 1.0f;
		}

		return Math.Clamp(raw, MinimumZoom, MaximumZoom);
	}
}
