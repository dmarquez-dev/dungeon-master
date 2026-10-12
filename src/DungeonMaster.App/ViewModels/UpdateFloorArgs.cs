namespace DungeonMaster.App.ViewModels;

/// <summary>
///		Arguments for renaming or re-describing a floor. All three text fields are written as
///		supplied, so a caller editing only one of them passes the current values of the other two.
/// </summary>
/// <param name="FloorId">The unique identifier of the floor to update.</param>
/// <param name="Name">The floor's new name.</param>
/// <param name="Description">The floor's new description.</param>
/// <param name="Notes">The floor's new notes.</param>
public sealed record UpdateFloorArgs(Guid FloorId, string? Name, string? Description, string? Notes);
