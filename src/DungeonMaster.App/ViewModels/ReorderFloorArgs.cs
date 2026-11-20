namespace DungeonMaster.App.ViewModels;

/// <summary>
///		Arguments for moving a floor to a new position in the stack.
/// </summary>
/// <param name="FloorId">The unique identifier of the floor to move.</param>
/// <param name="NewNumber">The floor's new display number.</param>
public sealed record ReorderFloorArgs(Guid FloorId, int NewNumber);
