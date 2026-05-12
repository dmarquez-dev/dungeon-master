using DungeonMaster.Application.Features.Dungeons;
using DungeonMaster.Application.Features.Dungeons.Queries.GetDungeons;
using FluentAssertions;
using NSubstitute;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Queries;

/// <summary>
///		Unit tests for <see cref="GetDungeonsQueryHandler"/>.
/// </summary>
public class GetDungeonsQueryHandlerTests
{
    private readonly IDungeonReadRepository _readRepository;
    private readonly GetDungeonsQueryHandler _handler;

    /// <summary>
    ///		Initializes the test class with a substituted repository and a real handler instance.
    /// </summary>
    public GetDungeonsQueryHandlerTests()
    {
        _readRepository = Substitute.For<IDungeonReadRepository>();
        _handler = new GetDungeonsQueryHandler(_readRepository);
    }

    /// <summary>
    ///		Verifies that when the repository returns summaries, the handler passes them through.
    /// </summary>
    [Fact]
    public async Task HandleAsync_WithDungeons_ReturnsSummaries()
    {
        // Arrange
        var summaries = new List<DungeonSummaryDto>
        {
            new(Guid.NewGuid(), "Caverns of Doom", "A dark and dangerous place.", 0, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow),
            new(Guid.NewGuid(), "Crystal Labyrinth", null, 2, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow),
        };

        _readRepository
            .GetSummariesAsync(Arg.Any<CancellationToken>())
            .Returns(summaries);

        // Act
        var result = await _handler.HandleAsync(
            new GetDungeonsQuery(),
            CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Caverns of Doom");
        result[1].Name.Should().Be("Crystal Labyrinth");
        result[1].Description.Should().BeNull();
    }

    /// <summary>
    ///		Verifies that when the repository returns no dungeons, the handler returns an empty list.
    /// </summary>
    [Fact]
    public async Task HandleAsync_WithNoDungeons_ReturnsEmptyList()
    {
        // Arrange
        _readRepository
            .GetSummariesAsync(Arg.Any<CancellationToken>())
            .Returns(new List<DungeonSummaryDto>());

        // Act
        var result = await _handler.HandleAsync(
            new GetDungeonsQuery(),
            CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}
