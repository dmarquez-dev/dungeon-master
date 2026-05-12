namespace DungeonMaster.Application.Common.Results;

/// <summary>
///		Categorizes the type of error returned by an operation.
/// </summary>
public enum ErrorCategory
{
	/// <summary>
	///		The request failed because one or more inputs did not pass validation.
	/// </summary>
	Validation,

	/// <summary>
	///		The requested resource could not be found.
	/// </summary>
	NotFound,

	/// <summary>
	///		The operation could not be completed because of a conflict with existing data.
	/// </summary>
	Conflict,

	/// <summary>
	///		The caller is not authenticated and cannot perform this operation.
	/// </summary>
	Unauthorized,
}
