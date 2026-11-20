using DungeonMaster.Application.Common.Results;
using FluentAssertions;

namespace DungeonMaster.Tests.Application.Common.Results;

/// <summary>
///		Unit tests for <see cref="ResultMessage"/>.
/// </summary>
public class ResultMessageTests
{
	/// <summary>
	///		Verifies that a successful generic result produces no message.
	/// </summary>
	[Fact]
	public void ForFailure_WithSuccess_ReturnsNull()
	{
		// Arrange
		var result = Result<Guid>.Success(Guid.NewGuid());

		// Act
		var message = ResultMessage.ForFailure(result);

		// Assert
		message.Should().BeNull();
	}

	/// <summary>
	///		Verifies that a validation failure carrying no individual errors falls back to the summary message.
	/// </summary>
	[Fact]
	public void ForFailure_WithValidationCategory_ReturnsTheSummaryMessage()
	{
		// Arrange
		var result = Result<Guid>.Failure(
			"The request was invalid.",
			ErrorCategory.Validation);

		// Act
		var message = ResultMessage.ForFailure(result);

		// Assert
		message.Should().Be("The request was invalid.");
	}

	/// <summary>
	///		Verifies that a not-found failure displays the message the handler supplied.
	/// </summary>
	[Fact]
	public void ForFailure_WithNotFoundCategory_ReturnsTheSuppliedMessage()
	{
		// Arrange
		var result = Result<Guid>.Failure(
			"Dungeon not found.",
			ErrorCategory.NotFound);

		// Act
		var message = ResultMessage.ForFailure(result);

		// Assert
		message.Should().Be("Dungeon not found.");
	}

	/// <summary>
	///		Verifies that a conflict failure displays the message the handler supplied.
	/// </summary>
	[Fact]
	public void ForFailure_WithConflictCategory_ReturnsTheSuppliedMessage()
	{
		// Arrange
		var result = Result<Guid>.Failure(
			"That floor number is already taken.",
			ErrorCategory.Conflict);

		// Act
		var message = ResultMessage.ForFailure(result);

		// Assert
		message.Should().Be("That floor number is already taken.");
	}

	/// <summary>
	///		Verifies that an unauthorized failure displays the fixed sign-in message rather than the internal one.
	/// </summary>
	[Fact]
	public void ForFailure_WithUnauthorizedCategory_ReturnsTheSignedOutMessage()
	{
		// Arrange
		var result = Result<Guid>.Failure(
			"No bearer token.",
			ErrorCategory.Unauthorized);

		// Act
		var message = ResultMessage.ForFailure(result);

		// Assert
		message.Should().Be("You are not signed in.");
	}

	/// <summary>
	///		Verifies that a forbidden failure displays the fixed permission message rather than the internal one.
	/// </summary>
	[Fact]
	public void ForFailure_WithForbiddenCategory_ReturnsThePermissionMessage()
	{
		// Arrange
		var result = Result<Guid>.Failure(
			"Role check failed.",
			ErrorCategory.Forbidden);

		// Act
		var message = ResultMessage.ForFailure(result);

		// Assert
		message.Should().Be("You do not have permission to do that.");
	}

	/// <summary>
	///		Verifies that every validation message appears on its own line, prefixed by its property name.
	/// </summary>
	[Fact]
	public void ForFailure_WithValidationErrors_JoinsEveryMessageOnItsOwnLine()
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
				string.Empty,
				"The dungeon must fit on the grid."),
		};

		var result = Result<Guid>.ValidationFailure(errors);

		// Act
		var message = ResultMessage.ForFailure(result);

		// Assert
		message.Should().Be(
			string.Join(
				Environment.NewLine,
				"Name: Name is required.",
				"Width: Width must be positive.",
				"The dungeon must fit on the grid."));
	}

	/// <summary>
	///		Verifies that the non-generic overload produces the same message as the generic one for the same failure.
	/// </summary>
	[Fact]
	public void ForFailure_WithNonGenericResult_ReturnsTheSameMessageAsTheGeneric()
	{
		// Arrange
		var nonGeneric = Result.Failure(
			"Dungeon not found.",
			ErrorCategory.NotFound);

		var generic = Result<Guid>.Failure(
			"Dungeon not found.",
			ErrorCategory.NotFound);

		// Act
		var nonGenericMessage = ResultMessage.ForFailure(nonGeneric);
		var genericMessage = ResultMessage.ForFailure(generic);

		// Assert
		nonGenericMessage.Should().Be(genericMessage);
		nonGenericMessage.Should().Be("Dungeon not found.");
	}

	/// <summary>
	///		Verifies that a successful non-generic result produces no message.
	/// </summary>
	[Fact]
	public void ForFailure_WithSucceedingNonGenericResult_ReturnsNull()
	{
		// Arrange
		var result = Result.Success();

		// Act
		var message = ResultMessage.ForFailure(result);

		// Assert
		message.Should().BeNull();
	}
}
