using FluentValidation;

namespace DungeonMaster.Application.Features.Dungeons.Commands.UpdateAreaOutline;

/// <summary>
///		Validates an <see cref="UpdateAreaOutlineCommand"/> before it is handled.
/// </summary>
public sealed class UpdateAreaOutlineCommandValidator
	: AbstractValidator<UpdateAreaOutlineCommand>
{
	/// <summary>
	///		Initializes a new instance of <see cref="UpdateAreaOutlineCommandValidator"/> with rules for all command fields.
	/// </summary>
	public UpdateAreaOutlineCommandValidator()
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

		RuleFor(command =>
			command.Outline)
			.NotNull()
			.NotEmpty();
	}
}
