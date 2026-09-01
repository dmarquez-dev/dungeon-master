using DungeonMaster.App.Graphics;
using DungeonMaster.Domain.ValueObjects;
using Microsoft.Maui.Graphics;

namespace DungeonMaster.App.Tests.Graphics;

/// <summary>
///		Measures what one map frame allocates, and pins the two reductions MASTER-097 made that are
///		observable from outside the drawable.
/// </summary>
/// <remarks>
///		UIKIT-009 asks for a number rather than an argument: "Measure before and after -- this is
///		the kind of change that should be justified by a number." <see cref="RecordingCanvas"/> is
///		put in its non-recording mode for the measurement, so the bytes counted are the drawable's
///		own and not a mock's call log. The palette is supplied fixed, so theme resolution is
///		outside the measurement on both sides of the comparison.
/// </remarks>
public class FloorMapDrawableAllocationTests
{
	private const int MeasuredFrames = 500;

	private const int WarmupFrames = 200;

	private static readonly RectF FrameRect = new(0f, 0f, 1600f, 1200f);

	/// <summary>
	///		The drawable and scene the measurement runs against: twenty areas of twelve segments,
	///		two of them curved, six placed objects, a ghost outline, and a five-vertex in-progress
	///		draw with a live phantom segment. <paramref name="withTileOffsets"/> puts a pending move
	///		offset on every area, which is the only difference between the two measured runs.
	/// </summary>
	private static FloorMapDrawable RepresentativeDrawable(bool withTileOffsets)
	{
		var drawable = MapTestScene.Drawable();
		drawable.TileSize = 40f;
		drawable.Floor = MapTestScene.RepresentativeFloor();
		drawable.ActiveAreaId = MapTestScene.AreaId(3);
		drawable.HoveredAreaId = MapTestScene.AreaId(7);
		drawable.AddGhostArea(MapTestScene.Square(30, 30));
		drawable.SetInProgressVertices(
		[
			new TilePoint(0, 0),
			new TilePoint(2, 0),
			new TilePoint(2, 2),
			new TilePoint(4, 2),
			new TilePoint(4, 4),
		]);
		drawable.PhantomEnd = new PointF(360f, 200f);

		if (withTileOffsets)
		{
			for (var i = 0; i < drawable.Floor.Areas.Count; i++)
				drawable.SetPendingMoveOffset(MapTestScene.AreaId(i), new TilePoint(2, 3));
		}

		return drawable;
	}

	/// <summary>
	///		Bytes allocated per <c>Draw</c> call in steady state, after the JIT and the label cache
	///		have warmed.
	/// </summary>
	private static long BytesPerFrame(FloorMapDrawable drawable)
	{
		var canvas = new RecordingCanvas { Recording = false };

		for (var i = 0; i < WarmupFrames; i++)
			drawable.Draw(canvas, FrameRect);

		var before = GC.GetAllocatedBytesForCurrentThread();

		for (var i = 0; i < MeasuredFrames; i++)
			drawable.Draw(canvas, FrameRect);

		return (GC.GetAllocatedBytesForCurrentThread() - before) / MeasuredFrames;
	}

	/// <summary>
	///		A frame that paints twenty MOVED areas allocates no more than the same frame painting
	///		them unmoved: after MASTER-097 the tile offset is folded into the pixel conversion
	///		instead of building a shifted <see cref="TilePoint"/> — a record, and so a heap
	///		allocation — for every segment end and every curve control point.
	/// </summary>
	/// <remarks>
	///		This is a difference rather than a byte ceiling, because a ceiling here would be noise.
	///		Measured on this scene with <c>dotnet test</c> on 2026-09-01: before MASTER-097 the
	///		moved frame allocated 24,664 bytes and the unmoved frame 17,848; after it, both
	///		allocate 17,208. The difference the offsets used to cost -- 6,816 bytes a frame -- is
	///		now exactly zero, which is a sharp assertion, while the 17,208 that remains is almost
	///		all <c>PathF</c> and its point lists and would need a wide, uninformative ceiling.
	/// </remarks>
	[Fact]
	public void MovedAreas_AllocateNoMorePerFrameThanUnmovedOnes()
	{
		var moved = BytesPerFrame(RepresentativeDrawable(withTileOffsets: true));
		var unmoved = BytesPerFrame(RepresentativeDrawable(withTileOffsets: false));

		Assert.True(
			moved <= unmoved,
			$"a frame painting moved areas allocated {moved} bytes against {unmoved} for the same "
			+ "areas unmoved, so the tile offset is allocating per point again");
	}

	/// <summary>
	///		Two frames drawn with the pointer at the same distance from the last vertex paint the
	///		same distance-label INSTANCE, because the label is formatted only when the number
	///		changes rather than once per frame.
	/// </summary>
	[Fact]
	public void PhantomDistanceLabel_IsTheSameInstanceAcrossFramesAtTheSameDistance()
	{
		var drawable = MapTestScene.Drawable();
		drawable.TileSize = 40f;
		drawable.Floor = MapTestScene.FloorWith();
		drawable.SetInProgressVertices([new TilePoint(0, 0)]);
		drawable.PhantomEnd = new PointF(120f, 0f);

		var canvas = new RecordingCanvas();
		drawable.Draw(canvas, FrameRect);
		drawable.Draw(canvas, FrameRect);

		Assert.Equal(2, canvas.DrawnStrings.Count);
		Assert.Same(canvas.DrawnStrings[0], canvas.DrawnStrings[1]);
	}

	/// <summary>
	///		Moving the pointer to a new distance formats a new label, so the cache reports the
	///		distance the frame is actually showing rather than a stale one.
	/// </summary>
	[Fact]
	public void PhantomDistanceLabel_IsReformattedWhenTheDistanceChanges()
	{
		var drawable = MapTestScene.Drawable();
		drawable.TileSize = 40f;
		drawable.TileSizeInFeet = 5f;
		drawable.Floor = MapTestScene.FloorWith();
		drawable.SetInProgressVertices([new TilePoint(0, 0)]);

		var canvas = new RecordingCanvas();

		drawable.PhantomEnd = new PointF(120f, 0f);
		drawable.Draw(canvas, FrameRect);

		drawable.PhantomEnd = new PointF(200f, 0f);
		drawable.Draw(canvas, FrameRect);

		Assert.Equal(["15'", "25'"], canvas.DrawnStrings);
	}
}
