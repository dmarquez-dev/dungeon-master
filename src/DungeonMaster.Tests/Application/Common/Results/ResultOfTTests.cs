using DungeonMaster.Application.Common.Results;
using FluentAssertions;

namespace DungeonMaster.Tests.Application.Common.Results;

/// <summary>
///		Unit tests for the generic <see cref="Result{T}"/> record.
/// </summary>
public class ResultOfTTests
{
	/// <summary>
	///		Verifies that a successful result carries its value and no failure message or category.
	/// </summary>
	[Fact]
	public void Success_WithValue_IsSuccessAndCarriesNoError()
	{
		// Arrange
		var id = Guid.NewGuid();

		// Act
		var result = Result<Guid>.Success(id);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.IsFailure.Should().BeFalse();
		result.Value.Should().Be(id);
		result.Error.Should().BeNull();
		result.ErrorCategory.Should().BeNull();
		result.ValidationErrors.Should().BeEmpty();
	}

	/// <summary>
	///		Verifies that a failed result carries the supplied message and category and no value.
	/// </summary>
	[Fact]
	public void Failure_WithCategory_IsNotSuccessAndCarriesNoValue()
	{
		// Act
		var result = Result<Guid>.Failure(
			"Dungeon not found.",
			ErrorCategory.NotFound);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.IsFailure.Should().BeTrue();
		result.Value.Should().Be(Guid.Empty);
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
		var result = Result<Guid>.ValidationFailure(errors);

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
		var result = Result<Guid>.ValidationFailure([]);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.ValidationErrors.Should().NotBeNull();
		result.ValidationErrors.Should().BeEmpty();
		result.Error.Should().BeEmpty();
	}

	/// <summary>
	///		Verifies that a successful result converts implicitly to a successful valueless result.
	/// </summary>
	[Fact]
	public void ImplicitConversion_FromSuccess_ProducesASuccessfulResult()
	{
		// Arrange
		var source = Result<Guid>.Success(Guid.NewGuid());

		// Act
		Result converted = source;

		// Assert
		converted.IsSuccess.Should().BeTrue();
		converted.IsFailure.Should().BeFalse();
		converted.Error.Should().BeNull();
		converted.ErrorCategory.Should().BeNull();
		converted.ValidationErrors.Should().BeEmpty();
	}

	/// <summary>
	///		Verifies that a plain failure converts implicitly, carrying its message and category across.
	/// </summary>
	[Fact]
	public void ImplicitConversion_FromFailure_PreservesTheMessageAndCategory()
	{
		// Arrange
		var source = Result<Guid>.Failure(
			"Dungeon not found.",
			ErrorCategory.NotFound);

		// Act
		Result converted = source;

		// Assert
		converted.IsSuccess.Should().BeFalse();
		converted.IsFailure.Should().BeTrue();
		converted.Error.Should().Be("Dungeon not found.");
		converted.ErrorCategory.Should().Be(ErrorCategory.NotFound);
		converted.ValidationErrors.Should().BeEmpty();
	}

	/// <summary>
	///		Verifies that a validation failure converts implicitly without losing the individual validation errors that the summary message alone cannot carry.
	/// </summary>
	[Fact]
	public void ImplicitConversion_FromValidationFailure_PreservesEveryValidationError()
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

		var source = Result<Guid>.ValidationFailure(errors);

		// Act
		Result converted = source;

		// Assert
		converted.IsSuccess.Should().BeFalse();
		converted.IsFailure.Should().BeTrue();
		converted.ErrorCategory.Should().Be(ErrorCategory.Validation);
		converted.Error.Should().Be(source.Error);
		converted.ValidationErrors.Should().HaveCount(3);
		converted.ValidationErrors.Should().Equal(source.ValidationErrors);
	}

	/// <summary>
	///		Verifies that a validation failure carrying no individual errors still converts to a validation failure rather than losing its category.
	/// </summary>
	[Fact]
	public void ImplicitConversion_FromValidationFailureWithNoErrors_PreservesTheValidationCategory()
	{
		// Arrange
		var source = Result<Guid>.ValidationFailure([]);

		// Act
		Result converted = source;

		// Assert
		converted.IsSuccess.Should().BeFalse();
		converted.IsFailure.Should().BeTrue();
		converted.ErrorCategory.Should().Be(ErrorCategory.Validation);
		converted.Error.Should().Be(source.Error);
		converted.ValidationErrors.Should().BeEmpty();
	}
}
