namespace DungeonMaster.App.Services.Platform;

/// <content>
///		The macCatalyst secondary-button implementation.
/// </content>
internal static partial class PlatformPointer
{
	// TODO(macOS): there is no GetAsyncKeyState equivalent on macCatalyst. The platform maps a
	// right-click / two-finger-tap to the SecondaryTapped event, which already routes to the page's
	// OnMapSecondaryTapped handler. However, if the native UIGestureRecognizer fires
	// StartInteraction before SecondaryTapped on macCatalyst (which can happen), a spurious
	// StartInteraction will execute draw/place logic. Verify on a real Mac that right-click-only
	// does NOT trigger OnMapStartInteraction; if it does, answer this question here using a
	// UIEvent.IsSecondaryButton check over the platform view's gesture recognizers.
	//
	// Returning false is what the page did on this platform before the seam existed: the guard was
	// inside `#if WINDOWS`, so macCatalyst never suppressed a StartInteraction. This platform
	// builds but cannot be run under ADR-001, so that is compiled and inspected, never observed.
	/// <inheritdoc cref="IsSecondaryButtonPressed()"/>
	internal static partial bool IsSecondaryButtonPressed() =>
		false;
}
