using DungeonMaster.Application.Features.Dungeons.Commands.AddFloor;
using FluentAssertions;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Validators;

/// <summary>
///		Tests for <see cref="AddFloorCommandValidator"/>.
/// </summary>
public class AddFloorCommandValidatorTests
{
	private readonly AddFloorCommandValidator _validator = new();

	[Fact]
	public void Validate_WithValidCommand_Passes()
	{
		// Arrange
		var command = new AddFloorCommand(
			Guid.NewGuid(),
			"Cellar");

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
	}

	[Fact]
	public void Validate_WithNoName_Passes()
	{
		// Arrange
		var command = new AddFloorCommand(
			Guid.NewGuid(),
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
		var command = new AddFloorCommand(
			Guid.Empty,
			"Cellar");

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().ContainSingle(error =>
			error.PropertyName == nameof(AddFloorCommand.DungeonId));
	}

	[Fact]
	public void Validate_WithNameExceedingMaxLength_ReturnsViolation()
	{
		// Arrange
		var command = new AddFloorCommand(
			Guid.NewGuid(),
			new string('A', 101));

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().ContainSingle(error =>
			error.PropertyName == nameof(AddFloorCommand.Name));
	}
}
