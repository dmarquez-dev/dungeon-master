using DungeonMaster.App.Core.Presentation;
using DungeonMaster.Application.Features.Dungeons.Queries.GetDungeonDetail;
using DungeonMaster.Domain.Entities.Floors;
using DungeonMaster.Domain.ValueObjects;
using FluentAssertions;

namespace DungeonMaster.Tests.AppCore.Presentation;

public class DungeonDetailPresentationMapperTests
{
	private static AreaDto AreaDtoWith(
		string name = "Area",
		string? description = null,
		string? notes = null,
		IReadOnlyList<PathSegment>? outline = null) =>
		new(Guid.NewGuid(), name, description, notes, outline ?? []);

	private static PlacedObjectDto PlacedObjectDtoWith(float scale = 1f) =>
		new(Guid.NewGuid(), ObjectKind.DoorWood, new CanvasPoint(3f, 4f), ObjectRotation.CW90, scale);

	private static FloorDto FloorDtoWith(
		string? name = "Floor",
		IReadOnlyList<AreaDto>? areas = null,
		IReadOnlyList<PlacedObjectDto>? objects = null) =>
		new(Guid.NewGuid(), 2, name, null, null, areas ?? [], objects ?? []);

	// ---- ToItem(AreaDto) ---------------------------------------------------

	[Fact]
	public void ToItemArea_CopiesEveryFieldOntoTheAreaItem()
	{
		var outline = new PathSegment[]
		{
			new(PathSegmentKind.Line, new TilePoint(1, 1), null, null),
		};
		var dto = AreaDtoWith("Vault", "a description", "some notes", outline);

		var item = DungeonDetailPresentationMapper.ToItem(dto);

		item.Id.Should().Be(dto.Id);
		item.Name.Should().Be("Vault");
		item.Description.Should().Be("a description");
		item.Notes.Should().Be("some notes");
		item.Outline.Should().Equal(outline);
	}

	[Fact]
	public void ToItemArea_NullArea_ThrowsArgumentNullExceptionNamingTheAreaParameter()
	{
		var act = () => DungeonDetailPresentationMapper.ToItem((AreaDto)null!);

		act.Should().Throw<ArgumentNullException>()
			.And.ParamName.Should().Be("area");
	}

	[Fact]
	public void ToItemArea_WhitespaceNameAndEmptyOutline_MapThroughUnchangedWithoutThrowing()
	{
		var dto = AreaDtoWith("   ", outline: []);

		var item = DungeonDetailPresentationMapper.ToItem(dto);

		item.Name.Should().Be("   ");
		item.Outline.Should().BeEmpty();
	}

	// ---- ToItem(PlacedObjectDto) -------------------------------------------

	[Fact]
	public void ToItemPlacedObject_CopiesEveryFieldOntoThePlacedObjectItem()
	{
		var dto = PlacedObjectDtoWith(2.5f);

		var item = DungeonDetailPresentationMapper.ToItem(dto);

		item.Id.Should().Be(dto.Id);
		item.Kind.Should().Be(ObjectKind.DoorWood);
		item.Position.Should().Be(new CanvasPoint(3f, 4f));
		item.Rotation.Should().Be(ObjectRotation.CW90);
		item.Scale.Should().Be(2.5f);
	}

	[Fact]
	public void ToItemPlacedObject_NullPlacedObject_ThrowsArgumentNullExceptionNamingThePlacedObjectParameter()
	{
		var act = () => DungeonDetailPresentationMapper.ToItem((PlacedObjectDto)null!);

		act.Should().Throw<ArgumentNullException>()
			.And.ParamName.Should().Be("placedObject");
	}

	[Fact]
	public void ToItemPlacedObject_NegativeScale_MapsThroughUnchangedWithoutThrowing()
	{
		var item = DungeonDetailPresentationMapper.ToItem(PlacedObjectDtoWith(-3f));

		item.Scale.Should().Be(-3f);
	}

	// ---- ToItem(FloorDto) --------------------------------------------------

	[Fact]
	public void ToItemFloor_CopiesEveryScalarFieldOntoTheFloorItem()
	{
		var dto = new FloorDto(Guid.NewGuid(), 7, "Cellar", "a description", "some notes", [], []);

		var item = DungeonDetailPresentationMapper.ToItem(dto);

		item.Id.Should().Be(dto.Id);
		item.Number.Should().Be(7);
		item.Name.Should().Be("Cellar");
		item.Description.Should().Be("a description");
		item.Notes.Should().Be("some notes");
	}

	[Fact]
	public void ToItemFloor_NullFloor_ThrowsArgumentNullExceptionNamingTheFloorParameter()
	{
		var act = () => DungeonDetailPresentationMapper.ToItem((FloorDto)null!);

		act.Should().Throw<ArgumentNullException>()
			.And.ParamName.Should().Be("floor");
	}

	[Fact]
	public void ToItemFloor_MapsAreasIndexForIndexInTheOrderTheyWereSupplied()
	{
		var first = AreaDtoWith("first");
		var second = AreaDtoWith("second");
		var third = AreaDtoWith("third");

		var item = DungeonDetailPresentationMapper.ToItem(FloorDtoWith(areas: [first, second, third]));

		item.Areas.Select(area => area.Name).Should().ContainInOrder("first", "second", "third");
		item.Areas.Select(area => area.Id).Should().ContainInOrder(first.Id, second.Id, third.Id);
	}

	[Fact]
	public void ToItemFloor_MapsObjectsIndexForIndexInTheOrderTheyWereSupplied()
	{
		var first = PlacedObjectDtoWith();
		var second = PlacedObjectDtoWith();
		var third = PlacedObjectDtoWith();

		var item = DungeonDetailPresentationMapper.ToItem(FloorDtoWith(objects: [first, second, third]));

		item.Objects.Select(placed => placed.Id).Should().ContainInOrder(first.Id, second.Id, third.Id);
	}

	[Fact]
	public void ToItemFloor_NullAreaElement_ThrowsArgumentExceptionNamingTheCollectionAndTheZeroBasedIndex()
	{
		var dto = FloorDtoWith(areas: [AreaDtoWith(), null!, AreaDtoWith()]);

		var act = () => DungeonDetailPresentationMapper.ToItem(dto);

		var thrown = act.Should().Throw<ArgumentException>().Which;
		thrown.Should().NotBeOfType<ArgumentNullException>();
		thrown.ParamName.Should().Be("floor");
		thrown.Message.Should().Contain("floor.Areas").And.Contain("index 1");
	}

	[Fact]
	public void ToItemFloor_NullObjectElement_ThrowsArgumentExceptionNamingTheCollectionAndTheZeroBasedIndex()
	{
		var dto = FloorDtoWith(objects: [PlacedObjectDtoWith(), PlacedObjectDtoWith(), null!]);

		var act = () => DungeonDetailPresentationMapper.ToItem(dto);

		var thrown = act.Should().Throw<ArgumentException>().Which;
		thrown.Should().NotBeOfType<ArgumentNullException>();
		thrown.ParamName.Should().Be("floor");
		thrown.Message.Should().Contain("floor.Objects").And.Contain("index 2");
	}

	[Fact]
	public void ToItemFloor_NullAreaElement_ThrowsRatherThanReturningAFloorMissingThatArea()
	{
		var dto = FloorDtoWith(areas: [AreaDtoWith("kept"), null!]);

		var act = () => DungeonDetailPresentationMapper.ToItem(dto);

		act.Should().Throw<ArgumentException>();
	}

	// ---- ToItems(IReadOnlyList<FloorDto>) ----------------------------------

	[Fact]
	public void ToItems_NullFloors_ThrowsArgumentNullExceptionNamingTheFloorsParameter()
	{
		var act = () => DungeonDetailPresentationMapper.ToItems(null!);

		act.Should().Throw<ArgumentNullException>()
			.And.ParamName.Should().Be("floors");
	}

	[Fact]
	public void ToItems_NullFloorElement_ThrowsArgumentExceptionNamingTheCollectionAndTheZeroBasedIndex()
	{
		var act = () => DungeonDetailPresentationMapper.ToItems([FloorDtoWith(), null!]);

		var thrown = act.Should().Throw<ArgumentException>().Which;
		thrown.Should().NotBeOfType<ArgumentNullException>();
		thrown.ParamName.Should().Be("floors");
		thrown.Message.Should().Contain("floors").And.Contain("index 1");
	}

	[Fact]
	public void ToItems_EmptyList_ReturnsAnEmptyList()
	{
		DungeonDetailPresentationMapper.ToItems([]).Should().BeEmpty();
	}

	[Fact]
	public void ToItems_MapsFloorsIndexForIndexInTheOrderTheyWereSupplied()
	{
		var first = FloorDtoWith("first");
		var second = FloorDtoWith("second");
		var third = FloorDtoWith("third");

		var items = DungeonDetailPresentationMapper.ToItems([first, second, third]);

		items.Select(floor => floor.Name).Should().ContainInOrder("first", "second", "third");
		items.Select(floor => floor.Id).Should().ContainInOrder(first.Id, second.Id, third.Id);
	}

	[Fact]
	public void ToItems_PreservesNestedAreaAndObjectOrderWithinEachFloor()
	{
		var areaA = AreaDtoWith("a");
		var areaB = AreaDtoWith("b");
		var objectA = PlacedObjectDtoWith();
		var objectB = PlacedObjectDtoWith();
		var floor = FloorDtoWith(areas: [areaA, areaB], objects: [objectA, objectB]);

		var items = DungeonDetailPresentationMapper.ToItems([floor]);

		items[0].Areas.Select(area => area.Id).Should().ContainInOrder(areaA.Id, areaB.Id);
		items[0].Objects.Select(placed => placed.Id).Should().ContainInOrder(objectA.Id, objectB.Id);
	}

	[Fact]
	public void ToItems_CalledTwiceWithTheSameInput_ProducesEqualResults()
	{
		var floors = new[]
		{
			FloorDtoWith(areas: [AreaDtoWith("a", "d", "n")], objects: [PlacedObjectDtoWith()]),
			FloorDtoWith("second"),
		};

		var first = DungeonDetailPresentationMapper.ToItems(floors);
		var second = DungeonDetailPresentationMapper.ToItems(floors);

		second.Should().BeEquivalentTo(first);
	}

	[Fact]
	public void ToItems_DoesNotCacheAcrossCallsSoADifferentInputProducesADifferentResult()
	{
		var firstResult = DungeonDetailPresentationMapper.ToItems([FloorDtoWith("first")]);
		var secondResult = DungeonDetailPresentationMapper.ToItems([FloorDtoWith("second")]);

		firstResult[0].Name.Should().Be("first");
		secondResult[0].Name.Should().Be("second");
	}

	[Fact]
	public void ToItems_ComputesHasNotebookContentFromTheMappedDescriptionAndNotes()
	{
		var floors = new[]
		{
			FloorDtoWith(areas: [AreaDtoWith("named", "a description", null)]),
		};

		var items = DungeonDetailPresentationMapper.ToItems(floors);

		items[0].HasNotebookContent.Should().BeFalse();
		items[0].Areas[0].HasNotebookContent.Should().BeTrue();
	}
}
