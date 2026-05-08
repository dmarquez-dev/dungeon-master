using DungeonMaster.Application.Features.Dungeons;
using DungeonMaster.Application.Features.Dungeons.Queries.GetDungeons;
using DungeonMaster.Domain.Entities.Dungeon;
using FluentAssertions;
using NSubstitute;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Queries;

/// <summary>
///		Unit tests for <see cref="GetDungeonsQueryHandler"/>.
/// </summary>
public class GetDungeonsQueryHandlerTests
{
	private readonly IDungeonRepository _dungeonRepository;
	private readonly GetDungeonsQueryHandler _handler;

	/// <summary>
	///		Initializes the test class with a substituted repository and a real handler instance.
	/// </summary>
	public GetDungeonsQueryHandlerTests()
	{
		_dungeonRepository = Substitute.For<IDungeonRepository>();
		_handler = new GetDungeonsQueryHandler(_dungeonRepository);
	}

	/// <summary>
	///		Verifies that when the repository returns dungeons, the handler maps each one correctly to a <see cref="DungeonSummaryDto"/> preserving all projected fields.
	/// </summary>
	[Fact]
	public async Task HandleAsync_WithDungeons_ReturnsMappedSummaries()
	{
		// Arrange
		var firstDungeon = new Dungeon(
			"Caverns of Doom",
			"A dark and dangerous place.");

		var secondDungeon = new Dungeon(
			"Crystal Labyrinth",
			null);

		_dungeonRepository
			.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(new List<Dungeon> { firstDungeon, secondDungeon });

		// Act
		var result = await _handler.HandleAsync(
			new GetDungeonsQuery(),
			CancellationToken.None);

		// Assert
		result.Should().HaveCount(2);

		result[0].Name.Should().Be(firstDungeon.Name);
		result[0].Description.Should().Be(firstDungeon.Description);

		result[1].Name.Should().Be(secondDungeon.Name);
		result[1].Description.Should().BeNull();
	}

	/// <summary>
	///		Verifies that when the repository returns no dungeons, the handler returns an empty list.
	/// </summary>
	[Fact]
	public async Task HandleAsync_WithNoDungeons_ReturnsEmptyList()
	{
		// Arrange
		_dungeonRepository
			.GetAllAsync(Arg.Any<CancellationToken>())
			.Returns(new List<Dungeon>());

		// Act
		var result = await _handler.HandleAsync(
			new GetDungeonsQuery(),
			CancellationToken.None);

		// Assert
		result.Should().BeEmpty();
	}
}
