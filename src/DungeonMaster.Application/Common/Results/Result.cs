namespace DungeonMaster.Application.Common.Results;

/// <summary>
///		Represents the outcome of an operation that produces no value.
/// </summary>
public sealed record Result
{
	private Result()
	{
	}

	/// <summary>
	///		Gets a value indicating whether the operation succeeded.
	/// </summary>
	public bool IsSuccess { get; private init; }

	/// <summary>
	///		Gets a value indicating whether the operation failed.
	/// </summary>
	public bool IsFailure =>
		!IsSuccess;

	/// <summary>
	///		Gets the summary failure message, or <see langword="null"/> when the operation succeeded.
	/// </summary>
	public string? Error { get; private init; }

	/// <summary>
	///		Gets the category of the failure, or <see langword="null"/> when the operation succeeded.
	/// </summary>
	public ErrorCategory? ErrorCategory { get; private init; }

	/// <summary>
	///		Gets the individual validation failures. This is never <see langword="null"/>, and is empty unless the operation failed validation.
	/// </summary>
	public IReadOnlyList<ValidationError> ValidationErrors { get; private init; } = [];

	/// <summary>
	///		Creates a successful result.
	/// </summary>
	/// <returns>
	///		A <see cref="Result"/> representing a successful outcome.
	/// </returns>
	public static Result Success() =>
		new()
		{
			IsSuccess = true,
		};

	/// <summary>
	///		Creates a failed result carrying a message and a category.
	/// </summary>
	/// <param name="error">
	///		A human-readable description of the failure.
	/// </param>
	/// <param name="category">
	///		The category that classifies the type of failure.
	/// </param>
	/// <returns>
	///		A <see cref="Result"/> representing a failed outcome.
	/// </returns>
	public static Result Failure(
		string error,
		ErrorCategory category) =>
			new()
			{
				IsSuccess = false,
				Error = error,
				ErrorCategory = category,
			};

	/// <summary>
	///		Creates a failed result carrying every validation failure.
	/// </summary>
	/// <param name="errors">
	///		The individual validation failures produced by the operation.
	/// </param>
	/// <returns>
	///		A <see cref="Result"/> representing a validation failure whose summary message carries every supplied message.
	/// </returns>
	public static Result ValidationFailure(
		IReadOnlyList<ValidationError> errors) =>
			new()
			{
				IsSuccess = false,
				Error = string.Join(
					" ",
					errors.Select(error =>
						error.Message)),
				ErrorCategory = Results.ErrorCategory.Validation,
				ValidationErrors = errors,
			};
}
