using DungeonMaster.Application.Features.Dungeons.Commands.RemovePlacedObject;
using FluentAssertions;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Validators;

/// <summary>
///		Tests for <see cref="RemovePlacedObjectCommandValidator"/>.
/// </summary>
public class RemovePlacedObjectCommandValidatorTests
{
	private readonly RemovePlacedObjectCommandValidator _validator = new();

	[Fact]
	public void Validate_WithValidCommand_Passes()
	{
		// Arrange
		var command = new RemovePlacedObjectCommand(
			Guid.NewGuid(),
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
		var command = new RemovePlacedObjectCommand(
			Guid.Empty,
			Guid.NewGuid(),
			Guid.NewGuid());

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().ContainSingle(error =>
			error.PropertyName == nameof(RemovePlacedObjectCommand.DungeonId));
	}

	[Fact]
	public void Validate_WithEmptyFloorId_ReturnsViolation()
	{
		// Arrange
		var command = new RemovePlacedObjectCommand(
			Guid.NewGuid(),
			Guid.Empty,
			Guid.NewGuid());

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().ContainSingle(error =>
			error.PropertyName == nameof(RemovePlacedObjectCommand.FloorId));
	}

	[Fact]
	public void Validate_WithEmptyObjectId_ReturnsViolation()
	{
		// Arrange
		var command = new RemovePlacedObjectCommand(
			Guid.NewGuid(),
			Guid.NewGuid(),
			Guid.Empty);

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().ContainSingle(error =>
			error.PropertyName == nameof(RemovePlacedObjectCommand.ObjectId));
	}
}
