using FluentValidation;

namespace DungeonMaster.Application.Features.Dungeons.Commands.UpdateDungeon;

/// <summary>
///		Validates an <see cref="UpdateDungeonCommand"/> before it is handled.
/// </summary>
public sealed class UpdateDungeonCommandValidator
	: AbstractValidator<UpdateDungeonCommand>
{
	/// <summary>
	///		Initializes a new instance of <see cref="UpdateDungeonCommandValidator"/> with rules for all command fields.
	/// </summary>
	public UpdateDungeonCommandValidator()
	{
		RuleFor(command =>
			command.Name)
			.NotEmpty()
			.MaximumLength(100);

		RuleFor(command =>
			command.Description)
			.MaximumLength(2000);

		RuleFor(command =>
			command.Notes)
			.MaximumLength(4000);
	}
}
