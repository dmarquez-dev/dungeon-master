using FluentValidation;

namespace DungeonMaster.Application.Features.Dungeons.Commands.CreateDungeon;

/// <summary>
///		Validates a <see cref="CreateDungeonCommand"/> before it is handled.
/// </summary>
public sealed class CreateDungeonCommandValidator
	: AbstractValidator<CreateDungeonCommand>
{
	/// <summary>
	///		Initializes a new instance of <see cref="CreateDungeonCommandValidator"/> with rules for all command fields.
	/// </summary>
	public CreateDungeonCommandValidator()
	{
		RuleFor(command =>
			command.Name)
			.NotEmpty()
			.MaximumLength(100);
	}
}
