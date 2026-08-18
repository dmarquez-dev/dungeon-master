using FluentValidation;

namespace DungeonMaster.Application.Features.Dungeons.Commands.RemoveFloor;

/// <summary>
///		Validates a <see cref="RemoveFloorCommand"/> before it is handled.
/// </summary>
public sealed class RemoveFloorCommandValidator
	: AbstractValidator<RemoveFloorCommand>
{
	/// <summary>
	///		Initializes a new instance of <see cref="RemoveFloorCommandValidator"/> with rules for all command fields.
	/// </summary>
	public RemoveFloorCommandValidator()
	{
		RuleFor(command =>
			command.DungeonId)
			.NotEmpty();

		RuleFor(command =>
			command.FloorId)
			.NotEmpty();
	}
}
