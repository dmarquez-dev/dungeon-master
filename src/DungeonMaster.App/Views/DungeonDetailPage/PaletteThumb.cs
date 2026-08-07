using DungeonMaster.App.Behaviors;
using DungeonMaster.App.Theme;

namespace DungeonMaster.App.Views.DungeonDetail;

/// <summary>
///		Shared hover treatment for the 48x48 object thumbnails rendered by both
///		<see cref="ObjectPaletteView"/> and <see cref="SecondaryPaletteView"/>. The two palettes
///		show the same chip in two places, so the four colours and two pointer events live here
///		rather than being restated per view, where they had already drifted apart.
/// </summary>
internal static class PaletteThumb
{
	// Properties and methods, not static readonly fields: see DmTheme's remarks and the guard
	// test NoEagerDmThemeRead.
	private static Color SurfaceNormal =>
		DmTheme.SurfaceToolChip;

	private static Color SurfaceHover =>
		DmTheme.SurfaceToolChipHover;

	// A brush per pointer event rather than one shared instance: a SolidColorBrush handed to
	// several views at once is a MAUI anti-pattern, and no brush token exists for either colour.
	private static SolidColorBrush StrokeNormal() =>
		new(DmTheme.BorderToolChip);

	private static SolidColorBrush StrokeHover() =>
		new(DmTheme.Gold);

	/// <summary>
	///		Adds the hand cursor and the pointer-enter/exit surface and stroke swap to a
	///		thumbnail chip.
	/// </summary>
	/// <param name="border">
	///		The chip to attach the hover treatment to.
	/// </param>
	internal static void AttachHover(Border border)
	{
		border.Behaviors.Add(new HandCursorBehavior());
		var ptr = new PointerGestureRecognizer();
		ptr.PointerEntered += (_, _) =>
		{
			border.BackgroundColor = SurfaceHover;
			border.Stroke = StrokeHover();
		};
		ptr.PointerExited += (_, _) =>
		{
			border.BackgroundColor = SurfaceNormal;
			border.Stroke = StrokeNormal();
		};
		border.GestureRecognizers.Add(ptr);
	}
}
