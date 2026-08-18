using FluentValidation;

namespace DungeonMaster.Application.Features.Dungeons.Commands.RemoveArea;

/// <summary>
///		Validates a <see cref="RemoveAreaCommand"/> before it is handled.
/// </summary>
public sealed class RemoveAreaCommandValidator
	: AbstractValidator<RemoveAreaCommand>
{
	/// <summary>
	///		Initializes a new instance of <see cref="RemoveAreaCommandValidator"/> with rules for all command fields.
	/// </summary>
	public RemoveAreaCommandValidator()
	{
		RuleFor(command =>
			command.DungeonId)
			.NotEmpty();

		RuleFor(command =>
			command.FloorId)
			.NotEmpty();

		RuleFor(command =>
			command.AreaId)
			.NotEmpty();
	}
}
