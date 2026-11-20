using DungeonMaster.Domain.ValueObjects;
using FluentAssertions;
using System.Text.Json;

namespace DungeonMaster.Tests.Infrastructure.Persistence.Configurations;

/// <summary>
///		Covers the persistence consequence of the <c>PathSegment</c> control-point invariant (MASTER-170).
///		<c>AreaConfiguration</c> stores the outline as JSON and reads it back with
///		<c>JsonSerializer.Deserialize&lt;List&lt;PathSegment&gt;&gt;</c>, and <c>System.Text.Json</c> builds a
///		positional record through its constructor, so the guard now runs on every database read. The first test is
///		the positive control that proves a normal outline still loads; the second pins the observed behaviour for a
///		corrupt row.
/// </summary>
public class PathSegmentJsonRoundTripTests
{
	/// <summary>
	///		The same options shape <c>AreaConfiguration</c> uses for the outline conversion.
	/// </summary>
	private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.General);

	[Fact]
	public void Deserialize_ValidOutlineOfALineAndACubicBezier_RoundTripsToEqualValues()
	{
		// Arrange
		const string raw = """
			[
				{ "Kind": 0, "End": { "Col": 2, "Row": 3 }, "Ctrl1": null, "Ctrl2": null },
				{ "Kind": 1, "End": { "Col": 4, "Row": 4 }, "Ctrl1": { "Col": 6, "Row": 1 }, "Ctrl2": { "Col": 6, "Row": 3 } }
			]
			""";

		// Act
		var outline = JsonSerializer.Deserialize<List<PathSegment>>(raw, JsonOptions);

		// Assert
		outline.Should().Equal(
			new PathSegment(PathSegmentKind.Line, new TilePoint(2, 3), null, null),
			new PathSegment(
				PathSegmentKind.CubicBezier,
				new TilePoint(4, 4),
				new TilePoint(6, 1),
				new TilePoint(6, 3)));
	}

	[Fact]
	public void Deserialize_CorruptRowWhoseCubicBezierHasANullCtrl2_Throws()
	{
		// Arrange
		const string raw = """
			[
				{ "Kind": 1, "End": { "Col": 4, "Row": 4 }, "Ctrl1": { "Col": 6, "Row": 1 }, "Ctrl2": null }
			]
			""";

		// Act
		var act = () => JsonSerializer.Deserialize<List<PathSegment>>(raw, JsonOptions);

		// Assert
		act.Should().ThrowExactly<ArgumentException>()
			.Which.ParamName.Should().Be("Ctrl2");
	}
}
