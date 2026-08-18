using FluentValidation;

namespace DungeonMaster.Application.Features.Dungeons.Commands.DeleteDungeon;

/// <summary>
///		Validates a <see cref="DeleteDungeonCommand"/> before it is handled.
/// </summary>
public sealed class DeleteDungeonCommandValidator
	: AbstractValidator<DeleteDungeonCommand>
{
	/// <summary>
	///		Initializes a new instance of <see cref="DeleteDungeonCommandValidator"/> with rules for all command fields.
	/// </summary>
	public DeleteDungeonCommandValidator()
	{
		RuleFor(command =>
			command.Id)
			.NotEmpty();
	}
}
