using DungeonMaster.Application.Features.Dungeons.Commands.CreateDungeon;
using FluentAssertions;
using FluentValidation;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Validators;

/// <summary>
///		The shared Name-rule behaviour, run against <see cref="CreateDungeonCommandValidator"/>, plus its Description length rule.
/// </summary>
public sealed class CreateDungeonCommandValidatorTests
	: NameOnlyValidatorTestBase<CreateDungeonCommand>
{
	/// <inheritdoc/>
	protected override IValidator<CreateDungeonCommand> Validator { get; } = new CreateDungeonCommandValidator();

	/// <inheritdoc/>
	protected override string NamePropertyName =>
		nameof(CreateDungeonCommand.Name);

	/// <inheritdoc/>
	protected override CreateDungeonCommand CreateCommandWithName(string? name) =>
		new(name!, null);

	[Fact]
	public void Validate_WithDescriptionOf2001Characters_FailsWithSingleDescriptionError()
	{
		// Arrange
		var command = new CreateDungeonCommand("Valid Name", new string('A', 2001));

		// Act
		var result = Validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().ContainSingle()
			.Which.PropertyName.Should().Be(nameof(CreateDungeonCommand.Description));
	}

	[Fact]
	public void Validate_WithDescriptionOfExactly2000Characters_Passes()
	{
		// Arrange
		var command = new CreateDungeonCommand("Valid Name", new string('A', 2000));

		// Act
		var result = Validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
	}

	[Fact]
	public void Validate_WithNullDescription_Passes()
	{
		// Arrange
		var command = new CreateDungeonCommand("Valid Name", null);

		// Act
		var result = Validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
	}
}
