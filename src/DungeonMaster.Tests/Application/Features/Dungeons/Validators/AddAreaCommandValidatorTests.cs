using DungeonMaster.Application.Features.Dungeons.Commands.AddArea;
using FluentAssertions;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Validators;

public class AddAreaCommandValidatorTests
{
    private readonly AddAreaCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_Passes()
    {
        // Arrange
        var command = new AddAreaCommand(Guid.NewGuid(), Guid.NewGuid(), "Entry Hall", null);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_WithEmptyName_Fails(string? name)
    {
        // Arrange
        var command = new AddAreaCommand(Guid.NewGuid(), Guid.NewGuid(), name!, null);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e =>
            e.PropertyName == nameof(AddAreaCommand.Name));
    }

    [Fact]
    public void Validate_WithNameExceedingMaxLength_Fails()
    {
        // Arrange
        var command = new AddAreaCommand(Guid.NewGuid(), Guid.NewGuid(), new string('A', 101), null);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e =>
            e.PropertyName == nameof(AddAreaCommand.Name));
    }
}
