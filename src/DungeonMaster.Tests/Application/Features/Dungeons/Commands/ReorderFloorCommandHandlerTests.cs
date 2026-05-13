using DungeonMaster.Application.Common.Results;
using DungeonMaster.Application.Features.Dungeons;
using DungeonMaster.Application.Features.Dungeons.Commands.ReorderFloor;
using DungeonMaster.Domain.Entities.Dungeon;
using FluentAssertions;
using NSubstitute;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Commands;

public class ReorderFloorCommandHandlerTests
{
    private readonly IDungeonWriteRepository _writeRepository;
    private readonly ReorderFloorCommandHandler _handler;

    public ReorderFloorCommandHandlerTests()
    {
        _writeRepository = Substitute.For<IDungeonWriteRepository>();
        _handler = new ReorderFloorCommandHandler(_writeRepository);
    }

    [Fact]
    public async Task HandleAsync_WithValidReorder_UpdatesFloorNumberAndReturnsSuccess()
    {
        // Arrange
        var dungeon = new Dungeon("Test Dungeon");
        dungeon.AddFloor("Ground");
        var upper = dungeon.AddFloor("Upper");
        _writeRepository
            .LoadAsync(dungeon.Id, Arg.Any<CancellationToken>())
            .Returns(dungeon);
        _writeRepository
            .UpdateAsync(Arg.Any<Dungeon>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var command = new ReorderFloorCommand(dungeon.Id, upper.Id, 1);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        upper.Number.Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentDungeon_ReturnsNotFoundFailure()
    {
        // Arrange
        _writeRepository
            .LoadAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Dungeon?)null);

        var command = new ReorderFloorCommand(Guid.NewGuid(), Guid.NewGuid(), 1);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCategory.Should().Be(ErrorCategory.NotFound);
    }
}
