using DungeonMaster.Application.Features.Dungeons.Commands.RemoveFloor;
using FluentAssertions;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Validators;

/// <summary>
///		Tests for <see cref="RemoveFloorCommandValidator"/>.
/// </summary>
public class RemoveFloorCommandValidatorTests
{
	private readonly RemoveFloorCommandValidator _validator = new();

	[Fact]
	public void Validate_WithValidCommand_Passes()
	{
		// Arrange
		var command = new RemoveFloorCommand(
			Guid.NewGuid(),
			Guid.NewGuid());

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
	}

	[Fact]
	public void Validate_WithEmptyDungeonId_ReturnsViolation()
	{
		// Arrange
		var command = new RemoveFloorCommand(
			Guid.Empty,
			Guid.NewGuid());

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().ContainSingle(error =>
			error.PropertyName == nameof(RemoveFloorCommand.DungeonId));
	}

	[Fact]
	public void Validate_WithEmptyFloorId_ReturnsViolation()
	{
		// Arrange
		var command = new RemoveFloorCommand(
			Guid.NewGuid(),
			Guid.Empty);

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().ContainSingle(error =>
			error.PropertyName == nameof(RemoveFloorCommand.FloorId));
	}
}
