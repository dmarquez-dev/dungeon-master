using DungeonMaster.Application.Features.Dungeons.Commands.UpdatePlacedObject;
using DungeonMaster.Domain.Entities.Floors;
using DungeonMaster.Domain.ValueObjects;
using FluentAssertions;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Validators;

/// <summary>
///		Tests for <see cref="UpdatePlacedObjectCommandValidator"/>, including the scale bounds it takes from <see cref="ObjectScale"/>.
/// </summary>
public class UpdatePlacedObjectCommandValidatorTests
{
	private readonly UpdatePlacedObjectCommandValidator _validator = new();

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
	public void Validate_WithNewScaleAtTheBounds_Passes()
	{
		// Act
		var atMinimum = _validator.Validate(CommandWith(ObjectScale.Min));
		var atMaximum = _validator.Validate(CommandWith(ObjectScale.Max));

		// Assert
		atMinimum.IsValid.Should().BeTrue();
		atMaximum.IsValid.Should().BeTrue();
	}

	[Fact]
	public void Validate_WithNewScaleBelowMinimum_ReturnsViolation()
	{
		// Arrange
		var command = CommandWith(ObjectScale.Min / 2f);

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().ContainSingle(error =>
			error.PropertyName == nameof(UpdatePlacedObjectCommand.NewScale));
	}

	[Fact]
	public void Validate_WithNewScaleAboveMaximum_ReturnsViolation()
	{
		// Arrange
		var command = CommandWith(ObjectScale.Max * 2f);

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().ContainSingle(error =>
			error.PropertyName == nameof(UpdatePlacedObjectCommand.NewScale));
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
			error.PropertyName == nameof(UpdatePlacedObjectCommand.DungeonId));
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
			error.PropertyName == nameof(UpdatePlacedObjectCommand.FloorId));
	}

	[Fact]
	public void Validate_WithEmptyObjectId_ReturnsViolation()
	{
		// Arrange
		var command = CommandWith(
			ObjectScale.Default,
			objectId: Guid.Empty);

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().ContainSingle(error =>
			error.PropertyName == nameof(UpdatePlacedObjectCommand.ObjectId));
	}

	private static UpdatePlacedObjectCommand CommandWith(
		float newScale,
		Guid? dungeonId = null,
		Guid? floorId = null,
		Guid? objectId = null) =>
			new(
				dungeonId ?? Guid.NewGuid(),
				floorId ?? Guid.NewGuid(),
				objectId ?? Guid.NewGuid(),
				new CanvasPoint(0f, 0f),
				ObjectRotation.None,
				newScale);
}
