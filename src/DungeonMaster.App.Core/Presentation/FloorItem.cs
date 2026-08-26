namespace DungeonMaster.App.Core.Presentation;

/// <summary>
///		Represents the control kit's own shape for a single floor of a dungeon.
///		The kit depends on this record rather than on a query projection, so a change to the GetDungeonDetail projection is no longer a change to a control's public surface.
/// </summary>
/// <param name="Id">
///		The unique identifier of the floor.
/// </param>
/// <param name="Number">
///		The display order number of the floor within its dungeon.
/// </param>
/// <param name="Name">
///		The optional display name of the floor.
/// </param>
/// <param name="Description">
///		An optional short description of the floor.
/// </param>
/// <param name="Notes">
///		Optional free-form notes about the floor.
/// </param>
/// <param name="Areas">
///		The areas belonging to this floor, in the order the query produced them.
/// </param>
/// <param name="Objects">
///		The objects placed on this floor, in the order the query produced them.
/// </param>
public sealed record FloorItem(
	Guid Id,
	int Number,
	string? Name,
	string? Description,
	string? Notes,
	IReadOnlyList<AreaItem> Areas,
	IReadOnlyList<PlacedObjectItem> Objects)
{
	/// <summary>
	///		Gets a value indicating whether either <see cref="Description"/> or <see cref="Notes"/> holds non-whitespace text.
	///		This value is computed on every read and never stored, so it cannot go stale against the text it describes.
	/// </summary>
	public bool HasNotebookContent =>
		!string.IsNullOrWhiteSpace(Description) ||
		!string.IsNullOrWhiteSpace(Notes);
}
