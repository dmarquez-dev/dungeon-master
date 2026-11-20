using DungeonMaster.Application.Features.Dungeons.Commands.ReorderFloor;
using FluentAssertions;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Validators;

/// <summary>
///		Tests for <see cref="ReorderFloorCommandValidator"/>.
/// </summary>
public class ReorderFloorCommandValidatorTests
{
	private readonly ReorderFloorCommandValidator _validator = new();

	[Fact]
	public void Validate_WithValidCommand_Passes()
	{
		// Arrange
		var command = new ReorderFloorCommand(
			Guid.NewGuid(),
			Guid.NewGuid(),
			1);

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
	}

	[Fact]
	public void Validate_WithEmptyDungeonId_ReturnsViolation()
	{
		// Arrange
		var command = new ReorderFloorCommand(
			Guid.Empty,
			Guid.NewGuid(),
			1);

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().ContainSingle(error =>
			error.PropertyName == nameof(ReorderFloorCommand.DungeonId));
	}

	[Fact]
	public void Validate_WithEmptyFloorId_ReturnsViolation()
	{
		// Arrange
		var command = new ReorderFloorCommand(
			Guid.NewGuid(),
			Guid.Empty,
			1);

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().ContainSingle(error =>
			error.PropertyName == nameof(ReorderFloorCommand.FloorId));
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-1)]
	[InlineData(int.MinValue)]
	public void Validate_WithNewNumberBelowOne_ReturnsViolation(int newNumber)
	{
		// Arrange
		var command = new ReorderFloorCommand(
			Guid.NewGuid(),
			Guid.NewGuid(),
			newNumber);

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().ContainSingle(error =>
			error.PropertyName == nameof(ReorderFloorCommand.NewNumber));
	}
}
