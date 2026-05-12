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
	public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

	/// <summary>
	///		Gets the UTC timestamp when this entity was last modified.
	/// </summary>
	public DateTimeOffset LastModified { get; private set; } = DateTimeOffset.UtcNow;

	/// <summary>
	///		Updates the <see cref="LastModified"/> timestamp to the current UTC time.
	/// </summary>
	protected void Touch() =>
		LastModified = DateTimeOffset.UtcNow;
}
