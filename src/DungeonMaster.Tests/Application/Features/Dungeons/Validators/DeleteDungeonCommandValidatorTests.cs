using DungeonMaster.Application.Features.Dungeons.Commands.DeleteDungeon;
using FluentAssertions;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Validators;

/// <summary>
///		Tests for <see cref="DeleteDungeonCommandValidator"/>.
/// </summary>
public class DeleteDungeonCommandValidatorTests
{
	private readonly DeleteDungeonCommandValidator _validator = new();

	[Fact]
	public void Validate_WithValidCommand_Passes()
	{
		// Arrange
		var command = new DeleteDungeonCommand(Guid.NewGuid());

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
	}

	[Fact]
	public void Validate_WithEmptyId_ReturnsViolation()
	{
		// Arrange
		var command = new DeleteDungeonCommand(Guid.Empty);

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().ContainSingle(error =>
			error.PropertyName == nameof(DeleteDungeonCommand.Id));
	}
}
