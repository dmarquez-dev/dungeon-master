using DungeonMaster.Application.Features.Dungeons.Commands.UpdateArea;
using FluentAssertions;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Validators;

public class UpdateAreaCommandValidatorTests
{
	private readonly UpdateAreaCommandValidator _validator = new();

	[Fact]
	public void Validate_WithValidCommand_Passes()
	{
		// Arrange
		var command = new UpdateAreaCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Hall", null, null);

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
		var command = new UpdateAreaCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), name!, null, null);

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().ContainSingle(e =>
			e.PropertyName == nameof(UpdateAreaCommand.Name));
	}

	[Fact]
	public void Validate_WithNameExceedingMaxLength_Fails()
	{
		// Arrange
		var command = new UpdateAreaCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new string('X', 101), null, null);

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().ContainSingle(e =>
			e.PropertyName == nameof(UpdateAreaCommand.Name));
	}
}
