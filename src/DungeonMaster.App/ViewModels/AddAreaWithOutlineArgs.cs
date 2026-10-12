using DungeonMaster.Domain.ValueObjects;

namespace DungeonMaster.App.ViewModels;

/// <summary>
///		Arguments for creating an area and writing the outline the user has just drawn.
/// </summary>
/// <param name="FloorId">The unique identifier of the floor that will receive the new area.</param>
/// <param name="Outline">The outline drawn by the user, in canvas coordinates.</param>
public sealed record AddAreaWithOutlineArgs(Guid FloorId, IReadOnlyList<PathSegment> Outline);
