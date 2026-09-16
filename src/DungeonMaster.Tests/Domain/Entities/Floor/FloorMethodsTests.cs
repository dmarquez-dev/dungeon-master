using DungeonMaster.Domain.Entities;
using FluentAssertions;
using DomainDungeon = DungeonMaster.Domain.Entities.Dungeons.Dungeon;

namespace DungeonMaster.Tests.Domain.Entities.Floor;

public class FloorMethodsTests
{
	[Fact]
	public async Task UpdateNumber_WhenCalled_UpdatesLastModified()
	{
		// Arrange
		var dungeon = new DomainDungeon("Test Dungeon");
		var first = dungeon.AddFloor("Entry");
		var second = dungeon.AddFloor("Cellar");
		var before = second.LastModified;

		await Task.Delay(20);

		// Act
		var outcome = dungeon.ReorderFloor(
			first.Id,
			2);

		// Assert
		outcome.Should().Be(MutationOutcome.Success);
		second.LastModified.Should().BeAfter(before);
	}
}
