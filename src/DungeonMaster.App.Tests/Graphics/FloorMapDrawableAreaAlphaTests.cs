using Microsoft.Maui.Graphics;

namespace DungeonMaster.App.Tests.Graphics;

/// <summary>
///		Pins the area dimming rule that MASTER-035 collapsed from two loops into one.
/// </summary>
/// <remarks>
///		<c>DrawAreasViewMode</c> and <c>DrawAreasEditMode</c> were byte-identical apart from their
///		alpha expression, and the collapsed loop must agree with BOTH on every combination of
///		<c>HoveredAreaId</c> and <c>ActiveAreaId</c> being null or set. There are four such
///		combinations and there is one fact per combination, because a collapse that got one of them
///		wrong is a rendering change no build or type check would catch.
/// </remarks>
public class FloorMapDrawableAreaAlphaTests
{
	private static RecordingCanvas DrawThreeAreas(Guid? hovered, Guid? active)
	{
		var drawable = MapTestScene.Drawable();
		drawable.Floor = MapTestScene.FloorWith(
			MapTestScene.Square(0, 0),
			MapTestScene.Square(10, 0),
			MapTestScene.Square(20, 0));
		drawable.HoveredAreaId = hovered;
		drawable.ActiveAreaId = active;

		var canvas = new RecordingCanvas();
		drawable.Draw(canvas, new RectF(0f, 0f, 400f, 300f));

		return canvas;
	}

	/// <summary>
	///		With neither an area hovered nor one focused, no area is singled out and every area
	///		renders at full opacity — the view-mode rule when <c>HoveredAreaId</c> is null.
	/// </summary>
	[Fact]
	public void NoHoverAndNoFocus_DrawsEveryAreaAtFullOpacity()
	{
		var canvas = DrawThreeAreas(hovered: null, active: null);

		Assert.Equal(
			[MapTestScene.FullOpacityStroke, MapTestScene.FullOpacityStroke, MapTestScene.FullOpacityStroke],
			canvas.StrokeColorAtDrawPath);
	}

	/// <summary>
	///		With an area hovered and none focused, the hovered area renders at full opacity and the
	///		rest render dimmed — the view-mode rule when <c>HoveredAreaId</c> is set.
	/// </summary>
	[Fact]
	public void HoverWithoutFocus_DrawsOnlyTheHoveredAreaAtFullOpacity()
	{
		var canvas = DrawThreeAreas(hovered: MapTestScene.AreaId(1), active: null);

		Assert.Equal(
			[MapTestScene.DimmedStroke, MapTestScene.FullOpacityStroke, MapTestScene.DimmedStroke],
			canvas.StrokeColorAtDrawPath);
	}

	/// <summary>
	///		With an area focused and none hovered, the focused area renders at full opacity and the
	///		rest render dimmed — the edit-mode rule when <c>HoveredAreaId</c> is null.
	/// </summary>
	[Fact]
	public void FocusWithoutHover_DrawsOnlyTheFocusedAreaAtFullOpacity()
	{
		var canvas = DrawThreeAreas(hovered: null, active: MapTestScene.AreaId(2));

		Assert.Equal(
			[MapTestScene.DimmedStroke, MapTestScene.DimmedStroke, MapTestScene.FullOpacityStroke],
			canvas.StrokeColorAtDrawPath);
	}

	/// <summary>
	///		With one area hovered and a DIFFERENT one focused, the hovered area wins: it renders at
	///		full opacity and the focused area renders dimmed — the edit-mode rule when
	///		<c>HoveredAreaId</c> is set. This is the case the two old loops disagreed about most
	///		visibly, and the one a naive collapse gets wrong.
	/// </summary>
	[Fact]
	public void HoverOverridesFocus_DrawsOnlyTheHoveredAreaAtFullOpacity()
	{
		var canvas = DrawThreeAreas(hovered: MapTestScene.AreaId(0), active: MapTestScene.AreaId(2));

		Assert.Equal(
			[MapTestScene.FullOpacityStroke, MapTestScene.DimmedStroke, MapTestScene.DimmedStroke],
			canvas.StrokeColorAtDrawPath);
	}

	/// <summary>
	///		A ghost area — an outline drawn but not yet persisted — always renders at full opacity,
	///		whatever is hovered or focused, and is drawn after the floor's own areas.
	/// </summary>
	[Fact]
	public void GhostArea_AlwaysDrawsAtFullOpacityAfterTheFloorAreas()
	{
		var drawable = MapTestScene.Drawable();
		drawable.Floor = MapTestScene.FloorWith(MapTestScene.Square(0, 0));
		drawable.ActiveAreaId = MapTestScene.AreaId(99);
		drawable.AddGhostArea(MapTestScene.Square(30, 30));

		var canvas = new RecordingCanvas();
		drawable.Draw(canvas, new RectF(0f, 0f, 400f, 300f));

		Assert.Equal(
			[MapTestScene.DimmedStroke, MapTestScene.FullOpacityStroke],
			canvas.StrokeColorAtDrawPath);
	}
}
