using DungeonMaster.App.Graphics;
using Microsoft.Maui.Graphics;

namespace DungeonMaster.App.Tests.Graphics;

/// <summary>
///		Proves that a type declared in the <c>DungeonMaster.App</c> assembly can be CONSTRUCTED and
///		EXECUTED by a test, rather than asserted about as source text.
///		<para>
///		This is the whole point of the <c>DungeonMaster.App.Tests</c> project (MASTER-026 / XCUT-002).
///		Everything under <c>src/DungeonMaster.Tests/App/</c> reads App source off disk with a regular
///		expression, because <c>DungeonMaster.Tests</c> targets <c>net10.0</c> and cannot reference a
///		MAUI project. <see cref="FloorMapDrawable"/> is a real subject for that proof and not a
///		contrivance: it implements <see cref="IDrawable"/> and its signature names
///		<see cref="PointF"/>, so this file cannot compile unless MAUI types are genuinely resolvable
///		here.
///		</para>
///		<para>
///		<c>ScreenToCanvas</c> is chosen because it is the MAUI-typed half of the drawable that needs
///		no application host. Per <c>contracts/new-projects.md</c> §3.3 these tests run OUTSIDE a MAUI
///		host — <c>MauiProgram.CreateMauiApp()</c> is never called, there is no
///		<c>Application.Current</c> and no handler registry — so anything deriving from
///		<c>ContentView</c>, <c>ContentPage</c> or <c>Shell</c> is out of reach. Whether that half can
///		be reached at all is Session 6's question and is not answered here.
///		</para>
/// </summary>
public class FloorMapDrawableExecutionTests
{
	/// <summary>
	///		Inverts the pan-and-zoom transform: a screen point is shifted by the pan offset and then
	///		divided by the zoom level. With a pan of (10, 20) and a zoom of 2, the screen point
	///		(50, 80) is the canvas point (20, 30).
	/// </summary>
	[Fact]
	public void ScreenToCanvasInvertsPanAndZoom()
	{
		var drawable = new FloorMapDrawable
		{
			PanOffset = new PointF(10f, 20f),
			ZoomLevel = 2f,
		};

		var canvas = drawable.ScreenToCanvas(new PointF(50f, 80f));

		Assert.Equal(20f, canvas.X, 3);
		Assert.Equal(30f, canvas.Y, 3);
	}
}
