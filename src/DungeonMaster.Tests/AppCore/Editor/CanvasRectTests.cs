using DungeonMaster.App.Core.Editor;
using FluentAssertions;

namespace DungeonMaster.Tests.AppCore.Editor;

public class CanvasRectTests
{
	// Spans 10..40 horizontally and 20..60 vertically.
	private static CanvasRect Rect() => new(10f, 20f, 30f, 40f);

	[Fact]
	public void Contains_PointStrictlyInside_ReturnsTrue()
	{
		Rect().Contains(25f, 40f).Should().BeTrue();
	}

	[Theory]
	[InlineData(9f, 40f)]
	[InlineData(41f, 40f)]
	[InlineData(25f, 19f)]
	[InlineData(25f, 61f)]
	public void Contains_PointOutsideOnOneOfTheFourSides_ReturnsFalse(float x, float y)
	{
		Rect().Contains(x, y).Should().BeFalse();
	}

	[Theory]
	[InlineData(10f, 40f)]
	[InlineData(40f, 40f)]
	[InlineData(25f, 20f)]
	[InlineData(25f, 60f)]
	public void Contains_PointOnOneOfTheFourEdges_ReturnsTrueBecauseEdgesAreInclusive(float x, float y)
	{
		Rect().Contains(x, y).Should().BeTrue();
	}

	[Theory]
	[InlineData(10f, 20f)]
	[InlineData(40f, 20f)]
	[InlineData(40f, 60f)]
	[InlineData(10f, 60f)]
	public void Contains_PointOnOneOfTheFourCorners_ReturnsTrue(float x, float y)
	{
		Rect().Contains(x, y).Should().BeTrue();
	}

	[Fact]
	public void Contains_ZeroSizedRectangleAtItsOwnOrigin_ReturnsTrue()
	{
		new CanvasRect(5f, 5f, 0f, 0f).Contains(5f, 5f).Should().BeTrue();
	}
}
