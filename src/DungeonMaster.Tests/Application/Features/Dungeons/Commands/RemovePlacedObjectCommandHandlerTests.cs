using DungeonMaster.Application.Common.Results;
using DungeonMaster.Application.Features.Dungeons;
using DungeonMaster.Application.Features.Dungeons.Commands.RemovePlacedObject;
using DungeonMaster.Domain.Entities.Area;
using DungeonMaster.Domain.Entities.Dungeon;
using DungeonMaster.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Commands;

/// <summary>
///		Unit tests for <see cref="RemovePlacedObjectCommandHandler"/>.
/// </summary>
public class RemovePlacedObjectCommandHandlerTests
{
    private readonly IDungeonWriteRepository _writeRepository;
    private readonly RemovePlacedObjectCommandHandler _handler;

    /// <summary>
    ///		Initializes the test class with a substituted repository and a real handler instance.
    /// </summary>
    public RemovePlacedObjectCommandHandlerTests()
    {
        _writeRepository = Substitute.For<IDungeonWriteRepository>();
        _handler = new RemovePlacedObjectCommandHandler(_writeRepository);
    }

    /// <summary>
    ///		Verifies that a valid command removes the object from the area and returns success.
    /// </summary>
    [Fact]
    public async Task HandleAsync_ValidCommand_RemovesObjectAndReturnsSuccess()
    {
        // Arrange
        var dungeon = new Dungeon("D");
        var floor = dungeon.AddFloor("F");
        var area = dungeon.AddArea(
            floor.Id,
            "A",
            null);

        dungeon.AddPlacedObject(
            floor.Id,
            area.Id,
            ObjectKind.Barrel,
            new CanvasPoint(80f, 80f),
            ObjectRotation.None);

        var objId = area.Objects[0].Id;
        _writeRepository
            .LoadAsync(dungeon.Id, Arg.Any<CancellationToken>())
            .Returns(dungeon);

        var command = new RemovePlacedObjectCommand(
            dungeon.Id,
            floor.Id,
            area.Id,
            objId);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        area.Objects.Should().BeEmpty();

        await _writeRepository
            .Received(1)
            .UpdateAsync(dungeon, Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///		Verifies that a missing dungeon returns a not-found failure.
    /// </summary>
    [Fact]
    public async Task HandleAsync_DungeonNotFound_ReturnsNotFound()
    {
        // Arrange
        _writeRepository
            .LoadAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Dungeon?)null);

        var command = new RemovePlacedObjectCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid());

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCategory.Should().Be(ErrorCategory.NotFound);
    }

    /// <summary>
    ///		Verifies that an unknown floor ID is a silent no-op at the domain level;
    ///		the handler still persists the aggregate and returns success.
    /// </summary>
    [Fact]
    public async Task HandleAsync_FloorNotFound_ReturnsSuccessWithNoChange()
    {
        // Arrange
        var dungeon = new Dungeon("D", null);
        _writeRepository
            .LoadAsync(dungeon.Id, Arg.Any<CancellationToken>())
            .Returns(dungeon);

        var command = new RemovePlacedObjectCommand(
            dungeon.Id,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid());

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await _writeRepository
            .Received(1)
            .UpdateAsync(dungeon, Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///		Verifies that an unknown area ID is a silent no-op at the domain level;
    ///		the handler still persists the aggregate and returns success.
    /// </summary>
    [Fact]
    public async Task HandleAsync_AreaNotFound_ReturnsSuccessWithNoChange()
    {
        // Arrange
        var dungeon = new Dungeon("D");
        var floor = dungeon.AddFloor("F");
        _writeRepository
            .LoadAsync(dungeon.Id, Arg.Any<CancellationToken>())
            .Returns(dungeon);

        var command = new RemovePlacedObjectCommand(
            dungeon.Id,
            floor.Id,
            Guid.NewGuid(),
            Guid.NewGuid());

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await _writeRepository
            .Received(1)
            .UpdateAsync(dungeon, Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///		Verifies that an unknown object ID within a valid area is a silent no-op at the domain level;
    ///		the handler still persists the aggregate and returns success.
    /// </summary>
    [Fact]
    public async Task HandleAsync_ObjectNotFound_ReturnsSuccessWithNoChange()
    {
        // Arrange
        var dungeon = new Dungeon("D");
        var floor = dungeon.AddFloor("F");
        var area = dungeon.AddArea(
            floor.Id,
            "A",
            null);

        _writeRepository
            .LoadAsync(dungeon.Id, Arg.Any<CancellationToken>())
            .Returns(dungeon);

        var command = new RemovePlacedObjectCommand(
            dungeon.Id,
            floor.Id,
            area.Id,
            Guid.NewGuid());

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        area.Objects.Should().BeEmpty();

        await _writeRepository
            .Received(1)
            .UpdateAsync(dungeon, Arg.Any<CancellationToken>());
    }
}
