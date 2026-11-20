using DungeonMaster.App.Core.Map;
using DungeonMaster.Domain.ValueObjects;
using FluentAssertions;

namespace DungeonMaster.Tests.AppCore.Map;

public class AreaOutlineGeometryTests
{
	private const float TileSize = 10f;

	[Fact]
	public void Translate_NonZeroDelta_OffsetsEndAndBothControlPointsOfEverySegment()
	{
		// Arrange
		IReadOnlyList<PathSegment> outline =
		[
			new(PathSegmentKind.CubicBezier, new TilePoint(2, 3), new TilePoint(4, 5), new TilePoint(6, 7)),
			new(PathSegmentKind.CubicBezier, new TilePoint(-1, -2), new TilePoint(0, 0), new TilePoint(1, 1)),
		];

		// Act
		var result = AreaOutlineGeometry.Translate(outline, new TilePoint(10, 20));

		// Assert
		result.Should().HaveCount(2);
		result[0].End.Should().Be(new TilePoint(12, 23));
		result[0].Ctrl1.Should().Be(new TilePoint(14, 25));
		result[0].Ctrl2.Should().Be(new TilePoint(16, 27));
		result[1].End.Should().Be(new TilePoint(9, 18));
		result[1].Ctrl1.Should().Be(new TilePoint(10, 20));
		result[1].Ctrl2.Should().Be(new TilePoint(11, 21));
	}

	[Fact]
	public void Translate_NonZeroDelta_PreservesSegmentKind()
	{
		// Arrange
		IReadOnlyList<PathSegment> outline =
		[
			Line(1, 1),
			new(PathSegmentKind.CubicBezier, new TilePoint(2, 2), new TilePoint(3, 3), new TilePoint(4, 4)),
		];

		// Act
		var result = AreaOutlineGeometry.Translate(outline, new TilePoint(1, 1));

		// Assert
		result[0].Kind.Should().Be(PathSegmentKind.Line);
		result[1].Kind.Should().Be(PathSegmentKind.CubicBezier);
	}

	[Fact]
	public void Translate_SegmentWithBothControlPointsNull_KeepsThemNull()
	{
		// Arrange
		IReadOnlyList<PathSegment> outline = [new(PathSegmentKind.Line, new TilePoint(2, 3), null, null)];

		// Act
		var result = AreaOutlineGeometry.Translate(outline, new TilePoint(5, 5));

		// Assert
		result[0].End.Should().Be(new TilePoint(7, 8));
		result[0].Ctrl1.Should().BeNull();
		result[0].Ctrl2.Should().BeNull();
	}

	[Fact]
	public void Translate_EmptyOutline_ReturnsEmptyList()
	{
		// Act
		var result = AreaOutlineGeometry.Translate([], new TilePoint(3, 4));

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public void Translate_ZeroDelta_ReturnsAnOutlineEqualInValueToTheInput()
	{
		// Arrange
		var outline = Rectangle();

		// Act
		var result = AreaOutlineGeometry.Translate(outline, new TilePoint(0, 0));

		// Assert
		result.Should().Equal(outline);
	}

	[Fact]
	public void Translate_NullOutline_ThrowsArgumentNullException()
	{
		// Act
		var act = () => AreaOutlineGeometry.Translate(null!, new TilePoint(1, 1));

		// Assert
		act.Should().Throw<ArgumentNullException>();
	}

	[Fact]
	public void Translate_NullElementInOutline_ThrowsArgumentNullException()
	{
		// Arrange
		IReadOnlyList<PathSegment> outline = [Line(0, 0), null!];

		// Act
		var act = () => AreaOutlineGeometry.Translate(outline, new TilePoint(1, 1));

		// Assert
		act.Should().Throw<ArgumentNullException>();
	}

	[Fact]
	public void Translate_NullDelta_ThrowsArgumentNullException()
	{
		// Act
		var act = () => AreaOutlineGeometry.Translate(Rectangle(), null!);

		// Assert
		act.Should().Throw<ArgumentNullException>();
	}

	[Fact]
	public void Translate_LineSegmentCarryingAControlPoint_ThrowsArgumentExceptionNamingCtrl1()
	{
		// Arrange
		// A with expression skips the constructor's validation, so it is the only way to build the invalid segment.
		var invalid = new PathSegment(PathSegmentKind.Line, new TilePoint(2, 3), null, null) with
		{
			Ctrl1 = new TilePoint(4, 5),
		};
		IReadOnlyList<PathSegment> outline = [invalid];

		// Act
		var act = () => AreaOutlineGeometry.Translate(outline, new TilePoint(1, 1));

		// Assert
		act.Should().ThrowExactly<ArgumentException>().WithParameterName("Ctrl1");
	}

	[Fact]
	public void ToPolygon_ClosedOutline_EmitsTheLastSegmentEndFirstThenEverySegmentEndInOrder()
	{
		// Act
		var result = AreaOutlineGeometry.ToPolygon(Rectangle(), TileSize);

		// Assert
		result.Should().Equal(
			(0f, 30f),
			(0f, 0f),
			(40f, 0f),
			(40f, 30f));
	}

	[Fact]
	public void ToPolygon_ClosedOutline_ReturnsOneVertexPerSegmentBecauseTheDuplicateIsDropped()
	{
		// Arrange
		var outline = Rectangle();

		// Act
		var result = AreaOutlineGeometry.ToPolygon(outline, TileSize);

		// Assert
		result.Should().HaveCount(outline.Count);
	}

	[Fact]
	public void ToPolygon_SingleSegmentOutline_ReturnsThatSingleVertexOnce()
	{
		// Arrange
		IReadOnlyList<PathSegment> outline = [Line(2, 5)];

		// Act
		var result = AreaOutlineGeometry.ToPolygon(outline, TileSize);

		// Assert
		result.Should().Equal((20f, 50f));
	}

	[Fact]
	public void ToPolygon_EmptyOutline_ReturnsEmptyList()
	{
		// Act
		var result = AreaOutlineGeometry.ToPolygon([], TileSize);

		// Assert
		result.Should().BeEmpty();
	}

	[Fact]
	public void ToPolygon_NullOutline_ThrowsArgumentNullException()
	{
		// Act
		var act = () => AreaOutlineGeometry.ToPolygon(null!, TileSize);

		// Assert
		act.Should().Throw<ArgumentNullException>();
	}

	[Fact]
	public void ToPolygon_NullElementInOutline_ThrowsArgumentNullException()
	{
		// Arrange
		IReadOnlyList<PathSegment> outline = [Line(0, 0), null!];

		// Act
		var act = () => AreaOutlineGeometry.ToPolygon(outline, TileSize);

		// Assert
		act.Should().Throw<ArgumentNullException>();
	}

	[Theory]
	[InlineData(0f)]
	[InlineData(-1f)]
	public void ToPolygon_TileSizeNotGreaterThanZero_ThrowsArgumentOutOfRangeException(float tileSize)
	{
		// Act
		var act = () => AreaOutlineGeometry.ToPolygon(Rectangle(), tileSize);

		// Assert
		act.Should().Throw<ArgumentOutOfRangeException>();
	}

	[Fact]
	public void ContainsPoint_PointInsideThePolygon_ReturnsTrue()
	{
		AreaOutlineGeometry.ContainsPoint(Square(), 20f, 20f).Should().BeTrue();
	}

	[Theory]
	[InlineData(-5f, 20f)]
	[InlineData(45f, 20f)]
	[InlineData(20f, -5f)]
	[InlineData(20f, 45f)]
	public void ContainsPoint_PointOutsideThePolygon_ReturnsFalse(float x, float y)
	{
		AreaOutlineGeometry.ContainsPoint(Square(), x, y).Should().BeFalse();
	}

	[Fact]
	public void ContainsPoint_EmptyPolygon_ReturnsFalse()
	{
		// Pins the documented intent of the n < 3 guard in ContainsPoint rather than covering live logic: an empty polygon encloses no area, so the answer is false.
		AreaOutlineGeometry.ContainsPoint([], 0f, 0f).Should().BeFalse();
	}

	[Fact]
	public void ContainsPoint_PolygonOfOneVertex_ReturnsFalse()
	{
		// Pins the documented intent of the n < 3 guard in ContainsPoint rather than covering live logic: a single vertex encloses no area, so the answer is false.
		AreaOutlineGeometry.ContainsPoint([(0f, 0f)], 0f, 0f).Should().BeFalse();
	}

	[Fact]
	public void ContainsPoint_PolygonOfTwoVertices_ReturnsFalse()
	{
		// Pins the documented intent of the n < 3 guard in ContainsPoint rather than covering live logic: two vertices enclose no area, so the answer is false.
		AreaOutlineGeometry.ContainsPoint([(0f, 0f), (40f, 40f)], 20f, 20f).Should().BeFalse();
	}

	[Fact]
	public void ContainsPoint_NullPolygon_ThrowsArgumentNullException()
	{
		// Act
		var act = () => AreaOutlineGeometry.ContainsPoint(null!, 0f, 0f);

		// Assert
		act.Should().Throw<ArgumentNullException>();
	}

	[Fact]
	public void Area_SquareWoundOneWay_ReturnsTheProductOfItsSides()
	{
		AreaOutlineGeometry.Area(Square()).Should().Be(1600f);
	}

	[Fact]
	public void Area_SameSquareWoundTheOtherWay_ReturnsTheSamePositiveValue()
	{
		// Arrange
		IReadOnlyList<(float X, float Y)> reversed = [(0f, 40f), (40f, 40f), (40f, 0f), (0f, 0f)];

		// Act
		var result = AreaOutlineGeometry.Area(reversed);

		// Assert
		result.Should().Be(1600f);
		result.Should().Be(AreaOutlineGeometry.Area(Square()));
	}

	[Fact]
	public void Area_PolygonOfTwoVertices_ReturnsZero()
	{
		AreaOutlineGeometry.Area([(0f, 0f), (40f, 40f)]).Should().Be(0f);
	}

	[Fact]
	public void Area_NullPolygon_ThrowsArgumentNullException()
	{
		// Act
		var act = () => AreaOutlineGeometry.Area(null!);

		// Assert
		act.Should().Throw<ArgumentNullException>();
	}

	private static PathSegment Line(int col, int row) =>
		new(PathSegmentKind.Line, new TilePoint(col, row), null, null);

	// A 4x3 tile rectangle. Its segment end points are, in order, (0,0), (4,0), (4,3), (0,3).
	private static IReadOnlyList<PathSegment> Rectangle() =>
		[Line(0, 0), Line(4, 0), Line(4, 3), Line(0, 3)];

	// A 40x40 pixel square, so its area is 1600.
	private static IReadOnlyList<(float X, float Y)> Square() =>
		[(0f, 0f), (40f, 0f), (40f, 40f), (0f, 40f)];
}
