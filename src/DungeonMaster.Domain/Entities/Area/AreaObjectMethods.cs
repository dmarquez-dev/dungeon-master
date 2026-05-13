using DungeonMaster.Domain.ValueObjects;

namespace DungeonMaster.Domain.Entities.Area;

public sealed partial class Area
{
    /// <summary>
    ///		Adds a new <see cref="PlacedObject"/> of the specified kind to this area.
    /// </summary>
    /// <param name="kind">
    ///		The kind of object to place.
    /// </param>
    /// <param name="position">
    ///		The tile-grid position at which to place the object.
    /// </param>
    /// <param name="anchor">
    ///		The snap anchor point to use when placing the object.
    /// </param>
    /// <param name="rotation">
    ///		The rotation to apply to the object.
    /// </param>
    /// <returns>
    ///		The newly created <see cref="PlacedObject"/> instance.
    /// </returns>
    internal PlacedObject AddObject(
        ObjectKind kind,
        TilePoint position,
        SnapAnchor anchor,
        ObjectRotation rotation)
    {
        var obj = new PlacedObject(Id, kind, position, anchor, rotation);
        _objects.Add(obj);
        Touch();
        return obj;
    }

    /// <summary>
    ///		Removes the <see cref="PlacedObject"/> with the specified identifier from this area.
    ///		If no matching object is found, the call is a no-op.
    /// </summary>
    /// <param name="objectId">
    ///		The unique identifier of the object to remove.
    /// </param>
    internal void RemoveObject(Guid objectId)
    {
        var obj = _objects.FirstOrDefault(o => o.Id == objectId);
        if (obj is not null)
        {
            _objects.Remove(obj);
            Touch();
        }
    }

    /// <summary>
    ///		Updates the position, anchor, and rotation of the <see cref="PlacedObject"/> with the specified identifier.
    ///		If no matching object is found, the call is a no-op.
    /// </summary>
    /// <param name="objectId">
    ///		The unique identifier of the object to update.
    /// </param>
    /// <param name="newPosition">
    ///		The new tile-grid position.
    /// </param>
    /// <param name="newAnchor">
    ///		The new snap anchor point.
    /// </param>
    /// <param name="newRotation">
    ///		The new rotation.
    /// </param>
    internal void UpdateObject(
        Guid objectId,
        TilePoint newPosition,
        SnapAnchor newAnchor,
        ObjectRotation newRotation)
    {
        var obj = _objects.FirstOrDefault(o => o.Id == objectId);
        if (obj is not null)
        {
            obj.Update(newPosition, newAnchor, newRotation);
            Touch();
        }
    }
}
