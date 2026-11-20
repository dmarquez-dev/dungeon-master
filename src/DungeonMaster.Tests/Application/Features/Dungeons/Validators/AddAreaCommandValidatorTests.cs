using DungeonMaster.Application.Features.Dungeons.Commands.AddArea;
using FluentAssertions;
using FluentValidation;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Validators;

/// <summary>
///		The name-only validator behaviour, run against <see cref="AddAreaCommandValidator"/>.
/// </summary>
public sealed class AddAreaCommandValidatorTests
	: NameOnlyValidatorTestBase<AddAreaCommand>
{
	/// <inheritdoc/>
	protected override IValidator<AddAreaCommand> Validator { get; } = new AddAreaCommandValidator();

	/// <inheritdoc/>
	protected override string NamePropertyName =>
		nameof(AddAreaCommand.Name);

	/// <inheritdoc/>
	protected override AddAreaCommand CreateCommandWithName(string? name) =>
		new(Guid.NewGuid(), Guid.NewGuid(), name!, null);

	[Fact]
	public void Validate_WithDescriptionExceedingMaxLength_ReturnsSingleDescriptionViolation()
	{
		// Arrange
		var command = new AddAreaCommand(
			Guid.NewGuid(),
			Guid.NewGuid(),
			"Throne Room",
			new string('A', 2001));

		// Act
		var result = Validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().ContainSingle()
			.Which.PropertyName.Should().Be(nameof(AddAreaCommand.Description));
	}

	[Fact]
	public void Validate_WithDescriptionAtMaxLength_Passes()
	{
		// Arrange
		var command = new AddAreaCommand(
			Guid.NewGuid(),
			Guid.NewGuid(),
			"Throne Room",
			new string('A', 2000));

		// Act
		var result = Validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
	}

	[Fact]
	public void Validate_WithNullDescription_Passes()
	{
		// Arrange
		var command = new AddAreaCommand(
			Guid.NewGuid(),
			Guid.NewGuid(),
			"Throne Room",
			null);

		// Act
		var result = Validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
	}
}
