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
	///		Replaces the outline path segments for this area.
	/// </summary>
	/// <param name="outline">
	///		The new ordered list of path segments that form the closed outline polygon,
	///		in floor-absolute tile coordinates.
	/// </param>
	internal void SetOutline(IReadOnlyList<PathSegment> outline)
	{
		_outline.Clear();
		_outline.AddRange(outline);

		Touch();
	}
}
