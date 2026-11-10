using DungeonMaster.App.Graphics;
using Microsoft.Maui.Graphics;

namespace DungeonMaster.App.Tests.Graphics;

/// <summary>
///		Pins what <see cref="FloorMapDrawable.ScreenToCanvas"/> returns for a degenerate <see cref="FloorMapDrawable.ZoomLevel"/> or a non-finite <see cref="FloorMapDrawable.PanOffset"/>, and records whether production can reach either state.
///		The division at <c>FloorMapDrawable.cs:629-630</c> is the one arithmetic operation in the chain that ends in a persisted <c>CanvasPoint</c>: the call at <c>DungeonDetailPage.xaml.cs:1437</c> feeds the conversion at <c>DungeonDetailPage.xaml.cs:1442</c>.
///		The drawable is constructed with <c>new FloorMapDrawable { ... }</c> and no MAUI host, exactly as <c>FloorMapDrawableExecutionTests.cs</c> does.
/// </summary>
/// <remarks>
///		<para>
///		FINDING, on the question of whether an input reachable from <c>DungeonDetailPage.xaml.cs:1437</c> can leave <c>ZoomLevel</c> or <c>PanOffset</c> non-finite.
///		</para>
///		<para>
///		<c>ZoomLevel</c>: NO, on every target.
///		Every production write of <c>ZoomLevel</c> takes its value from <see cref="DungeonMaster.App.Views.DungeonDetail.ZoomLevelGuard.Next"/>: the one assignment site is inside <c>ApplyZoom</c>, and every zoom input reaches it through <c>ApplyZoom</c>, including <c>ResetViewport</c>, which requests <c>1.0f</c>.
///		That guard returns the current level unchanged for a non-finite <c>raw</c> and clamps a finite one into <c>[0.25, 4.0]</c>, so starting from the finite <c>1.0f</c> default every level it produces is finite and inside that range.
///		<c>ZoomLevel</c> is therefore never <c>0f</c> either, which makes the two <c>ZoomLevel == 0f</c> tests below unreachable-by-construction rather than reproductions of a live defect.
///		</para>
///		<para>
///		That conclusion depends on the guard's finiteness test, because of one mechanism claim which was RUN rather than assumed.
///		<c>Math.Clamp</c> does NOT sanitize <c>NaN</c>, as <see cref="MathClampWithNaNReturnsNaNRatherThanABound"/> shows, so a clamp alone would pass a <c>NaN</c> <c>raw</c> straight through to <c>ZoomLevel</c>; that is what makes the guard necessary.
///		<see cref="DungeonMaster.App.Views.DungeonDetail.ZoomLevelGuard.Next"/> tests <c>raw</c> for finiteness before it snaps or clamps, and returns the current level for a <c>NaN</c> or infinite <c>raw</c>, as <see cref="DungeonMaster.App.Tests.Views.ZoomLevelGuardTests"/> pins.
///		That covers the one caller that could supply a <c>NaN</c> <c>raw</c>, the pinch handler's <c>ApplyZoom(_pinchStartZoom * (float)e.Scale)</c> inside the <c>#elif MACCATALYST</c> fence, as much as the Windows callers.
///		</para>
///		<para>
///		<c>PanOffset</c>: NOT closed by source inspection.
///		Its only non-constant writer accumulates raw touch coordinates as <c>_drawable.PanOffset.X + delta.X</c>, where <c>delta</c> is built from <c>e.Touches[0].X</c> and <c>.Y</c>.
///		Nothing on that path tests the incoming coordinate for finiteness, and no later writer sanitizes <c>PanOffset</c> except <c>ResetViewport</c>.
///		A single non-finite touch coordinate would therefore make <c>PanOffset</c> permanently <c>NaN</c> and every subsequent <c>ScreenToCanvas</c> result <c>NaN</c>, which <c>DungeonDetailPage.xaml.cs:1442</c> then persists as a <c>CanvasPoint</c>.
///		Whether the MAUI Windows pointer pipeline can ever deliver such a coordinate is not decidable from this repository's source and is NOT asserted here.
///		Reaching <c>±Infinity</c> by accumulation alone is not practically possible, because each <c>delta</c> is bounded by the canvas pixel extent.
///		</para>
///		<para>
///		ASSIGNMENT SITES CHECKED in reaching that answer, the complete set.
///		They come from <c>grep -rn "ZoomLevel"</c> and <c>grep -rn "PanOffset"</c> over <c>src/</c> with <c>--include=*.cs --include=*.xaml</c>, filtered through <c>grep -Ev "[\\/](obj|bin)[\\/]"</c>, with every hit then classified by eye as a write or a read.
///		Only the <c>ZoomLevel</c> text was corrected after <c>ZoomLevelGuard</c> was introduced, namely the two paragraphs above on <c>ZoomLevel</c> and <c>Math.Clamp</c>, list items 3 and 4, and the summary of <see cref="MathClampWithNaNReturnsNaNRatherThanABound"/>, and every line-number citation elsewhere in this file was not re-measured by that correction.
///		<list type="number">
///		<item><c>src/DungeonMaster.App/Graphics/FloorMapDrawable.cs:439</c>, the <c>ZoomLevel</c> declaration initializer <c>= 1.0f</c>, finite.</item>
///		<item><c>src/DungeonMaster.App/Graphics/FloorMapDrawable.cs:442</c>, the <c>PanOffset</c> declaration initializer <c>= PointF.Zero</c>, finite.</item>
///		<item><c>src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:520</c>, <c>_drawable.ZoomLevel = ZoomLevelGuard.Next(_drawable.ZoomLevel, raw);</c> inside <c>ApplyZoom</c> at <c>:518</c>, the only production <c>ZoomLevel</c> write, whose callers are <c>:513</c> and <c>:516</c> unfenced, <c>:528</c> inside <c>ResetViewport</c>, <c>:617</c> inside <c>#if WINDOWS</c> spanning <c>:606-619</c>, and <c>:631</c> inside <c>#elif MACCATALYST</c> spanning <c>:619-635</c>.</item>
///		<item><c>src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:528</c>, <c>ApplyZoom(1.0f);</c> inside <c>ResetViewport</c> at <c>:525</c>, which no longer writes <c>ZoomLevel</c> directly: its finite <c>1.0f</c> reaches the property through <c>ZoomLevelGuard.Next</c>.</item>
///		<item><c>src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:508</c>, <c>_drawable.PanOffset = PointF.Zero;</c> inside <c>ResetViewport</c>, finite.</item>
///		<item><c>src/DungeonMaster.App/Views/DungeonDetailPage/DungeonDetailPage.xaml.cs:1632-1634</c>, the pan-drag accumulator <c>_drawable.PanOffset = new PointF(_drawable.PanOffset.X + delta.X, _drawable.PanOffset.Y + delta.Y);</c>, the one unbounded writer.</item>
///		</list>
///		Two test-only writes were excluded from the reachability question: <c>FloorMapDrawableExecutionTests.cs:39-40</c> and this file.
///		</para>
///		<para>
///		Why that grep is exhaustive over the ways either property can be set, with each part checked rather than assumed.
///		<see cref="FloorMapDrawable"/> is declared <c>sealed class FloorMapDrawable : IDrawable, IDisposable</c> at <c>FloorMapDrawable.cs:98</c> and so is not a <c>BindableObject</c>, and <c>grep -c BindableProperty</c> over <c>FloorMapDrawable.cs</c> returns <c>0</c>, so neither property is reachable through the MAUI property system or a XAML binding.
///		The only occurrences of <c>FloorMapDrawable</c> in any <c>.xaml</c> are two prose lines inside a comment block, at <c>Resources/Theme/Tokens.xaml:263</c> and <c>:267</c>, which are not bindings.
///		No string literal or <c>nameof</c> occurrence of either property name exists anywhere under <c>src/</c> outside this comment, so no reflective write by literal name exists either.
///		What remains unexcluded is a reflective write through a name assembled at run time, which no command can rule out.
///		</para>
///		<para>
///		<see cref="ScreenToCanvas_WithFinitePanOffsetAndUnitZoom_ReturnsFinitePoint"/> is the positive control.
///		A <c>ScreenToCanvas</c> that returned something non-finite for every input would satisfy the four degenerate tests while proving nothing, so that control is what stops a run in which every degenerate case reports non-finite from being misread.
///		</para>
/// </remarks>
public class FloorMapDrawableNonFiniteTests
{
	/// <summary>
	///		A zero <c>ZoomLevel</c> with a screen point away from the pan offset leaves both numerators non-zero, so the division at <c>FloorMapDrawable.cs:629-630</c> is non-zero over zero.
	/// </summary>
	[Fact]
	public void ScreenToCanvas_WithZeroZoomAndScreenPointAwayFromPanOffset_ReturnsSignedInfinities()
	{
		var drawable = new FloorMapDrawable
		{
			PanOffset = new PointF(10f, 20f),
			ZoomLevel = 0f,
		};

		var canvas = drawable.ScreenToCanvas(new PointF(50f, 0f));

		Assert.Equal(float.PositiveInfinity, canvas.X);
		Assert.Equal(float.NegativeInfinity, canvas.Y);
	}

	/// <summary>
	///		A zero <c>ZoomLevel</c> with a screen point equal to the pan offset leaves both numerators zero, so the division is zero over zero.
	/// </summary>
	[Fact]
	public void ScreenToCanvas_WithZeroZoomAndScreenPointEqualToPanOffset_ReturnsNaN()
	{
		var drawable = new FloorMapDrawable
		{
			PanOffset = new PointF(10f, 20f),
			ZoomLevel = 0f,
		};

		var canvas = drawable.ScreenToCanvas(new PointF(10f, 20f));

		Assert.True(float.IsNaN(canvas.X), $"expected NaN X, got {canvas.X}");
		Assert.True(float.IsNaN(canvas.Y), $"expected NaN Y, got {canvas.Y}");
	}

	/// <summary>
	///		A <c>NaN</c> in one <c>PanOffset</c> component poisons only that component's subtraction.
	/// </summary>
	[Fact]
	public void ScreenToCanvas_WithNaNPanOffsetX_ReturnsNaNXAndFiniteY()
	{
		var drawable = new FloorMapDrawable
		{
			PanOffset = new PointF(float.NaN, 20f),
			ZoomLevel = 1f,
		};

		var canvas = drawable.ScreenToCanvas(new PointF(50f, 80f));

		Assert.True(float.IsNaN(canvas.X), $"expected NaN X, got {canvas.X}");
		Assert.Equal(60f, canvas.Y, 3);
	}

	/// <summary>
	///		Both <c>PanOffset</c> components <c>NaN</c> is the shape that reaches <c>DungeonDetailPage.xaml.cs:1442</c> and becomes a fully non-finite persisted <c>CanvasPoint</c>.
	/// </summary>
	[Fact]
	public void ScreenToCanvas_WithBothPanOffsetComponentsNaN_ReturnsNaNInBothComponents()
	{
		var drawable = new FloorMapDrawable
		{
			PanOffset = new PointF(float.NaN, float.NaN),
			ZoomLevel = 1f,
		};

		var canvas = drawable.ScreenToCanvas(new PointF(50f, 80f));

		Assert.True(float.IsNaN(canvas.X), $"expected NaN X, got {canvas.X}");
		Assert.True(float.IsNaN(canvas.Y), $"expected NaN Y, got {canvas.Y}");
	}

	/// <summary>
	///		The positive control for the four degenerate cases above.
	/// </summary>
	[Fact]
	public void ScreenToCanvas_WithFinitePanOffsetAndUnitZoom_ReturnsFinitePoint()
	{
		var drawable = new FloorMapDrawable
		{
			PanOffset = new PointF(10f, 20f),
			ZoomLevel = 1f,
		};

		var canvas = drawable.ScreenToCanvas(new PointF(50f, 80f));

		Assert.True(float.IsFinite(canvas.X), $"expected finite X, got {canvas.X}");
		Assert.True(float.IsFinite(canvas.Y), $"expected finite Y, got {canvas.Y}");
		Assert.Equal(40f, canvas.X, 3);
		Assert.Equal(60f, canvas.Y, 3);
	}

	/// <summary>
	///		The mechanism claim the <c>ZoomLevel</c> half of this class's finding rests on, run rather than assumed.
	///		<c>Math.Clamp</c> returns <c>NaN</c> unchanged instead of substituting a bound, while clamping both infinities to the bounds.
	///		The bounds are the values of <c>ZoomLevelGuard.MinimumZoom</c> and <c>ZoomLevelGuard.MaximumZoom</c>, but this test asserts about <c>Math.Clamp</c> alone and would not notice an edit to those constants.
	/// </summary>
	[Fact]
	public void MathClampWithNaNReturnsNaNRatherThanABound()
	{
		var clampedNaN = Math.Clamp(float.NaN, 0.25f, 4.0f);

		Assert.True(float.IsNaN(clampedNaN), $"expected NaN, got {clampedNaN}");
		Assert.Equal(4.0f, Math.Clamp(float.PositiveInfinity, 0.25f, 4.0f));
		Assert.Equal(0.25f, Math.Clamp(float.NegativeInfinity, 0.25f, 4.0f));
	}
}
