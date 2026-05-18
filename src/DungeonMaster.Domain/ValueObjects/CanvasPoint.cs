namespace DungeonMaster.Domain.ValueObjects;

/// <summary>
///     Represents a position in canvas-space pixels. Used for placed object positions,
///     stored relative to the containing area's origin vertex.
/// </summary>
/// <param name="X">Horizontal pixel offset from the area origin.</param>
/// <param name="Y">Vertical pixel offset from the area origin.</param>
public record CanvasPoint(float X, float Y);
