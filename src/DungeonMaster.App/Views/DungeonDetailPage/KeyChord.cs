using Plugin.Maui.KeyListener;

namespace DungeonMaster.App.Views.DungeonDetail;

/// <summary>
///		Decides which modifier combinations the page's Control chords accept.
///		The decision lives here rather than in the key handler so it executes on the test host with no page.
/// </summary>
internal static class KeyChord
{
	/// <summary>
	///		Returns whether the modifiers held during a key press are exactly <see cref="KeyboardModifiers.Control"/>.
	///		<see cref="KeyboardModifiers"/> is a flags enumeration, so Control combined with any other modifier is a different value and is rejected.
	/// </summary>
	/// <param name="modifiers">
	///		The modifiers reported by the key press.
	/// </param>
	/// <returns>
	///		<see langword="true"/> when Control is the only modifier held; otherwise <see langword="false"/>.
	/// </returns>
	internal static bool IsControlOnly(KeyboardModifiers modifiers) =>
		modifiers == KeyboardModifiers.Control;
}
