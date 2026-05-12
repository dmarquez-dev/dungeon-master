namespace DungeonMaster.Application.Common.Results;

/// <summary>
///		Represents the outcome of an operation, carrying either a success value or an error.
/// </summary>
/// <typeparam name="T">
///		The type of the value returned on success.
/// </typeparam>
public record Result<T>
{
	/// <summary>
	///		Gets a value indicating whether the operation succeeded.
	/// </summary>
	public bool IsSuccess { get; init; }

	/// <summary>
	///		Gets the value returned by the operation, or <see langword="null"/> when the operation failed.
	/// </summary>
	public T? Value { get; init; }

	/// <summary>
	///		Gets the error message, or <see langword="null"/> when the operation succeeded.
	/// </summary>
	public string? Error { get; init; }

	/// <summary>
	///		Gets the error category, or <see langword="null"/> when the operation succeeded.
	/// </summary>
	public ErrorCategory? ErrorCategory { get; init; }

	/// <summary>
	///		Creates a successful result carrying the given value.
	/// </summary>
	/// <param name="value">
	///		The value produced by the operation.
	/// </param>
	/// <returns>
	///		A <see cref="Result{T}"/> representing a successful outcome.
	/// </returns>
	public static Result<T> Success(T value) =>
		new() { IsSuccess = true, Value = value };

	/// <summary>
	///		Creates a failed result with an error message and category.
	/// </summary>
	/// <param name="error">
	///		A human-readable description of the failure.
	/// </param>
	/// <param name="category">
	///		The category that classifies the type of failure.
	/// </param>
	/// <returns>
	///		A <see cref="Result{T}"/> representing a failed outcome.
	/// </returns>
	public static Result<T> Failure(
		string error,
		ErrorCategory category) =>
			new() { IsSuccess = false, Error = error, ErrorCategory = category };
}
