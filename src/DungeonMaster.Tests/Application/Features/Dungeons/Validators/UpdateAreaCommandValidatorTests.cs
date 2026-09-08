using DungeonMaster.Application.Features.Dungeons.Commands.UpdateArea;
using FluentValidation;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Validators;

/// <summary>
///		The name-only validator behaviour, run against <see cref="UpdateAreaCommandValidator"/>.
/// </summary>
public sealed class UpdateAreaCommandValidatorTests
	: NameOnlyValidatorTestBase<UpdateAreaCommand>
{
	/// <inheritdoc/>
	protected override IValidator<UpdateAreaCommand> Validator { get; } = new UpdateAreaCommandValidator();

	/// <inheritdoc/>
	protected override string NamePropertyName =>
		nameof(UpdateAreaCommand.Name);

	/// <inheritdoc/>
	protected override UpdateAreaCommand CreateCommandWithName(string? name) =>
		new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), name!, null, null);
}
