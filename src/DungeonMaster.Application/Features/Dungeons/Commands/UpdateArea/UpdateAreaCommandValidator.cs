using FluentValidation;

namespace DungeonMaster.Application.Features.Dungeons.Commands.UpdateArea;

/// <summary>
///		Validates an <see cref="UpdateAreaCommand"/> before it is handled.
/// </summary>
public sealed class UpdateAreaCommandValidator
	: AbstractValidator<UpdateAreaCommand>
{
	/// <summary>
	///		Initializes a new instance of <see cref="UpdateAreaCommandValidator"/> with rules for all command fields.
	/// </summary>
	public UpdateAreaCommandValidator()
	{
		RuleFor(command =>
			command.Name)
			.NotEmpty()
			.MaximumLength(100);
	}
}
