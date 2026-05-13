using DungeonMaster.Application.Common.Results;
using DungeonMaster.Application.Features.Dungeons;
using DungeonMaster.Application.Features.Dungeons.Commands.UpdateAreaOutline;
using DungeonMaster.Domain.Entities.Dungeon;
using DungeonMaster.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Commands;

public class UpdateAreaOutlineCommandHandlerTests
{
    private readonly IDungeonWriteRepository _writeRepository;
    private readonly UpdateAreaOutlineCommandHandler _handler;

    public UpdateAreaOutlineCommandHandlerTests()
    {
        _writeRepository = Substitute.For<IDungeonWriteRepository>();
        _handler = new UpdateAreaOutlineCommandHandler(_writeRepository);
    }

    [Fact]
    public async Task HandleAsync_WithExistingArea_UpdatesOutlineAndReturnsSuccess()
    {
        // Arrange
        var dungeon = new Dungeon("Test Dungeon");
        var floor = dungeon.AddFloor("Ground");
        dungeon.AddArea(floor.Id, "Room", null);
        _writeRepository
            .LoadAsync(dungeon.Id, Arg.Any<CancellationToken>())
            .Returns(dungeon);
        _writeRepository
            .UpdateAsync(Arg.Any<Dungeon>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var area = floor.Areas[0];
        var origin = new TilePoint(2, 3);
        var outline = new List<PathSegment>
        {
            new(PathSegmentKind.Line, new TilePoint(4, 3), null, null),
            new(PathSegmentKind.Line, new TilePoint(4, 6), null, null),
            new(PathSegmentKind.Line, new TilePoint(2, 3), null, null),
        };

        var command = new UpdateAreaOutlineCommand(dungeon.Id, floor.Id, area.Id, origin, outline);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        area.Origin.Should().Be(origin);
        area.Outline.Should().HaveCount(3);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentDungeon_ReturnsNotFoundFailure()
    {
        // Arrange
        _writeRepository
            .LoadAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Dungeon?)null);

        var command = new UpdateAreaOutlineCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new TilePoint(0, 0),
            []);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCategory.Should().Be(ErrorCategory.NotFound);
    }
}
