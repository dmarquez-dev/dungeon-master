using DungeonMaster.Application.Common.Results;
using DungeonMaster.Application.Features.Dungeons;
using DungeonMaster.Application.Features.Dungeons.Commands.RemoveArea;
using DungeonMaster.Domain.Entities.Dungeon;
using FluentAssertions;
using NSubstitute;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Commands;

public class RemoveAreaCommandHandlerTests
{
    private readonly IDungeonWriteRepository _writeRepository;
    private readonly RemoveAreaCommandHandler _handler;

    public RemoveAreaCommandHandlerTests()
    {
        _writeRepository = Substitute.For<IDungeonWriteRepository>();
        _handler = new RemoveAreaCommandHandler(_writeRepository);
    }

    [Fact]
    public async Task HandleAsync_WithExistingArea_RemovesAreaAndReturnsSuccess()
    {
        // Arrange
        var dungeon = new Dungeon("Test Dungeon");
        var floor = dungeon.AddFloor("Ground");
        var area = dungeon.AddArea(floor.Id, "Throne Room", null);
        _writeRepository
            .LoadAsync(dungeon.Id, Arg.Any<CancellationToken>())
            .Returns(dungeon);
        _writeRepository
            .UpdateAsync(Arg.Any<Dungeon>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var command = new RemoveAreaCommand(dungeon.Id, floor.Id, area.Id);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        floor.Areas.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentDungeon_ReturnsNotFoundFailure()
    {
        // Arrange
        _writeRepository
            .LoadAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Dungeon?)null);

        var command = new RemoveAreaCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCategory.Should().Be(ErrorCategory.NotFound);
    }
}
