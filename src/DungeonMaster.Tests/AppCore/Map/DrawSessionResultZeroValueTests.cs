using DungeonMaster.App.Core.Map;
using DungeonMaster.Domain.ValueObjects;
using FluentAssertions;

namespace DungeonMaster.Tests.AppCore.Map;

public class DrawSessionResultZeroValueTests
{
	[Fact]
	public void Default_IsNone()
	{
		default(DrawSessionResult).Should().Be(DrawSessionResult.None);
	}

	[Fact]
	public void PreExistingMembers_KeepTheirRelativeDeclarationOrder()
	{
		// Act
		var added = (int)DrawSessionResult.Added;
		var closed = (int)DrawSessionResult.Closed;
		var ignored = (int)DrawSessionResult.Ignored;

		// Assert
		added.Should().Be(1);
		closed.Should().Be(2);
		ignored.Should().Be(3);
	}

	[Fact]
	public void PlaceVertex_NeverReturnsNone_OnAnyOfItsThreeOutcomes()
	{
		// Arrange
		var session = new MapDrawingSession();

		// Act
		var added = session.PlaceVertex(new TilePoint(0, 0));
		var ignored = session.PlaceVertex(new TilePoint(0, 0));
		session.PlaceVertex(new TilePoint(5, 0));
		session.PlaceVertex(new TilePoint(5, 5));
		var closed = session.PlaceVertex(new TilePoint(0, 0));

		// Assert
		added.Should().NotBe(DrawSessionResult.None);
		ignored.Should().NotBe(DrawSessionResult.None);
		closed.Should().NotBe(DrawSessionResult.None);
	}

	[Fact]
	public void DeclaresExactlyFourMembers_AndOnlyNoneCastsToZero()
	{
		// Act
		var values = Enum.GetValues<DrawSessionResult>();

		// Assert
		values.Should().HaveCount(4);
		values.Where(result => (int)result == 0).Should().ContainSingle()
			.Which.Should().Be(DrawSessionResult.None);
	}
}
