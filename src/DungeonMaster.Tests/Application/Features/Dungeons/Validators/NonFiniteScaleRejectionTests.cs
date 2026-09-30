using DungeonMaster.Application.Features.Dungeons.Commands.AddPlacedObject;
using DungeonMaster.Application.Features.Dungeons.Commands.UpdatePlacedObject;
using DungeonMaster.Domain.Entities.Floors;
using DungeonMaster.Domain.ValueObjects;
using FluentAssertions;

namespace DungeonMaster.Tests.Application.Features.Dungeons.Validators;

/// <summary>
///		Pins what FluentValidation's <c>InclusiveBetween(ObjectScale.Min, ObjectScale.Max)</c> rule actually returns for a non-finite scale, on <see cref="AddPlacedObjectCommandValidator"/> and <see cref="UpdatePlacedObjectCommandValidator"/>.
///		Every verdict recorded below was observed by running these tests, not derived from the rule.
///		<c>float.NaN</c> is REJECTED on both validators, established by <see cref="Validate_WithScaleNaN_ReturnsInclusiveBetweenViolationOnBothValidators"/>.
///		<c>float.PositiveInfinity</c> is REJECTED on both validators, established by <see cref="Validate_WithScalePositiveInfinity_ReturnsInclusiveBetweenViolationOnBothValidators"/>.
///		<c>float.NegativeInfinity</c> is REJECTED on both validators, established by <see cref="Validate_WithScaleNegativeInfinity_ReturnsInclusiveBetweenViolationOnBothValidators"/>.
///		In all six rejections the single reported failure carries error code <c>InclusiveBetweenValidator</c> on the scale property, so the rejection is that rule and not another rule on the same command.
/// </summary>
/// <remarks>
///		The <c>NaN</c> verdict is worth stating explicitly because it does not follow from C# comparison operators.
///		Both <c>float.NaN &lt; ObjectScale.Min</c> and <c>float.NaN &gt; ObjectScale.Max</c> are <c>false</c>, so a range check written in the NEGATIVE operator form — <c>if (v &lt; Min || v &gt; Max) fail;</c> — would ACCEPT <c>NaN</c>. The claim is limited to that form: the positive form <c>if (!(v &gt;= Min &amp;&amp; v &lt;= Max)) fail;</c> rejects <c>NaN</c>, because <c>float.NaN &gt;= ObjectScale.Min</c> is <c>false</c> as well.
///		<c>InclusiveBetween</c> compares through <c>FluentValidation.Validators.ComparableComparer&lt;float&gt;.Instance</c> instead, whose total order places <c>NaN</c> below every other value.
///		Nothing in this class asserts that mechanism, only the verdict, which is what was run.
///		<para>
///		<see cref="Validate_WithScaleAtDefault_IsValidOnBothValidators"/> is the positive control.
///		A validator that rejected every command would satisfy all three rejection tests while proving nothing, so that control is what stops a run in which every non-finite case reports invalid from being misread as a rejection result.
///		</para>
/// </remarks>
public class NonFiniteScaleRejectionTests
{
	private readonly AddPlacedObjectCommandValidator _addValidator = new();
	private readonly UpdatePlacedObjectCommandValidator _updateValidator = new();

	[Fact]
	public void Validate_WithScaleNaN_ReturnsInclusiveBetweenViolationOnBothValidators()
	{
		// Act
		var add = _addValidator.Validate(AddCommandWith(float.NaN));
		var update = _updateValidator.Validate(UpdateCommandWith(float.NaN));

		// Assert
		add.IsValid.Should().BeFalse();
		add.Errors.Should().ContainSingle(error =>
			error.PropertyName == nameof(AddPlacedObjectCommand.Scale)
			&& error.ErrorCode == InclusiveBetweenErrorCode);

		update.IsValid.Should().BeFalse();
		update.Errors.Should().ContainSingle(error =>
			error.PropertyName == nameof(UpdatePlacedObjectCommand.NewScale)
			&& error.ErrorCode == InclusiveBetweenErrorCode);
	}

	[Fact]
	public void Validate_WithScalePositiveInfinity_ReturnsInclusiveBetweenViolationOnBothValidators()
	{
		// Act
		var add = _addValidator.Validate(AddCommandWith(float.PositiveInfinity));
		var update = _updateValidator.Validate(UpdateCommandWith(float.PositiveInfinity));

		// Assert
		add.IsValid.Should().BeFalse();
		add.Errors.Should().ContainSingle(error =>
			error.PropertyName == nameof(AddPlacedObjectCommand.Scale)
			&& error.ErrorCode == InclusiveBetweenErrorCode);

		update.IsValid.Should().BeFalse();
		update.Errors.Should().ContainSingle(error =>
			error.PropertyName == nameof(UpdatePlacedObjectCommand.NewScale)
			&& error.ErrorCode == InclusiveBetweenErrorCode);
	}

	[Fact]
	public void Validate_WithScaleNegativeInfinity_ReturnsInclusiveBetweenViolationOnBothValidators()
	{
		// Act
		var add = _addValidator.Validate(AddCommandWith(float.NegativeInfinity));
		var update = _updateValidator.Validate(UpdateCommandWith(float.NegativeInfinity));

		// Assert
		add.IsValid.Should().BeFalse();
		add.Errors.Should().ContainSingle(error =>
			error.PropertyName == nameof(AddPlacedObjectCommand.Scale)
			&& error.ErrorCode == InclusiveBetweenErrorCode);

		update.IsValid.Should().BeFalse();
		update.Errors.Should().ContainSingle(error =>
			error.PropertyName == nameof(UpdatePlacedObjectCommand.NewScale)
			&& error.ErrorCode == InclusiveBetweenErrorCode);
	}

	[Fact]
	public void Validate_WithScaleAtDefault_IsValidOnBothValidators()
	{
		// Act
		var add = _addValidator.Validate(AddCommandWith(ObjectScale.Default));
		var update = _updateValidator.Validate(UpdateCommandWith(ObjectScale.Default));

		// Assert
		add.IsValid.Should().BeTrue();
		update.IsValid.Should().BeTrue();
	}

	private const string InclusiveBetweenErrorCode = "InclusiveBetweenValidator";

	private static AddPlacedObjectCommand AddCommandWith(float scale) =>
		new(
			Guid.NewGuid(),
			Guid.NewGuid(),
			ObjectKind.DoorWood,
			new CanvasPoint(0f, 0f),
			ObjectRotation.None,
			scale);

	private static UpdatePlacedObjectCommand UpdateCommandWith(float newScale) =>
		new(
			Guid.NewGuid(),
			Guid.NewGuid(),
			Guid.NewGuid(),
			new CanvasPoint(0f, 0f),
			ObjectRotation.None,
			newScale);
}
