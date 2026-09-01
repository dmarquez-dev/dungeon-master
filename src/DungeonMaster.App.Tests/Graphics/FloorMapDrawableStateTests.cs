using DungeonMaster.App.Graphics;
using DungeonMaster.Domain.ValueObjects;
using Microsoft.Maui.Graphics;

namespace DungeonMaster.App.Tests.Graphics;

/// <summary>
///		Pins the mutation seam MASTER-059 put in place of the four public collections.
/// </summary>
/// <remarks>
///		UIKIT-008's concrete complaint was that the drawable could not observe its own state: the
///		page mutated a handed-out <c>List</c> or <c>Dictionary</c> and had to remember to call
///		<c>Invalidate()</c> afterwards. Every mutating member now raises
///		<c>RedrawRequested</c>, so a forgotten invalidation is no longer possible; these facts are
///		what would notice one of them losing the notification again.
/// </remarks>
public class FloorMapDrawableStateTests
{
	private static int CountRedrawRequests(Action<Harness> mutate)
	{
		var harness = new Harness();
		mutate(harness);

		return harness.Redraws;
	}

	/// <summary>
	///		A drawable with a counter attached to its redraw notification.
	/// </summary>
	internal sealed class Harness
	{
		public Harness()
		{
			Drawable = MapTestScene.Drawable();
			Drawable.RedrawRequested += (_, _) =>
				Redraws++;
		}

		public FloorMapDrawable Drawable { get; }

		public int Redraws { get; private set; }
	}

	/// <summary>
	///		Recording a pending move offset asks for a redraw, because the area it names now paints
	///		somewhere else.
	/// </summary>
	[Fact]
	public void SetPendingMoveOffset_RequestsARedraw() =>
		Assert.Equal(
			1,
			CountRedrawRequests(harness =>
				harness.Drawable.SetPendingMoveOffset(MapTestScene.AreaId(0), new TilePoint(1, 1))));

	/// <summary>
	///		Dropping the pending move offsets asks for a redraw, because every area they named
	///		snaps back to the position the floor reports.
	/// </summary>
	[Fact]
	public void ClearPendingMoveOffsets_RequestsARedraw() =>
		Assert.Equal(
			1,
			CountRedrawRequests(harness =>
				harness.Drawable.ClearPendingMoveOffsets()));

	/// <summary>
	///		Adding a ghost area asks for a redraw, because an outline that was not painted before now is.
	/// </summary>
	[Fact]
	public void AddGhostArea_RequestsARedraw() =>
		Assert.Equal(
			1,
			CountRedrawRequests(harness =>
				harness.Drawable.AddGhostArea(MapTestScene.Square(0, 0))));

	/// <summary>
	///		Dropping the ghost areas asks for a redraw, because outlines that were painted no longer are.
	/// </summary>
	[Fact]
	public void ClearGhostAreas_RequestsARedraw() =>
		Assert.Equal(
			1,
			CountRedrawRequests(harness =>
				harness.Drawable.ClearGhostAreas()));

	/// <summary>
	///		Replacing the in-progress vertices asks for a redraw, because the preview polyline changes.
	/// </summary>
	[Fact]
	public void SetInProgressVertices_RequestsARedraw() =>
		Assert.Equal(
			1,
			CountRedrawRequests(harness =>
				harness.Drawable.SetInProgressVertices([new TilePoint(0, 0), new TilePoint(1, 1)])));

	/// <summary>
	///		Dropping the in-progress vertices asks for a redraw, because the preview polyline goes away.
	/// </summary>
	[Fact]
	public void ClearInProgressVertices_RequestsARedraw() =>
		Assert.Equal(
			1,
			CountRedrawRequests(harness =>
				harness.Drawable.ClearInProgressVertices()));

	/// <summary>
	///		The vertices handed to <c>SetInProgressVertices</c> are the ones the next frame paints,
	///		and a later call replaces them rather than appending to them.
	/// </summary>
	[Fact]
	public void SetInProgressVertices_ReplacesThePreviousVerticesRatherThanAppending()
	{
		var drawable = MapTestScene.Drawable();
		drawable.TileSize = 40f;
		drawable.Floor = MapTestScene.FloorWith();
		drawable.SetInProgressVertices([new TilePoint(0, 0), new TilePoint(1, 0), new TilePoint(2, 0)]);
		drawable.SetInProgressVertices([new TilePoint(5, 5), new TilePoint(6, 5)]);

		var canvas = new RecordingCanvas();
		drawable.Draw(canvas, new RectF(0f, 0f, 400f, 300f));

		Assert.Equal(
			[new PointF(200f, 200f), new PointF(240f, 200f)],
			Assert.Single(canvas.DrawnPaths));
	}

	/// <summary>
	///		Clearing the in-progress vertices leaves nothing for the next frame to paint.
	/// </summary>
	[Fact]
	public void ClearInProgressVertices_LeavesNoPreviewPolylineToPaint()
	{
		var drawable = MapTestScene.Drawable();
		drawable.Floor = MapTestScene.FloorWith();
		drawable.SetInProgressVertices([new TilePoint(0, 0), new TilePoint(1, 0)]);
		drawable.ClearInProgressVertices();

		var canvas = new RecordingCanvas();
		drawable.Draw(canvas, new RectF(0f, 0f, 400f, 300f));

		Assert.Empty(canvas.DrawnPaths);
	}

	/// <summary>
	///		Clearing the ghost areas leaves nothing for the next frame to paint, so an optimistic
	///		outline does not survive the reload that replaced it with a real area.
	/// </summary>
	[Fact]
	public void ClearGhostAreas_LeavesNoGhostOutlineToPaint()
	{
		var drawable = MapTestScene.Drawable();
		drawable.Floor = MapTestScene.FloorWith();
		drawable.AddGhostArea(MapTestScene.Square(0, 0));
		drawable.ClearGhostAreas();

		var canvas = new RecordingCanvas();
		drawable.Draw(canvas, new RectF(0f, 0f, 400f, 300f));

		Assert.Empty(canvas.DrawnPaths);
	}

	/// <summary>
	///		Clearing the pending move offsets puts the area back where the floor says it is.
	/// </summary>
	[Fact]
	public void ClearPendingMoveOffsets_PaintsTheAreaBackAtItsStoredPosition()
	{
		var drawable = MapTestScene.Drawable();
		drawable.TileSize = 40f;
		drawable.Floor = MapTestScene.FloorWith(MapTestScene.Square(1, 1));
		drawable.SetPendingMoveOffset(MapTestScene.AreaId(0), new TilePoint(10, 10));
		drawable.ClearPendingMoveOffsets();

		var canvas = new RecordingCanvas();
		drawable.Draw(canvas, new RectF(0f, 0f, 400f, 300f));

		Assert.Equal(new PointF(40f, 40f), Assert.Single(canvas.DrawnPaths)[0]);
	}

	/// <summary>
	///		A drawable constructed without a palette source is rejected rather than failing on the
	///		first frame, so the fault is reported where the mistake was made.
	/// </summary>
	[Fact]
	public void NullPaletteSource_IsRejectedAtConstruction() =>
		Assert.Throws<ArgumentNullException>(() =>
			new FloorMapDrawable(null!));
}
