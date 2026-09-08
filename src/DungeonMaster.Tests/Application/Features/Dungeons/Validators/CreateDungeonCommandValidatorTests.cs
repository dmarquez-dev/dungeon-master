using DungeonMaster.Application.Features.Dungeons.Commands.CreateDungeon;
using FluentValidation;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Validators;

/// <summary>
///		The name-only validator behaviour, run against <see cref="CreateDungeonCommandValidator"/>.
/// </summary>
public sealed class CreateDungeonCommandValidatorTests
	: NameOnlyValidatorTestBase<CreateDungeonCommand>
{
	/// <inheritdoc/>
	protected override IValidator<CreateDungeonCommand> Validator { get; } = new CreateDungeonCommandValidator();

	/// <inheritdoc/>
	protected override string NamePropertyName =>
		nameof(CreateDungeonCommand.Name);

	/// <inheritdoc/>
	protected override CreateDungeonCommand CreateCommandWithName(string? name) =>
		new(name!, null);
}
