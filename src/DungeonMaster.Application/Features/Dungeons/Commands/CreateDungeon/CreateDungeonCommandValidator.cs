using FluentValidation;

namespace DungeonMaster.Application.Features.Dungeons.Commands.CreateDungeon;

/// <summary>
///		Validates <see cref="CreateDungeonCommand"/> before it reaches the handler.
/// </summary>
public sealed class CreateDungeonCommandValidator
	: AbstractValidator<CreateDungeonCommand>
{
	/// <summary>
	///		Initializes a new instance and registers all validation rules.
	/// </summary>
	public CreateDungeonCommandValidator()
	{
		RuleFor(command =>
			command.Name)
			.NotEmpty()
			.MaximumLength(100);
	}
}
