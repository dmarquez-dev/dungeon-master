using DungeonMaster.Application.Common.Handlers;
using DungeonMaster.Application.Common.Results;
using DungeonMaster.Domain.Entities;
using FluentAssertions;

namespace DungeonMaster.Tests.Application.Common.Handlers;

/// <summary>
///		Unit tests for <see cref="MutationOutcomeExtensions"/>.
/// </summary>
public class MutationOutcomeExtensionsTests
{
	/// <summary>
	///		Verifies that every <see cref="MutationOutcome"/> member maps to the contracted success flag and error category.
	/// </summary>
	/// <param name="outcome">
	///		The outcome under test.
	/// </param>
	/// <param name="expectedSuccess">
	///		Whether the mapped result is expected to report success.
	/// </param>
	/// <param name="expectedCategory">
	///		The error category the mapped result is expected to carry, or <see langword="null"/> on success.
	/// </param>
	[Theory]
	[InlineData(
		MutationOutcome.Success,
		true,
		null)]
	[InlineData(
		MutationOutcome.NotFound,
		false,
		ErrorCategory.NotFound)]
	[InlineData(
		MutationOutcome.OutOfRange,
		false,
		ErrorCategory.Validation)]
	public void ToResult_WithEachOutcome_MapsToTheContractedCategory(
		MutationOutcome outcome,
		bool expectedSuccess,
		ErrorCategory? expectedCategory)
	{
		// Arrange
		const string notFoundMessage = "Floor not found.";

		// Act
		var result = outcome.ToResult(notFoundMessage);

		// Assert
		result.IsSuccess.Should().Be(expectedSuccess);
		result.ErrorCategory.Should().Be(expectedCategory);
	}

	/// <summary>
	///		Verifies that the not-found message supplied by the caller is carried onto the result.
	/// </summary>
	[Fact]
	public void ToResult_WithNotFound_CarriesTheSuppliedMessage()
	{
		// Arrange
		const string notFoundMessage = "Floor 42 not found.";

		// Act
		var result = MutationOutcome.NotFound.ToResult(notFoundMessage);

		// Assert
		result.Error.Should().Be(notFoundMessage);
	}

	/// <summary>
	///		Verifies that the single-message overload falls back to the default out-of-range wording.
	/// </summary>
	[Fact]
	public void ToResult_WithOutOfRangeAndNoSpecificMessage_UsesTheDefaultWording()
	{
		// Act
		var result = MutationOutcome.OutOfRange.ToResult("Floor not found.");

		// Assert
		result.Error.Should().Be(MutationOutcomeExtensions.DefaultOutOfRangeMessage);
	}

	/// <summary>
	///		Verifies that the two-message overload carries the caller's out-of-range wording.
	/// </summary>
	[Fact]
	public void ToResult_WithOutOfRangeAndSpecificMessage_CarriesThatMessage()
	{
		// Arrange
		const string outOfRangeMessage = "Floor number 9 is out of range for this dungeon.";

		// Act
		var result = MutationOutcome.OutOfRange.ToResult(
			"Floor not found.",
			outOfRangeMessage);

		// Assert
		result.Error.Should().Be(outOfRangeMessage);
		result.ErrorCategory.Should().Be(ErrorCategory.Validation);
	}

	/// <summary>
	///		Verifies that an outcome outside the declared enum members is rejected.
	/// </summary>
	[Fact]
	public void ToResult_WithUnrecognisedOutcome_Throws()
	{
		// Arrange
		var outcome = (MutationOutcome)99;

		// Act
		var act = () =>
			outcome.ToResult("Floor not found.");

		// Assert
		// ParamName and ActualValue are both asserted so that this distinguishes the
		// guard in ToResult from an ArgumentOutOfRangeException raised a level below it.
		act.Should().Throw<ArgumentOutOfRangeException>()
			.Which.ParamName.Should().Be("outcome");

		act.Should().Throw<ArgumentOutOfRangeException>()
			.Which.ActualValue.Should().Be((MutationOutcome)99);
	}

	/// <summary>
	///		Verifies that the zero member, which is what <see langword="default"/> produces, is rejected.
	/// </summary>
	[Fact]
	public void ToResult_WithNone_Throws()
	{
		// Act
		var act = () =>
			default(MutationOutcome).ToResult("Floor not found.");

		// Assert
		act.Should().Throw<ArgumentOutOfRangeException>()
			.Which.ParamName.Should().Be("outcome");

		act.Should().Throw<ArgumentOutOfRangeException>()
			.Which.ActualValue.Should().Be(MutationOutcome.None);
	}
}
