using DungeonMaster.Application.Common.Results;
using DungeonMaster.Application.Features.Dungeons;
using DungeonMaster.Application.Features.Dungeons.Commands.RemoveFloor;
using DungeonMaster.Domain.Entities.Dungeon;
using FluentAssertions;
using NSubstitute;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Commands;

public class RemoveFloorCommandHandlerTests
{
    private readonly IDungeonWriteRepository _writeRepository;
    private readonly RemoveFloorCommandHandler _handler;

    public RemoveFloorCommandHandlerTests()
    {
        _writeRepository = Substitute.For<IDungeonWriteRepository>();
        _handler = new RemoveFloorCommandHandler(_writeRepository);
    }

    [Fact]
    public async Task HandleAsync_WithExistingFloor_RemovesFloorAndReturnsSuccess()
    {
        var dungeon = new Dungeon("Test Dungeon");
        var floor = dungeon.AddFloor("Entry");
        _writeRepository.LoadAsync(dungeon.Id, Arg.Any<CancellationToken>()).Returns(dungeon);
        _writeRepository.UpdateAsync(Arg.Any<Dungeon>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var result = await _handler.HandleAsync(new RemoveFloorCommand(dungeon.Id, floor.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        dungeon.Floors.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentDungeon_ReturnsNotFoundFailure()
    {
        _writeRepository.LoadAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Dungeon?)null);

        var result = await _handler.HandleAsync(
            new RemoveFloorCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCategory.Should().Be(ErrorCategory.NotFound);
    }
}
