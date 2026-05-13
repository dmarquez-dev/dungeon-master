using DungeonMaster.Application.Common.Results;
using DungeonMaster.Application.Features.Dungeons;
using DungeonMaster.Application.Features.Dungeons.Commands.UpdateFloor;
using DungeonMaster.Domain.Entities.Dungeon;
using FluentAssertions;
using NSubstitute;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Commands;

public class UpdateFloorCommandHandlerTests
{
    private readonly IDungeonWriteRepository _writeRepository;
    private readonly UpdateFloorCommandHandler _handler;

    public UpdateFloorCommandHandlerTests()
    {
        _writeRepository = Substitute.For<IDungeonWriteRepository>();
        _handler = new UpdateFloorCommandHandler(_writeRepository);
    }

    [Fact]
    public async Task HandleAsync_WithExistingFloor_UpdatesFloorAndReturnsSuccess()
    {
        // Arrange
        var dungeon = new Dungeon("Test Dungeon");
        var floor = dungeon.AddFloor("Old Name");
        _writeRepository
            .LoadAsync(dungeon.Id, Arg.Any<CancellationToken>())
            .Returns(dungeon);
        _writeRepository
            .UpdateAsync(Arg.Any<Dungeon>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var command = new UpdateFloorCommand(dungeon.Id, floor.Id, "New Name", "Some notes");

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        floor.Name.Should().Be("New Name");
        floor.Notes.Should().Be("Some notes");
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentDungeon_ReturnsNotFoundFailure()
    {
        // Arrange
        _writeRepository
            .LoadAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Dungeon?)null);

        var command = new UpdateFloorCommand(Guid.NewGuid(), Guid.NewGuid(), "Name", null);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCategory.Should().Be(ErrorCategory.NotFound);
    }
}
