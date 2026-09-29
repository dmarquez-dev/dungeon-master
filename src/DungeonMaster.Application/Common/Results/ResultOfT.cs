namespace DungeonMaster.Application.Common.Results;

/// <summary>
///		Represents the outcome of an operation, carrying either a success value or a failure.
/// </summary>
/// <typeparam name="T">
///		The type of the value returned on success.
/// </typeparam>
public sealed record Result<T>
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
	///		Gets the value returned by the operation. A failed result carries <see langword="default"/> of
	///		<typeparamref name="T"/>, because the failure factories never assign this property.
	/// </summary>
	/// <remarks>
	///		<para>
	///		That default is NOT a failure signal and must never be read as one. <see cref="IsSuccess"/> and
	///		<see cref="IsFailure"/> are the only failure signals; branch on one of them before reading this
	///		property.
	///		</para>
	///		<para>
	///		<typeparamref name="T"/> is unconstrained, so the <c>?</c> annotation has no effect once
	///		<typeparamref name="T"/> is closed as a value type. For <c>Result&lt;Guid&gt;</c>, the only closed
	///		construction this application uses, the failure value is therefore <c>Guid.Empty</c>, which is
	///		indistinguishable from a Guid a successful operation could in principle return.
	///		</para>
	/// </remarks>
	public T? Value { get; private init; }

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
	///		Creates a successful result carrying the given value.
	/// </summary>
	/// <param name="value">
	///		The value produced by the operation.
	/// </param>
	/// <returns>
	///		A <see cref="Result{T}"/> representing a successful outcome.
	/// </returns>
	public static Result<T> Success(T value) =>
		new()
		{
			IsSuccess = true,
			Value = value,
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
	///		A <see cref="Result{T}"/> representing a failed outcome.
	/// </returns>
	public static Result<T> Failure(
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
	///		A <see cref="Result{T}"/> representing a validation failure whose summary message carries every supplied message.
	/// </returns>
	public static Result<T> ValidationFailure(
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

	/// <summary>
	///		Converts a <see cref="Result{T}"/> into a valueless <see cref="Result"/>, discarding only the value. The summary message, the category and every individual validation error are carried across, so a caller that reports the converted result still has the per-field validation text to show.
	/// </summary>
	/// <param name="result">
	///		The result whose outcome is to be carried across.
	/// </param>
	/// <returns>
	///		A <see cref="Result"/> representing the same outcome as <paramref name="result"/>.
	/// </returns>
	public static implicit operator Result(Result<T> result)
	{
		if (result.IsSuccess)
		{
			return Result.Success();
		}

		if (result.ValidationErrors.Count > 0)
		{
			return Result.ValidationFailure(result.ValidationErrors);
		}

		return Result.Failure(
			result.Error!,
			result.ErrorCategory!.Value);
	}
}
