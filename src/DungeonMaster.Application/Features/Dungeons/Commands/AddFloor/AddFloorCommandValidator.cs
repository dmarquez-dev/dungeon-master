using FluentValidation;

namespace DungeonMaster.Application.Features.Dungeons.Commands.AddFloor;

/// <summary>
///		Validates an <see cref="AddFloorCommand"/> before it is handled.
/// </summary>
public sealed class AddFloorCommandValidator
	: AbstractValidator<AddFloorCommand>
{
	/// <summary>
	///		Initializes a new instance of <see cref="AddFloorCommandValidator"/> with rules for all command fields.
	/// </summary>
	public AddFloorCommandValidator()
	{
		RuleFor(command =>
			command.DungeonId)
			.NotEmpty();

		RuleFor(command =>
			command.Name)
			.MaximumLength(100);
	}
}
