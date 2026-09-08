using DungeonMaster.Application.Features.Dungeons.Commands.UpdateDungeon;
using FluentValidation;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Validators;

/// <summary>
///		The name-only validator behaviour, run against <see cref="UpdateDungeonCommandValidator"/>.
/// </summary>
public sealed class UpdateDungeonCommandValidatorTests
	: NameOnlyValidatorTestBase<UpdateDungeonCommand>
{
	/// <inheritdoc/>
	protected override IValidator<UpdateDungeonCommand> Validator { get; } = new UpdateDungeonCommandValidator();

	/// <inheritdoc/>
	protected override string NamePropertyName =>
		nameof(UpdateDungeonCommand.Name);

	/// <inheritdoc/>
	protected override UpdateDungeonCommand CreateCommandWithName(string? name) =>
		new(Guid.NewGuid(), name!, null, null);
}
