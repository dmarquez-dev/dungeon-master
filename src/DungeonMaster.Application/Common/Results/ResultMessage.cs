namespace DungeonMaster.Application.Common.Results;

/// <summary>
///		Turns a failed result into the single string a presentation layer displays.
/// </summary>
public static class ResultMessage
{
	/// <summary>
	///		Returns the message to display for the given result, or <see langword="null"/> when the result succeeded.
	/// </summary>
	/// <param name="result">
	///		The result to describe.
	/// </param>
	/// <returns>
	///		The message to display, or <see langword="null"/> when the result succeeded.
	/// </returns>
	public static string? ForFailure(Result result) =>
		Format(
			result.IsSuccess,
			result.Error,
			result.ErrorCategory,
			result.ValidationErrors);

	/// <summary>
	///		Returns the message to display for the given result, or <see langword="null"/> when the result succeeded.
	/// </summary>
	/// <typeparam name="T">
	///		The type of the value carried by a successful result.
	/// </typeparam>
	/// <param name="result">
	///		The result to describe.
	/// </param>
	/// <returns>
	///		The message to display, or <see langword="null"/> when the result succeeded.
	/// </returns>
	public static string? ForFailure<T>(Result<T> result) =>
		Format(
			result.IsSuccess,
			result.Error,
			result.ErrorCategory,
			result.ValidationErrors);

	private static string? Format(
		bool isSuccess,
		string? error,
		ErrorCategory? category,
		IReadOnlyList<ValidationError> validationErrors)
	{
		if (isSuccess)
		{
			return null;
		}

		return category switch
		{
			Results.ErrorCategory.Validation =>
				FormatValidation(
					error,
					validationErrors),
			Results.ErrorCategory.NotFound =>
				error,
			Results.ErrorCategory.Conflict =>
				error,
			Results.ErrorCategory.Unauthorized =>
				"You are not signed in.",
			Results.ErrorCategory.Forbidden =>
				"You do not have permission to do that.",
			_ =>
				error ?? "The operation failed.",
		};
	}

	private static string FormatValidation(
		string? error,
		IReadOnlyList<ValidationError> validationErrors) =>
			validationErrors.Count == 0
				? error ?? "The operation failed."
				: string.Join(
					Environment.NewLine,
					validationErrors.Select(validationError =>
						string.IsNullOrEmpty(validationError.PropertyName)
							? validationError.Message
							: $"{validationError.PropertyName}: {validationError.Message}"));
}
