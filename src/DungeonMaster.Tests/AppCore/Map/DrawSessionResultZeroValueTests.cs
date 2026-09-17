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
		((int)DrawSessionResult.Added).Should().Be(1);
		((int)DrawSessionResult.Closed).Should().Be(2);
		((int)DrawSessionResult.Ignored).Should().Be(3);
	}

	[Fact]
	public void PlaceVertex_NeverReturnsNone_OnAnyOfItsThreeOutcomes()
	{
		var session = new MapDrawingSession();

		var added = session.PlaceVertex(new TilePoint(0, 0));
		var ignored = session.PlaceVertex(new TilePoint(0, 0));
		session.PlaceVertex(new TilePoint(5, 0));
		session.PlaceVertex(new TilePoint(5, 5));
		var closed = session.PlaceVertex(new TilePoint(0, 0));

		added.Should().NotBe(DrawSessionResult.None);
		ignored.Should().NotBe(DrawSessionResult.None);
		closed.Should().NotBe(DrawSessionResult.None);
	}
}
