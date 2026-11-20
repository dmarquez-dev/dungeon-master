using DungeonMaster.Domain.Entities;
using DungeonMaster.Domain.Entities.Dungeons;
using FluentAssertions;

namespace DungeonMaster.Tests.Domain.Entities.Floors;

public class FloorMethodsTests
{
	[Fact]
	public void UpdateNumber_WhenCalled_UpdatesLastModified()
	{
		// Arrange
		var dungeon = new Dungeon("Test Dungeon");
		var first = dungeon.AddFloor("Entry");
		var second = dungeon.AddFloor("Cellar");

		BackdateLastModified(second, new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc));

		var before = second.LastModified;

		// Act
		var outcome = dungeon.ReorderFloor(
			first.Id,
			2);

		// Assert
		outcome.Should().Be(MutationOutcome.Success);
		second.LastModified.Should().BeAfter(before);
	}

	private static void BackdateLastModified(Entity entity, DateTime lastModified) =>
		typeof(Entity)
			.GetProperty(nameof(Entity.LastModified))!
			.SetValue(entity, lastModified);
}
