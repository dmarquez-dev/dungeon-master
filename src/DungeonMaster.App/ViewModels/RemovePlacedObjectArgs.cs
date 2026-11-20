namespace DungeonMaster.App.ViewModels;

/// <summary>
///		Arguments for removing a placed object. There is no dungeon identifier: the editing
///		context holds the one identifier that is correct.
/// </summary>
/// <param name="FloorId">The unique identifier of the floor that owns the object.</param>
/// <param name="ObjectId">The unique identifier of the object to remove.</param>
public sealed record RemovePlacedObjectArgs(Guid FloorId, Guid ObjectId);
