using DungeonMaster.App.Controls;
using DungeonMaster.App.UnitTests.Fixtures;

using FluentAssertions;

namespace DungeonMaster.App.UnitTests.Controls;

/// <summary>
///		Guards the notch clamp in <see cref="BookmarkTabShape.PathForBounds"/>: the notch may rise
///		at most to half the ribbon's height, so the apex never reaches the top edge and the path
///		never degenerates.
/// </summary>
/// <remarks>
///		<para>
///		THE DEFECT THESE FACTS EXIST FOR IS MASTER-148. The clamp was
///		<c>Math.Min(NotchDepth, bounds.Height)</c>, which permits <c>notch == bounds.Height</c>
///		exactly. At that value the apex lands on <c>bounds.Top</c> and the ribbon collapses to two
///		triangles meeting at a point, which renders as nothing at all and reads as a missing
///		control rather than as a bug. The full height was therefore the degenerate case the
///		surrounding comment claimed to prevent, not the safe limit.
///		</para>
///		<para>
///		THE APEX IS THE 4TH POINT OF THE PATH, index 3. The path is built as
///		<c>MoveTo(left, top)</c>, <c>LineTo(right, top)</c>, <c>LineTo(right, bottom)</c>,
///		<c>LineTo(center.X, bottom - notch)</c>, <c>LineTo(left, bottom)</c>, so index 3 is the
///		notch apex and its <c>Y</c> is <c>bottom - notch</c>. These facts read that point rather
///		than recomputing the clamp, so they assert the geometry the renderer receives and not a
///		copy of the arithmetic under test.
///		</para>
///		<para>
///		IT JOINS <see cref="AppTestCollection"/> WITHOUT INJECTING <c>AppFixture</c>, on the
///		precedent of <c>MauiPropertyTypeTests</c>: <see cref="BookmarkTabShape"/> is a plain sealed
///		class implementing <c>IShape</c> with no <c>Element</c> involvement, so there is nothing to
///		ask the fixture for, but touching it runs the application assembly's type initialisers and
///		the collection keeps that sequential.
///		</para>
/// </remarks>
[Collection(AppTestCollection.Name)]
public class BookmarkTabShapeNotchTests
{
	/// <summary>
	///		The default <see cref="BookmarkTabShape.NotchDepth"/>, duplicated here so a change to
	///		the production default makes these facts fail rather than silently re-scaling them.
	/// </summary>
	private const double DefaultNotchDepth = 10;

	/// <summary>
	///		The index of the notch apex in the path <see cref="BookmarkTabShape.PathForBounds"/>
	///		builds.
	/// </summary>
	private const int ApexIndex = 3;

	/// <summary>
	///		Asserts that the production default is still the value these facts were written
	///		against. Every height below is chosen relative to it, so a changed default would move
	///		each case into a different branch of the clamp without failing anything.
	/// </summary>
	[Fact]
	public void TheDefaultNotchDepthIsStillTheValueTheseFactsAreScaledTo()
	{
		new BookmarkTabShape().NotchDepth.Should().Be(
			DefaultNotchDepth,
			"every height in this class is chosen relative to the default notch depth");
	}

	/// <summary>
	///		At every height below twice the default notch depth of 10 the half-height clamp binds,
	///		and the apex must stay strictly below the top edge. Height 10 is the case MASTER-148 is
	///		about, and the only one of these three the old clamp reached: there it yielded a notch
	///		of exactly 10, equal to the height, and the apex landed exactly on <c>bounds.Top</c>.
	///		At 15 and 19 the old clamp yielded 10 as well, leaving the apex at 5 and at 9, both
	///		strictly below the top edge, so neither of those cases discriminates against it. Only
	///		height 10 does.
	/// </summary>
	/// <param name="height">
	///		The ribbon height, positive and strictly smaller than twice the default notch depth.
	/// </param>
	[Theory]
	[InlineData(10.0)]
	[InlineData(15.0)]
	[InlineData(19.0)]
	public void ApexStaysStrictlyBelowTheTopEdge_AtHeightsUnderTwiceTheNotchDepth(double height)
	{
		var bounds = new Rect(0, 0, 40, height);
		height.Should().BeLessThan(
			2 * DefaultNotchDepth,
			"this fact is about the clamped branch, so the case must sit inside it");

		var path = new BookmarkTabShape().PathForBounds(bounds);

		var apex = path[ApexIndex];
		apex.Y.Should().BeGreaterThan(
			(float)bounds.Top,
			"the notch apex must leave a strictly positive band of ribbon above it");
		apex.Y.Should().BeLessThan(
			(float)bounds.Bottom,
			"an apex on the bottom edge would mean no notch at all");
	}

	/// <summary>
	///		At a height of 20, 40 or 60 with the default notch depth of 10 the clamp is not the
	///		binding term, so the notch is exactly the notch depth and the change made for
	///		MASTER-148 is invisible to the geometry a ribbon of that height already rendered.
	/// </summary>
	/// <param name="height">
	///		The ribbon height, at least twice the default notch depth.
	/// </param>
	[Theory]
	[InlineData(20.0)]
	[InlineData(40.0)]
	[InlineData(60.0)]
	public void NotchIsExactlyTheNotchDepth_AtHeightsOfAtLeastTwiceTheNotchDepth(double height)
	{
		var bounds = new Rect(0, 0, 40, height);
		height.Should().BeGreaterThanOrEqualTo(
			2 * DefaultNotchDepth,
			"this fact is about the unclamped branch, so the case must sit inside it");

		var path = new BookmarkTabShape().PathForBounds(bounds);

		var apex = path[ApexIndex];
		((double)((float)bounds.Bottom - apex.Y)).Should().Be(
			DefaultNotchDepth,
			"above twice the notch depth the clamp must not bind, so the notch is the full depth");
	}
}
