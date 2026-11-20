namespace DungeonMaster.Domain.Entities;

/// <summary>
///		Base class for all domain entities, providing identity and audit timestamps.
/// </summary>
public abstract class Entity : IEquatable<Entity>
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

	/// <summary>
	///		Determines whether the specified entity has the same identity as this entity.
	/// </summary>
	/// <param name="other">
	///		The entity to compare with this entity.
	/// </param>
	/// <returns>
	///		<see langword="true"/> when <paramref name="other"/> is the same instance, or is of the same runtime type and carries the same <see cref="Id"/>; otherwise <see langword="false"/>.
	/// </returns>
	/// <remarks>
	///		Runtime type participates through <see cref="object.GetType"/> rather than a pattern match on a named type, so two entities of different types can never compare equal even when their identifiers collide.
	/// </remarks>
	public bool Equals(Entity? other)
	{
		if (other is null)
		{
			return false;
		}

		if (ReferenceEquals(this, other))
		{
			return true;
		}

		return GetType() == other.GetType() && Id == other.Id;
	}

	/// <summary>
	///		Determines whether the specified object is an entity with the same identity as this entity.
	/// </summary>
	/// <param name="obj">
	///		The object to compare with this entity.
	/// </param>
	/// <returns>
	///		<see langword="true"/> when <paramref name="obj"/> is an <see cref="Entity"/> that this entity is equal to; otherwise <see langword="false"/>.
	/// </returns>
	public override bool Equals(object? obj) =>
		Equals(obj as Entity);

	/// <summary>
	///		Returns a hash code derived from the runtime type and <see cref="Id"/> of this entity.
	/// </summary>
	/// <returns>
	///		A hash code for this entity.
	/// </returns>
	/// <remarks>
	///		No mutable state takes part, <see cref="LastModified"/> in particular, so the hash code of an entity does not change when <see cref="Touch"/> mutates it while it sits in a hash-based collection.
	/// </remarks>
	public override int GetHashCode() =>
		HashCode.Combine(GetType(), Id);
}
