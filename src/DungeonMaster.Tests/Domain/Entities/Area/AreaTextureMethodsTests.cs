using DungeonMaster.Domain.Entities.Area;
using DungeonMaster.Domain.ValueObjects;
using FluentAssertions;
using DomainArea = DungeonMaster.Domain.Entities.Area.Area;
using DomainDungeon = DungeonMaster.Domain.Entities.Dungeon.Dungeon;

namespace DungeonMaster.Tests.Domain.Entities.Area;

public class AreaTextureMethodsTests
{
	private static DomainArea MakeArea()
	{
		var dungeon = new DomainDungeon("D");
		var floor = dungeon.AddFloor("F");
		return dungeon.AddArea(floor.Id, "A", null);
	}

	[Fact]
	public void SetTileTexture_NewPosition_AddsTileTexture()
	{
		var area = MakeArea();
		var pos = new TilePoint(2, 3);

		area.SetTileTexture(pos, TextureKind.StoneFloor);

		area.TileTextures.Should().ContainSingle()
			.Which.Position.Should().Be(pos);
	}

	[Fact]
	public void SetTileTexture_ExistingPosition_UpdatesKindWithoutDuplicating()
	{
		var area = MakeArea();
		var pos = new TilePoint(1, 1);
		area.SetTileTexture(pos, TextureKind.StoneFloor);

		area.SetTileTexture(pos, TextureKind.WoodPlanks);

		area.TileTextures.Should().ContainSingle()
			.Which.Kind.Should().Be(TextureKind.WoodPlanks);
	}

	[Fact]
	public void ClearTileTexture_ExistingPosition_RemovesTexture()
	{
		var area = MakeArea();
		var pos = new TilePoint(0, 0);
		area.SetTileTexture(pos, TextureKind.DirtFloor);

		area.ClearTileTexture(pos);

		area.TileTextures.Should().BeEmpty();
	}

	[Fact]
	public void ClearTileTexture_MissingPosition_DoesNotThrow()
	{
		var area = MakeArea();

		var act = () => area.ClearTileTexture(new TilePoint(5, 5));

		act.Should().NotThrow();
	}

	[Fact]
	public void AddStructureElement_ReturnsElementWithCorrectProperties()
	{
		var area = MakeArea();
		var from = new VertexPoint(1, 1);
		var to = new VertexPoint(1, 1);

		var element = area.AddStructureElement(from, to, StructureKind.RoundColumn);

		element.From.Should().Be(from);
		element.To.Should().Be(to);
		element.Kind.Should().Be(StructureKind.RoundColumn);
		area.StructureElements.Should().ContainSingle().Which.Id.Should().Be(element.Id);
	}

	[Fact]
	public void RemoveStructureElement_ExistingId_ReturnsTrueAndRemoves()
	{
		var area = MakeArea();
		var element = area.AddStructureElement(
			new VertexPoint(0, 0), new VertexPoint(1, 0), StructureKind.WallStub);

		var result = area.RemoveStructureElement(element.Id);

		result.Should().BeTrue();
		area.StructureElements.Should().BeEmpty();
	}

	[Fact]
	public void RemoveStructureElement_MissingId_ReturnsFalse()
	{
		var area = MakeArea();

		var result = area.RemoveStructureElement(Guid.NewGuid());

		result.Should().BeFalse();
	}

	[Fact]
	public void AddEdgeCenterStructureElement_ReturnsElementWithEdgeAnchor()
	{
		var area = MakeArea();
		var anchor = new EdgePoint(1, 2, EdgeSide.N);

		var element = area.AddEdgeCenterStructureElement(anchor, StructureKind.WoodDoor);

		element.EdgeAnchor.Should().Be(anchor);
		element.From.Should().BeNull();
		element.To.Should().BeNull();
		element.Kind.Should().Be(StructureKind.WoodDoor);
		area.StructureElements.Should().ContainSingle().Which.Id.Should().Be(element.Id);
	}

	[Fact]
	public void AddStructureElement_VertexPair_FromAndToAreSet()
	{
		var area = MakeArea();
		var from = new VertexPoint(0, 0);
		var to = new VertexPoint(1, 0);

		var element = area.AddStructureElement(from, to, StructureKind.WallStub);

		element.From.Should().Be(from);
		element.To.Should().Be(to);
		element.EdgeAnchor.Should().BeNull();
	}
}
