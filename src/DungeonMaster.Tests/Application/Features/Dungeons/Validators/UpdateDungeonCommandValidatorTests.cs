using DungeonMaster.Application.Features.Dungeons.Commands.UpdateDungeon;
using FluentAssertions;
using FluentValidation;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Validators;

/// <summary>
///		The shared Name-rule behaviour, run against <see cref="UpdateDungeonCommandValidator"/>, plus its Description and Notes length rules.
/// </summary>
public sealed class UpdateDungeonCommandValidatorTests
	: NameOnlyValidatorTestBase<UpdateDungeonCommand>
{
	/// <inheritdoc/>
	protected override IValidator<UpdateDungeonCommand> Validator { get; } = new UpdateDungeonCommandValidator();

	/// <inheritdoc/>
	protected override string NamePropertyName =>
		nameof(UpdateDungeonCommand.Name);

	/// <inheritdoc/>
	protected override UpdateDungeonCommand CreateCommandWithName(string? name) =>
		new(Guid.NewGuid(), name!, null, null);

	[Fact]
	public void Validate_WithDescriptionOf2001Characters_FailsWithSingleDescriptionError()
	{
		// Arrange
		var command = new UpdateDungeonCommand(Guid.NewGuid(), "Valid Name", new string('A', 2001), null);

		// Act
		var result = Validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().ContainSingle()
			.Which.PropertyName.Should().Be(nameof(UpdateDungeonCommand.Description));
	}

	[Fact]
	public void Validate_WithDescriptionOfExactly2000Characters_Passes()
	{
		// Arrange
		var command = new UpdateDungeonCommand(Guid.NewGuid(), "Valid Name", new string('A', 2000), null);

		// Act
		var result = Validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
	}

	[Fact]
	public void Validate_WithNullDescription_Passes()
	{
		// Arrange
		var command = new UpdateDungeonCommand(Guid.NewGuid(), "Valid Name", null, null);

		// Act
		var result = Validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
	}

	[Fact]
	public void Validate_WithNotesOf4001Characters_FailsWithSingleNotesError()
	{
		// Arrange
		var command = new UpdateDungeonCommand(Guid.NewGuid(), "Valid Name", null, new string('A', 4001));

		// Act
		var result = Validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().ContainSingle()
			.Which.PropertyName.Should().Be(nameof(UpdateDungeonCommand.Notes));
	}

	[Fact]
	public void Validate_WithNotesOfExactly4000Characters_Passes()
	{
		// Arrange
		var command = new UpdateDungeonCommand(Guid.NewGuid(), "Valid Name", null, new string('A', 4000));

		// Act
		var result = Validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
	}

	[Fact]
	public void Validate_WithNullNotes_Passes()
	{
		// Arrange
		var command = new UpdateDungeonCommand(Guid.NewGuid(), "Valid Name", null, null);

		// Act
		var result = Validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
	}
}
