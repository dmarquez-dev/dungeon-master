namespace DungeonMaster.Domain.Entities;

/// <summary>
///		Base class for all domain entities.
///		Provides a stable unique identifier and audit timestamps.
///		Subclasses must expose a private parameterless constructor for EF Core hydration.
/// </summary>
public abstract class Entity
{
	/// <summary>
	///		Gets the unique identifier assigned at creation.
	/// </summary>
	public Guid Id { get; private set; } = Guid.NewGuid();

	/// <summary>
	///		Gets the UTC timestamp when this entity was created.
	/// </summary>
	public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

	/// <summary>
	///		Gets the UTC timestamp of the most recent modification.
	/// </summary>
	public DateTimeOffset LastModified { get; private set; } = DateTimeOffset.UtcNow;

	/// <summary>
	///		Updates <see cref="LastModified"/> to the current UTC time.
	/// </summary>
	protected void Touch() =>
		LastModified = DateTimeOffset.UtcNow;
}
