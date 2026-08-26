using DungeonMaster.App.Assets.ObjectAssets;
using DungeonMaster.App.Core.Map;
using DungeonMaster.App.Core.Presentation;
using DungeonMaster.App.Theme;
using DungeonMaster.Domain.Entities.Floor;
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
	// Colours are read from DmTheme at draw time, never cached in a static field: see DmTheme's
	// remarks and the guard test NoEagerDmThemeRead.
	private readonly Dictionary<ObjectKind, Microsoft.Maui.Graphics.IImage?> _imageCache = new();

	public FloorMapDrawable() { }

	/// <summary>
	///     Disposes every cached object image and clears the cache so assets reload from the active pack.
	/// </summary>
	/// <remarks>
	///     Disposal here is not the fix for the cached-image lifetime defect (UIKIT-001 / MASTER-013),
	///     which is still open: nothing releases these handles when the owning page is torn down.
	///     It is here only so that PreloadObjectImagesAsync, which fills an entry for every kind the
	///     active pack declares rather than only the kinds actually drawn, does not make that open
	///     defect worse on every pack change.
	/// </remarks>
	public void ClearObjectImageCache()
	{
		foreach (var image in _imageCache.Values)
		{
			try
			{
				image?.Dispose();
			}
			catch
			{
				// Ignored per entry, and only per entry: one failing native release must not stop the
				// remaining handles from being released. The entry is dropped by the Clear() below
				// regardless, so retrying it is not possible and re-throwing would only strand the rest.
			}
		}

		_imageCache.Clear();
	}

	/// <summary>
	///     Loads and decodes every image named by <see cref="ObjectAssetCatalogue.Paths"/> into the cache.
	///     Must be called on the UI thread. Safe to call repeatedly; entries already cached are not reloaded.
	/// </summary>
	/// <remarks>
	///     Every write to _imageCache happens on the thread that called this method. The awaits deliberately
	///     carry no ConfigureAwait(false), so each continuation resumes on the caller's UI context, and no
	///     work is offloaded to a Task.Run, a thread or a Parallel call. _imageCache is a plain Dictionary
	///     that Draw reads on the UI thread; it must never be written from another one.
	/// </remarks>
	public async Task PreloadObjectImagesAsync()
	{
		foreach (var (kind, path) in ObjectAssetCatalogue.Paths)
		{
			// A cached non-null image is already decoded. A cached null is a previous failed attempt
			// (e.g. the Win2D resource creator was not ready yet) and is retried on the next call.
			if (_imageCache.TryGetValue(kind, out var cached) && cached is not null)
				continue;

			try
			{
				using var stream = await FileSystem.OpenAppPackageFileAsync(path);
				_imageCache[kind] = PlatformImage.FromStream(stream);
			}
			catch
			{
				// Ignored per entry so one unreadable asset does not abort the remaining kinds: this
				// kind renders as the existing missing-object placeholder ("?") until a later call
				// succeeds. On Windows this also catches the Win2D "No resource creator" exception
				// raised when the GraphicsView handler is not yet ready.
				_imageCache[kind] = null;
			}
		}
	}

	/// <summary>
	///     The floor whose areas are rendered. When null only the grid is drawn.
	/// </summary>
	public FloorItem? Floor { get; set; }

	/// <summary>
	///     When set, this area renders at full opacity and all others render dimmed.
	///     Purely a view filter — it never affects what the user is allowed to place or move.
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
	public Dictionary<Guid, IReadOnlyList<PathSegment>> PendingOutlines { get; } = new();

	/// <summary>
	///     New areas drawn on the canvas that have not yet been persisted to the DB.
	/// </summary>
	public List<IReadOnlyList<PathSegment>> GhostAreas { get; } = [];

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
	///     the bounding-box center stored on <see cref="PlacedObjectItem.Position"/>: absolute floor pixels).
	///     Null when no drag is in progress.
	/// </summary>
	public PointF? DraggingObjectTargetCanvas { get; set; }

	/// <summary>Active ghost object kind for the Place tool preview. Null when no object is selected.</summary>
	public ObjectKind? GhostObjectKind { get; set; }

	/// <summary>Current ghost position in canvas-space pixels (absolute floor pixels). Null when cursor is off canvas.</summary>
	public PointF? GhostCanvasPosition { get; set; }

	/// <summary>Rotation applied to the ghost preview (and used when placing).</summary>
	public ObjectRotation GhostRotation { get; set; } = ObjectRotation.None;

	/// <summary>Scale multiplier applied to the ghost preview (and used when placing).</summary>
	public float GhostScale { get; set; } = ObjectScale.Default;

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
			DrawGhostObject(canvas);
			DrawInProgressOverlay(canvas);
		}

		canvas.RestoreState();
	}

	private void DrawGrid(ICanvas canvas, RectF dirtyRect)
	{
		canvas.StrokeColor = DmTheme.MapGrid;
		canvas.StrokeSize = 0.5f;

		// Convert screen edges to canvas space so the grid fills the entire view
		// regardless of zoom level or pan position.
		var left = (float)Math.Floor((-PanOffset.X / ZoomLevel) / TileSize) * TileSize;
		var top = (float)Math.Floor((-PanOffset.Y / ZoomLevel) / TileSize) * TileSize;
		var right = (float)Math.Ceiling(((dirtyRect.Width - PanOffset.X) / ZoomLevel) / TileSize) * TileSize;
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
			var outline = GetEffectiveOutline(area);
			DrawAreaOutline(canvas, outline, alpha, GetEffectiveOffset(area.Id));
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
			var outline = GetEffectiveOutline(area);
			DrawAreaOutline(canvas, outline, alpha, GetEffectiveOffset(area.Id));
		}

		DrawGhostAreas(canvas);
	}

	private void DrawGhostAreas(ICanvas canvas)
	{
		foreach (var outline in GhostAreas)
		{
			DrawAreaOutline(canvas, outline, 1f);
		}
	}

	private IReadOnlyList<PathSegment> GetEffectiveOutline(AreaItem area)
	{
		if (PendingOutlines.TryGetValue(area.Id, out var pending))
			return pending;
		return area.Outline;
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
		IReadOnlyList<PathSegment> outline,
		float alpha,
		TilePoint? offset = null)
	{
		if (outline.Count == 0)
			return;

		canvas.StrokeColor = DmTheme.MapAreaStroke.WithAlpha(alpha);
		canvas.StrokeSize = 1.5f;

		// The polygon is closed, so the final segment's end point is also its start point.
		var start = outline[^1].End;
		var visualStart = offset is null
			? start
			: new TilePoint(start.Col + offset.Col, start.Row + offset.Row);
		var (ox, oy) = TileCoordinateHelper.ToPixel(visualStart, TileSize);
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

	/// <summary>
	///     Reads an already-decoded object image out of the cache. This is a pure dictionary lookup:
	///     it opens no file, decodes nothing and starts no task, because it runs on the Draw path and
	///     Draw runs on the UI thread for every frame.
	/// </summary>
	/// <remarks>
	///     The cache is filled by PreloadObjectImagesAsync. A miss, or a cached null left by a load that
	///     failed, renders as the missing-object placeholder via DrawMissingObjectFallback — the same
	///     fallback a failed decode produced before the load moved off this path.
	/// </remarks>
	private Microsoft.Maui.Graphics.IImage? LoadOrGetImage(ObjectKind kind) =>
		_imageCache.TryGetValue(kind, out var cached) ? cached : null;

	private void DrawPlacedObjects(ICanvas canvas)
	{
		if (Floor is null)
			return;

		foreach (var obj in Floor.Objects)
			DrawPlacedObject(canvas, obj);
	}

	private void DrawPlacedObject(ICanvas canvas, PlacedObjectItem obj)
	{
		var isDragging = DraggingObjectId == obj.Id;

		// Object position is floor-absolute canvas pixels, center of bounding box.
		float centerX, centerY;
		if (isDragging && DraggingObjectTargetCanvas is PointF dragTarget)
		{
			centerX = dragTarget.X;
			centerY = dragTarget.Y;
		}
		else
		{
			centerX = obj.Position.X;
			centerY = obj.Position.Y;
		}

		var (wTiles, hTiles) = ObjectKindInfo.GetObjectSize(obj.Kind);
		var boxW = wTiles * TileSize * obj.Scale;
		var boxH = hTiles * TileSize * obj.Scale;

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
			var scale = Math.Min(scaleW, scaleH);
			imgW = image.Width * scale;
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
			canvas.FillColor = DmTheme.MapAreaFillActive;
			canvas.FillRoundedRectangle(imgX - 2, imgY - 2, imgW + 4, imgH + 4, 3);
			canvas.StrokeColor = DmTheme.MapAreaStrokeActive;
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
			ObjectRotation.CW90 => 90f,
			ObjectRotation.CW180 => 180f,
			ObjectRotation.CW270 => 270f,
			_ => 0f
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
		canvas.StrokeColor = DmTheme.MapObjectPlaceholder;
		canvas.StrokeSize = 1.5f;
		canvas.DrawRectangle(x, y, size, size);
		canvas.FontColor = DmTheme.MapObjectPlaceholder;
		canvas.FontSize = size * 0.4f;
		canvas.DrawString("?", x, y, size, size, HorizontalAlignment.Center, VerticalAlignment.Center);
	}

	private void DrawGhostObject(ICanvas canvas)
	{
		if (GhostObjectKind is not ObjectKind kind || GhostCanvasPosition is not PointF pos)
			return;

		var (wTiles, hTiles) = ObjectKindInfo.GetObjectSize(kind);
		var boxW = wTiles * TileSize * GhostScale;
		var boxH = hTiles * TileSize * GhostScale;

		// Ghost centers on the cursor position.
		var ghostCenterX = pos.X;
		var ghostCenterY = pos.Y;
		var drawX = ghostCenterX - boxW / 2f;
		var drawY = ghostCenterY - boxH / 2f;

		var image = LoadOrGetImage(kind);

		canvas.SaveState();
		canvas.Alpha = 0.5f;
		ApplyRotation(canvas, GhostRotation, ghostCenterX, ghostCenterY);

		if (image is not null)
		{
			var scaleW = boxW / image.Width;
			var scaleH = boxH / image.Height;
			var scale = Math.Min(scaleW, scaleH);
			var imgW = image.Width * scale;
			var imgH = image.Height * scale;
			canvas.DrawImage(image, drawX + (boxW - imgW) / 2f, drawY + (boxH - imgH) / 2f, imgW, imgH);
		}
		else
		{
			canvas.StrokeColor = DmTheme.MapObjectPlaceholder;
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
			canvas.StrokeColor = DmTheme.MapVertex.WithAlpha(0.85f);
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

		canvas.FillColor = DmTheme.MapVertex;

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

			canvas.StrokeColor = DmTheme.MapVertex.WithAlpha(0.5f);
			canvas.StrokeSize = 1f;
			canvas.StrokeDashPattern = [4f, 4f];
			canvas.DrawLine(lastX, lastY, PhantomEnd.Value.X, PhantomEnd.Value.Y);
			canvas.StrokeDashPattern = null;

			// Distance label: integer feet to the snapped position, matching what clicking now would produce.
			if (snapped != lastVert)
			{
				var distFeet = TileDistance.InFeet(lastVert, snapped, TileSizeInFeet);
				var midX = (lastX + PhantomEnd.Value.X) / 2f;
				var midY = (lastY + PhantomEnd.Value.Y) / 2f;
				canvas.FontColor = DmTheme.MapVertex.WithAlpha(0.9f);
				canvas.FontSize = (float)DmTheme.FontSizeMicro;
				canvas.DrawString($"{distFeet}'", midX + 4f, midY - 14f, 48f, 14f,
					HorizontalAlignment.Left, VerticalAlignment.Center);
			}
		}
	}
}
