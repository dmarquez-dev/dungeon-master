using DungeonMaster.Domain.ValueObjects;

namespace DungeonMaster.Domain.Entities.Area;

/// <summary>
///		Represents a game object placed at a specific position within an area.
/// </summary>
public sealed class PlacedObject : Entity
{
    private PlacedObject() { }

    internal PlacedObject(
        Guid areaId,
        ObjectKind kind,
        TilePoint position,
        SnapAnchor anchor,
        ObjectRotation rotation)
    {
        AreaId = areaId;
        Kind = kind;
        Position = position;
        Anchor = anchor;
        Rotation = rotation;
    }

    /// <summary>
    ///		Gets the unique identifier of the area this object belongs to.
    /// </summary>
    public Guid AreaId { get; private set; }

    /// <summary>
    ///		Gets the kind of game object this represents.
    /// </summary>
    public ObjectKind Kind { get; private set; }

    /// <summary>
    ///		Gets the tile-grid position of this object.
    /// </summary>
    public TilePoint Position { get; private set; } = default!;

    /// <summary>
    ///		Gets the snap anchor point used when placing this object.
    /// </summary>
    public SnapAnchor Anchor { get; private set; }

    /// <summary>
    ///		Gets the rotation applied to this object.
    /// </summary>
    public ObjectRotation Rotation { get; private set; }

    /// <summary>
    ///		Updates the position, anchor, and rotation of this object.
    /// </summary>
    internal void Update(TilePoint newPosition, SnapAnchor newAnchor, ObjectRotation newRotation)
    {
        Position = newPosition;
        Anchor = newAnchor;
        Rotation = newRotation;
        Touch();
    }
}
