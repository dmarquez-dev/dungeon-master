using DungeonMaster.Application.Common.Results;
using DungeonMaster.Application.Features.Dungeons;
using DungeonMaster.Application.Features.Dungeons.Commands.AddPlacedObject;
using DungeonMaster.Domain.Entities.Area;
using DungeonMaster.Domain.Entities.Dungeon;
using DungeonMaster.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Commands;

/// <summary>
///		Unit tests for <see cref="AddPlacedObjectCommandHandler"/>.
/// </summary>
public class AddPlacedObjectCommandHandlerTests
{
    private readonly IDungeonWriteRepository _writeRepository;
    private readonly AddPlacedObjectCommandHandler _handler;

    /// <summary>
    ///		Initializes the test class with a substituted repository and a real handler instance.
    /// </summary>
    public AddPlacedObjectCommandHandlerTests()
    {
        _writeRepository = Substitute.For<IDungeonWriteRepository>();
        _handler = new AddPlacedObjectCommandHandler(_writeRepository);
    }

    private static Dungeon MakeDungeon(out Guid floorId, out Guid areaId)
    {
        var dungeon = new Dungeon("D");
        var floor = dungeon.AddFloor("F");
        var area = dungeon.AddArea(
            floor.Id,
            "A",
            null);

        floorId = floor.Id;
        areaId = area.Id;
        return dungeon;
    }

    /// <summary>
    ///		Verifies that a valid command produces a successful result containing a non-empty Guid,
    ///		and that <see cref="IDungeonWriteRepository.UpdateAsync"/> is called exactly once.
    /// </summary>
    [Fact]
    public async Task HandleAsync_ValidCommand_ReturnsSuccessWithId()
    {
        // Arrange
        var dungeon = MakeDungeon(out var floorId, out var areaId);
        _writeRepository
            .LoadAsync(dungeon.Id, Arg.Any<CancellationToken>())
            .Returns(dungeon);

        var command = new AddPlacedObjectCommand(
            dungeon.Id,
            floorId,
            areaId,
            ObjectKind.Chair,
            new CanvasPoint(40f, 40f),
            ObjectRotation.None);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        await _writeRepository
            .Received(1)
            .UpdateAsync(dungeon, Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///		Verifies that the object is actually added to the area after the command succeeds.
    /// </summary>
    [Fact]
    public async Task HandleAsync_ValidCommand_AddsObjectToArea()
    {
        // Arrange
        var dungeon = MakeDungeon(out var floorId, out var areaId);
        var area = dungeon.Floors.First().Areas.First();
        _writeRepository
            .LoadAsync(dungeon.Id, Arg.Any<CancellationToken>())
            .Returns(dungeon);

        var command = new AddPlacedObjectCommand(
            dungeon.Id,
            floorId,
            areaId,
            ObjectKind.Chair,
            new CanvasPoint(120f, 160f),
            ObjectRotation.CW90);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        area.Objects.Should().HaveCount(1);
        area.Objects[0].Id.Should().Be(result.Value);
        area.Objects[0].Kind.Should().Be(ObjectKind.Chair);
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

        var command = new AddPlacedObjectCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            ObjectKind.Chair,
            new CanvasPoint(40f, 40f),
            ObjectRotation.None);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCategory.Should().Be(ErrorCategory.NotFound);
    }

    /// <summary>
    ///		Verifies that a missing floor returns a validation failure (domain returns null when floor is absent).
    /// </summary>
    [Fact]
    public async Task HandleAsync_FloorNotFound_ReturnsValidationFailure()
    {
        // Arrange
        var dungeon = MakeDungeon(out _, out var areaId);
        _writeRepository
            .LoadAsync(dungeon.Id, Arg.Any<CancellationToken>())
            .Returns(dungeon);

        var command = new AddPlacedObjectCommand(
            dungeon.Id,
            Guid.NewGuid(),
            areaId,
            ObjectKind.Chair,
            new CanvasPoint(40f, 40f),
            ObjectRotation.None);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCategory.Should().Be(ErrorCategory.Validation);
    }

    /// <summary>
    ///		Verifies that a missing area returns a validation failure (domain returns null when area is absent).
    /// </summary>
    [Fact]
    public async Task HandleAsync_AreaNotFound_ReturnsValidationFailure()
    {
        // Arrange
        var dungeon = MakeDungeon(out var floorId, out _);
        _writeRepository
            .LoadAsync(dungeon.Id, Arg.Any<CancellationToken>())
            .Returns(dungeon);

        var command = new AddPlacedObjectCommand(
            dungeon.Id,
            floorId,
            Guid.NewGuid(),
            ObjectKind.Chair,
            new CanvasPoint(40f, 40f),
            ObjectRotation.None);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCategory.Should().Be(ErrorCategory.Validation);
    }
}
