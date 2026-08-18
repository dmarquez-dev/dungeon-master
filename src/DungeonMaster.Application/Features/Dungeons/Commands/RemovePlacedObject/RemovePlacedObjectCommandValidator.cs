using FluentValidation;

namespace DungeonMaster.Application.Features.Dungeons.Commands.RemovePlacedObject;

/// <summary>
///		Validates a <see cref="RemovePlacedObjectCommand"/> before it is handled.
/// </summary>
public sealed class RemovePlacedObjectCommandValidator
	: AbstractValidator<RemovePlacedObjectCommand>
{
	/// <summary>
	///		Initializes a new instance of <see cref="RemovePlacedObjectCommandValidator"/> with rules for all command fields.
	/// </summary>
	public RemovePlacedObjectCommandValidator()
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
	}
}
