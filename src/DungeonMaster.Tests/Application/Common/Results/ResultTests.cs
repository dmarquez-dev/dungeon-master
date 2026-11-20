using DungeonMaster.Application.Common.Results;
using FluentAssertions;

namespace DungeonMaster.Tests.Application.Common.Results;

/// <summary>
///		Unit tests for the non-generic <see cref="Result"/> record.
/// </summary>
public class ResultTests
{
	/// <summary>
	///		Verifies that a successful result reports success and carries no failure message or category.
	/// </summary>
	[Fact]
	public void Success_IsSuccessAndCarriesNoError()
	{
		// Act
		var result = Result.Success();

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.IsFailure.Should().BeFalse();
		result.Error.Should().BeNull();
		result.ErrorCategory.Should().BeNull();
	}

	/// <summary>
	///		Verifies that a failed result reports failure and carries both the supplied message and the supplied category.
	/// </summary>
	[Fact]
	public void Failure_WithCategory_IsNotSuccessAndCarriesTheCategory()
	{
		// Act
		var result = Result.Failure(
			"Dungeon not found.",
			ErrorCategory.NotFound);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.IsFailure.Should().BeTrue();
		result.Error.Should().Be("Dungeon not found.");
		result.ErrorCategory.Should().Be(ErrorCategory.NotFound);
		result.ValidationErrors.Should().BeEmpty();
	}

	/// <summary>
	///		Verifies that the summary message of a validation failure carries every supplied message, not only the first.
	/// </summary>
	[Fact]
	public void ValidationFailure_WithThreeErrors_ErrorContainsAllThreeMessages()
	{
		// Arrange
		var errors = new List<ValidationError>
		{
			new(
				"Name",
				"Name is required."),
			new(
				"Width",
				"Width must be positive."),
			new(
				"Height",
				"Height must be positive."),
		};

		// Act
		var result = Result.ValidationFailure(errors);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.ErrorCategory.Should().Be(ErrorCategory.Validation);
		result.Error.Should().Contain("Name is required.");
		result.Error.Should().Contain("Width must be positive.");
		result.Error.Should().Contain("Height must be positive.");
		result.ValidationErrors.Should().HaveCount(3);
	}

	/// <summary>
	///		Verifies that a validation failure carrying no individual errors still exposes an empty, non-null collection.
	/// </summary>
	[Fact]
	public void ValidationFailure_WithNoErrors_ProducesEmptyValidationErrors()
	{
		// Act
		var result = Result.ValidationFailure([]);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.ValidationErrors.Should().NotBeNull();
		result.ValidationErrors.Should().BeEmpty();
		result.Error.Should().BeEmpty();
	}

	/// <summary>
	///		Verifies that the validation failures of a successful result are never null and are empty.
	/// </summary>
	[Fact]
	public void Success_HasEmptyValidationErrors()
	{
		// Act
		var result = Result.Success();

		// Assert
		result.ValidationErrors.Should().NotBeNull();
		result.ValidationErrors.Should().BeEmpty();
	}
}
