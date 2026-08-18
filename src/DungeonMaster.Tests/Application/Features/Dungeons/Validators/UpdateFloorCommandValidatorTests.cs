using DungeonMaster.Application.Features.Dungeons.Commands.UpdateFloor;
using FluentAssertions;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Validators;

/// <summary>
///		Tests for <see cref="UpdateFloorCommandValidator"/>.
/// </summary>
public class UpdateFloorCommandValidatorTests
{
	private readonly UpdateFloorCommandValidator _validator = new();

	[Fact]
	public void Validate_WithValidCommand_Passes()
	{
		// Arrange
		var command = new UpdateFloorCommand(
			Guid.NewGuid(),
			Guid.NewGuid(),
			"Cellar",
			"A damp cellar.",
			"Trapdoor in the north wall.");

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
	}

	[Fact]
	public void Validate_WithNoName_Passes()
	{
		// Arrange
		var command = new UpdateFloorCommand(
			Guid.NewGuid(),
			Guid.NewGuid(),
			null,
			null,
			null);

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
	}

	[Fact]
	public void Validate_WithEmptyDungeonId_ReturnsViolation()
	{
		// Arrange
		var command = new UpdateFloorCommand(
			Guid.Empty,
			Guid.NewGuid(),
			"Cellar",
			null,
			null);

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().ContainSingle(error =>
			error.PropertyName == nameof(UpdateFloorCommand.DungeonId));
	}

	[Fact]
	public void Validate_WithEmptyFloorId_ReturnsViolation()
	{
		// Arrange
		var command = new UpdateFloorCommand(
			Guid.NewGuid(),
			Guid.Empty,
			"Cellar",
			null,
			null);

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().ContainSingle(error =>
			error.PropertyName == nameof(UpdateFloorCommand.FloorId));
	}

	[Fact]
	public void Validate_WithNameExceedingMaxLength_ReturnsViolation()
	{
		// Arrange
		var command = new UpdateFloorCommand(
			Guid.NewGuid(),
			Guid.NewGuid(),
			new string('A', 101),
			null,
			null);

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().ContainSingle(error =>
			error.PropertyName == nameof(UpdateFloorCommand.Name));
	}
}
