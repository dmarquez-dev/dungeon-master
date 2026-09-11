using DungeonMaster.Application.Features.Dungeons.Commands.AddArea;
using FluentValidation;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Validators;

/// <summary>
///		The name-only validator behaviour, run against <see cref="AddAreaCommandValidator"/>.
/// </summary>
public sealed class AddAreaCommandValidatorTests
	: NameOnlyValidatorTestBase<AddAreaCommand>
{
	/// <inheritdoc/>
	protected override IValidator<AddAreaCommand> Validator { get; } = new AddAreaCommandValidator();

	/// <inheritdoc/>
	protected override string NamePropertyName =>
		nameof(AddAreaCommand.Name);

	/// <inheritdoc/>
	protected override AddAreaCommand CreateCommandWithName(string? name) =>
		new(Guid.NewGuid(), Guid.NewGuid(), name!, null);
}
