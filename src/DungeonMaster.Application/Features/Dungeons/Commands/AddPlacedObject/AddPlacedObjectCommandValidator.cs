using DungeonMaster.Domain.Entities.Floor;
using FluentValidation;

namespace DungeonMaster.Application.Features.Dungeons.Commands.AddPlacedObject;

/// <summary>
///		Validates an <see cref="AddPlacedObjectCommand"/> before it is handled.
/// </summary>
public sealed class AddPlacedObjectCommandValidator
	: AbstractValidator<AddPlacedObjectCommand>
{
	/// <summary>
	///		Initializes a new instance of <see cref="AddPlacedObjectCommandValidator"/> with rules for all command fields. The scale bounds reference <see cref="ObjectScale"/> so that this validator cannot drift from the domain guard.
	/// </summary>
	public AddPlacedObjectCommandValidator()
	{
		RuleFor(command =>
			command.DungeonId)
			.NotEmpty();

		RuleFor(command =>
			command.FloorId)
			.NotEmpty();

		RuleFor(command =>
			command.Kind)
			.IsInEnum();

		RuleFor(command =>
			command.Position)
			.NotNull();

		RuleFor(command =>
			command.Rotation)
			.IsInEnum();

		RuleFor(command =>
			command.Scale)
			.InclusiveBetween(
				ObjectScale.Min,
				ObjectScale.Max);
	}
}
