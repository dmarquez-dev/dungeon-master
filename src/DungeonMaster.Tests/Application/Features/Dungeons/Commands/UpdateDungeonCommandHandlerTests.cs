using DungeonMaster.Application.Common.Results;
using DungeonMaster.Application.Features.Dungeons;
using DungeonMaster.Application.Features.Dungeons.Commands.UpdateDungeon;
using DungeonMaster.Domain.Entities.Dungeon;
using FluentAssertions;
using NSubstitute;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Commands;

public class UpdateDungeonCommandHandlerTests
{
    private readonly IDungeonWriteRepository _writeRepository;
    private readonly UpdateDungeonCommandHandler _handler;

    public UpdateDungeonCommandHandlerTests()
    {
        _writeRepository = Substitute.For<IDungeonWriteRepository>();
        _handler = new UpdateDungeonCommandHandler(_writeRepository);
    }

    [Fact]
    public async Task HandleAsync_WithExistingDungeon_UpdatesAndReturnsSuccess()
    {
        var dungeon = new Dungeon("Old Name", "Old Desc");
        _writeRepository.LoadAsync(dungeon.Id, Arg.Any<CancellationToken>()).Returns(dungeon);
        _writeRepository.UpdateAsync(Arg.Any<Dungeon>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var command = new UpdateDungeonCommand(dungeon.Id, "New Name", "New Desc", "My Notes");

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        dungeon.Name.Should().Be("New Name");
        dungeon.Notes.Should().Be("My Notes");
        await _writeRepository.Received(1).UpdateAsync(dungeon, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentId_ReturnsNotFoundFailure()
    {
        _writeRepository.LoadAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Dungeon?)null);

        var result = await _handler.HandleAsync(
            new UpdateDungeonCommand(Guid.NewGuid(), "Name", null, null), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCategory.Should().Be(ErrorCategory.NotFound);
    }
}
