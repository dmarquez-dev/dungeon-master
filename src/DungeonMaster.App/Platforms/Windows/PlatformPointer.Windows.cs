namespace DungeonMaster.App.Services.Platform;

/// <content>
///		The Windows secondary-button implementation.
/// </content>
internal static partial class PlatformPointer
{
	// The Win32 virtual-key code for the right mouse button. Named as Win32 names it so the value
	// can be checked against the platform documentation without a translation step.
	private const int VK_RBUTTON = 0x02;

	// GetAsyncKeyState polls the button state synchronously, which is what makes it usable from
	// inside a touch handler. The high bit of the returned short is set while the key is down.
	[System.Runtime.InteropServices.DllImport("user32.dll")]
	private static extern short GetAsyncKeyState(int vKey);

	/// <inheritdoc cref="IsSecondaryButtonPressed()"/>
	internal static partial bool IsSecondaryButtonPressed() =>
		(GetAsyncKeyState(VK_RBUTTON) & 0x8000) != 0;
}
