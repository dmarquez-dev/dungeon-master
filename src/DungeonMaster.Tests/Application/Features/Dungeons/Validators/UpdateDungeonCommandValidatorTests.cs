using DungeonMaster.Application.Features.Dungeons.Commands.UpdateDungeon;
using FluentAssertions;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Validators;

public class UpdateDungeonCommandValidatorTests
{
	private readonly UpdateDungeonCommandValidator _validator = new();

	[Fact]
	public void Validate_WithValidCommand_Passes()
	{
		// Arrange
		var command = new UpdateDungeonCommand(Guid.NewGuid(), "Valid Name", null, null);

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
		var command = new UpdateDungeonCommand(Guid.NewGuid(), name!, null, null);

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().ContainSingle(e =>
			e.PropertyName == nameof(UpdateDungeonCommand.Name));
	}

	[Fact]
	public void Validate_WithNameExceedingMaxLength_Fails()
	{
		// Arrange
		var command = new UpdateDungeonCommand(Guid.NewGuid(), new string('Z', 101), null, null);

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().ContainSingle(e =>
			e.PropertyName == nameof(UpdateDungeonCommand.Name));
	}
}
