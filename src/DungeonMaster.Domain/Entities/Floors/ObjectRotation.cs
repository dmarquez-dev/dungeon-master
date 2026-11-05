namespace DungeonMaster.Domain.Entities.Floors;

/// <summary>
///		Identifies the quarter-turn rotation applied to a placed object.
/// </summary>
public enum ObjectRotation
{
	/// <summary>
	///		The object is not rotated.
	/// </summary>
	None,

	/// <summary>
	///		The object is rotated 90 degrees clockwise.
	/// </summary>
	Clockwise90,

	/// <summary>
	///		The object is rotated 180 degrees clockwise.
	/// </summary>
	Clockwise180,

	/// <summary>
	///		The object is rotated 270 degrees clockwise.
	/// </summary>
	Clockwise270
}
