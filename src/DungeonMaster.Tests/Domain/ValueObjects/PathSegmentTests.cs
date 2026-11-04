using DungeonMaster.Domain.ValueObjects;
using FluentAssertions;

namespace DungeonMaster.Tests.Domain.ValueObjects;

/// <summary>
///		Covers the bidirectional control-point invariant that the <c>PathSegment</c> primary constructor enforces
///		(MASTER-170). A <c>CubicBezier</c> segment requires both control points; a <c>Line</c> segment requires
///		neither. Every rejection is asserted with <c>ThrowExactly</c> so that an <c>ArgumentNullException</c>, which
///		derives from <c>ArgumentException</c>, would fail the assertion, and with <c>ParamName</c> so that an
///		exception thrown for some other reason cannot satisfy it.
/// </summary>
public class PathSegmentTests
{
	[Fact]
	public void CubicBezier_WithBothControlPoints_Succeeds()
	{
		// Act
		var segment = new PathSegment(
			PathSegmentKind.CubicBezier,
			new TilePoint(4, 4),
			new TilePoint(6, 1),
			new TilePoint(6, 3));

		// Assert
		segment.Kind.Should().Be(PathSegmentKind.CubicBezier);
		segment.End.Should().Be(new TilePoint(4, 4));
		segment.Ctrl1.Should().Be(new TilePoint(6, 1));
		segment.Ctrl2.Should().Be(new TilePoint(6, 3));
	}

	[Fact]
	public void CubicBezier_WithNullCtrl1_ThrowsArgumentExceptionNamingCtrl1()
	{
		// Act
		var act = () => new PathSegment(
			PathSegmentKind.CubicBezier,
			new TilePoint(4, 4),
			null,
			new TilePoint(6, 3));

		// Assert
		act.Should().ThrowExactly<ArgumentException>()
			.Which.ParamName.Should().Be("Ctrl1");
	}

	[Fact]
	public void CubicBezier_WithNullCtrl2_ThrowsArgumentExceptionNamingCtrl2()
	{
		// Act
		var act = () => new PathSegment(
			PathSegmentKind.CubicBezier,
			new TilePoint(4, 4),
			new TilePoint(6, 1),
			null);

		// Assert
		act.Should().ThrowExactly<ArgumentException>()
			.Which.ParamName.Should().Be("Ctrl2");
	}

	[Fact]
	public void CubicBezier_WithBothControlPointsNull_ThrowsArgumentExceptionNamingCtrl1()
	{
		// Act
		var act = () => new PathSegment(
			PathSegmentKind.CubicBezier,
			new TilePoint(4, 4),
			null,
			null);

		// Assert
		act.Should().ThrowExactly<ArgumentException>()
			.Which.ParamName.Should().Be("Ctrl1");
	}

	[Fact]
	public void Line_WithNoControlPoints_Succeeds()
	{
		// Act
		var segment = new PathSegment(
			PathSegmentKind.Line,
			new TilePoint(2, 3),
			null,
			null);

		// Assert
		segment.Kind.Should().Be(PathSegmentKind.Line);
		segment.End.Should().Be(new TilePoint(2, 3));
		segment.Ctrl1.Should().BeNull();
		segment.Ctrl2.Should().BeNull();
	}

	[Fact]
	public void Line_CarryingCtrl1_ThrowsArgumentExceptionNamingCtrl1()
	{
		// Act
		var act = () => new PathSegment(
			PathSegmentKind.Line,
			new TilePoint(2, 3),
			new TilePoint(1, 1),
			null);

		// Assert
		act.Should().ThrowExactly<ArgumentException>()
			.Which.ParamName.Should().Be("Ctrl1");
	}

	[Fact]
	public void Line_CarryingCtrl2_ThrowsArgumentExceptionNamingCtrl2()
	{
		// Act
		var act = () => new PathSegment(
			PathSegmentKind.Line,
			new TilePoint(2, 3),
			null,
			new TilePoint(1, 1));

		// Assert
		act.Should().ThrowExactly<ArgumentException>()
			.Which.ParamName.Should().Be("Ctrl2");
	}

	[Fact]
	public void UndefinedKind_ThrowsArgumentOutOfRangeExceptionNamingKind()
	{
		// Arrange
		var end = new TilePoint(2, 3);

		// Act
		var act = () => new PathSegment((PathSegmentKind)2, end, null, null);

		// Assert
		act.Should().ThrowExactly<ArgumentOutOfRangeException>()
			.WithParameterName("Kind");
	}
}
