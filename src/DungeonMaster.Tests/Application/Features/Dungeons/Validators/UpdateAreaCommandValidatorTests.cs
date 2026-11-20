using DungeonMaster.Application.Features.Dungeons.Commands.UpdateArea;
using FluentAssertions;
using FluentValidation;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Validators;

/// <summary>
///		The name-only validator behaviour, run against <see cref="UpdateAreaCommandValidator"/>.
/// </summary>
public sealed class UpdateAreaCommandValidatorTests
	: NameOnlyValidatorTestBase<UpdateAreaCommand>
{
	/// <inheritdoc/>
	protected override IValidator<UpdateAreaCommand> Validator { get; } = new UpdateAreaCommandValidator();

	/// <inheritdoc/>
	protected override string NamePropertyName =>
		nameof(UpdateAreaCommand.Name);

	/// <inheritdoc/>
	protected override UpdateAreaCommand CreateCommandWithName(string? name) =>
		new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), name!, null, null);

	[Fact]
	public void Validate_WithDescriptionExceedingMaxLength_ReturnsSingleDescriptionViolation()
	{
		// Arrange
		var command = new UpdateAreaCommand(
			Guid.NewGuid(),
			Guid.NewGuid(),
			Guid.NewGuid(),
			"Throne Room",
			new string('A', 2001),
			null);

		// Act
		var result = Validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().ContainSingle()
			.Which.PropertyName.Should().Be(nameof(UpdateAreaCommand.Description));
	}

	[Fact]
	public void Validate_WithDescriptionAtMaxLength_Passes()
	{
		// Arrange
		var command = new UpdateAreaCommand(
			Guid.NewGuid(),
			Guid.NewGuid(),
			Guid.NewGuid(),
			"Throne Room",
			new string('A', 2000),
			null);

		// Act
		var result = Validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
	}

	[Fact]
	public void Validate_WithNullDescription_Passes()
	{
		// Arrange
		var command = new UpdateAreaCommand(
			Guid.NewGuid(),
			Guid.NewGuid(),
			Guid.NewGuid(),
			"Throne Room",
			null,
			"A hidden door behind the throne.");

		// Act
		var result = Validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
	}

	[Fact]
	public void Validate_WithNotesExceedingMaxLength_ReturnsSingleNotesViolation()
	{
		// Arrange
		var command = new UpdateAreaCommand(
			Guid.NewGuid(),
			Guid.NewGuid(),
			Guid.NewGuid(),
			"Throne Room",
			null,
			new string('A', 2001));

		// Act
		var result = Validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().ContainSingle()
			.Which.PropertyName.Should().Be(nameof(UpdateAreaCommand.Notes));
	}

	[Fact]
	public void Validate_WithNotesAtMaxLength_Passes()
	{
		// Arrange
		var command = new UpdateAreaCommand(
			Guid.NewGuid(),
			Guid.NewGuid(),
			Guid.NewGuid(),
			"Throne Room",
			null,
			new string('A', 2000));

		// Act
		var result = Validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
	}

	[Fact]
	public void Validate_WithNullNotes_Passes()
	{
		// Arrange
		var command = new UpdateAreaCommand(
			Guid.NewGuid(),
			Guid.NewGuid(),
			Guid.NewGuid(),
			"Throne Room",
			"A vaulted hall.",
			null);

		// Act
		var result = Validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
	}
}
