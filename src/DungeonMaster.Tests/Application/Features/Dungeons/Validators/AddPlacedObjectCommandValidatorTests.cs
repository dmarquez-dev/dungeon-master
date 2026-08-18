using DungeonMaster.Application.Features.Dungeons.Commands.AddPlacedObject;
using DungeonMaster.Domain.Entities.Floor;
using DungeonMaster.Domain.ValueObjects;
using FluentAssertions;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Validators;

/// <summary>
///		Tests for <see cref="AddPlacedObjectCommandValidator"/>, including the scale bounds it takes from <see cref="ObjectScale"/>.
/// </summary>
public class AddPlacedObjectCommandValidatorTests
{
	private readonly AddPlacedObjectCommandValidator _validator = new();

	[Fact]
	public void Validate_WithValidCommand_Passes()
	{
		// Arrange
		var command = CommandWith(ObjectScale.Default);

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
	}

	[Fact]
	public void Validate_WithScaleAtTheBounds_Passes()
	{
		// Act
		var atMinimum = _validator.Validate(CommandWith(ObjectScale.Min));
		var atMaximum = _validator.Validate(CommandWith(ObjectScale.Max));

		// Assert
		atMinimum.IsValid.Should().BeTrue();
		atMaximum.IsValid.Should().BeTrue();
	}

	[Fact]
	public void Validate_WithScaleBelowMinimum_ReturnsViolation()
	{
		// Arrange
		var command = CommandWith(ObjectScale.Min / 2f);

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().ContainSingle(error =>
			error.PropertyName == nameof(AddPlacedObjectCommand.Scale));
	}

	[Fact]
	public void Validate_WithScaleAboveMaximum_ReturnsViolation()
	{
		// Arrange
		var command = CommandWith(ObjectScale.Max * 2f);

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().ContainSingle(error =>
			error.PropertyName == nameof(AddPlacedObjectCommand.Scale));
	}

	[Fact]
	public void Validate_WithEmptyDungeonId_ReturnsViolation()
	{
		// Arrange
		var command = CommandWith(
			ObjectScale.Default,
			dungeonId: Guid.Empty);

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().ContainSingle(error =>
			error.PropertyName == nameof(AddPlacedObjectCommand.DungeonId));
	}

	[Fact]
	public void Validate_WithEmptyFloorId_ReturnsViolation()
	{
		// Arrange
		var command = CommandWith(
			ObjectScale.Default,
			floorId: Guid.Empty);

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().ContainSingle(error =>
			error.PropertyName == nameof(AddPlacedObjectCommand.FloorId));
	}

	[Fact]
	public void Validate_WithRotationOutsideTheEnum_ReturnsViolation()
	{
		// Arrange
		var command = new AddPlacedObjectCommand(
			Guid.NewGuid(),
			Guid.NewGuid(),
			ObjectKind.DoorWood,
			new CanvasPoint(0f, 0f),
			(ObjectRotation)99,
			ObjectScale.Default);

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().ContainSingle(error =>
			error.PropertyName == nameof(AddPlacedObjectCommand.Rotation));
	}

	private static AddPlacedObjectCommand CommandWith(
		float scale,
		Guid? dungeonId = null,
		Guid? floorId = null) =>
			new(
				dungeonId ?? Guid.NewGuid(),
				floorId ?? Guid.NewGuid(),
				ObjectKind.DoorWood,
				new CanvasPoint(0f, 0f),
				ObjectRotation.None,
				scale);
}
