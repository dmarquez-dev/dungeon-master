using FluentValidation;

namespace DungeonMaster.Application.Features.Dungeons.Commands.AddArea;

/// <summary>
///		Validates an <see cref="AddAreaCommand"/> before it is handled.
/// </summary>
public sealed class AddAreaCommandValidator
	: AbstractValidator<AddAreaCommand>
{
	/// <summary>
	///		Initializes a new instance of <see cref="AddAreaCommandValidator"/> with rules for all command fields.
	/// </summary>
	public AddAreaCommandValidator()
	{
		RuleFor(command =>
			command.Name)
			.NotEmpty()
			.MaximumLength(100);
	}
}
