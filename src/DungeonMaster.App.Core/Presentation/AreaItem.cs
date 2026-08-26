using DungeonMaster.Domain.ValueObjects;

namespace DungeonMaster.App.Core.Presentation;

/// <summary>
///		Represents the control kit's own shape for a single area on a dungeon floor.
///		The kit depends on this record rather than on a query projection, so a change to the GetDungeonDetail projection is no longer a change to a control's public surface.
/// </summary>
/// <param name="Id">
///		The unique identifier of the area.
/// </param>
/// <param name="Name">
///		The display name of the area.
/// </param>
/// <param name="Description">
///		An optional short description of the area.
/// </param>
/// <param name="Notes">
///		Optional free-form notes about the area.
/// </param>
/// <param name="Outline">
///		The ordered list of path segments that form the closed outline polygon of the area.
/// </param>
public sealed record AreaItem(
	Guid Id,
	string Name,
	string? Description,
	string? Notes,
	IReadOnlyList<PathSegment> Outline)
{
	/// <summary>
	///		Gets a value indicating whether either <see cref="Description"/> or <see cref="Notes"/> holds non-whitespace text.
	///		This value is computed on every read and never stored, so it cannot go stale against the text it describes.
	/// </summary>
	public bool HasNotebookContent =>
		!string.IsNullOrWhiteSpace(Description) ||
		!string.IsNullOrWhiteSpace(Notes);
}
