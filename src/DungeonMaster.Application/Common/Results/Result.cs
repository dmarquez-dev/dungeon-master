namespace DungeonMaster.Application.Common.Results;

/// <summary>
///		Represents the outcome of an operation that may succeed or fail with a known error.
///		Use the <see cref="Success"/> and <see cref="Failure"/> factory methods to construct instances.
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
	///		Gets the value returned by the operation, or <see langword="null"/> if it failed.
	/// </summary>
	public T? Value { get; init; }

	/// <summary>
	///		Gets the error message if the operation failed, or <see langword="null"/> if it succeeded.
	/// </summary>
	public string? Error { get; init; }

	/// <summary>
	///		Gets the error category if the operation failed, or <see langword="null"/> if it succeeded.
	/// </summary>
	public ErrorCategory? ErrorCategory { get; init; }

	/// <summary>
	///		Creates a successful result carrying <paramref name="value"/>.
	/// </summary>
	/// <param name="value">
	///		The value returned by the operation.
	/// </param>
	public static Result<T> Success(T value) =>
		new() { IsSuccess = true, Value = value };

	/// <summary>
	///		Creates a failed result with the given error message and category.
	/// </summary>
	/// <param name="error">
	///		A description of what went wrong.
	/// </param>
	/// <param name="category">
	///		The category of the failure.
	/// </param>
	public static Result<T> Failure(
		string error,
		ErrorCategory category) =>
			new() { IsSuccess = false, Error = error, ErrorCategory = category };
}
