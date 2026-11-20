namespace DungeonMaster.Domain.Entities.Floors;

/// <summary>
///		Identifies the direction in which a placed object's scale is stepped.
/// </summary>
public enum ScaleDirection
{
	/// <summary>
	///		Steps the scale upward, toward the maximum.
	/// </summary>
	Up,

	/// <summary>
	///		Steps the scale downward, toward the minimum.
	/// </summary>
	Down
}
