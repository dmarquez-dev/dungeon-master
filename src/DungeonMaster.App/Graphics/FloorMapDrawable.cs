using DungeonMaster.App.Assets.ObjectAssets;
using DungeonMaster.App.Core.Map;
using DungeonMaster.App.Core.Presentation;
using DungeonMaster.App.Theme;
using DungeonMaster.Domain.Entities.Floors;
using DungeonMaster.Domain.ValueObjects;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using Microsoft.Maui.Storage;

namespace DungeonMaster.App.Graphics;

/// <summary>
///     The colours and the one font size a single map frame is painted with, resolved together
///     rather than read one at a time from the draw path.
/// </summary>
/// <remarks>
///     LIFETIME: <see cref="FromTheme"/> reads <see cref="DmTheme"/>, so it must not be called
///     before <c>Application.Current.Resources</c> exists. <see cref="FloorMapDrawable"/> holds it
///     as a method group and invokes it at the top of <see cref="FloorMapDrawable.Draw"/> — inside
///     the draw path — never at construction time. See the remarks on <see cref="DmTheme"/> and the
///     guard test <c>DmThemeKeysResolveTests.NoEagerDmThemeRead</c>.
///     <para>
///     The four dimmed and tinted variants are materialised HERE, once per frame, because
///     <c>Color.WithAlpha</c> allocates: computing them at their use sites allocated one Color per
///     area and per phantom-segment label on every frame (MASTER-097 / UIKIT-009).
///     </para>
/// </remarks>
/// <param name="Grid">The tile grid stroke.</param>
/// <param name="AreaOutline">The area outline stroke at full opacity.</param>
/// <param name="AreaOutlineDimmed">The area outline stroke for areas that are not highlighted.</param>
/// <param name="ObjectHighlightFill">The hover-glow fill behind a hovered placed object.</param>
/// <param name="ObjectHighlightStroke">The hover-glow border around a hovered placed object.</param>
/// <param name="ObjectPlaceholder">The stroke and glyph colour of the missing-object placeholder.</param>
/// <param name="Vertex">The confirmed in-progress vertex dot fill.</param>
/// <param name="VertexPath">The stroke joining the confirmed in-progress vertices.</param>
/// <param name="PhantomSegment">The dashed stroke from the last vertex to the pointer.</param>
/// <param name="DistanceLabel">The font colour of the phantom segment's distance label.</param>
/// <param name="DistanceLabelFontSize">The font size of the phantom segment's distance label.</param>
public readonly record struct FloorMapPalette(
	Color Grid,
	Color AreaOutline,
	Color AreaOutlineDimmed,
	Color ObjectHighlightFill,
	Color ObjectHighlightStroke,
	Color ObjectPlaceholder,
	Color Vertex,
	Color VertexPath,
	Color PhantomSegment,
	Color DistanceLabel,
	float DistanceLabelFontSize)
{
	/// <summary>
	///     The opacity every area that is not the highlighted one renders at.
	/// </summary>
	public const float DimmedAlpha = 0.3f;

	/// <summary>
	///     Reads the live theme. Call this only from the draw path; see the remarks on this type.
	/// </summary>
	public static FloorMapPalette FromTheme()
	{
		// Read once each and derive the variants, so the twelve DmTheme reads a frame used to make
		// are seven resource-dictionary lookups.
		var areaOutline = DmTheme.MapAreaStroke;
		var vertex = DmTheme.MapVertex;

		return new FloorMapPalette(
			DmTheme.MapGrid,
			// WithAlpha(1f) rather than the token as read: the call sites this replaced always
			// went through WithAlpha, so a token declared with alpha below 1 must keep rendering
			// exactly as it did.
			areaOutline.WithAlpha(1f),
			areaOutline.WithAlpha(DimmedAlpha),
			DmTheme.MapAreaFillActive,
			DmTheme.MapAreaStrokeActive,
			DmTheme.MapObjectPlaceholder,
			vertex,
			vertex.WithAlpha(0.85f),
			vertex.WithAlpha(0.5f),
			vertex.WithAlpha(0.9f),
			(float)DmTheme.FontSizeMicro);
	}
}

/// <summary>
///     Renders the tile grid, area outlines, in-progress draw vertices, and phantom segment
///     for the currently selected dungeon floor.
/// </summary>
/// <remarks>
///     The drawable owns its own draw state. Callers do not reach into a collection on it; they ask
///     it to change — <see cref="SetInProgressVertices"/>, <see cref="AddGhostArea"/> and the rest —
///     and every one of those members raises <see cref="RedrawRequested"/>. That is the answer to
///     UIKIT-008 / MASTER-059: a mutation made through a handed-out <c>List</c> or
///     <c>Dictionary</c> is invisible to the drawable, so a caller that forgot to call
///     <c>Invalidate()</c> afterwards left a stale frame with nothing to catch it.
/// </remarks>
public sealed class FloorMapDrawable : IDrawable, IDisposable
{
	// Colours are read from DmTheme at draw time, never cached in a static field: see DmTheme's
	// remarks and the guard test NoEagerDmThemeRead. _paletteSource holds FloorMapPalette.FromTheme
	// as a METHOD GROUP — building the delegate reads nothing; the read happens when Draw invokes it.
	private readonly Func<FloorMapPalette> _paletteSource;

	// The two halves of the asset load, declared once here rather than called directly from
	// PreloadObjectImagesAsync, so a test can hand in a stream it controls and an IImage whose
	// Dispose() it can observe. Production passes the platform calls this file used to make inline,
	// so there is exactly one load path and the tests exercise it.
	private readonly Func<string, Task<Stream>> _streamSource;

	private readonly Func<Stream, Microsoft.Maui.Graphics.IImage> _imageFactory;

	private readonly Dictionary<ObjectKind, Microsoft.Maui.Graphics.IImage?> _imageCache = new();

	private readonly Dictionary<Guid, TilePoint> _pendingMoveOffsets = new();

	private readonly List<IReadOnlyList<PathSegment>> _ghostAreas = [];

	private readonly List<TilePoint> _inProgressVertices = [];

	// The phantom segment's distance label is the same string for every frame the pointer stays
	// the same whole number of feet away, so it is formatted only when that number changes rather
	// than once per frame (MASTER-097 / UIKIT-009). InFeet never returns a negative, so -1 is
	// unreachable and the first label always formats.
	private int _distanceLabelFeet = -1;

	private string _distanceLabelText = string.Empty;

	/// <summary>
	///     Initializes a new instance that reads its colours from <see cref="DmTheme"/>.
	/// </summary>
	public FloorMapDrawable()
		: this(FloorMapPalette.FromTheme)
	{
	}

	/// <summary>
	///     Initializes a new instance that reads its colours from <paramref name="paletteSource"/> and
	///     loads its object images from the app package.
	/// </summary>
	/// <param name="paletteSource">
	///     Supplies the frame's colours. Declared here and immutable thereafter, so nothing has to
	///     resolve a theme at a call site. Tests pass a fixed palette because <see cref="DmTheme"/>
	///     needs an <c>Application.Current</c> that the App.Tests host, which runs outside a MAUI
	///     host, cannot create.
	/// </param>
	/// <exception cref="ArgumentNullException"><paramref name="paletteSource"/> is null.</exception>
	public FloorMapDrawable(Func<FloorMapPalette> paletteSource)
		: this(paletteSource, FileSystem.OpenAppPackageFileAsync, stream => PlatformImage.FromStream(stream))
	{
	}

	/// <summary>
	///     Initializes a new instance from all three of its seams. The palette source is invoked once
	///     per <see cref="Draw"/> call; the other two are invoked once per kind by
	///     <see cref="PreloadObjectImagesAsync"/>.
	/// </summary>
	/// <param name="paletteSource">
	///     Supplies the frame's colours. Declared here and immutable thereafter, so nothing has to
	///     resolve a theme at a call site. Tests pass a fixed palette because <see cref="DmTheme"/>
	///     needs an <c>Application.Current</c> that the App.Tests host, which runs outside a MAUI
	///     host, cannot create.
	/// </param>
	/// <param name="streamSource">
	///     Takes an app-package-relative asset path and yields the stream to decode. Defaults to
	///     <c>FileSystem.OpenAppPackageFileAsync</c>.
	/// </param>
	/// <param name="imageFactory">
	///     Decodes an open stream into an image the drawable then owns. Defaults to
	///     <c>PlatformImage.FromStream</c>. Together with <paramref name="streamSource"/> this is what
	///     makes disposal observable without a MAUI host: a real <c>PlatformImage</c> cannot be asked
	///     whether it was disposed, and it cannot be decoded at all outside a host.
	/// </param>
	/// <exception cref="ArgumentNullException">Any argument is null.</exception>
	public FloorMapDrawable(
		Func<FloorMapPalette> paletteSource,
		Func<string, Task<Stream>> streamSource,
		Func<Stream, Microsoft.Maui.Graphics.IImage> imageFactory)
	{
		_paletteSource = paletteSource ?? throw new ArgumentNullException(nameof(paletteSource));
		_streamSource = streamSource ?? throw new ArgumentNullException(nameof(streamSource));
		_imageFactory = imageFactory ?? throw new ArgumentNullException(nameof(imageFactory));
	}

	/// <summary>
	///     True once <see cref="Dispose"/> has run. Never returns to false.
	/// </summary>
	/// <remarks>
	///     Public, rather than a private field, because the owner that disposes this drawable is not
	///     the type that can observe it: the page teardown path (UIKIT-001 / MASTER-013) needs a
	///     disposal signal it can assert on from outside.
	/// </remarks>
	public bool IsDisposed { get; private set; }

	/// <summary>
	///     Releases every cached object image. This is the teardown half of UIKIT-001 / MASTER-013:
	///     the handles this drawable decoded are owned by it, and nothing else releases them.
	/// </summary>
	/// <remarks>
	///     Idempotent, and never throws. After it runs the drawable stays usable in the degraded sense
	///     the framework needs: <see cref="Draw"/> paints the missing-object placeholder and
	///     <see cref="PreloadObjectImagesAsync"/> loads nothing. Neither raises
	///     <see cref="ObjectDisposedException"/> — the framework can drive a drawable after the page
	///     that owned it is gone, and turning that into an exception would convert a leak into a crash.
	/// </remarks>
	public void Dispose()
	{
		if (IsDisposed)
			return;

		IsDisposed = true;

		// Same per-entry swallow as ClearObjectImageCache, and for the same reason: one failing
		// native release must not strand the remaining handles.
		ClearObjectImageCache();
	}

	/// <summary>
	///     Raised when a change to this drawable's state means the painted frame is now out of
	///     date. The host subscribes once and calls <c>Invalidate()</c> on the view it is drawn
	///     into; nothing else has to remember to.
	/// </summary>
	/// <remarks>
	///     The scalar rendering properties on this type deliberately do NOT raise it. Those are
	///     plain assignments, visible in the diff at the site that makes them, and each is already
	///     paired with an invalidation there. What UIKIT-008 named was the mutation made THROUGH a
	///     handed-out collection, which the drawable could not see at all — that class of mutation
	///     is what this event covers, and after MASTER-059 it is the only way to make one.
	/// </remarks>
	public event EventHandler? RedrawRequested;

	/// <summary>
	///     Disposes every cached object image and clears the cache so assets reload from the active pack.
	/// </summary>
	/// <remarks>
	///     This is the pack-change release; <see cref="Dispose"/> is the teardown release. It does not
	///     set <see cref="IsDisposed"/>: the cache may be refilled afterwards, which is the whole point
	///     of calling it on a pack change. Calling it after <see cref="Dispose"/> is a no-op.
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
		// Not an ObjectDisposedException: see the remarks on Dispose.
		if (IsDisposed)
			return;

		foreach (var (kind, path) in ObjectAssetCatalogue.Paths)
		{
			// A cached non-null image is already decoded. A cached null is a previous failed attempt
			// (e.g. the Win2D resource creator was not ready yet) and is retried on the next call.
			if (_imageCache.TryGetValue(kind, out var cached) && cached is not null)
				continue;

			// MASTER-160: decoded into a LOCAL, and only assigned into the cache once the stream is
			// closed. The old shape was `using var stream = ...; cache[kind] = factory(stream);`,
			// where a stream that threw on close after a successful decode left the catch below
			// overwriting a live, undisposed handle with null. Holding the image here means the catch
			// still owns it and can release it. `decoded` is nulled the instant the cache takes
			// ownership, so the catch never disposes an image the cache is still holding.
			Microsoft.Maui.Graphics.IImage? decoded = null;

			try
			{
				var stream = await _streamSource(path);

				try
				{
					decoded = _imageFactory(stream);
				}
				finally
				{
					stream.Dispose();
				}

				_imageCache[kind] = decoded;
				decoded = null;
			}
			catch
			{
				// Ignored per entry so one unreadable asset does not abort the remaining kinds: this
				// kind renders as the existing missing-object placeholder ("?") until a later call
				// succeeds. On Windows this also catches the Win2D "No resource creator" exception
				// raised when the GraphicsView handler is not yet ready.
				try
				{
					decoded?.Dispose();
				}
				catch
				{
					// Per entry, as everywhere else in this file: a failing native release must not
					// abort the remaining kinds.
				}

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
	///     When set, this area renders highlighted and all others render dimmed. It outranks
	///     <see cref="ActiveAreaId"/>.
	/// </summary>
	public Guid? HoveredAreaId { get; set; }

	/// <summary>
	///     The area currently being dragged. Its live delta is added on top of any pending offset.
	/// </summary>
	public Guid? DragPreviewAreaId { get; set; }

	/// <summary>
	///     The live drag delta for <see cref="DragPreviewAreaId"/>. Null when no drag is in progress.
	/// </summary>
	public TilePoint? DragPreviewDelta { get; set; }

	/// <summary>
	///     Records a committed move offset for one area (not yet saved to the database), so the
	///     area keeps rendering at its new position while the write completes.
	/// </summary>
	/// <param name="areaId">The area the offset applies to.</param>
	/// <param name="offset">The offset in tiles.</param>
	/// <exception cref="ArgumentNullException"><paramref name="offset"/> is null.</exception>
	public void SetPendingMoveOffset(Guid areaId, TilePoint offset)
	{
		ArgumentNullException.ThrowIfNull(offset);

		_pendingMoveOffsets[areaId] = offset;
		RaiseRedrawRequested();
	}

	/// <summary>
	///     Drops every pending move offset, so areas render at the position the floor reports.
	/// </summary>
	public void ClearPendingMoveOffsets()
	{
		_pendingMoveOffsets.Clear();
		RaiseRedrawRequested();
	}

	/// <summary>
	///     Adds a newly drawn outline that has not yet been persisted, so it is visible from the
	///     moment it is closed rather than from the reload that follows the write.
	/// </summary>
	/// <param name="outline">The closed outline polygon, in tile coordinates.</param>
	/// <exception cref="ArgumentNullException"><paramref name="outline"/> is null.</exception>
	public void AddGhostArea(IReadOnlyList<PathSegment> outline)
	{
		ArgumentNullException.ThrowIfNull(outline);

		_ghostAreas.Add(outline);
		RaiseRedrawRequested();
	}

	/// <summary>
	///     Drops every unpersisted outline, normally because the reload that now carries them has
	///     completed.
	/// </summary>
	public void ClearGhostAreas()
	{
		_ghostAreas.Clear();
		RaiseRedrawRequested();
	}

	/// <summary>
	///     Replaces the confirmed vertices of the in-progress draw with <paramref name="vertices"/>.
	/// </summary>
	/// <param name="vertices">The vertices, in the order they were placed.</param>
	/// <exception cref="ArgumentNullException"><paramref name="vertices"/> is null.</exception>
	public void SetInProgressVertices(IEnumerable<TilePoint> vertices)
	{
		ArgumentNullException.ThrowIfNull(vertices);

		// Refilling the list this drawable already owns, rather than taking a caller's new one,
		// is what stops a fresh List allocation on every vertex the user places.
		_inProgressVertices.Clear();
		_inProgressVertices.AddRange(vertices);
		RaiseRedrawRequested();
	}

	/// <summary>
	///     Drops the confirmed vertices of the in-progress draw.
	/// </summary>
	public void ClearInProgressVertices()
	{
		_inProgressVertices.Clear();
		RaiseRedrawRequested();
	}

	/// <summary>
	///     Current pointer position for the phantom segment preview.
	///     Only rendered when the in-progress draw has at least one confirmed vertex.
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
		ArgumentNullException.ThrowIfNull(canvas);

		// Resolved here, at the top of the draw path, and threaded down. Hoisting it any higher —
		// a field initialiser, the constructor — would read DmTheme before
		// Application.Current.Resources exists; see the remarks on FloorMapPalette.
		var palette = _paletteSource();

		canvas.SaveState();
		canvas.Translate(PanOffset.X, PanOffset.Y);
		canvas.Scale(ZoomLevel, ZoomLevel);

		DrawGrid(canvas, dirtyRect, palette.Grid);

		if (Floor is not null)
		{
			DrawAreas(canvas, in palette);
			DrawPlacedObjects(canvas, in palette);
			DrawGhostObject(canvas, palette.ObjectPlaceholder);
			DrawInProgressOverlay(canvas, in palette);
		}

		canvas.RestoreState();
	}

	private void RaiseRedrawRequested() =>
		RedrawRequested?.Invoke(this, EventArgs.Empty);

	private void DrawGrid(ICanvas canvas, RectF dirtyRect, Color stroke)
	{
		canvas.StrokeColor = stroke;
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

	/// <summary>
	///     Draws every area on the floor, then the unpersisted ghost outlines.
	/// </summary>
	/// <remarks>
	///     One loop, where MASTER-035 found two byte-identical ones differing only in the alpha
	///     expression. The rule both encoded is the same rule: ONE area renders at full opacity and
	///     the rest render dimmed, and which one it is is the hovered area when there is one and
	///     the active area otherwise — that is exactly <c>HoveredAreaId ?? ActiveAreaId</c>. When
	///     neither is set, nothing is singled out and every area renders at full opacity.
	/// </remarks>
	private void DrawAreas(ICanvas canvas, in FloorMapPalette palette)
	{
		var highlightedAreaId = HoveredAreaId ?? ActiveAreaId;

		foreach (var area in Floor!.Areas)
		{
			var stroke = highlightedAreaId is null || area.Id == highlightedAreaId
				? palette.AreaOutline
				: palette.AreaOutlineDimmed;

			var (offsetCol, offsetRow) = GetEffectiveOffset(area.Id);
			DrawAreaOutline(canvas, area.Outline, stroke, offsetCol, offsetRow);
		}

		foreach (var outline in _ghostAreas)
		{
			DrawAreaOutline(canvas, outline, palette.AreaOutline);
		}
	}

	/// <summary>
	///     The total tile offset an area renders at: its committed pending move plus, if it is the
	///     one being dragged, the live drag delta. (0, 0) when neither applies.
	/// </summary>
	private (int Col, int Row) GetEffectiveOffset(Guid areaId)
	{
		_pendingMoveOffsets.TryGetValue(areaId, out var pending);
		var drag = DragPreviewAreaId == areaId ? DragPreviewDelta : null;

		return (
			(pending?.Col ?? 0) + (drag?.Col ?? 0),
			(pending?.Row ?? 0) + (drag?.Row ?? 0));
	}

	/// <summary>
	///     The pixel position of a tile point shifted by a whole number of tiles.
	/// </summary>
	/// <remarks>
	///     This is <see cref="TileCoordinateHelper.ToPixel"/> with the shift folded in, and
	///     <c>FloorMapDrawableGeometryTests.OffsetPixelPositionMatchesTileCoordinateHelper</c> pins
	///     the two together. It exists because the helper takes a <see cref="TilePoint"/>, which is
	///     a record and therefore a heap allocation: building the shifted point cost one allocation
	///     per segment and two more per curve, on every frame (MASTER-097 / UIKIT-009). The
	///     addition stays INSIDE the multiplication, exactly where the old code put it, so the
	///     float result is bit-for-bit what it was.
	/// </remarks>
	private (float X, float Y) ToPixel(TilePoint tile, int offsetCol, int offsetRow) =>
		((tile.Col + offsetCol) * TileSize, (tile.Row + offsetRow) * TileSize);

	private void DrawAreaOutline(
		ICanvas canvas,
		IReadOnlyList<PathSegment> outline,
		Color stroke,
		int offsetCol = 0,
		int offsetRow = 0)
	{
		if (outline.Count == 0)
			return;

		canvas.StrokeColor = stroke;
		canvas.StrokeSize = 1.5f;

		// The polygon is closed, so the final segment's end point is also its start point.
		var (ox, oy) = ToPixel(outline[^1].End, offsetCol, offsetRow);

		// NOT pooled across frames, deliberately: PathF exposes no reset — no Clear, no
		// truncate-to-empty — and it caches a per-instance PlatformPath, so a shared instance
		// cannot be emptied and refilled. UIKIT-009 names this allocation; it is declined here
		// with that reason rather than addressed.
		var path = new PathF(ox, oy);

		foreach (var segment in outline)
		{
			var (ex, ey) = ToPixel(segment.End, offsetCol, offsetRow);

			if (segment.Kind == PathSegmentKind.Line)
			{
				path.LineTo(ex, ey);
			}
			else if (segment.Kind == PathSegmentKind.CubicBezier
				&& segment.Ctrl1 is not null
				&& segment.Ctrl2 is not null)
			{
				var (c1x, c1y) = ToPixel(segment.Ctrl1, offsetCol, offsetRow);
				var (c2x, c2y) = ToPixel(segment.Ctrl2, offsetCol, offsetRow);
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

	private void DrawPlacedObjects(ICanvas canvas, in FloorMapPalette palette)
	{
		if (Floor is null)
			return;

		foreach (var obj in Floor.Objects)
			DrawPlacedObject(canvas, obj, in palette);
	}

	private void DrawPlacedObject(ICanvas canvas, PlacedObjectItem obj, in FloorMapPalette palette)
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
			canvas.FillColor = palette.ObjectHighlightFill;
			canvas.FillRoundedRectangle(imgX - 2, imgY - 2, imgW + 4, imgH + 4, 3);
			canvas.StrokeColor = palette.ObjectHighlightStroke;
			canvas.StrokeSize = 1.5f;
			canvas.DrawRoundedRectangle(imgX - 2, imgY - 2, imgW + 4, imgH + 4, 3);
		}

		if (image is not null)
		{
			canvas.DrawImage(image, imgX, imgY, imgW, imgH);
		}
		else
		{
			DrawMissingObjectFallback(canvas, drawX, drawY, Math.Max(boxW, boxH), palette.ObjectPlaceholder);
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

	private static void DrawMissingObjectFallback(ICanvas canvas, float x, float y, float size, Color placeholder)
	{
		canvas.StrokeColor = placeholder;
		canvas.StrokeSize = 1.5f;
		canvas.DrawRectangle(x, y, size, size);
		canvas.FontColor = placeholder;
		canvas.FontSize = size * 0.4f;
		canvas.DrawString("?", x, y, size, size, HorizontalAlignment.Center, VerticalAlignment.Center);
	}

	private void DrawGhostObject(ICanvas canvas, Color placeholder)
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
			canvas.StrokeColor = placeholder;
			canvas.StrokeDashPattern = [4f, 3f];
			canvas.StrokeSize = 1.5f;
			canvas.DrawRectangle(drawX, drawY, boxW, boxH);
		}

		canvas.RestoreState();
	}

	private void DrawInProgressOverlay(ICanvas canvas, in FloorMapPalette palette)
	{
		if (_inProgressVertices.Count == 0)
			return;

		if (_inProgressVertices.Count >= 2)
		{
			canvas.StrokeColor = palette.VertexPath;
			canvas.StrokeSize = 1.5f;
			canvas.StrokeDashPattern = null;

			var (sx, sy) = TileCoordinateHelper.ToPixel(_inProgressVertices[0], TileSize);

			// Not pooled across frames, for the same reason as the area outline path above:
			// PathF has no reset.
			var inProgressPath = new PathF(sx, sy);

			for (var i = 1; i < _inProgressVertices.Count; i++)
			{
				var (ex, ey) = TileCoordinateHelper.ToPixel(_inProgressVertices[i], TileSize);
				inProgressPath.LineTo(ex, ey);
			}

			canvas.DrawPath(inProgressPath);
		}

		canvas.FillColor = palette.Vertex;

		foreach (var vertex in _inProgressVertices)
		{
			var (vx, vy) = TileCoordinateHelper.ToPixel(vertex, TileSize);
			canvas.FillCircle(vx, vy, 4f);
		}

		if (PhantomEnd is not null)
		{
			var lastVert = _inProgressVertices[^1];
			var (lastX, lastY) = TileCoordinateHelper.ToPixel(lastVert, TileSize);

			// Snap phantom end to nearest grid corner — same logic as actual vertex placement.
			var snapped = TileCoordinateHelper.SnapToGrid(PhantomEnd.Value.X, PhantomEnd.Value.Y, TileSize);

			canvas.StrokeColor = palette.PhantomSegment;
			canvas.StrokeSize = 1f;
			canvas.StrokeDashPattern = [4f, 4f];
			canvas.DrawLine(lastX, lastY, PhantomEnd.Value.X, PhantomEnd.Value.Y);
			canvas.StrokeDashPattern = null;

			// Guard-equivalence note (bounded, currently unreachable divergence) — recorded, not fixed.
			// This guard replaced `if (distTiles > 0f)` when the distance arithmetic moved into
			// TileDistance.InFeet (called immediately below). The two agree on every reachable input:
			// TilePoint is a positional record over two ints, so `snapped != lastVert` is exactly
			// `dx != 0 || dy != 0`. The one divergence is that `dx * dx + dy * dy` inside InFeet is
			// unchecked int arithmetic — at |dx| >= 46341 it can overflow to a negative value (MathF.Sqrt
			// then returns NaN, so the old `> 0f` guard was false) or to exactly 0 (where
			// `snapped != lastVert` is true). That is unreachable today: TileSize is declared
			// `public float TileSize { get; set; } = 40f;` in this file, and zoom is a separate value
			// clamped to [0.25, 4.0], so a 46,000-tile separation cannot reach a rendered frame.
			// Distance label: integer feet to the snapped position, matching what clicking now would produce.
			if (snapped != lastVert)
			{
				var distFeet = TileDistance.InFeet(lastVert, snapped, TileSizeInFeet);
				var midX = (lastX + PhantomEnd.Value.X) / 2f;
				var midY = (lastY + PhantomEnd.Value.Y) / 2f;
				canvas.FontColor = palette.DistanceLabel;
				canvas.FontSize = palette.DistanceLabelFontSize;
				canvas.DrawString(DistanceLabelFor(distFeet), midX + 4f, midY - 14f, 48f, 14f,
					HorizontalAlignment.Left, VerticalAlignment.Center);
			}
		}
	}

	/// <summary>
	///     The distance label for a whole number of feet, formatted only when the number changes.
	/// </summary>
	private string DistanceLabelFor(int feet)
	{
		if (feet != _distanceLabelFeet)
		{
			_distanceLabelFeet = feet;
			_distanceLabelText = $"{feet}'";
		}

		return _distanceLabelText;
	}
}
