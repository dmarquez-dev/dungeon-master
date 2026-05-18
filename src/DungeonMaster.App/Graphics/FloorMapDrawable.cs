using DungeonMaster.App.Assets.ObjectAssets;
using DungeonMaster.Application.Common.Map;
using DungeonMaster.Application.Features.Dungeons.Queries.GetDungeonDetail;
using DungeonMaster.Domain.Entities.Area;
using DungeonMaster.Domain.ValueObjects;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using Microsoft.Maui.Storage;

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

    private readonly Dictionary<ObjectKind, Microsoft.Maui.Graphics.IImage?> _imageCache = new();

    public FloorMapDrawable() { }

    /// <summary>Clears the image cache so assets reload from the active pack.</summary>
    public void ClearObjectImageCache() =>
        _imageCache.Clear();

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

    /// <summary>Object ID currently under the pointer for hover-glow rendering. Null when nothing is hovered.</summary>
    public Guid? HoveredObjectId { get; set; }

    /// <summary>Object currently being drag-moved. Rendered at <see cref="DraggingObjectTargetCanvas"/> instead of its stored position.</summary>
    public Guid? DraggingObjectId { get; set; }

    /// <summary>
    ///     Target position for the object being drag-moved, in canvas-space pixels (same coordinate space as
    ///     the bounding-box center stored on <see cref="PlacedObjectDto.Position"/>: absolute floor pixels).
    ///     Null when no drag is in progress.
    /// </summary>
    public PointF? DraggingObjectTargetCanvas { get; set; }

    /// <summary>Active ghost object kind for the Place tool preview. Null when no object is selected.</summary>
    public ObjectKind? GhostObjectKind { get; set; }

    /// <summary>Current ghost position in canvas-space pixels (absolute floor pixels). Null when cursor is off canvas.</summary>
    public PointF? GhostCanvasPosition { get; set; }

    /// <summary>Rotation applied to the ghost preview (and used when placing).</summary>
    public ObjectRotation GhostRotation { get; set; } = ObjectRotation.None;

    /// <summary>When true the ghost renders with a red tint to indicate the placement is out-of-bounds.</summary>
    public bool GhostIsInvalidPlacement { get; set; }

    /// <summary>Feet per tile used for phantom segment distance labels. Default 5 (standard D&amp;D).</summary>
    public float TileSizeInFeet { get; set; } = 5f;

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
            {
                DrawAreasEditMode(canvas);
            }
            else
            {
                DrawAreasViewMode(canvas);
            }

            DrawPlacedObjects(canvas);

            if (ActiveAreaId is not null)
            {
                DrawGhostObject(canvas);
            }

            DrawInProgressOverlay(canvas);
        }

        canvas.RestoreState();
    }

    private void DrawGrid(ICanvas canvas, RectF dirtyRect)
    {
        canvas.StrokeColor = GridColor;
        canvas.StrokeSize = 0.5f;

        // Convert screen edges to canvas space so the grid fills the entire view
        // regardless of zoom level or pan position.
        var left   = (float)Math.Floor((-PanOffset.X / ZoomLevel) / TileSize) * TileSize;
        var top    = (float)Math.Floor((-PanOffset.Y / ZoomLevel) / TileSize) * TileSize;
        var right  = (float)Math.Ceiling(((dirtyRect.Width  - PanOffset.X) / ZoomLevel) / TileSize) * TileSize;
        var bottom = (float)Math.Ceiling(((dirtyRect.Height - PanOffset.Y) / ZoomLevel) / TileSize) * TileSize;

        for (var x = left; x <= right; x += TileSize)
        {
            canvas.DrawLine(x, top, x, bottom);
        }

        for (var y = top; y <= bottom; y += TileSize)
        {
            canvas.DrawLine(left, y, right, y);
        }
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
            float alpha;
            if (HoveredAreaId is not null)
                alpha = area.Id == HoveredAreaId ? 1f : 0.3f;
            else
                alpha = area.Id == ActiveAreaId ? 1f : 0.3f;
            var (origin, outline) = GetEffectiveOutline(area);
            DrawAreaOutline(canvas, origin, outline, alpha, GetEffectiveOffset(area.Id));
        }

        DrawGhostAreas(canvas);
    }

    private void DrawGhostAreas(ICanvas canvas)
    {
        foreach (var (origin, outline) in GhostAreas)
        {
            DrawAreaOutline(canvas, origin, outline, 1f);
        }
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

    private Microsoft.Maui.Graphics.IImage? LoadOrGetImage(ObjectKind kind)
    {
        // A cached null means a previous load attempt failed (e.g. Win2D creator not yet ready).
        // Return it as-is so that in-progress frames are not blocked, but do not treat it as a
        // permanent miss — ClearObjectImageCache() is called after the GraphicsView handler is
        // ready so that the next Invalidate() triggers a fresh decode attempt.
        if (_imageCache.TryGetValue(kind, out var cached) && cached is not null)
            return cached;

        // TODO(macOS): FileSystem.OpenAppPackageFileAsync().GetAwaiter().GetResult() is sync-over-async.
        // On iOS/macCatalyst this is safe in practice because the MAUI file system implementation on Apple
        // platforms does not use a thread-pool continuation (it resolves synchronously via NSBundle), so no
        // deadlock occurs. On Windows the Win2D resource creator must be initialised before PlatformImage
        // can decode; the outer try/catch already handles the "No resource creator" exception gracefully by
        // falling back to the missing-image placeholder. The correct long-term fix is to pre-load images
        // asynchronously (e.g. in a Task that runs after the GraphicsView Loaded event) and store them in
        // _imageCache before the first Draw call, removing the sync-over-async pattern entirely.
        Microsoft.Maui.Graphics.IImage? image = null;
        if (ObjectAssetCatalogue.Paths.TryGetValue(kind, out var path))
        {
            try
            {
                using var stream = FileSystem.OpenAppPackageFileAsync(path).GetAwaiter().GetResult();
                image = PlatformImage.FromStream(stream);
            }
            catch
            {
                // Swallowed: image renders as the missing-object placeholder ("?").
                // On Windows this also catches the Win2D "No resource creator" exception on first call
                // before the GraphicsView handler is ready; the cache stores null and the next
                // Invalidate() after the view is loaded will re-enter here with a valid creator.
                image = null;
            }
        }

        _imageCache[kind] = image;
        return image;
    }

    private void DrawPlacedObjects(ICanvas canvas)
    {
        if (Floor is null)
            return;

        foreach (var area in Floor.Areas)
        {
            foreach (var obj in area.Objects)
                DrawPlacedObject(canvas, area, obj);
        }
    }

    private void DrawPlacedObject(ICanvas canvas, AreaDto area, PlacedObjectDto obj)
    {
        var isDragging = DraggingObjectId == obj.Id;

        // Area origin in absolute canvas pixels.
        var areaOffsetCol = GetEffectiveOffset(area.Id)?.Col ?? 0;
        var areaOffsetRow = GetEffectiveOffset(area.Id)?.Row ?? 0;
        var areaOriginX = (area.Origin.Col + areaOffsetCol) * TileSize;
        var areaOriginY = (area.Origin.Row + areaOffsetRow) * TileSize;

        // Object position is area-relative canvas pixels, center of bounding box.
        float centerX, centerY;
        if (isDragging && DraggingObjectTargetCanvas is PointF dragTarget)
        {
            centerX = dragTarget.X;
            centerY = dragTarget.Y;
        }
        else
        {
            centerX = areaOriginX + obj.Position.X;
            centerY = areaOriginY + obj.Position.Y;
        }

        var (wTiles, hTiles) = ObjectKindInfo.GetObjectSize(obj.Kind);
        var boxW = wTiles * TileSize;
        var boxH = hTiles * TileSize;

        var drawX = centerX - boxW / 2f;
        var drawY = centerY - boxH / 2f;

        var image = LoadOrGetImage(obj.Kind);

        // Compute the actual rendered image rect (aspect-fit) so the highlight
        // wraps the visible image rather than the full bounding box.
        float imgX, imgY, imgW, imgH;
        if (image is not null)
        {
            var scaleW = boxW / image.Width;
            var scaleH = boxH / image.Height;
            var scale  = Math.Min(scaleW, scaleH);
            imgW = image.Width  * scale;
            imgH = image.Height * scale;
            imgX = drawX + (boxW - imgW) / 2f;
            imgY = drawY + (boxH - imgH) / 2f;
        }
        else
        {
            imgX = drawX; imgY = drawY; imgW = boxW; imgH = boxH;
        }

        canvas.SaveState();

        if (isDragging)
        {
            canvas.Alpha = 0.55f;
        }

        ApplyRotation(canvas, obj.Rotation, centerX, centerY);

        if (!isDragging && HoveredObjectId == obj.Id)
        {
            canvas.FillColor = Color.FromArgb("#40C8A040");
            canvas.FillRoundedRectangle(imgX - 2, imgY - 2, imgW + 4, imgH + 4, 3);
            canvas.StrokeColor = Color.FromArgb("#90C8A040");
            canvas.StrokeSize = 1.5f;
            canvas.DrawRoundedRectangle(imgX - 2, imgY - 2, imgW + 4, imgH + 4, 3);
        }

        if (image is not null)
        {
            canvas.DrawImage(image, imgX, imgY, imgW, imgH);
        }
        else
        {
            DrawMissingObjectFallback(canvas, drawX, drawY, Math.Max(boxW, boxH));
        }

        canvas.RestoreState();
    }

    private static void ApplyRotation(ICanvas canvas, ObjectRotation rotation, float cx, float cy)
    {
        var degrees = rotation switch
        {
            ObjectRotation.CW90  => 90f,
            ObjectRotation.CW180 => 180f,
            ObjectRotation.CW270 => 270f,
            _                    => 0f
        };

        if (degrees != 0f)
        {
            canvas.Translate(cx, cy);
            canvas.Rotate(degrees);
            canvas.Translate(-cx, -cy);
        }
    }

    private static void DrawMissingObjectFallback(ICanvas canvas, float x, float y, float size)
    {
        canvas.StrokeColor = Color.FromArgb("#C8A040");
        canvas.StrokeSize = 1.5f;
        canvas.DrawRectangle(x, y, size, size);
        canvas.FontColor = Color.FromArgb("#C8A040");
        canvas.FontSize = size * 0.4f;
        canvas.DrawString("?", x, y, size, size, HorizontalAlignment.Center, VerticalAlignment.Center);
    }

    private void DrawGhostObject(ICanvas canvas)
    {
        if (GhostObjectKind is not ObjectKind kind || GhostCanvasPosition is not PointF pos)
            return;

        var (wTiles, hTiles) = ObjectKindInfo.GetObjectSize(kind);
        var boxW = wTiles * TileSize;
        var boxH = hTiles * TileSize;

        // Ghost centers on the cursor position.
        var ghostCenterX = pos.X;
        var ghostCenterY = pos.Y;
        var drawX = ghostCenterX - boxW / 2f;
        var drawY = ghostCenterY - boxH / 2f;

        var image = LoadOrGetImage(kind);

        canvas.SaveState();
        canvas.Alpha = GhostIsInvalidPlacement ? 0.35f : 0.5f;
        ApplyRotation(canvas, GhostRotation, ghostCenterX, ghostCenterY);

        if (GhostIsInvalidPlacement)
        {
            // Red overlay to signal invalid placement zone.
            canvas.FillColor = Color.FromArgb("#80B03020");
            canvas.FillRectangle(drawX, drawY, boxW, boxH);
            canvas.StrokeColor = Color.FromArgb("#E04020");
            canvas.StrokeDashPattern = [4f, 3f];
            canvas.StrokeSize = 1.5f;
            canvas.DrawRectangle(drawX, drawY, boxW, boxH);
        }
        else if (image is not null)
        {
            var scaleW = boxW / image.Width;
            var scaleH = boxH / image.Height;
            var scale  = Math.Min(scaleW, scaleH);
            var imgW   = image.Width  * scale;
            var imgH   = image.Height * scale;
            canvas.DrawImage(image, drawX + (boxW - imgW) / 2f, drawY + (boxH - imgH) / 2f, imgW, imgH);
        }
        else
        {
            canvas.StrokeColor = Color.FromArgb("#C8A040");
            canvas.StrokeDashPattern = [4f, 3f];
            canvas.StrokeSize = 1.5f;
            canvas.DrawRectangle(drawX, drawY, boxW, boxH);
        }

        canvas.RestoreState();
    }

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
            var lastVert = InProgressVertices[^1];
            var (lastX, lastY) = TileCoordinateHelper.ToPixel(lastVert, TileSize);

            // Snap phantom end to nearest grid corner — same logic as actual vertex placement.
            var snapped = TileCoordinateHelper.SnapToGrid(PhantomEnd.Value.X, PhantomEnd.Value.Y, TileSize);

            canvas.StrokeColor = VertexColor.WithAlpha(0.5f);
            canvas.StrokeSize = 1f;
            canvas.StrokeDashPattern = [4f, 4f];
            canvas.DrawLine(lastX, lastY, PhantomEnd.Value.X, PhantomEnd.Value.Y);
            canvas.StrokeDashPattern = null;

            // Distance label: integer feet to the snapped position, matching what clicking now would produce.
            var dx = snapped.Col - lastVert.Col;
            var dy = snapped.Row - lastVert.Row;
            var distTiles = MathF.Sqrt(dx * dx + dy * dy);
            if (distTiles > 0f)
            {
                var distFeet = (int)MathF.Round(distTiles * TileSizeInFeet);
                var midX = (lastX + PhantomEnd.Value.X) / 2f;
                var midY = (lastY + PhantomEnd.Value.Y) / 2f;
                canvas.FontColor = VertexColor.WithAlpha(0.9f);
                canvas.FontSize = 9f;
                canvas.DrawString($"{distFeet}'", midX + 4f, midY - 14f, 48f, 14f,
                    HorizontalAlignment.Left, VerticalAlignment.Center);
            }
        }
    }
}
