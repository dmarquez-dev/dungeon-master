namespace DungeonMaster.Domain.Entities.Floors;

/// <summary>
///		Identifies the point on a cell to which a placed object is snapped.
/// </summary>
public enum SnapAnchor
{
	/// <summary>
	///		The object is snapped to the centre of the cell.
	/// </summary>
	Center,

	/// <summary>
	///		The object is snapped to the northern edge of the cell.
	/// </summary>
	North,

	/// <summary>
	///		The object is snapped to the southern edge of the cell.
	/// </summary>
	South,

	/// <summary>
	///		The object is snapped to the eastern edge of the cell.
	/// </summary>
	East,

	/// <summary>
	///		The object is snapped to the western edge of the cell.
	/// </summary>
	West
}
