using DungeonMaster.Application.Features.Dungeons.Commands.CreateDungeon;
using FluentAssertions;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Validators;

public class CreateDungeonCommandValidatorTests
{
	private readonly CreateDungeonCommandValidator _validator = new();

	[Fact]
	public void Validate_WithValidCommand_Passes()
	{
		// Arrange
		var command = new CreateDungeonCommand("The Vault", null);

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
		var command = new CreateDungeonCommand(name!, null);

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().ContainSingle(e =>
			e.PropertyName == nameof(CreateDungeonCommand.Name));
	}

	[Fact]
	public void Validate_WithNameExceedingMaxLength_Fails()
	{
		// Arrange
		var command = new CreateDungeonCommand(new string('A', 101), null);

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().ContainSingle(e =>
			e.PropertyName == nameof(CreateDungeonCommand.Name));
	}
}
