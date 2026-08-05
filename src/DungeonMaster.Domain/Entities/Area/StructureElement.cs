using DungeonMaster.Domain.ValueObjects;

namespace DungeonMaster.Domain.Entities.Area;

/// <summary>
///     Represents a structural element placed within an area.
///     Vertex-pair elements (columns, wall spans): From and To are set, EdgeAnchor is null.
///     Edge-center elements (doors, arches): EdgeAnchor is set, From and To are null.
/// </summary>
public sealed class StructureElement : Entity
{
	private StructureElement() { }

	internal StructureElement(Guid areaId, VertexPoint from, VertexPoint to, StructureKind kind)
	{
		AreaId = areaId;
		From = from;
		To = to;
		Kind = kind;
	}

	internal StructureElement(Guid areaId, EdgePoint edgeAnchor, StructureKind kind)
	{
		AreaId = areaId;
		EdgeAnchor = edgeAnchor;
		Kind = kind;
	}

	public Guid AreaId { get; private set; }

	public VertexPoint? From { get; private set; }

	public VertexPoint? To { get; private set; }

	public EdgePoint? EdgeAnchor { get; private set; }

	public StructureKind Kind { get; private set; }
}
