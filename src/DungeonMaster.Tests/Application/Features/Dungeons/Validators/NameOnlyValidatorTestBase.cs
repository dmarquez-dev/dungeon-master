using FluentAssertions;
using FluentValidation;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Validators;

/// <summary>
///		The shared behaviour of every command validator whose only rule is <c>RuleFor(c =&gt; c.Name).NotEmpty().MaximumLength(100)</c> (MASTER-064).
///		xUnit does not discover an abstract class, so each fact below runs once per concrete leaf, against that leaf's own command type and that leaf's own validator instance.
///		Variation is supplied only through the abstract members: a leaf is a leaf because it overrides them, never because a scan found it.
/// </summary>
/// <typeparam name="TCommand">
///		The command type the leaf's validator validates.
/// </typeparam>
public abstract class NameOnlyValidatorTestBase<TCommand>
{
	/// <summary>
	///		The validator under test, supplied by the leaf.
	/// </summary>
	protected abstract IValidator<TCommand> Validator { get; }

	/// <summary>
	///		The name of the validated property, supplied by the leaf through <c>nameof</c> on its own command type so the reference stays compile-checked.
	/// </summary>
	protected abstract string NamePropertyName { get; }

	/// <summary>
	///		Builds a command that is valid in every respect except that its name is the supplied value.
	/// </summary>
	/// <param name="name">
	///		The name to place on the command; may be empty, whitespace or <see langword="null"/>.
	/// </param>
	/// <returns>
	///		A command carrying <paramref name="name"/>.
	/// </returns>
	protected abstract TCommand CreateCommandWithName(string? name);

	[Fact]
	public void Validate_WithValidCommand_Passes()
	{
		// Arrange
		var command = CreateCommandWithName("Valid Name");

		// Act
		var result = Validator.Validate(command);

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
		var command = CreateCommandWithName(name);

		// Act
		var result = Validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().ContainSingle(e =>
			e.PropertyName == NamePropertyName);
	}

	[Fact]
	public void Validate_WithNameExceedingMaxLength_Fails()
	{
		// Arrange
		var command = CreateCommandWithName(new string('A', 101));

		// Act
		var result = Validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().ContainSingle(e =>
			e.PropertyName == NamePropertyName);
	}
}
