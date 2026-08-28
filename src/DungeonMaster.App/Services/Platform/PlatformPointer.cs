namespace DungeonMaster.App.Services.Platform;

/// <summary>
///		Pointer-button state the shared page cannot read for itself.
/// </summary>
/// <remarks>
///		A compile-time seam on the same terms as <see cref="PlatformCanvasCursor"/>: the
///		implementing declaration lives under <c>Platforms/Windows</c> and
///		<c>Platforms/MacCatalyst</c>, one is compiled per target framework, and nothing is
///		registered in <c>MauiProgram.cs</c>.
/// </remarks>
internal static partial class PlatformPointer
{
	/// <summary>
	///		Whether the pointer's secondary (right) button is down at this instant.
	/// </summary>
	/// <returns>
	///		<see langword="true"/> while the secondary button is held. Platforms that cannot
	///		answer the question return <see langword="false"/>, which is the answer that leaves
	///		the caller's behaviour unchanged.
	/// </returns>
	/// <remarks>
	///		The question is asked synchronously from a touch handler, because
	///		<c>GraphicsView.StartInteraction</c> fires for every pointer button and the caller has
	///		to decide there and then whether the press it is looking at is a secondary one.
	/// </remarks>
	internal static partial bool IsSecondaryButtonPressed();
}
