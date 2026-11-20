using FluentValidation;

namespace DungeonMaster.Application.Features.Dungeons.Commands.ReorderFloor;

/// <summary>
///		Validates a <see cref="ReorderFloorCommand"/> before it is handled. The upper bound on the new floor number depends on how many floors the dungeon holds and belongs to the aggregate, so only the lower bound is checked here.
/// </summary>
public sealed class ReorderFloorCommandValidator
	: AbstractValidator<ReorderFloorCommand>
{
	/// <summary>
	///		Initializes a new instance of <see cref="ReorderFloorCommandValidator"/> with rules for all command fields.
	/// </summary>
	public ReorderFloorCommandValidator()
	{
		RuleFor(command =>
			command.DungeonId)
			.NotEmpty();

		RuleFor(command =>
			command.FloorId)
			.NotEmpty();

		RuleFor(command =>
			command.NewNumber)
			.GreaterThanOrEqualTo(1);
	}
}
