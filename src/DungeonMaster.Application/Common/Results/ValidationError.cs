namespace DungeonMaster.Application.Common.Results;

/// <summary>
///		A single validation failure, naming the property that failed and why.
/// </summary>
/// <param name="PropertyName">
///		The name of the property that failed validation. This is empty when the rule is not property-scoped.
/// </param>
/// <param name="Message">
///		The human-readable description of the failure.
/// </param>
public sealed record ValidationError(
	string PropertyName,
	string Message);
