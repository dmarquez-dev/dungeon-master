using DungeonMaster.App.Core.Editor;
using DungeonMaster.Domain.Entities;
using DungeonMaster.Domain.Entities.Dungeons;
using FluentAssertions;

namespace DungeonMaster.Tests.AppCore;

/// <summary>
///		Covers every row of the drop-landing table, every gap position the insertion line can take, the agreement of the landing with the drop resolver, and the agreement of the gap with where the domain actually moves the floor.
/// </summary>
public class FloorDropIndicatorTests
{
	private const double CardHeight = 54;
	private const double CardPitch = 58;

	/// <summary>
	///		Builds floor-card bands laid out the way the running sidebar lays them out, 54 DIU tall at a 58 DIU pitch, so every pair of neighbours has a 4 DIU seam.
	/// </summary>
	private static List<FloorListBand> Bands(
		int count,
		double firstTop = 0)
	{
		var bands = new List<FloorListBand>();

		for (int index = 0; index < count; index++)
		{
			bands.Add(new FloorListBand(
				FloorListBandKind.FloorCard,
				firstTop + (index * CardPitch),
				CardHeight));
		}

		return bands;
	}

	private static double InsideBand(
		IReadOnlyList<FloorListBand> bands,
		int index) =>
			bands[index].Top + (CardHeight / 2);

	[Fact]
	public void Locate_WithNullBands_ReturnsNone()
	{
		// Act
		var landing = FloorDropIndicator.Locate(
			null,
			10,
			0);

		// Assert
		landing.Should().Be(FloorDropLanding.None);
		landing.HasLanding.Should().BeFalse();
	}

	[Fact]
	public void Locate_WithEmptyBands_ReturnsNone()
	{
		// Act
		var landing = FloorDropIndicator.Locate(
			[],
			10,
			0);

		// Assert
		landing.Should().Be(FloorDropLanding.None);
	}

	[Theory]
	[InlineData(double.NaN)]
	[InlineData(double.PositiveInfinity)]
	[InlineData(double.NegativeInfinity)]
	public void Locate_WithANonFiniteY_ReturnsNone(double y)
	{
		// Arrange
		var bands = Bands(4);

		// Act
		var landing = FloorDropIndicator.Locate(
			bands,
			y,
			0);

		// Assert
		landing.Should().Be(FloorDropLanding.None);
	}

	[Theory]
	[InlineData(-1)]
	[InlineData(4)]
	[InlineData(int.MaxValue)]
	public void Locate_WithADraggedIndexOutsideTheBands_ReturnsNone(int draggedIndex)
	{
		// Arrange
		var bands = Bands(4);

		// Act
		var landing = FloorDropIndicator.Locate(
			bands,
			InsideBand(bands, 2),
			draggedIndex);

		// Assert
		landing.Should().Be(FloorDropLanding.None);
	}

	/// <summary>
	///		Every band is unmeasured, so the resolver has nothing to resolve to and reports no owner.
	/// </summary>
	[Fact]
	public void Locate_WhenTheResolverFindsNoOwner_ReturnsNone()
	{
		// Arrange
		var bands = new List<FloorListBand>
		{
			new(FloorListBandKind.FloorCard, 0, 0),
			new(FloorListBandKind.FloorCard, 58, 0),
		};

		FloorReorderNearest.Resolve(bands, 10).Owner.Should().Be(FloorDropOwner.None);

		// Act
		var landing = FloorDropIndicator.Locate(
			bands,
			10,
			0);

		// Assert
		landing.Should().Be(FloorDropLanding.None);
	}

	/// <summary>
	///		Both owners are exercised: 85 lies inside band 1, and 113 lies in the seam below it and resolves to band 1 as a near-miss.
	/// </summary>
	[Theory]
	[InlineData(85)]
	[InlineData(113)]
	public void Locate_WhenThePointerResolvesToTheDraggedCard_ReturnsNone(double y)
	{
		// Arrange
		var bands = Bands(4);

		FloorReorderNearest.Resolve(bands, y).BandIndex.Should().Be(1);

		// Act
		var landing = FloorDropIndicator.Locate(
			bands,
			y,
			1);

		// Assert
		landing.Should().Be(FloorDropLanding.None);
	}

	/// <summary>
	///		The kind is built from an undeclared value so this row is pinned without naming any member other than the floor card.
	/// </summary>
	[Fact]
	public void Locate_WhenTheResolvedBandIsNotAFloorCard_ReturnsNone()
	{
		// Arrange
		var bands = Bands(4);

		bands[2] = new FloorListBand(
			(FloorListBandKind)int.MaxValue,
			bands[2].Top,
			bands[2].Height);

		FloorReorderNearest.Resolve(bands, InsideBand(bands, 2)).BandIndex.Should().Be(2);

		// Act
		var landing = FloorDropIndicator.Locate(
			bands,
			InsideBand(bands, 2),
			0);

		// Assert
		landing.Should().Be(FloorDropLanding.None);
	}

	[Fact]
	public void Locate_WhenMovingDown_TargetsTheResolvedCardAndLandsInTheGapBelowIt()
	{
		// Arrange
		var bands = Bands(4);

		// Act
		var landing = FloorDropIndicator.Locate(
			bands,
			InsideBand(bands, 2),
			0);

		// Assert
		landing.HasLanding.Should().BeTrue();
		landing.TargetIndex.Should().Be(2);
		landing.GapIndex.Should().Be(3);
	}

	[Fact]
	public void Locate_WhenMovingUp_TargetsTheResolvedCardAndLandsInTheGapAboveIt()
	{
		// Arrange
		var bands = Bands(4);

		// Act
		var landing = FloorDropIndicator.Locate(
			bands,
			InsideBand(bands, 1),
			3);

		// Assert
		landing.HasLanding.Should().BeTrue();
		landing.TargetIndex.Should().Be(1);
		landing.GapIndex.Should().Be(1);
	}

	/// <summary>
	///		The first band starts at 12 rather than 0, so a line left at the default of zero fails.
	/// </summary>
	[Fact]
	public void Locate_WhenLandingInGapZero_PutsTheLineAtTheFirstBandsTop()
	{
		// Arrange
		var bands = Bands(4, firstTop: 12);

		// Act
		var landing = FloorDropIndicator.Locate(
			bands,
			InsideBand(bands, 0),
			2);

		// Assert
		landing.GapIndex.Should().Be(0);
		landing.LineY.Should().Be(12);
	}

	/// <summary>
	///		The last band spans 186 to 240, so the line sits on its bottom edge at 240.
	/// </summary>
	[Fact]
	public void Locate_WhenLandingInTheGapBelowTheLastBand_PutsTheLineAtTheLastBandsBottom()
	{
		// Arrange
		var bands = Bands(4, firstTop: 12);

		// Act
		var landing = FloorDropIndicator.Locate(
			bands,
			InsideBand(bands, 3),
			0);

		// Assert
		landing.GapIndex.Should().Be(4);
		landing.LineY.Should().Be(240);
	}

	/// <summary>
	///		Band 1 ends at 124 and band 2 starts at 128, so the seam's middle is 126, which differs from both neighbours' edges.
	/// </summary>
	[Fact]
	public void Locate_WhenLandingInAnInteriorGap_PutsTheLineInTheMiddleOfTheSeam()
	{
		// Arrange
		var bands = Bands(4, firstTop: 12);

		// Act
		var landing = FloorDropIndicator.Locate(
			bands,
			InsideBand(bands, 1),
			0);

		// Assert
		landing.GapIndex.Should().Be(2);
		landing.LineY.Should().Be(126);
	}

	/// <summary>
	///		The one-owner rule, swept over every dragged card and every integer pointer position from above the first card to below the last.
	///		The landing must exist exactly when a drop there would resolve to a card other than the dragged one, and must then name that same card.
	/// </summary>
	[Fact]
	public void Locate_ForEveryDraggedCardAndPointerPosition_AgreesWithTheDropResolver()
	{
		// Arrange
		var bands = Bands(11);

		for (int draggedIndex = 0; draggedIndex <= 10; draggedIndex++)
		{
			for (int y = -10; y <= 648; y++)
			{
				var resolution = FloorReorderNearest.Resolve(
					bands,
					y);

				// Act
				var landing = FloorDropIndicator.Locate(
					bands,
					y,
					draggedIndex);

				// Assert
				var expectedLanding = resolution.Owner != FloorDropOwner.None
					&& resolution.BandIndex != draggedIndex;

				landing.HasLanding.Should().Be(
					expectedLanding,
					"dragged {0} at y {1} resolves to {2} band {3}",
					draggedIndex,
					y,
					resolution.Owner,
					resolution.BandIndex);

				if (landing.HasLanding)
				{
					landing.TargetIndex.Should().Be(
						resolution.BandIndex,
						"dragged {0} at y {1}",
						draggedIndex,
						y);
				}
			}
		}
	}

	/// <summary>
	///		The gap is counted in the list as it looks during the drag, with the dragged card still in its slot.
	///		Once that card is removed, every gap below it moves up by one, which is where the reorder must leave the floor.
	/// </summary>
	[Fact]
	public void Locate_ForEveryOrderedPairOfFiveFloors_MarksTheGapTheDomainReorderMovesTheFloorInto()
	{
		// Arrange
		const int floorCount = 5;
		var bands = Bands(floorCount);

		for (int draggedIndex = 0; draggedIndex < floorCount; draggedIndex++)
		{
			for (int targetIndex = 0; targetIndex < floorCount; targetIndex++)
			{
				if (draggedIndex == targetIndex)
				{
					continue;
				}

				var dungeon = new Dungeon("Cross-check");

				for (int number = 1; number <= floorCount; number++)
				{
					dungeon.AddFloor($"Floor {number}");
				}

				var dragged = dungeon.Floors
					.OrderBy(floor =>
						floor.Number)
					.ElementAt(draggedIndex);

				var landing = FloorDropIndicator.Locate(
					bands,
					InsideBand(bands, targetIndex),
					draggedIndex);

				// Act
				var outcome = dungeon.ReorderFloor(
					dragged.Id,
					targetIndex + 1);

				// Assert
				outcome.Should().Be(MutationOutcome.Success);

				var position = dungeon.Floors
					.OrderBy(floor =>
						floor.Number)
					.ToList()
					.IndexOf(dragged);

				var expectedPosition = landing.GapIndex <= draggedIndex
					? landing.GapIndex
					: landing.GapIndex - 1;

				position.Should().Be(
					expectedPosition,
					"dragged {0} onto {1} landed in gap {2}",
					draggedIndex,
					targetIndex,
					landing.GapIndex);
			}
		}
	}
}
