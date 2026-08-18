using DungeonMaster.Application.Features.Dungeons.Commands.RemoveArea;
using FluentAssertions;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Validators;

/// <summary>
///		Tests for <see cref="RemoveAreaCommandValidator"/>.
/// </summary>
public class RemoveAreaCommandValidatorTests
{
	private readonly RemoveAreaCommandValidator _validator = new();

	[Fact]
	public void Validate_WithValidCommand_Passes()
	{
		// Arrange
		var command = new RemoveAreaCommand(
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
		var command = new RemoveAreaCommand(
			Guid.Empty,
			Guid.NewGuid(),
			Guid.NewGuid());

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().ContainSingle(error =>
			error.PropertyName == nameof(RemoveAreaCommand.DungeonId));
	}

	[Fact]
	public void Validate_WithEmptyFloorId_ReturnsViolation()
	{
		// Arrange
		var command = new RemoveAreaCommand(
			Guid.NewGuid(),
			Guid.Empty,
			Guid.NewGuid());

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().ContainSingle(error =>
			error.PropertyName == nameof(RemoveAreaCommand.FloorId));
	}

	[Fact]
	public void Validate_WithEmptyAreaId_ReturnsViolation()
	{
		// Arrange
		var command = new RemoveAreaCommand(
			Guid.NewGuid(),
			Guid.NewGuid(),
			Guid.Empty);

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().ContainSingle(error =>
			error.PropertyName == nameof(RemoveAreaCommand.AreaId));
	}
}
