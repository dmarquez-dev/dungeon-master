using DungeonMaster.App.Core.Map;
using DungeonMaster.Domain.ValueObjects;
using FluentAssertions;

namespace DungeonMaster.Tests.AppCore.Map;

public class MapDrawingSessionTests
{
	[Fact]
	public void PlaceVertex_FirstVertex_ReturnsAdded()
	{
		// Arrange
		var session = new MapDrawingSession();

		// Act
		var result = session.PlaceVertex(new TilePoint(0, 0));

		// Assert
		result.Should().Be(DrawSessionResult.Added);
	}

	[Fact]
	public void PlaceVertex_FirstVertex_HoldsOneVertex()
	{
		// Arrange
		var session = new MapDrawingSession();

		// Act
		session.PlaceVertex(new TilePoint(0, 0));

		// Assert
		session.Vertices.Should().HaveCount(1);
	}

	[Fact]
	public void PlaceVertex_FirstVertex_ActivatesSession()
	{
		// Arrange
		var session = new MapDrawingSession();

		// Act
		session.PlaceVertex(new TilePoint(0, 0));

		// Assert
		session.IsActive.Should().BeTrue();
	}

	[Fact]
	public void PlaceVertex_DuplicateOfLast_ReturnsIgnored()
	{
		// Arrange
		var session = new MapDrawingSession();
		session.PlaceVertex(new TilePoint(1, 1));

		// Act
		var result = session.PlaceVertex(new TilePoint(1, 1));

		// Assert
		result.Should().Be(DrawSessionResult.Ignored);
		session.Vertices.Should().HaveCount(1);
	}

	[Fact]
	public void PlaceVertex_NearOriginWithFewerThanThree_ReturnsAdded()
	{
		// Arrange
		var session = new MapDrawingSession();
		session.PlaceVertex(new TilePoint(0, 0));

		// Act
		var result = session.PlaceVertex(new TilePoint(0, 1));

		// Assert
		result.Should().Be(DrawSessionResult.Added);
		session.Vertices.Should().HaveCount(2);
	}

	[Fact]
	public void PlaceVertex_ExactOriginWithThreeOrMore_ReturnsClosed()
	{
		// Arrange
		var session = new MapDrawingSession();
		session.PlaceVertex(new TilePoint(0, 0));
		session.PlaceVertex(new TilePoint(5, 0));
		session.PlaceVertex(new TilePoint(5, 5));

		// Act
		var result = session.PlaceVertex(new TilePoint(0, 0));

		// Assert
		result.Should().Be(DrawSessionResult.Closed);
	}

	[Fact]
	public void PlaceVertex_AdjacentToOriginWithThreeOrMore_ReturnsAdded()
	{
		// Arrange
		// Regression: a 1-tile-wide hallway places its 4th corner 1 tile from the origin.
		// That corner must not prematurely close the polygon.
		var session = new MapDrawingSession();
		session.PlaceVertex(new TilePoint(0, 0));
		session.PlaceVertex(new TilePoint(3, 0));
		session.PlaceVertex(new TilePoint(3, 1));

		// Act
		var result = session.PlaceVertex(new TilePoint(0, 1));

		// Assert
		result.Should().Be(DrawSessionResult.Added);
	}

	[Fact]
	public void UndoLastVertex_RemovesLast_ReturnsTrue()
	{
		// Arrange
		var session = new MapDrawingSession();
		session.PlaceVertex(new TilePoint(0, 0));
		session.PlaceVertex(new TilePoint(3, 0));

		// Act
		var undone = session.UndoLastVertex();

		// Assert
		undone.Should().BeTrue();
		session.Vertices.Should().HaveCount(1);
		session.Vertices[^1].Should().Be(new TilePoint(0, 0));
	}

	[Fact]
	public void UndoLastVertex_WhenEmpty_ReturnsFalse()
	{
		// Arrange
		var session = new MapDrawingSession();

		// Act
		var undone = session.UndoLastVertex();

		// Assert
		undone.Should().BeFalse();
	}

	[Fact]
	public void Reset_ClearsAllVertices()
	{
		// Arrange
		var session = new MapDrawingSession();
		session.PlaceVertex(new TilePoint(0, 0));
		session.PlaceVertex(new TilePoint(3, 0));

		// Act
		session.Reset();

		// Assert
		session.Vertices.Should().BeEmpty();
		session.IsActive.Should().BeFalse();
	}

	[Fact]
	public void BuildOutline_WithThreeVertices_ReturnsThreeSegments()
	{
		// Arrange
		var session = new MapDrawingSession();
		session.PlaceVertex(new TilePoint(0, 0));
		session.PlaceVertex(new TilePoint(4, 0));
		session.PlaceVertex(new TilePoint(4, 3));

		// Act
		var outline = session.BuildOutline();

		// Assert
		outline.Should().HaveCount(3);
		outline[0].Should().Be(new PathSegment(PathSegmentKind.Line, new TilePoint(4, 0), null, null));
		outline[1].Should().Be(new PathSegment(PathSegmentKind.Line, new TilePoint(4, 3), null, null));
		outline[2].Should().Be(new PathSegment(PathSegmentKind.Line, new TilePoint(0, 0), null, null));
	}

	[Fact]
	public void BuildOutline_WithFewerThanThreeVertices_ReturnsEmpty()
	{
		// Arrange
		var session = new MapDrawingSession();
		session.PlaceVertex(new TilePoint(0, 0));
		session.PlaceVertex(new TilePoint(4, 0));

		// Act
		var outline = session.BuildOutline();

		// Assert
		outline.Should().BeEmpty();
	}
}
