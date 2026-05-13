using DungeonMaster.Application.Common.Map;
using DungeonMaster.Application.Features.Dungeons.Queries.GetDungeonDetail;
using DungeonMaster.Domain.ValueObjects;
using Microsoft.Maui.Graphics;

namespace DungeonMaster.App.Graphics;

/// <summary>
///     Renders the tile grid, area outlines, in-progress draw vertices, and phantom segment
///     for the currently selected dungeon floor.
/// </summary>
public sealed class FloorMapDrawable : IDrawable
{
    private static readonly Color GridColor = Color.FromArgb("#1AC4862A");
    private static readonly Color AreaColor = Color.FromArgb("#C8A040");
    private static readonly Color VertexColor = Color.FromArgb("#F5E4B0");

    /// <summary>
    ///     The floor whose areas are rendered. When null only the grid is drawn.
    /// </summary>
    public FloorDto? Floor { get; set; }

    /// <summary>
    ///     When set, this area is the target of an active draw operation; all others render dimmed.
    ///     Only applies when <see cref="InProgressVertices"/> is non-empty.
    /// </summary>
    public Guid? ActiveAreaId { get; set; }

    /// <summary>
    ///     When set in view mode, this area renders highlighted; all others render dimmed.
    /// </summary>
    public Guid? HoveredAreaId { get; set; }

    /// <summary>
    ///     Accumulated committed move offsets per area (not yet saved to DB).
    /// </summary>
    public Dictionary<Guid, TilePoint> PendingMoveOffsets { get; } = new();

    /// <summary>
    ///     The area currently being dragged. Its live delta is added on top of any pending offset.
    /// </summary>
    public Guid? DragPreviewAreaId { get; set; }

    /// <summary>
    ///     The live drag delta for <see cref="DragPreviewAreaId"/>. Null when no drag is in progress.
    /// </summary>
    public TilePoint? DragPreviewDelta { get; set; }

    /// <summary>
    ///     Pending outline replacements for existing areas (not yet saved to DB).
    /// </summary>
    public Dictionary<Guid, (TilePoint Origin, IReadOnlyList<PathSegment> Outline)> PendingOutlines { get; } = new();

    /// <summary>
    ///     New areas drawn on the canvas that have not yet been persisted to the DB.
    /// </summary>
    public List<(TilePoint Origin, IReadOnlyList<PathSegment> Outline)> GhostAreas { get; } = [];

    /// <summary>
    ///     Confirmed vertices for the in-progress draw operation.
    /// </summary>
    public List<TilePoint> InProgressVertices { get; set; } = [];

    /// <summary>
    ///     Current pointer position for the phantom segment preview.
    ///     Only rendered when InProgressVertices has at least one point.
    /// </summary>
    public PointF? PhantomEnd { get; set; }

    /// <summary>
    ///     Size of one tile in pixels. Defaults to 40.
    /// </summary>
    public float TileSize { get; set; } = 40f;

    /// <summary>Zoom multiplier. Clamped to [0.25, 4.0]. Default 1.0.</summary>
    public float ZoomLevel { get; set; } = 1.0f;

    /// <summary>Viewport pan offset in screen pixels. Default (0,0).</summary>
    public PointF PanOffset { get; set; } = PointF.Zero;

    /// <inheritdoc/>
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.SaveState();
        canvas.Translate(PanOffset.X, PanOffset.Y);
        canvas.Scale(ZoomLevel, ZoomLevel);

        DrawGrid(canvas, dirtyRect);

        if (Floor is not null)
        {
            if (ActiveAreaId is not null)
                DrawAreasEditMode(canvas);
            else
                DrawAreasViewMode(canvas);

            DrawInProgressOverlay(canvas);
        }

        canvas.RestoreState();
    }

    private void DrawGrid(ICanvas canvas, RectF dirtyRect)
    {
        canvas.StrokeColor = GridColor;
        canvas.StrokeSize = 0.5f;

        for (var x = 0f; x <= dirtyRect.Width; x += TileSize)
            canvas.DrawLine(x, 0, x, dirtyRect.Height);

        for (var y = 0f; y <= dirtyRect.Height; y += TileSize)
            canvas.DrawLine(0, y, dirtyRect.Width, y);
    }

    private void DrawAreasViewMode(ICanvas canvas)
    {
        foreach (var area in Floor!.Areas)
        {
            var alpha = HoveredAreaId is null || area.Id == HoveredAreaId ? 1f : 0.3f;
            var (origin, outline) = GetEffectiveOutline(area);
            DrawAreaOutline(canvas, origin, outline, alpha, GetEffectiveOffset(area.Id));
        }

        DrawGhostAreas(canvas);
    }

    private void DrawAreasEditMode(ICanvas canvas)
    {
        foreach (var area in Floor!.Areas)
        {
            var alpha = area.Id == ActiveAreaId ? 1f : 0.3f;
            var (origin, outline) = GetEffectiveOutline(area);
            DrawAreaOutline(canvas, origin, outline, alpha, GetEffectiveOffset(area.Id));
        }

        DrawGhostAreas(canvas);
    }

    private void DrawGhostAreas(ICanvas canvas)
    {
        foreach (var (origin, outline) in GhostAreas)
            DrawAreaOutline(canvas, origin, outline, 1f);
    }

    private (TilePoint Origin, IReadOnlyList<PathSegment> Outline) GetEffectiveOutline(AreaDto area)
    {
        if (PendingOutlines.TryGetValue(area.Id, out var pending))
            return pending;
        return (area.Origin, area.Outline);
    }

    private TilePoint? GetEffectiveOffset(Guid areaId)
    {
        PendingMoveOffsets.TryGetValue(areaId, out var pending);
        var drag = DragPreviewAreaId == areaId ? DragPreviewDelta : null;

        if (pending is null && drag is null)
            return null;

        return new TilePoint(
            (pending?.Col ?? 0) + (drag?.Col ?? 0),
            (pending?.Row ?? 0) + (drag?.Row ?? 0));
    }

    private void DrawAreaOutline(
        ICanvas canvas,
        TilePoint origin,
        IReadOnlyList<PathSegment> outline,
        float alpha,
        TilePoint? offset = null)
    {
        if (outline.Count == 0)
            return;

        canvas.StrokeColor = AreaColor.WithAlpha(alpha);
        canvas.StrokeSize = 1.5f;

        var visualOrigin = offset is null
            ? origin
            : new TilePoint(origin.Col + offset.Col, origin.Row + offset.Row);
        var (ox, oy) = TileCoordinateHelper.ToPixel(visualOrigin, TileSize);
        var path = new PathF(ox, oy);

        foreach (var segment in outline)
        {
            var end = offset is null
                ? segment.End
                : new TilePoint(segment.End.Col + offset.Col, segment.End.Row + offset.Row);
            var (ex, ey) = TileCoordinateHelper.ToPixel(end, TileSize);

            if (segment.Kind == PathSegmentKind.Line)
            {
                path.LineTo(ex, ey);
            }
            else if (segment.Kind == PathSegmentKind.CubicBezier
                && segment.Ctrl1 is not null
                && segment.Ctrl2 is not null)
            {
                var ctrl1 = offset is null
                    ? segment.Ctrl1
                    : new TilePoint(segment.Ctrl1.Col + offset.Col, segment.Ctrl1.Row + offset.Row);
                var ctrl2 = offset is null
                    ? segment.Ctrl2
                    : new TilePoint(segment.Ctrl2.Col + offset.Col, segment.Ctrl2.Row + offset.Row);
                var (c1x, c1y) = TileCoordinateHelper.ToPixel(ctrl1, TileSize);
                var (c2x, c2y) = TileCoordinateHelper.ToPixel(ctrl2, TileSize);
                path.CurveTo(c1x, c1y, c2x, c2y, ex, ey);
            }
        }

        canvas.DrawPath(path);
    }

    /// <summary>Converts a screen-space point to canvas-space by inverting the viewport transform.</summary>
    public PointF ScreenToCanvas(PointF screen) =>
        new((screen.X - PanOffset.X) / ZoomLevel,
            (screen.Y - PanOffset.Y) / ZoomLevel);

    private void DrawInProgressOverlay(ICanvas canvas)
    {
        if (InProgressVertices.Count == 0)
            return;

        if (InProgressVertices.Count >= 2)
        {
            canvas.StrokeColor = VertexColor.WithAlpha(0.85f);
            canvas.StrokeSize = 1.5f;
            canvas.StrokeDashPattern = null;

            var (sx, sy) = TileCoordinateHelper.ToPixel(InProgressVertices[0], TileSize);
            var inProgressPath = new PathF(sx, sy);

            for (var i = 1; i < InProgressVertices.Count; i++)
            {
                var (ex, ey) = TileCoordinateHelper.ToPixel(InProgressVertices[i], TileSize);
                inProgressPath.LineTo(ex, ey);
            }

            canvas.DrawPath(inProgressPath);
        }

        canvas.FillColor = VertexColor;

        foreach (var vertex in InProgressVertices)
        {
            var (vx, vy) = TileCoordinateHelper.ToPixel(vertex, TileSize);
            canvas.FillCircle(vx, vy, 4f);
        }

        if (PhantomEnd is not null)
        {
            var (lastX, lastY) = TileCoordinateHelper.ToPixel(InProgressVertices[^1], TileSize);

            canvas.StrokeColor = VertexColor.WithAlpha(0.5f);
            canvas.StrokeSize = 1f;
            canvas.StrokeDashPattern = [4f, 4f];
            canvas.DrawLine(lastX, lastY, PhantomEnd.Value.X, PhantomEnd.Value.Y);
            canvas.StrokeDashPattern = null;
        }
    }
}
