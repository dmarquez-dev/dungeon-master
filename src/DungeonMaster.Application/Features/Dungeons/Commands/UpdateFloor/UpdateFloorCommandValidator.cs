using FluentValidation;

namespace DungeonMaster.Application.Features.Dungeons.Commands.UpdateFloor;

/// <summary>
///		Validates an <see cref="UpdateFloorCommand"/> before it is handled.
/// </summary>
public sealed class UpdateFloorCommandValidator
	: AbstractValidator<UpdateFloorCommand>
{
	/// <summary>
	///		Initializes a new instance of <see cref="UpdateFloorCommandValidator"/> with rules for all command fields.
	/// </summary>
	public UpdateFloorCommandValidator()
	{
		RuleFor(command =>
			command.DungeonId)
			.NotEmpty();

		RuleFor(command =>
			command.FloorId)
			.NotEmpty();

		RuleFor(command =>
			command.Name)
			.MaximumLength(100);
	}
}
