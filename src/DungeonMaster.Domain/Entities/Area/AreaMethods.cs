using DungeonMaster.Domain.ValueObjects;

namespace DungeonMaster.Domain.Entities.Area;

public sealed partial class Area
{
	/// <summary>
	///		Updates the name, description, and notes of this area.
	/// </summary>
	/// <param name="name">
	///		The new display name. Must not be null or whitespace.
	/// </param>
	/// <param name="description">
	///		The new optional description.
	/// </param>
	/// <param name="notes">
	///		The new optional notes.
	/// </param>
	internal void Update(
		string name,
		string? description,
		string? notes)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);
		Name = name;
		Description = description;
		Notes = notes;
		Touch();
	}

	/// <summary>
	///		Sets the tile-grid origin and replaces the outline path segments for this area.
	/// </summary>
	/// <param name="origin">
	///		The new tile-grid origin point for this area.
	/// </param>
	/// <param name="outline">
	///		The new ordered list of path segments that form the closed outline polygon.
	/// </param>
	internal void SetOutline(
		TilePoint origin,
		IReadOnlyList<PathSegment> outline)
	{
		Origin = origin;
		_outline.Clear();
		_outline.AddRange(outline);

		Touch();
	}
}
