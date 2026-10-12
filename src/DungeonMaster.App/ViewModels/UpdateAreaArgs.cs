namespace DungeonMaster.App.ViewModels;

/// <summary>
///		Arguments for renaming or re-describing an area. All three text fields are written as
///		supplied, so a caller editing only one of them passes the current values of the other two.
/// </summary>
/// <param name="FloorId">The unique identifier of the floor that owns the area.</param>
/// <param name="AreaId">The unique identifier of the area to update.</param>
/// <param name="Name">The area's new name.</param>
/// <param name="Description">The area's new description.</param>
/// <param name="Notes">The area's new notes.</param>
public sealed record UpdateAreaArgs(Guid FloorId, Guid AreaId, string Name, string? Description, string? Notes);
