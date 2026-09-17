namespace DungeonMaster.Domain.Entities;

/// <summary>
///		The outcome of an aggregate mutation that resolves a child entity by identifier.
/// </summary>
public enum MutationOutcome
{
	/// <summary>
	///		No outcome was reported. No aggregate mutation ever returns this member, and the
	///		Application layer's mapping onto its result type rejects it.
	/// </summary>
	None,

	/// <summary>
	///		The mutation was applied, or was a no-op that leaves the aggregate in the requested state.
	/// </summary>
	Success,

	/// <summary>
	///		No child with the supplied identifier exists on the aggregate, so nothing was changed.
	/// </summary>
	NotFound,

	/// <summary>
	///		The requested value lies outside the range the aggregate permits, so nothing was changed.
	/// </summary>
	OutOfRange,
}
