namespace DungeonMaster.App.ViewModels;

/// <summary>
///		Arguments for removing an area from a floor.
/// </summary>
/// <param name="FloorId">The unique identifier of the floor that owns the area.</param>
/// <param name="AreaId">The unique identifier of the area to remove.</param>
public sealed record RemoveAreaArgs(Guid FloorId, Guid AreaId);
