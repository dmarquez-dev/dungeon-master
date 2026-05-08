namespace DungeonMaster.Application.Common.Results;

/// <summary>
///		Categorizes the kind of failure a <see cref="Result{T}"/> represents.
/// </summary>
public enum ErrorCategory
{
	/// <summary>
	///		The request contained invalid or missing data.
	/// </summary>
	Validation,

	/// <summary>
	///		The requested resource was not found.
	/// </summary>
	NotFound,

	/// <summary>
	///		The operation conflicts with existing state.
	/// </summary>
	Conflict,

	/// <summary>
	///		The caller does not have permission to perform the operation.
	/// </summary>
	Unauthorized,
}
