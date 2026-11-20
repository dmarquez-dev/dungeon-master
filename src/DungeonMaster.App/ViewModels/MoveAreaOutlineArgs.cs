using DungeonMaster.Domain.ValueObjects;

namespace DungeonMaster.App.ViewModels;

/// <summary>
///		Arguments for writing a moved area's translated outline.
/// </summary>
/// <param name="FloorId">The unique identifier of the floor that owns the area.</param>
/// <param name="AreaId">The unique identifier of the area that moved.</param>
/// <param name="Outline">The translated outline, in canvas coordinates.</param>
public sealed record MoveAreaOutlineArgs(Guid FloorId, Guid AreaId, IReadOnlyList<PathSegment> Outline);
