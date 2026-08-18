using DungeonMaster.Domain.Entities.Floor;
using FluentValidation;

namespace DungeonMaster.Application.Features.Dungeons.Commands.UpdatePlacedObject;

/// <summary>
///		Validates an <see cref="UpdatePlacedObjectCommand"/> before it is handled.
/// </summary>
public sealed class UpdatePlacedObjectCommandValidator
	: AbstractValidator<UpdatePlacedObjectCommand>
{
	/// <summary>
	///		Initializes a new instance of <see cref="UpdatePlacedObjectCommandValidator"/> with rules for all command fields. The scale bounds reference <see cref="ObjectScale"/> so that this validator cannot drift from the domain guard.
	/// </summary>
	public UpdatePlacedObjectCommandValidator()
	{
		RuleFor(command =>
			command.DungeonId)
			.NotEmpty();

		RuleFor(command =>
			command.FloorId)
			.NotEmpty();

		RuleFor(command =>
			command.ObjectId)
			.NotEmpty();

		RuleFor(command =>
			command.NewPosition)
			.NotNull();

		RuleFor(command =>
			command.NewRotation)
			.IsInEnum();

		RuleFor(command =>
			command.NewScale)
			.InclusiveBetween(
				ObjectScale.Min,
				ObjectScale.Max);
	}
}
