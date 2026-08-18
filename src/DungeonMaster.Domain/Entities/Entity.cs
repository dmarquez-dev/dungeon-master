namespace DungeonMaster.Domain.Entities;

/// <summary>
///		Base class for all domain entities, providing identity and audit timestamps.
/// </summary>
public abstract class Entity
{
	/// <summary>
	///		Gets the unique identifier for this entity.
	/// </summary>
	public Guid Id { get; private set; } = Guid.NewGuid();

	/// <summary>
	///		Gets the UTC timestamp when this entity was created.
	/// </summary>
	/// <remarks>
	///		The choice of <see cref="DateTime"/> here is a persistence decision, recorded with its reasoning in docs/persistence-contract.md.
	/// </remarks>
	public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

	/// <summary>
	///		Gets the UTC timestamp when this entity was last modified.
	/// </summary>
	/// <remarks>
	///		The choice of <see cref="DateTime"/> here is a persistence decision, recorded with its reasoning in docs/persistence-contract.md.
	/// </remarks>
	public DateTime LastModified { get; private set; } = DateTime.UtcNow;

	/// <summary>
	///		Updates the <see cref="LastModified"/> timestamp to the current UTC time.
	/// </summary>
	protected void Touch() =>
		LastModified = DateTime.UtcNow;
}
