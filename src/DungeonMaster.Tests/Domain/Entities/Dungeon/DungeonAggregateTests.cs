using DomainDungeon = DungeonMaster.Domain.Entities.Dungeon.Dungeon;
using FluentAssertions;

namespace DungeonMaster.Tests.Domain.Entities.Dungeon;

public class DungeonAggregateTests
{
    [Fact]
    public void AddFloor_IncrementsFloorCount()
    {
        var dungeon = new DomainDungeon("Test Dungeon");

        dungeon.AddFloor("Entry Level");

        dungeon.Floors.Should().HaveCount(1);
        dungeon.Floors[0].Number.Should().Be(1);
        dungeon.Floors[0].Name.Should().Be("Entry Level");
    }

    [Fact]
    public void AddFloor_AssignsSequentialNumbers()
    {
        var dungeon = new DomainDungeon("Test Dungeon");

        dungeon.AddFloor(null);
        dungeon.AddFloor("Second");
        dungeon.AddFloor(null);

        dungeon.Floors.Select(f => f.Number).Should().Equal(1, 2, 3);
    }

    [Fact]
    public void RemoveFloor_RemovesAndRenumbers()
    {
        var dungeon = new DomainDungeon("Test Dungeon");
        dungeon.AddFloor("First");
        var second = dungeon.AddFloor("Second");
        dungeon.AddFloor("Third");

        dungeon.RemoveFloor(second.Id);

        dungeon.Floors.Should().HaveCount(2);
        dungeon.Floors.Select(f => f.Number).Should().Equal(1, 2);
    }

    [Fact]
    public void RemoveFloor_WithNonExistentId_IsNoOp()
    {
        var dungeon = new DomainDungeon("Test Dungeon");
        dungeon.AddFloor(null);

        var act = () => dungeon.RemoveFloor(Guid.NewGuid());

        act.Should().NotThrow();
        dungeon.Floors.Should().HaveCount(1);
    }

    [Fact]
    public void ReorderFloor_MovesFloorAndRenumbersOthers()
    {
        var dungeon = new DomainDungeon("Test Dungeon");
        dungeon.AddFloor("First");
        dungeon.AddFloor("Second");
        var third = dungeon.AddFloor("Third");

        dungeon.ReorderFloor(third.Id, 1);

        var ordered = dungeon.Floors.OrderBy(f => f.Number).ToList();
        ordered[0].Name.Should().Be("Third");
        ordered[1].Name.Should().Be("First");
        ordered[2].Name.Should().Be("Second");
    }

    [Fact]
    public void AddArea_AddsToCorrectFloor()
    {
        var dungeon = new DomainDungeon("Test Dungeon");
        var floor = dungeon.AddFloor("Entry");

        var area = dungeon.AddArea(floor.Id, "Grand Foyer", "A large entrance hall.");

        floor.Areas.Should().HaveCount(1);
        floor.Areas[0].Name.Should().Be("Grand Foyer");
        area.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void RemoveArea_RemovesFromCorrectFloor()
    {
        var dungeon = new DomainDungeon("Test Dungeon");
        var floor = dungeon.AddFloor("Entry");
        var area = dungeon.AddArea(floor.Id, "Grand Foyer", null);

        dungeon.RemoveArea(floor.Id, area.Id);

        floor.Areas.Should().BeEmpty();
    }

    [Fact]
    public void Update_SetsAllThreeFields()
    {
        var dungeon = new DomainDungeon("Old Name", "Old Desc");

        dungeon.Update("New Name", "New Desc", "DM Notes");

        dungeon.Name.Should().Be("New Name");
        dungeon.Description.Should().Be("New Desc");
        dungeon.Notes.Should().Be("DM Notes");
    }
}
