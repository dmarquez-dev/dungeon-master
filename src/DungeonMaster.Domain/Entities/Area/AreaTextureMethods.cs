using DungeonMaster.Domain.ValueObjects;

namespace DungeonMaster.Domain.Entities.Area;

public sealed partial class Area
{
	/// <summary>
	///		Sets or updates the texture for the tile at the given area-relative position.
	///		If a texture already exists at that position, its kind is updated in place.
	/// </summary>
	internal void SetTileTexture(TilePoint position, TextureKind kind)
	{
		var existing = _tileTextures.FirstOrDefault(t => t.Position == position);

		if (existing is not null)
		{
			existing.UpdateKind(kind);
		}
		else
		{
			_tileTextures.Add(new TileTexture(Id, position, kind));
		}

		Touch();
	}

	/// <summary>
	///		Removes the texture at the given area-relative position. No-op if no texture exists there.
	/// </summary>
	internal void ClearTileTexture(TilePoint position)
	{
		var existing = _tileTextures.FirstOrDefault(t => t.Position == position);

		if (existing is null)
		{
			return;
		}

		_tileTextures.Remove(existing);
		Touch();
	}

	/// <summary>
	///		Adds a structure element between two area-relative vertex positions and returns the new entity.
	///		Use From == To for point structures (columns).
	/// </summary>
	internal StructureElement AddStructureElement(VertexPoint from, VertexPoint to, StructureKind kind)
	{
		var element = new StructureElement(Id, from, to, kind);
		_structureElements.Add(element);
		Touch();
		return element;
	}

	/// <summary>
	///     Adds a door/arch structure element anchored to the center of a tile edge and returns the new entity.
	/// </summary>
	internal StructureElement AddEdgeCenterStructureElement(EdgePoint edgeAnchor, StructureKind kind)
	{
		var element = new StructureElement(Id, edgeAnchor, kind);
		_structureElements.Add(element);
		Touch();
		return element;
	}

	/// <summary>
	///		Removes the structure element with the specified id. Returns true if found and removed.
	/// </summary>
	internal bool RemoveStructureElement(Guid elementId)
	{
		var element = _structureElements.FirstOrDefault(e => e.Id == elementId);

		if (element is null)
		{
			return false;
		}

		_structureElements.Remove(element);
		Touch();
		return true;
	}
}
